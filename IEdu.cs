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

namespace iedu
{
	/// <summary>
	/// Description of IEdu.
	/// </summary>
	public class IEdu
	{
		public IEdu()
		{
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
		public static string html_post(string url, Dictionary<string,string> body) {
		
			string result = "";
			
			string strPost = ""; //"username="+username+"&password="+password+"&firstname="+firstname+"&lastname="+lastname;
			foreach(KeyValuePair<string, string> entry in body)
			{
				strPost += ((strPost=="?")?"":"&") + entry.Key + "=" + HttpUtility.UrlEncode(entry.Value);
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
		
		public static string get_service_path(string s_name) {
			string result = null;
			string try_path = null;
			string projects_path = null;
			string project_path = null;
			if (s_name!=null&&s_name.Length>0) {
				//NOTE: don't use SpecialFolders, since should be running as Local System
				projects_path = "C:\\Users\\Owner\\Documents\\GitHub";
				project_path = Path.Combine(projects_path, s_name);
				try_path = Path.Combine(project_path, "bin", "Release", s_name+".exe");
				if (result==null) { try { if (File.Exists(try_path)) result=try_path; } catch {} }
				try_path = Path.Combine(project_path, "bin", "Release", s_name+".exe");
				if (result==null) { try { if (File.Exists(try_path)) result=try_path; } catch {} }
			}
			return result;
		}
	}
}
