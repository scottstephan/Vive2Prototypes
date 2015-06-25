using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentInteractionManager : MonoBehaviour
{

	private static AgentInteractionManager singleton;
//	public float maxMouseMoveForClick = 25.0f;
//	private float maxMouseMoveForClickSqrd;
//	private Vector3 clickDownMousePosition;
//	private bool mouseClickUp;
//	private bool mouseClickDown;
//	private bool mouseClickDownHold;
	public ClickValidator clickValidator = new ClickValidator ();
    
	public static AgentInteractionManager Singleton {
		get { return singleton; }
	}
    
	void Start ()
	{

		singleton = this;

//		maxMouseMoveForClickSqrd = maxMouseMoveForClick * maxMouseMoveForClick;

//		mouseClickDownHold = false;		

	}
	
	#region Cast - Update
/*	void Update ()
	{

		if (!App.AppReady () 
		   || App.SphereManager.LEGACY_IsLoadingSphere ()
		   || CameraManager.IsInTravelCamera ()
		   || CameraManager.IsInIntroCamMode ()
           || CameraManager.IsInOculusMode() ) {
			return;
		}

		mouseClickUp = false;
		mouseClickDown = false;

		
		if (InputManager.GetKeyDown ("mouse 0")) {
			clickDownMousePosition = CameraManager.MousePosition;
			mouseClickDown = true;
			mouseClickDownHold = true;
		}

		if (Input.GetKeyUp ("mouse 0")) {
			mouseClickUp = true;			
			mouseClickDownHold = false;
			Vector2 clickPos = new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y);

			if (!clickValidator.IsInvalidClick (clickPos) &&
               !SyncManager.blockCasting &&
			   !SimInstance.isVerifyAssetMode &&
			   !App.OwnedFishManager.OwnershipMomentActive () &&
			   !CameraManager.IsInTravelCamera ()) {
				
				Vector3 move_amt = clickDownMousePosition - CameraManager.MousePosition;
				if (move_amt.sqrMagnitude < maxMouseMoveForClickSqrd) {
					AgentInteractionCast ();
				}
			}
		}
	}*/
	#endregion
	
	#region Cast - Result Logic
	void AgentInteractionCast ()
	{
		//The result of whether or not the collection cast hit a collectible
		bool agentInteractionCastResult = false;

		//Holds information on the collectible that was hit
		RaycastHit[] hits;

		//Get the current camera being used
		Camera currentCam = CameraManager.GetCurrentCamera ();

		//Caluculate the Ray from mousePosition into the Screen
		Ray tempRay = currentCam.ScreenPointToRay (CameraManager.MousePosition);

		//Cast a sphere along the precalculated ray to see if it collides with any Items (Fish, stationary agents)
		float radius = 0.1f;
		GameObject AgentObjectHit = null;
		float dist = GlobalOceanShaderAdjust.CurrentDistance ();
		float radius_max;
		float radius_increment;
		int use_mask;
		float closest_item_dist = dist * 2f;
		GameObject closest_item_hit = null;
		GeneralSpeciesData gsd = null;
		GeneralSpeciesData best_gsd = null;
		radius_max = 15.0f;
		radius_increment = 2.5f;
		

//		Debug.Log("CASTING...");
		//Cast a series of Spheres and see if they connect with valid objects in the sphere
//        float ok_dist_sqrd = (dist - 25f);
        float ok_dist_sqrd = dist;
        ok_dist_sqrd *= ok_dist_sqrd;
		while (radius < radius_max) {
			use_mask = 1 << 9 | 1 << 15;

			float sub_dist = 0f;
			float sub_inc = 50f;
			float sub_radius = radius;
			float sub_radius_inc = 5.75f;
			Ray sub_ray = tempRay;
			while (sub_dist < dist) {
				hits = Physics.SphereCastAll (sub_ray.origin, sub_radius, sub_ray.direction, sub_inc, use_mask);
				float best_dot = -2f;
				
				for (int i = 0; i<hits.Length; i++) {
					RaycastHit tm = hits [i];

					// pass if outside of our slop distance.
					if ((tm.point - tempRay.origin).sqrMagnitude > ok_dist_sqrd) {
						continue;
					}

					// pass if not being rendered. ie off screen and we clipped its tail or head..
					GameObject hit_gobj = tm.collider.gameObject;
					gsd = hit_gobj.GetComponent<GeneralSpeciesData> ();
					if (gsd == null && hit_gobj.transform.parent != null) {
						gsd = hit_gobj.transform.parent.gameObject.GetComponent<GeneralSpeciesData> ();
					}

					if (gsd != null && !gsd.myCritterInfo.critterLODData.curLOD.LODrenderer.isVisible) {
						continue;
					}

					// if we have found a critter, do not check any more seabed species.
					if (best_gsd != null && gsd == null) {
						continue;
					}
					
					// grab the guy that is closest to our center line ray.
					Vector3 hitdir = tm.transform.position - tempRay.origin;
					float hit_dist = hitdir.magnitude;
					hitdir *= 1 / hit_dist;
					float new_dot = Vector3.Dot (hitdir, tempRay.direction);
					if (new_dot > best_dot
					   || (gsd != null && best_gsd == null && new_dot > 0.96f)) {	// always choose a critter if none selected and we are nearly clicking on it.
						agentInteractionCastResult = true;
						AgentObjectHit = hit_gobj;
						best_dot = new_dot;
						best_gsd = gsd;

						if (gsd == null && hit_dist < closest_item_dist) {
							closest_item_dist = hit_dist;
							closest_item_hit = hit_gobj;
						}
					}
				}
				sub_ray.origin += (sub_ray.direction * sub_inc);
				sub_dist += sub_inc;
				sub_inc += sub_inc;

				// make sure we are not casting beyond our viewable distance..
				if ((sub_dist + sub_inc) > dist) {
					sub_inc = dist - sub_dist; // we dont have to worry about negatives here because the loop will end if sub_dist is greater than dist.
				}
				sub_radius += sub_radius_inc;
			}

			radius += radius_increment;
		}

		if (best_gsd == null) {
			AgentObjectHit = closest_item_hit;
		}

		if (agentInteractionCastResult) {

			GameObject currentTarget = CameraManager.GetCurrentTarget ();

			//If player is following anyone AND they clicked on their target
			if (best_gsd != null && CameraManager.CurrentCameraFollowsTargets () && best_gsd.gameObject == currentTarget) {
				
//				Debug.Log ("...we are already following this critter.");
				//Layers are specific to collision
				WemoItemData item_data = AgentObjectHit.GetComponent<WemoItemData> ();

				if (item_data == null && AgentObjectHit.transform.parent != null) {
					item_data = AgentObjectHit.transform.parent.gameObject.GetComponent<WemoItemData> ();
				}

				if (item_data != null) {
					GetInfo (best_gsd, item_data);
					//AudioManager.PlayInGameAudio(SoundFXID.GetInfo);
				}
			} else {
//				Debug.Log ("...yay something new!");
				//Follow the agent!
				if (best_gsd != null) {
					CameraManager.SwitchToTarget (best_gsd.myCritterInfo.critterObject, CameraType.FollowBehindCamera);
					//GUIMasterManager.RemoveVariantAndMakerInfo();
				}


				//TODO: Optimize the following code - It is duplicated from above
				WemoItemData item_data = AgentObjectHit.GetComponent<WemoItemData> ();

				if (item_data == null && AgentObjectHit.transform.parent != null) {
					item_data = AgentObjectHit.transform.parent.gameObject.GetComponent<WemoItemData> ();
				}
				if (item_data != null) {
					// debug toggle off the environment based on what we've clicked on.
					if (best_gsd == null) {
						SphereInstance.Instance.DeleteEnvironmentObject (item_data.variantID);
					}

//					AudioManager.PlayInGameAudio (SoundFXID.SoftBeep);
					GetInfo (best_gsd, item_data);
				}
			}
		} else {
//			Debug.Log("... INTO NOTHING");
// BLU_REBOOT TODO > close the info panel... when it exists again!			
//			GUIStateManager.CloseInfoPanel();
		}
	}

    public bool AgentInteractionCastVR (Ray ray, int layerMask, out WemoItemData best_item)
    {
        OculusFPSCameraMode.singleton.ToggleColliderCollision( false );

        //The result of whether or not the collection cast hit a collectible
        bool agentInteractionCastResult = false;
        
        //Holds information on the collectible that was hit
        RaycastHit[] hits;
        
        //Cast a sphere along the precalculated ray to see if it collides with any Items (Fish, stationary agents)
        float radius = 1f;
		float dist = 450f;//1500f;//GlobalOceanShaderAdjust.CurrentDistance ();
        float radius_max;
        float radius_increment;

        best_item = null;
        radius_max = 20.0f;
        radius_increment = 7.0f;        

//              Debug.Log("CASTING...");
        //Cast a series of Spheres and see if they connect with valid objects in the sphere
        float ok_dist_sqrd = dist * dist;
        while (radius < radius_max) 
        {
            float sub_dist = 0f;
            float sub_inc = 300f;
            float sub_radius = radius;
            float sub_radius_inc = 10f;
            Ray sub_ray = ray;
            while (sub_dist < dist)
            {
                hits = Physics.SphereCastAll (sub_ray.origin, sub_radius, sub_ray.direction, sub_inc, layerMask);
                float best_dot = -2f;
                
                for (int i = 0; i<hits.Length; i++) 
                {
                    RaycastHit tm = hits [i];

                    float distSqr = (tm.point - ray.origin).sqrMagnitude;
                    // pass if outside of our slop distance.
                    if (distSqr > ok_dist_sqrd) 
                    {
                        continue;
                    }

                    // pass if not being rendered. ie off screen and we clipped its tail or head..
                    GameObject hit_gobj = tm.collider.gameObject;                   
                    WemoItemData itm = hit_gobj.GetComponent<WemoItemData>();

                    if (hit_gobj.layer == 14 && itm == null && hit_gobj.isStatic)
                    {
                        if (tm.distance < dist)
                        {
                            dist = tm.distance;
                            ok_dist_sqrd = dist * dist;
                        }

                        continue;
                    }

                    GeneralSpeciesData gsd = null;

                    if( itm == null )
                    {
                        if( hit_gobj.transform.parent != null ) {
                            itm = hit_gobj.transform.parent.GetComponent<WemoItemData>();
                        }

                        if( itm == null ) {
                            continue;
                        }
                    }

                    bool testAngleFromCenter = true;

                    if (itm.critterInfo != null)
                    {
                        gsd = itm.critterInfo.generalSpeciesData;

                        if (itm.critterInfo.swimPlayerViewData != null)
                        {
                            testAngleFromCenter = itm.critterInfo.swimPlayerViewData.allowRotate;
                        }
                    }

                    if (gsd != null)
                    {
                        if (!gsd.myCritterInfo.critterLODData.curLOD.LODrenderer.isVisible) 
                        {
                            continue;
                        }

                        float critterDistLimit = 400f + Mathf.Max (Mathf.Max (gsd.myCritterInfo.generalMotionData.critterBoxColliderSize.x, gsd.myCritterInfo.generalMotionData.critterBoxColliderSize.y), gsd.myCritterInfo.generalMotionData.critterBoxColliderSize.z); 
                        if (distSqr > critterDistLimit * critterDistLimit) 
                        {
                            continue;
                        }
                    }

                    // grab the guy that is closest to our center line ray.
                    Vector3 hitdir;

                    if (testAngleFromCenter)
                    {
                        hitdir = tm.transform.position - ray.origin;
                    }
                    else
                    {
                        // super big critters marked as ground, use their hit pos
                        hitdir = tm.point - ray.origin;
                    }

                    float hit_dist = hitdir.magnitude;
                    hitdir *= 1 / hit_dist;
                    float new_dot = Vector3.Dot (hitdir, ray.direction);
                    if( new_dot < 0.93f )
                    {
                        continue;
                    }

                    if (new_dot > best_dot
                        || (best_item == null && new_dot > 0.96f))
                    {   // always choose a critter if none selected and we are nearly clicking on it.
                        agentInteractionCastResult = true;
                        best_dot = new_dot;
                        best_item = itm;
                    }
                }

                // cut out when we find something in a more focused test
                if (agentInteractionCastResult)
                {
                    break;
                }

                sub_ray.origin += (sub_ray.direction * sub_inc);
                sub_dist += sub_inc;
                sub_inc += sub_inc;
                
                // make sure we are not casting beyond our viewable distance..
                if ((sub_dist + sub_inc) > dist) 
                {
                    sub_inc = dist - sub_dist; // we dont have to worry about negatives here because the loop will end if sub_dist is greater than dist.
                }

                sub_radius += sub_radius_inc;
            }
            
            radius += radius_increment;
        }

        OculusFPSCameraMode.singleton.ToggleColliderCollision( true );

        return agentInteractionCastResult;
    }

	#endregion

	#region Get Info
	void GetInfo (GeneralSpeciesData gsd, WemoItemData item_data)
	{       
/*		CritterInfo critter = null;
		int dbVariantId = 0;
		int owned_userid = 0;
		int variant_owned_id = 0;		
		bool first_click = false;

		if (gsd != null) {
			critter = SimInstance.Instance.GetCritterInfoFromIndex (gsd.masterIndex);			
			if (critter == null) {
				return;
			}
			
			critter.clickedOn = true;
            
			dbVariantId = critter.dbVariantID;
			variant_owned_id = critter.dbOwnedID;
//BLU 2.0 No owned items yet..            
			DataUserItem owned_data = App.OwnedFishManager.GetOwnedItemByLegacyId( critter.dbOwnedID );
            if ( owned_data != null ) {
                owned_userid = owned_data.legacyuserid;
            }
            else if( owned_data == null && critter.waldoData != null && critter.waldoData.isWanderer ) {
                owned_userid = WaldoManager.GetWandererUserID( critter.waldoData );
            }
		} else {
			if (item_data.variantID <= 0)				
				return;
			
			dbVariantId = item_data.variantID;
		}

		if (GUIGraffitiManager.IsInfoPanelOpenOnVariant (dbVariantId) && (GUIGraffitiManager.IsInfoPanelOpenOnOwnedVariant (variant_owned_id) || variant_owned_id == 0)) {
			DebugDisplay.AddDebugText ("CLOSE INFO PANEL ON VARIANT CLICK");			
			DebugDisplay.AddDebugText ("dbVariantId = " + dbVariantId);
			DebugDisplay.AddDebugText ("variant_owned_id = " + variant_owned_id);
			
			GUIStateManager.CloseInfoPanel (); 
			return;
		}
				
		//Send Data to get displayed
		GUIGraffitiManager.UpdateInfoPanel (dbVariantId,
											owned_userid, 
											variant_owned_id,
											first_click);*/
	}

	#endregion

	#region COOKIES
	//private void GetVariantClickedStatus(string variant_name ){
	public static void GetVariantClickedStatus (string variant_name)
	{
		//singleton.firstSightingCookieName = variant_name+"FIRSTCLICK";
//		NetworkManager.GetCookie(singleton.firstSightingCookieName,singleton.CheckVariantClickedStatus, true);
	}

	void CheckVariantClickedStatus (string the_cookie)
	{
/*		if (the_cookie != null) {
			DebugDisplay.AddDebugText ("VARIANT CLICKED STATUS RECEIVED! = " + the_cookie);
			//Don't display fish sighting info
		} else {
			DebugDisplay.AddDebugText ("VARIANT CLICKED STATUS NOT RECEIVED");

			//Draw first sighting notification
			GUIGraffitiManager.DrawFirstSightingMessage ();
			DebugDisplay.AddDebugText ("DRAW FIRST SIGHTING NOTIFICATION!!!");

			//Set cookie
			SetVariantClickedStatusCookie (the_cookie);
		}*/
	}

	//Set Select Variant Cookie
	private void SetVariantClickedStatusCookie (string cookie)
	{
		//Need to recreate the cookiename
//		NetworkManager.SetCookie(firstSightingCookieName, "1");
	}
	#endregion
}
