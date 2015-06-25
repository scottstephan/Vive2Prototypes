
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using System.Text;

public class BundleAssets : ScriptableWizard
{
	
/*  Temporary tool script for adding a new component to the fish.
 *  Please keep around for future components.
   * [@MenuItem("WEMOTools/AddThrottleToFish")]
	static void AddThrottleToCritters()
	{
//		BuildAssetBundleOptions options = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets;

//		string build_info_string = System.DateTime.Now.ToString("G") + " " + GitUtilities.VersionString();
		string path = "Standard Assets/Prefabs/Fish";
		string [] fishAssetFiles = Directory.GetFiles(Application.dataPath+"/"+path,"*.*",SearchOption.AllDirectories);
		foreach(string fishFileNameAndDir in fishAssetFiles)
		{
			// get our fish file name (which is a full path filename) relative to our asset path.
//			WemoLog.Log(fishFileNameAndDir);
            if (fishFileNameAndDir.EndsWith(".meta")) // ignore .meta files
                continue;

			string fishFileName = fishFileNameAndDir.Replace("\\", "/");
			int index = fishFileName.LastIndexOf("/Assets/");
			fishFileName = fishFileName.Substring(index + 1);
//			WemoLog.Log(fishFileName);

			GameObject fishy = AssetDatabase.LoadMainAssetAtPath(fishFileName) as GameObject;
			
			if (fishy != null)
			{
				ThrottledFishSteering new_steer = fishy.AddComponent<ThrottledFishSteering>();
				GeneralMotionData gmd = fishy.GetComponent<GeneralMotionData>();
		
				if( new_steer == null || gmd == null ) {
					continue;
				}
				
				new_steer.yawAccel = gmd.steeringYawAccel;
				new_steer.yawDecel = gmd.steeringYawDecel;
				new_steer.yawMaxSpeed = gmd.steeringYawMaxSpeed;
				new_steer.pitchAccel = gmd.steeringPitchAccel;
				new_steer.pitchDecel = gmd.steeringPitchDecel;
				new_steer.pitchMaxSpeed = gmd.steeringPitchMaxSpeed;
				new_steer.rollAccel = gmd.steeringRollAccel;
				new_steer.rollDecel = gmd.steeringRollDecel;
				new_steer.rollMaxSpeed = gmd.steeringRollMaxSpeed;
				new_steer.rollOnYawMult = gmd.steeringRollMult;
				new_steer.rollStrafingMult = gmd.steeringRollStrafingMult;

//				Object fishprefab = EditorUtility.CreateEmptyPrefab(fishFileName);
//				EditorUtility.ReplacePrefab(fishy,fishy);
			}
		}
		AssetDatabase.SaveAssets();				                   
	}*/
	public string domainNamePrefix = "";
	public string pathToCloudRepro = "C:/Projects/Wemo/theBluBuild/android/data/";
	public bool buildVRCritters = true;

//Borut
//	public string domainNamePrefix = "scott.sandbox.";
//	public string pathToCloudRepro = "E:\\theblu\\";
//	private int numberOfFileCopied = 0;
    [@MenuItem("WEMOTools/Build/Bundle All Fish Prefabs")]
	static void CreateWizard()
	{
		
		BundleAssets me = ScriptableWizard.DisplayWizard("Bundle All Fish Prefabs", typeof(BundleAssets), "Build") as BundleAssets;
		
		string LastBundleBuildSettingFile = string.Concat(Application.dataPath, "/../WEMOData/LastBundleBuildSetting.json");
		if(File.Exists(LastBundleBuildSettingFile))
		{
			string[] lastBundleBuildSettings = File.ReadAllLines(LastBundleBuildSettingFile);
			me.domainNamePrefix = lastBundleBuildSettings[0];
			me.pathToCloudRepro = lastBundleBuildSettings[1];
		}
		
	}

 	void OnWizardUpdate()
	{
		helpString = "Domain Name Prefix gets appended to theblu.com\n" +
			"Path To Cloud Repro is exactly that.";
	}
	
	void BuildAllCritters()
	{
		BuildAssetBundleOptions options = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets;

		string build_info_string = System.DateTime.Now.ToString("G") + " " + GitUtilities.VersionString();
		string path = "Standard Assets/Prefabs/fish";
		if( buildVRCritters ) {
			path += "_vr";
		}

		string bundleDirectoryPath = "AssetBundles/FishPrefabs/";
		string [] fishAssetFiles = Directory.GetFiles(Application.dataPath+"/"+path,"*.*",SearchOption.AllDirectories);
		foreach(string fishFileNameAndDir in fishAssetFiles)
		{
			// get our fish file name (which is a full path filename) relative to our asset path.
//			WemoLog.Log(fishFileNameAndDir);
            if (fishFileNameAndDir.EndsWith(".meta")) // ignore .meta files
                continue;

			string fishFileName = fishFileNameAndDir.Replace("\\", "/");
			int index = fishFileName.LastIndexOf("/Assets/");
			fishFileName = fishFileName.Substring(index + 1);
//			WemoLog.Log(fishFileName);

			GameObject fishy = AssetDatabase.LoadMainAssetAtPath(fishFileName) as GameObject;
			
			if (fishy != null)
			{
				WemoItemData item_data = fishy.GetComponent<WemoItemData>();
				
				if( item_data != null ) 
				{
//					Debug.Log("Updating Build String : " + build_info_string);
					item_data.buildInfoString = build_info_string;
				}
				if(!System.IO.Directory.Exists(bundleDirectoryPath))
			    {
		            Directory.CreateDirectory(bundleDirectoryPath);
					Debug.Log("Creating : " + bundleDirectoryPath);
			    }
				string bundlePath = bundleDirectoryPath + fishy.name + ".unity3d";
				// Build the resource file from the active selection.
				BuildPipeline.BuildAssetBundle(fishy, null, bundlePath, options, EditorUserBuildSettings.activeBuildTarget);
			}
        }
		
	}
	
    void OnWizardCreate() 
    {	
		BuildAllCritters();
		
        DeployCrittersToSandbox(domainNamePrefix, pathToCloudRepro);
        
		string LastBundleBuildSettingFile = string.Concat(Application.dataPath, "/../WEMOData/LastBundleBuildSetting.json");
		string WEMODataDirectory = string.Concat(Application.dataPath, "/../WEMOData/");
		if(!Directory.Exists(WEMODataDirectory))
		{
			Directory.CreateDirectory(WEMODataDirectory);
		}
		if(File.Exists(LastBundleBuildSettingFile))
		{
			File.Delete(LastBundleBuildSettingFile);
		}

		string[] lastBundleBuildSettings = new string[2];
		lastBundleBuildSettings[0] = domainNamePrefix;
		lastBundleBuildSettings[1] = pathToCloudRepro;
		File.WriteAllLines(LastBundleBuildSettingFile, lastBundleBuildSettings);
    }
    
    [@MenuItem("WEMOTools/Build/Deploy Critters To Sandbox")]
	public static void DeployCrittersToSandbox( ) 
	{
		DeployCrittersToSandbox("", "C:/Projects/Wemo/theBluBuild/android/data");
	}

	public static void DeployCrittersToSandbox( string domain_name, string sandbox_path ) 
    {
		try
		{
	        string jsonContent = string.Empty;
	        string urlOriginal = "http://" + domain_name + "theblu.com/api/itemvariants/include=version/allvariants.json";

	        string url = urlOriginal;// + currentProcessingPage;
	        Debug.LogWarning(url);
	        
	        // Creates an HttpWebRequest with the specified URL. 
	        HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url); 
	        // Sends the HttpWebRequest and waits for the response.         
	        HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse(); 
	        // Gets the stream associated with the response.
	        Stream receiveStream = myHttpWebResponse.GetResponseStream();
	        Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
	        // Pipes the stream to a higher level stream reader with the required encoding format. 
	        StreamReader readStream = new StreamReader( receiveStream, encode );
	        //           Debug.Log("begin to read");
	        
	        jsonContent = readStream.ReadToEnd();
	        Debug.Log(jsonContent);
	        // Releases the resources of the response.
	        myHttpWebResponse.Close();
	        // Releases the resources of the Stream.
	        readStream.Close();
	        
	        JSONObject jsonFile = new JSONObject(jsonContent);
	        //Debug.Log(jsonFile.print(true));
	        if(!jsonFile)
	        {
	            Debug.LogError("something f wrong");
	        }
	        JSONObject variants = jsonFile.GetField("variants");
	        string[] fishBundleFiles = Directory.GetFiles("../unity/AssetBundles/FishPrefabs/");
	        //           Debug.Log(fishBundleFiles.Length);
	        
	        Debug.Log("variants.list.Count = " + variants.list.Count);
	        
	        // if our request has less than our request amount, we are done this iteration
	        /*           if( variants.list.Count < 20 ) {
	        done = true;
	        }*/
	        
	        foreach(JSONObject variant in variants.list)
	        {
	            JSONObject version = variant.GetField("version");
	            if( version == null )
				{
					Debug.Log("DeployCrittersToSandbox Skipping null");
					continue;
	            }

				JSONObject urlKey = variant.GetField("urlkey");
				if( urlKey == null )
				{
					Debug.Log("DeployCrittersToSandbox Skipping null urlkey");
					continue;
				}

				JSONObject metaData = version.GetField("metadata");
	            if( metaData == null ) 
				{
					Debug.Log("DeployCrittersToSandbox Skipping null metadata: "+ urlKey.print(true));
					continue;
	            }

	            JSONObject files = metaData.GetField("files");
	            if( files == null )
				{
					Debug.Log("DeployCrittersToSandbox Skipping null files: "+urlKey.n);
					continue;
	            }

	            JSONObject unityPath = files.GetField("unity");
	            if( unityPath == null )
				{
					Debug.Log("DeployCrittersToSandbox Skipping null unityPath: "+urlKey.n);
					continue;
	            }

	            Debug.Log(unityPath.str);

	//			string websiteFishBundleDirectory = sandbox_path + "web/sites/theblu.com/htdocs/public/";
				//Borut
				string websiteFishBundleDirectory = "C:/Projects/Wemo/theBluBuild/android/data/";
	            string fishBundleSubPath = unityPath.str.Replace("\\","");
	            string moveToWebsitePath = websiteFishBundleDirectory + fishBundleSubPath;     
	            string urlKeyName = urlKey.str;
	            
	            int last_dir_idx = moveToWebsitePath.LastIndexOf("/");
	            string raw_dir = moveToWebsitePath.Substring(0,last_dir_idx);
	            if( !Directory.Exists(raw_dir) )
				{
	                Directory.CreateDirectory(raw_dir);
	            }
	            
	            foreach(string fishBundleFile in fishBundleFiles)
	            {
	                if(fishBundleFile.EndsWith(".meta"))
	                {
	                    continue;
	                }

	                string bundleName = fishBundleFile.ToLower();
	                
	                
//                   Debug.Log("bundleName "+ bundleName + "urlKeyName " + urlKeyName);
	                if( bundleName.Contains(urlKeyName.ToLower()))
	                {
	                    Debug.Log(urlKeyName + ":: copying from " + fishBundleFile + "to " + moveToWebsitePath);
//                       numberOfFileCopied +=1;
						string destDirName = Path.GetDirectoryName(moveToWebsitePath);
						if (!Directory.Exists(destDirName))
						{
							Directory.CreateDirectory(destDirName);
						}
	//                    File.Copy(fishBundleFile, sandbox_path,true);
	                    File.Copy(fishBundleFile, moveToWebsitePath,true);
	                    break;
	                }
	            }
	        //               Debug.LogWarning("currentProcessingPage = " + currentProcessingPage);
	        //               Debug.Log("numberOfFileCopied = " + numberOfFileCopied);
	        }
		}
		catch (System.Exception e)
		{
			Debug.LogError("DeployCrittersToSandbox received exception: " + e.Message);
		}
	}
	
	[@MenuItem("WEMOTools/Build/Music Bundles")]
    static void BuildMusicBundles()
    {
        //TODO: The paths are hard-coded now. Need to be more flexible in converting directory paths, relative and absolute.

        string assetBundlesRelativePath = "AssetBundles/Audio/";
        string assetBundlesFolder = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/Assets")) + "/" + assetBundlesRelativePath;

        if (!Directory.Exists(assetBundlesFolder.Replace("/", "\\")))
        {
            try
            {
                Directory.CreateDirectory(assetBundlesFolder);
            }
            catch
            {
                //Debug.Log("Cannot initialize folder for archiving asset bundles. Please manually create the folder: " + assetBundlesFolder);
            }
        }

        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/Global", assetBundlesRelativePath);
        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/Reef", assetBundlesRelativePath);
        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/Sandy", assetBundlesRelativePath);
        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/Cliff", assetBundlesRelativePath);
        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/Open", assetBundlesRelativePath);
        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/MyGallery", assetBundlesRelativePath);
        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/Garden", assetBundlesRelativePath);
        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/DarkDepths", assetBundlesRelativePath);
        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/Seychelles", assetBundlesRelativePath);
        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/ScreenSaver", assetBundlesRelativePath);
        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/Prochloro", assetBundlesRelativePath);
        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/Kelp", assetBundlesRelativePath);
        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/BluWhale", assetBundlesRelativePath);
        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/Iceberg", assetBundlesRelativePath);
        BuildAudioBundles(Application.dataPath + "/Standard Assets/Audio/Music/OculusShark", assetBundlesRelativePath);
    }
     
    static void BuildAudioBundles(string sourceFolder, string desFolder)
    {
//		string sandboxDesFolder = Path.Combine(desFolder, "unity/audio");
		//Borut
//		string sandboxDesFolder = "../web/sites/theblu.com/htdocs/localfiles/public/unity/audio/";
        BuildAssetBundleOptions options = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets;
        string[] audioAssetFiles = Directory.GetFiles(sourceFolder);

/*		if (!Directory.Exists(sandboxDesFolder))
		{
			Directory.CreateDirectory(sandboxDesFolder);
		}*/
        foreach (string audioFullPath in audioAssetFiles)
        {
            if (audioFullPath.EndsWith(".meta")) // ignore .meta files
                continue;

            string localPath = audioFullPath.Substring(audioFullPath.LastIndexOf("/Assets/") + 1).Replace("\\", "/");
            Object audio = AssetDatabase.LoadMainAssetAtPath(localPath);

            if (audio != null)
            {
                string bundlePath = desFolder + audio.name + ".unity3d";
//				string sandboxPath = sandboxDesFolder + audio.name + ".unity3d";
                BuildPipeline.BuildAssetBundle(audio, null, bundlePath, options, EditorUserBuildSettings.activeBuildTarget);

/*				if (!Directory.Exists(sandboxDesFolder))
				{
					Directory.CreateDirectory(sandboxDesFolder);
				}

				File.Copy(bundlePath,sandboxPath,true);*/
            }
        }
    }
}


//BUILD Seabed PREFABS
public class BundleSeabedAssets
{
    //[@MenuItem("WEMOTools/Build/Bundle All Seabed Prefabs")]
    static void BuildSeabedPrefabs() 
    {
		BuildAssetBundleOptions options = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets;
		
		string path = "Standard Assets/Prefabs/seabed";
		string [] plantAssetFiles = Directory.GetFiles(Application.dataPath+"/"+path,"*.*",SearchOption.AllDirectories);
		foreach(string plantFileNameAndDir in plantAssetFiles)
		{
            if (plantFileNameAndDir.EndsWith(".meta")) // ignore .meta files
                continue;

			string plantFileName = plantFileNameAndDir.Replace("\\", "/");
			int index = plantFileName.LastIndexOf("/Assets/");
			plantFileName = plantFileName.Substring(index + 1);

			Object planty = AssetDatabase.LoadMainAssetAtPath(plantFileName);
			if (planty != null)
			{
				string bundlePath = "AssetBundles/SeabedPrefabs/" + planty.name + ".unity3d";
				// Build the resource file from the active selection.
				BuildPipeline.BuildAssetBundle(planty, null, bundlePath, options);
			}
        }
    } 
} 