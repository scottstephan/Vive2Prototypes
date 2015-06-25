using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
public class MeshTextureViewerCreator
{
	public static Mesh meshToDisplayInScene;
	public static string defaultPath = "Assets/Scenes/";
	static string textureDirInEditor = string.Empty;
	static string fishGeoDirectory = "Assets/Standard Assets/Geo/fish/";
	static string seabedGeoDirectory = "Assets/Standard Assets/Geo/seabed/";
	
//	static string baseScenePath = "Assets/Scenes/" + "preview-turntable" + ".unity";
	public static string baseLevelToBuild = "preview-turntable";
	public static string meshViewerPath = "../MeshViewer/";
	public static string generatePath = "Assets/Standard Assets/Scripts/TheBlueLoadingData.cs";
	public static string oldBlueLoadingData;
	public static string buildFromFolder = "Assets/Scenes/builddata/";
	public static string baseScenePath
	{
		get
		{
			return buildFromFolder + baseLevelToBuild + ".unity";
		}
	}
	
	public static void CopySingleSceneFileAndLightMap()
	{
		string[] copyFromScenefilePaths = Directory.GetFiles(defaultPath,"*.unity", SearchOption.AllDirectories);
		
		
		foreach(string path in copyFromScenefilePaths)
		{
			if(path.EndsWith(".meta"))
		    {
				continue;
			}
			if(path.Contains("builddata"))
			{
				continue;
			}
			string sceneFilePath = path.Replace("\\", "/");
			string sceneFileDirectory = sceneFilePath.Substring(0, sceneFilePath.LastIndexOf("/")+1);
			sceneFileDirectory = sceneFileDirectory.Replace(defaultPath, buildFromFolder);
		/*	if(!Directory.Exists( sceneFileDirectory))
			{
				Directory.CreateDirectory(sceneFileDirectory);
			}
			 */
			if(sceneFilePath.Contains(baseLevelToBuild + ".unity"))
			{
				if(!Directory.Exists( sceneFileDirectory))
				{
					Directory.CreateDirectory(sceneFileDirectory);
				}
				if(File.Exists(baseScenePath))
				{
					File.Delete(baseScenePath);
				}
				AssetDatabase.Refresh();
				AssetDatabase.CopyAsset(sceneFilePath, baseScenePath);
				AssetDatabase.Refresh();
			}
		}
		if(Directory.Exists(defaultPath + baseLevelToBuild + "/"))
		{
			string[] copyFromLightMapFilePaths = Directory.GetFiles(defaultPath + baseLevelToBuild + "/");
			foreach(string path in copyFromLightMapFilePaths)
			{
				if(path.EndsWith(".meta"))
				{
					continue;
				}
				if(path.Contains("builddata"))
				{
					continue;
				}
				string lightMapPath = path.Replace("\\", "/");
				string lightMapFileDirectory = lightMapPath.Substring(0, lightMapPath.LastIndexOf("/")+1);
				lightMapFileDirectory = lightMapFileDirectory.Replace(defaultPath, buildFromFolder);
				if(!lightMapFileDirectory.EndsWith("/"))
				{
					lightMapFileDirectory += "/";
				}
				if(!Directory.Exists( lightMapFileDirectory))
				{
					
					Directory.CreateDirectory(lightMapFileDirectory);
					AssetDatabase.Refresh();
				}
				string lightMapCopyToPath = lightMapPath.Replace(defaultPath, buildFromFolder);
				File.Copy(lightMapPath, lightMapCopyToPath,true);
				AssetDatabase.Refresh();
			}
		}
	}
	public static void LoadFishToPreviewScene(List<UnityEngine.Object> meshList, bool bIsSeabed, JSONObject currentBeingProcessedJSONData)
	{
		CopySingleSceneFileAndLightMap();
		
		if (!HasSceneOpened(baseScenePath,null))
			return;
		foreach(UnityEngine.Object prefab in meshList)
		{
			GameObject go = new GameObject(prefab.name);
			go.AddComponent(typeof(MeshRenderer));
			go.AddComponent(typeof(MeshFilter));
			MeshFilter meshFilter = go.GetComponent<MeshFilter>();
			MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
			meshFilter.mesh = prefab as Mesh;
			string name = go.name;
			
			//string[] filesInMaterialsDirectory = Directory.GetFiles(Application.dataPath+ "/Standard Assets/Materials/");
			//Shader shader = Shader.Find("Underwater/DiffCaus");
			//Material materialToApply = new Material(shader); 
			
			Material objMaterial = null; // Assume that the renderer only has one Material, and modify that one
			if(bIsSeabed)
			{
				textureDirInEditor = seabedGeoDirectory;
				objMaterial = new Material(Shader.Find("Underwater/DiffNormalDepthShad"));
				if(name.Contains("_alpha_"))
				{
					objMaterial = new Material(Shader.Find("Underwater/VegetationDispTex"));
				}
				if(name.Contains("_wavy_"))
				{
					objMaterial = new Material(Shader.Find("Underwater/VegetationDispUvTex"));
				}	
			}
			else
			{
				if (name.Contains("_alpha_")) {
					objMaterial = new Material(Shader.Find("Underwater/SpecNormalCausticsDepth2passBlend"));
				} else {
					objMaterial = new Material(Shader.Find("Underwater/SpecNormalCausticsDepth2pass"));
				}
				Texture causticTex = AssetDatabase.LoadAssetAtPath("Assets/Standard Assets/Textures/caustics.png", typeof(Texture)) as Texture;
				Texture noiseTex = AssetDatabase.LoadAssetAtPath("Assets/Standard Assets/Textures/noise.jpg", typeof(Texture)) as Texture;
				objMaterial.SetTexture("_CausticTex", causticTex);
				objMaterial.SetTexture("_NoiseTex", noiseTex);
				objMaterial.SetTextureScale("_CausticTex",new Vector2(0.2f, 0.2f));
			}
			
			
			meshRenderer.material = objMaterial;                   
			go.transform.position = GameObject.Find("sbpoint").transform.position;
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			JSONObject meshMaterials = currentBeingProcessedJSONData.GetField("MeshMaterials");
			string parent = JSONObject.Lookup(currentBeingProcessedJSONData, "parent");
			string itemUrlKey = JSONObject.Lookup(currentBeingProcessedJSONData, "item-url-key");
			string varientUrlKey = JSONObject.Lookup(currentBeingProcessedJSONData, "variant-url-key");
			if(bIsSeabed)
			{
				textureDirInEditor = seabedGeoDirectory + parent +"/"+ itemUrlKey + "/" + varientUrlKey + "/Materials/";
			}
			else
			{
				textureDirInEditor =  fishGeoDirectory + parent +"/"+ itemUrlKey + "/" + varientUrlKey + "/Materials/";
			}
			foreach(JSONObject meshMaterial in meshMaterials.list)
			{
//				JSONObject meshName = meshMaterial.GetField("MeshName");
				JSONObject material = meshMaterial.GetField("Material");
				JSONObject textures = material.GetField("Textures");
				GameObject mesh = new GameObject();
				mesh = go;
				foreach(JSONObject texture in textures.list)
				{
					JSONObject color = texture.GetField("Color");
					JSONObject textureType = texture.GetField("TextureType");
					JSONObject textureName = texture.GetField("Name");
					if(textureName.str != "")
					{
						
						string textureNameInString = textureName.str.Replace("\\\\", "/");
						if(textureNameInString.Contains("/"))
							textureNameInString = textureNameInString.Substring(textureNameInString.LastIndexOf("/"));
						string texturePath = textureDirInEditor + textureNameInString;
						texturePath = texturePath.Replace("//","/");
						Texture textureToApply = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture)) as Texture;
						if(textureToApply)
						{
							if(mesh.GetComponent<Renderer>().sharedMaterial.HasProperty(textureType.str))
							{
								mesh.GetComponent<Renderer>().sharedMaterial.SetTexture(textureType.str,textureToApply);
							}
						}
					}
					if(color.str != "null")
					{
						//color.type = JSONObject.Type.NUMBER;
						float b = JSONObject.LookupFloat(color,"B");
						float r = JSONObject.LookupFloat(color,"R");
						float g = JSONObject.LookupFloat(color,"G");
						mesh.GetComponent<Renderer>().sharedMaterial.color = new Color(r,g,b);
					}
				}
			}
		}
		Generate_TheBlueLoadingData(null);
		CSAdjustGlobalShderParams.AdjustGlobalShaderParams();
		BuildScene();
		RevertModified_TheBlueLoadingData();
		AssetDatabase.SaveAssets();
	}
	public static bool HasSceneOpened(string scenePath, Object prefab)
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
	public static void Generate_TheBlueLoadingData(List<string> sceneBundles)
	{
		string blueLoadingDataPath = GetFullPath(generatePath);
		oldBlueLoadingData = File.ReadAllText(blueLoadingDataPath);
		
		string newBlueLoadingData = string.Empty;
		//if( buildPreviewScene ) {
			Debug.Log("Building Preview Data");
			newBlueLoadingData = oldBlueLoadingData.Replace("public static bool previewScene = false;","public static bool previewScene = true;");
	//	}
//else {
//			newBlueLoadingData = oldBlueLoadingData;
//		}
		
		if( sceneBundles != null ) {
			string streamingSceneNames = string.Empty;
			foreach (string scene in sceneBundles)
			{
				streamingSceneNames += "\"" + GetSceneName(scene) + "\",";
			}
			if (streamingSceneNames.EndsWith(","))
				streamingSceneNames = streamingSceneNames.Substring(0, streamingSceneNames.Length - 1);
	
			string pre = newBlueLoadingData.Substring(0, newBlueLoadingData.LastIndexOf("{") + 1);
			string pos = newBlueLoadingData.Substring(newBlueLoadingData.LastIndexOf("};"), newBlueLoadingData.Length - newBlueLoadingData.LastIndexOf("};"));
	
			File.WriteAllText(blueLoadingDataPath, pre + streamingSceneNames + pos);
		}
		else {
			File.WriteAllText(blueLoadingDataPath, newBlueLoadingData);
		}
		AssetDatabase.SaveAssets();
	}
	public static void BuildScene()
	{
		string[] scenes = new string[] { baseScenePath };
		BuildPipeline.BuildPlayer(scenes, meshViewerPath, BuildTarget.WebPlayerStreamed, BuildOptions.None);
	}
	public static void RevertModified_TheBlueLoadingData()
	{
		if (!string.IsNullOrEmpty(oldBlueLoadingData))
		{
			string blueLoadingDataPath = GetFullPath(generatePath);
			File.WriteAllText(blueLoadingDataPath, oldBlueLoadingData);
			oldBlueLoadingData = string.Empty;
		}
	}
	public static string GetFullPath(string relative)
	{
		string tmp = Application.dataPath;
		return tmp.Substring(0, tmp.LastIndexOf("/") + 1) + relative;
	}
	public static string GetSceneName(string path)
	{
		return path.Substring(path.LastIndexOf("/") + 1, path.LastIndexOf(".unity") - path.LastIndexOf("/") - 1);
	}

}

