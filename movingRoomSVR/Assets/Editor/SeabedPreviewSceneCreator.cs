using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
public class SeabedPreviewSceneCreator : MonoBehaviour
{
	static string baseScenePath = "Assets/Scenes/" + "preview-seabed" + ".unity";
	static string oldBlueLoadingData;
	static string buildPath = "Assets/PrefabStaging";
	// Use this for initialization
	public static  void LoadSeabedToPreviewScene(Object prefab)
	{
		if (!HasSceneOpened(baseScenePath,prefab))
			return;
		CSAdjustGlobalShderParams.AdjustGlobalShaderParams();
		BuildScene();
		AssetDatabase.SaveAssets();
	}
	static bool HasSceneOpened(string scenePath, Object prefab)
	{
		// force open the chosen view to build
		if (!EditorApplication.currentScene.Equals(scenePath))
		{
			if (File.Exists(scenePath))
			{
				EditorApplication.OpenScene(scenePath);	
				
				List<Transform> sb_spawnpoints = new List<Transform>();
				GameObject sb_spawnpointsHolder = GameObject.Find("sb_spawnpoints");
				Transform[] sb_spawnpointsTransforms = sb_spawnpointsHolder.transform.GetComponentsInChildren<Transform>();
				Debug.Log("sb_spawnpointsTransforms.Length = "  + sb_spawnpointsTransforms.Length);
				for(int i = 0; i < sb_spawnpointsTransforms.Length; ++i )
				{
					Instantiate(prefab,sb_spawnpointsTransforms[i].position,sb_spawnpointsTransforms[i].rotation);
					sb_spawnpoints.Add(sb_spawnpointsTransforms[i]);
				}
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

