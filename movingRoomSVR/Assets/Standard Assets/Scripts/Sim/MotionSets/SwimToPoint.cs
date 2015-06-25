using UnityEngine;
using System.Collections;

[System.Serializable]
public class SwimToPoint : BehaviorBase {
	// Use this for initialization
		
	public override void OneTimeStart( CritterInfo critter_info ) 
	{
		SwimToPointData sd = critter_info.swimToPointData;
				
		sd.swimSpeed = sd.swimSpeedMult * critter_info.generalMotionData.swimSpeed;
		sd.smoothRotate = sd.smoothRotateMult * critter_info.generalMotionData.smoothRotate;
		sd.cameraFadeStarted = false;
	}
	
/*	public void ExitEffectDone( CritterInfo critter_info )
	{
		critter_info.markedForRemove = true;
	}
	
	public void EnterEffectDone( CritterInfo critter_info )
	{
		EnterSwimTargetedMode(critter_info);
	}*/
	
	public override void Start(CritterInfo critter_info)
	{
		SwimToPointData sd = critter_info.swimToPointData;
		GeneralMotionData gmd = critter_info.generalMotionData;
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		gmd.lockVelocityToHeading = true;
		gsd.swimToPoint = false;
		sd.exitDirSet = false;
		if( gsd.isExitingScene ) {
			sd.pointReachedType = PointReachedType.ExitScene;	
			sd.staticCameraSwapTimer = 1f;
		}
	}
	
	public static void LeaveSceneCameraFadeDone(object arg) {
        if( arg != null ) {
            CritterInfo critter = (CritterInfo)arg;
            critter.markedForRemove = true;
        }
        
		SimManager.GetRandomCameraTarget( false );
		CameraManager.JumpToCameraOrder(0);
        OculusCameraFadeManager.StartCameraFadeFromBlack(1.5f,null,null);
	}
	
	public static void EnterSwimTargetedMode( CritterInfo critter_info ) 
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;	
		GeneralMotion.SetRandomTarget(critter_info);
		gsd.switchBehavior = true;
		gsd.hungerLevel = Random.value*0.99f;
		gsd.searchNewLeaderCounter = Mathf.FloorToInt(Random.value*60);
	}
	
	// Update is called once per frame
	public override void Update( CritterInfo critter_info ) 
	{
		SwimToPointData sd = critter_info.swimToPointData;
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		GeneralMotionData gmd = critter_info.generalMotionData;

		// TODO> swim to point only has a single throttle control.
		critter_info.critterSteering.desiredSteeringThrottle = sd.desiredSteeringThrottle;
		bool cam_is_following = CameraManager.GetActiveCameraType() == CameraType.OculusFollowCamera || 
                                OculusCameraFadeManager.IsFading();

		
		float dt = Time.deltaTime;
		sd.blockCameraSwitchTimer -= dt;
		sd.staticCameraSwapTimer -= dt;

		// we are never hungry or searching.
		gsd.searchNewLeaderCounter = 0;
		gsd.hungerLevel = 0;
		
		float speedMult = 1.0f;
		Vector3 diff = new Vector3(0f,0f,0f);
		float diff_dist = 0.0f;
		
		diff = sd.targetPosition - critter_info.critterTransform.position;
		switch(sd.pointReachedType) {
			case PointReachedType.EnterTargetedMotion:{
				gmd.desiredVelocityDirection = diff;
				//WemoLog.Eyal(sd.targetPosition);
#if UNITY_EDITOR
				Debug.DrawLine(sd.targetPosition, sd.targetPosition + Vector3.up * 180f, Color.white);
#endif		
				diff_dist = diff.magnitude;
				// attempt to make a solution that works for clownfish and shark 
				//float distMult = MathfExt.Fit(gmd.critterBoxColliderRadius,10f,100f,5f,2.5f);
				float distMult = 5f - MathfExt.Fit(gmd.critterBoxColliderRadius,10f,100f,0f,2.5f);
				float dot = Vector3.Dot(diff,sd.savedTargetDirection);
				if( gmd.collidedWithGroundLastFrame // bail if entering the scene and we hit the ground.
					|| ( diff_dist < gmd.critterBoxColliderRadius * distMult )
					|| dot < 0f )
				{
					EnterSwimTargetedMode(critter_info);
				}
				break;	
			}
			case PointReachedType.ExitScene: {
				if( !cam_is_following 
                   && !critter_info.critterLODData.curLOD.LODrenderer.isVisible 
                   && (critter_info.critterTransform.position-CameraManager.GetCurrentCameraPosition()).magnitude >= critter_info.markedForRemoveCameraDist 
                   && (CameraManager.singleton.fxFrame == null || CameraManager.singleton.fxFrame.currentTarget != critter_info.critterItemData)) {
					critter_info.markedForRemove = true;
				}
				if( !sd.exitDirSet ) {
					gmd.desiredVelocityDirection = diff;
					sd.exitDirSet = true;
				}
				break;	
			}
		}
		
		gmd.desiredSpeed = speedMult * sd.swimSpeed;
		if( sd.staticCameraSwap ) {
			gmd.desiredSpeed *= 10f;
		}
	}

	public override float EvaluatePriority(CritterInfo critter_info)
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		SwimToPointData sd = critter_info.swimToPointData;
		if(sd)
		{
            if(gsd.isExitingScene)
            {
                sd.currentPriorityValue = 1000f; // beat out script behaviors
            }
            else if (gsd.swimToPoint) 
            {
                sd.currentPriorityValue = sd.priorityValue;
            }
			else
            {
                sd.currentPriorityValue = 0f;
            }
			return sd.currentPriorityValue;
		}
		else return 0f;
	}
}


