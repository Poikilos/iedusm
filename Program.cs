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
using System.ServiceProcess;
using System.Text;
using System.Configuration.Install;  // ManagedInstallerClass etc
using System.Reflection;  // Assembly etc

namespace iedu
{
	static class Program
	{
		/// <summary>
		/// This method starts the service.
		/// </summary>
		static void Main(string[] args)
		{
			//normally Main is one line (and no args above) for services:
			// To run more than one service you have to add them here
			//ServiceBase.Run(new ServiceBase[] { new iedusm() });
			//but let's implement self-install as per <https://stackoverflow.com/questions/2072288/installing-windows-service-programmatically>:
			if (System.Environment.UserInteractive)
            {
                Console.Error.WriteLine("This program would normally be run as a service");
                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "-install":
                            {
	                    		Console.Error.WriteLine(" but found "+args[0]+" option so continuing with that...");
	                            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                            	//starting it as per codemonkey from <https://stackoverflow.com/questions/1036713/automatically-start-a-windows-service-on-install>:
                            	//results in access denied (same if done manually, unless "Log on as" is changed from LocalService to Local System
								//serviceInstaller
                            	//using (ServiceController sc = new ServiceController(serviceInstaller.ServiceName))
                            	//using (ServiceController sc = new ServiceController("iedusm"))
							    //{
							    //     sc.Start();
							    //}
								break;
                            }
                        case "-uninstall":
                            {
	                    		Console.Error.WriteLine(" but found "+args[0]+" option so continuing with that...");
	                            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
	                            break;
                            }
                        case "-detach_managed_software":
                            {
	                    		Console.Error.WriteLine(" but found "+args[0]+" option so continuing with that...");
	                    		IEduSM.uninstall_services(false);
	                    		break;
                    		}
                        case "-delete_managed_software":
                            {
	                    		Console.Error.WriteLine(" but found "+args[0]+" option so continuing with that...");
	                    		IEduSM.uninstall_services(true);
	                    		break;
                    		}
                        case "-install_managed_software":
                            {
	                    		Console.Error.WriteLine(" but found "+args[0]+" option so continuing with that...");
	                    		IEduSM.update_software();
	                    		break;
                    		}
                        case "-update_managed_software":
                            {
	                    		Console.Error.WriteLine(" but found "+args[0]+" option so continuing with that...");
	                    		IEduSM.update_software();
	                    		break;
                    		}
                    	default:
                    		{
                    			Console.Error.WriteLine(" ERROR: unknown option '"+args[0]+"'");
                    			break;
                    		}
                    }
                }
                else {
        			//make sure other program are install (such as if someone right-clicked this to run as Administrator)
        			//Console.Error.WriteLine(" or be run with -install, -uninstall -services_install or -services_uninstall option.");
        			Console.Error.WriteLine(" so taking defensive measures...");
        			IEduSM.update_software();
                }
            }
            else
            {
            	Console.Error.WriteLine("Running service...");
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { new IEduSM() };
                ServiceBase.Run(ServicesToRun);
            }
		}//end  Main
	}
}
