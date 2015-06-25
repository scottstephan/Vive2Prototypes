using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum TwoFingerState {
	NONE,
	GOOD,
	FAILED
}

public class TouchHistory {
	public Vector2 vp;			// viewport position
	public float speed;			// speed based on the previous position
	public float t;				// time since the the position was recorded
}

public static class InputManager {
    private static List<TouchHistory> touchHistory = new List<TouchHistory>();
	
	public static Action __tapped = null;

	private static float keepTouchHistoryTime = 0.5f;
	
	public static bool iPad3 = false;
	
	public static float timeSinceMouseMovement = 0.0f; // this is also handling no touches.
	public static float timeSinceAnyKeyPress = 0.0f;
	public static float timeAnyKeyHeld = 0f;
	
	private static bool noTapTillNextTouch = false;
	public static void SetNoTapTillNextTouch() {
//		Debug.Log ("SetNoTapTillNextTouch!" );
		noTapTillNextTouch = true;
		tapCount = 0;
	}
	
	public static int tapCount = 0;
	public static int uniqueTapIndex = 0; // to ensure that game code does not register the same tap twice!
	public static Vector2 lastTapPosition;
	public static bool touchActive = false;
	// how long has the current touch lasted.
	public static float touchTime = 0f;
	// how long has the current touch lasted.
	public static float noTouchTime = 0f;
	// was it quick enough to trigger a 'tap'
	private static float touchTapTime = 0.315f; // based on a small increment less than a small move..
	
	// how long before another touch can start before we reset our tapCount.
	private static float touchTapResetTime = 0.33f;
	public static Vector2 touchOGPosition;
	public static Vector2 touchLastPosition;
	public static float maxTouchSqrDistForTap = 0.004225f; // 0.065 squared

    public enum MoonlightBackInputType
    {
        None,
        ShortTap,
        LongTap,
        DoubleTap
    }

    static MoonlightBackInputType moonlightBackLastFrameInput;

    static float moonlightBackHoldTime;
    static float moonlightBackLastDownTime = 0f;
    static bool moonlightBackTapHandled;

/*	private static Vector3 realWorldFullSwipeDirection;
	public static Vector3 RealWorldFullSwipeDirection {
		get {
			return realWorldFullSwipeDirection;
		}
	}
	
	private static Vector3 realWorldInstantSwipeDirection;
	public static Vector3 RealWorldInstantSwipeDirection {
		get {
			return realWorldInstantSwipeDirection;
		}
	}

	private static Vector3 viewportFullSwipeDirection;
	public static Vector3 ViewportFullSwipeDirection {
		get {
			return viewportFullSwipeDirection;
		}
	}
	
	private static Vector3 viewportInstantSwipeDirection;
	public static Vector3 ViewportInstantSwipeDirection {
		get {
			return viewportInstantSwipeDirection;
		}
	}*/

	public static float swipeSpeed = 0f;
	public static float swipeDistance = 0f;
	public static float swipeSummedDistance = 0f;
	private static float swipeSmoothingSpeed = 55f;
	public static bool swipeValid = false;
	
	public static bool pinchActive = false;
	public static float pinchDistance = 0f;
	public static float pinchSpeed = 0f;
	public static Vector2 pinchCentriod;
	private static float pinchSmoothingSpeed = 55f; // 75% every frame at 60hz = 45

	// no min or max motion for the two finger tap. only timing.
	public static bool anyTouchActive = false;
	public static float anyTouchTime = 0;
	public static TwoFingerState twoFingerTapState = TwoFingerState.NONE;
	
	public static bool twoFingerTapTriggered = false;
	public static int twoFingerTapUniqueIndex = 0;	
	
	public static bool debugKeysActive = false;
	private static char[] debugKeysPassword = {'d','e','b','u','g'};
	private static int debugKeysPasswordIdx = 0;
	
	private static Vector3 prevMousePosition;

	// special metrics tracking.
	public static bool touchedButNoControl = false;
	
//    private static bool keyboardInputAllowed = true;
	public static void InitInput() {	
		touchHistory.Clear();
		
		iPad3 = ( CameraManager.ScreenWidth > 1024 );
/*		if( iPad3 ) {
			systemMaxTouchSqrDistForTap = maxTouchSqrDistForTapIPAD3;
			Debug.Log ("IPAD3");
		}
		else {
			systemMaxTouchSqrDistForTap = maxTouchSqrDistForTap;
			Debug.Log ("IPAD2");
		}*/
		debugKeysActive = AppBase.Instance.RunningAsPreview();
		
		debugKeysPasswordIdx = 0;

		prevMousePosition = CameraManager.MousePosition;

		ClearTouchData();
	}
	
	public static void ClearTouchData() {
		timeSinceMouseMovement = 0.0f;
		timeSinceAnyKeyPress = 0.0f;
		if( touchHistory != null ) {
			touchHistory.Clear();
		}
		
		touchActive = false;
		touchTime = 0f;		
		noTouchTime = 0f;
		
		anyTouchActive = false;
		anyTouchTime = 0f;
		
		twoFingerTapState = TwoFingerState.NONE;
		twoFingerTapTriggered = false;		
		
		pinchActive = false;
		pinchDistance = 0f;
		pinchSpeed = 0f;
		
		swipeValid = false;
		swipeSpeed = 0f;
		swipeDistance = 0f;
		swipeSummedDistance = 0f;
		
		// special metrics tracking.
		touchedButNoControl = false;
	}
	
	private static void DebugPasswordCheck() {
		if( debugKeysActive ) {
			return;
		}
		
		if( InputManager.GetKeyDown(debugKeysPassword[debugKeysPasswordIdx].ToString()) ) {
			debugKeysPasswordIdx++;
			if( debugKeysPasswordIdx >= debugKeysPassword.Length ) {
				debugKeysActive = true;
			}
		}
		else if( Input.anyKeyDown ) {
			debugKeysPasswordIdx = 0;
		}
	}
		
	public static bool NoTapPossible() {
		return ( touchTime > touchTapTime );
	}
	
	// hmmm . every frame?? gotta be a better way.
	static void UpdateTouchHistory( float dt ) {
        for( int i = 0; i < touchHistory.Count; i++ ) {
            TouchHistory t = touchHistory[i];
            if( t != null ) {
    			t.t += dt;
            }
		}
		
		bool done = false;
		while( !done ) {
			int cnt = touchHistory.Count;
			TouchHistory remove_me = null;
			for( int i = 0; i < cnt; i++ ) {
				TouchHistory t = touchHistory[i];
                if( t != null && t.t > keepTouchHistoryTime && cnt > 2 ) {
					remove_me = t;
					i = cnt;
				}
			}
			
			if( remove_me != null ) {
				touchHistory.Remove( remove_me );
			}
			else {
				done = true;
			}
		}
	}

	static void AddToTouchHistory( Vector3 pos, float speed ) {
		TouchHistory new_data = new TouchHistory();
		new_data.t = 0f;
		new_data.vp = pos;
		new_data.speed = speed;
		touchHistory.Add( new_data );
	}
	
	public static float TotalTouchSqrDistance() {		
		float total_d = 0f;
		if( touchHistory.Count > 1 ) {
			Vector3 p = touchHistory[0].vp;
			for( int i = 1; i < touchHistory.Count; i++ ) {
				Vector3 np = touchHistory[i].vp; 
				total_d += (p - np).sqrMagnitude;
				p = np;
			}
		}
		
		return total_d;
	}

	public static float TotalTouchDistance() {		
		return Mathf.Sqrt(TotalTouchSqrDistance());
	}

	public static Vector2 ClosestPointToTime( float tm ) {
		// find the closest, but not over time
		Vector2 ret_vec = touchOGPosition;
		
		float closest_t = tm;
        for( int i = 0; i < touchHistory.Count; i++ ) {
            TouchHistory t = touchHistory[i];
            if( t != null && t.t < tm ) {				
				float d = tm - t.t;
				if( d <= closest_t ) {
					closest_t = d;
					ret_vec = t.vp;
				}
			}
		}
		
		return ret_vec;
	}
	
	public static float SwipeLowestDotAngleOverTime( float tm ) {
		// find the closest, but not over time
		Vector2 og_pt = ClosestPointToTime( tm );
		Vector2 main_vec = touchLastPosition - og_pt;
		main_vec.Normalize();
		
		float lowest_dot = 1f;
				
        for( int i = 0; i < touchHistory.Count; i++ ) {
            TouchHistory t = touchHistory[i];
			if( t != null && t.t < tm ) {				
				Vector2 sub_vec = t.vp - og_pt;
				if( !MathfExt.Approx( sub_vec, Vector2.zero, 0.01f ) ) {
					sub_vec.Normalize();
					float dt = Vector2.Dot( sub_vec, main_vec );
					if( dt < lowest_dot ) {
						lowest_dot = dt;
					}
					
				}
			}
		}
		
		return lowest_dot;
	}
	
	public static Vector2 SwipeDirectionOverTime( float tm, out float distance, out float max_speed ) {
		// find the closest, but not over touch
		Vector2 p0 = touchOGPosition;
		
		max_speed = 0f;
		
		float closest_t = tm;
		bool setit = false;
        for( int i = 0; i < touchHistory.Count; i++ ) {
            TouchHistory t = touchHistory[i];
            if( t != null && t.t < tm ) {				
				float d = tm - t.t;
				if( d <= closest_t ) {
					setit = true;
					closest_t = d;
					p0 = t.vp;
				}
				if( max_speed < t.speed ) {
					max_speed = t.speed;
				}
			}
		}
	
		// low framerate.
		if( !setit ) {
//			Debug.Log("LOW FRAMERATE DIR CHECK! " + touchHistory.Count + " : " + tm );
			closest_t = tm * 4f;
            for( int i = 0; i < touchHistory.Count; i++ ) {
                TouchHistory t = touchHistory[i];
                if( t != null && t.t < tm ) {               
                	float d = tm - t.t;
					if( d <= closest_t ) {
						closest_t = d;
						p0 = t.vp;
					}
					if( max_speed < t.speed ) {
						max_speed = t.speed;
					}
				}
			}		
		}
		
		Vector2 dir = touchLastPosition - p0;
		distance = dir.magnitude;
		if( distance <= 0.0001f ) {
			dir = Vector2.zero;
			distance = 0f;
		}
		else {
			dir *= ( 1/distance );
		}
		return dir;
	}

	public static int NumTouches
	{
		get
		{
#if UNITY_EDITOR || UNITY_ANDROID
			return Input.GetMouseButton(0) ? 1 : 0;
#else
			return Input.touchCount;
#endif
		}
	}

	public static Vector3 GetTouchPosition(int i)
	{
#if UNITY_EDITOR || UNITY_ANDROID
		return Input.mousePosition;
#else				
		return Input.GetTouch(i).position;
#endif
	}

	static void UpdateTaps( float dt ) { 
		
		UpdateTouchHistory( dt );

// Debug logging for broken OVR touchpad
//        if (Input.GetMouseButton(0))
//        {
//            Debug.Log ("Mouse DOWN " + Input.mousePosition);
//        }
//        else
//        {
//            Debug.Log ("Mouse UP ");
//        }

		if( NumTouches == 1 ) {

			Vector3 cur = GetTouchPosition(0);
			cur = CameraManager.GetCurrentCamera().ScreenToViewportPoint( cur );
			
			noTouchTime = 0f;
			
			// actively touching .. or the single frame lapse we are allowing for better swipe controls..
			if( touchActive || swipeValid ) {
//				Debug.Log("TOUCH OR SWIPE CASE");
				touchActive = true;
				swipeValid = true;
				
				Vector2 vp = cur;
				Vector2 diff = vp - touchLastPosition;
				float cur_swipe_dist = diff.magnitude;
				float ratio = Mathf.Clamp01(swipeSmoothingSpeed * dt);
				float new_speed = cur_swipe_dist/dt;
				swipeSpeed *= (1.0f - ratio);
				swipeSpeed += (ratio * new_speed);
//				Debug.Log("CUR SwipeSPeed " + swipeSpeed);
				
				swipeSummedDistance += cur_swipe_dist;
				
				Vector3 dir = vp - touchOGPosition;				
				dir.z = 0f;
				float new_dist = dir.magnitude;
				if( new_dist > swipeDistance ) {
					swipeDistance = new_dist;
				}
//				Debug.Log("CUR SwipeSPeed " + swipeSpeed + "D " + swipeDistance + " :: T " + touchTime );
				
/*				viewportFullSwipeDirection = dir * ( 1f / swipeDistance );
				realWorldFullSwipeDirection = CameraManager.GetCurrentCameraTransform().localToWorldMatrix * viewportFullSwipeDirection;
				realWorldFullSwipeDirection.Normalize();
//				realWorldFullSwipeDirection.x *= -1f;
//				realWorldFullSwipeDirection.z *= -1f;
				dir = diff;
				dir.z = 0f;
				viewportInstantSwipeDirection = dir.normalized;
				realWorldInstantSwipeDirection = CameraManager.GetCurrentCameraTransform().localToWorldMatrix * dir;
				realWorldInstantSwipeDirection.Normalize();
//				realWorldInstantSwipeDirection.x *= -1f;
//				realWorldInstantSwipeDirection.z *= -1f;*/
				
/*				if(SphereCritters.Instance != null
					&& SphereCritters.Instance.playerCritterInfo != null
					&& SphereCritters.Instance.playerCritterInfo.critterTransform != null ) {
					Vector3 cp = SphereCritters.Instance.playerCritterInfo.critterTransform.position;
					Vector3 ep = cp + ( realWorldFullSwipeDirection * 100f );
					Debug.DrawLine(cp, ep, Color.grey); 
					ep = cp + ( realWorldInstantSwipeDirection * 100f );
					Debug.DrawLine(cp, ep, Color.red); 
				}
*/			
				
				touchLastPosition = vp;
				touchTime += dt;
//				Debug.Log("TOUCHING!!!");
			}
			else {	// touching again after not for 2 frames min.
//				Debug.Log("TOUCH!");
				touchActive = true;
				swipeValid = false;
				
				touchOGPosition = cur;
				
				touchLastPosition = touchOGPosition;
				touchTime = 0f;
				swipeSpeed = 0f;
				swipeDistance = 0f;
				swipeSummedDistance = 0f;
				touchHistory.Clear();

				// special metrics tracking.
				touchedButNoControl = true;

//				Debug.Log("TOUCH ACTIVE! CLEARING SWIPE DATA!");
			}
			
			if( touchTime > touchTapTime ) {
//				Debug.Log("TAP COUNT RESET TOUCH TIME EXCEED");
				tapCount = 0;
			}
			
			AddToTouchHistory( cur, swipeSpeed );
		}
		else {
			noTouchTime += dt;

			if( touchActive ) {
//				Debug.Log("TOUCH OFF");
				touchActive = false;
//				swipeValid = false;
//				touchHistory.Clear();
				
				if( NumTouches == 0  // only allow a tap if we no longer have any touches!
					&& touchTime <= touchTapTime ) {
					float d = TotalTouchSqrDistance();
//					Debug.Log ("TAP DIST" + d + " m " + maxTouchSqrDistForTap + " t " + touchTime);
					if( d <= maxTouchSqrDistForTap ) {
						lastTapPosition = touchLastPosition;
						if( !noTapTillNextTouch ) {								
							tapCount++;
							uniqueTapIndex++;
							if( __tapped != null ) {
								__tapped();
							}
//							Debug.Log ("TAP!!! " + tapCount);
						}
					}					
				}
//				touchTime = 0f;
//				swipeSpeed = 0f;
//				swipeDistance = 0f;
			}
			else {
//				Debug.Log("SWIPE OFF");
				swipeValid = false;
			}
		
#if UNITY_EDITOR || UNITY_ANDROID
			touchTapResetTime = 0.2f;
#endif
			if( timeSinceMouseMovement > touchTapResetTime ) {
//				Debug.Log("RESET TAP " + timeSinceMouseMovement );
				tapCount = 0;
			}
			
			if( NumTouches == 0 ) {
				noTapTillNextTouch = false;
			}
		}
	}
	
	static void UpdateTwoFingerTap( float dt ) {
		int tc = Input.touchCount;
		if( tc > 0 ) {
			if( !anyTouchActive ) {
				twoFingerTapState = TwoFingerState.NONE;
				anyTouchTime = 0f;
				anyTouchActive = true;
			}
			else {
				anyTouchTime += dt;
			}
			
			switch( twoFingerTapState ) {
				case TwoFingerState.NONE:
				{
					if( tc == 2 ) {
//						Debug.Log("TWO FINGER -> NONE TO GOOD! :: " + tc);
						twoFingerTapState = TwoFingerState.GOOD;
					}
					else if( tc > 2 ) {
//						Debug.Log("TWO FINGER -> NONE TO FAIL! :: " + tc);
						twoFingerTapState = TwoFingerState.FAILED;
					}					
					break;
				}
				case TwoFingerState.GOOD:
				{
					if( tc > 2 ) {
//						Debug.Log("TWO FINGER -> GOOD TO FAIL! :: " + tc);
						twoFingerTapState = TwoFingerState.FAILED;
					}
					break;
				}
				case TwoFingerState.FAILED:
				default:
				{
					break;
				}				
			}
		}
		else {
			if( anyTouchActive ) {
				if( twoFingerTapState == TwoFingerState.GOOD && anyTouchTime <= touchTapTime ) {
					twoFingerTapTriggered = true;
					twoFingerTapUniqueIndex++;
				}
				anyTouchActive = false;
				anyTouchTime = 0f;
			}
			else {
				twoFingerTapTriggered = false;
			}
		}
		
/*#if UNITY_EDITOR
		if( Input.GetKeyDown("space") ) {
			twoFingerTapTriggered = true;
			twoFingerTapUniqueIndex++;		
		}
#endif*/
	}
	
	static void UpdatePinch( float dt ) {
		bool pinch_active = false; // used so we can test using mouse 1 and the screen center for pinch

		Vector3 fp = Vector3.zero, sp = Vector3.zero;
		if( Input.touchCount == 2 ) {
			Touch f = Input.GetTouch(0);
			fp = CameraManager.GetCurrentCamera().ScreenToViewportPoint( f.position );			
			fp.z = 0f;
			
			Touch s = Input.GetTouch(1);
			sp = CameraManager.GetCurrentCamera().ScreenToViewportPoint( s.position );
			sp.z = 0f;
			pinch_active = true;
		}

#if UNITY_EDITOR || UNITY_ANDROID
		if( Input.GetMouseButton( 1 ) ) {
			fp = CameraManager.GetCurrentCamera().ScreenToViewportPoint( Input.mousePosition );			
			fp.z = 0f;
			
			Vector3 center = new Vector3( 0.5f, 0.5f, 0f );
			Vector3 to_center = center - fp;
			sp = fp + ( to_center * 2f );
			pinch_active = true;
		}
#endif	
		if( pinch_active ) {
			Vector3 pinch_dir = fp - sp;
//			Debug.Log(" FP " + fp + " :: SP " + sp);
			float new_pinch_distance = pinch_dir.magnitude;
			pinchCentriod = sp + ( pinch_dir * 0.5f );
			if( !pinchActive ) {
				pinchSpeed = 0;
				pinchDistance = new_pinch_distance;
				pinchActive = true;
			}
			else {
				float ratio = Mathf.Clamp01(pinchSmoothingSpeed * dt);
				float og = pinchDistance;
				pinchDistance *= (1.0f - ratio);
				pinchDistance += (ratio * new_pinch_distance);
				pinchSpeed = (pinchDistance - og)/dt;
			}
		}
		else {
			pinchActive = false;
			pinchDistance = 0f;
			pinchSpeed = 0f;
		}
	}
	
	public static void UpdateInput( float dt ) {

//		DebugPasswordCheck();
		
		if( Input.anyKey ) {
			timeAnyKeyHeld += dt;
			timeSinceAnyKeyPress = 0.0f;
		}
		else {
			timeAnyKeyHeld = 0f;
			timeSinceAnyKeyPress += dt;
		}
		
		Vector3 new_position = CameraManager.MousePosition;
		if( new_position != prevMousePosition
			|| NumTouches > 0
			) { 
			timeSinceMouseMovement = 0.0f;
		}
		else {
			timeSinceMouseMovement += dt;
		}
		
//		UpdateTaps( dt );
//		UpdatePinch( dt );
//		UpdateTwoFingerTap( dt );		// TODO>this should be folded into a general multi finger tap..

        UpdateMoonlightBackButton( dt );

		// handle hard restart/reset
		if( Input.GetKeyDown("r")
		   || ( timeAnyKeyHeld > 0.4f && Input.GetKey(KeyCode.Joystick4Button4) && Input.GetKey(KeyCode.Joystick4Button5 ) ) ) {
//			AppBase.Instance.RestartApp();
//            AppBase.Instance.DelayedRestartApp(3f);
            AppBase.Instance.DelayedReload(3f);
        }

		prevMousePosition = new_position;
	}

	public static bool AnyMoonlightBackTap() {

		if (moonlightBackLastFrameInput == MoonlightBackInputType.ShortTap || moonlightBackLastFrameInput == MoonlightBackInputType.LongTap)
			return true;

		return false;
	}

    public static MoonlightBackInputType MoonlighBackTap()
    {
        return moonlightBackLastFrameInput;
    }

    static void UpdateMoonlightBackButton(float dt)
    {
		// ee edit for valve.
		return;
        moonlightBackLastFrameInput = MoonlightBackInputType.None;

        if (Input.GetKey(KeyCode.Escape))
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                moonlightBackHoldTime = 0f;

                if (moonlightBackLastDownTime > 0f &&
                    moonlightBackLastDownTime >= (Time.time - 0.25f))
                {
                    // double tap
//                    Debug.Log ("Moonlight Back DOUBLE TAP");
                    moonlightBackLastFrameInput = MoonlightBackInputType.DoubleTap;
                    moonlightBackTapHandled = true;
                    OVRDevice.ResetOrientation();
                }
                else
                {
                    moonlightBackLastDownTime = Time.time;
                }
            }

            moonlightBackHoldTime += dt;

            if (!moonlightBackTapHandled &&
                moonlightBackHoldTime >= 0.75f)
            {
                // long tap
//                Debug.Log ("Moonlight Back LONG TAP");
                moonlightBackLastFrameInput = MoonlightBackInputType.LongTap;               
                moonlightBackTapHandled = true;
            }
        }
        else 
        {
            if (!moonlightBackTapHandled &&
                moonlightBackHoldTime > 0f &&
                moonlightBackHoldTime < 0.75f)
            {
                if (moonlightBackLastDownTime < (Time.time - 0.25f))
                {
                    // short tap, no doublt tap
//                    Debug.Log ("Moonlight Back SHORT TAP");
                    moonlightBackLastFrameInput = MoonlightBackInputType.ShortTap;
                    moonlightBackHoldTime = 0f;
                }
            }
            else
            {
                moonlightBackHoldTime = 0f;
                moonlightBackTapHandled = false;
            }
        }
    }

    public static bool GetKeyDown( string key )
    {
        return /*KeyboardInputAllowed &&*/ Input.GetKeyDown( key );
    }
    
    public static bool GetKeyDown( KeyCode key )
    {
        return /*KeyboardInputAllowed &&*/ Input.GetKeyDown( key );
    }

    public static void OnApplicationPause( bool pause ) {
        moonlightBackLastFrameInput = MoonlightBackInputType.None;
        moonlightBackTapHandled = false;
        moonlightBackHoldTime = 0f;
    }
}
