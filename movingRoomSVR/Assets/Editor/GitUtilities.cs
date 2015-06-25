using UnityEngine;
using System.Collections;

public class GitUtilities {
		// add your favorite git path here
	static string[] possibleGitPaths = new string[] {"", "/usr/local/git/bin/","C:/Program Files (x86)/Git/bin/"};
	
	public static string VersionString() {
		foreach (string path in possibleGitPaths) {
			System.Diagnostics.Process gitShell = new System.Diagnostics.Process();
			string applicationPath = path + "git";
		    try
	        {
	            gitShell.StartInfo.FileName = applicationPath;
				gitShell.StartInfo.Arguments = "describe";
	            gitShell.StartInfo.UseShellExecute = false;
				gitShell.StartInfo.RedirectStandardOutput = true;
	            gitShell.Start();
	            string result = gitShell.StandardOutput.ReadToEnd();
	            return result;
	        }
	        catch (System.Exception)
	        {
//	            System.Console.Write(e.Message);
	        }
		}
        Debug.LogWarning("git not found, you can add explicit path to BuildTheBlu.possibleGitPaths");
        return "(unknown git version)";
	}
}
