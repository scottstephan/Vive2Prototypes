using UnityEngine;
using System.Collections;

public class ExitOnBack : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if (FloatingMenuManager.IsMenuUp())
			return;
		
#if UNITY_ANDROID
		if (InputManager.AnyMoonlightBackTap())
			CameraManager.singleton.ovrController.ReturnToLauncher();
#endif	
	}

    void OnApplicationPause( bool pause ) {
        InputManager.OnApplicationPause( pause );
    }
}
