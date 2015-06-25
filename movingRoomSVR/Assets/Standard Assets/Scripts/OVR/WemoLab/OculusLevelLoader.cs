using UnityEngine;
using System.Collections;

public class OculusLevelLoader : MonoBehaviour
{
	
	public enum OculusLevelLoaderMode
	{
		ON_ENTER,
		ON_LEAVE
	}
	
	private bool isTriggered = false;
	
	public OculusLevelLoaderMode loaderMode = OculusLevelLoaderMode.ON_ENTER;
	
	public void OnTriggerExit (Collider col)
	{
		if(!isTriggered && col.tag == "MainCamera"){
			isTriggered = true;
			App.SphereManager.LoadSphere("51df39a88eb84da4e4000005");
		}
	}
}
