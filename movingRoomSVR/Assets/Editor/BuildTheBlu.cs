using UnityEngine;
using UnityEditor;
//using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public enum BuildTypes
{
	GraffitiWindows,
	GraffitiMac,
	All
}

public enum BuildPlatforms
{
	PC,
	Android,
	IOS
}


public class BuildTheBlu : ScriptableWizard
{
///	public string baseBuildLetter;
///
///do not change the order of the public parameters 
    public string bootLevelToBuild = "Assets/Scenes/theblu/the-blu-boot";
	public BuildTypes buildType = BuildTypes.All;
	public string gitVersion;
	public BuildPlatforms BuildPlatform;

	string buildFromFolder = "Assets/Scenes/builddata/";

	string baseBuildPath = "Builds";
	
	string pcPath = "/pc";
	string macPath = "/mac";
	
	string pcNative = "/theblu-windows.exe";
/*	string pc64Native = "/theblu-windows64.exe";
	string macPPCNative = "/theblu-mac-ppc.app";
	string macIntelNative = "/theblu-mac-intel.app";
	string macUniversalNative = "/theblu-mac-universal.app";
*/
	string macIntelNative = "/theblu-mac-intel.app";

	string buildAllInSelectedFolder //= "theblu/";
	{
		get
		{
			return bootLevelToBuild.Substring(0,bootLevelToBuild.LastIndexOf("/")+1);
		}
	}
	string baseScenePath
	{
		get
		{
			return bootLevelToBuild + ".unity";
		}
	}
	
	[MenuItem("WEMOTools/Build/BUILD THE BLU BOOT")]
	static void CreateWizard()
	{		
		BuildTheBlu me = ScriptableWizard.DisplayWizard("BUILD THE BLU BOOT", typeof(BuildTheBlu), "Build") as BuildTheBlu;
		string LastBuildSettingFile = string.Concat(Application.dataPath, "/../WEMOData/BuildTheBluBootSetting.json");
		if(File.Exists(LastBuildSettingFile))
		{
			string[] lastBuildSettings = File.ReadAllLines(LastBuildSettingFile);
			me.bootLevelToBuild = lastBuildSettings[0];
			me.buildType = (BuildTypes)System.Enum.Parse(typeof(BuildTypes), lastBuildSettings[1]);
		}
        me.gitVersion = GitUtilities.VersionString();
	}
	void OnWizardUpdate()
	{
		helpString =  "Default Path: the base scenes directory, normally it doesn't need to be changed.\n\n" +
			"Base Level To Build: theblu/the-blu-boot\n" +
			"Normally, this doesn't need to be changed.";
			
	}
	string GetFullPath(string relative)
	{
		string tmp = Application.dataPath;
		return tmp.Substring(0, tmp.LastIndexOf("/") + 1) + relative;
	}

	string GetSceneName(string path)
	{
		
		return path.Substring(path.LastIndexOf("/") + 1, path.LastIndexOf(".unity") - path.LastIndexOf("/") - 1);
	}

	void OnWizardCreate()
	{
		if (!buildFromFolder.EndsWith("/"))
		{
			buildFromFolder += "/";
		}
	
		
		string WEMODataDirectory = string.Concat(Application.dataPath, "/../WEMOData/");
		if(!Directory.Exists(WEMODataDirectory))
		{
			Directory.CreateDirectory(WEMODataDirectory);
		}
	
        BuildScene();
		
		string LastBuildSettingFile = string.Concat(Application.dataPath, "/../WEMOData/BuildTheBluBootSetting.json");
		WEMODataDirectory = string.Concat(Application.dataPath, "/../WEMOData/");
		if(File.Exists(LastBuildSettingFile))
		{
			File.Delete(LastBuildSettingFile);
		}
		string[] lastBuildSettings = new string[2];
		lastBuildSettings[0] = bootLevelToBuild;
		lastBuildSettings[1] = buildType.ToString();
		File.WriteAllLines(LastBuildSettingFile, lastBuildSettings);
	}
	
	
	bool HasSceneOpened(string scenePath)
	{
		// force open the chosen view to build
		if (!EditorApplication.currentScene.Equals(scenePath))
		{
			if (File.Exists(scenePath))
			{
                Debug.Log("Opening :: " + scenePath);
				EditorApplication.OpenScene(scenePath);				
			}
			else
			{
				Debug.LogWarning("Cannot find " + scenePath);
				return false;
			}
		}

		return true;
	}

	void BuildBaseSceneForBuildType( BuildTypes build_type ) 
	{
		string type_path = pcPath;
		string type_name = pcNative;

		BuildTarget build_target = BuildTarget.StandaloneWindows;
		if (BuildPlatform == BuildPlatforms.Android)
		{
			build_target = BuildTarget.Android;
		}
		else if (BuildPlatform == BuildPlatforms.IOS)
		{
			build_target = BuildTarget.iOS;
		}
		else
		{
			switch ( build_type ) {
			case BuildTypes.GraffitiWindows:
				type_path = pcPath;
				type_name = pcNative;
				build_target = BuildTarget.StandaloneWindows;
				break;
			case BuildTypes.GraffitiMac:
				type_path = macPath;
				type_name = macIntelNative;
				build_target = BuildTarget.StandaloneOSXIntel;
				break;
			}
		}
		
		string[] scenes = { baseScenePath };
		string output_path = baseBuildPath + type_path;
		
		if(!Directory.Exists(output_path))
		{
			Directory.CreateDirectory(output_path);
		}
		
		string output_file = output_path + type_name;
		BuildPipeline.BuildPlayer(scenes, output_file, build_target, BuildOptions.None);		
	}

	void BuildScene()//string buildDataFile, JSONObject buildData, JSONObject nbr)
	{
		if (!HasSceneOpened( baseScenePath ) )
			return;
		
		// update our build date.
//		string gitVersion = GitUtilities.VersionString();
        
        GameObject build_text_obj = GameObject.Find("BuildNumberText");
		if (build_text_obj != null) {
            build_text_obj.GetComponent<GUIText>().text = System.DateTime.Now.ToString("G") + " " + gitVersion;
            Debug.Log("Build Number Object : version string set: " + build_text_obj.GetComponent<GUIText>().text);
        }

		// expost git version to application
        GameObject startup_obj = GameObject.Find("MasterOceanObject");
		if (startup_obj != null) {
/*			StartupObject ld = startup_obj.GetComponent<StartupObject>();
			if( ld != null ) {
				ld.gitVersionString = gitVersion.Trim();
				Debug.Log("Startup Object: version string set: " + ld.gitVersionString);
			}
			else {
				Debug.LogError("Startup Object: not found. Versioning failed");
			}*/
		}
		else {
			Debug.LogError("MasterOceanObject: not found. Versioning failed");
		}
        AssetDatabase.SaveAssets();
        
		if( buildType == BuildTypes.All ) {

			BuildBaseSceneForBuildType( BuildTypes.GraffitiWindows );
			BuildBaseSceneForBuildType( BuildTypes.GraffitiMac );
		}
		else {
			BuildBaseSceneForBuildType( buildType );
		}
	}

}
