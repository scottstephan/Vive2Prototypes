// Copyright (c) 2014 Nathan Martz

using UnityEditor;
using UnityEngine;

public class SECTR_VisMenu : SECTR_Menu 
{
	const string rootPath = createMenuRootPath + "VIS/";
	const string createSectorCamera = rootPath + "Camera";
	const string createSectorOccluder = rootPath + "Occluder";
	const int createSectorPriority = visPriority + 0;
	const int createOccluderPriority = visPriority + 5;
	const int makeCullerPriority = visPriority + 10;

	[MenuItem(createSectorCamera, false, createSectorPriority)]
	public static void CreateSectorCamera() 
	{
		string newObjectName = "SECTR Camera";
		string undoName = "Create " + newObjectName;
		if(Selection.activeGameObject && Selection.activeGameObject.GetComponent<Camera>())
		{
			if(Selection.activeGameObject.GetComponent<SECTR_CullingCamera>())
			{
				Debug.LogWarning("Selected Camera already has a SECTR CullingCamera.");
			}
			else
			{
				SECTR_CullingCamera newCullingCamera = Selection.activeGameObject.AddComponent<SECTR_CullingCamera>();
				SECTR_Undo.Created(newCullingCamera, undoName);
			}
		}
		else
		{
			GameObject newObject = CreateGameObject(newObjectName);
			newObject.AddComponent<SECTR_CullingCamera>();
			SECTR_Undo.Created(newObject, undoName);
			Selection.activeGameObject = newObject;
		}
	}

	[MenuItem(createSectorOccluder, false, createOccluderPriority)]
	public static void CreateSectorOccluder() 
	{
		string newObjectName = "SECTR Occluder";
		string undoName = "Create " + newObjectName;
		GameObject newObject = CreateGameObject(newObjectName);
		SECTR_Occluder newOccluder = newObject.AddComponent<SECTR_Occluder>();
		newOccluder.ForceEditHull = true;
		newOccluder.CenterOnEdit = true;
		SECTR_Undo.Created(newObject, undoName);
		Selection.activeGameObject = newObject;
	}
}
