using UnityEngine;
using System.Collections;

public class GeneralMotion 
{
	private static Transform cameraTransform = null;
//	private static FishBowl spawnFishBowl = null;
//	private static bool forceInViewToggle = false;
	
	//private bool drawGizmos = false;
	public static void SetDrawGizmos( bool draw, GeneralMotionData gmd )
	{
		gmd.drawGizmos = draw;
	}
	
    public static void SetAvoidanceDelays( CritterInfo critter_info )
    {
        GeneralMotionData gmd = critter_info.generalMotionData;
        gmd.avoidanceDelay = critter_info.masterIndex % ( gmd.avoidanceEveryNFrames + 1 );//Mathf.FloorToInt(20*Random.value);
        gmd.avoidanceGroundDelay = critter_info.masterIndex % ( gmd.avoidanceGroundEveryNFrames + 1 );
    }

	public static void Start(CritterInfo critter_info)
	{
//		GameObject spawnFishBowlObject = GameObject.Find("SpawnFishBowl");
//		spawnFishBowl = spawnFishBowlObject.GetComponent<FishBowl>();
		GeneralMotionData gmd = critter_info.generalMotionData;
		gmd.predatorOffsetMult =  0.5f + 0.5f*Random.value;
		gmd.isDispersed = false;
        gmd.myDisperseCritter = null;
        gmd.myDisperseXform = null;
        gmd.disperseRadius = 1.0f;
        SetAvoidanceDelays( critter_info );
        gmd.avoidanceBoxPreCalcRotationFactor = gmd.avoidanceRotationCheckFrames * Mathf.Deg2Rad;
        gmd.avoidGround = false;
		gmd.avoidFish = false;
		gmd.desiredDirNorm = new Vector3(0.0f,0.0f,1.0f);
		gmd.desiredDirNormGround = new Vector3(0.0f,0.0f,1.0f);
		//SphereCollider tmpc = critter_info.critterObject.GetComponent<SphereCollider>();
		//if( tmpc == null ) {
		//	tmpc = critter_info.critterObject.GetComponentInChildren<SphereCollider>();
		//}
		//gmd.halfSizeZ = tmpc.radius * 0.95f;
		//scale our collision values.
		gmd.critterBoxColliderSize = critter_info.critterBoxCollider.size * critter_info.critterTransform.localScale.z;
		gmd.critterBoxColliderVolume = gmd.critterBoxColliderSize.x * gmd.critterBoxColliderSize.y * gmd.critterBoxColliderSize.z;
		//WemoLog.Eyal(critter_info.critterObject.name + "  " + gmd.critterBoxColliderVolume);
		//gmd.critterBoxColliderRadius = 0.5f * Mathf.Sqrt(gmd.critterBoxColliderSize.x * gmd.critterBoxColliderSize.x + gmd.critterBoxColliderSize.y * gmd.critterBoxColliderSize.y);
		gmd.critterBoxColliderRadius = 0.5f * Mathf.Max(gmd.critterBoxColliderSize.x , gmd.critterBoxColliderSize.y );
		gmd.layerMask = 1 << 11 | 1 << 12 | 1 << 13;
		Transform[] xforms = critter_info.critterObject.GetComponentsInChildren<Transform>();
        for( int i = 0; i < xforms.Length; i++ ) {
            Transform xform = xforms[i];
			GameObject child = xform.gameObject;
			if(child.name == "fishAvoidanceBox") gmd.fishAvoidanceXform = xform;
		}
	}

    static System.Collections.Generic.List<RaycastHit> s_allFishHits = new System.Collections.Generic.List<RaycastHit>();
    static System.Collections.Generic.List<Rigidbody> s_allRBHits = new System.Collections.Generic.List<Rigidbody>();

	public static void Avoidance(CritterInfo critter_info)
	{
//		Profiler.BeginSample("Avoidance");
		//WemoLog.Eyal("avoidance " + critter_info.critterObject.name);
		GeneralMotionData gmd = critter_info.generalMotionData;
			
		//GeneralSpeciesData gsd = critter_info.generalSpeciesData;
        gmd.avoidanceDelay--;

        if (gmd.avoidanceDelay < 0)
        {
            gmd.avoidanceDelay = 0;
        }

		gmd.desiredDirFrameOutCount--; 

		if( gmd.desiredDirFrameOutCount < 0 ) 
        {
			gmd.desiredDirFrameOutCount = 0;
		}

		Vector3 fwd = critter_info.cachedForward;
        Vector3 right = critter_info.cachedRight;
        Vector3 up = critter_info.cachedUp;
        Vector3 pushVector = Vector3.zero;
		
		float distToCheckOrig = 100000f;
        int layerMask = gmd.useAvoidanceFish ? gmd.layerMask : 1 << 22; // fish + player or playerbodymask (diff collision but easier to hack in)

		if(gmd.avoidanceDelay%gmd.avoidanceEveryNFrames==0 || gmd.isDispersed || gmd.avoidFish)
		{	
            gmd.avoidanceDelay = gmd.avoidanceEveryNFrames;
			// assume we are not colliding anymore.
			gmd.avoidFish = false;

            float schoolAvoidPushMult = 1f;
            
            if (critter_info.generalSpeciesData.myCurrentBehaviorType == SwimBehaviorType.SWIM_SCHOOL_FOLLOW &&
                critter_info.swimSchoolFollowData != null)
            {
                schoolAvoidPushMult = critter_info.swimSchoolFollowData.schoolAvoidPushMult;
            }

            float halfLengthZ = gmd.critterBoxColliderSize.z*0.5f;
//				Debug.Log("ON" + critter_info.critterObject.name);
			Vector3 frontPt = critter_info.cachedPosition + fwd * halfLengthZ;
            Vector3 backPt = critter_info.cachedPosition - fwd * halfLengthZ;
				
			float totalCheckFrames = gmd.avoidanceForwardCheckFrames + gmd.avoidanceGroundEveryNFrames;
			float timeToRotate = Mathf.PI/2/critter_info.critterSteering.GetMaxYawSpeed();
			float distToRotate = timeToRotate * gmd.currentSpeed;
            float distToCheck = distToRotate + gmd.currentSpeed * SimManager.avgDeltaTime * totalCheckFrames + halfLengthZ;
			distToCheckOrig = distToCheck;
			float x = distToCheck * 0.15f * Mathf.Sin(critter_info.critterSteering.GetYawSpeed() * SimManager.avgDeltaTime * totalCheckFrames * Mathf.Deg2Rad);
			float y = distToCheck * 0.15f * Mathf.Sin(critter_info.critterSteering.GetPitchSpeed() * SimManager.avgDeltaTime * totalCheckFrames * Mathf.Deg2Rad);
		
			Vector3 dirCheck = critter_info.critterTransform.TransformDirection(new Vector3(x,-y,distToCheck));
			gmd.heading = dirCheck;
			dirCheck.Normalize();

#if UNITY_EDITOR
            //if(gmd.drawGizmos) Debug.DrawLine(critter_info.critterTransform.position,critter_info.critterTransform.position + dirCheck,Color.white); 
            Debug.DrawLine(critter_info.cachedPosition,critter_info.cachedPosition + (dirCheck * distToCheck), Color.white);
#endif
			
            s_allFishHits.Clear ();
            s_allRBHits.Clear ();

            //Physics.RaycastAll(critter_info.critterTransform.position, dirCheck, distToCheck+halfLengthZ, layerMask);  
            RaycastHit[] hits = Physics.RaycastAll(frontPt+up*(gmd.critterBoxColliderSize.y*0.25f), dirCheck, distToCheck, layerMask);  
            AddFishHits(hits);

//              hits = Physics.RaycastAll(frontPt-up*(gmd.critterBoxColliderSize.y*0.25f), dirCheck, distToCheck, layerMask );            
//              AddFishHits(hits, s_allFishHits);

            if (critter_info.generalSpeciesData.mySize != SpeciesSize.TINY)
            {
                hits = Physics.RaycastAll(backPt-up*(gmd.critterBoxColliderSize.y*0.45f) - fwd*0.25f, dirCheck, distToCheck+gmd.critterBoxColliderSize.z*1.25f, layerMask);  
                AddFishHits(hits);
            }
//              hits = Physics.RaycastAll(backPt+up*(gmd.critterBoxColliderSize.y*0.45f) - fwd*0.25f, dirCheck, distToCheck+gmd.critterBoxColliderSize.z*1.25f, layerMask);  
//              AddFishHits(hits, s_allFishHits);

            hits = Physics.RaycastAll(frontPt+ (dirCheck*distToCheck), -fwd, distToCheck+gmd.critterBoxColliderSize.z*2f, layerMask);  
            AddFishHits(hits);

            for(int i = 0; i<s_allFishHits.Count; i++)
            {       
                RaycastHit hit = s_allFishHits[i];

                if (hit.transform.gameObject == critter_info.critterObject)
                {
                    continue;
                }

#if UNITY_EDITOR
                Debug.DrawLine(critter_info.cachedPosition,hit.point,Color.green);
#endif

                bool isPlayer = false;
                GeneralSpeciesData hitgsd = hit.transform.gameObject.GetComponent<GeneralSpeciesData>();
                GeneralMotionData hitgmd = null;
                Vector3 hitHeadingNorm = Vector3.forward;
                Transform hitTransform = hit.transform;
                Vector3 hit_push = Vector3.zero;
                
                // gibs or other objects that don't have species data
                if (hitgsd == null)
                {
                    isPlayer = hit.transform.gameObject.GetComponent<OVRCameraController>() != null;
                    
                    if (isPlayer)
                    {
                        hitHeadingNorm = CameraManager.GetCurrentCameraForward();
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    hitgmd = hitgsd.myCritterInfo.generalMotionData;
                }
                
                Vector3 hitdiff = critter_info.cachedPosition - hit.point;
                float hitdiffMag = hitdiff.magnitude;
                
                bool bHitNeedsAvoid = hitgmd != null || isPlayer;
                
                // dealing with a fish, not the player?
                if (hitgsd != null && hitgmd != null)
                {
                    hitHeadingNorm = hitgmd.heading.normalized;
                    
                    // if idleMotion poke the fish to wake him up.
                    if(hitgsd.myCurrentBehaviorType == SwimBehaviorType.SWIM_IDLE
                       && hitdiffMag < (gmd.critterBoxColliderRadius + hitgmd.critterBoxColliderRadius))
                    {
                        CritterInfo hitCI =  hitgsd.myCritterInfo;
                        SwimIdle.pokeIdle(hitCI, critter_info);
                    }
                    
                    if (hitgmd.critterBoxColliderVolume < gmd.critterBoxColliderVolume * gmd.avoidFishLargerThanMyVolumeRatio)
                    {
                        bHitNeedsAvoid = false;
                    }
                }
                
                // only consider if fish is bigger than you
                if(bHitNeedsAvoid)
                {   
                    gmd.desiredDirFrameOutCount = gmd.avoidanceEveryNFrames;
                    critter_info.critterSteering.desiredSteeringThrottle += 0.3f;
                    gmd.avoidFish = true;
                    gmd.avoidanceDelay = 13;
                    Vector3 headingNorm = gmd.heading.normalized;
                    Vector3 my_pos = critter_info.cachedPosition;
                    Vector3 other_pos = hitTransform.position;
                    float facingRatio = Vector3.Dot(headingNorm, hitHeadingNorm); // fish to fish heading
                    Vector3 body_dir = other_pos - my_pos;                                          
                    bool bInSchool = schoolAvoidPushMult != 1f && hitgsd != null && critter_info.IsInSchool(hitgsd.myCritterInfo);
                    
                    // drift up and down accordingly
                    if( body_dir.y > 0 ) {
                        // hit is above us, turn down.
                        hit_push -= up * 0.5f;// * 0.05f;                           
                    }
                    else {
                        // hit is below us, turn up.
                        hit_push += up * 0.5f;// * 0.05f;                           
                    }
                    
                    body_dir.Normalize();
                    float forward_dot = Vector3.Dot(fwd, body_dir);
                    if( !bInSchool && forward_dot > 0f ) 
                    {
                        gmd.desiredSpeed *= gmd.avoidFishForwardDesiredSpeedMult;
                    }
                    
                    if(facingRatio > 0.707) // moving in the same general direction
                    {                           
                        // turn based on the hit being on our right vs left.
                        float body_side = Vector3.Dot(right, body_dir);
                        if( body_side < 0.0f ) {// always turn a little.
                            hit_push -= right * 0.8f;
                        }
                        else {
                            hit_push += right * 0.8f;
                        }
                        //                          Debug.Log("same :: " + body_side);
                    }                       
                    else 
                    {
                        
                        if (facingRatio < -0.707) // head on collision!
                        {
                            // turn based on whether we are the left or right side of the hit body
                            float body_side = Vector3.Dot(hitTransform.right,body_dir);
                            if( body_side < 0.05f && body_side > -0.05f ) {// always turn a little.
                                body_side = 0.05f;
                            }
                            //                              Debug.Log("headon :: " + body_side);
                            
                            hit_push += right * -body_side;
                        }
                        else // side crossing
                        {
                            // turn based on crossing left to right or vice versa
                            float crossing_dir = Vector3.Dot(right,hitTransform.forward);
                            
                            if( crossing_dir > 0f ) {
                                hit_push += ( right * 0.5f );
                            }
                            else {
                                hit_push -= ( right * 0.5f );
                            }
                        }
                    }
                    
                    if (bInSchool)
                    {
                        hit_push *= schoolAvoidPushMult;
                    }

                    if (hitgmd != null)
                    {
                        hitgmd.desiredDirFrameOutCount = hitgmd.avoidanceEveryNFrames;
                        hitgmd.desiredDirNorm -= hit_push * gmd.avoidanceFishPushFrameMult;     
                    }
                }

                if (isPlayer)
                {
                    hit_push *= 3f;
                }

                pushVector += hit_push;
            }

            s_allFishHits.Clear ();
            s_allRBHits.Clear ();
		}
		else
        {
            gmd.avoidFish = false;		
        }

		gmd.desiredDirNorm += pushVector;
		float ratio_out = gmd.desiredDirFrameOutCount / gmd.avoidanceEveryNFrames;
		gmd.desiredDirNorm *= ratio_out;
		//if(gmd.drawGizmos) 
#if UNITY_EDITOR
        //Debug.DrawLine(critter_info.cachedPosition,critter_info.cachedPosition + (gmd.desiredDirNorm * 300.0f),Color.red);
#endif
//			Debug.Log("FISH AVOID PUSH :: " + gmd.desiredDirNorm);

        // ugh, lower priority on fish avoidance when colliding with ground
        if (gmd.collidedWithGroundLastFrame)
        {
            gmd.desiredDirNorm *= 0.3f;
        }

		gmd.desiredVelocityDirection += gmd.desiredDirNorm;
		float speedMult = 1f - MathfExt.Fit(pushVector.magnitude, 0f, distToCheckOrig * 4f, 0f, 0.5f);
		//WemoLog.Eyal(critter_info.critterObject.name + " speedMult " + speedMult + " pushVector " + pushVector.magnitude);
		gmd.desiredSpeed *= speedMult;
		
        // BV: ground avoidance seems to be broken - all schooled fish that are following a leader swim only at the bottom of their fish bowl.
        // possibly there is a ocean height hardcoded value somewhere?
        //if(gmd.useAvoidanceGround)
        //{
        //    GeneralMotion.AvoidanceGround(critter_info);
        //}
//		Profiler.EndSample();
	}

    static void AddFishHits(RaycastHit[] hits)
    {
        for (int i=0; i<hits.Length; ++i)
        {
            RaycastHit hit = hits[i];
            Rigidbody hitRB = hit.rigidbody;
            
            bool bFound = false;
            int numHits = s_allRBHits.Count;
            for (int j=0; j<numHits && !bFound; ++j)
            {
                if (hitRB == s_allRBHits[j])
                {
                    bFound = true;
                }
            }
            
            if (!bFound)
            {
                s_allFishHits.Add(hit);
                s_allRBHits.Add(hitRB);
            }
        }
    }

	public static void AvoidanceGround(CritterInfo critter_info)
	{
//Profiler.BeginSample("AvoidanceGround");
		GeneralMotionData gmd = critter_info.generalMotionData;
		GeneralSpeciesData gsd = critter_info.generalSpeciesData;

		Vector3 fwd = critter_info.cachedForward;
//unused		Vector3 right = critter_info.critterTransform.right;
		Vector3 up = critter_info.cachedUp;

		RaycastHit hit;	

		float lastSpeed = gmd.lastFrameVelocity.magnitude;
		Vector3 testNorm;
		if( lastSpeed < 0.1f ) {
			testNorm = fwd;
		}
		else {
			testNorm = gmd.lastFrameVelocity * ( 1f/lastSpeed);
			testNorm += fwd;
			testNorm.Normalize();
		}

		float totalCheckFrames = gmd.avoidanceForwardCheckFrames + gmd.avoidanceGroundEveryNFrames;
		float timeToRotate = Mathf.PI/2/critter_info.critterSteering.GetMaxYawSpeed();
		float distToRotate = timeToRotate * gmd.currentSpeed  ;
		float distToCheck = distToRotate + gmd.currentSpeed * Time.deltaTime * totalCheckFrames + gmd.critterBoxColliderSize.z * 0.5f;

		if( gmd.avoidanceGroundDelay == gmd.avoidanceGroundEveryNFrames-1 || gmd.avoidGround )
		{
            Vector3 pos = critter_info.cachedPosition;
			gmd.avoidanceGroundDelay = 0;
			bool _hit = false;
			Vector3 hit_pos = Vector3.zero;
			Vector3 hit_normal = Vector3.zero;
			if( critter_info.generalSpeciesData.airIncrement == 0f ) {
				Vector3 end_pt = pos + (testNorm * distToCheck );
				if( end_pt.y > -gmd.critterBoxColliderRadius ) {
					// there is probably a better way to do this..
					float ratio = ( end_pt.y + gmd.critterBoxColliderRadius ) / ( end_pt.y - pos.y );
					float sdist = distToCheck * ratio;
					Vector3 tmp = pos + (testNorm * sdist);
					hit_pos = new Vector3(tmp.x, 0f, tmp.z);
					hit_normal = Vector3.down;
					_hit = true;
				}
			}
			if(!_hit && Physics.SphereCast(pos,gmd.critterBoxColliderRadius,testNorm,out hit, distToCheck , 1<<14)) {
				hit_pos = hit.point;					
				hit_normal = hit.normal;
				_hit = true;
			}
			if( _hit )
			{
				float fdot = Vector3.Dot(hit_normal,fwd);
				Vector3 right_push = Vector3.zero;
				// we are heading nearly right into something. turn a little to the left or right accordingly

                if (fdot > 0.7f)
                {
                    hit_normal = -hit_normal;
                    right_push = Vector3.Cross(up, hit_normal);
                    right_push.Normalize();
                }
                else if( fdot < -0.7f )
                {
					right_push = Vector3.Cross(up, hit_normal);
					right_push.Normalize();
				}				

				if( fdot < 0 ) {
					fdot *= -1;
				}
				
				Vector3 tmpDesiredDir = gmd.desiredDirNormGround;
				Vector3 reflect;
				reflect = Vector3.Reflect(testNorm,hit_normal);
				Vector3 origReflect = reflect;
				gmd.desiredDirNormGround = reflect;

                float avoidGroundPushFrameMult = gmd.avoidanceGroundPushFrameMult;
                if (gmd.stuckCount > 3)
                {
                    avoidGroundPushFrameMult *= MathfExt.Fit (gmd.stuckCount, 3, 120, 1f, 5f);
                }

                gmd.desiredVelocityDirection += right_push * (avoidGroundPushFrameMult * 0.9f) + hit_normal * (avoidGroundPushFrameMult * fdot);

				if(gsd.myCurrentBehaviorType != SwimBehaviorType.SWIM_FREE 
				   && gsd.myCurrentBehaviorType != SwimBehaviorType.SWIM_STRAFING)
				{			
					// move the target up . gross.
					if(Vector3.Dot(fwd,critter_info.swimTargetedData.targetPosition - pos) > 0.9f) 
					{
						critter_info.swimTargetedData.targetPosition += Vector3.up * gmd.critterBoxColliderRadius * 2f;
						critter_info.swimTargetedData.targetPosition.y = Mathf.Min(critter_info.swimTargetedData.targetPosition.y, -gmd.critterBoxColliderRadius * 2f);
					}
					if(Vector3.Dot(fwd,critter_info.swimToPointData.targetPosition - pos) > 0.9f) 
					{
						critter_info.swimToPointData.targetPosition += Vector3.up * gmd.critterBoxColliderRadius * 2f;
						critter_info.swimToPointData.targetPosition.y = Mathf.Min(critter_info.swimToPointData.targetPosition.y, -gmd.critterBoxColliderRadius * 2f);
					}
				}					

                critter_info.critterSteering.desiredSteeringThrottle += 0.4f;
                gmd.avoidGround = true;

#if UNITY_EDITOR
                //Debug.DrawLine(hit_pos,critter_info.cachedPosition,Color.red);
				//Debug.DrawLine(hit_pos,hit_pos + gmd.desiredVelocityDirection * distToCheck ,Color.green);

				if ( DebugInputHandler.DO_AVOIDANCE_DEBUG_OUTPUT ) {
					DebugDisplay.AddDebugText("Time of collision        " + Time.time);
					DebugDisplay.AddDebugText("dist checked             " + distToCheck);
					Vector3 distCol = hit_pos - critter_info.critterTransform.position;
					DebugDisplay.AddDebugText("dist to collision point  " + distCol.magnitude);
					float delta = Vector3.Dot(tmpDesiredDir.normalized,reflect.normalized);
					float acos = Mathf.Acos(delta);
					float deg = Mathf.Rad2Deg * acos;
					DebugDisplay.AddDebugText("delta reflect vector ang " + deg);
					DebugDisplay.AddDebugText("ct angSpeed max angSpeed " + critter_info.critterSteering.GetPitchSpeed()*Time.deltaTime + " " + gmd.maxSpeed*Time.deltaTime);
					DebugDisplay.AddDebugText("ct angSpeed max angSpeed " + critter_info.critterSteering.GetYawSpeed()*Time.deltaTime + " " + gmd.maxSpeed*Time.deltaTime);
					DebugDisplay.AddDebugText("reflect modified-reflect " + origReflect + " " + reflect);
				}		
#endif
			}
			else {
				gmd.avoidGround = false;
			}
		}

		gmd.avoidanceGroundDelay ++;
//		Profiler.EndSample();
	}

	public static void UpdateMotion(CritterInfo critter_info, bool do_rotation )
	{		
//		Profiler.BeginSample ("UpdateMotion");
		GeneralMotionData gmd = critter_info.generalMotionData;
		
		//if(critter_info.generalSpeciesData.myCurrentBehaviorType == SwimBehaviorType.SWIM_DISPERSE)  WemoLog.Eyal("updatemotion " + critter_info.critterObject.name + " " + gmd.desiredSpeed + " " + gmd.desiredVelocityDirection );

		gmd.desiredVelocityDirection.Normalize();
		if( gmd.useAvoidance )
		{
			GeneralMotion.Avoidance(critter_info);
		}
		
		float mag = gmd.desiredVelocityDirection.magnitude;
		//if( MathfExt.Approx(mag,0f,0.01f) ) {
		if( mag < 0.01f) 
		{
			//this actually happens sometimes in disperse since we are doing double cross
//			DebugDisplay.AddDebugText("######### don't update motion , something is wrong ###########");
//			DebugDisplay.AddDebugText(" " + critter_info.critterObject.name + " " + critter_info.generalSpeciesData.myCurrentBehavior);
			gmd.desiredVelocityDirection = critter_info.cachedForward;
			mag = 1.0f;			
		}
		// normalize the desiredVelocityDirection
		
		float dt = Time.deltaTime;
        if( SimInstance.Instance.slowdownActive && critter_info.generalSpeciesData.myCurrentBehaviorType != SwimBehaviorType.SWIM_PLAYER_VIEW )
        {
            dt *= SimInstance.slowdownMultiplierInv;
        }

		gmd.desiredVelocityDirection *= 1f/mag;
		
		gmd.desiredVelocityDirection.y = Mathf.Clamp(gmd.desiredVelocityDirection.y,-0.5f,0.5f);
		// ee edit for valve
		//if(critter_info.cachedPosition.y > -60f && !critter_info.generalSpeciesData.airNeeded) gmd.desiredVelocityDirection.y= Mathf.Min(-0.3f,gmd.desiredVelocityDirection.y);
#if UNITY_EDITOR
        Debug.DrawLine(critter_info.cachedPosition , critter_info.cachedPosition + gmd.desiredVelocityDirection*2.5f*gmd.critterBoxColliderRadius , Color.black);
#endif
		//Quaternion desiredDir = Quaternion.LookRotation(desiredVelocityDirection.normalized);
		//critter_info.critterTransform.rotation = Quaternion.Slerp(critter_info.critterTransform.rotation, desiredDir,dt * smoothRotateMult);

		
		if( do_rotation ) {
			if(gmd.lockVelocityToHeading || gmd.avoidFish) critter_info.critterSteering.desiredRotation = Quaternion.LookRotation(gmd.desiredVelocityDirection);
			critter_info.critterSteering.SteerUpdate(dt);
		}

		float maxAcc = gmd.maxAcc;
		float desiredSpeed = gmd.desiredSpeed;
		float desiredAcc = (desiredSpeed - gmd.currentSpeed)/dt;

        ThrottledFishSteering tfs = critter_info.critterSteering as ThrottledFishSteering;
        if (tfs != null &&
            tfs.scriptOverride)
		{
            if (tfs.scriptOverrideTime > 0f)
            {
                tfs.scriptOverrideTime -= dt;
                tfs.scriptOverride = tfs.scriptOverrideTime > 0f;
            }

			desiredSpeed *= tfs.scriptThrottleMult;
            desiredAcc = tfs.scriptThrottleMult * (desiredSpeed - gmd.currentSpeed)/dt;
            maxAcc *= tfs.scriptThrottleMult;
			critter_info.critterSteering.desiredSteeringThrottle = 1.0f;
		}

		gmd.currentAcc = Mathf.Clamp(desiredAcc,-maxAcc,maxAcc);
		gmd.currentSpeed += gmd.currentAcc*dt;
		gmd.currentSpeed += gmd.turningSpeedAdjust;
		float use_speed = gmd.currentSpeed * SimInstance.Instance.speedMult;
		float dist = use_speed * dt;
		gmd.currentSpeed -= gmd.turningSpeedAdjust;
		
		Vector3 dir_motion = critter_info.critterTransform.rotation * new Vector3(0f,0f,dist);
		if( gmd.collidedWithGroundLastFrame ||
		   gmd.useVelocityDirection || 
		   ( !gmd.lockVelocityToHeading && gmd.turningSpeedAdjust == 0 ) ) {
			dir_motion = dist * gmd.desiredVelocityDirection;
		}

        Vector3 new_position = critter_info.cachedPosition + dir_motion;

		if( OceanCurrents.Singleton != null ) 
        {
            OceanCurrents.Singleton.UpdateCritter( critter_info, ref new_position );
		}
		
		if( gmd.useGroundSlide )
        {
            GeneralMotion.SlideCollision(critter_info, new_position);
		}		
        else
        {
            critter_info.critterTransform.position = new_position;
            critter_info.cachedPosition = new_position;
        }

//		Profiler.EndSample();
	}
	
	public static void UpdateAvoidanceBox( CritterInfo critter_info)
	{
        GeneralMotionData gmd = critter_info.generalMotionData;
        if (critter_info == null ||
            gmd.fishAvoidanceXform == null)
        {
            return;
        }
            
//		Profiler.BeginSample("UpdateAvoidanceBox");
		Vector3 s = gmd.critterBoxColliderSize;
		float zLength = s.z + gmd.currentSpeed * gmd.avoidanceBoxScaleTime;
		
		 
        float ff = SimManager.avgDeltaTime * gmd.avoidanceBoxPreCalcRotationFactor;
        float xFactor = Mathf.Sin(critter_info.critterSteering.GetYawSpeed() * ff);
        float yFactor = Mathf.Sin(critter_info.critterSteering.GetPitchSpeed() * ff);
        float abs_xfactor = xFactor;
        float xsign = 1f;
        if( xFactor < 0 ) {
            abs_xfactor *= -1f;
            xsign = -1f;
        }
        float abs_yfactor = yFactor;
        float minus_ysign = -1f;
        if( yFactor < 0 ) {
            abs_yfactor *= -1f;
            minus_ysign = 1f;
        }
		float xScale = 1f + s.z/s.x * abs_xfactor;
		float yScale = 1f + s.z/s.y * abs_yfactor; 
		float zScale = Mathf.Clamp(zLength / s.z, gmd.avoidanceBoxZMin, gmd.avoidanceBoxZMax);
		float xMove = 0.5f * (xScale * s.x - s.x) * xsign;
        float yMove = 0.5f * (yScale * s.y - s.y) * minus_ysign;
		float zMove = 0.5f * (zScale * s.z - s.z);
		//float zMove = 0.5f * zScale * s.z;
		gmd.fishAvoidanceXform.localScale = new Vector3(xScale,yScale,zScale);
		gmd.fishAvoidanceXform.localPosition = new Vector3(xMove,yMove,zMove);
		//WemoLog.Eyal(zLength + " "  + zScale + " " + zMove);
//		Profiler.EndSample();
	}

	public static void SetRandomTarget(CritterInfo critter_info, float minRandYaw, float maxRandYaw, float minRandPitch, float maxRandPitch)
	{
		//      WemoLog.Eyal("pick new target " + critter_info.critterObject.name + " isHungry " + gsd.isHungry + " switchBehavior " + gsd.switchBehavior);
		SwimTargetedData std = critter_info.swimTargetedData;
		GeneralMotionData gmd = critter_info.generalMotionData;
		FishBowlData bd = critter_info.generalSpeciesData.fishBowlData;

		//WemoLog.Eyal( critter_info.critterObject.name + " picked target in fishbowl " + bd.name);
//		Vector3 vec = critter_info.critterTransform.position - bd.position;

		float rnd_p = Random.Range(minRandPitch,maxRandPitch);
		float rnd_y = Random.Range(minRandYaw,maxRandYaw);
		
		Vector3 dr = critter_info.critterTransform.forward;
		dr[1] = 0f;
		dr.Normalize();
		
		dr = Quaternion.Euler(new Vector3(rnd_p,rnd_y,0f)) * dr;
		
		float rnd_r = Random.Range(4f,10f);
		float rad = gmd.critterBoxColliderRadius;
		Vector3 new_pos = critter_info.critterTransform.position + ( dr * rad * rnd_r ); 

		if (bd != null)
		{
			new_pos.x = Mathf.Clamp( new_pos.x, bd.position.x - bd.size.x/2f, bd.position.x + bd.size.x/2f);
			new_pos.y = Mathf.Clamp( new_pos.y, bd.position.y - bd.size.y/2f, bd.position.y + bd.size.y/2f);
			new_pos.z = Mathf.Clamp( new_pos.z, bd.position.z - bd.size.z/2f, bd.position.z + bd.size.z/2f);
		}

		//WemoLog.Eyal("precollision target " + critter_info.critterObject.name + " " + new_pos + " z " + gmd.critterBoxColliderSize.z);
				
		Vector3 dir = new_pos - critter_info.critterTransform.position;
		float testRadius = Mathf.Min(3000f, gmd.critterBoxColliderSize.z * 10f);
		if(dir.magnitude < testRadius)
		{
			Vector3 newdir = dir.normalized;
			if(newdir.y>0) newdir.y *=-0.1f;
            new_pos = critter_info.cachedPosition + newdir.normalized * testRadius;
		}
		//WemoLog.Eyal("too close to current " + critter_info.critterObject.name + " " + new_pos);
		
		// Start collision checks to pick a nice clean path to the target.
		
		
		RaycastHit hit;
        Vector3 pos = critter_info.cachedPosition;
		
		//first check from ocean surface (y=0) to new_pos 
		//ee edit for valve
		Vector3 posOcean = new Vector3(new_pos.x, GlobalOceanShaderAdjust.Instance.oceanTransform.position.y, new_pos.z);
		Vector3 diff = new_pos - posOcean;
		Ray new_ray = new Ray(posOcean, diff);
		float radiusMult = 2f; // this is the cross-section mult, which we use to multiply the radius of the fish
		
		if( Physics.SphereCast(new_ray,gmd.critterBoxColliderRadius * radiusMult,out hit,diff.magnitude + 50f,1<<14) )
		{
			new_pos = hit.point - new_ray.direction * gmd.critterBoxColliderRadius;
		}
		
		//WemoLog.Eyal("collision water " + critter_info.critterObject.name + " " + new_pos);
		
		// now check from current pos to new_pos
		diff = new_pos - pos;
		new_ray = new Ray(pos, diff);
		if( Physics.SphereCast(new_ray,gmd.critterBoxColliderRadius * radiusMult,out hit,diff.magnitude + 50f,1<<14) )
		{
			//new_pos = hit.point - new_ray.direction * gmd.critterBoxColliderSize.z;
			//if new_pos y is lower than fish.. set y to be same as fish
			if(new_pos.y < pos.y)
			{
				new_pos.y = pos.y;
				// now check to see if there's anything in between
				diff = new_pos - pos;
				new_ray = new Ray(pos, diff);
				if( Physics.SphereCast(new_ray,gmd.critterBoxColliderRadius * radiusMult,out hit,diff.magnitude + 50f,1<<14) )
				{
					new_pos = hit.point - new_ray.direction * gmd.critterBoxColliderSize.z * 0.5f;
				}
			}
			else //if not below the y and collision very close to the fish. pick a point 90 deg to the side.
			{
				// first pick a point between fish and collision ( not inside the collision)
				new_pos = hit.point - new_ray.direction * gmd.critterBoxColliderSize.z * 0.5f;
				diff = new_pos - pos;
				//is this point too close to fish?
				if(diff.magnitude < gmd.critterBoxColliderSize.z * 2f)
				{
					// pick a new point 90 deg off to the collision and farther away from the fish
					new_pos = pos + Vector3.Cross(diff.normalized,Vector3.up) * gmd.critterBoxColliderSize.z * 5f;
					if(new_pos.y < pos.y) new_pos.y = pos.y; // if lower than current y keep on the same y. cross product may place new_pos under current y
					diff = new_pos - pos;
					new_ray = new Ray(pos, diff);
					// and check again
					if( Physics.SphereCast(new_ray,gmd.critterBoxColliderRadius * radiusMult,out hit,diff.magnitude + 50f,1<<14) )
					{
						new_pos = hit.point - new_ray.direction * gmd.critterBoxColliderSize.z * 0.5f;
					}
				}
			}
		}        
		
		if(new_pos.y > -gmd.critterBoxColliderRadius * radiusMult) {
			new_pos.y = -gmd.critterBoxColliderRadius * radiusMult;
		}
		std.targetPosition = new_pos;
		std.isTargetFood = false;
        std.savedTargetDirection = new_pos - critter_info.cachedPosition;
		std.savedTargetDirection = std.savedTargetDirection.normalized;
		
		//WemoLog.Eyal("post target " + critter_info.critterObject.name + " " + new_pos);
	}

	public static void SetRandomTarget(CritterInfo critter_info)
	{
//		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
			
/*		if( DatabaseManager.GetTotalCollectedItems() <= 0
		   && !CameraManager.IsInScreensaverMode()) {
			forceInViewToggle = !forceInViewToggle;
			if( forceInViewToggle ) {
				force_in_view = true;
			}
		}*/
		
//		WemoLog.Eyal("pick new target " + critter_info.critterObject.name + " isHungry " + gsd.isHungry + " switchBehavior " + gsd.switchBehavior);
		SwimTargetedData std = critter_info.swimTargetedData;
		GeneralMotionData gmd = critter_info.generalMotionData;
		Vector3 new_pos;
		if( critter_info.generalSpeciesData.fishBowlData != null ) {
			// are we outside our fishbowl..
			FishBowlData bd = critter_info.generalSpeciesData.fishBowlData;
			//Critters use small fishbowl in front of Camera 1 if the player is just starting the game (IE: they own less than 4 species)

			//WemoLog.Eyal( critter_info.critterObject.name + " picked target in fishbowl " + bd.name);
            Vector3 vec = critter_info.cachedPosition - bd.position;
			// choose a random spot in the bowl
//			DebugDisplay.AddDebugText(" "+vec + bd.size);
			if( !DebugInputHandler.DO_NEW_RANDOM_TARGETING 
				|| ( Mathf.Abs(vec.x) > ( bd.size.x * 0.5f ) ) 
				|| ( Mathf.Abs(vec.z) > ( bd.size.z * 0.5f ) ) ) {
				Vector3 rand_spot = RandomExt.VectorRange(bd.size);
				new_pos = bd.position + ( bd.rotation * rand_spot );		
			}
			else {
//				DebugDisplay.AddDebugText("in front");
				float RandYaw = 40f;
				float RandPitch = 15f;
				float rnd_p = Random.Range(-RandPitch,RandPitch);
				float rnd_y = Random.Range(-RandYaw,RandYaw);
				
				Vector3 dr = critter_info.critterTransform.forward;
				dr[1] = 0f;
				dr.Normalize();
				
				dr = Quaternion.Euler(new Vector3(rnd_p,rnd_y,0f)) * dr;
				
				float rnd_r = Random.Range(4f,10f);
				float rad = gmd.critterBoxColliderRadius;
                new_pos = critter_info.cachedPosition + ( dr * rad * rnd_r ); 
				new_pos.x = Mathf.Clamp( new_pos.x, bd.position.x - bd.size.x/2f, bd.position.x + bd.size.x/2f);
				new_pos.y = Mathf.Clamp( new_pos.y, bd.position.y - bd.size.y/2f, bd.position.y + bd.size.y/2f);
				new_pos.z = Mathf.Clamp( new_pos.z, bd.position.z - bd.size.z/2f, bd.position.z + bd.size.z/2f);
			}
		}
		else {
			// dont use the camera transform if we are following
			if( !CameraManager.CurrentCameraFollowsTargets() || cameraTransform == null ) {
				cameraTransform = CameraManager.GetCurrentCameraTransform();
			}
			
			float randDist = std.randomTargetOffset.z + std.randomTargetCameraDistanceMin + std.randomTargetCameraDistanceMax * Random.value;
			float max_x = critter_info.generalMotionData.critterBoxColliderRadius * 5f;
			float max_y = critter_info.generalMotionData.critterBoxColliderRadius * 5f;
			Vector3 pre_pos = new Vector3(std.randomTargetOffset.x*(Random.value-0.5f),std.randomTargetOffset.y*(Random.value-0.5f),randDist + std.randomTargetOffset.z*(Random.value-0.5f));
			float new_x = pre_pos.x;
			if( Mathf.Abs(new_x) > max_x ) {
				new_x = ( new_x < 0f ) ? -max_x : max_x;
			}
			float new_y = pre_pos.y;
			if( Mathf.Abs(new_y) > max_y ) {
				new_y = ( new_y < 0f ) ? -max_y : max_y;
			}
			float new_z = pre_pos.z;
			if( new_z > max_x || ( new_z < critter_info.generalMotionData.critterBoxColliderRadius ) ) {
				new_z = max_x;
			}
			new_pos = cameraTransform.TransformPoint(new Vector3(new_x,new_y,new_z));
		}
		
		//WemoLog.Eyal("precollision target " + critter_info.critterObject.name + " " + new_pos + " z " + gmd.critterBoxColliderSize.z);
		
		
        Vector3 dir = new_pos - critter_info.cachedPosition;
		float testRadius = Mathf.Min(3000f, gmd.critterBoxColliderSize.z * 10f);
		if(dir.magnitude < testRadius)
		{
			Vector3 newdir = dir.normalized;
			if(newdir.y>0) newdir.y *=-0.1f;
			new_pos = critter_info.critterTransform.position + newdir.normalized * testRadius;
		}
		//WemoLog.Eyal("too close to current " + critter_info.critterObject.name + " " + new_pos);
		
		// Start collision checks to pick a nice clean path to the target.
		
		
		RaycastHit hit;
        Vector3 pos = critter_info.cachedPosition;
		
		//first check from ocean surface (y=0) to new_pos
		//ee edit for valve
		Vector3 posOcean = new Vector3(new_pos.x, GlobalOceanShaderAdjust.Instance.oceanTransform.position.y, new_pos.z);
		Vector3 diff = new_pos - posOcean;
		Ray new_ray = new Ray(posOcean, diff);
		float radiusMult = 2f; // this is the cross-section mult, which we use to multiply the radius of the fish
		
		if( Physics.SphereCast(new_ray,gmd.critterBoxColliderRadius * radiusMult,out hit,diff.magnitude + 50f,1<<14) )
		{
			new_pos = hit.point - new_ray.direction * gmd.critterBoxColliderRadius;
		}
	
		//WemoLog.Eyal("collision water " + critter_info.critterObject.name + " " + new_pos);
				
		// now check from current pos to new_pos
		diff = new_pos - pos;
		new_ray = new Ray(pos, diff);
		if( Physics.SphereCast(new_ray,gmd.critterBoxColliderRadius * radiusMult,out hit,diff.magnitude + 50f,1<<14) )
		{
			//new_pos = hit.point - new_ray.direction * gmd.critterBoxColliderSize.z;
			//if new_pos y is lower than fish.. set y to be same as fish
			if(new_pos.y < pos.y)
			{
				new_pos.y = pos.y;
				// now check to see if there's anything in between
				diff = new_pos - pos;
				new_ray = new Ray(pos, diff);
				if( Physics.SphereCast(new_ray,gmd.critterBoxColliderRadius * radiusMult,out hit,diff.magnitude + 50f,1<<14) )
				{
					new_pos = hit.point - new_ray.direction * gmd.critterBoxColliderSize.z * 0.5f;
				}
			}
			else //if not below the y and collision very close to the fish. pick a point 90 deg to the side.
			{
				// first pick a point between fish and collision ( not inside the collision)
				new_pos = hit.point - new_ray.direction * gmd.critterBoxColliderSize.z * 0.5f;
				diff = new_pos - pos;
				//is this point too close to fish?
				if(diff.magnitude < gmd.critterBoxColliderSize.z * 2f)
				{
					// pick a new point 90 deg off to the collision and farther away from the fish
					new_pos = pos + Vector3.Cross(diff.normalized,Vector3.up) * gmd.critterBoxColliderSize.z * 5f;
					if(new_pos.y < pos.y) new_pos.y = pos.y; // if lower than current y keep on the same y. cross product may place new_pos under current y
					diff = new_pos - pos;
					new_ray = new Ray(pos, diff);
					// and check again
					if( Physics.SphereCast(new_ray,gmd.critterBoxColliderRadius * radiusMult,out hit,diff.magnitude + 50f,1<<14) )
					{
						new_pos = hit.point - new_ray.direction * gmd.critterBoxColliderSize.z * 0.5f;
					}
				}
			}
		}
		
		
		if(new_pos.y > -gmd.critterBoxColliderRadius * radiusMult) {
			new_pos.y = -gmd.critterBoxColliderRadius * radiusMult;
		}
		std.targetPosition = new_pos;
		std.isTargetFood = false;
        std.savedTargetDirection = new_pos - critter_info.cachedPosition;
		std.savedTargetDirection = std.savedTargetDirection.normalized;
				
		//WemoLog.Eyal("post target " + critter_info.critterObject.name + " " + new_pos);
	}

	public static void SetSurfaceTarget(CritterInfo critter_info)
	{
		//WemoLog.Eyal("pick new target " + critter_info.critterObject.name);
		SwimTargetedData std = critter_info.swimTargetedData;
        Vector3 pos = critter_info.cachedPosition;
		Vector3 new_pos = pos + new Vector3(Random.Range(-2f,2f)* pos.y, 0f, Random.Range(-2f,2f)* pos.y);
		new_pos.y = 400f;
		
		std.targetPosition = new_pos;
		std.isTargetFood = false;
        std.savedTargetDirection = new_pos - critter_info.cachedPosition;
		
		
		//WemoLog.Eyal("post target " + critter_info.critterObject.name + " " + new_pos);
	}
	
	// WARNING> ONLY CALL ON STARTUP! 
	public static void SetCameraTarget(CritterInfo critter_info)
	{
		GeneralMotionData gmd = critter_info.generalMotionData;
		SwimTargetedData std = critter_info.swimTargetedData;
		Transform camXform = CameraManager.GetCurrentCameraTransform();
		float randDist = std.randomTargetCameraDistanceMin + std.randomTargetCameraDistanceMax * Random.value;
		Vector3 new_pos =  camXform.TransformPoint( new Vector3(Random.Range(-40f,40f),Random.Range(-40f,40f),randDist));
		RaycastHit hit;
		Vector3 test_pos;
		if( critter_info.generalSpeciesData.myCategory == SpeciesCategory.REEF ) {
            test_pos = critter_info.cachedPosition;
		}
		else {
			test_pos = new Vector3(new_pos.x, 0f, new_pos.z);
		}
		
		//if(critter_info.generalSpeciesData.mySpecies == SpeciesType.GREATWHITESHARK) WemoLog.Eyal(new_pos.y);
		
		Vector3 top = new_pos;
		top[1] = 0f;
		Vector3 diff = new_pos - top;
		Ray new_ray = new Ray(top, diff);
		
		if( Physics.SphereCast(new_ray,critter_info.generalMotionData.critterBoxColliderRadius * 2f,out hit,diff.magnitude,1<<14) )
		{
			//new_pos = top + new_ray.direction * ( hit.distance - 1f );
			new_pos = top + new_ray.direction * ( hit.distance -  gmd.critterBoxColliderSize.z);
		}
		
		//if(critter_info.generalSpeciesData.mySpecies == SpeciesType.GREATWHITESHARK) WemoLog.Eyal(new_pos.y);
		
		diff = new_pos - test_pos;
		new_ray.origin = test_pos;
		new_ray.direction = diff;	
		
		if( Physics.SphereCast(new_ray,critter_info.generalMotionData.critterBoxColliderRadius * 2f,out hit,diff.magnitude + 50f,1<<14) )
		{
			//new_pos = hit.point + (new_ray.direction * (-critter_info.generalMotionData.critterBoxColliderRadius));
			new_pos = new_ray.origin + new_ray.direction * ( hit.distance -  gmd.critterBoxColliderSize.z);
//			Debug.Log("camera TARGET collided" + hit.distance);
		}
		
		//if(critter_info.generalSpeciesData.mySpecies == SpeciesType.GREATWHITESHARK) WemoLog.Eyal(new_pos.y);
		// ee edit for valve
		//if(new_pos.y > -110f ) new_pos.y = -110f;
		std.targetPosition = new_pos;
		std.isTargetFood = false;
		critter_info.critterTransform.LookAt(new_pos);
		critter_info.critterSteering.desiredRotation = critter_info.critterTransform.rotation;
	}
	
	public static void SetBackwardTarget(CritterInfo critter_info)
	{
		GeneralMotionData gmd = critter_info.generalMotionData;
		Vector3 newTarget = critter_info.critterTransform.position - critter_info.critterTransform.forward * gmd.critterBoxColliderSize.z ;
		//newTarget += 0.2f * gmd.critterBoxColliderSize.z * new Vector3(Random.value - 0.5f,Random.value - 0.5f,Random.value - 0.5f);

		RaycastHit hit;
		
		Vector3 diff = newTarget - critter_info.critterTransform.position;
		Ray new_ray = new Ray(critter_info.critterTransform.position, diff);
		if( Physics.SphereCast(new_ray,gmd.critterBoxColliderRadius * 2f ,out hit,diff.magnitude ,1<<14) )
		{
			newTarget = new_ray.origin + new_ray.direction * (hit.distance - 0.02f);
		}
		critter_info.swimTargetedData.targetPosition = newTarget;
		critter_info.swimIdleData.targetPosition = newTarget;
		
		//WemoLog.Eyal("setBackwardTarget " + critter_info.critterObject.name );
	}

	public static void SlideCollision( CritterInfo critter_info, Vector3 moveToPos)
	{
//		Profiler.BeginSample ("SlideCollision");
//		GeneralSpeciesData gsd = critter_info.generalSpeciesData;
		GeneralMotionData gmd = critter_info.generalMotionData;

		// TODO::run our collision step if we are flagged to do so.. get avoidance to turn this block on and off..
        Vector3 move_amt = moveToPos - critter_info.cachedPosition;
		float use_radius = gmd.critterBoxColliderRadius;
		Vector3 norm = Vector3.zero;
		
// only used in SwimParking, behavior not active anymore
//        gsd.myCurrentBehavior.PreCollision( critter_info );

		if( CollisionHelpers.SphereSlide(critter_info.critterTransform,move_amt,use_radius,1<<14,2, ref norm) ) {
			gmd.collidedWithGroundLastFrame = true;
			Vector3 left = Vector3.Cross(norm,critter_info.cachedForward);
			Vector3 new_forward = Vector3.Cross(left, norm);
			critter_info.critterSteering.desiredRotation = Quaternion.LookRotation(new_forward,norm);
// TODO>offset the throttle.
//			GeneralMotion.SteeringControl(critter_info);
			//WemoLog.Eyal("slideCollision " + critter_info.critterObject.name);
		}// groundlayer = 14
		else {
			gmd.collidedWithGroundLastFrame = false;
		}

// only used in SwimParking, behavior not active anymore
//		gsd.myCurrentBehavior.PostCollision( critter_info );
		// TODO>>TEMP>make sure certain critters cannot leave the water.
		// ee edit for valve , 

        Vector3 new_pos = critter_info.critterTransform.position;

		if( critter_info.generalSpeciesData.airIncrement == 0f 
		   && critter_info.critterTransform.position.y > -use_radius + GlobalOceanShaderAdjust.Instance.oceanTransform.position.y ) 
        {
			new_pos.y = -use_radius + GlobalOceanShaderAdjust.Instance.oceanTransform.position.y;
            critter_info.critterTransform.position = new_pos;
		}

        critter_info.cachedPosition = new_pos;


        gmd.lastFrameVelocity = move_amt * (1f/Time.deltaTime);
//		DebugDisplay.AddDebugText("end " + gmd.lastFrameVelocity + " :: " + gmd.lastFrameVelocity.magnitude );
		
		if(gmd.collidedWithGroundLastFrame){
			float true_speed = gmd.lastFrameVelocity.magnitude;
			float speed_diff = Mathf.Abs(gmd.currentSpeed - (true_speed - gmd.turningSpeedAdjust));
			if( speed_diff > gmd.currentSpeed * 0.2f)
			{
				//WemoLog.Eyal("+++++++++++ gmd.collidedWithGroundLastFrame " + gmd.collidedWithGroundLastFrame);
				if( gmd.desiredSpeed > 0f && true_speed < gmd.desiredSpeed * 0.1f ) 
                {
					gmd.stuckCount++;

                    if (gmd.stuckCount == 2)
                    {
                        SetBackwardTarget(critter_info);
                    }

                    if (gmd.stuckCount > 2)
                    {
                        gmd.avoidanceGroundDelay = gmd.avoidanceGroundEveryNFrames-1;
                    }
				}
				else 
                {
					gmd.stuckCount = 0;
				}
			}
			else 
            {
				gmd.stuckCount = 0;
			}
		}
		
//		Profiler.EndSample ();
	}
}
