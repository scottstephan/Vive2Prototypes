using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SwimTargeted : BehaviorBase {
	// Use this for initialization

	static List<CritterInfo> targetDesignCritters = new List<CritterInfo>();
    static string[] s_hungrytags_any;
    static string[] s_hungrytags_species;

	public override void OneTimeStart( CritterInfo critter_info ) 
	{
		SwimTargetedData sd = critter_info.swimTargetedData;
		sd.posOffset = new Vector3((Random.value-0.5f)*sd.followRadius,(Random.value-0.5f)*sd.followRadius,(Random.value-0.5f)*sd.followRadius);
		sd.randOffsetSpeed =  new Vector3((Random.value-0.5f)*2.0f,(Random.value-0.5f)*2.0f,(Random.value-0.5f)*2.0f);
		sd.posLast = critter_info.critterTransform.position;
		sd.isTargetFood = false;
		sd.movingTarget = null;
		sd.targetGSD = null;
		sd.targetDeadData = null;
				
		sd.followRadius = sd.followRadiusMult * critter_info.generalMotionData.followRadius;
		sd.swimSpeed = sd.swimSpeedMult * critter_info.generalMotionData.swimSpeed;
		sd.smoothRotate = sd.smoothRotateMult * critter_info.generalMotionData.smoothRotate;
		sd.sinMotionFreqMult = sd.sinMotionFreqMult * critter_info.generalMotionData.sinMotionFreq;
		//sd.lastDistToTarget = (critter_info.generalMotionData.halfSizeZ * 2.0f) + 1f;
		sd.lastDistToTarget = (critter_info.generalMotionData.critterBoxColliderSize.z ) + 1f;

        if (s_hungrytags_any == null)
        {
            s_hungrytags_any = new string[1];
            s_hungrytags_any[0] = "reef_food";
        }

        if (s_hungrytags_species == null)
        {
            s_hungrytags_species = new string[2];
            s_hungrytags_species[0] =  "reef_food";
        }
	}
	
	public override void Start(CritterInfo critter_info)
	{
		GeneralMotionData gmd = critter_info.generalMotionData;
		GeneralSpeciesData gsd  = critter_info.generalSpeciesData;
		SwimTargetedData sd = critter_info.swimTargetedData;
		gmd.lockVelocityToHeading = true;
		CritterAnimationBase anim_base = critter_info.animBase;
		anim_base.isFeeding = false;
		sd.targetLerp = -1f;

		if(gsd.airNeeded)
		{
			GeneralMotion.SetSurfaceTarget(critter_info);
			sd.swimToSurfaceSpeedMult =  Random.Range(0.8f,2f);
			return;
		}
		if(gsd.becameHungry)
		{
			gsd.becameHungry = false;
			sd.isTargetFood = true;
			if (gsd.eatsReef)
			{
                s_hungrytags_species[1] = gsd.speciesTag;

                GameObject feeding_zone = SphereInstance.FindRandomWemoObjectWithAllTagsAndAnyTags(s_hungrytags_species, null);

                //WemoLog.Eyal(critter_info.critterObject.name + " FindRandomWemoObjectWithAllTagsAndAnyTags " + feeding_zone.gameObject.name + " pos " + critter_info.swimTargetedData.targetPosition );
				if (feeding_zone == null)
				{
                    feeding_zone = SphereInstance.FindRandomWemoObjectWithAllTagsAndAnyTags(s_hungrytags_any, null);
				}

				if (feeding_zone != null)
				{
                    Vector3 extents = feeding_zone.GetComponent<Collider>().bounds.size;
                    float radius = Mathf.Max(extents.x, Mathf.Max(extents.y, extents.z));
                    sd.targetPosition = feeding_zone.transform.position + new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f) * radius;
					Vector3 test_pos = new Vector3(sd.targetPosition.x, 0f, critter_info.swimTargetedData.targetPosition.z);
					Vector3 diff = sd.targetPosition - test_pos;
					Ray ray = new Ray(test_pos, diff);
					RaycastHit hit;
					if (Physics.SphereCast(ray, gmd.critterBoxColliderRadius * 2f, out hit, diff.magnitude, 1 << 14))
					{
						sd.targetPosition = hit.point + new Vector3(0f, gmd.critterBoxColliderRadius * 2f, 0f);
					}
					sd.savedTargetDirection = sd.targetPosition - critter_info.critterTransform.position;
					//WemoLog.Eyal(critter_info.critterObject.name + " found feeding_zone " + feeding_zone.gameObject.name + " pos " + critter_info.swimTargetedData.targetPosition );
				}
				else
				{
					//WemoLog.Eyal(critter_info.critterObject.name + " didn't found feeding_zone " );
				}

			}
			else
			{
				//Debug.Log(sd.transform + "SwimTargeted: Init search");
				sd.movingTarget = null;
				if (string.IsNullOrEmpty(critter_info.targetDesignGroupName))
				{
					AI.SearchForMovingTarget(critter_info);
				}
				else
				{
					targetDesignCritters.Clear();
					SimInstance.Instance.GetCrittersByDesignGroup(critter_info.targetDesignGroupName, targetDesignCritters);
					AI.SearchForMovingTarget(critter_info, targetDesignCritters);
				}

				// need to know if there is species data for eating the target
				if (sd.movingTarget != null)
				{
					//Debug.Log(sd.transform + "SwimTargeted: Found " + sd.movingTarget);
					sd.targetGSD = sd.movingTarget.GetComponent<GeneralSpeciesData>();
					sd.targetDeadData = sd.movingTarget.GetComponent<DeadData>();
					foreach (Transform t in critter_info.critterTransform)
					{
						if (t.name == "BitePos")
						{
							sd.myBitePosition = t;
							break;
						}
					}
				}
			}
		}
		
		if(gsd.becameNotHungry)
		{
			gsd.becameNotHungry = false;
			sd.isTargetFood = false;
			GeneralMotion.SetRandomTarget(critter_info);
		}
	}

	public override void End(CritterInfo critter_info)
	{
		critter_info.swimTargetedData.targetGSD = null;
		critter_info.swimTargetedData.myBitePosition = null;
		critter_info.targetDesignGroupName = null;
	}

	// Update is called once per frame
	public override void Update (CritterInfo critter_info) 
	{
		SwimTargetedData sd = critter_info.swimTargetedData;
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		GeneralMotionData gmd = critter_info.generalMotionData;
		CritterAnimationBase anim_base = critter_info.animBase;
				
		// targeted motion only has a constant steering throttle for now.
		critter_info.critterSteering.desiredSteeringThrottle = sd.desiredSteeringThrottle;

		bool scriptedTarget = sd.targetDeadData != null && sd.targetGSD != null && sd.myBitePosition != null;
		Vector3 c_pos = critter_info.critterTransform.position;
		if( gmd.avoidGround && !scriptedTarget) {
			sd.movingTarget = null;
			gsd.isHungry = false;
			gsd.hungerLevel = -1;
			//Debug.Log(critter_info.critterTransform + " hunger level: avoiding ground");
			sd.targetPosition = c_pos + gmd.desiredVelocityDirection * 6f * gmd.critterBoxColliderRadius;			
		}

		float speedMult = 1.0f;
		Vector3 real_diff = gmd.desiredVelocityDirection;
		float diff_dist = 0.0f;
		float real_diff_dist = 0.0f;
		
		//Debug.DrawLine(sd.targetPosition,sd.targetPosition + new Vector3(0f,30f,0f),Color.red);

		// doing this fiddly time calc so that the fish do not jump after being paused.
		float use_time = critter_info.lifetime;
		if(sd.movingTarget == null)
		{
			real_diff = sd.targetPosition - c_pos;
			real_diff_dist = real_diff.magnitude;
			sd.posOffset = Vector3.zero;
			if(sd.doSinMotion)
			{
				float followRadiusMult = MathfExt.Fit(real_diff_dist,sd.radiusMultDistMin,sd.radiusMultDistMax,sd.radiusMultMin,sd.radiusMultMax) *(0.5f+0.5f*Mathf.Cos(use_time*sd.randOffsetSpeed.x*0.1f + sd.randOffsetSpeed.y*52.4f));
				sd.posOffset = new Vector3(sd.followRadius*followRadiusMult* Mathf.Cos(use_time*sd.randOffsetSpeed.x*sd.sinMotionFreqMult),0.35f*sd.followRadius*followRadiusMult* Mathf.Cos(use_time*sd.randOffsetSpeed.y*sd.sinMotionFreqMult),sd.followRadius*followRadiusMult* Mathf.Cos(use_time*sd.randOffsetSpeed.z*sd.sinMotionFreqMult));
				gmd.desiredVelocityDirection  = sd.targetPosition + sd.posOffset - critter_info.critterTransform.position;
			}
			else
			{
				gmd.desiredVelocityDirection = real_diff;
			}
#if UNITY_EDITOR
			if(gsd.isHungry) 
            {
                Debug.DrawLine(sd.targetPosition,sd.targetPosition + new Vector3(0f,80f,0f),Color.green);
            }
			else
            {
                //Debug.DrawLine(sd.targetPosition,sd.targetPosition + new Vector3(0f,80f,0f),Color.red);
            }

			Debug.DrawLine(sd.targetPosition, critter_info.critterTransform.position,Color.magenta);
#endif
        }
		else
		{
			//Debug.DrawLine(sd.movingTarget.position,sd.movingTarget.position + new Vector3(0f,100f,0f),Color.black);
			real_diff = sd.movingTarget.position - critter_info.critterTransform.position;
			real_diff_dist = real_diff.magnitude;
			sd.posOffset = Vector3.zero;
			if(sd.doSinMotion)
			{
				float followRadiusMult = MathfExt.Fit(real_diff_dist,sd.radiusMultDistMin,sd.radiusMultDistMax,sd.radiusMultMin,sd.radiusMultMax) *(0.5f+0.5f*Mathf.Cos(use_time*sd.randOffsetSpeed.x*0.1f + sd.randOffsetSpeed.y*52.4f));
				sd.posOffset = new Vector3(sd.followRadius*followRadiusMult* Mathf.Cos(use_time*sd.randOffsetSpeed.x),0.35f*sd.followRadius*followRadiusMult* Mathf.Cos(use_time*sd.randOffsetSpeed.y),sd.followRadius*followRadiusMult* Mathf.Cos(use_time*sd.randOffsetSpeed.z));
				gmd.desiredVelocityDirection  = sd.movingTarget.position + sd.posOffset - critter_info.critterTransform.position;
			}	
			else
			{
				gmd.desiredVelocityDirection = real_diff;
			}
			
			if(real_diff_dist < gmd.critterBoxColliderSize.z * 0.8f)
			{
				anim_base.isFeeding = true;
			}
			else
			{
				anim_base.isFeeding = false;
			}
			
			Debug.DrawLine(sd.movingTarget.position, critter_info.critterTransform.position,Color.green);
		}

		// if not hungry go back to your leader   -    need to change that to eatingBehavior  
		diff_dist = gmd.desiredVelocityDirection.magnitude;
		float dot = Vector3.Dot(real_diff, sd.savedTargetDirection);
		// line up for more tightly registered moment
		if (scriptedTarget &&
            sd.myBitePosition != null)
        {
            // use mouth pos
            real_diff = sd.movingTarget.position - sd.myBitePosition.position;
            real_diff_dist = real_diff.magnitude;

            bool bDoLerp = real_diff_dist < sd.EatTargetLerpDist && dot > 0;
            if (bDoLerp || sd.targetLerp >= 0f) // check stage 2, final registration.  Once started don't stop
            {
				if( sd.targetLerp < 0f ) {
					sd.targetLerp = 0f;
				}

//                Debug.Log("SwimTargeted lerping target");
                sd.targetLerp += sd.EatTargetLerpTime * Time.deltaTime;
                Vector3 halfway = 0.5f * (sd.targetGSD.transform.position + sd.myBitePosition.position);
                sd.targetGSD.transform.position = Vector3.Lerp(sd.targetGSD.transform.position, halfway, sd.targetLerp);
                // may need to adjust shark swim motion as well during this, and/or leading up to it
                // note: shark's transform position is his center, but bite is at the mouth
                sd.transform.position = Vector3.Lerp(sd.transform.position, sd.transform.position + halfway - sd.myBitePosition.position, sd.targetLerp);
                if (sd.targetLerp >= 1f)
                {
                    sd.targetDeadData.SpawnGibs = true;
                    gsd.hungerLevel = -1f;
                    sd.targetDeadData = null;
                    sd.targetGSD = null;
                    // TODO: trigger new behavior to go after gibs
                }
            }
            else if (real_diff_dist < sd.EatTargetSeqStartDist && dot > 0f) // check stage 1, wider range to engage victim.
            {
                // not technically dead, but about to die. This blocks other behavior from triggering on the fish during death sequence
                // TODO: modify shark swimming behavior to align better
                sd.targetGSD.isDead = true;
                sd.targetGSD.switchBehavior = true;
            }
            else
            {
                sd.lastDistToTarget = real_diff_dist;
            }
        } else
        {
            if ((real_diff_dist < gmd.critterBoxColliderSize.z * 1f) || dot < 0f)
            {
                sd.lastDistToTarget = (gmd.critterBoxColliderSize.z * 3.0f) + 1f;

                if (!gsd.isHungry)
                {
                    //WemoLog.Eyal("Not hungry setting randomtarget");
                    GeneralMotion.SetRandomTarget (critter_info);
                    sd.movingTarget = null;
                } else if (sd.movingTarget != null && dot < 0f)
                {
                    // we have passed the target.. force us to not be hungry to stop circling..
                    gsd.hungerLevel = -1f;
                    sd.movingTarget = null;
                } else
                {
                    gsd.hungerLevel = -1f;
                }

            } else
            {
                sd.lastDistToTarget = real_diff_dist;
            }

            //reset airLevel if above water level
            if (critter_info.critterTransform.position.y > - gmd.critterBoxColliderSize.z * 0.2f)
            {
                gsd.airLevel = 1f;
                gsd.airNeeded = false;
                gsd.switchBehavior = true;
                gsd.becameAirborn = true;
                GeneralMotion.SetRandomTarget (critter_info);
            }
        }

		//speedMult = MathfExt.Fit(diff_dist,sd.speedMultDistMin,sd.speedMultDistMax,sd.speedMultMin,sd.speedMultMax);
		speedMult = MathfExt.Fit(diff_dist,sd.speedMultDistMin,sd.speedMultDistMax,sd.speedMultMin,sd.speedMultMax);
		//if(gsd.airNeeded) speedMult *= sd.swimToSurfaceSpeedMult;
		if(gsd.airNeeded && !gsd.isHungry) speedMult *= MathfExt.Fit(diff_dist,400f,800,0.1f, 1f);
		//if(gsd.mySpecies == SpeciesType.BARACUDA) WemoLog.Eyal(critter_info.critterObject.name + " dist " + diff_dist + " speedmult " + speedMult);
		gmd.desiredSpeed = speedMult * sd.swimSpeed;		
		//if(gsd.mySpecies == SpeciesType.GREATWHITESHARK) WemoLog.Eyal(critter_info.critterObject.name +  " diff_dist " + diff_dist +" DistMin " + sd.speedMultDistMin + " DistMax "  + sd.speedMultDistMax + " min "  + sd.speedMultMin +  " max " + sd.speedMultMax + " speedMult " + speedMult); 
	}

	public override float EvaluatePriority(CritterInfo critter_info)
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		SwimTargetedData sd = critter_info.swimTargetedData;
		//WemoLog.Eyal("airNeeded " + gsd.airNeeded + " isHungry " + gsd.isHungry + " becameHungry " + gsd.becameHungry);
		if(sd)
		{
			if(gsd.airNeeded) sd.currentPriorityValue = sd.priorityValue;
			else if(gsd.isHungry) sd.currentPriorityValue = sd.priorityValue * 0.625f; // priority 50 instead of 80
				else sd.currentPriorityValue = 0f;
			return sd.currentPriorityValue;
		}
		else return 0f;
	}	
}


