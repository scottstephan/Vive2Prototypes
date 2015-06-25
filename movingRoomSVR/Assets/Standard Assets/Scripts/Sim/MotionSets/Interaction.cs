using UnityEngine;
using System.Collections;

[System.Serializable]
public class Interaction : BehaviorBase {

    
    // Use this for initialization	
	public override void OneTimeStart( CritterInfo critter_info ) 
	{
		InteractionData idb = critter_info.interactionData;
		if( idb != null ) {
		 	idb.delayToRotate = idb.touchDelayToRotate;		
			idb.swimSpeed = idb.swimSpeedMult * critter_info.generalMotionData.swimSpeed;
		}
	}        
		
	public override void Start(CritterInfo critter_info)
	{
        base.Start(critter_info);
		GeneralMotionData gmd = critter_info.generalMotionData;
		gmd.lockVelocityToHeading = true;
		gmd.desiredVelocityDirection = critter_info.critterTransform.forward;
		gmd.desiredVelocityDirection.y = 0f;
		gmd.desiredSpeed = 0f;

		InteractionData idb = critter_info.interactionData;
	 	idb.delayToRotate = idb.touchDelayToRotate;
		idb.startledTapIndex = InputManager.uniqueTapIndex;
		critter_info.critterSteering.desiredSteeringThrottle = idb.steeringThrottle;
		
		idb.happynessHoldTimer = idb.happynessHoldTime;
		
		ApplyThrottleOverrides( critter_info, idb );
	}
	
	public override void End(CritterInfo critter_info)
	{
		RestoreThrottleOverrides( critter_info, critter_info.interactionData );
	}
	
	void ApplyThrottleOverrides( CritterInfo critter_info, InteractionData idb ) {
		ThrottledFishSteering s = (ThrottledFishSteering)critter_info.critterSteering;
		
		// save the current steering values
		idb.savedThrottleMaxSpeed = s.throttleMaxSpeed;
		idb.savedThrottleSpeedAccel = s.throttleSpeedAccel;
		idb.savedThrottleSpeedDecel = s.throttleSpeedDecel;
		idb.savedYawAccel = s.yawAccel;
		idb.savedYawDecel = s.yawDecel;
		idb.savedYawMaxSpeed = s.yawMaxSpeed;	
		idb.savedPitchAccel = s.pitchAccel;
		idb.savedPitchDecel = s.pitchDecel;
		idb.savedPitchMaxSpeed = s.pitchMaxSpeed;
		idb.savedRollAccel = s.rollAccel;
		idb.savedRollDecel = s.rollDecel;
		idb.savedRollMaxSpeed = s.rollMaxSpeed;
		idb.savedRollOnYawMult = s.rollOnYawMult;
		idb.savedRollStrafingMult = s.rollStrafingMult;	
	
		// apply the overrides
		s.throttleMaxSpeed = idb.throttleMaxSpeed;
		s.throttleSpeedAccel = idb.throttleSpeedAccel;
		s.throttleSpeedDecel = idb.throttleSpeedDecel;
		s.yawAccel = idb.yawAccel;
		s.yawDecel = idb.yawDecel;
		s.yawMaxSpeed = idb.yawMaxSpeed;
		s.pitchAccel = idb.pitchAccel;
		s.pitchDecel = idb.pitchDecel;
		s.pitchMaxSpeed = idb.pitchMaxSpeed;
		s.rollAccel = idb.rollAccel;
		s.rollDecel = idb.rollDecel;
		s.rollMaxSpeed = idb.rollMaxSpeed;
		s.rollOnYawMult = idb.rollOnYawMult;
		s.rollStrafingMult = idb.rollStrafingMult;
	}
	
	void RestoreThrottleOverrides( CritterInfo critter_info, InteractionData idb ) {
		ThrottledFishSteering s = (ThrottledFishSteering)critter_info.critterSteering;
		
		s.throttleMaxSpeed = idb.savedThrottleMaxSpeed;
		s.throttleSpeedAccel = idb.savedThrottleSpeedAccel;
		s.throttleSpeedDecel = idb.savedThrottleSpeedDecel;
		s.yawAccel = idb.savedYawAccel;
		s.yawDecel = idb.savedYawDecel;
		s.yawMaxSpeed = idb.savedYawMaxSpeed;
		s.pitchAccel = idb.savedPitchAccel;
		s.pitchDecel = idb.savedPitchDecel;
		s.pitchMaxSpeed = idb.savedPitchMaxSpeed;
		s.rollAccel = idb.savedRollAccel;
		s.rollDecel = idb.savedRollDecel;
		s.rollMaxSpeed = idb.savedRollMaxSpeed;
		s.rollOnYawMult = idb.savedRollOnYawMult;
		s.rollStrafingMult = idb.savedRollStrafingMult;
	}

	// Update is called once per frame
	public override void Update (CritterInfo critter_info) 
	{
		float dt = Time.deltaTime;
        base.Update( critter_info );
		
		InteractionData idb = critter_info.interactionData;

		GeneralMotionData gmd = critter_info.generalMotionData;
		Transform ct = CameraManager.GetCurrentCameraTransform();
		if( Input.touchCount > 0 ) { 
			Touch t = Input.GetTouch( 0 );

			idb.delayToRotate = idb.touchDelayToRotate;
			
			if( t.phase == TouchPhase.Stationary ) {
				Camera currentCam = CameraManager.GetCurrentCamera();
		
				//Caluculate the Ray from mousePosition into the Screen
				Ray tempRay = currentCam.ScreenPointToRay( t.position );
				
				Vector3 local_dir = ct.worldToLocalMatrix * tempRay.direction;
				
				Vector3 move_pos = ct.position;
				move_pos += ct.right * local_dir.x * 120.0f;
				move_pos += ct.up * local_dir.y * 120.0f;
				move_pos += ct.forward * gmd.critterBoxColliderRadius;
//				Debug.Log(local_dir);
//				float side = Vector3.Dot( tempRay.direction, ct.right );
				gmd.desiredVelocityDirection = move_pos - critter_info.critterTransform.position;
				gmd.desiredSpeed = idb.swimSpeed;
			}
		}
		else {
			idb.delayToRotate -= dt;
			idb.happynessHoldTimer = idb.happynessHoldTime;
		}

		if( idb.delayToRotate < 0f ) {
			Vector3 forward = ct.forward * -1f;
			forward.y *= 0.75f;
			gmd.desiredVelocityDirection = forward;
			gmd.desiredSpeed = 0f;
		}
	}
	
	public override float EvaluatePriority(CritterInfo critter_info)
	{
		InteractionData idb = critter_info.interactionData;
		if( idb != null )
		{
/*			if( CameraManager.GetActiveCameraType() == CameraType.AnalyzeCamera 
				&& CameraManager.GetCurrentTarget() == critter_info.critterObject ) {
				return idb.priorityValue;
			}*/
			return 0f;
		}
		else return 0f;
	}
}


