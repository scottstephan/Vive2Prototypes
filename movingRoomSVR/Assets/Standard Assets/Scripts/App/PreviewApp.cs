using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class PreviewApp : AppBase {
 
    public int previewSphereID = 0;
//    private static Log log = Log.GetLog(typeof(PreviewApp).FullName)
	
	public override bool RunningAsPreview() {
		return true;
	}

	public override bool Initialized() {
		return true;
	}

	public override void Start() {
		base.Start();
	}

	void Update() {
		if( !firstUpdate ) {	

/*            CameraManager.SwitchToCamera(CameraType.GraffitiCamera);

            // TODO> refactor this weird way to trigger these cameras.
            CameraManager.SwitchToCamera(CameraType.PreviewSceneMakerDebugCamera);
            CameraManager.SwitchToCamera(CameraType.RotateAroundObjectCamera);*/

            CameraManager.singleton.PostSphereLoadSetup();
            AudioManager.SetGlobalMute( DEBUG_muteMusic ); // set this lst so audio has been loaded to trigger if turning sound on
            firstUpdate = true;
		}

		float dt = Time.deltaTime;

		InputManager.UpdateInput( dt );		

		SphereInstance.Instance.UpdateSphere();

		SimManager.UpdateSim( dt );
	}
}
