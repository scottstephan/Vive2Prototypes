using UnityEngine;
using System.Collections;

[System.Serializable]
public class CircleAroundObject : BehaviorBase {    
	
	// this behavior is self contained.
	public override bool IsSingletonBehavior() { return true; }

	public override void OneTimeStart( CritterInfo critter_info ) 
	{
		CircleAroundObjectData cao = critter_info.circleAroundObjectData;
		
		if( cao == null ) {
			return;
		}		
		
		cao.CircleTimeExpired = null;
	}        
	
	public override void Start(CritterInfo critter_info)
	{
        base.Start(critter_info);

		CircleAroundObjectData cao = critter_info.circleAroundObjectData;
		
		if( cao == null ) {
			return;
		}
		if( cao.targetTransform == null ) {
			cao.targetTransform = CameraManager.GetCurrentCameraTransform();
		}

		if( cao.targetTransform == null ) {
			cao.prevTargetPosition = Vector3.zero;
		}
		else {
			cao.prevTargetPosition = cao.targetTransform.position;
		}

		critter_info.critterSteering.desiredSteeringThrottle = cao.steeringThrottle;

		ApplyMotionOverrides( critter_info, cao );
		
		//figre out our circle direction.
		Vector3 to_object = cao.prevTargetPosition - critter_info.critterTransform.position;
//        float y_offset = to_object.y;
		to_object.y = 0f;
		Vector3 fwd = critter_info.critterTransform.forward;
		fwd.y = 0f;		
		Vector3 cross = Vector3.Cross( fwd, to_object );
		cao.circleDir = cross.y > 0f ? -1f : 1f;
		
		critter_info.generalMotionData.lockVelocityToHeading = true;
	}
	
	public override void End(CritterInfo critter_info)
	{
		RestoreMotionOverrides( critter_info, critter_info.circleAroundObjectData );

/*		if( critter_info.critterFx != null ) {
			critter_info.critterFx.StopPixieTrail();		
		}*/
	}
	
	void ApplyMotionOverrides( CritterInfo critter_info, CircleAroundObjectData cao ) {
		ThrottledFishSteering s = (ThrottledFishSteering)critter_info.critterSteering;
		

		if( cao.useSteeringOverrides ) {
			// save the current steering values
			cao.savedThrottleMaxSpeed = s.throttleMaxSpeed;
			cao.savedThrottleSpeedAccel = s.throttleSpeedAccel;
			cao.savedThrottleSpeedDecel = s.throttleSpeedDecel;
			cao.savedYawAccel = s.yawAccel;
			cao.savedYawDecel = s.yawDecel;
			cao.savedYawMaxSpeed = s.yawMaxSpeed;	
			cao.savedPitchAccel = s.pitchAccel;
			cao.savedPitchDecel = s.pitchDecel;
			cao.savedPitchMaxSpeed = s.pitchMaxSpeed;
			cao.savedRollAccel = s.rollAccel;
			cao.savedRollDecel = s.rollDecel;
			cao.savedRollMaxSpeed = s.rollMaxSpeed;
			cao.savedRollOnYawMult = s.rollOnYawMult;
			cao.savedRollStrafingMult = s.rollStrafingMult;	

			// apply the overrides
			s.throttleMaxSpeed = cao.throttleMaxSpeed;
			s.throttleSpeedAccel = cao.throttleSpeedAccel;
			s.throttleSpeedDecel = cao.throttleSpeedDecel;
			s.yawAccel = cao.yawAccel;
			s.yawDecel = cao.yawDecel;
			s.yawMaxSpeed = cao.yawMaxSpeed;
			s.pitchAccel = cao.pitchAccel;
			s.pitchDecel = cao.pitchDecel;
			s.pitchMaxSpeed = cao.pitchMaxSpeed;
			s.rollAccel = cao.rollAccel;
			s.rollDecel = cao.rollDecel;
			s.rollMaxSpeed = cao.rollMaxSpeed;
			s.rollOnYawMult = cao.rollOnYawMult;
			s.rollStrafingMult = cao.rollStrafingMult;
		}
		
		cao.savedMaxAccel = critter_info.generalMotionData.maxAcc;
		critter_info.generalMotionData.maxAcc = cao.swimAccelOverride;
	}
	
	void RestoreMotionOverrides( CritterInfo critter_info, CircleAroundObjectData cao ) {
		ThrottledFishSteering s = (ThrottledFishSteering)critter_info.critterSteering;
		
		if( cao.useSteeringOverrides ) {
			s.throttleMaxSpeed = cao.savedThrottleMaxSpeed;
			s.throttleSpeedAccel = cao.savedThrottleSpeedAccel;
			s.throttleSpeedDecel = cao.savedThrottleSpeedDecel;
			s.yawAccel = cao.savedYawAccel;
			s.yawDecel = cao.savedYawDecel;
			s.yawMaxSpeed = cao.savedYawMaxSpeed;
			s.pitchAccel = cao.savedPitchAccel;
			s.pitchDecel = cao.savedPitchDecel;
			s.pitchMaxSpeed = cao.savedPitchMaxSpeed;
			s.rollAccel = cao.savedRollAccel;
			s.rollDecel = cao.savedRollDecel;
			s.rollMaxSpeed = cao.savedRollMaxSpeed;
			s.rollOnYawMult = cao.savedRollOnYawMult;
			s.rollStrafingMult = cao.savedRollStrafingMult;
		}
		
		critter_info.generalMotionData.maxAcc = cao.savedMaxAccel;
	}

	
	// Update is called once per frame
	public override void Update (CritterInfo critter_info) 
	{
		float dt = Time.deltaTime;
        base.Update( critter_info );
				
		CircleAroundObjectData cao = critter_info.circleAroundObjectData;

		Vector3 p_pos = ( cao.targetTransform != null ) ? cao.targetTransform.position : Vector3.zero;
		Vector3 my_pos = critter_info.critterTransform.position;
		Vector3 cur = my_pos - p_pos;

        float y_offset = cur.y;

        if( cao.adjustsToHeight ) {
            if( Mathf.Abs( y_offset ) < cao.allowedHeightDifference ) {
                cur.y = 0f;
            }
            else {
                cur.y *= -1f;
            }
        }

		// rescale to our desired position.
		cur *= ( cao.circleRadius / cur.magnitude );
		
		GeneralMotionData gmd = critter_info.generalMotionData;
		
		// based on our current speed and position, get our desired position radially.
		float rot_ang = 20f * Mathf.Deg2Rad;
		Vector3 dpos = MathfExt.YawVector( cur, cao.circleDir * rot_ang );
		dpos += p_pos;

#if UNITY_EDITOR
		Debug.DrawLine( critter_info.critterTransform.position, dpos, Color.green );
#endif

		gmd.desiredVelocityDirection = dpos - my_pos;
//		gmd.desiredVelocityDirection.y = 0f;

		// calculate target instantaneous object speed.
		Vector3 diff = cao.prevTargetPosition - p_pos;
		float dist = diff.magnitude;
		float spd = dist / dt;

		cao.prevTargetPosition = p_pos;

		gmd.desiredSpeed = cao.swimSpeed + spd;

		if( cao.circleTime > 0f ) {
			cao.circleTime -= dt;
			if( cao.circleTime <= 0f ) {
				if( cao.CircleTimeExpired != null ) {
					cao.CircleTimeExpired();
				}
			}
		}
	}
	
	public override float EvaluatePriority(CritterInfo critter_info)
	{
		return 0f;
	}
}


