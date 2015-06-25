using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

public class MakeVariantPrefab {

	static string primaryPath = @"Assets/PrefabStaging";
	static string textureDirInEditor = string.Empty;
	static string parent = string.Empty;
	static string itemUrlKey = string.Empty;
	static string varientUrlKey = string.Empty;
	static string fishOrSeabed = string.Empty;
	static string stage = string.Empty;

	static string prefabDirectory =  @"Assets/Standard Assets/Prefabs/" ;
	static bool bIsSeabed = false;
	static JSONObject currentBeingProcessedJSONData = null;
	static string GetPrefabDirectory()
	{
		return prefabDirectory + fishOrSeabed +@"/" + parent +@"/" +itemUrlKey + @"/";
	}
	// called by the maker workflow to build out the unity project.
	static void BuildUnityNOOP() {
	}
	
	// uncomment to make menu item
	[MenuItem("WEMOTools/Make Variant Prefab")]
	static void MakeBluPrefab() {
		// ATTN: change to the maker's submitted directory, in context of where it is in the project directory
		string json_file = primaryPath + @"/Fish.json";
		JSONObject json_data = null;
		if (File.Exists(json_file)) {
			json_data = GetJSON(json_file);
		}
		else {
			json_file = primaryPath + @"/item_instance.json";
			if (File.Exists(json_file)) {
				json_data = GetJSON(json_file);
			}
		}
		
		if( json_data == null ) {
			return; // how do we send an error?
		}
		stage = JSONObject.Lookup(json_data, "stage");
		parent = JSONObject.Lookup(json_data, "parent");
		itemUrlKey = JSONObject.Lookup(json_data, "item-url-key");
		varientUrlKey = JSONObject.Lookup(json_data, "variant-url-key");
		currentBeingProcessedJSONData = json_data;
		
		string species_class = JSONObject.Lookup(json_data, "Species_Class");
		GameObject item_obj = null;
		bool is_seabed = species_class != null && species_class.Equals("seabed");
		bIsSeabed = is_seabed;
		if(!is_seabed)
		{
			if(stage == "model")
			{
				Debug.Log("Build Model");
				ModelViewerCreator.LoadFishToPreviewScene();
				return;
			}
			else if (stage == "texture")
			{
				Debug.Log("Build Texture");
				TextureViewerCreator.LoadFishToPreviewScene();
				return;
			}
			else if(stage == "anim")
			{
				Debug.Log("Build Anim");
				AnimationViewerCreator.LoadFishToPreviewScene();
				return;
			}
			else if(stage == "rig")
			{
				Debug.Log("Build Rig");
				RigViewerCreator.LoadFishToPreviewScene();
				return;
			}
		}
		else
		{
			if(stage == "model")
			{
				Debug.Log("Build Model");
				ModelViewerCreator.LoadFishToPreviewScene();
				return;
			}
			else if (stage == "texture")
			{
				Debug.Log("Build Texture");
				TextureViewerCreator.LoadFishToPreviewScene();
				return;
			}
		}
	
		//UnityEngine.Object prefabToLoadAsMesh = new UnityEngine.Object();
		/*string[] filesInPrefabStagingDirectory = Directory.GetFiles(primaryPath);
		string fbxFilePath = string.Empty;
		foreach(string fileInPrefabStagingDirectory in filesInPrefabStagingDirectory)
		{
			if(fileInPrefabStagingDirectory.EndsWith(".fbx") && !fileInPrefabStagingDirectory.Contains("@"))
			{
				fbxFilePath = fileInPrefabStagingDirectory;
			}
		}*/
			
		//prefabToLoadAsMesh = AssetDatabase.LoadMainAssetAtPath(fbxFilePath);
		if( is_seabed ) {
			item_obj = _MakeSeabedPrefab(json_data);
		}
		else {
			item_obj = _MakeFishPrefab(json_data);
		}
					
		List<string> itemPrefabPaths = new List<string>();
		if(!Directory.Exists( GetPrefabDirectory()))
		{
			Debug.LogWarning("Creating directory : " + GetPrefabDirectory());
			Directory.CreateDirectory( GetPrefabDirectory());
		}
		string itemPrefabPath = GetPrefabDirectory() + varientUrlKey + @".prefab";
		itemPrefabPaths.Add(itemPrefabPath);
		if(Directory.Exists(textureDirInEditor))
		{
			string[] pngSrcPaths = Directory.GetFiles(textureDirInEditor, "*.png");
			string[] tgaSrcPaths = Directory.GetFiles(textureDirInEditor, "*.tga");
			
			itemPrefabPaths.AddRange(pngSrcPaths);
			itemPrefabPaths.AddRange(tgaSrcPaths);
		}
			
		UnityEngine.Object emptyPrefab = PrefabUtility.CreateEmptyPrefab(itemPrefabPath);
			
		AssetDatabase.Refresh();
		AssetDatabase.SaveAssets();
		PrefabUtility.ReplacePrefab(item_obj, emptyPrefab);
		UnityEngine.Object.DestroyImmediate(item_obj); // destroy GameObject, now that prefab is an asset

		AssetDatabase.Refresh();
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		// Export the new fish prefab as a bundle
		UnityEngine.Object itemPrefab = AssetDatabase.LoadMainAssetAtPath(itemPrefabPath);
		string bundlePath = primaryPath + @"/" + itemPrefab.name + @".unity3d";
		BuildAssetBundleOptions bundleOptions = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets;
			BuildPipeline.BuildAssetBundle(itemPrefab, null, bundlePath, bundleOptions);
			
		// Export it as a unitypackage
		string packagePath = primaryPath + @"/" + itemPrefab.name + @".unitypackage";
		ExportPackageOptions packageOptions = ExportPackageOptions.IncludeDependencies | ExportPackageOptions.IncludeLibraryAssets | ExportPackageOptions.Recurse /*| ExportPackageOptions.Interactive*/;
		AssetDatabase.ExportPackage(itemPrefabPaths.ToArray(), packagePath, packageOptions);
			
		AssetDatabase.Refresh();
		UnityEngine.Object prefabToLoad = AssetDatabase.LoadMainAssetAtPath(itemPrefabPath);
			
		if(!is_seabed)
		{
			Debug.Log("Build webplayer");
			PreviewSceneCreator.LoadFishToPreviewScene(prefabToLoad);
		}
		else
		{
			Debug.Log("Build webplayer");
			SeabedPreviewSceneCreator.LoadSeabedToPreviewScene(prefabToLoad);
		}
		AssetDatabase.Refresh();
	}
/*	private static void MakeMeshViewer()
	{
		string[] fbxPaths = Directory.GetFiles( Application.dataPath + "/PrefabStaging/");
		string fbxPath = string.Empty;
		foreach(string path in fbxPaths)
		{
			if(path.EndsWith(".meta"))
			{
				continue;
			}
			if(path.EndsWith(".fbx"))
			{
				fbxPath = path;
				break;
			}
		}
		int index = fbxPath.LastIndexOf("/Assets/");
		fbxPath = fbxPath.Substring(index + 1);
		//Object emptyPrefab = EditorUtility.CreateEmptyPrefab(primaryPath + "/prefabToUse.prefab");
		//EditorUtility.ReplacePrefab(importedObject, prefab, ReplacePrefabOptions.ReplaceNameBased);
		UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(fbxPath);
		
		GameObject go = GameObject.Instantiate(obj) as GameObject;
		Transform[] transformInChildren = go.GetComponentsInChildren<Transform>();
		GameObject lod0 = new GameObject();
		foreach(Transform childTransform in transformInChildren)
		{
			if(childTransform.name.Contains("lod0") || childTransform.name.Contains("LOD0"))
			{
				lod0 = childTransform.gameObject;
				break;
			}
		}
		//GameObject.DestroyImmediate(go);
		Mesh meshToDisplay = new Mesh();
		if(lod0.GetComponent<MeshFilter>())
		 	meshToDisplay = lod0.GetComponent<MeshFilter>().sharedMesh;
		if(lod0.GetComponent<SkinnedMeshRenderer>())
		 	meshToDisplay = lod0.GetComponent<SkinnedMeshRenderer>().sharedMesh;
			
		if(File.Exists(MeshViewerCreator.baseScenePath))
		{
		//	File.Delete(MeshViewerCreator.baseScenePath);
		}
	
		MeshViewerCreator.LoadFishToPreviewScene(meshToDisplay);
		//EditorApplication.OpenScene(MeshViewerCreator.defaultPath + MeshViewerCreator.baseLevelToBuild + ".unity");	
		if(File.Exists(MeshViewerCreator.baseScenePath))
		{
		//	File.Delete(MeshViewerCreator.baseScenePath);
		}
	}*/
	private static GameObject _MakeFishPrefab(JSONObject fishData) {
		Debug.Log("Making a fish");
		// Instantiate a GameObject of the Prefab being used as a template
		string templatePrefabToCopy = JSONObject.Lookup(fishData, "PrefabToCopy");
		string templatePrefabToCopyPath = null;
		string fishGeoDirectory = @"Assets/Standard Assets/Geo/fish/";
		
		string path = @"Standard Assets/Prefabs/Fish";
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

//		string templatePrefabToCopyPath = @"Assets/Standard Assets/Prefabs/Fish/" + templatePrefabToCopy + @".prefab";
		GameObject templatePrefab = AssetDatabase.LoadAssetAtPath(templatePrefabToCopyPath, typeof(GameObject)) as GameObject;
		GameObject itemObj = GameObject.Instantiate(templatePrefab, Vector3.zero, Quaternion.identity) as GameObject;
		
		// Import and process model and animation files (".fbx")
		//string modelDir = JSONObject.Lookup(fishData, "Species_ModelDir");
		
		string modelDir = fishGeoDirectory + parent + @"/" + itemUrlKey + @"/" + varientUrlKey + @"/" ;
		//string animDir = JSONObject.Lookup(fishData, "Species_AnimationDir");
		string animDir =  fishGeoDirectory + parent+ @"/"+itemUrlKey + @"/" + varientUrlKey + @"/Animation/";
		ProcessModelAnimAssets(itemObj, primaryPath, modelDir, animDir, fishData);
		
		// Import and process texture files (".tga", ".png")
		//string textureDir = JSONObject.Lookup(fishData, "Species_MaterialTextureDir");
		string textureDir =  fishGeoDirectory + parent + @"/"+ itemUrlKey + @"/" + varientUrlKey + @"/Materials/";
		textureDirInEditor = textureDir;
		fishOrSeabed = "fish";
		ProcessTextureAssets(itemObj, primaryPath, textureDir);
		// Tweak existing components based on JSON data
		ModifyComponents(itemObj, fishData, primaryPath, true);
		
//		FishAnimationData fishAnimationData = itemObj.GetComponent<FishAnimationData>();
		/*if(fishAnimationData.breatheTransform == null)
		{
			GameObject tempGameObject = GameObject.Instantiate(itemObj) as GameObject;
			Transform head = tempGameObject.transform.FindChild("head");
			fishAnimationData.breatheTransform = head;
			EditorUtility.ReplacePrefab(tempGameObject, itemObj);
			GameObject.DestroyImmediate(tempGameObject);
		}
		*/
		return itemObj;
	}
	
	private static GameObject _MakeSeabedPrefab(JSONObject item_data) {
		Debug.Log("Making a seabed");
		// Instantiate a GameObject of the Prefab being used as a template
		string templatePrefabToCopy = JSONObject.Lookup(item_data, "PrefabToCopy");
		string seabedGeoDirectory = @"Assets/Standard Assets/Geo/seabed/";
		
		string templatePrefabToCopyPath = null;

		string path = @"Standard Assets/Prefabs/seabed";
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
		GameObject itemObj = GameObject.Instantiate(templatePrefab, Vector3.zero, Quaternion.identity) as GameObject;
		
		// Import and process model and animation files (".fbx")
		//string modelDir = JSONObject.Lookup(item_data, "Species_ModelDir");
//		string animDir = JSONObject.Lookup(item_data, "Species_AnimationDir");
		string modelDir = seabedGeoDirectory + parent + @"/" + itemUrlKey + @"/" + varientUrlKey + @"/" ;
		
		ProcessModelAnimAssets(itemObj, primaryPath, modelDir, null, item_data);
		
		// Import and process texture files (".tga", ".png")
		//string textureDir = JSONObject.Lookup(item_data, "Species_MaterialTextureDir");
		string textureDir =  seabedGeoDirectory + parent +@"/"+ itemUrlKey + @"/" + varientUrlKey + @"/Materials/";
		fishOrSeabed = "seabed";
		textureDirInEditor = textureDir;
		ProcessTextureAssets(itemObj, primaryPath, textureDir);
		
		// Tweak existing components based on JSON data
		ModifyComponents(itemObj, item_data, primaryPath, false);
		
		return itemObj;
	}
	
	
	// ===== Importing and processing models and animations =====
	
	private static void ProcessModelAnimAssets(GameObject itemObj, string srcDir, string modelDestDir, string animDestDir, JSONObject json) {
		// remove old GameObject children
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
		
        if(!System.IO.Directory.Exists(modelDestDir))
	    {
			Debug.Log("Creating ModelDir : " + modelDestDir);
            Directory.CreateDirectory(modelDestDir);
	    }

		string forwardSwimClip_name = null;
		string forwardSwimSlowClip_name = null;
		string forwardSwimFastClip_name =  null;
		string hoverClip_name =  null;
		string breatheClip_name =  null;
		string breatheTransform_name =  null;
		if( animDestDir != null ) {
			// Load the names of animation clips from JSON to relink as the assets are loaded
			forwardSwimClip_name = JSONObject.Lookup(json, "FishAnimationData_forwardSwimClip");
			forwardSwimSlowClip_name = JSONObject.Lookup(json, "FishAnimationData_forwardSwimSlowClip");
			forwardSwimFastClip_name = JSONObject.Lookup(json, "FishAnimationData_forwardSwimFastClip");
			hoverClip_name = JSONObject.Lookup(json, "FishAnimationData_hoverClip");
			breatheClip_name = JSONObject.Lookup(json, "FishAnimationData_breatheClip");
			breatheTransform_name = JSONObject.Lookup(json, "FishAnimationData_breatheTransform");
		
	        if(!System.IO.Directory.Exists(animDestDir))
		    {
				Debug.Log("Creating AnimDir : " + animDestDir);
	            Directory.CreateDirectory(animDestDir);
		    }
		}

		// Model and Animation files
		string[] fbxSrcPaths = Directory.GetFiles(srcDir, "*.fbx");
		foreach(string srcPath in fbxSrcPaths) {
			string fname = System.IO.Path.GetFileName(srcPath);
			Debug.Log("Processing : " + fname + " :: " + animDestDir);
			
			
			
			if( animDestDir != null 
			    && fname.Contains("@") ) { // Animation files
				string destPath = System.IO.Path.Combine(animDestDir, fname);
				if(File.Exists(srcPath))
				{
					if(File.Exists(destPath))
					{
						File.Delete(destPath);
						AssetDatabase.Refresh();
						AssetDatabase.SaveAssets();
					}

					string destDirName = Path.GetDirectoryName(destPath);
					if (!Directory.Exists(destDirName))
					{
						Directory.CreateDirectory(destDirName);
					}

					System.IO.File.Copy(srcPath, destPath, true); // CAUTION: will overwrite
					AssetDatabase.Refresh();
					AssetDatabase.SaveAssets();
				}
				AssetDatabase.Refresh(); // Must be called, or else accessing the object below returns null
				
				// Strip the animation FBX and just get the animation clip
				// ASSUMPTION: The FBX file has only one animation clip associated with it
				// ATTN: uncomment once StripAnimAsset function is complete
				//~ GameObject fbxObj = AssetDatabase.LoadAssetAtPath(destPath, typeof(GameObject)) as GameObject;
				//~ AnimationClip animClip = StripAnimAsset(animDestDir, fname, fbxObj);
				
				// Get the animation clip asset
				// ATTN: remove once StripAnimAsset function is complete
				GameObject animObj = AssetDatabase.LoadAssetAtPath(destPath, typeof(GameObject)) as GameObject;
				Animation anim = animObj.GetComponent<Animation>();
				AnimationClip animClip = anim.clip;
				AnimationClip clip =  UnityEngine.AnimationClip.Instantiate(animClip) as AnimationClip;
				AssetDatabase.DeleteAsset(destPath);
				AssetDatabase.Refresh();
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				if(File.Exists(animDestDir + animClip.name + ".anim"))
               	{
					//animDestDir = animDestDir.TrimEnd("/Animation/".ToCharArray())+ "/" +varientUrlKey + "/Animation/";
					if(!Directory.Exists(animDestDir))
					{
						Directory.CreateDirectory(animDestDir);
					}
				}
				AssetDatabase.CreateAsset(clip,animDestDir + animClip.name + ".anim");
				AssetDatabase.Refresh();
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				
				Debug.Log("animClip.name :: " + animClip.name);
				// Add the animation clip to the fish object
				Animation fishAnimComponent = itemObj.GetComponent<Animation>();
				fishAnimComponent.AddClip(animClip, animClip.name);
				// If the new clip has the same name as the default clip of the fish, update the default clip
				if ( fishAnimComponent.clip == null 
				    || animClip.name == fishAnimComponent.clip.name) {
					fishAnimComponent.clip = animClip;
				}
				
				// Update the FishAnimationData component of the fish object
				FishAnimationData fishAnimData = itemObj.GetComponent<FishAnimationData>() as FishAnimationData;
				if (animClip.name == forwardSwimClip_name) {
					fishAnimData.forwardSwimClip = animClip;
				} else if (animClip.name == forwardSwimSlowClip_name) {
					fishAnimData.forwardSwimSlowClip = animClip;
				} else if (animClip.name == forwardSwimFastClip_name) {
					fishAnimData.forwardSwimFastClip = animClip;
				} else if (animClip.name == hoverClip_name) {
					fishAnimData.hoverClip = animClip;
				} else if (animClip.name == breatheClip_name) {
					fishAnimData.breatheClip = animClip;
				} else {
					//Debug.Log("WARNING : animation clip \"" + animClip.name + "\" processed but based on JSON data was not assigned to FishAnimationData");
				}
			} else { // Model files
				
				Debug.Log("is model");
				ModelImporter modelImporter = AssetImporter.GetAtPath(srcPath) as ModelImporter;
				modelImporter.materialSearch = ModelImporterMaterialSearch.Local;
				modelImporter.meshCompression = ModelImporterMeshCompression.High;
				modelImporter.swapUVChannels = false;
				modelImporter.normalImportMode = ModelImporterTangentSpaceMode.Calculate;
				modelImporter.normalSmoothingAngle = 180.0f;
				modelImporter.materialSearch = ModelImporterMaterialSearch.Local;
				
				// Assuming there is only one model file in the directory...
				string destPath = System.IO.Path.Combine(modelDestDir, fname);
				Debug.Log("moving : " + srcPath + " : to : " + destPath);
				if(File.Exists(srcPath))
				{
					if(File.Exists(destPath))
					{
						File.Delete(destPath);
						AssetDatabase.Refresh();
						AssetDatabase.SaveAssets();
					}
					string destDirName = Path.GetDirectoryName(destPath);
					if (!Directory.Exists(destDirName))
					{
						Directory.CreateDirectory(destDirName);
					}

					System.IO.File.Copy(srcPath, destPath,true);
					AssetDatabase.Refresh();
					AssetDatabase.SaveAssets();
				}
				//System.IO.File.Copy(srcPath, destPath, true); // CAUTION: will overwrite
				modelImporter.materialSearch = ModelImporterMaterialSearch.Local;
				modelImporter.importMaterials = true;
				AssetDatabase.Refresh(); // Must be called, or else accessing the object below returns null
				AssetDatabase.SaveAssets();
				
				GameObject modelObj = AssetDatabase.LoadAssetAtPath(destPath, typeof(GameObject)) as GameObject;
		
				
				Debug.Log("modelObj :: " + modelObj);
				GameObject modelObjInstance = GameObject.Instantiate(modelObj) as GameObject;
				Debug.Log("modelObjInstance :: " + modelObjInstance);
				
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
					} else if (animDestDir != null && name == breatheTransform_name) {
						// Update the FishAnimationData component of the fish object
						FishAnimationData fishAnimData = itemObj.GetComponent<FishAnimationData>() as FishAnimationData;
						fishAnimData.breatheTransform = child;
					}
				}
				//ModelImporter settings
			
				modelImporter = AssetImporter.GetAtPath(destPath) as ModelImporter;
				modelImporter.meshCompression = ModelImporterMeshCompression.High;
				modelImporter.swapUVChannels = false;
				modelImporter.normalImportMode = ModelImporterTangentSpaceMode.Calculate;
				modelImporter.normalSmoothingAngle = 180.0f;
				modelImporter.materialSearch = ModelImporterMaterialSearch.Local;
				GameObject.DestroyImmediate(modelObjInstance);

			}
		}
		
		if(System.IO.Directory.Exists(animDestDir + @"Materials")){
			string[] materials = Directory.GetFiles(animDestDir + @"/Materials");
			foreach(string material in materials)
				File.Delete(material);
			System.IO.Directory.Delete(animDestDir + @"/Materials");
		}
		
		if(System.IO.Directory.Exists(animDestDir)){
			string[] anims = Directory.GetFiles(animDestDir);
			if(anims.Length == 0)
				System.IO.Directory.Delete(animDestDir);
		}
	}
	
	private static AnimationClip StripAnimAsset(string animDir, string fbxFilename, GameObject fbxObj) {
		AnimationClip animClip = fbxObj.GetComponent<Animation>().clip;
		//Debug.Log("animClip : " + animClip);
		
		// Copy the animation clip file to the directory
		string animClipPath = System.IO.Path.Combine(animDir, animClip.name + ".anim");
		// ATTN: save the animation clip as its own file, somehow
		//System.IO.File.Copy(srcPath, destPath, true); // CAUTION: will overwrite
		
		// Remove the FBX file from the directory
		string fbxPath = System.IO.Path.Combine(animDir, fbxFilename);
		AssetDatabase.DeleteAsset(fbxPath);
		
		AssetDatabase.Refresh(); // CONSIDER: is this necessary? probably
		AssetDatabase.SaveAssets();
		return AssetDatabase.LoadAssetAtPath(animClipPath, typeof(AnimationClip)) as AnimationClip;
	}
	
	
	// ===== Importing and processing textures =====
	
	// PRECONDITION: Should be called after animation and model assets have already been processed
	private static void ProcessTextureAssets(GameObject itemObj, string srcDir, string destDir) {
		if(stage != "final")
		{
			return;
		}
        if(!System.IO.Directory.Exists(destDir))
	    {
			Debug.Log("Creating Texture Dir : " + destDir);
            Directory.CreateDirectory(destDir);
	    }

		GameObject lod0 = itemObj.transform.Find("LOD0").gameObject;
		GameObject lod1 = itemObj.transform.Find("LOD1").gameObject;
		GameObject lod2 = itemObj.transform.Find("LOD2").gameObject;
		
		SetMaterialShaders(lod0, lod1, lod2);

		List<string> textureFileList = new List<string>();
		string[] pngSrcPaths = Directory.GetFiles(srcDir, "*.png");
		string[] tgaSrcPaths = Directory.GetFiles(srcDir, "*.tga");
		string[] tifSrcPaths = Directory.GetFiles(srcDir, "*.tif");
		
		foreach(string pngPath in pngSrcPaths)
			textureFileList.Add(pngPath);
		foreach(string tgaPath in tgaSrcPaths)
			textureFileList.Add(tgaPath);
		foreach(string tifPath in tifSrcPaths)
			textureFileList.Add(tifPath);
		
		
		foreach (string srcPath in textureFileList) {
			// Copy to the directory specified in JSON
			string fname = System.IO.Path.GetFileName(srcPath);
			Debug.Log("Processing texture : " + fname);
				
			TextureImporter	textureImport = AssetImporter.GetAtPath(srcPath) as TextureImporter;
			textureImport.textureType = TextureImporterType.Advanced;
			textureImport.mipmapEnabled = false;
			if(!Directory.Exists(destDir))
			{
				Directory.CreateDirectory(destDir);
			}
			string destPath = System.IO.Path.Combine(destDir, fname);
				
			Debug.Log (destPath);
				
			if(File.Exists(srcPath))
			{
				if(File.Exists(destPath))
				{
					File.Delete(destPath);
					AssetDatabase.Refresh();
					AssetDatabase.SaveAssets();
				}
				File.Copy(srcPath,destPath,true);//
				AssetDatabase.Refresh();
				AssetDatabase.SaveAssets();
			}

			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();
			//System.IO.File.Copy(srcPath, destPath, true);
			//AssetDatabase.Refresh(); // OLD COMMENT WITH ABOVE System.IO.File.Copy. Must be called, or else accessing the object below returns null
			Texture tex = AssetDatabase.LoadAssetAtPath(destPath, typeof(Texture)) as Texture;

			if( tex != null ) {
				ApplyLODTexture(itemObj, tex);
			}
		}
		if(Directory.Exists(srcDir + "/Materials"))
		{
			//Directory.Delete(srcDir + "/Materials",true);
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();
		}
		
		ApplyLODTexture(lod0,lod1,lod2);
		AssetDatabase.Refresh();
		AssetDatabase.SaveAssets();
		
	}
	
	private static void SetMaterialShaders(GameObject lod0, GameObject lod1, GameObject lod2) {
		
		// lod0 Shaders
		Transform[] tforms = lod0.transform.GetComponentsInChildren<Transform>();
		foreach (Transform child in tforms) {
			GameObject obj = child.gameObject;
			string name = obj.name;
			if (name != "LOD0") { // CONSIDER: find a better way to not get LOD objects in the tforms list
				Material objMaterial = obj.GetComponent<Renderer>().sharedMaterial; // Assume that the renderer only has one Material, and modify that one
				if(bIsSeabed)
				{
					objMaterial.shader = Shader.Find("Underwater/DiffNormalDepthShad");
					if(name.Contains("_alpha_"))
					{
						objMaterial.shader = Shader.Find("Underwater/DiffNormalDepthMaskShad");
					}
					if(name.Contains("_wavy_"))
					{
						objMaterial.shader = Shader.Find("Underwater/DiffCausDisp");
					}	
				}
				else
				{
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
		}
		if(stage != "final")
		{
			return;
		}
		// lod1 Shaders	
		tforms = lod1.transform.GetComponentsInChildren<Transform>();
		foreach (Transform child in tforms) {
			GameObject obj = child.gameObject;
			string name = obj.name;
			if (name != "LOD1") {
				Material objMaterial = obj.GetComponent<Renderer>().sharedMaterial;
				if(bIsSeabed)
				{
					objMaterial.shader = Shader.Find("Underwater/DiffNormalDepthShad");
					if(name.Contains("_alpha_"))
					{
						objMaterial.shader = Shader.Find("Underwater/DiffNormalDepthMaskShad");
					}
					if(name.Contains("_wavy_"))
					{
						objMaterial.shader = Shader.Find("Underwater/DiffCausDisp");
					}
				}
				else
				{
					if (name.Contains("_alpha_")) {
							objMaterial.shader = Shader.Find("Underwater/Fish_Alpha_LOD1");
					} else {
						objMaterial.shader = Shader.Find("Underwater/Fish_LOD1");
					}
					
					if(objMaterial.HasProperty("_CausticTex"))
					{
						if(!objMaterial.GetTexture("_CausticTex"))
						{
//							Debug.Log(importedAsset);
							objMaterial.SetTexture("_CausticTex", AssetDatabase.LoadAssetAtPath("Assets/Standard Assets/Textures/caustics.png",typeof(Texture)) as Texture);
						}
//						Debug.Log(importedAsset);
						objMaterial.SetTextureScale("_CausticTex",new Vector2(0.2f, 0.2f));
					}
					
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
				if(bIsSeabed)
				{
					objMaterial.shader = Shader.Find("Underwater/DiffDepthShad");
					if(name.Contains("_alpha_"))
					{
						objMaterial.shader = Shader.Find("Underwater/DiffDepthMaskBlendShad");
					}
					if(name.Contains("_wavy_"))
					{
						objMaterial.shader = Shader.Find("Underwater/DiffCausDisp");
					}
				}
				else
				{
					if(name.Contains("_alpha_"))
						objMaterial.shader = Shader.Find("Underwater/Fish_Alpha_LOD2");
					else 
						objMaterial.shader = Shader.Find ("Underwater/Fish_LOD2");
						
					if(objMaterial.HasProperty("_CausticTex"))
					{
						if(!objMaterial.GetTexture("_CausticTex"))
						{
//							Debug.Log(importedAsset);
							objMaterial.SetTexture("_CausticTex", AssetDatabase.LoadAssetAtPath("Assets/Standard Assets/Textures/caustics.png",typeof(Texture)) as Texture);
						}
//						Debug.Log(importedAsset);
						objMaterial.SetTextureScale("_CausticTex",new Vector2(0.2f, 0.2f));
					}
				}
			}
		}
	}
	private static void ApplyLODTexture(GameObject lod0, GameObject lod1, GameObject lod2)
	{
		if(stage != "final")
		{
			return;
		}
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
				Debug.LogError("something's wrong " + meshMaterial);
				Debug.LogError(meshName.str);
				continue;
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
					string texturePath = textureDirInEditor + textureNameInString;
					texturePath = texturePath.Replace("//","/");
					Texture textureToApply = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture)) as Texture;
					Debug.Log (texturePath);
					if(textureToApply)
					{
						if(mesh.GetComponent<Renderer>().sharedMaterial.HasProperty(textureType.str))
						{
							mesh.GetComponent<Renderer>().sharedMaterial.SetTexture(textureType.str,textureToApply);
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
	private static void ApplyLODTexture(GameObject obj, Texture tex) {
		string tex_name = tex.name.ToLower();
		GameObject lod = null;
		if (tex_name.Contains("_lod0")) {
			lod = obj.transform.Find("LOD0").gameObject;
		} else if (tex_name.Contains("_lod1")) {
			lod = obj.transform.Find("LOD1").gameObject;
		} else if (tex_name.Contains("_lod2")) {
			lod = obj.transform.Find("LOD2").gameObject;
		} else {
			Debug.Log("ERROR: improper LOD naming format for texture \"" + tex.name + "\"");
		}
		
		string texType = string.Empty;
		if (tex_name.Contains("_diff_")) {
			texType = "_MainTex";
		} else if (tex_name.Contains("_alpha_")) {
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
		} else if (tex_name.Contains("_spec_")) {
			texType = "_SpecMap";
		} else {
			Debug.Log("ERROR improper texture type naming format for texture: " + tex.name);
			return;
		}

		Debug.Log("TexType :: " + texType);
		
		// Find the material that the texture must be applied to
//		string texturePrefix = GetMaterialNamePrefix(tex_name);
		
		Transform[] lodChildTforms = lod.transform.GetComponentsInChildren<Transform>();
		foreach (Transform tform in lodChildTforms) {
			if (tform.name.ToLower() != lod.name.ToLower()) { // Since the LOD parent object itself is in the list, but we don't want to apply textures to it
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
	
	// TODO: find a better name
	private static string GetMaterialNamePrefix(string str) {
		int end = str.Length;
		for (int start = str.Length - 1; start >= 0; start--) {
			if ((str[start] == '_') || (str[start] == '-') || (start == 0)) {
				string token;
				if (start == 0) {
					token = str.Substring(start, end - start);
				} else {
					token = str.Substring(start + 1, end - start - 1);
				}
				
				if (!IsTextureInfo(token)) {
					return token;
				} else {
					end = start;
				}
			}
		}
		return str; // CONSIDER: is this good return for no find?
	}
	
	private static bool IsTextureInfo(string str) {
		string token = str.ToLower();
		bool isLODInfo = ( (token == "lod0") || (token == "lod1") || (token == "lod2") );
		bool isTexTypeInfo = ( (token == "diff") || (token == "alpha") || (token == "norm") || (token == "spec") );
		return (isLODInfo || isTexTypeInfo);
	}
	
	
	// ===== Update components =====
	
	// PRECONDITION: Must be called after new LOD GameObjects have been created (in ProcessModelAnimAssets)
	private static void ModifyComponents(GameObject obj, JSONObject json, string dir, bool is_critter) {
		// Transform component - set position to origin and scale to (1, 1, 1)
		Transform rootTform = obj.transform;
		rootTform.position = Vector3.zero;
		
		// Also set the transform components of all children to be scale (1, 1, 1)
		Transform[] tforms = obj.GetComponentsInChildren<Transform>();
		foreach (Transform tform in tforms) {
			tform.localScale = Vector3.one;
		}
		
		// LODModelData component
		LODModelData lodModelData  = obj.GetComponent<LODModelData>();
		lodModelData.LODs[0].LOD = obj.transform.Find("LOD0").gameObject;
		lodModelData.LODs[1].LOD = obj.transform.Find("LOD1").gameObject;
		lodModelData.LODs[2].LOD = obj.transform.Find("LOD2").gameObject;
		
		// WemoItemData component
		WemoItemData wemoData = obj.GetComponent<WemoItemData>();
		wemoData.speciesName = JSONObject.Lookup(json, "WemoItemData_speciesName");
		wemoData.makerNames = JSONObject.Lookup(json, "WemoItemData_makerNames");
		
		if( is_critter ) {
			// GeneralSpeciesData component
			GeneralSpeciesData speciesData = obj.GetComponent<GeneralSpeciesData>();
			speciesData.speciesTag = JSONObject.Lookup(json, "GeneralSpeciesData_speciesTag");
			
			// FishBendControllerData component
			FishBendControllerData fishBendData = obj.GetComponent<FishBendControllerData>();
			fishBendData.rootNode = obj.transform.Find(JSONObject.Lookup(json, "FishBendControllerData_rootNode"));
			// CONSIDER: optimize these with one traversal through tree searching for several things, ie. with map
			fishBendData.spineJointChain.firstTransform = FindChildTform(obj, JSONObject.Lookup(json, "FishBendControllerData_BendingJointChain_firstTransform"));
			fishBendData.spineJointChain.lastTransform = FindChildTform(obj, JSONObject.Lookup(json, "FishBendControllerData_BendingJointChain_lastTransform"));
			
			CritterAnimationBase critterAnimBase = obj.GetComponent<CritterAnimationBase>();
			
			if(critterAnimBase != null){
				critterAnimBase.bodyTransform = FindChildTform(obj, "body");
				if( critterAnimBase.bodyTransform == null)
					//if body does not exist set transform to root
					critterAnimBase.bodyTransform = obj.transform;

				
			}
		}			
	}
	
	
	// ===== Utility functions =====
	
	// CONSIDER: implement more in this script and replace a lot of other code
	private static Transform FindChildTform(GameObject obj, string searchName) {
		Transform[] tforms = obj.GetComponentsInChildren<Transform>();
		foreach (Transform tform in tforms) {
			if (tform.name == searchName) {
				return tform;
			}
		}
		return null;
	}
	
	// NOTE: currently not used
	private static GameObject FindChildGameObject(GameObject obj, string searchName) {
		Transform tform = FindChildTform(obj, searchName); // Transform component has same name as its associated GameObject
		if (tform == null) {
			return null;
		}
		return tform.gameObject;
	}
	
	private static JSONObject GetJSON(string filepath) {
		string filetext = File.ReadAllText(filepath);
		JSONObject fishData = new JSONObject(filetext);
		return fishData;
	}
	

}
