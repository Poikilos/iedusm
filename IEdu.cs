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
