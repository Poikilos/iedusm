# iedusm
IntegratorEdu System Management: manage your hardware from your self-hosted IntegratorEdu instance. 

## Compiling
(this program requires Windows, and is only useful for Windows unless fully reprogrammed with similar features)
* Open iedusm.sln file in SharpDevelop 5.1 or later (or use .NET 4.0 or later [C# 5.0] command line tools)
* Build, Set configuration, Release
* Build, Build Solution

## Usage:
1. Setup using batch (right-click install-service, run as Administrator)
2. Install service object using instructions which are shown by batch file!

### Manual usage:
* manipulate the software (not part of iedusm) that iedusm manages (to uninstall, detach then delete):
  ```
  iedusm.exe -detach_managed_software
  iedusm.exe -delete_managed_software
  iedusm.exe -install_managed_software
  iedusm.exe -update_managed_software
  ```

### The manual usages below are not recommended:
```
iedusm.exe -install
iedusm.exe -uninstall
```

## Changes
* (2017-12-01) added uiAccess="true" as property of requestedExecutionLevel in manifest, so that service can access GUI elements running at any System integrity level (helps with being able to see UAC prompts) -- see also <https://blogs.msdn.microsoft.com/cjacks/2009/10/15/using-the-uiaccess-attribute-of-requestedexecutionlevel-to-improve-applications-providing-remote-control-of-the-desktop/>
  ("Project," "Project Options," "Applications" tab, change "Embed default manifest" to "Create..." then edit the file that appears).
* (2017-12-01) signed assembly (Project Settings, Signing, choose same PublicPrivateKeyFile.snk for all iedu projects) 
* (2017-11-29) initial commit
	* IEdu.cs needs manual references: System.Web.dll, System.Net.dll, System.Net.Http.dll
	* implement self-install as per https://stackoverflow.com/questions/2072288/installing-windows-service-programmatically
	* (actual problem was bad syntax) resolve async could not be found
		* Project settings, compiling, convert to .NET Framework 4.5.1 [4.5.1 is installed in Windows 10 by default]
		* add the async targeting pack as per http://community.sharpdevelop.net/blogs/christophwille/archive/2012/05/04/async-targeting-pack-in-sharpdevelop-4-2-and-later.aspx
			* right-click project, "Manage Packages..." then download it from there (now called "Microsoft Async")
			  (still doesn't work)
	* resolve ManagementObjectSearcher could not be found:
		* add System.Management assembly reference manually


## Known Issues
* check IntegratorEdu instance for settings
	(add route for iedusm to IntegratorEdu project)
* regularly get config from server (the following variables):
	* push_interval_ms
	* pull_interval_ms
	* update_interval_hours
	* web_service_base_url


## Nuget packages of note:
	* cryptography (Microsoft)
	* System.Runtime.WindowsRuntime: "improve operation between managed code and the Windows Runtime" such as System.WindowsRuntimeSystemExtensions, System.IO.WindowsRuntimeStorageExtensions, System.Runtime.InteropServices.WindowsRuntime.AsyncInfo
		* possibly use storage extension for mapping drives in a userspace application
	* System.Net.WebSockets
	* System.Security.Claims
	* System.Net.Requests is for backward compatibility but says to use System.Net.Http (now builtin) instead.
	* System.Security.Principal.Windows: "for retrieving the current Windows user and for interacting with Windows users and groups"
	* System.Net.Security: "uses SSL/TLS protocols to provide secure network communication between client and server endpoints"
	* Microsoft.Extensions.WebEncoders (I don't fully understand this but it is related to dependency injection)
	* System.IO.Pipes: "interprocess communication through anonymous and/or named pipes"
	* Bouncy Castle
	* SpecFlow: bridges the gap between domain experts and developers
	* MailKit
	* (got to page 42 of over 500)
	* various LevelDB and Redis APIs
	
## Authors
### Jacob Gustafson

## Developer Notes
* RSSID is "wi-fi signal strength ID" according to <https://answers.ros.org/question/150691/wi-fi-localisation-using-amcl/>
* inline documentation of wlanSignalQuality says it is 0 to 100, which corresponds linearly to -100dBm to -50dBm (decibel-milliwatts, aka decibels relative to a milliwatt)
	* however sometimes RSSID is -40 or even -39 (0 is perfect)
* service can be updated by overwriting the exe file, as long as service is stopped
