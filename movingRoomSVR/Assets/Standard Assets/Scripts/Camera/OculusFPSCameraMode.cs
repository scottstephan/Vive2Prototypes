using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OculusFPSCameraMode : BaseCameraMode {

	const float TOUCH_TAP_TIME = 0.3f;

    public enum TouchPadMoveMode
    {
		None,
        ToggleTap,
        Hold,
        HoldSwipe
    }

    public enum TouchMoveState
    {
        None,
        Forward,
        Back,
        Up, 
        Down,
        FastForward,
        Boost
    }

	public bool debugMode = false;
	
	private float FPStrafeSpeed = 30.0f;
	private float FPVerticalSpeed = 10f;
    private float FPForwardSpeed = 42.0f; //adjusted on 5/19/14 from 60 to 42 to slow player down by ~30%

    private float accel = 42.0f * 10f;
    private float decel = -42.0f * 5f;

    [HideInInspector]
    public Vector3 _curSpeed = Vector3.zero;

    private TouchMoveState touchMoveState = TouchMoveState.None;

	public float maxCameraDistance = -1f;	// This keeps the player from moving too far from the center of the world
	public float maxCameraHeight = -10.0f;
	
    public TouchPadMoveMode TouchPadMove = TouchPadMoveMode.Hold;

	public static bool disableMovement = false;

    float touchMoveThreshold = 12f;
	float touchDownTimer;
    float touchStillAnyTimer;
    float touchStillThresholdTimer;
    float touchHoldSwipeMinTime = 0.05f;
    Vector3 startTouchPos;
    Vector3 lastTouchPos;
    bool bWasTouchDown;

    bool bUpdated;
    public Vector3 lastDesiredSpeed;
    private Transform ovrTransform;
    public float physicsAccel = 100f;
    public float maxSpeed = 50f;

	// fx to highlight fish
	public ParticleSystem fxRings;
	public pointConstraint fxConstraint;
	public PlayerFeelFX playerFeelFX;

    OculusAreaFadeTrigger fadeOutsideTrigger;
    Vector3 fadeOutsideStartPos;
    Vector3 fadeOutsideOrigin;
    float fadeOutsideDist = 200f;

    static public OculusFPSCameraMode singleton;

	//behavior detection bools for hints
//	static bool _hasMoved = false;
	static bool _hasInteracted = false;

//	float _tutorialDelay = 5f;
//	float _lastTutorialAttempt;

	static bool _isMoving = false;

	float _tutorialTimestamp;
//	float _tutorialShowtime = 4f;

    const float BOOST_SPEED_MAX = 53f;
    const float BOOST_SPEED_MIN = 53f;
    const float BOOST_ANGLE = 30f * Mathf.Deg2Rad;
    const float BOOST_STOPDIST = 15f;
    const float BOOST_STOPTIME = 0.3f;
    const float BOOST_TIME = 4f;

    CritterInfo boostCritter;
    float boostTimer;

	WemoItemData pickedItem;
	WemoItemData lastPickedItem;
    int pickItemFrame;

    float desiredFadeAlpha = 0f;
    float currentFadeAlpha = 0f;

    GameObject mercyKillMessage;
//    Renderer mercyKillRenderer;

    Collider myCollider = null;

    #region Title Sequence
	public float startDelay = 3f;
	float startDelayTimer = -1f;
	
    public float fadeInSeconds = 2f;
    public float fadeInBetweenSeconds = 3f;
    public float fadeOutSeconds = 1f;
    public float fadeOutBlackSeconds = 1.5f;
    float fadeInTimer = -1f;
    float fadeOutTimer = -1f;
    float fadeOutBlackTimer = -1f;
	
	bool fadeInStarted;
    float fadeInBetweenTimer; // in between fade in and fade out
	bool fadeOutStarted;
	bool titleSeqDone;
    bool titleSeqStarted;
    public bool isEndTitleOn;
	public bool didEndTitleStarted; // we need this since we are delaying the endTitle. sometimes user trigger the HideEndTitle() before we got to start fading it in
    bool isEndTitleFading;
    float endTitleFadeDir = 1f;
    float endTitleAlpha = 0f;
	public float endTitleDelay = 1.5f;
	float warningTitleTimer = 999999f; // this timer prevents us from launching the wemoMedia title , before the warning is off (the issue is that the warning is not on right away
	
	public GameObject introTitle;
    public GameObject endTitle;
	List<Material> introTitleMaterials = new List<Material>();
    public GameObject blackBox; // this is the box of planes for fade in / out
    public Material blackBoxMaterial;
    List<Material> endTitleMaterials = new List<Material>();



	#endregion

	const uint CAPS_NOVSYNC = 0x1000;
    enum SoundState
    {
        Init,
        Still,
        Moving
    }

    SoundState soundState;

    Quaternion originalRotation;
	
    public void Awake()
    {
        singleton = this;

//		_lastTutorialAttempt = Time.time;

        myCollider = GetComponent<Collider>();

        mercyKillMessage = GameObject.Find ("MercyKillMessage");
        if (mercyKillMessage != null)
        {
            mercyKillMessage.SetActive(false);

//            if (mercyKillMessage != null)
//            {
//                mercyKillRenderer = mercyKillMessage.renderer;
//            }
//            else
//            {
//                mercyKillRenderer = mercyKillMessage.GetComponentInChildren<Renderer>();
//            }
        }
    }

    public void ToggleColliderCollision( bool on ) {
        if( myCollider == null ) {
            return;
        }

        myCollider.enabled = on;
    }

    public void PauseMovementSound( bool on ) {
        AudioManager.Instance.PauseSFX( SoundFXID.Still, on );
        AudioManager.Instance.PauseSFX( SoundFXID.TravelMoving, on );
    }

    public void UpdateMovementSound( float mag ) {
        const float fadeInSFXTime = 0.5f;
//        const float fadeOutSFXTime = 1f;
        AudioManager audioMgr = AudioManager.Instance;

        if( !SimInstance.Instance.slowdownActive && !SimInstance.Instance.IsSimPaused() ) {
            if( !audioMgr.IsPlaying(SoundFXID.Still))
            {
                AudioManager.FadeInSFX(SoundFXID.Still, fadeInSFXTime);
            }
            
            if( !audioMgr.IsPlaying(SoundFXID.TravelMoving))
            {
                AudioManager.FadeInSFX(SoundFXID.TravelMoving, fadeInSFXTime);
            }
        }
        
        const float low = 0.4f;
        const float high = 1.1f;
        
        if( mag > 0.0001f)
        {
            if (soundState != SoundState.Moving)
            {
                soundState = SoundState.Moving;
            }

//            Debug.Log ( mag );
            float ratio = mag / 3f;
            if (ratio > 1f)
            {
                ratio = 1f;
            }

            float pitch = low + ( ratio * ( high - low ) );

            audioMgr.PitchAudio( SoundFXID.TravelMoving, pitch );
        }
        else {
            if (soundState != SoundState.Still)
            {
                soundState = SoundState.Still;
            }

            audioMgr.PitchAudio( SoundFXID.TravelMoving, low );
        }
    }
       
	public override void InitCameraMode() 
	{
        if( inited ) 
		{
            return;
        }
        
        base.InitCameraMode();

        if (CameraManager.singleton.ovrRB != null)
        {
            ovrTransform = CameraManager.singleton.ovrRB.transform;
        }
 
        myTransform = transform;

        cameraType = CameraType.OculusCamera;

        if (GetComponent<Rigidbody>())
		{
            GetComponent<Rigidbody>().freezeRotation = true;
		}

        originalRotation = myTransform.localRotation;
		
		cameraName = "Oculus Camera";

        runCollision = CameraCollisionType.None;

		playerFeelFX = CameraManager.singleton.GetComponent<PlayerFeelFX>();


#if UNITY_ANDROID && !UNITY_EDITOR
        if (PlayerPrefs.GetInt("controltype") == 0)
        {
    		TouchPadMove = TouchPadMoveMode.Hold;
        }
        else
        {
            TouchPadMove = TouchPadMoveMode.ToggleTap;
        }
#else
		TouchPadMove = TouchPadMoveMode.None;
#endif

//		_lastTutorialAttempt = Time.time;

        ToggleColliderCollision(false);
	}
	
    bool IsMoveTouchDown()
    {
		if (TouchPadMove == TouchPadMoveMode.Hold)
		{
			return touchDownTimer > TOUCH_TAP_TIME;
		}

        return Input.GetMouseButtonDown(0);
    }

	bool IsMoveTouchActive()
	{
        return Input.GetMouseButton(0);
	}

	bool IsMoveTouchUp()
    {
        return Input.GetMouseButtonUp(0);
    }

	public bool IsInteractButtonDown()
	{
		if (Input.GetButtonDown("Interact"))
		{
			return true;
		}

		if (TouchPadMove == TouchPadMoveMode.Hold ||
            TouchPadMove == TouchPadMoveMode.HoldSwipe)
		{
            if (Input.GetMouseButtonUp(0) && touchDownTimer <= TOUCH_TAP_TIME)
            {
                return true;
            }
		}
		else if (TouchPadMove != TouchPadMoveMode.None)
		{
            if (Input.GetMouseButtonDown(0))
            {
                return true;
            }
		}

		return false;
	}

	public override void StartCameraMode()
    {
		//temp
		Cursor.visible = false;

//		InputManager.__tapped += TapEvent;
        bUpdated = false;
        soundState = SoundState.Init;
        StartPosition();
        SetupTitleSeq();

        startTouchPos = Vector3.zero;
        lastTouchPos = Vector3.zero;
        bWasTouchDown = false;
        touchDownTimer = 0f;
        touchStillAnyTimer = 0f;
        touchStillThresholdTimer = 0f;
        pickItemFrame = 0;
        pickedItem = null;
        CameraManager.singleton.ovrPhysicsMove = false;
		if(CameraManager.singleton.ovrRB != null)
        {
			CameraManager.singleton.ovrRB.isKinematic = true;
        }

        ToggleColliderCollision(true);
	}
	void SetupTitleSeq()
	{
		//trying to fix tearing
		//OVRDevice.HMD.SetEnabledCaps(CAPS_NOVSYNC);
		
		startDelayTimer = startDelay;
		OculusCameraFadeManager.FadeToBlack(0f);
		
		//introTitle = GameObject.Find("IntroTitle");
		if (introTitle != null)
			BuildTitleMaterialList(introTitle);
		if (endTitle != null)
			BuildEndTitleMaterialList(endTitle);
		if (introTitle != null)
			introTitle.SetActive(false);
        FadeBlackBox(1f);
	}
    public override void IntroDownFadeFinished() 
    {
        StartPosition();
    }

    void StartPosition()
    {
        if (CameraManager.GetPreviousCameraType () == CameraType.OculusFollowCamera) 
        {
            CameraData cd = CameraManager.GetCameraData(CameraType.OculusFollowCamera);
            SetPositionOrientation(cd.cameraTransform.position, cd.cameraTransform.rotation, false);

//            CameraManager.singleton.ovrRB.velocity = OculusFollowCameraMode.singleton.lastDesiredSpeed;
            if (CameraManager.singleton.ovrPhysicsMove)
            {
                CameraManager.singleton.ovrRB.velocity = OculusFollowCameraMode.singleton.lastDesiredSpeed;
            }
            else
            {
                _curSpeed = OculusFollowCameraMode.singleton.lastDesiredSpeed;
            }
        }
        else if (CameraManager.GetPreviousCameraType() == CameraType.OculusTourCamera) 
        {
        }
        else
        {
            Transform oc = CameraManager.GetOculusStartTransformExpensive ();

            if (oc != null)
            {
                SetPositionOrientation(oc.position, oc.rotation, false);
//disable reset orient b/c we're just gonna go to tour mode                SetPositionOrientation(oc.position, oc.rotation, true);
            }
            else 
            {
                SetPositionOrientation(CameraManager.singleton.OVRCameraParent.position, CameraManager.singleton.OVRCameraParent.rotation, false);
            }
        }       
    }

    void SetPositionOrientation(Vector3 v, Quaternion q, bool bResetDevice)
    {
        myTransform.position = v;
        myTransform.rotation = q;
		 
        if (bResetDevice && (CameraManager.singleton != null))
        {
            CameraManager.singleton.SetYRotation( q.eulerAngles.y );
            OVRDevice.ResetOrientation();
        }

        if (CameraManager.singleton.ovrPhysicsMove)
        {
            CameraManager.singleton.OVRCameraParent.position = myTransform.position;
        }

        CameraManager.UpdateCurrentCameraTransform();
    }

	public override void EndCameraMode() 
    {
/*        if (touchMoveState == TouchMoveState.Boost && playerFeelFX != null)
        {
            playerFeelFX.StopBoost(); 
        }*/

        if (pickedItem != null)
        {
			if (pickedItem.critterInfo.swimPlayerViewData == null) {
				pickedItem = null;
			}
			else { //RALPH HACK!?

            	pickedItem.critterInfo.swimPlayerViewData.timer = 0f;
            	pickedItem.critterInfo.generalSpeciesData.switchBehavior = true;
            	pickedItem = null;
			}
        }

        if (CameraManager.singleton.fxFrame != null)
        {
            CameraManager.singleton.fxFrame.Clear();
        }

//		InputManager.__tapped -= TapEvent;
        if (CameraManager.singleton.ovrPhysicsMove)
        {
            CameraManager.singleton.ovrRB.velocity = Vector3.zero;
        }

		if (!OculusCameraFadeManager.IsFaded() && 
            !OculusCameraFadeManager.IsFading())
        {
			StopAreaFade( true );
        }

        ToggleColliderCollision(false);
	}

    void SetTouchPadMoveState(TouchMoveState state)
    {
        if (touchMoveState == state)
        {
            return;
        }

/*        if (playerFeelFX != null)
        {
            if (touchMoveState == TouchMoveState.Boost)
            {
                playerFeelFX.StopBoost(); 
            }
            else if (state == TouchMoveState.Boost)
            {
                playerFeelFX.StartBoost(); 
            }
        }*/

        touchMoveState = state;
    }

    void UpdateEndTitle(){
        if(!isEndTitleFading)
            return;
        endTitleAlpha +=  endTitleFadeDir * Time.deltaTime * 0.7f;
        FadeEndTitle(endTitleAlpha);
		//Debug.Log("UpdateEndTitle " + endTitleAlpha);
		if(endTitleAlpha >=1f && endTitleFadeDir == 1f){
			isEndTitleFading = false;
			didEndTitleStarted = true;
			Debug.Log("Done fading in");
		}
        if(endTitleAlpha <= 0f && endTitleFadeDir == -1f){
			isEndTitleFading = false;
			isEndTitleOn = false;
			endTitle.SetActive(false);
			didEndTitleStarted = false;
			Debug.Log("Done fading out");
        }
		endTitleAlpha = Mathf.Clamp(endTitleAlpha, 0f, 1f);
    }


	public void WarningTitleDismissed(){
		Debug.Log("WarningTitleDismissed");
		warningTitleTimer = 0.5f;
	}

    public bool WasWarningTitleDismissed()
    {
        return warningTitleTimer <= 0.5f;
    }

	bool UpdateTitleSeq()
	{
		if (titleSeqDone)
		{
			return false;
		}

		warningTitleTimer -= Time.deltaTime;
        //if(!OVRMainMenu.IsWarningActive() && Input.anyKey && !titleSeqStarted){
		if( !titleSeqStarted && warningTitleTimer < 0f){
			Debug.Log("ok , we can start fade in wemoMedia title now");
			titleSeqStarted = true;
            startDelayTimer = 0.2f;
        }
        if(!titleSeqStarted)
            return true;

        if (startDelayTimer > 0f )
		{
			startDelayTimer -= Time.deltaTime;
			if (startDelayTimer <= 0)
			{
				Debug.Log("fade black in updateTitleSeq");
				OculusCameraFadeManager.FadeToTransparent(fadeInSeconds);
                fadeOutTimer = fadeInSeconds;// set this to wait till the occulus warning is faded out
			}
        }
        else if (fadeOutTimer > 0f && !fadeInStarted){ // we are waiting to occulus to fade out before fading in our title
            fadeOutTimer -= Time.deltaTime;
            return true; 
        }
        else if (!fadeInStarted) // start fading in
        {
            //Debug.LogError("Start Fading IN " + Time.time);
            fadeInStarted = true;
            fadeOutTimer = fadeInSeconds;
            FadeIntroTitle(0f);
            introTitle.SetActive(true);
        }
        else if(fadeInStarted && !fadeOutStarted && fadeInBetweenTimer <= 0f){ // this is the fade in loop
            fadeOutTimer -= Time.deltaTime;
            if(fadeOutTimer < 0f){
                fadeInBetweenTimer = fadeInBetweenSeconds;
            }
            else{
                FadeIntroTitle(1f - fadeOutTimer/fadeInSeconds);
            }
        }
        else if (fadeInBetweenTimer > 0f){ // this is the in between. we don't use a timer anymore, just waiting for a click
            //fadeInBetweenTimer -= Time.deltaTime;
            //if(fadeInBetweenTimer < 0f){
			if(Input.anyKey){
               // Debug.LogError("fade in between complete " + Time.time);
				fadeInBetweenTimer = -1f;
                fadeOutStarted = true;
                fadeOutTimer = fadeOutSeconds ;
                fadeOutBlackTimer = fadeOutBlackSeconds;
            }
        }
        else if(fadeOutStarted){ // that's the fade out loop
            fadeOutTimer -= Time.deltaTime;
            fadeOutBlackTimer -= Time.deltaTime;
            if(fadeOutBlackTimer < 0f){
                titleSeqDone = true;
                //Debug.LogError("title seq done " + Time.time);
                introTitle.SetActive(false);
                blackBox.SetActive(false);
            }
            else{
                FadeIntroTitle(fadeOutTimer / fadeOutSeconds);
                //fontMaterial.SetColor("_Color", new Color(1f, 1f, 1f, fadeOutTimer / fadeOutSeconds));
                FadeBlackBox(fadeOutBlackTimer / fadeOutBlackSeconds    );
            }
        }
        return true;

	}


    void FadeIntroTitle(float alpha){
        for (int i=0; i < introTitleMaterials.Count; ++i)
        {
            Color c = introTitleMaterials[i].color;
            c.a = alpha;
            introTitleMaterials[i].color = c;
            introTitleMaterials[i].SetColor("_Color", c);
        }
    }
    void FadeEndTitle(float alpha){
        for (int i=0; i < endTitleMaterials.Count; ++i)
        {
            Color c = endTitleMaterials[i].color;
            c.a = alpha;
            endTitleMaterials[i].color = c;
            endTitleMaterials[i].SetColor("_Color", c);
        }
    }
    
    void FadeBlackBox(float alpha){
		if (blackBoxMaterial != null)
		{
	        blackBoxMaterial.color = new Color(0f, 0f, 0f, alpha);
		}
    }
    
    void BuildTitleMaterialList(GameObject g)
	{
		if (g == null)
		{
			return;
		}
		
		if (g.GetComponent<Renderer>() != null && g.GetComponent<Renderer>().material != null) 
		{
			introTitleMaterials.Add(g.GetComponent<Renderer>().material);
		}
		
		int cnt = g.transform.childCount;
		for (int i =0; i < cnt; ++i)
		{
			BuildTitleMaterialList(g.transform.GetChild(i).gameObject);
		}
	}
    void BuildEndTitleMaterialList(GameObject g)
    {
        if (g == null)
        {
            return;
        }
        
        if (g.GetComponent<Renderer>() != null && g.GetComponent<Renderer>().material != null) 
        {
            endTitleMaterials.Add(g.GetComponent<Renderer>().material);
        }
        
        int cnt = g.transform.childCount;
        for (int i =0; i < cnt; ++i)
        {
            BuildEndTitleMaterialList(g.transform.GetChild(i).gameObject);
        }
    }
    
    public void ShowEndTitle(){
        if(isEndTitleOn)
            return;
		isEndTitleOn = true;
		endTitleFadeDir = 1f;
		Invoke("ShowEndTitleNow", endTitleDelay);
        
        CameraManager.singleton.RestartCurrentCamera();
        CameraManager.singleton.SetYRotation(0f);
        OVRDevice.ResetOrientation();
    }

	public void ShowEndTitleNow(){
		Debug.Log("ShowEndTitleNow ");
		FadeEndTitle(0f);
		endTitleAlpha = 0f;
		endTitle.SetActive(true);
		isEndTitleFading = true;
	}

    public void HideEndTitle(){
        if(!isEndTitleOn)
            return;
		Debug.Log("HideEndTitle ");
        isEndTitleFading = true;
        endTitleFadeDir = -1f;

        AppBase.Instance.RestartApp();
        OculusCameraFadeManager.FadeToTransparent(1.6f);
        CameraManager.singleton.SetYRotation(0f);
        OVRDevice.ResetOrientation();
    }

	public override void UpdateCameraMode() 

    {
        if (TouchPadMove == TouchPadMoveMode.Hold)
        {
            if (IsMoveTouchDown())
            {
                SetTouchPadMoveState(TouchMoveState.Forward);
            }

            if (IsMoveTouchUp())
            {
                SetTouchPadMoveState(TouchMoveState.None);
            }
        }
        else if (TouchPadMove == TouchPadMoveMode.HoldSwipe)
        {
//            float mx = Input.GetAxisRaw("Mouse X");
//            float my = Input.GetAxisRaw("Mouse y");

//            if (mx != 0f ||
//                my != 0f ||
//                Input.GetMouseButton(0) ||
//                Input.GetButton("InteractMouse"))
//            {
//                Debug.Log ("Touch " +curFrame + " X:"+Input.GetAxisRaw("Mouse X") + " Y:" + Input.GetAxisRaw("Mouse Y") + " m:" +Input.GetMouseButton(0) + " btn:"+Input.GetButton("InteractMouse"));            
//            }

            if (IsMoveTouchDown())
            {
                if (!bWasTouchDown)
                {
                    touchStillThresholdTimer = 0f;
                    touchStillAnyTimer = 0f;
                    bWasTouchDown = true;
                    startTouchPos = lastTouchPos = Input.mousePosition;
                    SetTouchPadMoveState(TouchMoveState.None);
//					_hasMoved = true;
                }
            }

            if (bWasTouchDown)
            {
                if (IsMoveTouchUp())
                {
                    bWasTouchDown = false;
                    SetTouchPadMoveState(TouchMoveState.None);
                }
                else
                {
                    Vector2 mouseDelta = Input.mousePosition - startTouchPos;
                    Vector2 mouseFrameDelta = Input.mousePosition - lastTouchPos;
                    float moveAmt = mouseFrameDelta.magnitude;

                    if (moveAmt >= 2f)
                    {
                        touchStillAnyTimer = 0f;
                    }
                    else
                    {
                        touchStillAnyTimer += Time.deltaTime;
                    }

                    if ((touchStillAnyTimer > touchHoldSwipeMinTime || touchDownTimer > 0.2f) &&
                        touchMoveState == TouchMoveState.None)
                    {
                        SetTouchPadMoveState(TouchMoveState.Forward);
                    }

                    if (moveAmt < touchMoveThreshold)
                    {
                        touchStillThresholdTimer += Time.deltaTime;

                        if (touchStillThresholdTimer > 0.35f)
                        {
                            startTouchPos = Input.mousePosition;
                        }
                    }
                    else
                    {
                        touchStillThresholdTimer = 0f;

                        if (Mathf.Abs (mouseDelta.x) > Mathf.Abs(mouseDelta.y))
                        {
                            if (mouseDelta.x < 0f)
                            {
                                SetTouchPadMoveState(TouchMoveState.FastForward);
                            }
                            else if (mouseDelta.x > 0f)
                            {
                                SetTouchPadMoveState(TouchMoveState.Back);
                            }
                        }
                        else
                        {
                            if (mouseDelta.y > 0f)
                            {
                                SetTouchPadMoveState(TouchMoveState.Up);
                            }
                            else if (mouseDelta.y < 0f)
                            {
                                SetTouchPadMoveState(TouchMoveState.Down);
                            }
                        }
                    }

                    lastTouchPos = Input.mousePosition;
                }
            }            
        }
        else if (TouchPadMove == TouchPadMoveMode.ToggleTap)
        {
            if (IsMoveTouchDown() && (pickedItem == null || !pickedItem.vrInteractable))
            {
                SetTouchPadMoveState(touchMoveState == TouchMoveState.Forward ? TouchMoveState.None : TouchMoveState.Forward);
            }
        }

		Vector3 desiredSpeed = GetDesiredSpeed();
        lastDesiredSpeed = desiredSpeed;

        //Debug.Log("SPEED " + _curSpeed + " ACCEL " + _curAccel + " DESIRED SPEED " + desiredSpeed);

        Vector3 translateSpeed = Vector3.zero;

        float mag = desiredSpeed.sqrMagnitude;

        UpdateMovementSound( mag );

        if ( mag > 0.0001f )
		{
            Vector3 deltaSpeed = (desiredSpeed - _curSpeed).normalized;
            _curSpeed += deltaSpeed * (accel * Time.deltaTime);
            if ((deltaSpeed.x > 0f && _curSpeed.x > desiredSpeed.x) ||
                (deltaSpeed.x < 0f && _curSpeed.x < desiredSpeed.x))
            {
                _curSpeed.x = desiredSpeed.x;
            }

            if ((deltaSpeed.y > 0f && _curSpeed.y > desiredSpeed.y) ||
                (deltaSpeed.y < 0f && _curSpeed.y < desiredSpeed.y))
            {
                _curSpeed.y = desiredSpeed.y;
            }

            if ((deltaSpeed.z > 0f && _curSpeed.z > desiredSpeed.z) ||
                (deltaSpeed.z < 0f && _curSpeed.z < desiredSpeed.z))
            {
                _curSpeed.z = desiredSpeed.z;
            }

            translateSpeed = _curSpeed;
            
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.LeftShift))
            {
                translateSpeed *= 4f;
            }
#endif
//			Debug.Log ("CURSPEED ACCEL " + _curSpeed + " ACCEL="+accel);

			_isMoving = true;
		}
		else if (_curSpeed.sqrMagnitude > 0.0001f)
		{
			Vector3 decelAmt = _curSpeed * (decel * Time.deltaTime);

            // if decel will drop current speed to start moving negative to the way we were moving,
            // cut decel changes to min necessary to drop speed to 0

            if (_curSpeed.x <= 0.0f && decelAmt.x > -_curSpeed.x)
            {
                decelAmt.x = -_curSpeed.x;
            }
            else if (_curSpeed.x > 0.0f && decelAmt.x < -_curSpeed.x)
            {
                decelAmt.x = -_curSpeed.x;
            }

            if (_curSpeed.y <= 0.0f && decelAmt.y > -_curSpeed.y)
            {
                decelAmt.y = -_curSpeed.y;
            }
            else if (_curSpeed.y > 0.0f && decelAmt.y < -_curSpeed.y)
            {
                decelAmt.y = -_curSpeed.y;
            }

            if (_curSpeed.z <= 0.0f && decelAmt.z > -_curSpeed.z)
            {
                decelAmt.z = -_curSpeed.z;
            }
            else if (_curSpeed.z > 0.0f && decelAmt.z < -_curSpeed.z)
            {
                decelAmt.z = -_curSpeed.z;
            }

            _curSpeed.x = Mathf.Clamp(_curSpeed.x + decelAmt.x, -FPStrafeSpeed, FPStrafeSpeed);
            _curSpeed.y = Mathf.Clamp(_curSpeed.y + decelAmt.y, -FPVerticalSpeed, FPVerticalSpeed);
            _curSpeed.z = Mathf.Clamp(_curSpeed.z + decelAmt.z, -FPForwardSpeed, FPForwardSpeed);
            translateSpeed = _curSpeed;           
//            Debug.Log ("CURSPEED DECEL " + _curSpeed + " DECEL="+decelAmt);

			_isMoving = true;
		}
        else
        {
			_isMoving = false;
        }

        if (CameraManager.singleton.ovrPhysicsMove)
        {
            // update based on physics results

			if (ovrTransform != null)
           		myTransform.position = ovrTransform.position;
        }
        else
        {
            Vector3 move_amt = myTransform.rotation * (translateSpeed * Time.deltaTime);
            CameraManager.singleton.ovrCharCtrl.Move(move_amt);
            myTransform.position = CameraManager.singleton.OVRCameraParent.position;
//            myTransform.position += move_amt;	
        }

		// pull orientation from Oculus and apply it to this camera
		CameraManager.singleton.GetCameraOrientation(ref originalRotation);
		myTransform.rotation = originalRotation;

        if (Input.GetButtonDown("ToggleMovementMode"))
        {
            SetTouchPadMoveState(TouchMoveState.None);
            
            if (TouchPadMove == OculusFPSCameraMode.TouchPadMoveMode.Hold)
            {
                TouchPadMove = OculusFPSCameraMode.TouchPadMoveMode.HoldSwipe;
            }
            else if (TouchPadMove == OculusFPSCameraMode.TouchPadMoveMode.HoldSwipe)
            {
                TouchPadMove = OculusFPSCameraMode.TouchPadMoveMode.ToggleTap;
            }
            else 
            {
				TouchPadMove = OculusFPSCameraMode.TouchPadMoveMode.Hold;
            }

            Debug.Log("TOUCHPAD MOVE = " + TouchPadMove.ToString());
        }	

        if (Input.GetButtonDown("ResetOrientation"))
        {
            // Reset tracker position. 
            // We assume that the CameraController is at the desired neck location
            CameraManager.singleton.SetYRotation(0f);
            OVRDevice.ResetOrientation();
        }

        if (fadeOutsideTrigger != null)
        {
            Vector3 delta = fadeOutsideStartPos - myTransform.position;
            float deltaMag = delta.magnitude;

            if (deltaMag > fadeOutsideDist)
            {
                SetPositionOrientation(fadeOutsideTrigger.GetRespawnPosition(), fadeOutsideTrigger.GetRespawnRotation(), true);
            }
            else
            {
                float alpha = MathfExt.Fit (deltaMag, 0f, fadeOutsideDist, 0f, 1f);
                OculusCameraFadeManager.SetColor(new Color(1f, 1f, 1f, alpha));
                desiredFadeAlpha = currentFadeAlpha = alpha;
            }
        }

        if( desiredFadeAlpha != currentFadeAlpha ) {
            float inc = 0.45f * Time.deltaTime;
            if( currentFadeAlpha < desiredFadeAlpha ) {
                currentFadeAlpha += inc;
                if( currentFadeAlpha > desiredFadeAlpha ) {
                    currentFadeAlpha = desiredFadeAlpha;
                }
            }
            if( currentFadeAlpha > desiredFadeAlpha ) {
                currentFadeAlpha -= inc;
                if( currentFadeAlpha < desiredFadeAlpha ) {
                    currentFadeAlpha = desiredFadeAlpha;
                }
            }
            OculusCameraFadeManager.SetColor(new Color(1f, 1f, 1f, currentFadeAlpha));
        }

		// do this last after area fade because it may trigger a new fade due to camera switch
        // don't do these ray casts b/c it's no longer used
//		OnInteract( );

        // do this after OnInteract because otherwise on button up we reset the timer
        if (TouchPadMove != TouchPadMoveMode.None &&
            IsMoveTouchActive())
        {
            touchDownTimer += Time.deltaTime;
        }
        else
        {
            touchDownTimer = 0f;
        }

		bUpdated = true;
	} 

	Vector3 GetDesiredSpeed()
	{
		if (disableMovement)
			return (Vector3.zero); //cheap way to disable movement without going through all the control logic...?

		Vector3 desiredSpeed = Vector3.zero;

/*        if (touchMoveState == TouchMoveState.Boost)
        {
            boostTimer -= Time.deltaTime;

            if (boostTimer < 0f)
            {
                SetTouchPadMoveState(TouchMoveState.None);
                boostCritter = null;
            }
            else
            {
                Vector3 myFwd = myTransform.forward;
                Vector3 eyePos = CameraManager.GetEyePosition();
                Vector3 targetDir = boostCritter.critterTransform.position - eyePos;
                float targetDist = targetDir.magnitude;

                if (targetDist > 0f)
                {
                    targetDir /= targetDist;
                }

                float angle = Mathf.Acos(Vector3.Dot(targetDir, myFwd));

                Debug.Log ("Boost dir: " + targetDir + " fwd: " + myFwd + "angle: " + (angle*Mathf.Rad2Deg));

                if (angle > BOOST_ANGLE)
                {
                    SetTouchPadMoveState(TouchMoveState.None);
                    boostCritter = null;
                }
                else
                {
                    RaycastHit hitInfo;
                    float testDist = BOOST_STOPDIST + BOOST_SPEED_MAX * BOOST_STOPTIME;
                    bool bHit = boostCritter.critterBoxCollider.Raycast(new Ray(eyePos, targetDir), out hitInfo, testDist );

                    if (bHit)
                    {
                        testDist = hitInfo.distance;
                    }
                    else if (CollisionHelpers.IsInside(boostCritter.critterBoxCollider, eyePos))
                    {
                        bHit = true;
                        testDist = 0f;
                    }

                    if (bHit && testDist <= BOOST_STOPDIST)
                    {
                        SetTouchPadMoveState(TouchMoveState.None);
                        boostCritter = null;
                    }
                    else
                    {
                        if (bHit || boostTimer <= BOOST_STOPTIME)
                        {
                            desiredSpeed = targetDir * MathfExt.Fit (boostTimer, BOOST_STOPTIME, 0f, BOOST_SPEED_MAX, 0f);
                        }
                        else
                        {
                            desiredSpeed = targetDir * BOOST_SPEED_MAX;
                        }
//                        desiredSpeed = targetDir * MathfExt.Fit (angle, 0f, BOOST_ANGLE, BOOST_SPEED_MAX, BOOST_SPEED_MIN);

                        desiredSpeed = myTransform.worldToLocalMatrix.MultiplyVector(desiredSpeed);
                    }
                }
            }
        }*/
		if( touchMoveState == TouchMoveState.Forward )
		{
			desiredSpeed.z = FPForwardSpeed;	// Xbox controller reads fwd/back reversed
		}
        else if( touchMoveState == TouchMoveState.Back )
        {
            desiredSpeed.z = -FPStrafeSpeed;    // Xbox controller reads fwd/back reversed
        }
        else if( touchMoveState == TouchMoveState.Up )
        {
            desiredSpeed.y = FPVerticalSpeed;    // Xbox controller reads fwd/back reversed
        }
        else if( touchMoveState == TouchMoveState.Down )
        {
            desiredSpeed.y = -FPVerticalSpeed;    // Xbox controller reads fwd/back reversed
        }
        else if( touchMoveState == TouchMoveState.FastForward )
        {
            desiredSpeed.z = FPForwardSpeed * 2f;    // Xbox controller reads fwd/back reversed
        }

        if (Mathf.Abs(Input.GetAxis("Left_Right")) > 0.4f)
		{
            desiredSpeed.x = Input.GetAxis("Left_Right") * FPStrafeSpeed;
		}
        else if (Input.GetKey("a"))
        {
            desiredSpeed.x = -FPStrafeSpeed; // Xbox controller reads fwd/back reversed
        }
        else if (Input.GetKey("d"))
        {
            desiredSpeed.x = FPStrafeSpeed; // Xbox controller reads fwd/back reversed
        }

        if (Mathf.Abs(Input.GetAxis("Forward_Back")) > 0.4f)
		{
            desiredSpeed.z = Input.GetAxis("Forward_Back") * -FPForwardSpeed;	// Xbox controller reads fwd/back reversed
        }
        else if (Input.GetKey("w"))
        {
            desiredSpeed.z = FPForwardSpeed; // Xbox controller reads fwd/back reversed
        }
        else if (Input.GetKey("s"))
        {
            desiredSpeed.z = -FPForwardSpeed; // Xbox controller reads fwd/back reversed
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        if (Mathf.Abs(Input.GetAxis("Up_Down_Android")) > 0.4f)
        {
            desiredSpeed.y = Input.GetAxis("Up_Down_Android") * -FPVerticalSpeed;    // Swap L and R triggers by negating speed
        }
#else
        if (Mathf.Abs(Input.GetAxis("Up_Down")) > 0.4f)
		{
            desiredSpeed.y = Input.GetAxis("Up_Down") * -FPVerticalSpeed; 	// Swap L and R triggers by negating speed
        }
#endif
        else if (Input.GetKey("z"))
        {
            desiredSpeed.y = -FPVerticalSpeed; 
        }
        else if (Input.GetKey("q"))
        {
            desiredSpeed.y = FPVerticalSpeed; 
        }

        desiredSpeed *= externalSpeedModifier;

		return desiredSpeed;
	}

#if UNITY_EDITOR
    void LateUpdate()
    {
        if( !CameraManager.IsInOculusMode() ) {
            return;
        }

        float xRot = 0.0f;
        float yRot = 0.0f;
        CameraManager.singleton.GetYRotation(ref yRot);
        
        if (Input.GetMouseButton(0))
        {
            xRot += Input.GetAxis("Mouse Y") * -3f; // y movement on mouse turns into x camera rotation
            yRot += Input.GetAxis("Mouse X") * 3f; // x movement on mouse turns into y camera rotation, needs to be inverted
        }

        CameraManager.singleton.SetYRotation(yRot);
        
        CameraManager.UpdateMainCameraRotation( Quaternion.Euler( new Vector3(xRot, yRot, 0f ) ) );
    }
#endif

	public override void FixedUpdateCameraMode()
	{
		//UpdateTutorial();

        if (CameraManager.singleton && !CameraManager.singleton.ovrPhysicsMove)
            return;

        Vector3 force = myTransform.rotation * (Time.fixedDeltaTime * lastDesiredSpeed.normalized);
        Rigidbody rb = CameraManager.singleton.ovrRB;
        float mx = lastDesiredSpeed.magnitude * externalSpeedModifier;

        float useAccel = physicsAccel;
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftShift))
        {
            useAccel *= 4f;
        }
#endif

        if (rb.velocity.sqrMagnitude < mx * mx)
        {
            rb.AddForce( useAccel * force * externalSpeedModifier );
        }
        else
        {
            if (lastDesiredSpeed.sqrMagnitude > 0.01f)
            {
                rb.velocity = 0.99f * mx * force.normalized;
            }
            else if (mx > 0.01f)
            {
                rb.velocity = 0.99f * mx * rb.velocity.normalized;
            }
            else
            {
                rb.velocity = 0.9f * rb.velocity;
            }
        }
    }

    WemoItemData PickItem()
    {
		Ray ray = new Ray(CameraManager.GetEyePosition(), myTransform.forward);

        WemoItemData itm = null;
        int fishMask = 1<<13|1<<12|1<<11|1<<9|1<<15|1<<14; 

        AgentInteractionManager.Singleton.AgentInteractionCastVR(ray, fishMask, out itm);

        return itm;
    }

    void OnInteract()
    {
        if( SimInstance.Instance.slowdownActive || SimInstance.Instance.IsSimPaused() ) {
            pickedItem = null;
        }
        else if ((pickItemFrame % 4) == 0)
        {
            pickedItem = PickItem();
        }

        bool bEduMode = App.UserManager == null || App.UserManager.educationalMode == 1;

        if (bEduMode)
        {
    		//if(lastPickedItem != pickedItem && pickedItem != null){
    		if(CameraManager.singleton.fxFrame.nextTarget != pickedItem && pickedItem != null)
            {
    			CameraManager.singleton.fxFrame.SwitchTarget(pickedItem);
    		}

    		if(pickedItem == null && !CameraManager.singleton.fxFrame.isFading)
            {
    			CameraManager.singleton.fxFrame.Clear();
    		}
        }

		if(pickedItem != lastPickedItem ){
			if(pickedItem != null){
				UITravelNode utn = pickedItem.GetComponentInChildren<UITravelNode>();
				if(utn != null){
					utn.SetHovering(true);	
				}
			}
			if(lastPickedItem != null){
				UITravelNode lastUtn = lastPickedItem.GetComponentInChildren<UITravelNode>();
				if(lastUtn != null){
					lastUtn.SetHovering(false);
				}
			}
		}

		lastPickedItem = pickedItem;
		
        ++pickItemFrame;

        if( SimInstance.Instance.slowdownActive || SimInstance.Instance.IsSimPaused() )
        {
            return;
        }

        WemoItemData clickOnMe = null;

        if (CameraManager.singleton.fxFrame != null)
        {
            clickOnMe = CameraManager.singleton.fxFrame.currentTarget;
        }

        // if we're hovering over the travel icon click that instead
        if (pickedItem != null && 
            (pickedItem.critterInfo == null || pickedItem.critterInfo.generalSpeciesData == null))
        {
            clickOnMe = pickedItem;
        }

        bool interactButtonIsDown = IsInteractButtonDown();
        if (clickOnMe == null)
        {
            if(interactButtonIsDown && bEduMode) // if we clicked nothing
            {
                AudioManager.Instance.PlayAudio((int)SoundFXID.PanelClick);
            }

            return;
		}
	

        if (bUpdated &&
		    interactButtonIsDown)
        {
            if (clickOnMe.TriggerInteract()) 
            {
				_hasInteracted = true;
			}

            //picked itme might be cleared if we clicked on a follow fish
/*            if (pickedItem != null &&
                pickedItem.critterInfo != null)
            {
                SetTouchPadMoveState(TouchMoveState.Boost);
                boostTimer = BOOST_TIME;
                boostCritter = pickedItem.critterInfo;
            }*/
		}
		else 
		{
			//c.Hovering();
			//pickedItem.SetOutlineMaterial(true);
			/*
			if(CameraManager.singleton.fxFrame.target == null || CameraManager.singleton.fxFrame.target != pickedItem.transform){
				CameraManager.singleton.fxFrame.target = pickedItem.transform;
				CameraManager.singleton.fxFrame.size = pickedItem.critterInfo.generalMotionData.critterBoxColliderSize;
				CameraManager.singleton.fxFrame.Refresh();
				CameraManager.singleton.fxFrame.Fade(1f);
				//CameraManager.singleton.fxFrame.targetCollider = pickedItem.critterInfo.critterCollider;
			}
			*/


            if (clickOnMe.Hovering()) 
            {
				/*
				if (!_hasInteracted && (!FloatingMenuManager.HasShownTutorial(TutorialManager.ScreenMode.Select)))
					FloatingMenuManager.ShowTutorial(TutorialManager.ScreenMode.Select);
					*/
			}

			if(fxConstraint != null)
            {
                fxConstraint.xform = clickOnMe.transform;
            }
        }
	}

    public void StartAreaFade(OculusAreaFadeTrigger trigger, Vector3 pos)
    {
        fadeOutsideTrigger = trigger;
        fadeOutsideStartPos = myTransform.position;

        if (mercyKillMessage != null &&
            !mercyKillMessage.activeSelf)
        {
            mercyKillMessage.transform.position = myTransform.position;
            mercyKillMessage.transform.rotation = Quaternion.LookRotation(new Vector3(myTransform.forward.x, 0f, myTransform.forward.z));
            mercyKillMessage.SetActive(true);
        }
    }

    public void StopAreaFade( bool force=false )
    {
        if (mercyKillMessage != null)
        {
            mercyKillMessage.SetActive(false);
        }

        fadeOutsideTrigger = null;
        desiredFadeAlpha = 0f;
        if( force ) {
            currentFadeAlpha = 0f;
            OculusCameraFadeManager.SetColor(new Color(0f, 0f, 0f, 0f));
        }
    }

	void UpdateTutorial() {

		/*
		if (FloatingMenuManager.tutorialOn)
			return;

		if ((Time.time - _lastTutorialAttempt) < _tutorialDelay)
			return;

		_lastTutorialAttempt = Time.time;

		//see if we need to show one
		if (!_hasMoved && (!FloatingMenuManager.HasShownTutorial(TutorialManager.ScreenMode.Move))) {
			if (FloatingMenuManager.ShowTutorial(TutorialManager.ScreenMode.Move))
				return;
		}
		*/
	}

	public static bool isMoving {

		get {return(_isMoving);}
	}

	public static bool hasInteracted {

		get {return(_hasInteracted);}
	}
}
