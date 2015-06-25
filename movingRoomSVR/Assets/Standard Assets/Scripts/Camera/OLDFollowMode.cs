using UnityEngine;
using System.Collections;

public class OLDFollowMode : BaseCameraMode {
	private GameObject internalTarget;

	public float distance = 50.0f;
	public float height = 0.0f;
	public float damping = 2.0f;
	public bool smoothRotation = true;
	public float rotationDamping = 4.0f;
	
	public float maxCameraHeight = -10.0f;
	
	void Awake ()
	{
		internalTarget = null;
	}
	
    public override void InitCameraMode() {
        if( inited ) {
            return;
        }
        
        base.InitCameraMode();        
        
		cameraType = CameraType.OLDFollowBehindCamera;
		myTransform = transform;
	}

	public override bool GetFollowsTargets() 
	{ 
		return true; 
	}

	public override void UpdateCameraMode() {
		float dt = Time.deltaTime;

		if( internalTarget != CameraManager.currentTarget ) { 
			internalTarget = CameraManager.currentTarget;
			
	   		FishCameraConstants constants = internalTarget.GetComponent<FishCameraConstants>();
	   		distance = constants.OLDFollow_distance;
	   		height = constants.OLDFollow_height;
	   		damping = constants.OLDFollow_damping;
			smoothRotation = constants.OLDFollow_smoothRotation;
			rotationDamping = constants.OLDFollow_rotationDamping;
	    }
		
		Vector3 wantedPosition = internalTarget.transform.TransformPoint(0, height, -distance);
		
		// run our collision check
		myTransform.position = Vector3.Lerp (myTransform.position, wantedPosition, dt * damping);
		
		Vector3 center = CameraManager.GetFollowCamSphereCenter();
		Vector3 diff = myTransform.position - center;
		float d = CameraManager.GetFollowCamSphereDistance();
		if( diff.magnitude > d ) {
			diff *= ( d / diff.magnitude);
			myTransform.position = center + diff;
		}

		if (myTransform.position.y > maxCameraHeight ) {
			myTransform.position = new Vector3(myTransform.position.x,maxCameraHeight,myTransform.position.z);
		}

		if (smoothRotation) {
			Quaternion wantedRotation = Quaternion.LookRotation(internalTarget.transform.position - myTransform.position, internalTarget.transform.up);
			myTransform.rotation = Quaternion.Slerp (myTransform.rotation, wantedRotation, dt * rotationDamping);
		}
		else {
			myTransform.LookAt(internalTarget.transform, internalTarget.transform.up);
		}
		
	}
}