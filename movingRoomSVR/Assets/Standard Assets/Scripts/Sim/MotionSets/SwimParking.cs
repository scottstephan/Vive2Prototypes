using UnityEngine;
using System.Collections;

[System.Serializable]
public class SwimParking : BehaviorBase {
	// Use this for initialization
	
	public override void OneTimeStart( CritterInfo critter_info ) 
	{
		//WemoLog.Eyal("oneTimeStart SwimParking " + critter_info.critterObject.name);
		SwimParkingData sd = critter_info.swimParkingData;
		if(sd == null) return;
		sd.swimSpeed = sd.swimSpeedMult * critter_info.generalMotionData.swimSpeed;
	}

    public override void OneTimeEnd( CritterInfo critter_info ) 
    {
        if( critter_info.swimParkingData != null
            && critter_info.swimParkingData.spot != null ) {
            critter_info.swimParkingData.spot.available = true;
            critter_info.swimParkingData.spot = null;
        }
    }

	public override void Start(CritterInfo critter_info)
	{
		SwimParkingData sd = critter_info.swimParkingData;
		if(sd == null) return;
		GeneralMotionData gmd = critter_info.generalMotionData;
		gmd.lockVelocityToHeading = true;
		//gmd.useAvoidanceGround = false;
//		WemoLog.Eyal(critter_info.critterObject.name + " is now Parking ------------------------------------------------------");
		sd.isParked = false;
		sd.parkingNeeded = false;
		sd.parkingLevel = 1f;
		sd.parkingTimer = Random.Range(sd.parkingTimeMin,sd.parkingTimeMax);
		sd.targetPosition = new Vector3(0f,-468f,0f); // default position instead of y=0
		if(!sd.spot) ParkingLot.FindClosestSpot(critter_info);
        sd.doneParking = false;		
        sd.okToEnd = false;
	}
	
	// Update is called once per frame
	public override void Update (CritterInfo critter_info) 
	{
		SwimParkingData sd = critter_info.swimParkingData;
		if(sd == null) return;
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		GeneralMotionData gmd = critter_info.generalMotionData;
		if(!sd.isParked)
		{
			Vector3 dirToTarget = sd.targetPosition - critter_info.critterTransform.position;
            dirToTarget.y -= gmd.critterBoxColliderRadius;
			float distToTarget = dirToTarget.magnitude;
	
			gmd.desiredSpeed = sd.swimSpeed * MathfExt.Fit(distToTarget,30f,300f,0.1f,1f);
			gmd.desiredVelocityDirection = dirToTarget;
			
			if(distToTarget < gmd.critterBoxColliderRadius)
			{
				sd.isParked = true;
//				WemoLog.Eyal("is Parked");
			}
			
			Debug.DrawLine(sd.targetPosition, critter_info.critterTransform.position,Color.magenta);
			//Debug.DrawLine(critter_info.critterTransform.position, critter_info.critterTransform.position + offsetDir * 300f,Color.green);
		
		}
		else if( !sd.doneParking )
		{
			gmd.desiredSpeed = 0f;
			gmd.desiredVelocityDirection = critter_info.critterTransform.forward;
			sd.parkingTimer -= Time.deltaTime;
			if(sd.parkingTimer <= 0f)
			{
                sd.doneParking = true;
			}
		}
        else if( sd.doneParking ) 
        {
            gmd.desiredSpeed = sd.swimSpeed;
            gmd.desiredVelocityDirection = critter_info.critterTransform.forward + Vector3.up;
            if( sd.okToEnd ) {
                sd.isParked = false;
                sd.spot.available = true;
//                WemoLog.Eyal(sd.spot.name + " is now available " + sd.spot.available);
                sd.spot = null;
                //gsd.parkingNeeded = false;
                //gsd.parkingLevel = 1f;
                gsd.switchBehavior = true;
            }
        }
	}

	public override void End(CritterInfo critter_info)
	{
		if(critter_info.swimParkingData.spot)
		{
			critter_info.swimParkingData.spot.available = true;
			critter_info.swimParkingData.spot = null;
		}
	}

	public override float EvaluatePriority(CritterInfo critter_info)
	{
		SwimParkingData sd = critter_info.swimParkingData;
		if(sd)
		{
			if(sd.parkingNeeded) sd.currentPriorityValue = sd.priorityValue;
			else sd.currentPriorityValue = 0f;
			return sd.currentPriorityValue;
		}
		else return 0f;
	}
	
	public override void PreCollision(CritterInfo critter_info){
		if(critter_info.generalSpeciesData.myCurrentBehaviorType != SwimBehaviorType.SWIM_PARKING) return;
		//WemoLog.Eyal("PreParkingCollision " + critter_info.critterObject.name);
		float use_radius = critter_info.generalMotionData.critterBoxColliderRadius;
		RaycastHit hit;
		Vector3 pos = critter_info.critterTransform.position;
		Vector3 up = Vector3.up;
		Ray ray = new  Ray(pos, up);
		if(Physics.SphereCast(ray,use_radius, out hit, use_radius, 1<<14)){
			use_radius = hit.distance;
		}
		critter_info.critterTransform.position += up * use_radius;
	}
	public override void PostCollision(CritterInfo critter_info){
		if(critter_info.generalSpeciesData.myCurrentBehaviorType != SwimBehaviorType.SWIM_PARKING) return;
		//WemoLog.Eyal("PostParkingCollision " + critter_info.critterObject.name);
		float use_radius = critter_info.generalMotionData.critterBoxColliderRadius;
		RaycastHit hit;
		Vector3 pos = critter_info.critterTransform.position;
		Vector3 up = Vector3.up;
		Ray ray = new  Ray(pos, up);
		if(Physics.SphereCast(ray,use_radius, out hit, use_radius, 1<<14)) {
			use_radius = hit.distance;
		}
        else if( critter_info.swimParkingData.doneParking ) {
            critter_info.swimParkingData.okToEnd = true;
        }
		critter_info.critterTransform.position -= up * use_radius;
	}
}


