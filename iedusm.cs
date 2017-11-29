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
using System.ServiceProcess;
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
	public class iedusm : ServiceBase
	{
		private const bool debug_enable = true;
		public const string MyServiceName = "iedusm"; //IntegratorEdu System Management
		private static System.Timers.Timer ss_timer = null;
		private static bool timers_enable = false;
		//"System.Threading.Timer uses System.Timers.Timer internally" says Tim Robinson from https://stackoverflow.com/questions/246697/best-timer-for-using-in-a-windows-service
		//"System.Timers.Timer is geared towards multithreaded applications and is therefore thread-safe via its SynchronizationObject property, whereas System.Threading.Timer is ironically not thread-safe out-of-the-box." according to David Andres from https://stackoverflow.com/questions/1416803/system-timers-timer-vs-system-threading-timer
		public iedusm()
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
		
		private void update_software() {
			string s_name_noext = "iedup";
			string s_path = IEdu.get_service_path(s_name_noext);
			bool update_enable = false; //only enable if version changed
			
			if (update_enable) {
				//see <https://stackoverflow.com/questions/2072288/installing-windows-service-programmatically>:
	            ManagedInstallerClass.InstallHelper(new string[] { s_path });
	        	//starting it as per codemonkey from <https://stackoverflow.com/questions/1036713/automatically-start-a-windows-service-on-install>:
	        	//results in access denied (same if done manually, unless "Log on as" is changed from LocalService to Local System
				//serviceInstaller
	        	//using (ServiceController sc = new ServiceController(serviceInstaller.ServiceName))
	        	//using (ServiceController sc = new ServiceController("iedusm"))
			    //{
			    //     sc.Start();
			    //}
			}
		}
		
		private void uninstall_software() {
			string s_name_noext = "iedup";
			string s_path = IEdu.get_service_path(s_name_noext);
			
			ManagedInstallerClass.InstallHelper(new string[] { "/u", s_path });
		}

		//private async Task ss_timer_Elapsed//would normally be a Task but ok not since is event --see https://stackoverflow.com/questions/39260486/is-it-okay-to-attach-async-event-handler-to-system-timers-timer
		//private async void ss_timer_ElapsedAsync(object sender, ElapsedEventArgs e) {
		//	ss_timer.Stop();
		//	await Task.Run(() => update_software());
		//	if (timers_enable&&(ss_timer!=null)) ss_timer.Start();
		//}
		private void ss_timer_ElapsedSync(object sender, ElapsedEventArgs e) {
			ss_timer.Stop();
			update_software();
			if (timers_enable&&(ss_timer!=null)) ss_timer.Start();
		}
		
		/// <summary>
		/// Start this service.
		/// </summary>
		protected override void OnStart(string[] args)
		{
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
			// TODO: Add tear-down code here (if required) to stop your service.
			ss_timer.Stop();
			ss_timer = null;
		}		
	}
}
