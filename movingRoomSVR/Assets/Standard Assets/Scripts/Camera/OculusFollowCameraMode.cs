using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OculusFollowCameraMode : BaseCameraMode
{
	public static OculusFollowCameraMode singleton;

	[HideInInspector]
	public GameObject internalTarget;
	[HideInInspector]
	public CritterInfo internalCritter;
	private Transform internalTargetTransform;
	public float minDistance = 25.0f;
	public float desiredDistance = 40.0f;
	private float minFlatDistance;
	public Vector3 followOffset = new Vector3(0f, 50f, -100f);
	// based on the speed of the fish, we recalculate our acceleration.
//    private float lookAtDeadSpaceSqrd;
	//	private float lookAtDistClampSqrd;
	private float lookAtTimer;
//	private Vector3 lookAtTarget;
	private Vector3 desiredLookAtTarget;
	private Vector3 desiredPosition;
	public float maxCameraHeight = -10.0f;
//	private float lockAngle = 25f;
//	private float springRate = 3f;
	// new speed based model.
	//	private float lookAtAccelTime = 1f; // used to calculate lookat acceleration based on current fish speed.
	//	private float lookAtJerk = 150.0f;
	//	private float lookAtDesiredAccel = 10f;
	//	private float lookAtAccel = 10f;
//    private Vector3 lookAtVelocity;
	private float movementAccelTime = 1f;
	// user to calculate movement acceleration based on current fish speed.
	private float movementJerk = 150.0f;
	private float movementDesiredAccel = 10f;
	private float movementAccel = 10f;
	private Vector3 movementVelocity;
	private Vector3 rotationVelocity = Vector3.zero;

    private bool bListReadyToUse = false;
	private float accuTime = 0.0f;
	private const int PositionToStore = 3;
	private LinkedList<Vector3> internalTargetPositions = new LinkedList<Vector3> ();
//	private bool doNotInitPosition = false;

	public Vector3 lastDesiredSpeed;
//	private float physicsAccel = 100f;
//    private bool bUpdated;

    public Transform exitTeleportTransform;

    enum FollowState
    {
        EnterFadeOut,
        EnterFadeStay,
        EnterFadeIn,
        ExitFadeOut,
        ExitFadeStay,
        ExitFadeIn,
    }

    public float fadeOutTime = 0.3f;
    public float fadeStayMinTime = 0.5f;
    public float fadeStayMaxTime = 1.5f;
    public float fadeInTime = 0.3f;
    FollowState m_state;
    float m_fadeStayTimer = 0f;
    bool m_bringUpMenu;

    bool m_wasMainMenuUp;

    Transform initCritterScriptGoToTransform;

	//	private int layerMask;
	private float GetAccelForSpeed (float time, float speed, float cur_speed)
	{
		return (Mathf.Abs ((speed - cur_speed) / time));
	}
	
	void Awake ()
	{
		singleton = this;
//		doNotInitPosition = false;
		internalTarget = null;
		//		layerMask = 1<<14; // ground collision only right now
	}

	public override void InitCameraMode ()
	{
		if (inited)
        {
			return;
		}
        
		base.InitCameraMode ();        
        
		cameraType = CameraType.OculusFollowCamera;
		myTransform = transform;
		cameraName = "Oculus Follow Camera";
		bListReadyToUse = false;
	}
	
	public override bool GetFollowsTargets ()
	{ 
		return true; 
	}
	
    Color GetFadeColor()
    {
        Color c = GlobalOceanShaderAdjust.Instance.updateWater(myTransform.position.y);
        c.a = 1f;
        return c;
    }

	public override void StartCameraMode ()
	{
//        bUpdated = false;
        m_bringUpMenu = false;
		runCollision = CameraCollisionType.None;
		lastDesiredSpeed = Vector3.zero;
        myTransform.position = CameraManager.GetCurrentCameraPosition ();
        myTransform.rotation = CameraManager.GetCurrentCameraRotation ();
        CameraManager.singleton.ovrPhysicsMove = false;
        CameraManager.singleton.ovrRB.isKinematic = true;
        CameraManager.singleton.CollisionEnable(false);
        m_state = FollowState.EnterFadeOut;
        OculusCameraFadeManager.StartCameraFadeToColor(GetFadeColor(), fadeOutTime, null, null);
        initCritterScriptGoToTransform = null;
        exitTeleportTransform = null;
        AudioManager.FadeInSFX(SoundFXID.TravelMoving, 3f);
        AudioManager.PlayInGameAudio(SoundFXID.TravelMoving);
        m_wasMainMenuUp = false;
    }
    
    public override void EndCameraMode ()
    {
        if (SimInstance.Instance.IsSimPaused())
        {
            PauseControl.PublicSetPause(false);
        }

        if (internalCritter != null &&
            internalCritter.swimDisperseData != null)
        {
            internalCritter.swimDisperseData.playerDisperseDisableCount--;
        }

        CameraManager.UpdateCurrentCameraTransform ();
        internalCritter = null;
        internalTarget = null;
        internalTargetTransform = null;
        exitTeleportTransform = null;
        CameraManager.singleton.CollisionEnable(true);       
        AudioManager.FadeOutSFX(SoundFXID.TravelMoving, 3f);
    }

    public void SetTarget(CritterInfo c)
    {
        if (c == null)
        {
            return;
        }

        internalCritter = c;
        internalTarget = c.critterObject;
        internalTargetTransform = c.critterTransform;       

        FishCameraConstants constants = internalTarget.GetComponent<FishCameraConstants> ();
        if (constants != null)
        {
            followOffset = constants.oculusFollowOffset;
        }
        else
        {
            followOffset = new Vector3(0f, 50f, -100f);
        }

//        desiredDistance = constants.ropeAndStick_desiredDistance;
//        maxDistance = constants.ropeAndStick_maxDistance;

        movementAccel = 0f;
        movementDesiredAccel = 0f;
        movementVelocity = Vector3.zero;
        bListReadyToUse = false;
        internalTargetPositions.Clear ();
        desiredPosition = myTransform.position;         
//        lookAtTarget = internalTargetTransform.position;

        if (internalCritter.swimDisperseData != null)
        {
            internalCritter.swimDisperseData.playerDisperseDisableCount++;
        }
    }

    public void Exit(bool bringUpMenu)
    {
        m_bringUpMenu = bringUpMenu;
        m_state = FollowState.ExitFadeOut;
        initCritterScriptGoToTransform = null;
        OculusCameraFadeManager.StartCameraFadeToColor(GetFadeColor(), fadeOutTime, null, null);
    }

    bool SetCritterPos()
    {
        if (internalCritter.generalSpeciesData.myCurrentBehaviorType == SwimBehaviorType.SWIM_FOLLOWPATH)
        {
            internalTargetTransform.position = internalCritter.swimFollowPathData.followTransform.position;

            bool bHasMoved = internalCritter.swimFollowPathData.lastMoveDir.magnitude > 0.00001f;
            if (bHasMoved)
            {
                myTransform.rotation = internalTargetTransform.rotation = Quaternion.LookRotation(internalCritter.swimFollowPathData.lastMoveDir);
            }
            else
            {
                myTransform.rotation = internalTargetTransform.rotation = internalCritter.swimFollowPathData.followTransform.rotation;
            }

            internalCritter.generalMotionData.desiredVelocityDirection = Vector3.zero;
            internalCritter.critterSteering.Reset ();
            CameraManager.singleton.SetYRotation(myTransform.rotation.eulerAngles.y);
            OVRDevice.ResetOrientation();
            CameraManager.UpdateCurrentCameraTransform();

            return bHasMoved;
        }
        else if (internalCritter.generalSpeciesData.myCurrentBehaviorType == SwimBehaviorType.SWIM_SCRIPT_GOTO)
        {
            if (initCritterScriptGoToTransform == null &&
                internalCritter.swimScriptGoToPointData.goToTransform != null)
            {
                initCritterScriptGoToTransform = internalCritter.swimScriptGoToPointData.goToTransform;
            }

            if (initCritterScriptGoToTransform != null)
            {
                internalTargetTransform.position = initCritterScriptGoToTransform.position;           
                internalCritter.generalMotionData.desiredVelocityDirection = Vector3.zero;
                myTransform.rotation = internalTargetTransform.rotation = initCritterScriptGoToTransform.rotation;
                internalCritter.critterSteering.Reset ();
                CameraManager.singleton.SetYRotation(myTransform.rotation.eulerAngles.y);
                OVRDevice.ResetOrientation();
                CameraManager.UpdateCurrentCameraTransform();            
                return true;
            }
        }

        return false;
    }
    
    public override void UpdateCameraMode ()
	{
        bool bSetPos = false;

        if (m_state == FollowState.EnterFadeOut)
        {
            if (OculusCameraFadeManager.IsFaded())
            {
                m_state = FollowState.EnterFadeStay;
                m_fadeStayTimer = fadeStayMaxTime;
                OVRDevice.ResetOrientation();    
                bSetPos = true;
                SetCritterPos();
            }
            else
            {
                return;
            }
        }
        else if (m_state == FollowState.EnterFadeStay)
        {
            bSetPos = true;
            m_fadeStayTimer -= Time.deltaTime;
            if ((SetCritterPos() && fadeStayMaxTime - m_fadeStayTimer > fadeStayMinTime) ||
                m_fadeStayTimer < 0f)
            {
                m_state = FollowState.EnterFadeIn;
                OculusCameraFadeManager.StartCameraFadeFromBlack(fadeInTime, null, null);
            }
        }
        else if (m_state == FollowState.ExitFadeOut)
        {
            if (OculusCameraFadeManager.IsFaded())
            {
                if (exitTeleportTransform != null)
                {
                    myTransform.position = exitTeleportTransform.position;
                    myTransform.rotation = exitTeleportTransform.rotation;
                    lastDesiredSpeed = Vector3.zero;
                    CameraManager.singleton.SetYRotation(myTransform.rotation.eulerAngles.y);
                    OVRDevice.ResetOrientation();
                    CameraManager.UpdateCurrentCameraTransform();            
                }

                CameraManager.SwitchToCamera(CameraType.OculusCamera);

                if (m_bringUpMenu)
                {
                    OculusCameraFadeManager.SetColor(Color.clear);
                    FloatingMenuManager.ShowMenu(FloatingMenuManager.MenuType.TourComplete);
                }
                else
                {
                    OculusCameraFadeManager.StartCameraFadeFromBlack(fadeInTime, null, null);
                }

                return;
            }
        }

        if (FloatingMenuManager.IsMenuUp())
        {
            if (!m_wasMainMenuUp)
            {
                m_wasMainMenuUp = true;
                PauseControl.PublicSetPause(true);
            }

            return;
        }
        else if (m_wasMainMenuUp)
        {
            m_wasMainMenuUp = false;
            PauseControl.PublicSetPause(false);
        }

		float dt = Time.deltaTime;
		Vector3 oldPos = myTransform.position;

        Vector3 targetPos = internalTargetTransform.localToWorldMatrix.MultiplyPoint(followOffset);

//        lookAtTarget = targetPos;
		
		float cur_critter_speed = internalCritter.generalMotionData.currentSpeed;
        float cur_critter_accel = internalCritter.generalMotionData.currentAcc;

        Vector3 dir = myTransform.position - targetPos;
		float dist = dir.magnitude;
        if (dist > 0f)
        {
		    dir *= (desiredDistance / dist);
        }

        Vector3 original_desired_pos = targetPos + dir;
		if (original_desired_pos.y > maxCameraHeight) {
			original_desired_pos.y = maxCameraHeight;
		}
		Vector3 new_pos = original_desired_pos;

		if (internalTargetPositions.Count >= PositionToStore) {
			internalTargetPositions.RemoveFirst ();
			bListReadyToUse = true;
		}
		accuTime += Time.deltaTime;
		if (accuTime > 1.0f) {
            internalTargetPositions.AddLast (targetPos);
			accuTime = 0.0f;
		}
		//		this does a sphere cast from fish to camera.
        Ray internal_target_to_camera = new Ray (targetPos, dir.normalized);
		RaycastHit _hit;
		float scaler = 1.0f;
		float base_sphere_cast_radius = 50f;
		bool b_sphere_casting = true;
		float max_camera_up_and_hit_normal_angle = -0.5f;
		float minimum_camera_forward_and_target_up_angle = 0.3f;
		float lerpHitPosTimeScale = 0.3f;
		while (b_sphere_casting)
        {
            if (Physics.SphereCast (internal_target_to_camera, scaler * base_sphere_cast_radius, out _hit, dist, 1 << 14) &&
                _hit.collider.gameObject.isStatic) 
            {
//				string nn = _hit.collider.name;
				if (Vector3.Dot (myTransform.up, _hit.normal) > max_camera_up_and_hit_normal_angle)
                {
					if (Vector3.Dot (myTransform.forward, -internalTargetTransform.up) < minimum_camera_forward_and_target_up_angle) 
                    {
						//							we are only doing move up the camera, the hitnormal will kind of offset the camera move right left.
						Vector3 tempNewPos = new_pos;
						tempNewPos += (myTransform.up + _hit.normal) * base_sphere_cast_radius * (70.0f / scaler);
						if (tempNewPos.y > maxCameraHeight) 
                        {
							tempNewPos.y = maxCameraHeight;
						}
						new_pos = Vector3.SmoothDamp (new_pos, tempNewPos, ref rotationVelocity, lerpHitPosTimeScale);
					}
				}
				b_sphere_casting = false;
			} 
            else 
            { // this happens when camera is chasing and there is a chance that the sphere cast missed..(i think)
				scaler += 1.0f;
			}
			if (scaler > 5) 
            { // only wanna do the check 15 times, this while loop kills the performance.
				b_sphere_casting = false;
			}
		}
		//		this does a fish to fish forward sphere cast . this is useful when camera is behind of the fish.
		bool b_internal_target_forward_casting = true;
		float forward_casting_scaler = 1.0f;
		float desired_distance_scaler = 4.0f;
        Ray internal_target_forward_ray = new Ray (targetPos, internalTargetTransform.forward);
		while (b_internal_target_forward_casting)
        {
			if (Physics.SphereCast (internal_target_forward_ray, forward_casting_scaler * base_sphere_cast_radius, out _hit, desiredDistance * desired_distance_scaler, 1 << 14) &&
                _hit.collider.gameObject.isStatic) 
            {
				//ignore everything above our max camera height
				if (_hit.point.y < maxCameraHeight) 
                {
					if (Vector3.Dot (myTransform.forward, -internalTargetTransform.up) < minimum_camera_forward_and_target_up_angle) 
                    {
						Vector3 tempNewPos = new_pos;
						tempNewPos += (myTransform.up) * base_sphere_cast_radius * (70.0f / forward_casting_scaler);
						if (tempNewPos.y > maxCameraHeight) 
                        {
							tempNewPos.y = maxCameraHeight;
						}
						new_pos = Vector3.SmoothDamp (new_pos, tempNewPos, ref rotationVelocity, lerpHitPosTimeScale);
					}
					b_internal_target_forward_casting = false;
				} 
                else 
                {
					b_internal_target_forward_casting = false;
				}
			} 
            else 
            { // this happens when camera is chasing and there is a chance that the sphere cast missed..(i think)
				forward_casting_scaler += 1.0f;
			}
			if (forward_casting_scaler > 6)
            { // only wanna do the check 10 times, this while loop kills the performance.
				b_internal_target_forward_casting = false;
			}
		}
		if (bListReadyToUse) {
			new_pos = Vector3.SmoothDamp (new_pos, internalTargetPositions.Last.Value, ref rotationVelocity, lerpHitPosTimeScale * 2.0f);
		}
		if ((new_pos - original_desired_pos).sqrMagnitude > 0.01f * 0.01f) {
			new_pos = Vector3.SmoothDamp (original_desired_pos, new_pos, ref rotationVelocity, lerpHitPosTimeScale);
		}
		//		Vector3 new_pos = lookAtTarget - internalTargetTransform.forward * desiredDistance;		
		//		if( !MathfExt.Approx(new_pos, desiredPosition, 1f )) {
		desiredPosition = new_pos;

		//		Vector3 pos_diff = new_pos - myTransform.position;

        float desiredSpeed = cur_critter_speed;
        float curSpeed = movementVelocity.magnitude;

        float startDecelDist = curSpeed-cur_critter_speed; // (decel at .01m/s/s
        desiredSpeed = cur_critter_speed * MathfExt.Fit (dist, desiredDistance, desiredDistance+startDecelDist*2f, 1f, 2f);
        movementDesiredAccel = GetAccelForSpeed (movementAccelTime, 0f, desiredSpeed);
        
        if (movementDesiredAccel > 0f &&
            movementDesiredAccel < cur_critter_accel)
        {
            movementDesiredAccel = cur_critter_accel;
        }            
        else if (movementDesiredAccel < 0f &&
                 movementDesiredAccel > cur_critter_accel)
        {
            movementDesiredAccel = cur_critter_accel;
        }            

        movementDesiredAccel *= MathfExt.Fit(dist, desiredDistance, desiredDistance + startDecelDist*2f, 1f, 2f);

		//			DebugDisplay.AddDebugText("reset move timer");
		//		}
		float jerk_amt = movementJerk * dt;
		if (movementAccel < movementDesiredAccel) {
			movementAccel += jerk_amt;
			if (movementAccel > movementDesiredAccel) {
				movementAccel = movementDesiredAccel;
			}
		} else if (movementAccel > movementDesiredAccel) {
			movementAccel -= jerk_amt;
			if (movementAccel < movementDesiredAccel) {
				movementAccel = movementDesiredAccel;
			}
		}

		new_pos = myTransform.position;
		MathfExt.AccelDampDelt (desiredPosition, movementAccel, movementAccel * 2f, dt, 1500f, ref movementVelocity, ref new_pos);

        if (bSetPos)
        {
            myTransform.position = desiredPosition = targetPos; 
            bListReadyToUse = false;
            internalTargetPositions.Clear ();
        }
        else
        {
    		myTransform.position = new_pos;	
        }

		if (myTransform.position.y > maxCameraHeight)
		{
//			Debug.Log("CLAMPED HEIGHT!");
			myTransform.position = new Vector3 (myTransform.position.x, maxCameraHeight, myTransform.position.z);
			desiredPosition = myTransform.position;
		}
		
/*
		Quaternion og_rot = myTransform.rotation;
		myTransform.LookAt (lookAtTarget, Vector3.up);

		float angle = Quaternion.Angle (myTransform.rotation, og_rot);
		float new_angle = angle - (angle * springRate * 2.0f * Time.deltaTime);
		if (angle > 0.0f && new_angle > 0.0f) 
		{
			if (new_angle > lockAngle) 
			{
				new_angle = lockAngle;
			}
			float ratio = 1.0f - (new_angle / angle);
			Quaternion new_rot = Quaternion.Slerp (og_rot, myTransform.rotation, ratio);
			myTransform.rotation = new_rot;
		}
*/

        // pull orientation from Oculus and apply it to this camera
        Quaternion ovrRot = myTransform.rotation;
        CameraManager.singleton.GetCameraOrientation(ref ovrRot);
        myTransform.rotation = ovrRot;

		//usephysics move

		lastDesiredSpeed = (myTransform.position - oldPos)/Time.deltaTime;


//        if (bUpdated &&
//            Input.GetButtonDown("Interact"))
//        {
//            CameraManager.SwitchToCamera(CameraType.OculusCamera);
//        }

//        bUpdated = true;
	}
}
