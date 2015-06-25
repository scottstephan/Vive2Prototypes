using UnityEngine;
using UnityEditor;
using System.Collections;

// Changes the Scale Factor of imported FBX models to be 1 (instead of 0.01)
public class ResizeFbxOnImport : AssetPostprocessor {

/*	void OnPreprocessModel() {
		//Debug.Log("OnPostprocessModel function called");
		
		ModelImporter importer = (ModelImporter)assetImporter;
		importer.globalScale = 1;
	}*/
}
