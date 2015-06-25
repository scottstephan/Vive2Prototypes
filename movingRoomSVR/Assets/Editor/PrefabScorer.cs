using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PrefabScorer : ScriptableWizard {
	const int NORMAL_SHADER_DRAW_CALL_COST = 1;
	const int BLEND_VEGETATION_DRAW_CALL_COST = 2;
	
	public List<GameObject> toBeScoredPrefab = new List<GameObject>();
	
	private List<string> content = new List<string>();
	private string head = "Prefab Name,Score,Draw Calls(LOD0),Draw Calls(LOD1),Draw Calls(LOD2),Total Verts(LOD0),Total Verts(LOD1),Total Verts(LOD2),Total Texture Size(KB)(LOD0),Total Texture Size(KB)(LOD1),Total Texture Size(KB)(LOD2),Bones";
	[MenuItem("WEMOTools/Prefab Evaluator")]
	static PrefabScorer CreateWizard()
	{
		return ScriptableWizard.DisplayWizard("Prefab Scorer", typeof(PrefabScorer), "Make Score") as PrefabScorer;
	}
	void OnWizardUpdate()
	{
		helpString = "When 'To Be Scored Prefab list' is empty, \nthis will evaluate all the fish and seabed prefabs.\n" +
			"Make sure that specified prefabs in the list below are also in the empty scene. \n" +
			"\n" +
			"1. Start with a completely empty scene.\n" +
			"2. Either click on 'make score' for scoring all seabed and fish prefabs \n" +
			"    - or - \n" +
			"    drag prefabs into the empty scene as well as the list below.\n" +
			"3. csv file will be Generated in 'unity\\WEMOData\\prefabScores.csv'.\n" ;
	}
	void OnWizardCreate()
	{
		content.Add(head);
		
			
		if(toBeScoredPrefab.Count == 0)
		{
			List<GameObject> prefabs = new List<GameObject>();
			string fishPrefabDirectory = "Assets/Standard Assets/Prefabs/Fish/";
			string seabedPrefabDirectory = "Assets/Standard Assets/Prefabs/seabed/";
			string[] fishPrefabPaths = Directory.GetFiles(fishPrefabDirectory,"*.*",SearchOption.AllDirectories);
			string[] seabedPrefabPaths = Directory.GetFiles(seabedPrefabDirectory,"*.*",SearchOption.AllDirectories);
			foreach(string fishPrefabPath in fishPrefabPaths)
			{
				if(fishPrefabPath.Contains(".meta"))
				{
					continue;
				}
				prefabs.Add(AssetDatabase.LoadAssetAtPath(fishPrefabPath, typeof(GameObject)) as GameObject);
			}
			foreach(string seabedPrefabPath in seabedPrefabPaths)
			{
				if(seabedPrefabPath.Contains(".meta"))
				{
					continue;
				}
				prefabs.Add(AssetDatabase.LoadAssetAtPath(seabedPrefabPath, typeof(GameObject)) as GameObject);
			}
			foreach(GameObject prefab in prefabs)
			{
				GameObject go = Instantiate(prefab) as GameObject;
				toBeScoredPrefab.Add(go);
			}
		}
		
		foreach(GameObject prefab in toBeScoredPrefab)
		{
			//calc draw calls for this prefab for lod0
			int drawCalls = 0;
			int drawCalls1 = 0;
			int drawCalls2 = 0;
		/*	if(prefab.name.Contains( "anggie") ||
			   prefab.name.Contains( "pLight") ||
			   prefab.name.Contains( "spot-airline"))
			{
				continue;
			}*/
			Debug.Log(prefab.name);
			if(prefab.transform.Find("LOD0") != null)
			{
				GameObject lod0 = ((prefab.transform.Find("LOD0")).gameObject);
				Renderer[] allRendersInPrefab = lod0.GetComponentsInChildren<Renderer>();
				foreach(Renderer render in allRendersInPrefab)
				{
					if(render == null)
					{
						continue;
					}
					Material[] materials = render.sharedMaterials;
					foreach(Material material in materials)
					{
						if(material == null)
						{
							continue;
						}
						if(material.shader == null)
						{
							continue;
						}
						string shaderName = material.shader.name;
						shaderName = shaderName.ToLower();
						if(shaderName.Contains("blend") || shaderName.Contains("vegetation"))
						{
							drawCalls+=2;
						}
						else
						{
							drawCalls+=1;
						}
					}
				}
			}
			//calc draw calls for this prefab for lod1
			if(prefab.transform.Find("LOD1") != null)
			{
				GameObject lod0 = ((prefab.transform.Find("LOD1")).gameObject);
				Renderer[] allRendersInPrefab = lod0.GetComponentsInChildren<Renderer>();
				foreach(Renderer render in allRendersInPrefab)
				{
					if(render == null)
					{
						continue;
					}
					Material[] materials = render.sharedMaterials;
					foreach(Material material in materials)
					{
						if(material == null)
						{
							continue;
						}
						if(material.shader == null)
						{
							continue;
						}
						string shaderName = material.shader.name;
						shaderName = shaderName.ToLower();
						if(shaderName.Contains("blend") || shaderName.Contains("vegetation"))
						{
							drawCalls1+=2;
						}
						else
						{
							drawCalls1+=1;
						}
					}
				}
			}
			//calc draw calls for this prefab for lod2
			if(prefab.transform.Find("LOD2") != null)
			{
				GameObject lod0 = ((prefab.transform.Find("LOD2")).gameObject);
				Renderer[] allRendersInPrefab = lod0.GetComponentsInChildren<Renderer>();
				foreach(Renderer render in allRendersInPrefab)
				{
					if(render == null)
					{
						continue;
					}
					Material[] materials = render.sharedMaterials;
					foreach(Material material in materials)
					{
						if(material == null)
						{
							continue;
						}
						if(material.shader == null)
						{
							continue;
						}
						string shaderName = material.shader.name;
						shaderName = shaderName.ToLower();
						if(shaderName.Contains("blend") || shaderName.Contains("vegetation"))
						{
							drawCalls2+=2;
						}
						else
						{
							drawCalls2+=1;
						}
					}
				}
			}
			//calc verts
			int verts = 0;
			int vert0 = 0;
			int vert1 = 0;
			int vert2 = 0;
			
		/*	commented out, cause it was calculating all meshes besides just the submitted meshes from maker
		 * SkinnedMeshRenderer[] meshFilters = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach(SkinnedMeshRenderer meshFilter in meshFilters)
			{
				verts += meshFilter.sharedMesh.vertexCount;
			} */
			if(prefab.transform.Find("LOD0") != null)
			{
				SkinnedMeshRenderer[] meshFilters0 = prefab.transform.Find("LOD0").GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach(SkinnedMeshRenderer meshFilter in meshFilters0)
				{
					vert0 += meshFilter.sharedMesh.vertexCount;
				}
			}
			if(prefab.transform.Find("LOD1") != null)
			{
				SkinnedMeshRenderer[] meshFilters0 = prefab.transform.Find("LOD1").GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach(SkinnedMeshRenderer meshFilter in meshFilters0)
				{
					vert1 += meshFilter.sharedMesh.vertexCount;
				}
			}
			if(prefab.transform.Find("LOD2") != null)
			{
				SkinnedMeshRenderer[] meshFilters0 = prefab.transform.Find("LOD2").GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach(SkinnedMeshRenderer meshFilter in meshFilters0)
				{
					vert2 += meshFilter.sharedMesh.vertexCount;
				}
			}
			//verts now is just adding all LOD vert counts, to avoid calculating collision or other unecessary meshes. -Tamar
			verts = vert0 + vert1 + vert2;
			
			//calc texture size total
			double sizeOfTextures = 0;
			double lod0TextureSize = 0;
			double lod1TextureSize = 0;
			double lod2TextureSize = 0;
			
			Renderer[] allRendersInPrefabFromAllLODs = prefab.GetComponentsInChildren<Renderer>();
			foreach(Renderer render in allRendersInPrefabFromAllLODs)
			{
				if(render == null)
				{
					continue;
				}
				Material[] materials = render.sharedMaterials;
				foreach(Material material in materials)
				{
					if(material == null)
					{
						continue;
					}
					List<Texture> textureList = new List<Texture>();
					if(material.HasProperty("_MainTex"))
					{
						Texture textureToBeAdded = material.GetTexture("_MainTex");
						if(textureToBeAdded != null)
							textureList.Add(textureToBeAdded);
					}
					if(material.HasProperty("_NormMap"))
					{
						Texture textureToBeAdded = material.GetTexture("_NormMap");
						if(textureToBeAdded != null)
							textureList.Add(textureToBeAdded);
					}
					if(material.HasProperty("_SpecMap"))
					{
						Texture textureToBeAdded = material.GetTexture("_SpecMap");
						if(textureToBeAdded != null)
							textureList.Add(textureToBeAdded);
					}
					foreach(Texture texture in textureList)
					{
						
						string texturePath = AssetDatabase.GetAssetPath(texture);
						
						texturePath = string.Concat(Application.dataPath, texturePath.TrimStart("Assets".ToCharArray()));
						//if(File.Exists(texturePath))
						{
							Debug.Log(texturePath);
							
							FileInfo fileInfo = new FileInfo(texturePath);
							sizeOfTextures += (double)fileInfo.Length;
							
						}
					}
				}
			}
			//sizeOfTextures = (sizeOfTextures/24) * 0.001f * 4.0f ; // in kb compressed , 24 is fixed compressed size that unity does
			//lod0 texture
			if(prefab.transform.Find("LOD0") != null)
			{
			Renderer[] allRendersInPrefabFromLOD0 = prefab.transform.FindChild("LOD0").GetComponentsInChildren<Renderer>();
			foreach(Renderer render in allRendersInPrefabFromLOD0)
			{
				if(render == null)
				{
					continue;
				}
				Material[] materials = render.sharedMaterials;
				foreach(Material material in materials)
				{
					if(material == null)
					{
						continue;
					}
					List<Texture> textureList = new List<Texture>();
					if(material.HasProperty("_MainTex"))
					{
						Texture textureToBeAdded = material.GetTexture("_MainTex");
						if(textureToBeAdded != null)
							textureList.Add(textureToBeAdded);
					}
					if(material.HasProperty("_NormMap"))
					{
						Texture textureToBeAdded = material.GetTexture("_NormMap");
						if(textureToBeAdded != null)
							textureList.Add(textureToBeAdded);
					}
					if(material.HasProperty("_SpecMap"))
					{
						Texture textureToBeAdded = material.GetTexture("_SpecMap");
						if(textureToBeAdded != null)
							textureList.Add(textureToBeAdded);
					}
					foreach(Texture texture in textureList)
					{
						
						string texturePath = AssetDatabase.GetAssetPath(texture);
						
						texturePath = string.Concat(Application.dataPath, texturePath.TrimStart("Assets".ToCharArray()));
						//if(File.Exists(texturePath))
						{
							Debug.Log(texturePath);
							
							FileInfo fileInfo = new FileInfo(texturePath);
							lod0TextureSize += (double)fileInfo.Length;
							
						}
					}
				}
			}
			lod0TextureSize = (lod0TextureSize/24) * 0.001f * 3.9345f ;
			}
			//lod1 texture
			if(prefab.transform.Find("LOD1") != null)
			{
			Renderer[] allRendersInPrefabFromLOD1 = prefab.transform.FindChild("LOD1").GetComponentsInChildren<Renderer>();
			foreach(Renderer render in allRendersInPrefabFromLOD1)
			{
				if(render == null)
				{
					continue;
				}
				Material[] materials = render.sharedMaterials;
				foreach(Material material in materials)
				{
					if(material == null)
					{
						continue;
					}
					List<Texture> textureList = new List<Texture>();
					if(material.HasProperty("_MainTex"))
					{
						Texture textureToBeAdded = material.GetTexture("_MainTex");
						if(textureToBeAdded != null)
							textureList.Add(textureToBeAdded);
					}
					if(material.HasProperty("_NormMap"))
					{
						Texture textureToBeAdded = material.GetTexture("_NormMap");
						if(textureToBeAdded != null)
							textureList.Add(textureToBeAdded);
					}
				/* lod1 does not have a spec map	
				 * if(material.HasProperty("_SpecMap"))
					{
						Texture textureToBeAdded = material.GetTexture("_SpecMap");
						if(textureToBeAdded != null)
							textureList.Add(textureToBeAdded);
					} */
					foreach(Texture texture in textureList)
					{
						
						string texturePath = AssetDatabase.GetAssetPath(texture);
						
						texturePath = string.Concat(Application.dataPath, texturePath.TrimStart("Assets".ToCharArray()));
						//if(File.Exists(texturePath))
						{
							Debug.Log(texturePath);
							
							FileInfo fileInfo = new FileInfo(texturePath);
							lod1TextureSize += (double)fileInfo.Length;
							
						}
					}
				}
			}
			lod1TextureSize = (lod1TextureSize/24) * 0.001f * 2.063f ;
			}
			//lod2 texture
			if(prefab.transform.Find("LOD2") != null)
			{
			Renderer[] allRendersInPrefabFromLOD2 = prefab.transform.FindChild("LOD2").GetComponentsInChildren<Renderer>();
			foreach(Renderer render in allRendersInPrefabFromLOD2)
			{
				if(render == null)
				{
					continue;
				}
				Material[] materials = render.sharedMaterials;
				foreach(Material material in materials)
				{
					if(material == null)
					{
						continue;
					}
					List<Texture> textureList = new List<Texture>();
					if(material.HasProperty("_MainTex"))
					{
						Texture textureToBeAdded = material.GetTexture("_MainTex");
						if(textureToBeAdded != null)
							textureList.Add(textureToBeAdded);
					}
			/*		LOD2 does not use normal map or spec map
					if(material.HasProperty("_NormMap"))
					{
						Texture textureToBeAdded = material.GetTexture("_NormMap");
						if(textureToBeAdded != null)
							textureList.Add(textureToBeAdded);
					}
					if(material.HasProperty("_SpecMap"))
					{
						Texture textureToBeAdded = material.GetTexture("_SpecMap");
						if(textureToBeAdded != null)
							textureList.Add(textureToBeAdded);
					} */
					foreach(Texture texture in textureList)
					{
						
						string texturePath = AssetDatabase.GetAssetPath(texture);
						
						texturePath = string.Concat(Application.dataPath, texturePath.TrimStart("Assets".ToCharArray()));
						//if(File.Exists(texturePath))
						{
							Debug.Log(texturePath);
							
							FileInfo fileInfo = new FileInfo(texturePath);
							lod2TextureSize += (double)fileInfo.Length;
							
						}
					}
				}
			}
			lod2TextureSize = (lod2TextureSize/24) * 0.001f * 1.95f;
			
			}
			
			//Moved size of textures here so that it's just the addition of all textures from each LOD...cause the LOD math is right. 
			sizeOfTextures = lod0TextureSize + lod1TextureSize + lod2TextureSize;
			
			//calc bones
			int boneCount = 0;
			if((prefab.transform.Find("root")) != null)
			{
				GameObject root = (prefab.transform.Find("root")).gameObject;
				Transform[] childrenTransforms = root.GetComponentsInChildren<Transform>();
				boneCount = childrenTransforms.Length - 1;
			}
				
			string prefabPath = AssetDatabase.GetAssetPath(prefab);
			double score = 0;
			if(prefabPath.Contains("seabed"))
			{
				score =  ((drawCalls / 7) * 1.2) + ((verts/ 11400) * 1.1) + (((sizeOfTextures) / 3024)  ) + ((boneCount / 80) * 6);
			}
			else
			{
				score =  ((((float)verts)/ 11400) * 9) + (((float)sizeOfTextures) / 3072 ) + ((((float)boneCount) / 80) * 6); //3024
			}
			string lineToAdd = prefab.name + "," + ((float)score).ToString() + "," + drawCalls.ToString() + ","+ drawCalls1.ToString() + ","+ drawCalls2.ToString() + "," + vert0.ToString() +","+ vert1.ToString() +","+ vert2.ToString() +"," + ((float)lod0TextureSize).ToString("F2") +","+ ((float)lod1TextureSize).ToString("F2") +","+ ((float)lod2TextureSize).ToString("F2") +"," + boneCount.ToString();
			content.Add(lineToAdd);
		}
		string prefabScoresFile = string.Concat(Application.dataPath, "/../WEMOData/prefabScores.csv");
		
		if(File.Exists(prefabScoresFile))
		{
			File.Delete(prefabScoresFile);
		}
		string WEMODataDirectory = string.Concat(Application.dataPath, "/../WEMOData/");
		if(!Directory.Exists(WEMODataDirectory))
		{
			Directory.CreateDirectory(WEMODataDirectory);
		}
		File.WriteAllLines(prefabScoresFile,content.ToArray());
		
	}
}
