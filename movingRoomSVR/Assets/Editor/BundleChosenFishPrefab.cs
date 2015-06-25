using UnityEngine;
using UnityEditor;
//using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class BundleChosenFishPrefab : ScriptableWizard
{
    public string domainNamePrefix = "scott.sandbox.";
    public string pathToCloudRepo = "E:\\theblu\\";
    public bool deployBundlesToSandbox = false;
	public List<GameObject> fishesToBuild = new List<GameObject>();
	//AssetDatabase.GetAssetPath
	private List<string> fishesPrefabFilePath = new List<string>();
	[@MenuItem("WEMOTools/Build/Bundle Select Fish Prefabs")]
	static void CreateWindow() 
	{
        // Creates the wizard for display
        ScriptableWizard.DisplayWizard("Bundle Select Fish", 
            typeof(BundleChosenFishPrefab), 
            "Build Bundle");
    }
	void OnWizardCreate()
	{
		string bundleDirectoryPath = "AssetBundles/FishPrefabs/";
		BuildAssetBundleOptions options = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets;
		string build_info_string = System.DateTime.Now.ToString("G") + " " + GitUtilities.VersionString();
		foreach(GameObject gameObject in fishesToBuild)
		{
			if(gameObject != null)
			{
				string prefabPath = AssetDatabase.GetAssetPath(gameObject);
				fishesPrefabFilePath.Add(prefabPath);
			}
		}
		foreach(string fishPrefabPath in fishesPrefabFilePath)
		{
			string fishFileName = fishPrefabPath.Replace("\\", "/");
			int index = fishFileName.LastIndexOf("/Assets/");
			fishFileName = fishFileName.Substring(index + 1);
			Debug.Log(fishFileName);
			GameObject fishy = AssetDatabase.LoadMainAssetAtPath(fishFileName) as GameObject;
			if (fishy != null)
			{
				WemoItemData item_data = fishy.GetComponent<WemoItemData>();
				
				if( item_data != null ) 
				{
					Debug.Log("Updating Build String : " + build_info_string);
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
        
        if( deployBundlesToSandbox ) {
            BundleAssets.DeployCrittersToSandbox( domainNamePrefix, pathToCloudRepo );
        }
	}
}

