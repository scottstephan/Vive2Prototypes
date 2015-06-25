using UnityEngine;
using System.Collections;

public class SwimFree : BehaviorBase {
	
    public static bool forceMe = false;

    const float LOOKAT_ANIM_DELAY = 10f;
    static float lastLookAtAnimTime = -LOOKAT_ANIM_DELAY;

	public override void OneTimeStart( CritterInfo critter_info ) 
	{
		SwimFreeData sd = critter_info.swimFreeData;
		
        if(sd != null) 
		{
			sd.swimSpeed = sd.swimSpeedMult * critter_info.generalMotionData.swimSpeed;

            if (sd.lookAtAnim != null &&
                critter_info.animBase != null)
            {
                critter_info.animBase.AddAnimation(sd.lookAtAnim);
            }
		}
	}
	
	public override void Start(CritterInfo critter_info)
	{
		GeneralMotionData gmd = critter_info.generalMotionData;
		SwimFreeData sd = critter_info.swimFreeData;
		//GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		gmd.lockVelocityToHeading = true;
		sd.desiredDirection = critter_info.cachedForward;
        sd.desiredDirection.y = 0f;
        sd.desiredDirection.Normalize();
		sd.headingTimer = Random.Range(sd.headingTimeMin,sd.headingTimeMax);

        if (critter_info.generalSpeciesData.fishBowlData != null)
        {
            sd.lastFishBowlPos = critter_info.generalSpeciesData.fishBowlData.position;
        }

        sd.lookAtTimer = 0f;

		//WemoLog.Eyal("Start SwimFree headingTimer " + sd.headingTimer);
	}
	
	public override void Update( CritterInfo critter_info ) 
	{
		GeneralMotionData gmd = critter_info.generalMotionData;
		SwimFreeData sd = critter_info.swimFreeData;
		if(!sd) 
        {
            return;
        }

        if (sd.lookAtAnim != null) 
        {
            if (critter_info.isVisible)
            {
                sd.lookAtTimer += Time.deltaTime;
            }
            else
            {
                sd.lookAtTimer = 0f;
            }

            if (sd.lookAtTimer > 5f &&
                (Time.time - lastLookAtAnimTime) > LOOKAT_ANIM_DELAY)
            {
                critter_info.animBase.PlayAnimation(sd.lookAtAnim, false, true);
                lastLookAtAnimTime = Time.time;
            }
        }

		//see if outside of fishbowl
		FishBowlData bd = critter_info.generalSpeciesData.fishBowlData;
		sd.outside = false;
		if(bd != null)
		{
			Vector3 pos = critter_info.cachedPosition;

            if (sd.lastFishBowlPos != bd.position)
            {
                sd.outside = false;
                sd.outsideBowlDirectionSet = false;
            }

			if( ( pos.x > bd.position.x + bd.halfSize.x )
				|| ( pos.x < bd.position.x - bd.halfSize.x ) ) 
			{
				sd.outside = true;
			}

			if( ( pos.y > bd.position.y + bd.halfSize.y )
				|| ( pos.y < bd.position.y - bd.halfSize.y) )
			{
				sd.outside = true;
			}

			if( ( pos.z > bd.position.z + bd.halfSize.z )
				|| ( pos.z < bd.position.z - bd.halfSize.z ) ) 
			{
				sd.outside = true;
			}

            sd.lastFishBowlPos = bd.position;
		}
			
        Vector3 fwd = critter_info.cachedForward;
        float cur_y = fwd.y;
        if( !gmd.avoidGround && !gmd.avoidFish && !gmd.collidedWithGroundLastFrame) 
        {
            sd.savedDirYValue = cur_y;
        }

        if( gmd.avoidGround || gmd.collidedWithGroundLastFrame) 
        {
            if( ( sd.savedDirYValue < 0f && cur_y > 0f ) || ( sd.savedDirYValue > 0f && cur_y < 0f )) 
            {
                sd.desiredDirection.y = sd.savedDirYValue;
            }

            sd.outsideBowlDirectionSet = false;
            sd.headingTimer = -1;
        }

		gmd.desiredVelocityDirection = sd.desiredDirection;
		gmd.desiredSpeed = sd.swimSpeed;
		
		sd.headingTimer -= Time.deltaTime;
		if( sd.outside )
        {
			if( !sd.outsideBowlDirectionSet ) 
            {
//				Debug.Log("reseting dir due to outside state change.");
				sd.headingTimer = -1;
			}
		}
		else 
        {
			sd.outsideBowlDirectionSet = false;
		}
		
		if( sd.headingTimer<=0f )
		{
			SwimFree.ChangeDirection(critter_info, bd);
		}
		
#if UNITY_EDITOR
        Vector3 p1 = critter_info.cachedPosition;
		Vector3 p2 = p1 + (sd.desiredDirection * 100f);
		//Debug.DrawLine(p1,p2,Color.red);
#endif
	}
	
	public static void ChangeDirection( CritterInfo critter_info, FishBowlData bd ) 
	{
		SwimFreeData sd = critter_info.swimFreeData;
		GeneralMotionData gmd = critter_info.generalMotionData;
		if( sd.outside ) 
		{
			Vector3 dir_vec = bd.position - critter_info.cachedPosition; 
			dir_vec.y *= 0.5f;
			sd.desiredDirection = dir_vec;
			sd.desiredDirection.Normalize();
			float t = Random.Range(sd.outsideBowlThrottleMin,sd.outsideBowlThrottleMax);
			critter_info.critterSteering.desiredSteeringThrottle = t;		
			
//			Debug.Log("new outside steering throttle :: " + t );
			sd.outsideBowlDirectionSet = true;
            // only play disperse outside bowl b/c critter will turn more drastically
            AudioManager.PlayCritterSwimAway(critter_info);

		}
		else
		{
			float pitch = RandomExt.FloatWithRawBiasPower(sd.dirChangeMinPitch,sd.dirChangeMaxPitch,sd.dirChangePitchBiasedValue,sd.dirChangePitchPower);
			float rnd = Random.value;
			if( rnd < 0.5f ) {
				pitch *= -1f;
			}

			float yaw = RandomExt.FloatWithRawBiasPower(sd.dirChangeMinPitch,sd.dirChangeMaxPitch,sd.dirChangePitchBiasedValue,sd.dirChangePitchPower);
			rnd = Random.value;
			if( rnd < 0.5f ) {
				yaw *= -1f;
			}
			
			float cur_yaw = critter_info.cachedEulers.y;
			cur_yaw += yaw;
			cur_yaw = MathfExt.WrapAngle(cur_yaw);

            sd.desiredDirection = MathfExt.BuildYawPitchUnitVec(cur_yaw * Mathf.Deg2Rad, pitch * Mathf.Deg2Rad);
			
			float t = RandomExt.FloatWithRawBiasPower(sd.steeringThrottleMin,sd.steeringThrottleMax,sd.steeringThrottleBiasedValue,sd.steeringThrottlePower);
//			Debug.Log("new dir steering throttle :: " + t );
			critter_info.critterSteering.desiredSteeringThrottle = t;
		}

		// reset our heading timer.
		sd.headingTimer = Random.Range(sd.headingTimeMin,sd.headingTimeMax);
		float dist = sd.swimSpeed * sd.headingTimer;
		RaycastHit hit;
		Vector3 headPos = critter_info.cachedPosition + critter_info.cachedForward * gmd.critterBoxColliderSize.z * 0.5f;
		if(Physics.SphereCast(headPos, gmd.critterBoxColliderRadius * 1.01f, sd.desiredDirection,out hit, dist , 1<<14)) 
		{
			Vector3 hitDiff = hit.point - headPos;
			float hitDist = hitDiff.magnitude;
            float changeDirCollisionTime = (hitDist - gmd.critterBoxColliderSize.z * 0.5f) / sd.swimSpeed;
            if (changeDirCollisionTime < sd.headingTimer)
            {
                sd.headingTimer = changeDirCollisionTime;
            }
			//WemoLog.Eyal("future collision timer is " + sd.headingTimer);
			//SwimFree.ChangeDirection(critter_info);
		}
/*		Vector3 endPt = headPos + sd.desiredDirection * sd.swimSpeed * sd.headingTimer;
		if(endPt.y >= -gmd.critterBoxColliderRadius)
		{
			sd.desiredDirection.y = 0f;
		}*/
		//WemoLog.Eyal("changeDIrection " + sd.desiredDirection);
	}

	public override float EvaluatePriority(CritterInfo critter_info)
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		SwimFreeData sd = critter_info.swimFreeData;
		if(sd)
		{
            if( forceMe ) {
                return 100.0f;
            }
			if(gsd.becameNotHungry && !gsd.eatsReef && !gsd.isStrafing) sd.currentPriorityValue = sd.priorityValue;
			else if(!gsd.isStrafing) sd.currentPriorityValue = sd.priorityValue * 0.25f; //10 instead of 40 for default swim
				else sd.currentPriorityValue = 0f;
			return sd.currentPriorityValue;
		}
		else return 0f;
	}
}