using UnityEngine;
using System.Collections;

[System.Serializable]
public class SwimSchoolFollow : BehaviorBase {

	public override void OneTimeStart( CritterInfo critter_info ) 
	{
		SwimSchoolFollowData sd = critter_info.swimSchoolFollowData;
		sd.posOffset = new Vector3((Random.value-0.5f)*sd.followRadius,(Random.value-0.5f)*sd.followRadius,(Random.value-0.5f)*sd.followRadius);
		sd.randOffsetSpeed =  new Vector3((Random.value-0.5f)*2.0f,(Random.value-0.5f)*2.0f,(Random.value-0.5f)*2.0f) * sd.randOffsetMult;
		sd.posLast = critter_info.critterTransform.position;

		sd.followRadius = sd.followRadiusMult * critter_info.generalMotionData.followRadius;
		sd.swimSpeed = sd.swimSpeedMult * critter_info.generalMotionData.swimSpeed;
		sd.smoothRotate = sd.smoothRotateMult * critter_info.generalMotionData.smoothRotate;
		sd.sinMotionFreqMult = sd.sinMotionFreqMult * critter_info.generalMotionData.sinMotionFreq;
		sd.leaderTransform = null;
		sd.leaderCritterInfo = null;
        sd.state = SwimSchoolFollowData.SchoolState.Automatic;
	}

    bool ValidLeaderBehavior(SwimSchoolFollowData sd)
    {
        if (sd.state == SwimSchoolFollowData.SchoolState.Manual)
        {
            return true;
        }

        return sd.leaderCritterInfo.generalSpeciesData.myCurrentBehaviorType == SwimBehaviorType.SWIM_FREE;
    }

    bool PreventSchoolOutsideFishBowl(SwimSchoolFollowData sd)
    {
        if (sd.state == SwimSchoolFollowData.SchoolState.Manual)
        {
            return false;
        }

        // BV: fish look unnatural if they stop schooling, hopefully they leader will return to the fishbowl
        //return sd.leaderCritterInfo.swimFreeData.outside;
        return false;
    }

	public override void Start(CritterInfo critter_info)
	{
		GeneralMotionData gmd = critter_info.generalMotionData;
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		gmd.lockVelocityToHeading = true;
		//gsd.isSchooling = true;
		gsd.startSchooling = false;

        SwimSchoolFollowData sd = critter_info.swimSchoolFollowData;
        if (sd.scriptFollowRadius > 0f)
        {
            sd.oldFollowRadius = sd.followRadius;
            sd.followRadius = sd.scriptFollowRadius;
        }
    }

    public override void End (CritterInfo critter_info)
    {
        SwimSchoolFollowData sd = critter_info.swimSchoolFollowData;
        if (sd.scriptFollowRadius > 0f)
        {
            sd.followRadius = sd.oldFollowRadius;
        }
    }
    
	// Update is called once per frame
	public override void Update (CritterInfo critter_info) 
	{
		SwimSchoolFollowData sd = critter_info.swimSchoolFollowData;
		GeneralMotionData gmd = critter_info.generalMotionData;

		if( sd.leaderTransform == null 
		   || ( sd.leaderCritterInfo != null
			   && !sd.leaderCritterInfo.isPlayer
               && sd.leaderCritterInfo.generalSpeciesData != null
		       && ( sd.leaderCritterInfo.generalSpeciesData.isExitingScene 
                    || !ValidLeaderBehavior(sd) 
                    || PreventSchoolOutsideFishBowl(sd) ) ) )
        {
			GeneralSpeciesData gsd = critter_info.generalSpeciesData;
			gsd.switchBehavior = true;
			gsd.searchNewLeaderCounter = 60; // UCK. stupid hard coded value.
			return;
		}
		
		// if we are avoiding the ground. dont stomp the velocity direction.
		if( sd.state == SwimSchoolFollowData.SchoolState.Manual ||
           !gmd.avoidGround ) 
        {				
			// doing this fiddly time calc so that the fish do not jump after being paused.
			float use_time = critter_info.lifetime;
			float followRadiusMult = 0.7f+0.4f*Mathf.Cos(use_time*sd.randOffsetSpeed.x*0.1f + sd.randOffsetSpeed.y*52.4f);
			sd.posOffset = new Vector3(sd.followRadius*followRadiusMult* Mathf.Cos(use_time*sd.randOffsetSpeed.x*sd.sinMotionFreqMult),sd.offsetMultY*sd.followRadius*followRadiusMult* Mathf.Cos(use_time*sd.randOffsetSpeed.y*sd.sinMotionFreqMult),sd.followRadius*followRadiusMult* Mathf.Cos(use_time*sd.randOffsetSpeed.z*sd.sinMotionFreqMult));

            Vector3 targetPos = sd.posOffset + sd.leaderTransform.localToWorldMatrix.MultiplyPoint(sd.leaderOffset);
			
			//if outside of fishbowl, clamp
			FishBowlData bd = critter_info.generalSpeciesData.fishBowlData;
			if(bd != null && 
               sd.state != SwimSchoolFollowData.SchoolState.Manual &&
               !sd.leaderCritterInfo.followIgnoreFishBowl)
			{
				targetPos.x = Mathf.Clamp(targetPos.x, bd.position.x - bd.halfSize.x, bd.position.x + bd.halfSize.x);
				targetPos.y = Mathf.Clamp(targetPos.y, bd.position.y - bd.halfSize.y, bd.position.y + bd.halfSize.y);
				targetPos.z = Mathf.Clamp(targetPos.z, bd.position.z - bd.halfSize.z, bd.position.z + bd.halfSize.z);
				//WemoLog.Eyal("clamp schoool");
			}
			
			Vector3 dir  = targetPos - critter_info.cachedPosition;
			float mag = dir.magnitude;
			float speedMult = MathfExt.Fit(mag,gmd.critterBoxColliderSize.z * 0.2f,gmd.critterBoxColliderSize.z * 5f,0.1f,1.6f);
			dir = dir/mag; // normalized
			dir.y *= speedMult; // reduce up/down movement
			gmd.desiredVelocityDirection = dir;
			gmd.desiredSpeed = speedMult * sd.swimSpeed;
		}
		else
        {
			gmd.desiredSpeed = sd.swimSpeed;
		}
		
		critter_info.critterSteering.desiredSteeringThrottle = sd.desiredSteeringThrottle;

#if UNITY_EDITOR
		Debug.DrawLine(critter_info.cachedPosition,sd.leaderCritterInfo.cachedPosition,Color.yellow);
		//Debug.DrawLine(critter_info.critterTransform.position,targetPos,Color.magenta)
#endif
			
	}
	
	public override float EvaluatePriority(CritterInfo critter_info)
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		SwimSchoolFollowData sd = critter_info.swimSchoolFollowData;

		if(sd != null 
           && sd.state != SwimSchoolFollowData.SchoolState.None)
		{
            if (sd.state == SwimSchoolFollowData.SchoolState.Manual &&
                critter_info.swimTargetedData != null)
            {
                // slight boost for manual follow over eating
                sd.currentPriorityValue = Mathf.Max(critter_info.swimTargetedData.priorityValue+1, sd.priorityValue);
            }
			else if(gsd.startSchooling)
            {
                sd.currentPriorityValue = sd.priorityValue;
            }
			else
            {
                sd.currentPriorityValue = 0f;
            }

			return sd.currentPriorityValue;
		}

		return 0f;
	}
}



