using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
	

[CustomEditor(typeof(AtlasMaterial))]
[CanEditMultipleObjects]
public class AtlasMaterialEditor : Editor 
{
	public bool doReload;
	public string atlasName;
	public string spriteName;

	void OnEnable(){
	}


	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		AtlasMaterial myAtlasMaterial = (AtlasMaterial)target;
		//AtlasData data = myReader.atlasData;

		atlasName = EditorGUILayout.TextField("atlasName",myAtlasMaterial.atlasName);
		myAtlasMaterial.atlasName = atlasName;
		
		spriteName = EditorGUILayout.TextField("spriteName",myAtlasMaterial.spriteName);
		myAtlasMaterial.spriteName = spriteName;

		
		//origMesh = EditorGUILayout.ObjectField("origMesh", myAtlasMaterial.origMesh,typeof(Mesh), true) as Mesh;
		//myAtlasMaterial.origMesh = origMesh;

		doReload = EditorGUILayout.Toggle("Reload", doReload);
		if(doReload){
			doReload = false;
			myAtlasMaterial.Setup();
		}

		
		serializedObject.ApplyModifiedProperties();

			//EditorGUILayout.
		//EditorGUILayout.LabelField("atlas", "test");
		//myTarget.experience = EditorGUILayout.IntField("Experience", myTarget.experience);
		//EditorGUILayout.LabelField("Level", myTarget.Level.ToString());
	}

}
