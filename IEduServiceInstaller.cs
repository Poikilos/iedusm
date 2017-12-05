/*
 * Created by SharpDevelop.
 * User: Owner
 * Date: 12/5/2017
 * Time: 5:15 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Configuration.Install; //reference: System.Configuration.Install.dll; provides Installer
using System.ComponentModel; //provides RunInstaller, RunInstallerAttribute
using System.Collections; //provides IDictionary?
using System.ServiceProcess; //provides ServiceProcessInstaller, ServiceInstaller, ServiceController
//references:
//deprecated references:
//System.ServiceProcess.dll (didn't provide RunInstaller etc)
namespace iedu
{
	/// <summary>
	/// as per fre0n from <https://stackoverflow.com/questions/2253051/credentials-when-installing-windows-service>.
	/// </summary>
	[RunInstaller(true)]
	public class MyServiceInstaller : Installer
	{
	    public MyServiceInstaller()
	    {
	        ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
	        serviceProcessInstaller.Account = ServiceAccount.LocalSystem; // Or whatever account you want
	
	        var serviceInstaller = new ServiceInstaller
	        {
	            DisplayName = "Insert the display name here",
	            StartType = ServiceStartMode.Automatic, // Or whatever startup type you want
	            Description = "Insert a description for your service here",
	            ServiceName = "Insert the service name here"
	        };
	
	        Installers.Add(_serviceProcessInstaller);
	        Installers.Add(serviceInstaller);
	    }
	
	    public override void Commit( IDictionary savedState)
	    {
	        base.Commit(savedState);
	
	        // This will automatically start your service upon completion of the installation.
	        try
	        {
	            var serviceController = new ServiceController("Insert the service name here");
	            serviceController.Start();
	        }
	        catch
	        {
	            MessageBox.Show(
	                "Insert a message stating that the service couldn't be started, and that the user will have to do it manually");
	        }
	    }
	}
}
