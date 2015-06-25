using UnityEngine;
using System.Collections;

public class PlanarCameraMode : BaseCameraMode {
	private GameObject internalTarget;

	private float distanceFromTarget = 250.0f;
	private float lateralClampDistance = 80.0f;
	private float interpTime = 5.0f;
	
	private float timeLeft;

	private Vector3 camForward;

	private Vector3 desiredLocation;
	
	public float maxCameraHeight = -10.0f;
	
	void Awake () {
		internalTarget = null;
	}

	public override void InitCameraMode() {
        if( inited ) {
            return;
        }
        
        base.InitCameraMode();        
        
		cameraType = CameraType.FollowPlanarCamera;
		myTransform = transform;
		camForward = myTransform.forward;
		
		cameraName = "Planar Camera";
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
			distanceFromTarget = constants.planar_Distance;
			lateralClampDistance = constants.planar_LateralClampDistance;
			interpTime = constants.planar_InterpolationTime;
	   	}

		Vector3 new_location = internalTarget.transform.position + ( camForward * -distanceFromTarget );
		
		if( !MathfExt.Approx( new_location, desiredLocation, 0.01f ) )
		{
			desiredLocation = new_location;
			timeLeft = interpTime;
		}
		
		if( timeLeft > 0.0f ) 
		{
			timeLeft -= dt;
			if( timeLeft < 0.0f ) timeLeft = 0.0f;
			
			Vector3 total_move = desiredLocation - myTransform.position;
			float sqr_dist = total_move.sqrMagnitude;
			if( sqr_dist > lateralClampDistance * lateralClampDistance ) 
			{
				total_move *= ( lateralClampDistance / Mathf.Sqrt(sqr_dist) );
				myTransform.position = desiredLocation - total_move;
			}
			else
			{
				float ratio = 1.0f - ( timeLeft / interpTime );
			
				total_move *= ratio;
				myTransform.position += total_move;
			} 
			
			Vector3 center = CameraManager.GetFollowCamSphereCenter();
			Vector3 diff = myTransform.position - center;
			float d = CameraManager.GetFollowCamSphereDistance();
			if( diff.magnitude > d ) {
				diff *= ( d / diff.magnitude);
				myTransform.position = center + diff;
			}

		}
	}
}