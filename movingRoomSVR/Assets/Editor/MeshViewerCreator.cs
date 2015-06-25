using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
public class MeshViewerCreator 
{
	static string baseScenePath = "Assets/Scenes/" + "preview-turntable" + ".unity";
	static string buildPath = "Assets/PrefabStaging";
	public static  void LoadFishToPreviewScene(Object prefab, bool bWithTexture, bool bWithAnimation)
	{
		if (!HasSceneOpened(baseScenePath))
			return;
		
		//PreviewTurntableApp preview_app = Object.FindObjectOfType(typeof(PreviewTurntableApp)) as PreviewTurntableApp;
		
		GameObject theMesh = GameObject.Instantiate(prefab) as GameObject;

		
		Transform[] childrenTransforms = theMesh.GetComponentsInChildren<Transform>();
		int numberOfMeshEnabled = 0;
		string[] filePaths = Directory.GetFiles(buildPath);
		string specTexPath = string.Empty;
		string normalTexPath = string.Empty;
		string diffTexPath = string.Empty;
		List<string> animationFilePaths = new List<string>();
	
		foreach(string filePath in filePaths)
		{
			if(filePath.EndsWith(".meta"))
			{
				continue;
			}
			string filePathLowered = filePath.ToLower();
			
			if(filePathLowered.Contains("norm") && filePathLowered.Contains("lod0"))
			{
				normalTexPath = filePath;
			}
			if(filePathLowered.Contains("spec") && filePathLowered.Contains("lod0"))
			{
				specTexPath = filePath;
			}
			if((filePathLowered.Contains("main") || filePathLowered.Contains("dif")) && filePathLowered.Contains("lod0"))
			{
				diffTexPath = filePath;
			}
			if(filePathLowered.Contains("@"))
			{
				animationFilePaths.Add(filePathLowered);
				Debug.Log(filePathLowered);
			}
		}
		Texture casticTex = AssetDatabase.LoadAssetAtPath("Assets/Standard Assets/Textures/caustics.png",typeof(Texture)) as Texture;
		Texture noiseTex = AssetDatabase.LoadAssetAtPath("Assets/Standard Assets/Textures/noise.jpg", typeof(Texture)) as Texture;
		Texture specTex = AssetDatabase.LoadAssetAtPath(specTexPath, typeof(Texture)) as Texture;
		Texture normalTex = AssetDatabase.LoadAssetAtPath(normalTexPath, typeof(Texture)) as Texture;
		Texture diffTex =  AssetDatabase.LoadAssetAtPath(diffTexPath, typeof(Texture)) as Texture;
		foreach(Transform childTransform in childrenTransforms)
		{
			childTransform.gameObject.SetActive( false );
			
			if(childTransform.name.Contains("lod0") || childTransform.name.Contains("LOD0"))
			{
				numberOfMeshEnabled += 1;
				childTransform.gameObject.SetActive( true );
				if(!bWithTexture)
				{
					if(childTransform.GetComponent<SkinnedMeshRenderer>())
					{
						SkinnedMeshRenderer renderer = childTransform.GetComponent<SkinnedMeshRenderer>();
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
							renderer.sharedMaterial.shader = Shader.Find("Underwater/DiffCaus");
							renderer.sharedMaterial.SetTexture("_MainTex", null);
							renderer.sharedMaterial.SetTexture("_CausticTex", null);
							renderer.sharedMaterial.SetTexture("_NoiseTex", null);
						}
					}
				}
				else
				{
				
					
					if(childTransform.GetComponent<SkinnedMeshRenderer>())
					{
						SkinnedMeshRenderer renderer = childTransform.GetComponent<SkinnedMeshRenderer>();
						renderer.sharedMaterial.shader = Shader.Find("Underwater/SpecNormalCausticsDepth2pass");
						renderer.sharedMaterial.SetTexture("_CausticTex", casticTex);
						renderer.sharedMaterial.SetTexture("_NoiseTex", noiseTex);
						if(specTex)
							renderer.sharedMaterial.SetTexture("_SpecMap", specTex);
						if(normalTex)
							renderer.sharedMaterial.SetTexture("_NormMap", normalTex);
						if(diffTex)
							renderer.sharedMaterial.SetTexture("_MainTex", diffTex);
					}
					else
					{
						if(childTransform.GetComponent<MeshRenderer>())
						{
							MeshRenderer renderer = childTransform.GetComponent<MeshRenderer>();
							renderer.sharedMaterial.shader = Shader.Find("Underwater/SpecNormalCausticsDepth2pass");
							renderer.sharedMaterial.SetTexture("_CausticTex", casticTex);
							renderer.sharedMaterial.SetTexture("_NoiseTex", noiseTex);
						if(specTex)
							renderer.sharedMaterial.SetTexture("_SpecMap", specTex);
						if(normalTex)
							renderer.sharedMaterial.SetTexture("_NormMap", normalTex);
						if(diffTex)
							renderer.sharedMaterial.SetTexture("_MainTex", diffTex);
						}
					}
				}
			}
		}
		if(numberOfMeshEnabled < 1)
		{
			foreach(Transform childTransform in childrenTransforms)
			{
				childTransform.gameObject.SetActive( true );
				if(!bWithTexture)
				{
					if(childTransform.GetComponent<SkinnedMeshRenderer>())
					{
						SkinnedMeshRenderer renderer = childTransform.GetComponent<SkinnedMeshRenderer>();
						if(!renderer.sharedMaterial)
						{
							renderer.sharedMaterial = new Material(Shader.Find("Underwater/DiffCaus"));
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
								renderer.sharedMaterial = new Material(Shader.Find("Underwater/DiffCaus"));
							}
							renderer.sharedMaterial.shader = Shader.Find("Underwater/DiffCaus");
							renderer.sharedMaterial.SetTexture("_MainTex", null);
							renderer.sharedMaterial.SetTexture("_CausticTex", null);
							renderer.sharedMaterial.SetTexture("_NoiseTex", null);
						}
					}
				}
				else
				{
				
					
					if(childTransform.GetComponent<SkinnedMeshRenderer>())
					{
						SkinnedMeshRenderer renderer = childTransform.GetComponent<SkinnedMeshRenderer>();
						if(!renderer.sharedMaterial)
						{
							renderer.sharedMaterial = new Material(Shader.Find("Underwater/SpecNormalCausticsDepth2pass"));
						}
						renderer.sharedMaterial.shader = Shader.Find("Underwater/SpecNormalCausticsDepth2pass");
						renderer.sharedMaterial.SetTexture("_CausticTex", casticTex);
						renderer.sharedMaterial.SetTexture("_NoiseTex", noiseTex);
						if(specTex)
							renderer.sharedMaterial.SetTexture("_SpecMap", specTex);
						if(normalTex)
							renderer.sharedMaterial.SetTexture("_NormMap", normalTex);
						if(diffTex)
							renderer.sharedMaterial.SetTexture("_MainTex", diffTex);
					}
					else
					{
						if(childTransform.GetComponent<MeshRenderer>())
						{
							MeshRenderer renderer = childTransform.GetComponent<MeshRenderer>();
							if(!renderer.sharedMaterial)
							{
								renderer.sharedMaterial = new Material(Shader.Find("Underwater/SpecNormalCausticsDepth2pass"));
							}
							renderer.sharedMaterial.shader = Shader.Find("Underwater/SpecNormalCausticsDepth2pass");
							renderer.sharedMaterial.SetTexture("_CausticTex", casticTex);
							renderer.sharedMaterial.SetTexture("_NoiseTex", noiseTex);
						if(specTex)
							renderer.sharedMaterial.SetTexture("_SpecMap", specTex);
						if(normalTex)
							renderer.sharedMaterial.SetTexture("_NormMap", normalTex);
						if(diffTex)
							renderer.sharedMaterial.SetTexture("_MainTex", diffTex);
						}
					}
				}
			}
		}
		
		theMesh.transform.position = GameObject.Find("sbpoint").transform.position;
		if(bWithAnimation)
		{
			theMesh.SetActive( true );
			GameObject root =  theMesh.transform.FindChild("root").gameObject;
			root.SetActive(true);
			foreach(string animationFile in animationFilePaths)
			{
				GameObject animObj = AssetDatabase.LoadAssetAtPath(animationFile, typeof(GameObject)) as GameObject;
				Animation anim = animObj.GetComponent<Animation>();
				AnimationClip animClip = anim.clip;
				AnimationClip clip =  UnityEngine.AnimationClip.Instantiate(animClip) as AnimationClip;
				clip.name = animClip.name;
				//AssetDatabase.DeleteAsset(animationFile);
				AssetDatabase.Refresh();
				theMesh.GetComponent<Animation>().AddClip(clip,clip.name);
				
			}
			theMesh.AddComponent<MeshViewerAnimationController>();
		}
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

