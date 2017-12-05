/*
 * Created by SharpDevelop.
 * User: Owner
 * Date: 11/28/2017
 * Time: 8:25 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;// reference: System.ServiceProcess.dll; ServiceProcessInstaller etc; also was automatically added before
using System.Text;
using System.Threading;
using System.Timers; //ElapsedEventArgs etc
using System.Management; //ManagementObjectSearcher etc
using System.IO;
//using System.Threading.Tasks; //async etc
//using Microsoft.Bcl.Async;  // doesn't exist
//using System.ComponentModel.EventBasedAsync;  // doesn't exist
using System.Configuration.Install;  // ManagedInstallerClass etc


namespace iedu
{
	public class IEduSM : ServiceBase
	{
		private const bool debug_enable = true;
		public const string MyServiceName = "iedusm"; //IntegratorEdu System Management
		private static string[] managed_software_names = new string[] {"iedup"};
		private static System.Timers.Timer ss_timer = null;
		private static bool timers_enable = false;
		Dictionary<string, string> settings = null;
		//"System.Threading.Timer uses System.Timers.Timer internally" says Tim Robinson from https://stackoverflow.com/questions/246697/best-timer-for-using-in-a-windows-service
		//"System.Timers.Timer is geared towards multithreaded applications and is therefore thread-safe via its SynchronizationObject property, whereas System.Threading.Timer is ironically not thread-safe out-of-the-box." according to David Andres from https://stackoverflow.com/questions/1416803/system-timers-timer-vs-system-threading-timer
		public IEduSM()
		{
			InitializeComponent();
		}
		
		private void InitializeComponent()
		{
			this.ServiceName = MyServiceName;
		}
		
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (ss_timer!=null) {
				ss_timer.Stop();
				ss_timer = null;
			}
			base.Dispose(disposing);
		}
		public static void update_software() {
			update_software(managed_software_names);
		}
		private static void update_software(string[] names) {
			if (names!=null) {
				if (names.Length>0) {
					Console.Error.WriteLine("update_software will check for already-installed versions in (list may have dups since forces check for x86 in environment variables):");
					string[] destination_dir_paths = IEdu.get_software_destination_possible_folder_paths(names[0]);
					for (int j=0; j<destination_dir_paths.Length; j++) {
						Console.Error.WriteLine("  - "+destination_dir_paths[j].Replace(char.ToString(Path.DirectorySeparatorChar)+names[0], ""));
					}
					Console.Error.WriteLine();
					for (int i=0; i<names.Length; i++) {
						bool copyfiles_enable = false;
						string s_name = names[i];
						try {
							string source_file_path = IEdu.get_software_source_file_path(s_name);
							if (source_file_path==null) {
								string msg = "A source for "+s_name+" could not be found in:";
								string[] sources = IEdu.get_software_source_expected_paths(s_name);
								for (int l=0; l<sources.Length; l++) {
									msg += "\n  - "+sources[l];
								}
								throw new ApplicationException(msg);
							}
							string destination_file_path = IEdu.get_software_destination_file_path(s_name, false);
							//TODO: also update
							bool update_enable = true; //TODO: only enable if version changed
							if (destination_file_path==null) {
								Console.Error.WriteLine("update_software did not find the program "+s_name+", so installing...");
								string destination_folder_path = IEdu.get_software_destination_folder_path(s_name, true);
								if (source_file_path!=null && File.Exists(source_file_path)) {
									//setup (copy files to destination)
									//guaranteed to not to return null when 2nd param is true
									if (!Directory.Exists(destination_folder_path)) {
										Directory.CreateDirectory(destination_folder_path);
									}
									copyfiles_enable = true;
								}
								else {
									Console.Error.WriteLine("ERROR: The missing service was not copied to "+destination_folder_path+" because the installer service could not find the install source in any of the following locations:");
									string[] possible_sources = IEdu.get_software_source_expected_paths(s_name);
									for (int k=0; k<possible_sources.Length; k++) {
										Console.Error.WriteLine("  - "+possible_sources[k]);
									}
									Console.Error.WriteLine();
								}
							}
							else {
								if (update_enable) {
									IEduSM.detach_service(IEdu.get_software_destination_file_path(s_name, false));
									copyfiles_enable = true;
								}
							}
							
							if (copyfiles_enable) {
								//we are already assured by get_service_path that the following doesn't exist since fell through to github:
								string new_s_path = IEdu.get_software_destination_file_path(s_name, true);
								Console.Error.WriteLine("Copying '"+s_name+"' from GitHub build folder to '"+new_s_path+"'");
								try {
									if (File.Exists(new_s_path)) {
										File.Delete(new_s_path);
									}
								}
								catch (Exception exn) {
									Console.Error.WriteLine("Could not finish deleting old version of "+s_name+": "+exn.ToString());
								}
								if (!File.Exists(new_s_path)) {
									File.Copy(source_file_path, new_s_path);
									if (!File.Exists(new_s_path)) {
										Console.Error.WriteLine("Could not copy '"+source_file_path+"' to destination directory '"+IEdu.get_software_destination_folder_path(s_name, true)+"'. You must run this as Administrator.");
									}
									else {
										destination_file_path = IEdu.get_software_destination_file_path(s_name, false);
									}
								}
								else Console.Error.WriteLine("Could not install "+s_name+" since old version couldn't be deleted (maybe you didn't have permission to delete the file, or didn't have permission to detach (\"uninstall\") a service [or stop services which would have to be done first if running])!");
							}
							
							if (destination_file_path!=null) {
								if (copyfiles_enable) {
									//already copied files, but now need to install service SINCE they were (updated/added).
									//NOTE: we are assured by get_service_path that s_path exists
									//      (if it was in GitHub folder, install to ProgramFiles was already tried,
									//      and s_path changed to Program Files*\<s_name>\<s_name>.exe).
									
									//see <https://stackoverflow.com/questions/2072288/installing-windows-service-programmatically>:
									//detach_software(new string[] {s_name});
									//detach_service(s_path); //already done if existed
									ManagedInstallerClass.InstallHelper(new string[] { destination_file_path});
									//starting it as per codemonkey from <https://stackoverflow.com/questions/1036713/automatically-start-a-windows-service-on-install>:
									//results in access denied (same if done manually, unless "Log on as" is changed from LocalService to Local System
									//serviceInstaller
									//ServiceProcessInstaller serviceProcessInstaller1 = new ServiceProcessInstaller();
									//serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
									
									//using (ServiceController sc = new ServiceController("iedusm")) {//using (ServiceController sc = new ServiceController(serviceInstaller.ServiceName))
										
									//	sc.Start();
									//}
								}
								else {
									Console.Error.WriteLine("Update for "+s_name+" was not enabled/needed.");
								}
							}
							else {
								Console.Error.WriteLine("ERROR: Path to "+s_name+" could not be detected.");
							}
						}
						catch (Exception exn) {
							string var_msg = (names[i]!=null)?("'"+names[i]+"'"):"null";
							Console.Error.WriteLine("Could not finish update_software named "+var_msg+":"+exn.ToString());
						}
					}//end for i<names.Length
				}
				else Console.Error.WriteLine("ERROR: update_software got empty names array");
			}
			else Console.Error.WriteLine("ERROR: update_software got null names array");
		}
		public static void uninstall_services(bool delete_enable) {
			uninstall_services(managed_software_names, delete_enable);
		}
		private static void uninstall_services(string[] names, bool delete_enable) {
			if (names!=null) {
				if (names.Length>0) {
					for (int i=0; i<names.Length; i++) {
						try {
							string s_path = IEdu.get_software_destination_file_path(names[i], false);
							if (s_path!=null) {
								detach_service(s_path);
								string parent_path = IEdu.get_software_destination_folder_path(names[i], false);
								//guaranteed to be non-null since s_path was non-null
								if (!File.Exists(s_path)) throw new ApplicationException("file does not exist (this should never happen when 2nd param of get_software_destination_file_path is false)");
								if (delete_enable) {
									string service_uninstall_log_path = Path.Combine(parent_path, "iedup.InstallLog");
									Console.WriteLine("Waiting for service uninstall to finish...");
									if (File.Exists(service_uninstall_log_path)) {
										//TODO: maybe analyze that log
										//File.Delete(service_uninstall_log_path);
									}
									Thread.Sleep(5000); //wait for iedup.InstallLog to be written
									//TODO: see <https://stackoverflow.com/questions/10579679/c-sharp-winform-delete-folders-and-files-on-uninstall-permission-error> find out why access is denied in `File.Delete(s_path)` when s_path DOES exist and is detached (NOT "installed as service" anymore) -- service_uninstall_log_path can be deleted but s_path cannot.
									try {
										File.Delete(s_path); //true for recursive
									}
									catch {
										Console.Error.WriteLine("File.Delete access to \""+s_path+"\" was denied, trying system's delete command...");
										Process.Start("cmd.exe", "timeout 5 > Nul & Del \"" + s_path + "\"");  
										Thread.Sleep(8);
										if (File.Exists(s_path)) {
											Console.WriteLine("Still failed to delete, trying -delete-self option...");
											Process.Start(s_path, "-delete_self");
											Thread.Sleep(8);
											if (File.Exists(s_path)) {
												Console.WriteLine("WARNING: failed to delete '"+s_path+"'");
											}
										}
									}
									Directory.Delete(parent_path, true); //true for recursive
								}
							}
							else Console.Error.WriteLine("WARNING in uninstall_services: "+names[i]+" was already removed.");
						}
						catch (Exception exn) {
							string var_msg = (names[i]!=null)?("'"+names[i]+"'"):"null";
							Console.Error.WriteLine("Could not finish uninstalling service named "+var_msg+" in uninstall_services(...,"+(delete_enable?"true":"false")+"):"+exn.ToString());
						}
					}
				}
				else Console.Error.WriteLine("ERROR: uninstall_services got empty paths array.");
			}
			else Console.Error.WriteLine("ERROR: uninstall_services got null paths array.");
		}
		private static void detach_service(string s_path) {
			if (s_path != null) {
				try {
					if (!s_path.ToLower().EndsWith(".exe")) throw new ApplicationException("You must send a path, not a name, to detach_service");
					bool exists = false;
					try {
						exists = File.Exists(s_path); //I've seen windows throw invalid path exception here (dumb)
						if (!exists) Console.Error.WriteLine("ERROR: detach_service could not find '"+s_path+"'");
					}
					catch (Exception exn) {
						Console.Error.WriteLine("Could not finish detach_service while checking if '"+s_path+"' exists: "+exn.ToString());
					}
					if (exists) ManagedInstallerClass.InstallHelper(new string[] { "/u", s_path });
				}
				catch (Exception exn) {
					if (exn.ToString().Contains("t exist as an installed service")) {
						Console.Error.WriteLine("WARNING: skipping detach '"+s_path+"' since not installed as service.");
					}
					else if (exn.ToString().Contains("t exist")) {
						Console.Error.WriteLine("WARNING: skipping detach '"+s_path+"' since does not exist: "+exn.ToString());
					}
					else Console.Error.WriteLine("Could not finish detach_service: "+exn.ToString());
				}
			}
			else {
				Console.Error.WriteLine("ERROR: detach_service got null string");
			}
		}
		private static void detach_software(string[] names) {
			if (names!=null) {
				if (names.Length>0) {
					for (int i=0; i<names.Length; i++) {
						string s_name = names[i];
						string s_path = IEdu.get_software_destination_file_path(s_name, false);
						if (s_path!=null) {
							if (!File.Exists(s_path)) throw new ApplicationException("get_software_destination_file_path("+s_name+",false) returned a bad path (this should never happen when 2nd param is false--it should return null if service was not found in that case)");
							detach_service(s_path);
						}
						else Console.Error.WriteLine("WARNING: "+s_name+" was already uninstalled.");
					}
				}
				else Console.Error.WriteLine("ERROR: detach_software got empty name array.");
			}
			else Console.Error.WriteLine("ERROR: detach_software got null name array.");
		}

		//private async Task ss_timer_Elapsed//would normally be a Task but ok not since is event --see https://stackoverflow.com/questions/39260486/is-it-okay-to-attach-async-event-handler-to-system-timers-timer
		//private async void ss_timer_ElapsedAsync(object sender, ElapsedEventArgs e) {
		//	ss_timer.Stop();
		//	await Task.Run(() => update_software());
		//	if (timers_enable&&(ss_timer!=null)) ss_timer.Start();
		//}
		private void ss_timer_ElapsedSync(object sender, ElapsedEventArgs e) {
			ss_timer.Stop();
			update_software(new string[]{"iedup"});
			if (timers_enable&&(ss_timer!=null)) ss_timer.Start();
		}
		
		/// <summary>
		/// Start this service.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			
			settings = new Dictionary<string, string>();
			string settings_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "iedusm");
			if (File.Exists(settings_path)) {
				StreamReader ins = new StreamReader(settings_path);
				string line;
				//defaults:
				while ( (line=ins.ReadLine()) != null ) {
					string line_trim = line.Trim();
					if (line_trim.Length>0) {
						if (!line_trim.StartsWith("#")) {
							int ao_i = line_trim.IndexOf(":");
							if (ao_i>-1) {
								string name = line.Substring(0,ao_i).Trim();
								string val = line.Substring(ao_i+1).Trim();
								if (val.Length>1 && val.StartsWith("\"") && val.EndsWith("\"")) {
									val = val.Substring(1,val.Length-2);
								}
								else if (val.Length>1 && val.StartsWith("'") && val.EndsWith("'")) {
									val = val.Substring(1,val.Length-2);
								}
								if (name!="") {
									if (settings.ContainsKey(name)) settings[name] = val;
									else settings.Add(name, val);
								}
							}
						}
					}
				}
				ins.Close();
			}
			timers_enable = true;
			ss_timer = new System.Timers.Timer(debug_enable?5000:30000);  // 10000ms is 10s
			ss_timer.AutoReset = true;  // loop
			//ss_timer.Elapsed += ss_timer_ElapsedAsync;  // ss_timer.Elapsed += async (sender, arguments) => await ss_timer_Elapsed(sender, arguments);
			ss_timer.Elapsed += ss_timer_ElapsedSync;
			
			ss_timer.Enabled = true;  // default is false
			ss_timer.Start();
		}
		
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			timers_enable = false;
			ss_timer.Stop();
			ss_timer = null;
		}
	}
}
