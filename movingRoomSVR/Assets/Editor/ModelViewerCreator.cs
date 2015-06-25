using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class ModelViewerCreator 
{
	static string baseScenePath = "Assets/Scenes/" + "preview-turntable" + ".unity";
	static string buildPath = "Assets/PrefabStaging";
	public static  void LoadFishToPreviewScene()
	{
		if (!HasSceneOpened(baseScenePath))
			return;
		
		//PreviewTurntableApp preview_app = Object.FindObjectOfType(typeof(PreviewTurntableApp)) as PreviewTurntableApp
		int numberOfMeshEnabled = 0;;
	
		UnityEngine.Object prefab = new UnityEngine.Object();

		string[] filePaths = Directory.GetFiles(buildPath);
		string fbxFilePath = string.Empty;
	
		foreach(string filePath in filePaths)
		{
			if(filePath.EndsWith(".meta"))
			{
				continue;
			}
			string filePathLowered = filePath.ToLower();
			if(filePathLowered.EndsWith(".fbx") && !filePathLowered.Contains("@"))
			{
				fbxFilePath = filePathLowered;
			}

		}
		
		prefab = AssetDatabase.LoadMainAssetAtPath(fbxFilePath);
		GameObject theMesh  = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
		Transform[] childrenTransforms = theMesh.GetComponentsInChildren<Transform>();
		foreach(Transform childTransform in childrenTransforms)
		{
			childTransform.gameObject.SetActive( false );
			
			if(childTransform.name.Contains("lod0") || childTransform.name.Contains("LOD0"))
			{
				numberOfMeshEnabled += 1;
				childTransform.gameObject.SetActive( true );
				if(childTransform.GetComponent<SkinnedMeshRenderer>())
				{
					SkinnedMeshRenderer renderer = childTransform.GetComponent<SkinnedMeshRenderer>();
					renderer.sharedMaterial = new Material(Shader.Find("Underwater/Fish_LOD2"));
					renderer.sharedMaterial.SetTexture("_MainTex", null);
					renderer.sharedMaterial.SetTexture("_CausticTex", null);
					renderer.sharedMaterial.SetTexture("_NoiseTex", null);
				}
				else
				{
					if(childTransform.GetComponent<MeshRenderer>())
					{
						MeshRenderer renderer = childTransform.GetComponent<MeshRenderer>();
						renderer.sharedMaterial.shader = Shader.Find("Underwater/Fish_LOD2");
						renderer.sharedMaterial.SetTexture("_MainTex", null);
						renderer.sharedMaterial.SetTexture("_CausticTex", null);
						renderer.sharedMaterial.SetTexture("_NoiseTex", null);
					}
				}
			}
		}
		if(numberOfMeshEnabled < 1)
		{
			foreach(Transform childTransform in childrenTransforms)
			{
				childTransform.gameObject.SetActive( true );
				
				if(childTransform.GetComponent<SkinnedMeshRenderer>())
				{
					SkinnedMeshRenderer renderer = childTransform.GetComponent<SkinnedMeshRenderer>();
					if(!renderer.sharedMaterial)
					{
						renderer.sharedMaterial = new Material(Shader.Find("Underwater/Fish_LOD2"));
					}
					renderer.sharedMaterial.shader = Shader.Find("Underwater/DiffCaus");
					renderer.sharedMaterial.SetTexture("_MainTex", null);
					renderer.sharedMaterial.SetTexture("_CausticTex", null);
					renderer.sharedMaterial.SetTexture("_NoiseTex", null);
				}
				else
				{
					if(childTransform.GetComponent<MeshRenderer>())
					{
						MeshRenderer renderer = childTransform.GetComponent<MeshRenderer>();
						if(!renderer.sharedMaterial)
						{
							renderer.sharedMaterial = new Material(Shader.Find("Underwater/Fish_LOD2"));
						}
						renderer.sharedMaterial.shader = Shader.Find("Underwater/Fish_LOD2");
						renderer.sharedMaterial.SetTexture("_MainTex", null);
						renderer.sharedMaterial.SetTexture("_CausticTex", null);
						renderer.sharedMaterial.SetTexture("_NoiseTex", null);
					}
				}
			}
		}
		theMesh.transform.position = GameObject.Find("sbpoint").transform.position;
		TestMesh testMesh = Object.FindObjectOfType(typeof(TestMesh)) as TestMesh;
		testMesh = new TestMesh();
		testMesh.add(theMesh, prefab);
		
		
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