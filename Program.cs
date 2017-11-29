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
                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "-install":
                            {
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
                                ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                                break;
                            }
                    }
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { new iedusm() };
                ServiceBase.Run(ServicesToRun);
            }
		}
	}
}
