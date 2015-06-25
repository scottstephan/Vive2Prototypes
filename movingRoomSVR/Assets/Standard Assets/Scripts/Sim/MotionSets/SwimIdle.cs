using UnityEngine;
using System.Collections;

[System.Serializable]
public class SwimIdle : BehaviorBase {

	// Use this for initialization
	
	public override void OneTimeStart( CritterInfo critter_info ) 
	{
		SwimIdleData sd = critter_info.swimIdleData;
		sd.posOffset = new Vector3((Random.value-0.5f)*sd.followRadius,(Random.value-0.5f)*sd.followRadius,(Random.value-0.5f)*sd.followRadius);
		sd.randOffsetSpeed =  new Vector3((Random.value-0.5f)*2.0f,(Random.value-0.5f)*2.0f,(Random.value-0.5f)*2.0f);
		sd.posLast = critter_info.critterTransform.position;
		sd.idleTimer = 0f;
		sd.idleTimeRandom = Random.value * sd.idleTimeRandom;;
		sd.switchPosTimer = Random.value * sd.switchPosRandom; 
				
		sd.followRadius = sd.followRadiusMult * critter_info.generalMotionData.followRadius;
		sd.swimSpeed = sd.swimSpeedMult * critter_info.generalMotionData.swimSpeed;
		sd.smoothRotate = sd.smoothRotateMult * critter_info.generalMotionData.smoothRotate;
		sd.sinMotionFreqMult = sd.sinMotionFreqMult * critter_info.generalMotionData.sinMotionFreq;
		sd.desiredDir = critter_info.critterTransform.forward;
	}
	
	public override void Start(CritterInfo critter_info)
	{
		//WemoLog.Eyal("idle start " + critter_info.critterObject.name);
		SwimIdleData sd = critter_info.swimIdleData;
        if( sd.PreStart != null ) {
            sd.PreStart( critter_info );
        }

		GeneralMotionData gmd = critter_info.generalMotionData;
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		gmd.lockVelocityToHeading = false;
		gsd.becameNotHungry = false;
		sd.targetPosition = critter_info.critterTransform.position + critter_info.critterTransform.forward * gmd.currentSpeed;
		//WemoLog.Eyal("start idle " + gmd.currentSpeed);
		sd.desiredDir = critter_info.critterTransform.forward;
		sd.idleTimer = 0f;
		
        if( sd.PostStart != null ) {
            sd.PostStart( critter_info );
        }
	}
	
    public override void End( CritterInfo critter_info ) 
    {
        SwimIdleData sd = critter_info.swimIdleData;
        if( sd.PreEnd != null ) {
            sd.PreEnd( critter_info );
        }
     
        // this behavior doesnt have anything to cleanup yet..
        
        if( sd.PostEnd != null ) {
            sd.PostEnd( critter_info );
        }
    }
    
	// Update is called once per frame
	public override void Update (CritterInfo critter_info) 
	{
		SwimIdleData sd = critter_info.swimIdleData;
		GeneralMotionData gmd = critter_info.generalMotionData;
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		
		Vector3 fwd = critter_info.critterTransform.forward;
		gmd.desiredVelocityDirection = fwd;
		
		// doing this fiddly time calc so that the fish do not jump after being paused.
		float use_time = critter_info.lifetime;
		sd.posOffset = new Vector3(sd.randOffsetMult.x*sd.followRadius* Mathf.Cos(use_time*sd.randOffsetSpeed.x*sd.sinMotionFreqMult),sd.randOffsetMult.y*sd.followRadius* Mathf.Cos(use_time*sd.randOffsetSpeed.y*sd.sinMotionFreqMult),sd.randOffsetMult.z*sd.followRadius* (0.5f + 0.5f * Mathf.Cos(use_time*sd.randOffsetSpeed.z*sd.sinMotionFreqMult)));
		sd.posOffset = new Vector3(fwd.x * sd.posOffset.z, sd.posOffset.y,fwd.z * sd.posOffset.z);
		Vector3 targetPos = sd.targetPosition + sd.posOffset;
		gmd.desiredVelocityDirection = targetPos - critter_info.critterTransform.position;
		//if(diff.magnitude<gmd.halfSizeZ*0.1f) SwimIdle.ChangeTarget(critter_info);
#if UNITY_EDITOR
		Debug.DrawLine(targetPos,targetPos + new Vector3(0f,20f,0),Color.white);
#endif

		Vector3 dir = new Vector3(sd.desiredDir.x,0f,sd.desiredDir.z);

		
		//smoooth out motion when transition from another behavior
		if(sd.idleTimer < 5f)
		{
			float factor = sd.idleTimer / 5f;
			Vector3 smoothDir = gmd.desiredVelocityDirection.normalized * factor + critter_info.critterTransform.forward * (1f - factor);
			gmd.desiredVelocityDirection = smoothDir.normalized;
			dir = dir * factor + critter_info.critterTransform.forward * (1f - factor) ;
			dir = dir.normalized;
		}
		
		
		float mag = gmd.desiredVelocityDirection.magnitude;
		if( MathfExt.Approx(mag, 0f, 0.01f) ) {
			gmd.desiredVelocityDirection = critter_info.critterTransform.forward;
		}
		else {
			gmd.desiredVelocityDirection *= 1f/mag;
		}
		
		Quaternion desiredDir = Quaternion.LookRotation(dir);

		critter_info.critterSteering.desiredRotation = desiredDir;
		critter_info.critterSteering.desiredSteeringThrottle = sd.desiredSteeringThrottle;
		
		//float speedMult = MathfExt.Fit(gmd.desiredVelocityDirection.magnitude,gmd.critterBoxColliderSize.z * 0.3f,gmd.critterBoxColliderSize.z * 1f,0.1f,1.0f);
		float speedMult = MathfExt.Fit(gmd.desiredVelocityDirection.magnitude, 0f,gmd.critterBoxColliderRadius * 0.4f ,0.2f,1.0f);
		gmd.desiredSpeed = speedMult * sd.swimSpeed;
		
		// set and reset timers

		sd.idleTimer += Time.deltaTime;
		if(sd.idleTimer>sd.idleTime - sd.idleTimeRandom)
		{
			sd.idleTimer = 0f;	
			gsd.switchBehavior = true;
		}
		
		sd.switchPosTimer += Time.deltaTime;
		if(sd.switchPosTimer > sd.switchPosFreq) SwimIdle.ChangeTarget(critter_info);
	}

	public static void ChangeTarget (CritterInfo critter_info) 
	{
		SwimIdleData sd = critter_info.swimIdleData;
//		GeneralMotionData gmd = critter_info.generalMotionData;
		sd.switchPosTimer = Random.value * sd.switchPosRandom;
		sd.desiredDir = new Vector3(Random.value - 0.5f,0f,Random.value - 0.5f).normalized;
		//RaycastHit hit;
		//Vector3 newTarget = sd.targetPosition + 0.5f * sd.followRadius * new Vector3(Random.value-0.5f,Random.value-0.5f, Random.value-0.5f);
		//Vector3 dir = newTarget - critter_info.critterTransform.position;
		//if(! Physics.SphereCast (critter_info.critterTransform.position, gmd.halfSizeZ*0.7f,dir.normalized, out hit, dir.magnitude, 1<<14))
		//{
		//	sd.targetPosition = newTarget;
		//}
	}
	
	public static void pokeIdle(CritterInfo critter_info, CritterInfo poker)
	{
		if( critter_info == null 
			|| critter_info.markedForRemove )
		{
			return;
		}
		
		SwimIdleData sd = critter_info.swimIdleData;
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		GeneralMotionData gmd = critter_info.generalMotionData;
		
		if( sd == null || gsd == null || gmd == null || poker.generalMotionData == null ) {
			return;
		}
		
		float myVolume = gmd.critterBoxColliderVolume;
		float pokerVolume = poker.generalMotionData.critterBoxColliderVolume;
		if(pokerVolume > myVolume * gmd.avoidFishLargerThanMyVolumeRatio)
		{	
			sd.poked++;
			//WemoLog.Eyal(critter_info.critterObject.name + " was poked " + sd.poked + " by " + poker.critterObject.name);
			if(sd.poked > sd.stopIdleWhenPoked)
			{
				gsd.switchBehavior = true;
				sd.poked=0;
			}
		}
	}

	public override float EvaluatePriority(CritterInfo critter_info)
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		SwimIdleData sd = critter_info.swimIdleData;
		if(sd)
		{
			if(gsd.becameNotHungry && gsd.eatsReef) sd.currentPriorityValue = sd.priorityValue;
			else sd.currentPriorityValue = 0f;
			return sd.currentPriorityValue;
		}
		else return 0f;
	}	
}


