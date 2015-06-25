using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

#pragma warning disable 0219 // variable assigned but not used.

public class AnimationViewerCreator 
{
	static string baseScenePath = "Assets/Scenes/" + "preview-turntable" + ".unity";
	static string buildPath = "Assets/PrefabStaging";
	static JSONObject currentBeingProcessedJSONData = null;
	public static  void LoadFishToPreviewScene()
	{
		if (!HasSceneOpened(baseScenePath))
			return;
		
		//PreviewTurntableApp preview_app = Object.FindObjectOfType(typeof(PreviewTurntableApp)) as PreviewTurntableApp;
		
		//PreviewTurntableApp preview_app = Object.FindObjectOfType(typeof(PreviewTurntableApp)) as PreviewTurntableApp
		int numberOfMeshEnabled = 0;;
		//int numberOfMeshEnabled = 0;;
		List<string> animationFilePaths = new List<string>();
		if (!HasSceneOpened(baseScenePath))
			return;
		
		string json_file = buildPath + @"/Fish.json";
		JSONObject json_data = null;
		if (File.Exists(json_file)) {
			json_data = GetJSON(json_file);
		}
		else {
			json_file = buildPath + @"/item_instance.json";
			if (File.Exists(json_file)) {
				json_data = GetJSON(json_file);
			}
		}
		
		if( json_data == null ) {
			return; // how do we send an error?
		}
		
		currentBeingProcessedJSONData = json_data;
		
		//PreviewTurntableApp preview_app = Object.FindObjectOfType(typeof(PreviewTurntableApp)) as PreviewTurntableApp;
		
		//PreviewTurntableApp preview_app = Object.FindObjectOfType(typeof(PreviewTurntableApp)) as PreviewTurntableApp
		string varientUrlKey = JSONObject.Lookup(json_data, "variant-url-key");
	
		UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(buildPath + @"/" + varientUrlKey + ".prefab");

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
			if(filePathLowered.Contains("@"))
			{
				animationFilePaths.Add(filePathLowered);
				Debug.Log(filePathLowered);
			}

		}
		
	prefab = AssetDatabase.LoadMainAssetAtPath(fbxFilePath);
		
		string templatePrefabToCopy = JSONObject.Lookup(json_data, "PrefabToCopy");
		string templatePrefabToCopyPath = null;
		
		string path = "Standard Assets/Prefabs/Fish";
		string [] fishAssetFiles = Directory.GetFiles(Application.dataPath+"/"+path,"*.*",SearchOption.AllDirectories);
		foreach(string fishFileNameAndDir in fishAssetFiles)
		{
            if (fishFileNameAndDir.EndsWith(".meta")) // ignore .meta files
                continue;

			if (!fishFileNameAndDir.Contains(templatePrefabToCopy) ) {
				continue;
			}
			
			string fishFileName = fishFileNameAndDir.Replace("\\", "/");
			int index = fishFileName.LastIndexOf("/Assets/");
			templatePrefabToCopyPath = fishFileName.Substring(index + 1);
			break;
        }
		
		GameObject templatePrefab = AssetDatabase.LoadAssetAtPath(templatePrefabToCopyPath, typeof(GameObject)) as GameObject;
		
		GameObject itemObj;
		if(templatePrefab != null)
			itemObj = GameObject.Instantiate(templatePrefab, Vector3.zero, Quaternion.identity) as GameObject;
		else {
			itemObj = GameObject.Instantiate(prefab) as GameObject;
		}
		
		GameObject.DestroyImmediate(GameObject.Find("LOD0"));
		GameObject.DestroyImmediate(GameObject.Find("LOD1"));
		GameObject.DestroyImmediate(GameObject.Find("LOD2"));
		GameObject.DestroyImmediate(GameObject.Find("root"));
		
		// add new GameObject children (root GameObject will be added in when loaded)
		GameObject lod0 = new GameObject("LOD0");
		lod0.transform.parent = itemObj.transform;
		GameObject lod1 = new GameObject("LOD1");
		lod1.transform.parent = itemObj.transform;
		GameObject lod2 = new GameObject("LOD2");
		lod2.transform.parent = itemObj.transform;
		
		GameObject modelObj = AssetDatabase.LoadAssetAtPath(fbxFilePath, typeof(GameObject)) as GameObject;
		
				
		GameObject modelObjInstance = GameObject.Instantiate(modelObj) as GameObject;
		
		
		Transform[] childTforms = modelObjInstance.GetComponentsInChildren<Transform>();
		foreach (Transform child in childTforms) {
			string name = child.gameObject.name;
			if (name == "root") {
				child.parent = itemObj.transform;
			} else if (name.Contains("_lod0")) {
				child.parent = lod0.transform;
			} else if (name.Contains("_lod1")) {
				child.parent = lod1.transform;
			} else if (name.Contains("_lod2")) {
				child.parent = lod2.transform;
			}
		}
		
		GameObject.DestroyImmediate(modelObjInstance);
		
		
		GameObject loda0 = itemObj.transform.Find("LOD0").gameObject;
		GameObject loda1 = itemObj.transform.Find("LOD1").gameObject;
		GameObject loda2 = itemObj.transform.Find("LOD2").gameObject;
		
		SetMaterialShaders(loda0, loda1, loda2);
		
		string[] pngSrcPaths = Directory.GetFiles(buildPath, "*.png");
		string[] tgaSrcPaths = Directory.GetFiles(buildPath, "*.tga");
		string [][] srcPathArrays = new string[2][] {pngSrcPaths, tgaSrcPaths};
		
		foreach (string[] srcPaths in srcPathArrays) {
			foreach (string srcPath in srcPaths) {
				Texture tex = AssetDatabase.LoadAssetAtPath(srcPath, typeof(Texture)) as Texture;

				if( tex != null ) {
					//ApplyLODTexture(itemObj, tex);
				}
			}
		}
		ApplyLODTexture(loda0, loda1, loda2);
		
		//LODModelData lodModelData  = itemObj.GetComponent<LODModelData>();
		//lodModelData.LODs[0].LOD = itemObj.transform.Find("LOD0").gameObject;
		//lodModelData.LODs[1].LOD = itemObj.transform.Find("LOD1").gameObject;
		//lodModelData.LODs[2].LOD = itemObj.transform.Find("LOD2").gameObject;
		
		//PrefabUtility.ReplacePrefab(itemObj, prefab);
		
		GameObject.DestroyImmediate(loda1);
		GameObject.DestroyImmediate(loda2);
		
		GameObject theMesh = GameObject.Instantiate(itemObj, Vector3.zero, Quaternion.identity) as GameObject;
		UnityEngine.GameObject.DestroyImmediate(itemObj);
		theMesh.transform.position = GameObject.Find("sbpoint").transform.position;
		
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
	private static void ApplyLODTexture(GameObject obj, Texture tex) {
		string tex_name = tex.name.ToLower();
		/*GameObject lod = null;
		if (tex_name.Contains("_lod0")) {
			lod = obj.transform.Find("lod0").gameObject;
		} else if (tex_name.Contains("_lod1")) {
			lod = obj.transform.Find("LOD1").gameObject;
		} else if (tex_name.Contains("_lod2")) {
			lod = obj.transform.Find("LOD2").gameObject;
		} else {
			Debug.Log("ERROR: improper LOD naming format for texture \"" + tex.name + "\"");
		}*/
		string texType = string.Empty;
		if (tex_name.Contains("_diff_lod0")) {
			texType = "_MainTex";
		} else if (tex_name.Contains("_alpha_lod0")) {
			texType = "_MainTex";
			// For the LOD2 shader, clicking "Edit..." will show the needed texType keywords,
			// and it seems that the alpha texture is in fact the main and only texture
		} else if (tex_name.Contains("_norm")) {
			if(tex_name.Contains("lod0")){
				string path = AssetDatabase.GetAssetPath(tex);
				TextureImporter	textureImport = AssetImporter.GetAtPath(path) as TextureImporter;
				textureImport.textureType = TextureImporterType.Bump;
				textureImport.filterMode = FilterMode.Bilinear;
				AssetDatabase.Refresh();
				AssetDatabase.SaveAssets();
			}
			texType = "_NormMap";
		} else if (tex_name.Contains("_spec_lod0")) {
			texType = "_SpecMap";
		} else {
			Debug.Log("ERROR improper texture type naming format for texture: " + tex.name);
			return;
		}

		Debug.Log("TexType :: " + texType);
		
		// Find the material that the texture must be applied to
//		string texturePrefix = GetMaterialNamePrefix(tex_name);
		
		Transform[] lodChildTforms = obj.transform.GetComponentsInChildren<Transform>();
		foreach (Transform tform in lodChildTforms) {
			if (tform.name.ToLower() != obj.name.ToLower()) { // Since the LOD parent object itself is in the list, but we don't want to apply textures to it
				GameObject lodChildObj = tform.gameObject;
				Material childMaterial = lodChildObj.GetComponent<Renderer>().sharedMaterial;

//				string materialPrefix = GetMaterialNamePrefix(childMaterial.name);

//				if (materialPrefix.ToLower() == texturePrefix.ToLower()) {
					if (childMaterial.HasProperty(texType)) {
						Debug.Log("Setting texture (" + tex.name + ") to material (" + childMaterial.name +")");
						childMaterial.SetTexture(texType, tex);
					} else {
						Debug.Log("WARNING: material \"" + childMaterial.name + "\" does not have a property for texture \"" + tex.name + "\"");
					}
//				}
				// CONSIDER: else break
			}
		}
	}
	private static void ApplyLODTexture(GameObject lod0, GameObject lod1, GameObject lod2)
	{
		JSONObject meshMaterials = currentBeingProcessedJSONData.GetField("MeshMaterials");
		
		foreach(JSONObject meshMaterial in meshMaterials.list)
		{
			JSONObject meshName = meshMaterial.GetField("MeshName");
			JSONObject material = meshMaterial.GetField("Material");
			JSONObject textures = material.GetField("Textures");
			GameObject mesh = new GameObject();
			if(lod0.transform.FindChild(meshName.str))
			{
				mesh = lod0.transform.FindChild(meshName.str).gameObject;
			}
			else if(lod1.transform.FindChild(meshName.str))
			{
				mesh = lod1.transform.FindChild(meshName.str).gameObject;
			}
			else if(lod2.transform.FindChild(meshName.str))
			{
				mesh = lod2.transform.FindChild(meshName.str).gameObject;
			}
			else
			{
				Debug.LogError("something's wrong" + meshMaterial);
				Debug.LogError(meshName.str);
				return;
			}
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
					string texturePath = buildPath + @"/" +  textureNameInString;
					texturePath = texturePath.Replace("//","/");
					Texture textureToApply = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture)) as Texture;
					Debug.Log (texturePath);
					if(textureToApply)
					{
						if(mesh.GetComponent<Renderer>().sharedMaterial.HasProperty(textureType.str))
						{
							mesh.GetComponent<Renderer>().sharedMaterial.SetTexture(textureType.str,textureToApply);
							if(textureName.str.Contains("lod0") && textureName.str.Contains("norm")){
								TextureImporter	textureImport = AssetImporter.GetAtPath(texturePath) as TextureImporter;
								textureImport.textureType = TextureImporterType.Bump;
								textureImport.textureFormat = TextureImporterFormat.DXT5;
								AssetDatabase.Refresh();
								AssetDatabase.SaveAssets();
							}
							Debug.Log(textureType.str + " " + textureToApply);
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
	private static JSONObject GetJSON(string filepath) {
		string filetext = File.ReadAllText(filepath);
		JSONObject fishData = new JSONObject(filetext);
		return fishData;
	}
	private static void SetMaterialShaders(GameObject lod0, GameObject lod1, GameObject lod2) {	
		// lod0 Shaders
		
		Transform[] tforms = lod0.transform.GetComponentsInChildren<Transform>();
		foreach (Transform child in tforms) {
			GameObject obj = child.gameObject;
			string name = obj.name;
			if (name != "LOD0") { // CONSIDER: find a better way to not get LOD objects in the tforms list
				Material objMaterial = obj.GetComponent<Renderer>().sharedMaterial; // Assume that the renderer only has one Material, and modify that one
					/*JSONObject meshMaterials = currentBeingProcessedJSONData.GetField("MeshMaterials");
					
						JSONObject meshName = meshMaterials[0].GetField("tuna_lod0");
						obj.*/
					
				if (name.Contains("_alpha_")) {
						objMaterial.shader = Shader.Find("Underwater/Fish_Alpha_LOD0");
				} else {
					objMaterial.shader = Shader.Find("Underwater/Fish_LOD0");
				}
				Texture causticTex = AssetDatabase.LoadAssetAtPath("Assets/Standard Assets/Textures/caustics.png", typeof(Texture)) as Texture;
				Texture noiseTex = AssetDatabase.LoadAssetAtPath("Assets/Standard Assets/Textures/noise.jpg", typeof(Texture)) as Texture;
				objMaterial.SetTexture("_CausticTex", causticTex);
				objMaterial.SetTexture("_NoiseTex", noiseTex);
				objMaterial.SetTextureScale("_CausticTex",new Vector2(0.2f, 0.2f));
			}
		}
		// lod1 Shaders	
		tforms = lod1.transform.GetComponentsInChildren<Transform>();
		foreach (Transform child in tforms) {
			GameObject obj = child.gameObject;
			string name = obj.name;
			if (name != "LOD1") {
				Material objMaterial = obj.GetComponent<Renderer>().sharedMaterial;
				if (name.Contains("_alpha_")) {
						objMaterial.shader = Shader.Find("Underwater/Fish_Alpha_LOD1");
				} else {
					objMaterial.shader = Shader.Find("Underwater/Fish_LOD1");
				}
					
				if(objMaterial.HasProperty("_CausticTex"))
				{
					if(!objMaterial.GetTexture("_CausticTex"))
					{
//						Debug.Log(importedAsset);
						objMaterial.SetTexture("_CausticTex", AssetDatabase.LoadAssetAtPath("Assets/Standard Assets/Textures/caustics.png",typeof(Texture)) as Texture);
					}
//					Debug.Log(importedAsset);
					objMaterial.SetTextureScale("_CausticTex",new Vector2(0.2f, 0.2f));
				}
			}
		}
		
		// lod2 Shaders
		tforms = lod2.transform.GetComponentsInChildren<Transform>();
		foreach (Transform child in tforms) {
			GameObject obj = child.gameObject;
			string name = obj.name;
			if (name != "LOD2") {
				Material objMaterial = obj.GetComponent<Renderer>().sharedMaterial;
				if(name.Contains("_alpha_"))
					objMaterial.shader = Shader.Find("Underwater/Fish_Alpha_LOD2");
				else 
					objMaterial.shader = Shader.Find ("Underwater/Fish_LOD2");
						
				if(objMaterial.HasProperty("_CausticTex"))
				{
					if(!objMaterial.GetTexture("_CausticTex"))
					{
//						Debug.Log(importedAsset);
						objMaterial.SetTexture("_CausticTex", AssetDatabase.LoadAssetAtPath("Assets/Standard Assets/Textures/caustics.png",typeof(Texture)) as Texture);
					}
//					Debug.Log(importedAsset);
					objMaterial.SetTextureScale("_CausticTex",new Vector2(0.2f, 0.2f));
				}
			}
		}
	}
}

