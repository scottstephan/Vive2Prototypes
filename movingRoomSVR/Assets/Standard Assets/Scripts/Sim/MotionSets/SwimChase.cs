using UnityEngine;
using System.Collections;

[System.Serializable]
public class SwimChase : BehaviorBase {
	// Use this for initialization
	
	public override void OneTimeStart( CritterInfo critter_info ) 
	{
		SwimChaseData sd = critter_info.swimChaseData;
		sd.swimSpeed = sd.swimSpeedMult * critter_info.generalMotionData.swimSpeed;
	}
	
	public override void Start(CritterInfo critter_info)
	{
		GeneralMotionData gmd = critter_info.generalMotionData;
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		gmd.lockVelocityToHeading = true;
		gsd.becameAgrressive = false;
		critter_info.swimChaseData.pointOrig = critter_info.critterTransform.position;
		critter_info.swimChaseData.returnToPointOrig = false;
		critter_info.swimChaseData.chaseStartTime = 0f;
	}
	
	// Update is called once per frame
	public override void Update (CritterInfo critter_info) 
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		GeneralMotionData gmd = critter_info.generalMotionData;

        SwimChaseData sd = critter_info.swimChaseData;
	
		if( sd.victim == null 
			|| sd.victim.markedForRemove
			|| sd.victim.generalMotionData == null 
			|| sd.victim.generalSpeciesData == null 
			|| sd.victim.critterTransform == null
			|| sd.victim.generalSpeciesData.isExitingScene ) {
			gsd.switchBehavior = true;
			gsd.becameNotHungry = true;
			return;
		}
		
		// chase only has a default turning throttle
		critter_info.critterSteering.desiredSteeringThrottle = sd.desiredSteeringThrottle;
		
		Vector3 diff = critter_info.critterTransform.position - sd.pointOrig;
		float diffMag = diff.magnitude;
		sd.chaseStartTime += Time.deltaTime;
		if(!sd.returnToPointOrig
		   && (diffMag > sd.chaseRadius || sd.chaseStartTime > sd.maxChaseTime))
		{
			sd.returnToPointOrig = true;
			//release victim from disperse
			sd.victim.generalMotionData.isDispersed = false;
			sd.victim.generalMotionData.isBeingChased = false;
			sd.victim.generalSpeciesData.switchBehavior = true;
		}
		if(sd.returnToPointOrig 
		   && ( ( diffMag < gmd.critterBoxColliderRadius * 2f ) 
		       || ( sd.chaseStartTime > ( sd.maxChaseTime * 1.5f ) ) ) )
		{
			gsd.switchBehavior = true;
			gsd.becameNotHungry = true;
		}
		
		if(!sd.returnToPointOrig)
		{
			gmd.desiredVelocityDirection = sd.victim.critterTransform.position - critter_info.critterTransform.position;
			float mag = gmd.desiredVelocityDirection.magnitude;
			gmd.speedMult = 3f;//MathfExt.Fit(mag,gmd.critterBoxColliderSize.z,gmd.disperseRadius*1.1f*gmd.predatorOffsetMult,2.0f,1.0f);
			gmd.desiredVelocityDirection *= 1f/mag; // normalized
			gmd.avoidanceDelay = 0;
		    gmd.avoidanceGroundDelay = gmd.avoidanceGroundEveryNFrames-1;
			gmd.desiredSpeed = gmd.speedMult * sd.swimSpeed;
#if UNITY_EDITOR
            Debug.DrawLine(critter_info.critterTransform.position, sd.victim.critterTransform.position,Color.magenta);
#endif
		}
		else
		{
			gmd.desiredVelocityDirection = sd.pointOrig - critter_info.critterTransform.position;
			float mag = gmd.desiredVelocityDirection.magnitude;
			gmd.speedMult = 0.8f; //MathfExt.Fit(mag,gmd.critterBoxColliderSize.z,gmd.disperseRadius*1.1f*gmd.predatorOffsetMult,2.0f,1.0f);
			gmd.desiredVelocityDirection *= 1f/mag; // normalized
			gmd.desiredSpeed = gmd.speedMult * sd.swimSpeed;	
		}

	}

	public override float EvaluatePriority(CritterInfo critter_info)
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		SwimChaseData sd = critter_info.swimChaseData;
		if(sd)
		{
			if(gsd.becameAgrressive) sd.currentPriorityValue = sd.priorityValue;
			else sd.currentPriorityValue = 0f;
			return sd.currentPriorityValue;
		}
		else return 0f;
	}
}


