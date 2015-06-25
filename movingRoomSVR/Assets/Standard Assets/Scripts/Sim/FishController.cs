using UnityEngine;
using System.Collections;

[System.Serializable]

public class FishController {
		
	//private static BehaviorBase[] staticBehaviors;
	// one time initialization
	public static void Construct() {

	}
	
	
	// per fish initialization
	public static void Start( CritterInfo critter_info, float customScale ) {	
		// initialize any pieces
		if( critter_info.critterLODData ) {
			LODManager.InitLOD( critter_info.critterLODData );
		}

		if( critter_info.critterBendData ) {
			FishBendController.Init( critter_info );
		}

		if( critter_info.critterEyeData ) {
			EyeController.Init( critter_info );
		}

        float scaleMult = customScale > 0.001f ? customScale : Random.value*0.8f+0.6f;
		// randomize scale per critter, unless we have a custom scale
		//critter_info.critterTransform.localScale = new Vector3(critter_info.critterTransform.localScale.x *(rdm),critter_info.critterTransform.localScale.y *(rdm),critter_info.critterTransform.localScale.z *(rdm));
        critter_info.critterTransform.localScale *= (scaleMult);
			
		// setup critterInfo in GeneralSpeciesData
		critter_info.generalSpeciesData.myCritterInfo = critter_info;
		critter_info.generalSpeciesData.switchBehavior = false;
		critter_info.generalSpeciesData.swimToPoint = false;
		
		// we need our z setup
		GeneralMotion.Start( critter_info );		
				
		critter_info.animBase.Init();
		
		// here we add DisperseCollision.cs to triggerPredatorCollision
		int count = critter_info.critterTransform.childCount;
		for(int i=0; i<count; i++)
		{
			Transform child = critter_info.critterTransform.GetChild(i);
			if(child.name == "triggerPredatorCollision")
			{
                AwarenessCollision ac = child.gameObject.AddComponent<AwarenessCollision>() as AwarenessCollision;
                critter_info.awarenessCollision = ac;
                ac.myCritterInfo = critter_info;
                
                child.gameObject.AddComponent<DisperseCollision>();
				child.gameObject.AddComponent<Rigidbody>();
				Rigidbody rb = child.gameObject.GetComponent<Rigidbody>();
				rb.isKinematic = true;
				rb.useGravity = false;
			}
		}
		
        // if our custom scale is very diff, reset species size to new collider size scaled in GeneralMotion.Start()
        if (customScale > 0f && 
            (customScale >= 1.5f || customScale <= 0.5f))
        {
            critter_info.generalSpeciesData.mySize = SimInstance.GetSpeciesSize(critter_info.generalMotionData.critterBoxColliderSize.z);            
        }

		//DebugDisplay.AddDebugText("SETUP LE GLOW SPHERE");
/*		Transform glow = critter_info.critterTransform.FindChild("leGlow");
		if( glow ) {
			critter_info.generalSpeciesData.leGlowRenderer = glow.renderer;
			critter_info.generalSpeciesData.leGlowRenderer.enabled = false; // force it off
		}*/
		
        // cache our data for first frame.
        critter_info.cachedPosition = critter_info.critterTransform.position;
        critter_info.cachedRotation = critter_info.critterTransform.rotation;
        critter_info.cachedForward = critter_info.critterTransform.forward;
        critter_info.cachedUp = critter_info.critterTransform.up;
        critter_info.cachedRight = critter_info.critterTransform.right;
        critter_info.cachedEulers = critter_info.cachedRotation.eulerAngles;
	}
	
	// spawn in fishbowl or behind current camera.
	public static bool ShouldWeSpawnInFishBowl( CritterInfo critter ) {
		
		// yes, if in intro camera.
		FishBowlData my_fish_bowl = critter.generalSpeciesData.fishBowlData;
		if( CameraManager.CurrentState() == CameraState.Intro 
		   && critter.generalSpeciesData.fishBowlData != null ) {
			return true;
		}
		
		// no, if we are in a normal camera.
		if( !CameraManager.CurrentCameraFollowsTargets() ) {
			return false;
		}
		
		// we are in a follow camera now..		
		CritterInfo f_critter = SimManager.GetCritterForCameraTarget();
		if( f_critter == null ) {
			return true;
		}
		
		// no, if new critter fishbowl is the same fish bowl as the fish we are following..
		if( my_fish_bowl == f_critter.generalSpeciesData.fishBowlData ) {
			return false;
		}
		
		// yes otherwise we could add some more logic here.. but whatever.
		return true;
	}
	
	public static void FindSpawnPosition(CritterInfo critter_info, bool newly_aquired )
	{
        float rad = critter_info.generalMotionData.critterBoxColliderRadius;
        Vector3 new_pos = Vector3.zero;
        RaycastHit hit;
        Ray new_ray;
        float move_mag;

		if( newly_aquired ) {
			GameObject go = GameObject.Find("NewlyAquiredSpawnLocation");
			if( go != null ){
                
                Transform trans = go.transform;
                
                rad += 5f;
                int amount = App.SphereManager.spawnOwnedIDs.Count - 1;
                Vector3 offset = Vector3.zero;
                if( amount > 0 ) {
                    offset = Random.insideUnitSphere;
                    float dot = Vector3.Dot( trans.forward, offset );
                    if( dot > 0f ) {
                        offset *= -1f;
                        dot *= -1f;
                    }
                    if( dot < -.707f ) { // back 45 degrees. swap to get a sideways dir
                        Vector3 tmp = offset;
                        offset.x = tmp.z;
                        offset.z = tmp.x;
                    }
                    offset *= ( rad * ( amount + 1 ) );
                }
                
				Vector3 offset_back = trans.forward * -critter_info.generalMotionData.critterBoxColliderRadius;
				new_pos = trans.position + offset_back + offset;
                if( new_pos.y > -rad ) {
                    Vector3 tmp = new_pos;
                    tmp.y = -rad;
                    new_pos = tmp;
                }
                new_ray = new Ray( new Vector3( new_pos.x, 0, new_pos.z ), Vector3.down );
                move_mag = new_pos.y * -1;
                if(  Physics.SphereCast(new_ray,rad,out hit,move_mag,1<<14) ) {
                    new_pos = hit.point + new Vector3(0f,rad + 0.02f,0f);
                }      
                Quaternion rot = trans.rotation;
                critter_info.critterTransform.position = new_pos;
				critter_info.critterTransform.rotation = rot;
				critter_info.critterSteering.desiredRotation = rot;
				return;
			}
		}

		int layerMask = 1<<11|1<<12|1<<13;
		
		float mul = rad > 100.0f ? 1f : 4f;
		rad *= mul;
		Transform camXform = CameraManager.GetCurrentCameraTransform();
		GeneralMotionData gmd = critter_info.generalMotionData;
		bool foundSpawnPt = false;
		int testIter = 0;
		
//		int population =  OceanSphereController.GetCrittersInPopulation();
//		WemoLog.Eyal( population + " " + critter_info.critterObject.name);
		
		Vector3 use_point = gmd.spawnPointCameraSpace;
		bool transform_point = true;

		if( CameraManager.IsInIntroCamMode() 
		   || CameraManager.IsInTravelCamera() ) 
        {
			FishBowlData bd = critter_info.generalSpeciesData.fishBowlData;
            if (bd != null)
            {
//				DebugDisplay.AddDebugText(" intro fish bowl " +  bd.name + " :: p " + bd.position + " :: r "  + bd.rotation + " :: s " + bd.size);
    			Vector3 rand_spot = RandomExt.VectorRange(bd.size);
	    		use_point = bd.position + ( bd.rotation * rand_spot );
		    	transform_point = false;
            }
		}
		else
        {//if( ShouldWeSpawnInFishBowl(critter_info) ) {
//			WemoLog.Eyal("fishBowl location " +  critter_info.generalSpeciesData.fishBowlData.name);
			FishBowlData bd = critter_info.generalSpeciesData.fishBowlData;
            if (bd != null)
            {
    			Vector3 rand_spot = RandomExt.VectorRange(bd.size);
    			use_point = bd.position + ( bd.rotation * rand_spot );
    			Vector3 camera_vec = use_point - CameraManager.GetCurrentCameraPosition();
    			camera_vec.Normalize();
    			if( Vector3.Dot( CameraManager.GetCurrentCameraForward(), camera_vec ) > 0.707f ) {
    				GameObject go = GameObject.Find("NewlyAquiredSpawnLocation");
    				if( go != null ) {
    					use_point = go.transform.position;
    //					Debug.Log("USING PURCHASE SPAWN POINT TO SPAWN!!!!");
    				}	
    			}
    			transform_point = false;
            }
		}

		while(!foundSpawnPt)
		{
			Vector3 sp = use_point;
			
			// give a better distribution of points.. not all in a straight line!
			float rnd = Random.value;
			if( rnd < 0.25f ) {
				sp += new Vector3(-testIter * rad,0f,0f);
			}
			else if( rnd < 0.5f ) {
				sp += new Vector3(testIter * rad,0f,0f);
			}
			else if( rnd < 0.75f ) {
				sp += new Vector3(0f,0f,testIter * rad);
			}
			else {
				sp += new Vector3(0f,0f,-testIter * rad);
			}
		
			if( transform_point ) {
				new_pos =  camXform.TransformPoint( sp );
			}
			else {
				new_pos = sp;
			}
					
			new_ray = new Ray( new Vector3( new_pos.x, 0, new_pos.z ), Vector3.down );
			move_mag = new_pos.y * -1;
			if(  Physics.SphereCast(new_ray,rad,out hit,move_mag,1<<14) ) {
				new_pos = hit.point + new Vector3(0f,rad + 0.02f,0f);
			}
			new_pos.y = Mathf.Min(-rad,new_pos.y);
		
			int iter = Mathf.FloorToInt(-new_pos.y/rad);
			for(int i = 0; i < iter; i++)
			{
				new_ray = new Ray( new_pos, Vector3.up );
				move_mag = new_pos.y * -1;
				if( !Physics.SphereCast(new_ray,rad,out hit,move_mag,layerMask) ) 
				{
					critter_info.critterTransform.position = new_pos;	
					critter_info.critterTransform.rotation = CameraManager.GetCurrentCameraRotation();
					critter_info.critterSteering.desiredRotation = critter_info.critterTransform.rotation;
					foundSpawnPt = true;
					i=9999;
				}
				else
				{
					new_pos.y += rad;
				}
			}
			testIter++;
		}
	}
	
	// Update is called once per frame
	public static void Update( CritterInfo critter_info ) 
	{
//        Debug.Log("whale pos : " + critter_info.critterTransform.position);
        bool sim_is_paused = SimInstance.Instance.IsSimPaused();

        // ensure animations are speed appropriately
        if( critter_info.animBase.pauseActive != sim_is_paused ) {
            critter_info.animBase.pauseActive = sim_is_paused;
            float v = ( sim_is_paused ? 0f : ( SimInstance.Instance.slowdownActive ? SimInstance.slowdownMultiplierInv : 1f ) );
            foreach (AnimationState anim in critter_info.critterAnimation)
            {
                anim.speed = v;
            }
        }


        if( !sim_is_paused )
		{
            if( !critter_info.animBase.pauseSim ) {
                
                critter_info.lifetime += Time.deltaTime;

                //GeneralSpeciesData gsd = critter_info.generalSpeciesData;
                // save off our last frames position for our final collision check.
                critter_info.generalMotionData.lastPosition = critter_info.cachedPosition;
                
                
                AI.Update(critter_info);

                // Start lower level motion layer.  
				critter_info.generalSpeciesData.myCurrentBehavior.UpdateMotion( critter_info );

                // ensure animations are speed appropriately
                if( critter_info.animBase.slowdownActive != SimInstance.Instance.slowdownActive ) {
                    critter_info.animBase.slowdownActive = SimInstance.Instance.slowdownActive;
                    float mul = ( SimInstance.Instance.slowdownActive ? SimInstance.slowdownMultiplierInv : SimInstance.slowdownMultiplier );
                    foreach (AnimationState anim in critter_info.critterAnimation)
                    {
                        anim.speed *= mul;
                    }
                }

                critter_info.animBase.UpdateAnimation();
                // not using this anymore?
//                GeneralMotion.UpdateAvoidanceBox( critter_info);
            }
		}
		else
		{
			CritterAnimationBase.CycleThroughAnimations(critter_info);
		}
	}
	
	
	public static void LateUpdate( CritterInfo critter_info ) {
        bool paused = critter_info.animBase.pauseActive;

        // cache our data for rest of frame and next.
        if( !paused ) {
            critter_info.cachedPosition = critter_info.critterTransform.position;
            critter_info.cachedRotation = critter_info.critterTransform.rotation;
            critter_info.cachedForward = critter_info.critterTransform.forward;
            critter_info.cachedUp = critter_info.critterTransform.up;
            critter_info.cachedRight = critter_info.critterTransform.right;
            critter_info.cachedEulers = critter_info.cachedRotation.eulerAngles;

            if (critter_info.generalSpeciesData.myCurrentBehavior != null) {
    		    critter_info.generalSpeciesData.myCurrentBehavior.LateUpdate( critter_info );
            }
        }
		
		if( critter_info.critterBendData ) {
			FishBendController.UpdateBend( critter_info );
		}

		if( critter_info.critterEyeData ) {
			EyeController.UpdateEyes( critter_info );
		}

        if( !paused ) {
            critter_info.animBase.LateAnimationUpdate();
        }
    }
}
