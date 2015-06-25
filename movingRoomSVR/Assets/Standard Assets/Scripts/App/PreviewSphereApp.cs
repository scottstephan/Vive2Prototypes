using UnityEngine;
using System.Collections;

public class PreviewSphereApp : PreviewApp {
		
	public override bool Initialized() {
		return false;
	}
	// Use this for initialization
	public override void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	void Update () {
		if( !firstUpdate ) {	
            //RICHARDTODO: FIX THIS
            //SphereManagerOLD.startLoadingSphereID = 0;
			//SphereManagerOLD.SphereLoaded(null);

            // TODO> refactor this weird way to trigger these cameras.
            CameraManager.SwitchToCamera(CameraType.PreviewSceneMakerDebugCamera);
            CameraManager.SwitchToCamera(CameraType.RotateAroundObjectCamera);
			
            AudioManager.SetGlobalMute( DEBUG_muteMusic );

			firstUpdate = true;
		}
		
	if(CameraManager.GetActiveCameraType() != CameraType.PreviewSceneMakerDebugCamera){
			CameraManager.SwitchToCamera(CameraType.PreviewSceneMakerDebugCamera);
            CameraManager.SwitchToCamera(CameraType.RotateAroundObjectCamera);
		}

		float dt = Time.deltaTime;

		InputManager.UpdateInput( dt );		
		
		SphereInstance.Instance.UpdateSphere();

		SimManager.UpdateSim( dt );
	}	
}
