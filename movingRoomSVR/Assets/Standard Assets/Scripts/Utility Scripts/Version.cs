namespace TheBlu {
    
    public class VersionInfo {
        public static string Version = "1.3";
        public static string MinAllowedServerVersion = "1.0";
    
/*        public static string GetBuildID() 
        {
            if (StartupObject.singleton != null)
            {
                return StartupObject.singleton.gitVersionString;
            }
            else
            {
                return Version;
            }
        }*/

        public static string UserAgentString() {
			return "TheBluVR/" + TheBlu.VersionInfo.Version;
/*			return "TheBlu/" + TheBlu.VersionInfo.Version + " (" +
				TheBlu.VersionInfo.GetBuildID() + "; " +
					UnityEngine.SystemInfo.operatingSystem + ") " +
					"Unity/" + UnityEngine.Application.unityVersion;*/
		}
    }
}
