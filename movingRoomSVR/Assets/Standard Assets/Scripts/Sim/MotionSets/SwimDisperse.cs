using UnityEngine;
using System.Collections;

[System.Serializable]
public class SwimDisperse : BehaviorBase {
	// Use this for initialization

	public override void OneTimeStart( CritterInfo critter_info ) 
	{
	}

	public override void Start(CritterInfo critter_info)
	{
		SwimDisperseData sd = critter_info.swimDisperseData;
		GeneralMotionData gmd = critter_info.generalMotionData;
		gmd.lockVelocityToHeading = true;
		sd.pointOrig = critter_info.cachedPosition;

        if (sd.extraSpeedMult > 1.00001f)
        {
            gmd.currentSpeed = sd.desiredSpeedMult * gmd.swimSpeed * sd.extraSpeedMult;
        }

        AudioManager.PlayCritterSwimAway(critter_info);
	}

    public override void End (CritterInfo critter_info)
    {
        GeneralMotionData gmd = critter_info.generalMotionData;
        gmd.myDisperseCritter = null;
        gmd.myDisperseXform = null;

        SwimDisperseData sd = critter_info.swimDisperseData;

        sd.extraSpeedMult = 1f;
        sd.extraColliderDistMult = 1f;
        sd.useBounds = false;
    }
    
    // Update is called once per frame
	public override void Update (CritterInfo critter_info) 
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		GeneralMotionData gmd = critter_info.generalMotionData;
		if( gmd.myDisperseXform == null )
        {
			gmd.isDispersed = false;
			gmd.isBeingChased = false;
			gsd.switchBehavior = true;
			return;
		}

		SwimDisperseData sd = critter_info.swimDisperseData;
        Vector3 critterPos = critter_info.cachedPosition;
        Vector3 disperseXFormPos = gmd.myDisperseCritter != null ? gmd.myDisperseCritter.cachedPosition : gmd.myDisperseXform.position;
		critter_info.critterSteering.desiredSteeringThrottle = sd.desiredSteeringThrottle;		

		// Failsafe::dont allow us to disperse more than 20 times our radius away form where we started.
        Vector3 diff = critterPos - (sd.checkMovingTarget ? disperseXFormPos : sd.pointOrig);
		float diffMag = diff.magnitude;
        float distThreshold = gmd.critterBoxColliderRadius * sd.thresholdColliderDistMult * sd.extraColliderDistMult;

        if (sd.useBounds)
        {
            distThreshold += Mathf.Max (sd.bounds.extents.x, Mathf.Max (sd.bounds.extents.y, sd.bounds.extents.z))* 0.5f;
        }

        if((!sd.useBounds || !sd.bounds.Contains(critterPos))
           && diffMag > distThreshold) 
        {
			gmd.isDispersed = false;
			gmd.isBeingChased = false;
			gsd.switchBehavior = true;
			return;
		}
		
        Vector3 disperseXFormFwd = gmd.myDisperseCritter != null ? gmd.myDisperseCritter.cachedForward : gmd.myDisperseXform.forward;
        if( !gmd.avoidGround )
        {
            gmd.desiredVelocityDirection = critterPos - disperseXFormPos;
            gmd.desiredVelocityDirection.y *= 0.45f;
			float mag = gmd.desiredVelocityDirection.magnitude;
			//WemoLog.Eyal("gmd " + critter_info.critterObject.name + " " + gmd.desiredVelocityDirection + " mag " + mag);
			Vector3 diffNorm = gmd.desiredVelocityDirection.normalized;

            // this speedmult used for chase somewhere???
            gmd.speedMult = 3f - 1f * MathfExt.Fit(mag,gmd.critterBoxColliderSize.z,gmd.disperseRadius*1.1f*gmd.predatorOffsetMult,0.0f,1.0f);		
            gmd.desiredVelocityDirection = Vector3.Cross(Vector3.Cross(disperseXFormFwd,diffNorm),disperseXFormFwd);
			if(gmd.desiredVelocityDirection.sqrMagnitude < 0.01f) 
            {
                gmd.desiredVelocityDirection = Vector3.Cross(disperseXFormFwd,Vector3.up);
            }
		}
        else if (gmd.collidedWithGroundLastFrame)
        {
            gmd.isDispersed = false;
            gmd.isBeingChased = false;
            gsd.switchBehavior = true;
        }

        gmd.desiredSpeed = sd.desiredSpeedMult * gmd.swimSpeed;

		gmd.avoidanceDelay = 0;
	    gmd.avoidanceGroundDelay = gmd.avoidanceGroundEveryNFrames-1;

		//gmd.desiredSpeed = gmd.speedMult * sd.swimSpeed;	
        GeneralMotionData predatorGMD = null;
        if( gmd.myDisperseCritter != null ) {
            predatorGMD = gmd.myDisperseCritter.generalMotionData;
        }

        float predatorHalfLength = 10f;
        if (predatorGMD != null)
        {
            gmd.desiredSpeed = Mathf.Max(predatorGMD.currentSpeed * sd.maxPredatorSpeedMult, gmd.desiredSpeed);
            predatorHalfLength = predatorGMD.critterBoxColliderSize.z * 0.5f;
        }
		
        gmd.desiredSpeed *= sd.extraSpeedMult;

		//WemoLog.Eyal("swimDisperse " + critter_info.critterObject.name + " " + gmd.desiredSpeed + " " + gmd.desiredVelocityDirection + " speedMult " + gmd.speedMult + " isChased " + gmd.isBeingChased);
		//Debug.DrawLine(critter_info.critterTransform.position, gmd.myDisperseXform.position + Vector3.up,Color.white); 
		
		
		//collision failsafe
		//GeneralMotionData predatorGMD = gmd.myDisperseXform.GetComponent<GeneralMotionData>();
        Vector3 predatorHead = disperseXFormPos + disperseXFormFwd * predatorHalfLength;
        Vector3 dirFromPredatorHead = critterPos - predatorHead;
		float dirFromPredatorHeadMag = dirFromPredatorHead.magnitude;
			//Debug.Log(" dirFromPredatorHeadMag " + dirFromPredatorHeadMag + " gmd.critterBoxColliderSize.z " + gmd.critterBoxColliderSize.z);
		if(dirFromPredatorHeadMag < gmd.critterBoxColliderSize.z * 0.3f )
		{
            Vector3 dirFromPredatorHeadNorm = dirFromPredatorHead / dirFromPredatorHeadMag;
            Vector3 pushDir = Vector3.Cross(Vector3.Cross(disperseXFormFwd,dirFromPredatorHeadNorm),disperseXFormFwd);
            pushDir = (pushDir + dirFromPredatorHeadNorm).normalized;
           
            //Debug.Log("push:");
			critter_info.critterTransform.position += pushDir * ( gmd.critterBoxColliderSize.z * 0.3f - dirFromPredatorHeadMag )* 0.1f ;
            critter_info.cachedPosition = critter_info.critterTransform.position;
#if UNITY_EDITOR
            Debug.DrawLine(predatorHead, predatorHead +  dirFromPredatorHead, Color.white);
#endif           
		}	
	}

	public override float EvaluatePriority(CritterInfo critter_info)
	{
		GeneralMotionData gmd = critter_info.generalMotionData;
		SwimDisperseData sd = critter_info.swimDisperseData;
		if(sd)
		{
			if(gmd.isDispersed) sd.currentPriorityValue = sd.priorityValue;
			else  sd.currentPriorityValue = 0f;
			return sd.currentPriorityValue;
		}
		else return 0f;
	}
}


