using UnityEngine;
using System.Collections;

[System.Serializable]
public class SwimFreefall : BehaviorBase {
	// Use this for initialization
	
	public override void OneTimeStart( CritterInfo critter_info ) 
	{
	}
	
	public override void Start(CritterInfo critter_info)
	{
		GeneralMotionData gmd = critter_info.generalMotionData;
		GeneralSpeciesData gsd  = critter_info.generalSpeciesData;
		gmd.lockVelocityToHeading = true;
		gsd.becameAirborn = false;
		
	}
	
	// Update is called once per frame
	public override void Update (CritterInfo critter_info) 
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		GeneralMotionData gmd = critter_info.generalMotionData;
		Vector3 fwd = critter_info.critterTransform.forward;
		Vector3 vel = fwd * gmd.currentSpeed;
		float ySpeed = vel.y - 9.8f * Time.deltaTime;
		gmd.desiredVelocityDirection = new Vector3(vel.x, ySpeed, vel.z);
		gmd.desiredSpeed = gmd.desiredVelocityDirection.magnitude;
		
		if(critter_info.critterTransform.position.y < -gmd.critterBoxColliderSize.z * 0.5f)
		{
			gsd.switchBehavior = true;
			//WemoLog.Eyal("under waterline");
		}
	}
	
	public override float EvaluatePriority(CritterInfo critter_info)
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		SwimFreefallData sd = critter_info.swimFreefallData;
		if(sd)
		{
			if(gsd.becameAirborn) sd.currentPriorityValue = sd.priorityValue;
			else sd.currentPriorityValue = 0f;
			return sd.currentPriorityValue;
		}
		else return 0f;
	}
}


