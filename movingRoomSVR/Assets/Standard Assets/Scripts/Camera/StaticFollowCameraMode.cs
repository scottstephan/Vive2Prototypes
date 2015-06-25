using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StaticFollowCameraMode : BaseCameraMode
{
	private GameObject internalTarget;
	private Transform internalTargetTransform;
    private CritterInfo internalCritter;
	
	Quaternion ogRot;
	float lerpTime = 1f;

	void Awake()
    {
        cameraType = CameraType.StaticFollowCamera;
        myTransform = transform;
        cameraName = "Static Camera";
    }

	public override bool GetFollowsTargets() 
	{ 
		return true; 
	}

    public override void StartCameraMode()
    {
		internalTarget = null;
		internalTargetTransform = null;
		myTransform.rotation = CameraManager.GetCurrentCameraRotation();
		ogRot = myTransform.rotation;
		myTransform.position = CameraManager.GetCurrentCameraPosition();
		lerpTime = 1f;
    }
	
    public override void UpdateCameraMode()
    {
		runCollision = CameraCollisionType.None;
        if (internalTarget != CameraManager.currentTarget)
        {
            internalTarget = CameraManager.currentTarget;
            internalTargetTransform = internalTarget.transform;
            internalCritter = SimManager.GetCritterForCameraTarget();
            if (internalCritter == null)
            {
//				Debug.Log("critter si null");
                CameraManager.JumpToCameraOrder(0);
                return;
            }
		}
		
		Vector3 look_at_target = internalTargetTransform.position;
		myTransform.LookAt(look_at_target);
		if( lerpTime > 0f ) {
			Quaternion desired = myTransform.rotation;
			myTransform.rotation = Quaternion.Slerp(ogRot,desired,1f - lerpTime);
			lerpTime -= Time.deltaTime;
			if( lerpTime < 0f ) {
				lerpTime = 0f;
			}
	
		}
	}
}
