using UnityEngine;
using System.Collections;

// Special class for when the user loads into the blu without having the Unity plugin installed.
// This class calls out to the html to force reload the popup into the real scene once the plugin is installed.
public class InstallUnityScene : MonoBehaviour {
	
	private static bool called = false;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if( !called ) {
			Application.ExternalCall("goToSphere");
			called = true;
		}
	}
}
