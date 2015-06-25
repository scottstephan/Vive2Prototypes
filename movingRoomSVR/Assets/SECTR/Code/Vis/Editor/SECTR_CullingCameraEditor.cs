// Copyright (c) 2014 Nathan Martz

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SECTR_CullingCamera))]
[CanEditMultipleObjects]
public class SECTR_CullingCameraEditor : SECTR_Editor
{
	public override void OnInspectorGUI()
	{
		SECTR_CullingCamera myCamera = (SECTR_CullingCamera)target;
		base.OnInspectorGUI();
		if(GUILayout.Button(new GUIContent("Add Cullers To All Members", "Adds Cullers to all components, will save some first frame CPU time.")))
		{
			myCamera.AddCullersToAllMembers();
		}
	}
}
