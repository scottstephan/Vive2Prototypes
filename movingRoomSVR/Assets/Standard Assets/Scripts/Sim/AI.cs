using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI 
{	
	private static BehaviorBase[] staticBehaviors;
	
	public static bool blockChecks = false;

	public const int LEADER_SEARCH_COUNTER_MAX = 60;

    public static int NumBehaviorTypes = 0;

	// Use this for initialization
	public static void Construct () 
	{
        NumBehaviorTypes = System.Enum.GetNames(typeof(SwimBehaviorType)).Length;
        staticBehaviors = new BehaviorBase[NumBehaviorTypes];
		staticBehaviors[(int)SwimBehaviorType.SWIM_IDLE] = new SwimIdle();
		staticBehaviors[(int)SwimBehaviorType.SWIM_SCHOOL_FOLLOW] = new SwimSchoolFollow();
		staticBehaviors[(int)SwimBehaviorType.SWIM_TARGETED] = new SwimTargeted();
		staticBehaviors[(int)SwimBehaviorType.SWIM_TO_POINT] = new SwimToPoint();
		staticBehaviors[(int)SwimBehaviorType.SWIM_DISPERSE] = new SwimDisperse();
		staticBehaviors[(int)SwimBehaviorType.SWIM_CHASE] = new SwimChase();
		staticBehaviors[(int)SwimBehaviorType.SWIM_FREEFALL] = new SwimFreefall();
		staticBehaviors[(int)SwimBehaviorType.SWIM_STRAFING] = new SwimStrafing();
		staticBehaviors[(int)SwimBehaviorType.SWIM_PARKING] = new SwimParking();
		staticBehaviors[(int)SwimBehaviorType.SWIM_FREE] = new SwimFree();
		staticBehaviors[(int)SwimBehaviorType.SWIM_IDLE_BITE] = new SwimIdleBite();
		staticBehaviors[(int)SwimBehaviorType.SWIM_PLAYER_INTERACT] = new SwimPlayerInteract();
		staticBehaviors[(int)SwimBehaviorType.VIEWPORT_MOTION] = new ViewportMotion();
		staticBehaviors[(int)SwimBehaviorType.INTERACTION] = new Interaction();
		staticBehaviors[(int)SwimBehaviorType.CIRCLE_AROUND_OBJECT] = new CircleAroundObject();
        staticBehaviors[(int)SwimBehaviorType.SWIM_FOLLOWPATH] = new SwimFollowPath();
        staticBehaviors[(int)SwimBehaviorType.DEAD] = new Dead();
        staticBehaviors[(int)SwimBehaviorType.HOLD] = new Hold();
        staticBehaviors[(int)SwimBehaviorType.SWIM_STRAFE_PLAYER] = new SwimStrafePlayer();
        staticBehaviors[(int)SwimBehaviorType.SWIM_PLAYER_VIEW] = new SwimPlayerView();
        staticBehaviors[(int)SwimBehaviorType.SWIM_SCRIPT_GOTO] = new SwimScriptGoToPoint();

        for( int i = 0; i < NumBehaviorTypes; i++ ) 
        {
			staticBehaviors[i].Construct();
		}	
	}
	
    public static bool IsSingletonBehavior(SwimBehaviorType behType)
    {
        return staticBehaviors[(int)behType].IsSingletonBehavior();
    }

	public static void Start(CritterInfo critter_info, bool newly_purchased)
	{
		for( int i = 0; i < staticBehaviors.Length; i++ ) {
			staticBehaviors[i].OneTimeStart( critter_info );
		}
		
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		SwimParkingData spd = critter_info.swimParkingData;
		gsd.isExitingScene = false;
			
		GeneralMotion.SetCameraTarget(critter_info);
		if( !newly_purchased ) {
			gsd.switchBehavior = true;
			GeneralMotion.SetRandomTarget(critter_info);
			
			gsd.hungerLevel = Random.value*0.99f;
			gsd.swimToPoint = true;
			gsd.isHungry = false;
			gsd.airLevel = Random.value * 0.99f;
			if(spd) spd.parkingLevel = Random.value * 0.99f;
			gsd.searchNewLeaderCounter = Mathf.FloorToInt(Random.value*60);
			critter_info.swimToPointData.targetPosition = critter_info.critterTransform.position + ( critter_info.critterTransform.forward * 200.0f );
			critter_info.swimToPointData.savedTargetDirection = critter_info.swimToPointData.targetPosition - critter_info.critterTransform.position;
			critter_info.swimToPointData.pointReachedType = PointReachedType.EnterTargetedMotion;
		}
		else {
			gsd.switchBehavior = true;
			gsd.swimToPoint = true;
			gsd.hungerLevel = 0;
			gsd.airLevel = Random.value * 0.99f;
			if(spd) spd.parkingLevel = Random.value * 0.99f;
			gsd.searchNewLeaderCounter = 0;
			critter_info.swimToPointData.targetPosition = critter_info.swimTargetedData.targetPosition;
			critter_info.swimToPointData.savedTargetDirection = critter_info.swimToPointData.targetPosition - critter_info.critterTransform.position;
			critter_info.swimToPointData.pointReachedType = PointReachedType.EnterTargetedMotion;
		}
    }
	
	public static void End( CritterInfo critter_info )
	{
		for( int i = 0; i < staticBehaviors.Length; i++ ) {
			staticBehaviors[i].OneTimeEnd( critter_info );
		}
	}
	
	public static void ForceSwitchToBehavior( CritterInfo critter, SwimBehaviorType type ) 
	{
		//		Debug.Log("FORCING " + type );
		critter.generalSpeciesData.switchBehavior = false;
		SwitchToBehavior( critter, type );
	}

	// Update is called once per frame
	public static void Update ( CritterInfo critter_info ) 
	{
		//DebugDisplay.AddDebugText("AI UPDATE "  + critter_info.critterObject.name );
//		Profiler.BeginSample("AI.Update()");
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		if(gsd.switchBehavior)
		{
			CheckForStart( critter_info );
			gsd.switchBehavior = false;
		}
		
		// TODO? per behavoir update function registration? So certain behaviors can just always run for this critter?
		if( !blockChecks && !critter_info.ignoreAiAutoTriggers ) {
			UpdateAgrression( critter_info );
			UpdateAir( critter_info );
// no parking right now.
//			UpdateParking( critter_info );    		
			
			// let the sim and game get goin before starting these...	
	#if !UNITY_EDITOR
			if( SimInstance.Instance.realTimeRunningSimInSphere > 6f ) 
	#endif
			{
				//DebugDisplay.AddDebugText("AI UPDATE HUNGER/LEADER "  + critter_info.critterObject.name );
				UpdateHunger( critter_info );
				UpdateLeader( critter_info );
			}
		}
		
		gsd.myCurrentBehavior.Update( critter_info );
//		Profiler.EndSample();
	}
	
	private static void CheckForStart(CritterInfo critter_info)
	{
		if( critter_info.generalSpeciesData.myCurrentBehavior != null &&
			critter_info.generalSpeciesData.myCurrentBehavior.IsSingletonBehavior() ) 
		{
			critter_info.generalSpeciesData.switchBehavior = false;
			return;
		}

		//WemoLog.Log("checkForStart -------------------------------- " + critter_info.critterObject.name);
		//DebugDisplay.AddDebugText("checkForStart "  + critter_info.critterObject.name);
		float cp = 0f;
		SwimBehaviorType swimType = critter_info.generalSpeciesData.myCurrentBehaviorType;
        for( int i = 0; i < NumBehaviorTypes; i++ ) {
			if( staticBehaviors[i].IsSingletonBehavior() && !staticBehaviors[i].SingletonAllowsSwitchTo() ) {
				continue;
			}

			float p = staticBehaviors[i].EvaluatePriority(critter_info);
			//WemoLog.Eyal(staticBehaviors[i] + " " + p);
			if(p > cp)
			{
				cp = p;
				swimType = (SwimBehaviorType)i;
			}
		}

		SwitchToBehavior(critter_info, swimType);
	}
	
	private static void ResetBehaviorFlags(CritterInfo critter_info)
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		gsd.swimToPoint = false;
		//gsd.becameHungry = false;
		//gsd.becameNotHungry = false;
		gsd.becameAgrressive = false;
		gsd.startSchooling = false;	

		//if(gsd.myCurrentBehaviorType != SwimBehaviorType.SWIM_PARKING && critter_info.swimParkingData.spot)
		//{
		//	critter_info.swimParkingData.spot.available = true;
		//	critter_info.swimParkingData.spot = null;
		//}
	}
		
	public static void UpdateAir( CritterInfo critter_info)
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		if(gsd.airNeeded) return;
		gsd.airLevel -= gsd.airIncrement * Time.deltaTime;		
		if(gsd.airLevel<0f)
		{
			gsd.airNeeded= true;
			gsd.switchBehavior = true;
		}
	}
	
	public static void UpdateParking( CritterInfo critter_info)
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		SwimParkingData spd = critter_info.swimParkingData;
		if(!spd) return;
		//if(spd.parkingNeeded) return;
		if(gsd.myCurrentBehaviorType == SwimBehaviorType.SWIM_PARKING) return;
		spd.parkingLevel -= spd.parkingIncrement * Time.deltaTime;		
		if(spd.parkingLevel<=0f)
		{
			//spd.parkingLevel = 0f;
			spd.parkingNeeded = true;
			gsd.switchBehavior = true;
		}
	}	
	
	public static void UpdateHunger( CritterInfo critter_info)
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		//GeneralMotionData gmd = critter_info.generalMotionData;
		// if fairly hungry go eat.  this needs to be modified
		if(gsd.hungerLevel<1f && gsd.canGetHungry && !gsd.isHungry)
		{
			 gsd.hungerLevel+=gsd.hungerIncrement * Time.deltaTime;
		}

		if(gsd.hungerLevel>=1f && !gsd.isHungry)
		{
			gsd.hungerLevel = 1f;
			gsd.isHungry = true;
			gsd.becameHungry = true;
			gsd.switchBehavior = true;
		}		
		
		if(gsd.hungerLevel<0f)
		{
			gsd.hungerLevel = 0f;
			gsd.isHungry = false;
			gsd.becameNotHungry = true;
			gsd.switchBehavior = true;
		}
		
	}
	public static void UpdateLeader( CritterInfo critter_info)
	{		
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		//GeneralMotionData gmd = critter_info.generalMotionData;
		
		gsd.searchNewLeaderCounter++;
		if(!gsd.isHungry
			&& gsd.searchNewLeaderCounter > LEADER_SEARCH_COUNTER_MAX
			&& gsd.doesSchool
			&& gsd.myCurrentBehaviorType != SwimBehaviorType.SWIM_IDLE
			&& gsd.myCurrentBehaviorType != SwimBehaviorType.SWIM_DISPERSE
			&& gsd.myCurrentBehaviorType != SwimBehaviorType.SWIM_CHASE
			&& gsd.myCurrentBehaviorType != SwimBehaviorType.SWIM_TO_POINT)
		{
			SearchForNewLeader(critter_info);
			gsd.searchNewLeaderCounter=0;
		}
	}

	public static void UpdateAgrression( CritterInfo critter_info)
	{
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		if(gsd.isExitingScene 
		   || (gsd.myCurrentBehaviorType != SwimBehaviorType.SWIM_IDLE && gsd.myCurrentBehaviorType != SwimBehaviorType.SWIM_FREE) 
		   || gsd.aggressionLevel == 0f)
		{
			return;
		}

		if(gsd.myCurrentBehaviorType == SwimBehaviorType.SWIM_IDLE && critter_info.swimIdleData.idleTimer < 5f) 
		{
			return;
		}

		SwimChaseData scd = critter_info.swimChaseData;
		GeneralMotionData gmd = critter_info.generalMotionData;
		if(scd.checkCounter == 0)
		{
//            RaycastHit hit;
//            RaycastHit[] hits = Physics.SphereCastAll(critter_info.critterTransform.position, scd.searchForVictim, Vector3.up, 1f, gmd.layerMask);
            Collider hit;
			Collider[] hits = Physics.OverlapSphere(critter_info.critterTransform.position, scd.searchForVictim, gmd.layerMask);
			float bestDistSqr = 0f;
			CritterInfo victim = null; // this will be the victim if found one
			for(int i = 0; i<hits.Length; i++)
			{
				hit = hits[i];
                GeneralSpeciesData victimGSD = hit.transform.gameObject.GetComponent<GeneralSpeciesData>();
                if (victimGSD == null)
                    continue;

				CritterInfo tmpVictim = victimGSD.myCritterInfo;
				if( tmpVictim == null 
                    || tmpVictim.markedForRemove 
					|| tmpVictim.preMarkedForRemove
					|| tmpVictim.dbVariantID == -1
					|| tmpVictim.generalMotionData == null
					|| tmpVictim.generalSpeciesData == null
					|| tmpVictim.generalSpeciesData.isExitingScene ) {
					continue;
				}
				
				if (!string.IsNullOrEmpty(scd.chaseDesignGroupName) &&
                    (tmpVictim.designGroupName == null ||
                     string.Compare(scd.chaseDesignGroupName, tmpVictim.designGroupName, System.StringComparison.InvariantCultureIgnoreCase) != 0))
				{
					continue;
				}

				// replaced not related check with species tag comparison
				bool not_related = string.Compare(critter_info.generalSpeciesData.speciesTag, tmpVictim.generalSpeciesData.speciesTag, System.StringComparison.InvariantCultureIgnoreCase) != 0 ;

//              if( !AppBase.Instance.RunningAsPreview() ) 
//				{	
//					not_related = ( parent_id != tmpVictim.itemData.legacyitemid );
//				}
				
				if(tmpVictim != critter_info &&
				   not_related &&
				   gmd.critterBoxColliderVolume * scd.relativeSizeToAttack >= tmpVictim.generalMotionData.critterBoxColliderVolume &&
				   Random.value < gsd.aggressionLevel)
				{
					Vector3 diff = hit.transform.position - critter_info.cachedPosition;
					float diffSqrMag = diff.sqrMagnitude;
					if (victim == null || diffSqrMag < bestDistSqr)
					{
						bestDistSqr = diffSqrMag;
						victim = tmpVictim;	
					}
				}
			}
			//did you find a victim
			if(victim != null)
			{
				GeneralMotionData victimGmd = victim.generalMotionData;
				GeneralSpeciesData victimGsd = victim.generalSpeciesData;
				
				// notify self to start chasing
				scd.victim = victim;
				gsd.becameAgrressive = true;
				gsd.switchBehavior = true;
				
				// notify the victim to start disperse
				// NOTE: if scripted, we don't want the victim to get away, so will need to trigger a "fake" escape
				if (!gsd.myCritterInfo.swimTargetedData.HasScriptTarget())
				{
                    DisperseCollision.Disperse(victimGsd, critter_info, critter_info.critterTransform, scd.searchForVictim, false);
					victimGmd.isBeingChased = true;
					//WemoLog.Eyal(critter_info.critterObject.name + " is chasing " + hit.transform.gameObject.name);
				}
			}
		}
		
		// set and reset counter
		scd.checkCounter++;
		if(scd.checkCounter > 4) scd.checkCounter = 0;
	}

	// note: modifications here may be needed in other SearchForMovingTarget methods
	public static void SearchForMovingTarget(CritterInfo critter_info)
	{
		//WemoLog.Eyal("SearchForMovingTarget " + critter_info.critterObject.name);
		float dist = 1000000000f;
		Vector3 pos = critter_info.critterTransform.position;
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		GeneralMotionData gmd = critter_info.generalMotionData;
//		CritterInfo targetCritter = null;
		for(int i=0; i< SimInstance.Instance.crittersInPopulation; i++)
		{
			CritterInfo critter = SimInstance.Instance.critters[i];
			if(gmd.critterBoxColliderSize.z * gsd.maxSizeToEatFactor > critter.generalMotionData.critterBoxColliderSize.z
				&& gmd.critterBoxColliderSize.z * gsd.minSizeToEatFactor < critter.generalMotionData.critterBoxColliderSize.z)
			{
				if(critter_info != critter)
				{
					Vector3 targetPos = critter.critterTransform.position;
					Vector3 diff = targetPos - pos;
					float diffMag = diff.sqrMagnitude;
					//WemoLog.Eyal("yummy " + critter.critterObject.name + " diffMag " + diff.sqrMagnitude);
					if(diffMag < dist)
					{
						critter_info.swimTargetedData.movingTarget = critter.critterTransform;
						dist = diffMag;
//						targetCritter = critter;
                        critter_info.swimTargetedData.savedTargetDirection = critter.cachedPosition - critter_info.cachedPosition;
					}
				}
			}
		}
		if(critter_info.swimTargetedData.movingTarget == null)
		{
			gsd.isHungry = false;
			gsd.becameHungry = false;
			gsd.becameNotHungry = true;
			gsd.hungerLevel = 0f;
			gsd.switchBehavior = true;
			critter_info.swimTargetedData.isTargetFood = false;
//			Debug.Log("didn't find a target " + critter_info.critterObject.name + " size " + gmd.critterBoxColliderSize.z);
			CheckForStart(critter_info);
		}
		//else WemoLog.Eyal(critter_info.critterObject.name + " is hungry for " + 
		//				  critter_info.swimTargetedData.movingTarget.gameObject.name + " dist " + dist + " \n " +
		//				  gmd.critterBoxColliderSize.z);
		
		
	}

	// note: modifications here may be needed in other SearchForMovingTarget methods
	public static void SearchForMovingTarget(CritterInfo critter_info, List<CritterInfo> critters)
	{
		//WemoLog.Eyal("SearchForMovingTarget " + critter_info.critterObject.name);
		float dist = 1000000000f;
        Vector3 pos = critter_info.cachedPosition;
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
//		GeneralMotionData gmd = critter_info.generalMotionData;
		//		CritterInfo targetCritter = null;
		int num = critters.Count;
		for (int i = 0; i < num; i++)
		{
			CritterInfo critter = critters[i];
			//if (gmd.critterBoxColliderSize.z * gsd.maxSizeToEatFactor > critter.generalMotionData.critterBoxColliderSize.z
			//    && gmd.critterBoxColliderSize.z * gsd.minSizeToEatFactor < critter.generalMotionData.critterBoxColliderSize.z)
			{
				if (critter_info != critter)
				{
                    Vector3 targetPos = critter.cachedPosition;
					Vector3 diff = targetPos - pos;
					float diffMag = diff.sqrMagnitude;
					//WemoLog.Eyal("yummy " + critter.critterObject.name + " diffMag " + diff.sqrMagnitude);
					if (diffMag < dist)
					{
						critter_info.swimTargetedData.movingTarget = critter.critterTransform;
						dist = diffMag;
						//						targetCritter = critter;
                        critter_info.swimTargetedData.savedTargetDirection = critter.cachedPosition - critter_info.cachedPosition;
					}
				}
			}
		}
		if (critter_info.swimTargetedData.movingTarget == null)
		{
			gsd.isHungry = false;
			gsd.becameHungry = false;
			gsd.becameNotHungry = true;
			gsd.hungerLevel = 0f;
			gsd.switchBehavior = true;
			critter_info.swimTargetedData.isTargetFood = false;
//			Debug.Log("targeted: didn't find a target " + critter_info.critterObject.name + " size " + gmd.critterBoxColliderSize.z);
			CheckForStart(critter_info);
		}
		//else WemoLog.Eyal(critter_info.critterObject.name + " is hungry for " + 
		//				  critter_info.swimTargetedData.movingTarget.gameObject.name + " dist " + dist + " \n " +
		//				  gmd.critterBoxColliderSize.z);


	}


	public static void SetNewLeader(CritterInfo critter_info, CritterInfo newLeader)
	{
		SwimSchoolFollowData sd = critter_info.swimSchoolFollowData; 

		if (sd.leaderCritterInfo == newLeader)
		{
			return;
		}

		if (sd.leaderCritterInfo != null)
		{
			if (sd.leaderCritterInfo.followCount > 0)
			{
				sd.leaderCritterInfo.followCount--;

				if (sd.leaderCritterInfo.isPlayer &&
					sd.leaderCritterInfo.followCount == 6)
				{
					AudioManager.FadeOutSFX(SoundFXID.Schooling, 2f);
				}
			}                    
		}

        sd.followRadius = sd.followColliderRadiusMult * critter_info.generalMotionData.critterBoxColliderSize.z;

		if (newLeader != null)
		{
			newLeader.followCount++;
			critter_info.generalSpeciesData.startSchooling = true;

			if(newLeader.swimSchoolFollowData == null ||
			   newLeader.swimSchoolFollowData.leaderTransform == null)
			{
				sd.leaderCritterInfo = newLeader;
				sd.leaderTransform = newLeader.critterTransform;
			}
			else 
			{
				sd.leaderCritterInfo = newLeader.swimSchoolFollowData.leaderCritterInfo;
				sd.leaderTransform = newLeader.swimSchoolFollowData.leaderTransform;
			}

			if (newLeader.generalMotionData != null)
			{
				// Add leader's data to collider follow radius
				sd.followRadius += sd.followColliderRadiusMult * newLeader.generalMotionData.critterBoxColliderSize.z;
			}
			
			if (newLeader.isPlayer &&
				newLeader.followCount == 5)
			{
				AudioManager.PlaySFXAtObject(critter_info.critterObject, Vector3.zero, SoundFXID.Schooling);
				AudioManager.FadeInSFX(SoundFXID.Schooling, 4f);
			}
		}
		else
		{
			critter_info.generalSpeciesData.startSchooling = false;           
		}

		critter_info.generalSpeciesData.switchBehavior = true;
	}


	public static void SearchForNewLeader(CritterInfo critter_info)
	{
		//WemoLog.Log("searchNewLeaderCounter " + critter_info.critterObject.name);
		//DebugDisplay.AddDebugText("SearchForNewLeader "  + critter_info.critterObject.name);
		GeneralMotionData gmd = critter_info.generalMotionData;
		SwimSchoolFollowData sd = critter_info.swimSchoolFollowData; 
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		DataItem item_data = critter_info.itemData;
		
		if (sd == null
			|| sd.state != SwimSchoolFollowData.SchoolState.Automatic)
		{
			return;
		}

		float distSq = 100000f;
        Vector3 pos = critter_info.cachedPosition;
		CritterInfo bestLeaderCritter = null;

		bool schools_with_parents = true;
		if( !AppBase.Instance.RunningAsPreview() && item_data != null )
		{
			// Removing data-driven schooling for now. will bring back later. our API doesn't support it atm - RG
			/*
			schools_with_parents = ( item_data.variantSchoolingArray == null 
									 || item_data.variantSchoolingArray.Count <= 0 );
			*/
			schools_with_parents = true;
		}

        SimInstance instance = SimInstance.Instance;
		for(int i = 0 ;i<instance.crittersInPopulation; i++)
		{
			CritterInfo critter = instance.critters[i];
			CritterInfo leader = critter.swimSchoolFollowData.leaderCritterInfo;
            Vector3 leaderPos = critter.cachedPosition;

			if(leader!= null && leader.critterTransform != null)
			{
                leaderPos = leader.cachedPosition;
			}
			else
			{
				leader = critter;
			}

			if( critter == critter_info 
				|| leader == critter_info
				|| critter.generalSpeciesData.isExitingScene
				|| leader.generalSpeciesData == null)
			{
				continue;
			}

			if( !leader.generalSpeciesData.isExitingScene
				&& leader.generalSpeciesData.myCurrentBehaviorType == SwimBehaviorType.SWIM_FREE 
				&& !leader.swimFreeData.outside ) 
			{
				bool can_school_with_me = false;				
				// default is to school with our parent class. ie ok to school with all clownfish.
				if( schools_with_parents ) {
					if( !AppBase.Instance.RunningAsPreview() )
					{
						if( item_data != null && critter.itemData != null && item_data.parentid == critter.itemData.parentid ) {
                            can_school_with_me = true;
						}
					}
					else {
						if( critter_info.critterObject.name.Equals( critter.critterObject.name ) ) {
							can_school_with_me = true;
						}
                    }
				}
	
				if( can_school_with_me ) {
					Vector3 diff = leaderPos - pos;
					float diffMagSq = diff.sqrMagnitude;
					if(diffMagSq<distSq)
					{
						distSq = diffMagSq;
						bestLeaderCritter = critter;
					}	
				}
			}
		}

        float size = gmd.critterBoxColliderSize.z * 16f;
        if (bestLeaderCritter != null &&
		   gsd.myCurrentBehaviorType != SwimBehaviorType.SWIM_SCHOOL_FOLLOW)
		{
			if(distSq < size * size) 
			{
				SetNewLeader(critter_info, bestLeaderCritter);
			}
		}
		else if(distSq > size * size)// break leadership
		{
			gsd.switchBehavior = true;
		}	
	}

	private static void SwitchToBehavior( CritterInfo critter, SwimBehaviorType btype ) 
	{
		SwimBehaviorType currentBehaviorType = critter.generalSpeciesData.myCurrentBehaviorType;

		if( btype == currentBehaviorType ) 
		{
			return;
		}

		//WemoLog.Eyal( critter.generalSpeciesData.myCurrentBehaviorType +  " switching to new behavior " + btype);
		
		//DebugDisplay.AddDebugText("SwitchToBehavior "  + critter.critterObject.name);
		
		staticBehaviors[(int)currentBehaviorType].End(critter);
		
		critter.generalSpeciesData.myCurrentBehaviorType = btype;	
		critter.generalSpeciesData.myCurrentBehavior = staticBehaviors[(int)btype];	
		staticBehaviors[(int)btype].Start(critter);
		ResetBehaviorFlags(critter);
	}
	
	
}
