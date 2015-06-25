using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
	

[CustomEditor(typeof(AtlasReader))]
public class AtlasReaderEditor : Editor 
{
	public bool doReload;
	public string jsonPath;
	public bool doSetupAll;
	public bool doUndoAll;
	
	public override void OnInspectorGUI()
	{

		AtlasReader myReader = (AtlasReader)target;
		AtlasData data = myReader.atlasData;

		jsonPath = EditorGUILayout.TextField("Json Path",myReader.jsonPath);
		myReader.jsonPath = jsonPath;

		doReload = EditorGUILayout.Toggle("Reload", doReload);
		if(doReload){
			doReload = false;
			myReader.Reload();
		}

		
		doSetupAll = EditorGUILayout.Toggle("Setup All", doSetupAll);
		if(doSetupAll){
			doSetupAll = false;
			SetupAll();
		}
        	
		if(data != null && data.frames != null && data.frames.Count > 0){
			foreach( KeyValuePair<string, AtlasSprite> kvp in data.frames){
				AtlasSprite sp = kvp.Value;
				EditorGUILayout.SelectableLabel(sp.name);
			}
		}



			//EditorGUILayout.
		//EditorGUILayout.LabelField("atlas", "test");
		//myTarget.experience = EditorGUILayout.IntField("Experience", myTarget.experience);
		//EditorGUILayout.LabelField("Level", myTarget.Level.ToString());
	}

	
	public void SetupAll(){
		AtlasMaterial[] mats = FindObjectsOfType(typeof(AtlasMaterial)) as AtlasMaterial[];
		foreach(AtlasMaterial am in mats){
			if(am.gameObject.isStatic)
				am.Setup();
		}
	}
}