using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class LightmapLODTool : EditorWindow
{
	List<LODModelData> modelDataCollection = new List<LODModelData>();
	Dictionary<string, bool> foldoutStatus = new Dictionary<string, bool>();
	List<string> openingObjects = new List<string>();
	Vector2 scrollPos;

	[MenuItem("WEMOTools/Lightmap LOD tool")]
	static void ShowWindow()
	{
		LightmapLODTool window = (LightmapLODTool)EditorWindow.GetWindow(typeof(LightmapLODTool));
		window.ScanCurrentValues();
	}

	void OnGUI()
	{
		if (foldoutStatus.Count == 0)
		{
			ScanCurrentValues();
		}

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Start wizard", GUILayout.Width(100)))
		{
			StartWizard();
		}

		GUILayout.Space(10);
		GUILayout.EndHorizontal();

		if (modelDataCollection.Count > 0)
		{
			RenderCurrentValues();
		}
	}

	void OnHierarchyChange()
	{
		ResetState();
	}

	void RenderCurrentValues()
	{
		GUILayout.Label("Current values:", EditorStyles.boldLabel);
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		GUILayout.BeginVertical();
		foreach (LODModelData lodModel in modelDataCollection)
		{
			string ID = GetID(lodModel);
			GUILayout.BeginHorizontal();
			foldoutStatus[ID] = EditorGUILayout.Foldout(foldoutStatus[ID], ID);
			if (GUILayout.Button("Select", GUILayout.Width(50)))
			{
				Selection.activeGameObject = lodModel.gameObject;
			}
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			if (foldoutStatus[ID])
			{
				GUILayout.BeginVertical();
				// Browse for other LODs
				for (int i = 0; i < lodModel.LODs.Length; i++)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("===========LOD" + i + "===========");
					if (GUILayout.Button("Select", GUILayout.Width(50)))
					{
						Selection.activeGameObject = lodModel.LODs[i].LOD;
					}
					GUILayout.Space(10);
					GUILayout.EndHorizontal();

					// Get and modify renderers
					Renderer[] tmpArr = lodModel.LODs[i].LOD.GetComponentsInChildren<Renderer>();
					foreach (Renderer renderer in tmpArr)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label(renderer.gameObject.name + ": " + renderer.lightmapIndex.ToString() + " " + renderer.lightmapScaleOffset.ToString());
						if (GUILayout.Button("Select", GUILayout.Width(50)))
						{
							Selection.activeGameObject = renderer.gameObject;
						}
						GUILayout.Space(10);
						GUILayout.EndHorizontal();
					}
				}
				GUILayout.EndVertical();
			}
		}
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
	}

	void ResetState()
	{
		modelDataCollection = new List<LODModelData>();
		openingObjects = new List<string>();
		foreach (KeyValuePair<string, bool> pair in foldoutStatus)
		{
			if (pair.Value)
				openingObjects.Add(pair.Key);
		}
		foldoutStatus = new Dictionary<string, bool>();
	}

	void StartWizard()
	{
		LODModelData[] components = (LODModelData[])GameObject.FindObjectsOfType(typeof(LODModelData));

		Renderer tmp;
		int copiedLightmapIndex;
		Vector4 copiedLightmapTilingOffset;
		//List<Vector2[]> copiedUVs = new List<Vector2[]>();
		//List<Vector2[]> copiedUV1s = new List<Vector2[]>();
		//List<Vector2[]> copiedUV2s = new List<Vector2[]>();
		foreach (LODModelData model in components)
		{
			Debug.Log("model: "+ model.name);
			try{
				// Get first renderer of LOD0
				tmp = model.LODs[0].LOD.GetComponentInChildren<Renderer>();
				
				copiedLightmapIndex = tmp.lightmapIndex;
				copiedLightmapTilingOffset = tmp.lightmapScaleOffset;
	
				//MeshFilter[] meshFilters = model.LODs[0].LOD.GetComponentsInChildren<MeshFilter>();
				//copiedUVs.Clear();
				//copiedUV1s.Clear();
				//copiedUV2s.Clear();
				//foreach (MeshFilter mf in meshFilters)
				//{
				//    copiedUVs.Add(mf.sharedMesh.uv);
				//    copiedUV1s.Add(mf.sharedMesh.uv1);
				//    copiedUV2s.Add(mf.sharedMesh.uv2);
				//}
	
				// Browse for other LODs
				for (int i = 1; i < model.LODs.Length; i++)
				{
					// Get and modify renderers
					Renderer[] tmpArr = model.LODs[i].LOD.GetComponentsInChildren<Renderer>();
					foreach (Renderer renderer in tmpArr)
					{
						renderer.lightmapIndex = copiedLightmapIndex;
						renderer.lightmapScaleOffset = copiedLightmapTilingOffset;
					}
	
					// Get and modify uv information
					//MeshFilter[] tmpMFs = model.LODs[i].LOD.GetComponentsInChildren<MeshFilter>();
					//for (int j = 0; j < tmpMFs.Length; j++)
					//{
					//tmpMFs[j].mesh.uv = copiedUVs[j];
					//tmpMFs[j].mesh.uv1 = copiedUV1s[j];
					//tmpMFs[j].mesh.uv2 = copiedUV2s[j];
					//}
				}
			}catch{
			 	continue;
			}
		}

		ScanCurrentValues();
	}

	string GetID(Object obj)
	{
		return obj.GetInstanceID().ToString() + "." + obj.name;
	}

	void ScanCurrentValues()
	{
		ResetState();
		LODModelData[] lodModels = (LODModelData[])GameObject.FindObjectsOfType(typeof(LODModelData));
		foreach (LODModelData lodModel in lodModels)
		{
			string ID = GetID(lodModel);
			modelDataCollection.Add(lodModel);
			foldoutStatus.Add(ID, openingObjects.Contains(ID));
		}
	}
}