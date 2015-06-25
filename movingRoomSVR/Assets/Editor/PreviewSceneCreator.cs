using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
public class PreviewSceneCreator //: MonoBehaviour
{
	static string baseScenePath = "Assets/Scenes/" + "preview" + ".unity";
	static string oldBlueLoadingData;
	static string buildPath = "Assets/PrefabStaging";
	//static string beingProcessedStage = string.Empty;
	public static  void LoadFishToPreviewScene(Object prefab)
	{
		//beingProcessedStage = stage;
		if (!HasSceneOpened(baseScenePath))
		{
			return;
		}
		
		//PreviewApp preview_app = Object.FindObjectOfType(typeof(PreviewApp)) as PreviewApp;
		
		TestCritters testCritters = Object.FindObjectOfType(typeof(TestCritters)) as TestCritters;
		testCritters.add(prefab);
		
		
		
		PreviewSceneMakerDebugCameraMode camera = Object.FindObjectOfType(typeof(PreviewSceneMakerDebugCameraMode)) as PreviewSceneMakerDebugCameraMode;
		GameObject mesh = GameObject.Instantiate(testCritters.testCritters[0].critter) as GameObject;
		BoxCollider boxCollider = mesh.GetComponentInChildren<BoxCollider>() as BoxCollider;
		camera.speed = boxCollider.size.z * 2f;
		
		if(boxCollider.size.z > 80.0)
		{
			FishBowl[] fishBowls = Object.FindObjectsOfType(typeof(FishBowl)) as FishBowl[];
			foreach(FishBowl f in fishBowls)
			{
				f.defaultBowl = false;
			}
			FishBowl fish = GameObject.Find("BarrracudaFishBowl").GetComponent<FishBowl>();
			fish.defaultBowl = true;
		}
		
		GameObject.DestroyImmediate(mesh);
		CSAdjustGlobalShderParams.AdjustGlobalShaderParams();
		BuildScene();
		AssetDatabase.SaveAssets();
	}
	static bool HasSceneOpened(string scenePath)
	{
		// force open the chosen view to build
		if (!EditorApplication.currentScene.Equals(scenePath))
		{
			if (File.Exists(scenePath))
			{
				EditorApplication.OpenScene(scenePath);	
			
				
			//	oceanSphereController.testCritters.SetValue(prefab,0);
			}
			else
			{
				Debug.LogWarning("Cannot find " + scenePath);
				return false;
			}
		}

		return true;
	}
	static void BuildScene()
	{
		string[] scenes = new string[] { baseScenePath };
		BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.WebPlayerStreamed, BuildOptions.None);
	}
	static string GetFullPath(string relative)
	{
		string tmp = Application.dataPath;
		return tmp.Substring(0, tmp.LastIndexOf("/") + 1) + relative;
	}
	static string GetSceneName(string path)
	{
		return path.Substring(path.LastIndexOf("/") + 1, path.LastIndexOf(".unity") - path.LastIndexOf("/") - 1);
	}

}

