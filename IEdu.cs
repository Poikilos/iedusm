/*
 * Created by SharpDevelop.
 * User: Owner
 * Date: 11/29/2017
 * Time: 2:07 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;//Dictionary etc
using System.Net;
using System.Net.Http;
using System.Web; //urlencode, HttpUtility etc
using System.Net.Sockets; //AddressFamily etc
using System.Runtime.InteropServices; //DllImport etc

namespace iedu
{
	/// <summary>
	/// Description of IEdu.
	/// </summary>
	public class IEdu
	{
		private static string documents_path = null;
		private static string profile_path = null;
		private static bool WOW_mode_enable = false; //see static constructor
		/// <summary>
		/// http://www.pinvoke.net/default.aspx/kernel32.wtsgetactiveconsolesessionid says:
		/// The WTSGetActiveConsoleSessionId function retrieves the Remote Desktop Services session that
		/// is currently attached to the physical console. The physical console is the monitor, keyboard, and mouse.
		/// Note that it is not necessary that Remote Desktop Services be running for this function to succeed.
		/// </summary>
		/// <returns>The session identifier of the session that is attached to the physical console. If there is no
		/// session attached to the physical console, (for example, if the physical console session is in the process
		/// of being attached or detached), this function returns 0xFFFFFFFF.</returns>
		[DllImport("Kernel32.dll", SetLastError = true)]
		[return:MarshalAs(UnmanagedType.U4)]
		public static extern int WTSGetActiveConsoleSessionId ( );
		
		public IEdu()
		{
		}
		
		static IEdu() {
			WOW_mode_enable = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).ToLower().EndsWith(" (x86)"); //Windows on Windows
			documents_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			profile_path = documents_path;
			string parent_of_personal_path = (new FileInfo(profile_path)).DirectoryName;
			if (parent_of_personal_path.ToLower()=="documents") profile_path = parent_of_personal_path; //this is normally true
			else documents_path = Path.Combine(documents_path, "Documents"); //assumes Documents is good place to check for documents (and GitHub repos folder if not in profile_path)
		}
		
		public static string foldable_yaml_value(string indent, string val) {
			string result = null;
			if (val != null) {
				val = val.Replace("\r\n", "\n");
				string[] results = val.Split(new char[] {'\n'});
				result = "";
				if (result!=null&results.Length>0) {
					for (int i=0; i<results.Length; i++) {
						result += indent + results[i].Trim() + Environment.NewLine;
					}
				}
			}
			return result;
		}
		//by compnamehelper from <https://stackoverflow.com/questions/1444592/determine-clients-computer-name>
		public static string DetermineHostName(string IP) //formerly DetermineCompName
		{
		    IPAddress myIP = IPAddress.Parse(IP);
		    IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
		    //List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
		    //return compName.First();
		    return GetIPHost.HostName; //full name
		}
		
		//as per Mrchief from <https://stackoverflow.com/questions/6803073/get-local-ip-address> edited Oct 17 at 19:32 
		public static string GetLocalIPAddress()
		{
		    var host = Dns.GetHostEntry(Dns.GetHostName());
		    foreach (var ip in host.AddressList)
		    {
		        if (ip.AddressFamily == AddressFamily.InterNetwork)
		        {
		            return ip.ToString();
		        }
		    }
		    //throw new Exception("No network adapters with an IPv4 address in the system!");
		    return null;
		}		
		public static string GetHostName() {
			return DetermineHostName(GetLocalIPAddress());
		}
		
		//based on shanabus' answer from <https://stackoverflow.com/questions/1273998/how-to-submit-http-form-using-c-sharp> edited Oct 27 '15 at 13:15 
		public static string http_send_as_form(string url, string form_method, Dictionary<string,string> body) {
		
			string result = "";
			if (string.IsNullOrWhiteSpace(form_method)) form_method = "POST";
			else form_method = form_method.ToUpper();
			string strPost = ""; //"username="+username+"&password="+password+"&firstname="+firstname+"&lastname="+lastname;
			foreach(KeyValuePair<string, string> entry in body)
			{
				if (form_method=="GET") strPost += ((strPost=="")?"?":"&") + entry.Key + "=" + HttpUtility.UrlEncode(entry.Value);
				else strPost += ((strPost=="")?"":"&") + entry.Key + "=" + HttpUtility.UrlEncode(entry.Value);
			}			
			StreamWriter myWriter = null;
			
			HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
			objRequest.Method = "POST";
			objRequest.ContentLength = strPost.Length;
			objRequest.ContentType = "application/x-www-form-urlencoded";
			
			try
			{
			 myWriter = new StreamWriter(objRequest.GetRequestStream());
			 myWriter.Write(strPost);
			}
			catch (Exception e) 
			{
			 return e.Message;
			}
			finally {
			 myWriter.Close();
			}
			
			HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
			using (StreamReader sr = 
			 new StreamReader(objResponse.GetResponseStream()) )
			{
			 result = sr.ReadToEnd();
			
			 // Close and clean up the StreamReader
			 sr.Close();
			}
			return result;
		}
		public static string[] get_software_destination_possible_folder_paths(string s_name) {
			int programs_len = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).Length;
			return new string[] {
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), s_name),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), s_name),
				WOW_mode_enable
					? (Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).Substring(programs_len-6), s_name))
					: (Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)+" (x86)", s_name))
			};
		}
		public static string[] get_software_source_expected_paths(string s_name) {
			return new string[] {
				Path.Combine(Path.Combine(documents_path, "GitHub"), s_name),
				Path.Combine(Path.Combine(profile_path, "GitHub"), s_name),
				".",
				Path.Combine(".","dat"), //in case it is in a deepinstall dat folder
				Path.Combine(Path.Combine(".", "bin"), "Release")
			};
		}
		public static string get_software_source_file_path(string s_name) {
			return get_software_path(s_name, false, true, true);
		}
		public static string get_software_source_folder_path(string s_name) {
			return get_software_path(s_name, false, true, false);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="s_name"></param>
		/// <param name="return_even_if_doesnt_exist_enable">causes return to always be the most desirable path, and never null</param>
		/// <returns></returns>
		public static string get_software_destination_file_path(string s_name, bool return_even_if_doesnt_exist_enable) {
			return get_software_path(s_name, return_even_if_doesnt_exist_enable, false, true);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="s_name"></param>
		/// <param name="return_even_if_doesnt_exist_enable">causes return to always be the most desirable path, and never null</param>
		/// <returns></returns>
		public static string get_software_destination_folder_path(string s_name, bool return_even_if_doesnt_exist_enable) {
			return get_software_path(s_name, return_even_if_doesnt_exist_enable, false, false);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="s_name"></param>
		/// <param name="return_even_if_doesnt_exist_enable">causes return to always be the most desirable path, and never null</param>
		/// <param name="source_mode_enable">get install source (as opposed to ProgramFiles subfolder)</param>
		/// <param name="include_file_path_in_return_enable">returns full executable path (also causes return to be null if file doesn't exist, but only if !return_even_if_doesnt_exist_enable)</param>
		/// <returns>returns a location of a service directory or file, whichever is specified in parameters, or null if allowed by parameters and doesn't exist</returns>
		private static string get_software_path(string s_name, bool return_even_if_doesnt_exist_enable, bool source_mode_enable, bool include_file_path_in_return_enable) {
			string result = null;
			//string projects_path = null;
			//string project_path = null;
			string exe_name = s_name+".exe";
			if (s_name!=null&&s_name.Length>0) {
				//NOTE: SpecialFolders below (only used for source_mode_enable) are only going to have
				// install sources in them if user is same user as elevated Administrator.
				// (if on Windows, they refer to "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders"
				// so they do account for redirection)
				if (return_even_if_doesnt_exist_enable) {
					string programs_path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
					string program_path = Path.Combine(programs_path, s_name);
					if (source_mode_enable) result = Path.Combine(Path.Combine(documents_path, "GitHub"), s_name);
					else result = program_path;
				}
				else {
					if (source_mode_enable) {
						string[] source_folder_paths = get_software_source_expected_paths(s_name);
						int try_i = 0;
						while (result==null && try_i<source_folder_paths.Length) {
							if (File.Exists(Path.Combine(source_folder_paths[try_i], exe_name))) {
								//only return if FILE exists, even if returning folder, since in source mode
								if (include_file_path_in_return_enable) result = Path.Combine(source_folder_paths[try_i], exe_name);
								else result = source_folder_paths[try_i];
								break;
							}
							try_i++;
						}
					}//end else source_mode_enable
					else {
						
						string[] destination_folder_paths = get_software_destination_possible_folder_paths(s_name);
						int try_i = 0;
						while (result==null && try_i<destination_folder_paths.Length) {
							if (include_file_path_in_return_enable) {
								if (File.Exists(Path.Combine(destination_folder_paths[try_i], exe_name))) {
									result = Path.Combine(destination_folder_paths[try_i], exe_name);
									break;
								}
							}
							else {
								if (Directory.Exists(destination_folder_paths[try_i])) {
									result = destination_folder_paths[try_i];
									break;
								}
							}
							try_i++;
						}
					}//end else !source_mode_enable
				}//end else !return_even_if_doesnt_exist_enable
			}
			return result;
		}
	}
}
