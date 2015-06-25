using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class SECTR_VC
{
	public static bool HasVC()
	{
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1
		return false;
#else
		return UnityEditor.VersionControl.Provider.enabled && UnityEditor.VersionControl.Provider.isActive;
#endif
	}
	
	public static void WaitForVC()
	{
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_1)
		if(HasVC())
		{
			while(UnityEditor.VersionControl.Provider.activeTask != null)
			{
				UnityEditor.VersionControl.Provider.activeTask.Wait();
			}
		}
#endif
		AssetDatabase.Refresh();
		AssetDatabase.SaveAssets();

	}
	
	public static bool CheckOut(string path)
	{
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1
		return true;
#else
		if(HasVC())
		{
			UnityEditor.VersionControl.Asset vcAsset = UnityEditor.VersionControl.Provider.GetAssetByPath(path);
			if(vcAsset != null)
			{
				UnityEditor.VersionControl.Task task = UnityEditor.VersionControl.Provider.Checkout(vcAsset, UnityEditor.VersionControl.CheckoutMode.Both);
				task.Wait();
			}
		}
		return IsEditable(path);
#endif
	}
	
	public static void Revert(string path)
	{
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_1)
		if(HasVC())
		{
			UnityEditor.VersionControl.Asset vcAsset = UnityEditor.VersionControl.Provider.GetAssetByPath(path);
			if(vcAsset != null)
			{
				UnityEditor.VersionControl.Task task = UnityEditor.VersionControl.Provider.Revert(vcAsset, UnityEditor.VersionControl.RevertMode.Normal);
				task.Wait();
				AssetDatabase.Refresh();
			}
		}
#endif
	}
	
	public static bool IsEditable(string path)
	{
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1
		return true;
#else
		if(HasVC())
		{
			UnityEditor.VersionControl.Asset vcAsset = UnityEditor.VersionControl.Provider.GetAssetByPath(path);
			return vcAsset != null ? UnityEditor.VersionControl.Provider.IsOpenForEdit(vcAsset) : true;
		}
		else
		{
			return true;
		}
#endif
	}
}
