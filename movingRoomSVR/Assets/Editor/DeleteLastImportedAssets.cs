using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

public class DeleteLastImportedAssets : ScriptableWizard 
{
	public string[] toBeDeletedFiles;
	[MenuItem("WEMOTools/Delete Last Imported Assets &z")]
	static void CreateWizard()
	{
		DeleteLastImportedAssets me = ScriptableWizard.DisplayWizard("Delete last imported assets", typeof(DeleteLastImportedAssets), "Delete") as DeleteLastImportedAssets;
		string lastImportedAssetsDataFile = string.Concat(Application.dataPath, "/../WEMOData/LastImportedAssetsData.json");
		if (!File.Exists(lastImportedAssetsDataFile))
		{
			Debug.LogWarning("lastImportedAssetsDataFile doesn't exist");
			return;
		}
		
		me.toBeDeletedFiles = File.ReadAllLines(lastImportedAssetsDataFile);
	}
	
	void OnWizardCreate()
	{
		string displayString = string.Empty;
		foreach(string toDelete in toBeDeletedFiles)
		{
			displayString+=toDelete + "\n";
		}
		if( EditorUtility.DisplayDialog("Delete these files ?", displayString , "Yes", "No"))
		{
			foreach(string toDelete in toBeDeletedFiles)
			{
				AssetDatabase.DeleteAsset(toDelete);
			}
		}
	}
}

