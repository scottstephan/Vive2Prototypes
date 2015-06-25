using UnityEngine;
using System.Collections;

[System.Serializable]
public class ViewportMotion : BehaviorBase {
	public static bool forceAllBackToSim = false;
	
	public static void ForceAllBackToSim() {
		forceAllBackToSim = true;
//		SimInstance.Instance.UnpauseCrittersInAnimLock();
//		Debug.Log("ViewportMotion - ForceAllBackToSim - UNPAUSING ALL SIM");
	}
	
	public static void DelayedForceBackToSim( CritterInfo critter, float delay ) {
//		Debug.Log("ViewportMotion - DelayedForceBackToSim");
		ViewportMotionData vmd = critter.viewportMotionData;
		vmd.delayForceTime = delay;
	}
	
	public static void SetDesiredViewpoint( CritterInfo critter, Vector3 viewpoint, Vector3 forward, float time, GenericDelegate done_func ) {
//		Debug.Log("SETTING DESIRED VIEWPORT :: " + critter.critterObject.name + " :: " + viewpoint + " :: " + forward);
		ViewportMotionData vmd = critter.viewportMotionData;
		vmd.desiredViewportPosition = viewpoint;
		vmd.desiredEndRotation = Quaternion.LookRotation(forward);
		vmd.moveTime = time;
		vmd.moveTimeTotal = time;
		vmd.moveOGPosition = critter.critterTransform.position;
		vmd.moveOGRotation = critter.critterTransform.rotation;		
		vmd.MoveComplete = done_func;
	}
	
	public static void DoForwardMove( CritterInfo critter, float move_time, float move_speed, float stall_time, bool do_rotation, GenericDelegate done_func ) {
		ViewportMotionData vmd = critter.viewportMotionData;
		vmd.regularMoveDoRotation = do_rotation;
		vmd.regularMoveDir = critter.critterTransform.forward;
		vmd.regularMoveTime = move_time;
		vmd.regularMoveSpeed = move_speed;
		vmd.regularMoveDecay = 0f;
		vmd.stalledTime = stall_time;
		vmd.MoveComplete = done_func;
	}
	
	public static void DoSlideMove( CritterInfo critter, Vector3 dir, float move_time, float move_speed, float move_decay, float stall_time, GenericDelegate done_func ) {
		ViewportMotionData vmd = critter.viewportMotionData;
		vmd.regularMoveDir = dir;
		vmd.regularMoveDoRotation = true;
		vmd.regularMoveTime = move_time;
		vmd.regularMoveSpeed = move_speed;
		vmd.regularMoveDecay = move_decay;
		vmd.stalledTime = stall_time;
		vmd.MoveComplete = done_func;
	}
	
	public static void CancelMove( CritterInfo critter ) {
		ViewportMotionData vmd = critter.viewportMotionData;		
		vmd.moveTime = -1f;
		vmd.regularMoveTime = -1f;
		if( vmd.MoveComplete != null ) {
			vmd.MoveComplete();
			vmd.MoveComplete = null;
		}		
	}
	
	public static void StallForAnim( CritterInfo critter ) {
		ViewportMotionData vmd = critter.viewportMotionData;
		vmd.stalledForAnim = true;
	}
	
    // Use this for initialization	
	public override void OneTimeStart( CritterInfo critter_info ) 
	{
		ViewportMotionData vmd = critter_info.viewportMotionData;
		if( vmd == null ) {
			return;
		}
//		vmd.desiredViewportPosition = new Vector3( 0.5f, 0.5f, critter_info.cameraConstants.ropeAndStick_desiredDistance );
	}        
		
	public override void Start(CritterInfo critter_info)
	{
//		Debug.Log ("STARTING GESTURE FLOW MOTION!!!" + critter_info.critterTransform.position + " " + critter_info.critterTransform.rotation);
		
		// when a new critter gets put into this mode.. turn off the force into sim boolean.
		forceAllBackToSim = false;
		
        base.Start(critter_info);
		ViewportMotionData vmd = critter_info.viewportMotionData;
				
		vmd.currentViewportPosition = vmd.desiredViewportPosition;
		vmd.currentViewportPosition.z = 0f;
		vmd.moveTime = -1f;
		vmd.stalledForAnim = false;
		vmd.delayForceTime = -1f;
		vmd.regularMoveTime = -1f;
		vmd.MoveComplete = null;
		
		GeneralMotionData gmd = critter_info.generalMotionData;
		gmd.useVelocityDirection = true;
		gmd.lockVelocityToHeading = false;
	}
	
	public override void End(CritterInfo critter_info)
	{
		GeneralMotionData gmd = critter_info.generalMotionData;
		gmd.useVelocityDirection = false;
		gmd.lockVelocityToHeading = true;
	}
	
	// Update is called once per frame
	public override void Update(CritterInfo critter_info) 
	{
		ViewportMotionData vmd = critter_info.viewportMotionData;
		base.Update( critter_info );
		
		bool force_back = false;
		
		if( vmd.delayForceTime > 0f ) {
			vmd.delayForceTime -= Time.deltaTime;
			if( vmd.delayForceTime <= 0f ) {
				force_back = true;
			}
		}
		
		if( forceAllBackToSim || force_back ) {
			AI.ForceSwitchToBehavior( critter_info, SwimBehaviorType.SWIM_FREE );
		}

		if( vmd.stalledForAnim && !critter_info.animBase.playingOneOff ) {
//			Debug.Log("VIEWPORT STALLING FOR ANIM");
			vmd.stalledForAnim = false;
		}
	}
	
	public override void UpdateMotion( CritterInfo critter_info ) {
		ViewportMotionData vmd = critter_info.viewportMotionData;

		float dt = Time.deltaTime;
		
		if( vmd.stalledForAnim || vmd.stalledTime > 0f ) { 
			vmd.stalledTime -= dt;
			return;
		}
		
		if( vmd.moveTime >= 0f ) {
			//Debug.Log("MOVING!");
			vmd.moveTime -= dt;
			if( vmd.moveTime < 0f ) {
				vmd.moveTime = 0f;
			}
			float ratio = vmd.moveTime / vmd.moveTimeTotal;
			
			if( vmd.moveTime <= 0f ) {
				if( vmd.MoveComplete != null ) {
					vmd.MoveComplete();
					vmd.MoveComplete = null;
				}
				vmd.moveTime = -1f;
			}
			
			Camera cam = CameraManager.GetCurrentCamera();
			Vector3 cur = vmd.desiredViewportPosition;
			Transform ct = CameraManager.GetCurrentCameraTransform();
			cur.z = 0f;
			Ray new_ray = cam.ViewportPointToRay( cur );
			Vector3 new_position = new_ray.origin + new_ray.direction * vmd.desiredViewportPosition.z;
			Vector3 relative_pos = ct.worldToLocalMatrix * new_position;
			if( vmd.desiredViewportPosition.x < 0f ) {
				float x = -vmd.desiredViewportPosition.x;
				relative_pos.x -= ( x * relative_pos.x );
			}
			if( vmd.desiredViewportPosition.x > 1f ) {
				float x = vmd.desiredViewportPosition.x - 1f;
				relative_pos.x += ( x * relative_pos.x );
			}
			if( vmd.desiredViewportPosition.y < 0f ) {
				float y = -vmd.desiredViewportPosition.y;
				relative_pos.y -= ( y * relative_pos.y );
			}
			if( vmd.desiredViewportPosition.y > 1f ) {
				float y = vmd.desiredViewportPosition.y - 1f;
				relative_pos.y += ( y * relative_pos.y );
			}
			new_position = ct.localToWorldMatrix * relative_pos;
			critter_info.critterTransform.position = Vector3.Lerp( vmd.moveOGPosition, new_position, 1f - ratio );      
			vmd.currentViewportPosition = cam.WorldToViewportPoint( critter_info.critterTransform.position );
				
			critter_info.critterTransform.rotation = Quaternion.Lerp( vmd.moveOGRotation, vmd.desiredEndRotation, 1.0f - ratio );

			GeneralMotionData gmd = critter_info.generalMotionData;
			gmd.desiredVelocityDirection = critter_info.critterTransform.forward;
			gmd.desiredRotation = critter_info.critterTransform.rotation;
		}
		else if ( vmd.regularMoveTime > 0f ) {
			GeneralMotionData gmd = critter_info.generalMotionData;
			gmd.desiredVelocityDirection = vmd.regularMoveDir;
			gmd.desiredRotation = critter_info.critterTransform.rotation;
			if( vmd.regularMoveDecay > 0f ) {
				vmd.regularMoveSpeed -= ( vmd.regularMoveSpeed * vmd.regularMoveDecay * dt );
				if( vmd.regularMoveSpeed < 0.05f ) {
					vmd.regularMoveSpeed = 0f;
				}
			}
			
			gmd.desiredSpeed = vmd.regularMoveSpeed;
			gmd.currentSpeed = vmd.regularMoveSpeed;
			
			GeneralMotion.UpdateMotion( critter_info, vmd.regularMoveDoRotation );
			
			vmd.regularMoveTime -= dt;
			
			if( vmd.regularMoveTime <= 0f && vmd.MoveComplete != null ) {
				vmd.MoveComplete();
				vmd.MoveComplete = null;
			}
		}
	}
}


