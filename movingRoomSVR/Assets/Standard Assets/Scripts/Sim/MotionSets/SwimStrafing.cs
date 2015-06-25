using UnityEngine;
using System.Collections;

[System.Serializable]
public class SwimStrafing : SwimFree {

    void ChooseNewDirection( SwimStrafingData sd ) 
    {
        sd.desiredRandomX = RandomExt.FloatWithRawBiasPower(-sd.maxXDirOffset,sd.maxXDirOffset,0,sd.maxXDirPower);
        sd.randomXTimer = Random.Range(sd.timerOffsetMin,sd.timerOffsetMax);
    }
    
    // Use this for initialization	
	public override void OneTimeStart( CritterInfo critter_info ) 
	{
        base.OneTimeStart( critter_info );        
		SwimStrafingData sd = critter_info.swimStrafingData;
		sd.swimSpeed = sd.swimSpeedMult * critter_info.generalMotionData.swimSpeed;
		//WemoLog.Eyal("strafing speed " + sd.swimSpeed);
	}        
		
	public override void Start(CritterInfo critter_info)
	{
        base.Start(critter_info);        
		GeneralMotionData gmd = critter_info.generalMotionData;
		SwimStrafingData sd = critter_info.swimStrafingData;
		gmd.lockVelocityToHeading = false;
        sd.randomX = 0;
        ChooseNewDirection(sd);
//		WemoLog.Eyal(critter_info.critterObject.name + " is now strafing ------------------------------------------------------");
	}
	
	// Update is called once per frame
	public override void Update (CritterInfo critter_info) 
	{
        base.Update( critter_info );
        
        SwimStrafingData sd = critter_info.swimStrafingData;
        SwimFreeData sf = critter_info.swimFreeData;
		GeneralMotionData gmd = critter_info.generalMotionData;
        
//        Debug.Log(sf.desiredDirection);
        float dir_dot = Vector3.Dot(sf.desiredDirection,critter_info.critterTransform.forward);
        bool old_lock = gmd.lockVelocityToHeading;
        gmd.lockVelocityToHeading = gmd.avoidFish || gmd.avoidGround || dir_dot < 0.985f;
        
        if( old_lock != gmd.lockVelocityToHeading ) {
            if( gmd.lockVelocityToHeading ) {
                sd.desiredRandomX = 0f;
            }
            else {
                ChooseNewDirection( sd );
            }
        }
        
        float dt = Time.deltaTime;
        MathfExt.AccelDampDelt( sd.desiredRandomX, sd.xAccel, sd.xDecel, dt, sd.xMaxSpeed, ref sd.xSpeed, ref sd.randomX, ref sd.xDecelActive, false );
        
        Vector3 right = Vector3.Cross( sf.desiredDirection, Vector3.up );
        right.y = 0f;
        right.Normalize();
		Vector3 offsetDir = sf.desiredDirection + right * sd.randomX;
		
		gmd.desiredVelocityDirection = offsetDir;
//		critter_info.critterSteering.desiredRotation = Quaternion.LookRotation(dirToTarget);
		// TODO> strafing only has a single steering throttle
		critter_info.critterSteering.desiredSteeringThrottle = sd.desiredSteeringThrottle;
		gmd.desiredSpeed = sd.swimSpeed;
				
		//reset randomDirection
        if( !gmd.lockVelocityToHeading ) {
    		sd.randomXTimer -= dt;
    		if(sd.randomXTimer <= 0f)
    		{
                ChooseNewDirection(sd);	        
    		}
        }
		
		
//		Debug.DrawLine(sd.targetPosition, critter_info.critterTransform.position,Color.magenta);
#if UNITY_EDITOR
        Debug.DrawLine(critter_info.critterTransform.position, critter_info.critterTransform.position + offsetDir * 300f,Color.green);
#endif		
	}
	
	public override float EvaluatePriority(CritterInfo critter_info)
	{
//        Debug.Log("CHECKING STRAFE");
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		SwimStrafingData sd = critter_info.swimStrafingData;
		if(sd)
		{
			if(gsd.becameNotHungry && !gsd.eatsReef && gsd.isStrafing) 
                sd.currentPriorityValue = sd.priorityValue;
			else if(gsd.isStrafing) 
                sd.currentPriorityValue = sd.priorityValue * 0.25f; //10 instead of 40 for default swim
		    else 
                sd.currentPriorityValue = 0f;
			return sd.currentPriorityValue;
		}
		else return 0f;
	}
}


