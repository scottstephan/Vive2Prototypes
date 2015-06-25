using UnityEditor;
using UnityEngine;
using System.IO;
//may have to do this for audio files and textures file also. 
//but right now, the textures' settings were saved while exporting to a package.
//so just leave textures setting where it is right now ( they are in MakeVariantPrefab.cs )
public class FBXPostProcessor : AssetPostprocessor
{
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		if(importedAssets.Length > 0)
		{
			string lastImportedAssetsDataFile = string.Concat(Application.dataPath, "/../WEMOData/LastImportedAssetsData.json");
			string WEMODataDirectory = string.Concat(Application.dataPath, "/../WEMOData/");
			if(!Directory.Exists(WEMODataDirectory))
			{
				Directory.CreateDirectory(WEMODataDirectory);
			}
			if (File.Exists(lastImportedAssetsDataFile))
			{
//				Debug.Log("lastImportedAssetsDataFile exists");
				File.Delete(lastImportedAssetsDataFile);
			}
			File.WriteAllLines(lastImportedAssetsDataFile, importedAssets);		
					
/* Yifu, this needs to be moved into the make prefab script and only run on the 
			foreach(string importedAsset in importedAssets)
			{
				if (importedAsset.EndsWith(".meta")) // ignore .meta files
                	continue;
				if(importedAsset.Contains(".mat"))
				{		
					Material temp = AssetDatabase.LoadAssetAtPath(importedAsset,typeof(Material)) as Material;
				   
					if( temp != null ) {
						if(temp.HasProperty("_CausticTex"))
						{
							if(!temp.GetTexture("_CausticTex"))
							{
	//							Debug.Log(importedAsset);
								temp.SetTexture("_CausticTex", AssetDatabase.LoadAssetAtPath("Assets/Standard Assets/Textures/caustics.png",typeof(Texture)) as Texture);
							}
							
//							Debug.Log(importedAsset);
							temp.SetTextureScale("_CausticTex",new Vector2(0.2f, 0.2f));
						}
					}
				}
			}*/
		}
	}
	
	void OnPostprocessModel(Object fbx)
    {
		// TODO> we need to use this only when running the maker workflow.. or fish.. ugh.
		ModelImporter modelImporter = (ModelImporter)assetImporter;
//		Debug.Log("assetPath : " + assetPath);
//		Debug.Log("FBX name : " + fbx.name);
		
		if(assetPath.Contains("Assets/PrefabStaging/")
		   || assetPath.Contains("Assets/Standard Assets/Geo/Rocks/" )
		   || assetPath.Contains("Assets/Standard Assets/Geo/Ground/")
		   || assetPath.Contains("Assets/Standard Assets/Geo/Fish/"))
		{   
			modelImporter.meshCompression = ModelImporterMeshCompression.High;
			modelImporter.swapUVChannels = false;
			modelImporter.normalImportMode = ModelImporterTangentSpaceMode.Calculate;
			modelImporter.normalSmoothingAngle = 180.0f;
		}
    }
	void OnPreprocessModel()
    {
		// TODO> we need to use this only when running the maker workflow.. or fish.. ugh.
		ModelImporter modelImporter = (ModelImporter)assetImporter;
//		Debug.Log("assetPath : " + assetPath);
//		Debug.Log("FBX name : " + fbx.name);
		
		if(assetPath.Contains("Assets/PrefabStaging/")
		   || assetPath.Contains("Assets/Standard Assets/Geo/Rocks/" )
		   || assetPath.Contains("Assets/Standard Assets/Geo/Ground/")
		   || assetPath.Contains("Assets/Standard Assets/Geo/Fish/"))
		{   
			modelImporter.materialSearch = ModelImporterMaterialSearch.Local;
		}
    }
}

