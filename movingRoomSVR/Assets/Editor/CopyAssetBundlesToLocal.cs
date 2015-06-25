using UnityEngine;
using System.Collections;
using Unity;
using UnityEditor;

public class CopyAssetBundlesToLocal : Editor {
	
	[MenuItem("WEMOTools/Copy Android Streaming Assets")]
	public static void CopyAndroidAssets ()
	{
		CopyAssetsFrom("Assets/StreamingAssets_Android");
	}
	
	[MenuItem("WEMOTools/Copy PC Streaming Assets")]
	public static void CopyPCAssets ()
	{
		CopyAssetsFrom("Assets/StreamingAssets_PC");
	}


	static void CopyAssetsFrom(string from) {

		string[] folders = new string[1];
		folders[0] = from;

		string[] tofolders = new string[1];
		tofolders[0] = "Assets/StreamingAssets";

		string[] guids = AssetDatabase.FindAssets("", folders);

		string[] oldguids = AssetDatabase.FindAssets("*", tofolders);

		//delete the old ones
		foreach (string g in oldguids) {
			Debug.Log("Delete old: " + AssetDatabase.GUIDToAssetPath(g));
			AssetDatabase.MoveAssetToTrash(AssetDatabase.GUIDToAssetPath(g));
		}

		//copy over the new ones
		foreach (string ng in guids) {
			Debug.Log("Copy new: " + AssetDatabase.GUIDToAssetPath(ng) + " to: " + tofolders[0]);

			if (AssetDatabase.CopyAsset(AssetDatabase.GUIDToAssetPath(ng), tofolders[0]))
				Debug.Log("Asset Copy: Success!");
			else {
				Debug.Log("Asset Copy: Fail?! Manually copying.");
				FileUtil.CopyFileOrDirectory(AssetDatabase.GUIDToAssetPath(ng), tofolders[0]);
			}
		}

		AssetDatabase.Refresh();
	}
}
