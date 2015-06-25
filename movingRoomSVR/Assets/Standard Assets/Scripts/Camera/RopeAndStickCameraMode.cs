using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RopeAndStickCameraMode : BaseCameraMode
{
	private static RopeAndStickCameraMode singleton;
	private GameObject internalTarget;
	private CritterInfo internalCritter;
	private Transform internalTargetTransform;
	public float minDistance = 25.0f;
	public float desiredDistance = 35.0f;
	public float maxDistance = 45.0f;
	public float movementTime = 1.0f;
	private float minFlatDistance;
	private float movementTimer;
//	private float panelOpenRightOffset = 0.35f;
	public float heightOffset = 0.0f;
	// based on the speed of the fish, we recalculate our acceleration.
	public float lookAtDeadSpace = 10.0f;
	public float lookAtDelay = 0.3f;
	public float lookAtDistClamp = 20.0f;
//    private float lookAtDeadSpaceSqrd;
	//	private float lookAtDistClampSqrd;
	private float lookAtTimer;
	private Vector3 lookAtTarget;
	private Vector3 desiredLookAtTarget;
	private Vector3 desiredPosition;
	public float maxCameraHeight = -10.0f;
	private float lockAngle = 25f;
	private float springRate = 3f;
	// new speed based model.
	//	private float lookAtAccelTime = 1f; // used to calculate lookat acceleration based on current fish speed.
	//	private float lookAtJerk = 150.0f;
	//	private float lookAtDesiredAccel = 10f;
	//	private float lookAtAccel = 10f;
//    private Vector3 lookAtVelocity;
	private float movementAccelTime = 2f;
	// user to calculate movement acceleration based on current fish speed.
	private float movementJerk = 150.0f;
	private float movementDesiredAccel = 10f;
	private float movementAccel = 10f;
	private Vector3 movementVelocity;
	private bool atLimits = false;
	private Vector3 rotationVelocity = Vector3.zero;
//    private Vector3 cinematicRotationVelocity = Vector3.zero;
	private bool bListReadyToUse = false;
	private float accuTime = 0.0f;
	private const int PositionToStore = 3;
	private LinkedList<Vector3> internalTargetPositions = new LinkedList<Vector3> ();
	private bool doNotInitPosition = false;
	private bool allowOutsideCenterPositions = false;
	private float allowOutsideTime = 0.0f;
	private float allowOutsideTimer = 2.0f;
	private float allowOutsideDistance = 0f;
	private bool allowOutsideDistanceSet = false;
	private bool isInfoPanelOpen = false;
	
	// internaltargets' positions in last few seconds
	/*	private static float cosAngleCutoff = 0.866f; // 30degrees
        private static float sidewaysRadFactor = 5f;
        private static float intoRadFactor = 8f;*/
	//	private int layerMask;
	private float GetAccelForSpeed (float time, float speed, float cur_speed)
	{
		return (Mathf.Abs ((speed - cur_speed) / time));
	}
	
	void Awake ()
	{
		singleton = this;
		doNotInitPosition = false;
		internalTarget = null;
		//		layerMask = 1<<14; // ground collision only right now
		
//		UILayout_InfoPanel.OnOpenEvent += ShiftCameraOnInfoPanelOpen;
//		UILayout_InfoPanel.OnCloseEvent += ShiftCameraOnInfoPanelClose;
	}
	
	public static void SetPos (Vector3 position)
	{
		singleton.myTransform.position = position;
	}
    
	public static void BlockSwitch (bool block)
	{
		singleton.blockSwitch = block;
		// TODO>GROSS>OceanSphereController needs to be broken up ... badly.
		SimManager.SetBlockCameraTargetSwitching (block);
	}
	
	public static bool IsFollowingCritter (CritterInfo critter_info)
	{
		return (singleton.internalCritter == critter_info);
	}
	
	public static void DoNotInitPosition (bool allow_outside_positions)
	{
		singleton.doNotInitPosition = true;
		singleton.allowOutsideCenterPositions = allow_outside_positions;
		if (singleton.allowOutsideCenterPositions) {
			singleton.allowOutsideTimer = singleton.allowOutsideTime;
			singleton.allowOutsideDistanceSet = false;
			DebugDisplay.AddDebugText ("Turning ON outside positions for follow cam.");
		}
	}

	public override void InitCameraMode ()
	{
		if (inited) {
			return;
		}
        
		base.InitCameraMode ();        
        
		cameraType = CameraType.FollowBehindCamera;
		myTransform = transform;
		cameraName = "Follow Camera";
		bListReadyToUse = false;
	}
	
	public override bool GetFollowsTargets ()
	{ 
		return true; 
	}

	public static void StaticStartCameraMode ()
	{
		singleton.StartCameraMode ();
	}
	
	public override void StartCameraMode ()
	{
		internalTarget = null;
	}
	
	public static void StaticUpdateCameraMode ()
	{
		singleton.UpdateCameraMode ();
	}
	
	public static Transform GetTransform ()
	{
		return singleton.myTransform;
	}
	
	public void ShiftCameraOnInfoPanelOpen ()
	{
		isInfoPanelOpen = true;
	}
	
	public void ShiftCameraOnInfoPanelClose ()
	{		
		isInfoPanelOpen = false;
	}
	
	public override void UpdateCameraMode ()
	{
		//		runCollision = false;
		float dt = Time.deltaTime;
		float dot;
		runCollision = CameraCollisionType.Ground;
		
		if (internalTarget != CameraManager.currentTarget && !blockSwitch) {
			internalTarget = CameraManager.currentTarget;
			if (internalTarget == null) {
//                Debug.Log("internalTarget is null");
				CameraManager.JumpToCameraOrder (0);
				return;
			}

			internalTargetTransform = internalTarget.transform;
			internalCritter = SimManager.GetCritterForCameraTarget ();
			if (internalCritter == null) {
//                Debug.Log("critter si null");
				CameraManager.JumpToCameraOrder (0);
				return;
			}
			runCollision = CameraCollisionType.None;
			FishCameraConstants constants = internalTarget.GetComponent<FishCameraConstants> ();
			minDistance = constants.ropeAndStick_minDistance;
			minFlatDistance = minDistance * 0.5f;
			desiredDistance = constants.ropeAndStick_desiredDistance;
			maxDistance = constants.ropeAndStick_maxDistance;
			movementTime = constants.ropeAndStick_movementTime;
			heightOffset = constants.ropeAndStick_heightOffset;
			lookAtDeadSpace = constants.ropeAndStick_lookAtDeadSpace;
			lookAtDelay = constants.ropeAndStick_lookAtDelay;
			lookAtDistClamp = constants.ropeAndStick_lookAtDistClamp;
//			panelOpenRightOffset = constants.ropeAndStick_panelOpenRightOffset * minDistance;
//            lookAtDeadSpaceSqrd = lookAtDeadSpace * lookAtDeadSpace;
			//			lookAtDistClampSqrd = lookAtDistClamp * lookAtDistClamp;
			
			bListReadyToUse = false;
			internalTargetPositions.Clear ();
			
			if (doNotInitPosition) {
				myTransform.position = CameraManager.GetCurrentCameraPosition ();
//				Debug.Log("INIT : " + myTransform.position);
				myTransform.rotation = CameraManager.GetCurrentCameraRotation ();
				doNotInitPosition = false;
			} else {
				allowOutsideCenterPositions = false; // make sure this is off.
				RaycastHit hit;
				Vector3 xdiff = myTransform.position - internalTargetTransform.position;
				Ray new_ray = new Ray (internalTargetTransform.position, xdiff.normalized);
				dot = Vector3.Dot (new_ray.direction, Vector3.up);
				if (Mathf.Abs (dot) > 0.4f) {
					xdiff [0] += 10f;
					xdiff [1] *= 0.4f;
					xdiff [2] += 10f;
					new_ray.direction = xdiff;
				}
				bool done = false;
				float inc = MathfExt.PI_4;
				float total_inc = 0;
				Vector3 og_ray = xdiff;
				while (!done) {
					if (Physics.SphereCast (new_ray, 5.0f, out hit, desiredDistance - 15f, 1 << 14)) {
						myTransform.position = internalTargetTransform.position + (new_ray.direction * (hit.distance - 20));
//						Debug.Log("INITC : " + myTransform.position);
						if (hit.distance > (desiredDistance * 0.75)) {
							myTransform.position += new Vector3 (0f, 250f, 0);
//							Debug.Log("INITD : " + myTransform.position);
							done = true;
						} else {
							total_inc += inc;
							if (total_inc >= MathfExt.TWO_PI) {
								myTransform.position += new Vector3 (0f, 250f, 0);
//								Debug.Log("INITE : " + myTransform.position);
								done = true;
							}
							new_ray.direction = MathfExt.YawVector (og_ray, total_inc);
						}
					} else {
						myTransform.position = internalTargetTransform.position + (new_ray.direction * desiredDistance);
//						Debug.Log("INITF : " + myTransform.position);
						done = true;
					}
				}
			}
			desiredPosition = myTransform.position;
			
			lookAtTarget = internalTargetTransform.position;
			myTransform.LookAt (lookAtTarget, Vector3.up);
			
//			if (GUIStateManager.isLargeWebviewVisible) {
//				lookAtTarget -= myTransform.right * panelOpenRightOffset;
//				myTransform.LookAt (lookAtTarget, Vector3.up);
//			}
			
			//			lookAtTarget = internalTargetTransform.position;
			//			desiredLookAtTarget = internalTargetTransform.position;
			//			lookAtAccel = 0f;
			//			lookAtDesiredAccel = 0f;
			//          lookAtVelocity = Vector3.zero;
			movementAccel = 0f;
			movementDesiredAccel = 0f;
			movementVelocity = Vector3.zero;
			CameraManager.UpdateCurrentCameraTransform ();
		}

		//		Vector3 cur_forward = myTransform.forward;

		lookAtTarget = internalTargetTransform.position;
		if (isInfoPanelOpen) {
			lookAtTarget -= myTransform.up * (minDistance * 0.35f);
		}
			

		float cur_critter_speed = internalCritter.generalMotionData.currentSpeed;


		Vector3 dir = myTransform.position - internalTargetTransform.position;
		float dist = dir.magnitude;
		dir *= (desiredDistance / dist);
		Vector3 original_desired_pos = internalTargetTransform.position + dir;
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
			internalTargetPositions.AddLast (internalTargetTransform.position);
			accuTime = 0.0f;
		}
		//		this does a sphere cast from fish to camera.
		Ray internal_target_to_camera = new Ray (internalTargetTransform.position, dir.normalized);
		RaycastHit _hit;
		float scaler = 1.0f;
		float base_sphere_cast_radius = internalCritter.generalMotionData.critterBoxColliderRadius;
		bool b_sphere_casting = true;
		float max_camera_up_and_hit_normal_angle = -0.5f;
		float minimum_camera_forward_and_target_up_angle = 0.3f;
		float lerp_time_scaler_according_to_fish_speed = 60.0f / internalCritter.generalMotionData.currentSpeed;
		while (b_sphere_casting) {
			if (Physics.SphereCast (internal_target_to_camera, scaler * base_sphere_cast_radius, out _hit, dist, 1 << 14)) {
				string nn = _hit.collider.name;
				if (nn != "MainCamera") {
					if (Vector3.Dot (myTransform.up, _hit.normal) > max_camera_up_and_hit_normal_angle) {
						if (Vector3.Dot (myTransform.forward, -internalTargetTransform.up) < minimum_camera_forward_and_target_up_angle) {
							//							we are only doing move up the camera, the hitnormal will kind of offset the camera move right left.
							Vector3 tempNewPos = new_pos;
							tempNewPos += (myTransform.up + _hit.normal) * base_sphere_cast_radius * (70.0f / scaler);
							if (tempNewPos.y > maxCameraHeight) {
								tempNewPos.y = maxCameraHeight;
							}
							new_pos = Vector3.SmoothDamp (new_pos, tempNewPos, ref rotationVelocity, lerp_time_scaler_according_to_fish_speed);
						}
					}
					b_sphere_casting = false;
				} else { //if (_hit.collider.name == "MainCamera") //	if the spherecast only hit the camera, that means maybe the sphere cast is not big enough to make prediction.
					scaler += 1.0f;
				}
			} else { // this happens when camera is chasing and there is a chance that the sphere cast missed..(i think)
				scaler += 1.0f;
			}
			if (scaler > 5) { // only wanna do the check 15 times, this while loop kills the performance.
				b_sphere_casting = false;
			}
		}
		//		this does a fish to fish forward sphere cast . this is useful when camera is behind of the fish.
		bool b_internal_target_forward_casting = true;
		float forward_casting_scaler = 1.0f;
		float desired_distance_scaler = 4.0f;
		Ray internal_target_forward_ray = new Ray (internalTargetTransform.position, internalTargetTransform.forward);
		while (b_internal_target_forward_casting) {
			if (Physics.SphereCast (internal_target_forward_ray, forward_casting_scaler * base_sphere_cast_radius, out _hit, desiredDistance * desired_distance_scaler, 1 << 14)) {
				//ignore everything above our max camera height
				if (_hit.point.y < maxCameraHeight) {
					if (!_hit.collider.name.Equals ("MainCamera")) {
						if (Vector3.Dot (myTransform.forward, -internalTargetTransform.up) < minimum_camera_forward_and_target_up_angle) {
							Vector3 tempNewPos = new_pos;
							tempNewPos += (myTransform.up) * base_sphere_cast_radius * (70.0f / forward_casting_scaler);
							if (tempNewPos.y > maxCameraHeight) {
								tempNewPos.y = maxCameraHeight;
							}
							new_pos = Vector3.SmoothDamp (new_pos, tempNewPos, ref rotationVelocity, lerp_time_scaler_according_to_fish_speed);
						}
						b_internal_target_forward_casting = false;
					} else { //if (_hit.collider.name == "MainCamera") // if the spherecast only hit the camera, that means maybe the sphere cast is not big enough to make prediction.
						forward_casting_scaler += 1.0f;
					}
				} else {
					b_internal_target_forward_casting = false;
				}
			} else { // this happens when camera is chasing and there is a chance that the sphere cast missed..(i think)
				forward_casting_scaler += 1.0f;
			}
			if (forward_casting_scaler > 6) { // only wanna do the check 10 times, this while loop kills the performance.
				b_internal_target_forward_casting = false;
			}
		}
		if (bListReadyToUse) {
			new_pos = Vector3.SmoothDamp (new_pos, internalTargetPositions.Last.Value, ref rotationVelocity, lerp_time_scaler_according_to_fish_speed * 2.0f);
		}
		if ((new_pos - original_desired_pos).sqrMagnitude > 0.01f * 0.01f) {
			new_pos = Vector3.SmoothDamp (original_desired_pos, new_pos, ref rotationVelocity, lerp_time_scaler_according_to_fish_speed);
		}
		//		Vector3 new_pos = lookAtTarget - internalTargetTransform.forward * desiredDistance;		
		//		if( !MathfExt.Approx(new_pos, desiredPosition, 1f )) {
		desiredPosition = new_pos;

		if (!allowOutsideCenterPositions && !atLimits && ((myTransform.position - desiredPosition).sqrMagnitude > (desiredDistance * 3.0f) * (desiredDistance * 3.0f))) {
//			Debug.Log("AT : " + myTransform.position + " :: " + desiredPosition + " :: " + allowOutsideCenterPositions);
			myTransform.position = desiredPosition;
		}

		//		Vector3 pos_diff = new_pos - myTransform.position;
		movementDesiredAccel = GetAccelForSpeed (movementAccelTime, cur_critter_speed, movementVelocity.magnitude);
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
		MathfExt.AccelDampDelt (desiredPosition, movementAccel, movementAccel * 6f, dt, 1000f, ref movementVelocity, ref new_pos);
		// clamp on our min distance.
		dir = new_pos - lookAtTarget;
		float new_dist_sqrd = dir.sqrMagnitude;
		if (new_dist_sqrd < minDistance * minDistance) {
			dist = Mathf.Sqrt (new_dist_sqrd);
			float push_dist = minDistance - dist;
			dir = new_pos - lookAtTarget;
			if (MathfExt.Approx (dir, Vector3.zero, 0.1f)) {
				dir = myTransform.forward * -1;
			}
			dir.Normalize ();
			dir *= push_dist;
			new_pos += dir;
		}
		
		// clamp flat to eliminate any excessive spinning.
		dir = new_pos - lookAtTarget;
		dir.y = 0f;
		new_dist_sqrd = dir.sqrMagnitude;
		if (new_dist_sqrd < minFlatDistance * minFlatDistance) {
			dist = Mathf.Sqrt (new_dist_sqrd);
			float push_dist = minFlatDistance - dist;
			dir = new_pos - lookAtTarget;
			if (MathfExt.Approx (dir, Vector3.zero, 0.1f)) {
				dir = myTransform.forward * -1;
			}
			dir.y = 0f;
			dir.Normalize ();
			dir *= push_dist;
			new_pos += dir;
		}

		/*		else if( new_dist_sqrd > maxDistance * maxDistance ) {
//			DebugDisplay.AddDebugText("max clamped");
            dist = Mathf.Sqrt( new_dist_sqrd );
            dir *= ( maxDistance / dist );
            new_pos = lookAtTarget + dir;
        }*/
//		Debug.Log("REGULAR : " + myTransform.position + " :: " + new_pos + " :: " + desiredPosition);
		myTransform.position = new_pos;
		
		
		// always clamp our position to our camera center so that we do not leave the sphere visibility area..
		Vector3 center = CameraManager.GetFollowCamSphereCenter ();
		center.y = myTransform.position.y;
		Vector3 diff = myTransform.position - center;
		float d = CameraManager.GetFollowCamSphereDistance ();
		float diff_dist = diff.magnitude;
		if (!allowOutsideCenterPositions) {
			atLimits = false;
			if (diff_dist > d) {
				diff *= (d / diff_dist);
//				Debug.Log("CLAMPED!");
				myTransform.position = center + diff;
				atLimits = true;
			}
		} else {// if (!App.OwnedFishManager.OwnershipMomentActive ()) {	// HACK> not sure how this will work systematically. Bad to have state handling here.
			allowOutsideTimer -= dt;
			if (allowOutsideTimer < 0) {
//				Debug.Log("checking outside.");
				if (!allowOutsideDistanceSet) {
					allowOutsideDistance = diff_dist;
//					Debug.Log("setting clamp outside" + diff_dist);
					allowOutsideDistanceSet = true;
				} else {
					atLimits = false;
					if (diff_dist > allowOutsideDistance) {
						diff *= (allowOutsideDistance / diff_dist);
//						Debug.Log("clamped at outside");
						myTransform.position = center + diff;
						atLimits = true;
					}
				}
			}
		}

		if (myTransform.position.y > maxCameraHeight) {
//			Debug.Log("CLAMPED HEIGHT!");
			myTransform.position = new Vector3 (myTransform.position.x, maxCameraHeight, myTransform.position.z);
			desiredPosition = myTransform.position;
			atLimits = true;
		}
		
		Quaternion og_rot = myTransform.rotation;
		myTransform.LookAt (lookAtTarget, Vector3.up);

		float angle = Quaternion.Angle (myTransform.rotation, og_rot);
		float new_angle = angle - (angle * springRate * 2.0f * Time.deltaTime);
		if (angle > 0.0f && new_angle > 0.0f) {
			if (new_angle > lockAngle) {
				new_angle = lockAngle;
			}
			float ratio = 1.0f - (new_angle / angle);
			Quaternion new_rot = Quaternion.Slerp (og_rot, myTransform.rotation, ratio);
			myTransform.rotation = new_rot;
		}
		/*		dist = (myTransform.position - lookAtTarget).magnitude;		
        // do a final lock on the angle of the camera relative to the actual lookattarget.
        Quaternion real_rot = myTransform.rotation;
        myTransform.LookAt( desiredLookAtTarget, Vector3.up );

        float angle = Quaternion.Angle(real_rot,myTransform.rotation);
        if( angle >= lockAngle ) {
            float ratio	= lockAngle / angle;
            real_rot = Quaternion.Lerp(myTransform.rotation,real_rot,ratio);
        }
        myTransform.rotation = real_rot;
        lookAtTarget = myTransform.position + ( myTransform.forward * dist );*/
	}
}
