using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CameraCollisionType {
	None,
	Ground,
	GroundAndCritters
}

public enum CameraState {
	Intro,
	Normal,
	Reveal,
}

public enum IntroCameraState {
	FadedBlack,
	Stalling,
	InMotion,
	DivingDown,
	FadingToGame,
	SaverBlocked,
	MenuFalling,
	WaitingForLoad,
	None,
}

[System.Serializable]
public class CameraData {
	public GameObject cameraObject;
	public Transform cameraTransform;
    public Collider cameraCollider;
	public BaseCameraMode cameraBase;	
	public CameraType cameraType;
	
	public int cameraMasterIndex;
	public int modeSwitchesSinceLastUsed;
}

public class CameraManager : MonoBehaviour {
	
    #region OVR
    public Transform OVRCameraParent;
    [HideInInspector]
    public Collider ovrPlayerBody;
    public Camera leftCamera;
	public Camera rightCamera;
	[HideInInspector]
	protected OVRCameraController ovrController = null;
    [HideInInspector]
    public Rigidbody ovrRB = null;
    [HideInInspector]
	public CharacterController ovrCharCtrl = null;
    [HideInInspector]
    public bool ovrPhysicsMove;

    [HideInInspector]
    protected SteamVR_Camera svrCam = null;
    public bool HasVR { get { return ovrController != null || svrCam != null; } }
    Transform leftCameraTransform;
    Transform rightCameraTransform;
    #endregion

    public static CameraManager singleton;
	
	public float wantedAspectRatio = 1.77777778f; //16/9
	//private float setAspectRatio = 0f;
	
	public Camera mainCamera;

	public bool startupEditorOculusActive = true;
	public bool startupOculusActive = true;
	private bool oculusActive = false;

	private Transform mainCameraTransform;

    private Vector3 cameraForward;
    private bool cameraForwardSet = false;
    private Vector3 cameraRight;
    private bool cameraRightSet = false;
    private Vector3 cameraPosition;
    private bool cameraPositionSet = false;
    private Vector3 cameraFlattenedForward;
    private Quaternion cameraFlattenedRotation;
    private bool cameraFlattenedRotationSet = false;
    private Vector3 cameraEyePosition;
    private bool cameraEyePositionSet = false;

    public Vector3 prevCamPos = Vector3.zero;

	GameObject[] _preLoadCameraOrder;

	static GameObject ocean_intro;

	public GameObject[] cameraOrder
	{
		get
		{
			if (levelCameraData != null)
				return levelCameraData.cameraOrder;
			else {

				if (_preLoadCameraOrder == null)
					_preLoadCameraOrder = new GameObject[6]; //this is a hack so the loading scene can use the travel camera

				return _preLoadCameraOrder;

				//return null;
			}
		}
		set
		{
			if (levelCameraData != null)
				levelCameraData.cameraOrder = value;
		}
	}

	private int lastScreenWidth=0, lastScreenHeight=0;
//#if !UNITY_EDITOR
	private Rect forcedCameraRectInPixels = new Rect(0,0,0,0);
//#endif
	private Rect forcedCameraRect = new Rect(0,0,0,0);
    private CameraData[] cameras;
	private CameraData[] cameraDataInOrder;
	private CameraData currentCamera;
	private int numCamerasUnlocked;

	private int activeCameraIndex = 0;
    private int previousCameraIndex = -1;

	public static GameObject currentTarget = null;
//	private static SpeciesType currentTargetSpecies;
	
	public int minSwitchesForRepeatView = 4;
//	private int followCameraIndex;

	private static CameraState cameraState;
	
	private Vector3 introLocation = new Vector3(0f, -234.5f, 151.5f);
	private GameObject introGodRayObject;
	private Vector3 introRotationEuler = new Vector3( -10f, 90f, 0f );
//	private float introCurrentSphereDist;
//	private float introDesiredSphereDist = 1800.0f;
	private float introKeepBlackTime = 1.0f;
	private float introStallMotionTime = 2.0f;
	private float introMinMotionTime = 3.0f;
	private float introAccel = 1.5f;
	private float introMaxSpeed = 35.0f;
	private float introPitch = -10f;
	private float introDownPitch = 85f;
	private float introDownPitchAccel = 0.9f;
	private float introDownPitchDecel = 80f;
	private float introDownPitchSpeed = 0f;
	private float introDownTimeToFade = 1f;//4.25f;
	private float introFadeTime = 3.0f;
	private IntroCameraState introState;
	private float introTimer = 0f;
	private float introSpeed = 5f;
	private bool introEndTriggered;
	private bool introPitchDownDecelActive;
	
	private bool switchToCameraFadeActive;
	private GameObject switchToCameraFadeObject;
	private CameraType switchToCameraFadeType;
	
	[HideInInspector]
	public bool isOculusMode;
	
	public static event System.Action __diveInComplete = null;

	// cameras defined for the alpha 0-9 keys along the top of the keyboard.
	// these are not tied to the actual number of cameras defined in the scene.
	public GameObject[] inputSwitchCameras;

	public FrameHighlight fxFrame;
	protected GameObject defaultPlankton;
	public Transform dome;
    [HideInInspector]
    public pointConstraint domePointConstraint = null;
    public Light directionalLight;
	
//	public Camera interpCamera; // do we need this?

	// class log
//	private static Log log = Log.GetLog(typeof(CameraManager).FullName);

	public static CameraData GetCameraData( int idx ) {
		return singleton.cameras[idx];
	}

    public static CameraData GetCameraData( CameraType camType ) 
    {
        for (int i=0; i<singleton.cameras.Length; ++i)
        {
            if (singleton.cameras[i].cameraType == camType)
            {
                return singleton.cameras[i];
            }
        }

        return null;
    }

	public static int GetCameraDataCnt() {
		return singleton.cameras.Length;
	}
	
	public static CameraData GetCameraDataInOrder( int idx ) {
		return singleton.cameraDataInOrder[idx];
	}
	
	public static int GetCameraDataInOrderCnt() {
		return singleton.cameraDataInOrder.Length;
	}
	
	public static Vector3 GetFollowCamSphereCenter() {
		return singleton.levelCameraData.followCamSphereCenter;
	}
	
	public static float GetFollowCamSphereDistance() {
		return singleton.levelCameraData.followCamSphereDistance;
	}
	
	public static Transform GetOculusStartTransformExpensive() {
		if( singleton.levelCameraData != null && singleton.levelCameraData.oculusStart != null ) {
			return singleton.levelCameraData.oculusStart.transform;
		}

		return null;
	}
	
	public static CameraState CurrentState() {
		return cameraState;
	}
	
	void UpdateAspect(bool awake) {

        if (ChooseVRSDK.SteamActive)
        {
            return;
        }
		if( IsInOculusMode() ) {
			return;
		}

		if( lastScreenHeight == Screen.height && lastScreenWidth == Screen.width) {
			return;
		}
		
		if( !Screen.fullScreen && lastScreenHeight != 0 && Time.realtimeSinceStartup > 1f ) {
//			log.Trace("AUTO SIZE" + lastScreenHeight + " " +Screen.height + " " +lastScreenWidth + " " + Screen.width );
			// take the side with teh most change and enforce the aspect ratio on the other.
			float hd = Mathf.Abs( lastScreenHeight - Screen.height );
			float wd = Mathf.Abs( lastScreenWidth - Screen.width );
			if( hd > wd ) {
				lastScreenHeight = Screen.height;
				lastScreenWidth = (int)(Screen.height * wantedAspectRatio);
				Screen.SetResolution( lastScreenWidth, Screen.height, false );
			}
			else {
				lastScreenWidth = Screen.width;
				lastScreenHeight = (int)(Screen.width * ( 1 / wantedAspectRatio ) );
				Screen.SetResolution( Screen.width, lastScreenHeight, false );
			}
//			log.Trace("DONE" + lastScreenHeight + " " + lastScreenWidth + " ");
			return;
		}

		// error max..
        if (!IsInOculusMode())
        {
    		if( Screen.width > 1920 || Screen.height > 1080 ) 
            {
	    		Screen.SetResolution( 1920, 1080, false );			
		    	return;
		    }
        }
		
		// force Screen.width/height
//		log.Trace("Screen size change to " + Screen.width + "," + Screen.height);
		lastScreenHeight = Screen.height;
		lastScreenWidth = Screen.width;

//#if !UNITY_EDITOR			
		float currentAspectRatio = (float)Screen.width / Screen.height;
		//setAspectRatio = currentAspectRatio;
		
		// we have a special request for the camera rect
		if (forcedCameraRectInPixels.width > 0 && forcedCameraRectInPixels.height > 0) {
			float newLeft = forcedCameraRectInPixels.xMin / Screen.width;
			// y is flipped: geom is given from top left corner
			float newTop = (Screen.height - forcedCameraRectInPixels.yMin - forcedCameraRectInPixels.height) / Screen.height;
			float newWidth = forcedCameraRectInPixels.width / Screen.width;
			float newHeight = forcedCameraRectInPixels.height / Screen.height;
			forcedCameraRect = new Rect(newLeft, newTop, newWidth, newHeight);

//			log.Trace("Forced Camera: " + forcedCameraRect);
			mainCamera.rect = forcedCameraRect;
		}
		// If the current aspect ratio is already approximately equal to the desired aspect ratio, don't do anything
        else if ((int)(currentAspectRatio * 100) / 100.0f == (int)(wantedAspectRatio * 100) / 100.0f) {
			mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }
        // Pillarbox
        else if (currentAspectRatio > wantedAspectRatio) {
            float inset = 1f - wantedAspectRatio/currentAspectRatio;
			mainCamera.rect = new Rect(inset/2f, 0f, 1f-inset, 1f);
        }
        // Letterbox
        else {
            float inset = 1f - currentAspectRatio/wantedAspectRatio;
			mainCamera.rect = new Rect(0f, inset/2f, 1f, 1f-inset);
        }

		// FullScreen UI TODO: Reinit any non-anchored UI items now.

//#endif
	}

	void Awake() {
		cameras = null;
		cameraDataInOrder = null;

		singleton = this;
		activeCameraIndex = -1;

//		log.Detach();
//		log.threshold.console = Log.LogLevel.INFO;

		if( mainCamera != null ) {
			mainCameraTransform = mainCamera.transform;
		}

		SetupSteamVR();

		leftCameraTransform = leftCamera.transform;
		if (rightCamera != null)
	        rightCameraTransform = rightCamera.transform;

		UpdateAspect( true );

	}
    public void SetupSteamVR()
    {       
        svrCam = OVRCameraParent.GetComponentInChildren<SteamVR_Camera>();

        // update to new steamVR instantiated cameras
        if (svrCam != null)
        {
            Camera cam = svrCam.GetComponent<Camera>();
            InitSteamVRCamera(cam, leftCamera);
            leftCamera = cam;
			if (rightCamera != null)
			{
				DestroyImmediate(rightCamera);
				rightCamera = null;
			}
        }
    }

    public void BootLoad()
    {
		if( FloatingMenuManager.Instance != null && FloatingMenuManager.Instance.introOcean != null) {
			ocean_intro = FloatingMenuManager.Instance.introOcean;
		} else {
			ocean_intro = (GameObject)GameObject.Find("ocean_intro");
		}
        AttachPlankton();
    }

	public void FinishBootLoad()
	{
		ocean_intro.SetActive (true);
		dome.gameObject.SetActive (true);
	}

	void Start () {
	   if( dome != null ) {
            domePointConstraint = dome.GetComponent<pointConstraint>();
        }
        ovrController = GetComponentInChildren<OVRCameraController>();
        ovrRB = GetComponentInChildren<Rigidbody>();
        ovrCharCtrl = GetComponentInChildren<CharacterController>();
        if( OVRCameraParent != null ) 
        {
            ovrPlayerBody = OVRCameraParent.GetComponent<Collider>();
    	
            //the intro camera may leave the parent with gravity on
            if (ovrRB != null)
            {
                ovrRB.useGravity = false;
            }
        }

        ovrPhysicsMove = false;

        AttachPlankton();
        // rigid body is expected on a parent object in order to use physics movement.
        if (ovrRB != null &&
            ovrRB.gameObject == ovrController.gameObject)
        {
            ovrRB = null;
        }

		InitAudioMgr();

        if( AppBase.Instance != null &&
            AppBase.Instance.RunningAsPreview() )
        {
            PostLoadSetup(false);
            if( IsInOculusMode() ) {
    			SwitchToCamera(CameraType.OculusCamera);
            }
            cameraState = CameraState.Normal;
		}
        else
        {
            InitIntroCamera();
        }

        BootLoad ();

    }

	public void InitAudioMgr()
	{
		if (ovrController != null)
		{
			AudioManager.InitAudioSourceTransform(ovrController.transform);
		}
	}

	public bool IsCamera(Transform t)
	{
		if (ovrController != null)
		{
			return t == ovrController.transform;
		}

		return false;
	}

	public float farClipPlane {
		
		get {
			if( IsInOculusMode() ) {
				return leftCamera.farClipPlane;
			}
			else {
				return mainCamera.farClipPlane;
			}
		}
		
		set{
			Debug.Log("Setting far place to" + value);
			mainCamera.farClipPlane = value;
			leftCamera.farClipPlane = value;
            if (rightCamera != null)
				rightCamera.farClipPlane = value;

            if (ovrController != null)
            {
                ovrController.FarClipPlane = value;
		}
			if (svrCam != null)
            {
                leftCamera.GetComponent<Camera>().farClipPlane = value;
                if (rightCamera != null)
                    rightCamera.GetComponent<Camera>().farClipPlane = value;
	}
		}
	}
	
	public float orthographicSize {
		get {
			if( IsInOculusMode() ) {
				return leftCamera.orthographicSize;
			}
			else {
				return mainCamera.orthographicSize;
			}
		}
		
		set{
			// we do not set the orthographic size of the oculus camera.
			mainCamera.orthographicSize = value;
		}
		
	}
	
	
	public static void SetActive (bool active)
	{
		if( IsInOculusMode() ) {
			singleton.leftCamera.gameObject.SetActive(active);
            if (singleton.rightCamera != null)
				singleton.rightCamera.gameObject.SetActive(active);
		}
		else {
			singleton.mainCamera.gameObject.SetActive(active);
		}
	}

	LevelCameraData _levelCameraData;
	LevelCameraData levelCameraData
	{
		get
		{
			if (_levelCameraData == null)
				_levelCameraData = (LevelCameraData) GameObject.FindObjectOfType(typeof(LevelCameraData));

			return _levelCameraData;
		}
	}

	public void PostLoadSetup( bool force_load ) {

		//Debug.Log("POST LOAD SETUP! " + cameraState.ToString() + " : " + introState.ToString());

		//this is a total hack, but if we have a current camera, save it's type and then switch back to it
		//CameraData oldCurrent = currentCamera;
		//int newIndex = -1;

		_levelCameraData = null;
		
		// we cannot load the cameras if we are travelling with a critter. the travel camera will force this when it is ready to end.
		if( !force_load 
		   && IsInTravelCamera()
		   ) {
			return;
		}

		if( cameras != null ) {
			for( int i = 0; i < cameras.Length; i++ ) {
				CameraData cam = cameras[i];
				cam.cameraBase = null;
				cam.cameraObject = null;
                cam.cameraTransform = null;
                cam.cameraCollider = null;
            }
		}
		
		if( cameraDataInOrder != null ) {
			for( int i = 0; i < cameraDataInOrder.Length; i++ ) {
                CameraData cam = cameraDataInOrder[i];

                if (cam == null)
                {
                    continue;
                }

				cam.cameraBase = null;
				cam.cameraObject = null;
				cam.cameraTransform = null;
                cam.cameraCollider = null;
            }
		}
		
	   	BaseCameraMode[] modes = GameObject.FindObjectsOfType(typeof(BaseCameraMode)) as BaseCameraMode[];
		
		int pcam = 0;
		int count = 0;
		cameras = new CameraData[modes.Length];
		
		cameraDataInOrder = new CameraData[cameraOrder.Length];
        for( int i = 0; i < modes.Length; i++ ) {
            BaseCameraMode mode = modes[i];

			if( mode == null ) {
				continue;
			}
             	
            if( !mode.CameraModeIsLoadable() ) {
                continue;
            }
            
			CameraData cd = new CameraData();
			cd.cameraBase = mode;
			cd.cameraObject = mode.gameObject;
            cd.cameraTransform = mode.gameObject.transform;
            cd.cameraCollider = mode.gameObject.GetComponent<Collider>();
            cd.cameraMasterIndex = count;
			mode.masterIndex = count;
			mode.cameraOrderIndex = -1;
			for( int j = 0; j < cameraOrder.Length; j++ ) {
				if( cameraOrder[j] == mode.gameObject ) {
					mode.cameraOrderIndex = j;
					cameraDataInOrder[j] = cd;
					j = cameraOrder.Length;
				}
			}
			
			mode.InitCameraMode();
			cd.cameraType = mode.cameraType;

			if (cd.cameraType == CameraType.OculusCamera)
			{				
				if (mode.primaryCamera)
				{
					isOculusMode = true;
					
					// hacky and should be changed
					//cameras[count].cameraTransform = modes[0].transform;
//					_subInternalSwitchToCamera(0, true);
					
				}
				else
					isOculusMode = false;
			}
			
			cd.modeSwitchesSinceLastUsed = minSwitchesForRepeatView; // force all cameras available at load.
			cameras[count] = cd;
			if( mode.primaryCamera )
            {
				pcam = count;
			}

			/*
			//keep track of the mode we were in when the loading happened
			if (oldCurrent != null) {
				if (oldCurrent.cameraType == cd.cameraType)
					newIndex = count;
			}
			*/

			count++;
		}
		
		numCamerasUnlocked = 0;
		for( int i = 0; i < cameraDataInOrder.Length; i++ ) {
			CameraData cd = cameraDataInOrder[i];
			if( cd != null && cd.cameraBase != null ) {
				cd.cameraBase.cameraName += " " + i;
				if( cd.cameraBase.unlocked ) {
					numCamerasUnlocked++;
				}
			}
		}

        if (activeCameraIndex >= 0 &&
            cameras[activeCameraIndex].cameraType == CameraType.OculusTourCamera)
        {
            pcam = activeCameraIndex;
        }

		activeCameraIndex = -1; // force our active camera to get updated.
    	currentCamera = null;
        _subInternalSwitchToCamera(pcam, false);

        if (ovrController != null && !ChooseVRSDK.SteamActive)
        {
            float oculusScale = 5f;
            if (levelCameraData != null)
            {
                oculusScale = levelCameraData.oculusScale;
            }

            OculusScaler scaler = CameraManager.singleton.GetComponentInChildren<OculusScaler>();
            if (scaler != null)
            {
                scaler.Scale = oculusScale;
                scaler.ApplyScale();
            }
            else
            {
                ovrController.SetPlayerScale(oculusScale);
            }
        }

		AttachPlankton();

        if( !AppBase.Instance.RunningAsPreview() ) {
            OculusTourCameraMode.Init();
        }
        TourCompleteMenuManager.StartScene();

        AudioManager.InitAudioSourceTransform(ovrController.transform);
    }
	
    public void PostSphereLoadSetup()
    {
        SphereInstance.PostSphereLoadSetup( false );        
    }

    public void AttachPlankton()
    {
		// ee edit for valve  lets remove this and attach it in scene
		return;
		// should only happen once, first time in vr_boot scene, cache global effect since it can be deactivated 
		// if scene plankton is available and GameObject.Find() will not succeed.
		if (defaultPlankton == null)
		{
			defaultPlankton = GameObject.Find("/GlobalEffects/Plankton");
			if (defaultPlankton == null)
				defaultPlankton = GameObject.Find("/GlobalEffects/Plankton_dark");
			if (defaultPlankton == null)
				defaultPlankton = GameObject.Find("/GlobalEffects/PlanktonMoving");
			if (defaultPlankton == null)
				defaultPlankton = GameObject.Find("/GlobalEffects/planktonOpaqueLight");   
			if (defaultPlankton == null)
				defaultPlankton = GameObject.Find("/GlobalEffects/planktonOpaqueMoving");
		}

		GameObject scenePlankton = null;
		GameObject levelEffects = GameObject.Find ("LevelStreamingData/LevelEffects");
		// find child ScenePlankton object if it exists and use it as override
		if (levelEffects != null)
		{
			ScenePlankton sp = levelEffects.GetComponentInChildren<ScenePlankton>();
			if (sp != null)
			{
				scenePlankton = sp.gameObject;
			}
		}

		// if scene plankton exists, enable it and disable the global plankton
		if (scenePlankton != null) 
		{
			if (defaultPlankton != null)
			{
				defaultPlankton.SetActive(false);
			}
			internalAttach(scenePlankton);
		}
		else
		{
			if( defaultPlankton != null )
			{
				internalAttach(defaultPlankton);
			}
		}
	}

    public void RenderPlankton(bool on)
    {
        if (defaultPlankton == null)
        {
            return;
        }

        defaultPlankton.GetComponent<Renderer>().enabled = on;
    }

	void internalAttach(GameObject obj)
	{
		Transform xform = mainCameraTransform;
		if (isOculusMode)
		{
			// ee edit for valva. lets attach it to valve camera
			//xform = OVRCameraParent;
		}

		obj.SetActive(true);
		pointConstraint [] pts = obj.GetComponentsInChildren<pointConstraint> ();
		if( pts != null ) 
		{
			for (int i = 0; i < pts.Length; ++i)
			{
				pts[i].xform = xform;
			}
		}

		ParticleKillRange [] killers = obj.GetComponentsInChildren<ParticleKillRange> ();
		if (killers != null)
		{
			for (int i=0; i < killers.Length; ++i)
			{
				killers[i].xform = xform;
			}
		}
	}
	
	public void AttachDefaultPlankton()
	{
		if (defaultPlankton == null)
		{
			return;
		}

		internalAttach(defaultPlankton);
	}
	
	public static void LevelCamerasRemoved() {
		if( singleton != null 
		   && singleton.currentCamera != null 
		   && singleton.currentCamera.cameraType != CameraType.TravelCamera ) {
			SwitchToCamera( CameraType.StaticCamera );
		}
	}
	
    public void SetVRActive()
    {
        oculusActive = true;
    }

	public static bool IsInOculusMode() {
		if ( !singleton )
			return false;
		else 
			return ( singleton.oculusActive );
	}
		
	public static bool IsCritterWithinViewableDistance( CritterInfo critter ) {
        Vector3 dir = critter.cachedPosition - GetFollowCamSphereCenter();
		float dist = GlobalOceanShaderAdjust.CurrentDistance() + GetFollowCamSphereDistance();
		if( dir.sqrMagnitude < dist * dist ) {
			return true;
		}
		return false;
	}
	
	public static bool IsInMigrationCamMode()
	{
		return ( singleton.currentCamera != null && singleton.currentCamera.cameraType == CameraType.MigrationCamera );
	}
	public static bool IsInTravelCamera() {
		if(singleton == null)
			return false;
		return ( singleton.currentCamera != null && singleton.currentCamera.cameraType == CameraType.TravelCamera );
	}
	
	public static void StaticPostLoadSetup( bool force_load ) {
		singleton.PostLoadSetup( force_load );
		
	}
    	
    public static void SetCameraExternalSpeedModifier( float speed_mod ) {
        if( singleton != null && singleton.currentCamera != null ) {
            singleton.currentCamera.cameraBase.externalSpeedModifier = speed_mod;
        }
    }

    public static GameObject GetCurrentTarget() {
		return currentTarget;
	}
	
    public static CameraType GetPreviousCameraType()
    {
        if (singleton.previousCameraIndex >= 0 &&
            singleton.previousCameraIndex < singleton.cameras.Length) 
        {
            return singleton.cameras[singleton.previousCameraIndex].cameraBase.cameraType;
        }

        return CameraType.None;
    }

	public static Camera GetCurrentCamera() {
#if UNITY_EDITOR
        if (singleton == null)
            return null;
#endif
        if( singleton.oculusActive ) {
			return singleton.leftCamera;
		}

		return singleton.mainCamera;
	}
	
	public static int GetCurrentCameraOrder() {
		if( singleton != null && singleton.currentCamera != null ) {
			return singleton.currentCamera.cameraBase.GetCameraOrderIndex();
		}
		return -1;
	}
		
	public Rect rect {
		
		get {
			if( IsInOculusMode() ) {
				return leftCamera.rect;
			}

			return mainCamera.rect;
		}
		
		set {
			// We do not set the oculus camera rects.
			mainCamera.rect = value;
		}
	}

	// This value is used for special layout logic in GUIManagers, etc
	public static int ScreenVOffset
	{
		get
		{
			if( !IsInOculusMode() ) {
				int offsetY = (int)(Screen.height * (1.0 - singleton.mainCamera.rect.height - singleton.mainCamera.rect.yMin));

				//Fix weird 1-2 pixel height difference seen in some displays
				if (offsetY > 3)
					return offsetY;
			}

			return 0;
		}
	}

	public static int ScreenHeight
	{
		get
		{
			// return (int)(Screen.height);
			return (int)(Screen.height * singleton.rect.height);
		}
	}

	// keep in mind, ScreenTop >= ScreenBottom
	public static int ScreenTop
	{
		get
		{
			int screenTop = (int)(Screen.height * (singleton.rect.yMin + singleton.rect.height));
			// log.Trace("Screen Top is " +  screenTop);
			return screenTop;
		}
	}
	public static int ScreenBottom
	{
		get
		{
			return (int)(Screen.height * singleton.rect.yMin);

		}
	}
	public static int ScreenLeft
	{
		get
		{

			return (int)(Screen.width * singleton.rect.xMin);
		}
	}
	public static int ScreenWidth
	{
		get
		{
			// return (int)(Screen.width);
			return (int)(Screen.width * singleton.rect.width);
		}
	}
    
    public static Vector3 MousePosition {
        get {
            Vector3 mousePos = Input.mousePosition;
			Rect crect = singleton.rect;
			mousePos.y -= (int)(crect.y * Screen.height);
			mousePos.x -= (int)(crect.x * Screen.width);
        	// log.Trace("Mapped mouse " + Input.mousePosition + " to " + mousePos);
            return mousePos;
        }
    }
	
	public static void ToggleFullScreen(bool active) {
		Screen.fullScreen = active;
	}
	
	public static int GetNumberCamerasUnlocked() {
		return singleton.numCamerasUnlocked;
	}
	
	void SwitchToCameraFadeInFinished( object arg ) {
		switchToCameraFadeActive = false;
	}
	
	void SwitchToCameraFadeOutFinished( object arg ) {
		SwitchToTarget(switchToCameraFadeObject,switchToCameraFadeType);
		OculusCameraFadeManager.StartCameraFadeFromBlack(1.5f, SwitchToCameraFadeInFinished,null);		
	}

	public static void SwitchToTargetAfterCameraFade( GameObject new_target, CameraType type ) {
		if( !singleton.switchToCameraFadeActive ) {
			singleton.switchToCameraFadeActive = true;
			singleton.switchToCameraFadeObject = new_target;
			singleton.switchToCameraFadeType = type;
            OculusCameraFadeManager.StartCameraFadeToBlack(1.5f, singleton.SwitchToCameraFadeOutFinished,null);			
		}
	}

	public static void SwitchToSeabedTarget( GameObject new_target ) {
		if(SimManager.IsCameraTargetSwitchingBlocked() ) {
			return;
		}
		currentTarget = new_target;
		for( int i = 0; i < singleton.cameras.Length; i++ ) {
			CameraData cam = singleton.cameras[i];
			if( cam.cameraType == CameraType.DynamicDriftCamera ) {
				singleton._internalSwitchToCamera(i);
    		}
		}
		SimManager.SetCameraCritterIndex(-1);
	}
	
	public static void SwitchToTarget( GameObject new_target, CameraType type ) {
		//DebugDisplay.AddDebugText("CAMERA MANAGER = SWITCH TO TARGET");
		if(SimManager.IsCameraTargetSwitchingBlocked() ) {
			DebugDisplay.AddDebugText("CAMERA MANAGER - SWITCH TO TARGET - CAMERA TARGET SWITCHING BLOCKED");
			return;
		}
		// TODO>refactor part 2
		CritterInfo critter = SimInstance.Instance.GetCritterInfoForObject(new_target); // TODO>OPTIMIZE ME
		if( critter == null ) {
			DebugDisplay.AddDebugText("CAMERA MANAGER - SWITCH TO TARGET - GET CRITTER INFO FOR OBJECT IS NULL");
			return;
		}
		SimManager.SetCameraCritterIndex(critter.masterIndex);
		currentTarget = new_target;
		
		// TODO> Replace this with appropriate code.
		if( AppBase.Instance.RunningAsPreview() ) {
			if(FindObjectOfType(typeof(PreviewSceneMakerDebugCameraMode)))//maybe not the best way to do it.
				type = CameraType.PreviewSceneMakerDebugCamera;
			if(FindObjectOfType(typeof(RotateAroundObjectCamera)))
				type = CameraType.RotateAroundObjectCamera;
		}
		// end TODO>
		
//		string followVariantName;
//		followVariantName = App.FishManager.GetFishNameById( null, critter.critterItemData.variantID);
		//GUIManager.UpdateFollowedItemVariantName(followVariantName);
		
		for( int i = 0; i < singleton.cameras.Length; i++ ) {
			CameraData cam = singleton.cameras[i];
			if( cam.cameraType == type ) {
				singleton._internalSwitchToCamera(i);
    		}
		}
		
	}
	
	public static void AutoSwitchToFollowItem( int item_id ) {
		if(SimManager.IsCameraTargetSwitchingBlocked() ) {
			return;
		}
		GameObject new_obj = SimInstance.Instance.GetRandomCritterObjectWithItemId( item_id );
		if( new_obj != null ) {
			SwitchToTarget( new_obj, CameraType.FollowBehindCamera );
		}
	}
		
	public static void AutoSwitchToFollowVariant( int variant_id ) {	
		if(SimManager.IsCameraTargetSwitchingBlocked() ) {
			return;
		}
// BLU_REBOOT TODO::When this functionality is needed again, fix travel code for sphere
/*		GameObject new_obj = SimInstance.Instance.GetRandomCritterObjectWithVariantId( variant_id, false );
		if( new_obj != null ) {
			SwitchToTarget( new_obj, CameraType.FollowBehindCamera );
		}
		else {
			int sphere_id = App.FishManager.GetLegacySphereIdByFishId( variant_id );
			int owned_id = App.OwnedFishManager.GetLegacyOwnedIdFromFishId( variant_id );
			if( sphere_id == App.SphereManager.currentSphere.legacyid ) {
				WaldoManager.ForceAddAWaldo(variant_id, owned_id, true);
			}
			else {
                App.SphereManager.postLoadSpawnVariantID = variant_id;
                if( owned_id > 0 ) { 
                    App.SphereManager.spawnOwnedIDs.Add(owned_id);
                }
				App.SphereManager.postLoadSpawnFade = true;
				if( App.UserManager.LEGACY_ownedSphereIds.Contains( sphere_id ) ) {
					GUIStateManager.TravelToSphere(sphere_id);					
				}
				else {
					GUIStateManager.TravelToSphere( SphereManager.GALLERY_LEGACY_ID );
				}
			}			
		}*/
	}

	public static void AutoSwitchToFollowOwnedVariant( int owned_id, bool onlyIfInScene = false ) {	
		CritterInfo new_critter = SimInstance.Instance.GetOwnedCritterWithOwnedID(owned_id);
		
		if( new_critter != null 
            && new_critter.critterObject != null ) {
			SwitchToTarget( new_critter.critterObject, CameraType.FollowBehindCamera );
		}
        else {
            // if it's not in the scene and they've said they only want to follow if it's here,
            // then let's bail before we start travelling
            if ( onlyIfInScene )
            {
                return;
            }
            
			// BLU_REBOOT TODO::Fix once this functionality is needed again (following a specific owned critter.)			
			/*  DataUserItem owned_data = App.OwnedFishManager.GetOwnedItemByLegacyId( owned_id );
			if( owned_data == null ) {
				return;
			}
            int variant_id = App.OwnedFishManager.GetLegacyFishIdFromOwnedId( owned_id );
            int sphere_id = App.FishManager.GetLegacySphereIdByFishId( variant_id );
            
            App.SphereManager.postLoadSpawnVariantID = variant_id;
            App.SphereManager.postLoadSpawnFade = true;
            
            App.SphereManager.spawnOwnedIDs.Add(owned_id);
            
            // if shared,then go to the gallery!
            if( owned_data.Shared ) {
                GUIStateManager.TravelToSphere( SphereManager.GALLERY_LEGACY_ID );
            }
            else if( sphere_id == App.SphereManager.LEGACY_GetCurrentSphere() ) {
                DynamicCritterManager.QueueUpCritter(variant_id, owned_id, 1, null, true);
            }
            else {
                if( App.UserManager.LEGACY_ownedSphereIds.Contains( sphere_id ) ) {
                    GUIStateManager.TravelToSphere(sphere_id);                  
                }
                else {
                    GUIStateManager.TravelToSphere( SphereManager.GALLERY_LEGACY_ID );
                }
            }*/
        }
	}

    // assumes only a single camera of a given type.
    public static void ForceSwitchToCamera( CameraType type ) 
    {
        singleton.activeCameraIndex = -1;
        SwitchToCamera(type);
    }

	// assumes only a single camera of a given type.
	public static void SwitchToCamera( CameraType type ) {

		if( singleton.cameras == null ) {
			return;
		}
		
		for( int i = 0; i < singleton.cameras.Length; i++ ) {
			CameraData cam = singleton.cameras[i];
			if( cam != null && cam.cameraType == type ) {
				singleton._internalSwitchToCamera(i);
			}
		}
	}
	
	public static CameraType GetActiveCameraType() {
		if( singleton != null && singleton.currentCamera != null ) {
			return singleton.currentCamera.cameraType;
		}
		return CameraType.DriftCamera;
	}

    public static BaseCameraMode GetActiveCameraMode() {
        if( singleton != null && singleton.currentCamera != null ) {
            return singleton.currentCamera.cameraBase;
        }

        return null;
    }

	public static bool CurrentCameraFollowsTargets() {
		if( singleton != null && singleton.currentCamera != null ) {
			return singleton.currentCamera.cameraBase.GetFollowsTargets();
		}
		return false;
	}
	
	public static void UpdateCurrentCameraTransform() {
        if( singleton.oculusActive && !singleton.ovrPhysicsMove )
        {
            singleton.OVRCameraParent.position = singleton.currentCamera.cameraTransform.position;
        }

		singleton.mainCameraTransform.position = singleton.currentCamera.cameraTransform.position;
		singleton.mainCameraTransform.rotation = singleton.currentCamera.cameraTransform.rotation;
	}
    
    public static void UpdateMainCameraPosition( Vector3 new_pos ) {
        singleton.mainCameraTransform.position = new_pos;
    }
    
    public static void UpdateMainCameraRotation( Quaternion new_rot ) {
        singleton.mainCameraTransform.rotation = new_rot;
    }

    public static void UpdateCameraPositionFromParent() {
        if( singleton.oculusActive )
        {
            singleton.currentCamera.cameraTransform.position = singleton.OVRCameraParent.position;
        }
    }

	public static Vector3 GetCurrentCameraPosition() {
        if( !singleton.cameraPositionSet ) {
            if( singleton.oculusActive ) {
                if (singleton.svrCam != null)
                    singleton.cameraPosition = singleton.svrCam.transform.position;
                else
                    singleton.cameraPosition = singleton.OVRCameraParent.position;
            }
            else {
                singleton.cameraPosition = singleton.mainCameraTransform.position;
            }

            singleton.cameraPositionSet = true;
        }

        return singleton.cameraPosition;
	}

    public static Vector3 GetEyePosition( bool fresh = false )
    {
        if( fresh ) {
            singleton.cameraEyePositionSet = false;
        }
        if (!singleton.cameraEyePositionSet)
        {
            if (singleton.oculusActive)
            {
				if (singleton.svrCam != null)
					return singleton.svrCam.transform.position;
				else
					return (singleton.leftCameraTransform.position + singleton.rightCameraTransform.position) * 0.5f;
			}
			else
			{
				singleton.cameraEyePosition = singleton.mainCameraTransform.position;
            }
        }

        return singleton.cameraEyePosition;
    }	
	
	public static Vector3 GetCurrentCameraForward( bool fresh = false ) {
        if( fresh ) {
            singleton.cameraForwardSet = false;
        }
        if( !singleton.cameraForwardSet ) {
            if( singleton.oculusActive ) {
				if (singleton.svrCam != null)
					singleton.cameraForward = singleton.svrCam.transform.forward;
				else
	                singleton.cameraForward = singleton.OVRCameraParent.forward;
            }
            else {
                singleton.cameraForward = singleton.mainCameraTransform.forward;
            }

            singleton.cameraForwardSet = true;
        }

        return singleton.cameraForward;
	}	

    public static Vector3 GetCurrentCameraRight() {
        if( !singleton.cameraRightSet ) {
            if( singleton.oculusActive ) {
				if (singleton.svrCam != null)
					singleton.cameraRight = singleton.svrCam.transform.right;
				else
	                singleton.cameraRight = singleton.OVRCameraParent.right;
            }
            else {
                singleton.cameraRight = singleton.mainCameraTransform.right;
            }
            
            singleton.cameraRightSet = true;
        }
        
        return singleton.cameraRight;
    }   

    public static Vector3 GetCurrentCameraFlattenedForward() {
        if( !singleton.cameraFlattenedRotationSet ) {
            SetCurrentCameraFlattenedRotation();
        }

        return singleton.cameraFlattenedForward;
    }

	public static Quaternion GetCurrentCameraFlattenedRotation() {
		if( !singleton.cameraFlattenedRotationSet ) {
            SetCurrentCameraFlattenedRotation();
        }

		return singleton.cameraFlattenedRotation;

	}	
	
    private static void SetCurrentCameraFlattenedRotation() {
        Vector3 flat_forward = singleton.oculusActive ? singleton.OVRCameraParent.forward : singleton.mainCameraTransform.forward;
        flat_forward.y = 0f;
        flat_forward.Normalize();
        
        // singularity
        if( MathfExt.Approx( flat_forward, Vector3.zero, 0.1f ) )
        {
            flat_forward = singleton.oculusActive ? singleton.OVRCameraParent.up : singleton.mainCameraTransform.up;
            flat_forward.y = 0f;
            flat_forward.Normalize();
        }

        singleton.cameraFlattenedForward = flat_forward;
        singleton.cameraFlattenedRotation = Quaternion.LookRotation( flat_forward );
        singleton.cameraFlattenedRotationSet = true;
    }   

    public static Quaternion GetCurrentCameraRotation() {
        if( singleton.oculusActive ) {
			if (singleton.svrCam != null)
				return singleton.svrCam.transform.rotation;
			else
				return singleton.OVRCameraParent.rotation;
		}
		return singleton.mainCameraTransform.rotation;
	}	
	
	public static Transform GetCurrentCameraTransform() {
		if( singleton == null || singleton.currentCamera == null ) {
			return null;
		}

		return singleton.currentCamera.cameraTransform;
	}
	
	public static Vector3 GetScreenPixelsFromViewport( Vector3 viewport_pos ) {
		if( IsInOculusMode() ) {
			return singleton.leftCamera.ViewportToScreenPoint( viewport_pos );
		}
		return singleton.mainCamera.ViewportToScreenPoint( viewport_pos );
	}
	
	public static Vector3 GetViewportFromScreenPixel( Vector3 screen_pixels ) {
		if( IsInOculusMode() ) {
			return singleton.leftCamera.ScreenToViewportPoint( screen_pixels );
		}
		return singleton.mainCamera.ScreenToViewportPoint( screen_pixels );
	}
	
	public static Vector3 GetScreenLocationFromWorldPosition( Vector3 world_pos ) {
		if( IsInOculusMode() ) {
			return singleton.leftCamera.WorldToViewportPoint( world_pos );
		}
		return singleton.mainCamera.WorldToViewportPoint( world_pos );
	}
	
    public void SetBackgroundColor(Color c)
    {
        if (svrCam != null)
        {
            leftCamera.backgroundColor = c;
            if (rightCamera != null)
                rightCamera.backgroundColor = c;
        }
        if (ovrController != null)
        {
            ovrController.BackgroundColor = c;
        }
    }

    public void InitSteamVRCamera(Camera steamCam, Camera oldCam)
    {
        // update to new steamVR instantiated cameras if appropriate
        if (svrCam != null && svrCam.enabled)
        {
            steamCam.farClipPlane = oldCam.farClipPlane;
            steamCam.nearClipPlane = oldCam.nearClipPlane;
            steamCam.backgroundColor = oldCam.backgroundColor;
            steamCam.cullingMask = oldCam.cullingMask;

            // be careful with properties that are copied over; steam rendering works a bit differently
            // don't reparent things here, as steam camera could have a scale on it

            DestroyImmediate(oldCam.gameObject);
        }
    }

	public static void MoveForwardOnPath() {
		if ( singleton.currentCamera.cameraType == CameraType.PathCamera ) {
			((PathCameraMode)singleton.currentCamera.cameraBase).MoveForwardOnPath();
		}
	}

	public static void MoveBackwardOnPath() {
		if ( singleton.currentCamera.cameraType == CameraType.PathCamera ) {
			((PathCameraMode)singleton.currentCamera.cameraBase).MoveBackwardOnPath();
		}
	}

    public static void JumpToCameraMasterIndex( int index ) {
        cameraState = CameraState.Normal;
        singleton._internalSwitchToCamera( index );
    }
 
	public static void JumpToCameraOrder( int index ) {
        if( singleton.cameraDataInOrder == null || index < 0 || index >= singleton.cameraDataInOrder.Length ) {			
			return;
		}
		
		int master_idx = singleton.cameraDataInOrder[index].cameraMasterIndex;
		singleton._internalSwitchToCamera(master_idx);
	}

	
	//Camera Mode - View next camera in list
	public static void ChangeCameraBackward() {
		int new_cam_idx = singleton.cameras[singleton.activeCameraIndex].cameraBase.GetCameraOrderIndex() - 1;
		
		if( new_cam_idx < 0 ) {
			singleton._internalSwitchToCamera(singleton.cameraDataInOrder[singleton.numCamerasUnlocked-1].cameraMasterIndex);
		}
		else {
			singleton._internalSwitchToCamera(singleton.cameraDataInOrder[new_cam_idx].cameraMasterIndex);	
		}
		
	}
	
	//Camera Mode - View next camera in list
	public static void ChangeCameraForward() {
		int new_cam_idx = singleton.cameras[singleton.activeCameraIndex].cameraBase.GetCameraOrderIndex() + 1;
		
		if( new_cam_idx >= singleton.numCamerasUnlocked ) {
			singleton._internalSwitchToCamera(singleton.cameraDataInOrder[0].cameraMasterIndex);
		}
		else {
			singleton._internalSwitchToCamera(singleton.cameraDataInOrder[new_cam_idx].cameraMasterIndex);	
		}
	}

	public void RestartCurrentCamera() {
		if( currentCamera != null 
		   && currentCamera.cameraBase != null ) {
			currentCamera.cameraBase.StartCameraMode();
		}
	}
	private void _subInternalSwitchToCamera(int idx, bool do_hard_switch) {

		if( ( currentCamera != null 
		     && currentCamera.cameraBase.blockSwitch ) 
		   || idx == activeCameraIndex ) {
			return;
		}
		if( currentCamera != null ) {
			currentCamera.cameraBase.EndCameraMode();
			currentCamera.cameraBase.modeActive = false;
		}
		//DebugDisplay.AddDebugText("switching to camera " + idx + " " + cameras[idx].cameraBase.cameraName);
        previousCameraIndex = activeCameraIndex;
		activeCameraIndex = idx;
		currentCamera = cameras[idx];
	
		currentCamera.cameraBase.modeActive = true;
		currentCamera.cameraBase.StartCameraMode();
		
		// hard switch
		if( do_hard_switch ) 
        {
			currentCamera.cameraBase.UpdateCameraMode();
            if( oculusActive )
            {
                if (!ovrPhysicsMove)
                {
                    OVRCameraParent.position = currentCamera.cameraTransform.position;
                }

				OVRCameraParent.rotation = currentCamera.cameraTransform.rotation;
			}
			else
            {
				mainCameraTransform.position = currentCamera.cameraTransform.position;
				mainCameraTransform.rotation = currentCamera.cameraTransform.rotation;
			}
		}		
	}
	
	private void _internalSwitchToCamera(int idx) {
		_subInternalSwitchToCamera(idx, true);
	}
	
	void IntroDownFadeFinished( object arg ) {
//		GlobalOceanShaderAdjust.SetDist(introCurrentSphereDist);
//        long diveInTime = StartupObject.ElapsedMilliseconds;
//		log.Trace("DiveInComplete being sent: " + diveInTime);

		SetIntroGodRayObjectActive( false );
		
		cameraState = CameraState.Normal;
		
        SwitchToCamera( CameraType.OculusCamera );

		HideIntroOcean(null);
		
        OculusCameraFadeManager.StartCameraFadeFromBlack(introFadeTime,null,null);		

        PostSphereLoadSetup();

		// Raise dive-in complete event
		if ( __diveInComplete != null ) 
        {
			__diveInComplete();
			
			__diveInComplete = null;
		}
		
		SwitchToCamera( CameraType.OculusCamera );

        currentCamera.cameraBase.IntroDownFadeFinished();
	}
	
	void HideIntroOcean(object arg) {

		if (ocean_intro)
			ocean_intro.SetActive(false);
	}

	void InitIntroCamera(bool loadIntro = false) {
		introGodRayObject = null;
		cameraState = CameraState.Intro;
		//introState = IntroCameraState.None;

		if (loadIntro)
			introState = IntroCameraState.WaitingForLoad;
		else
			introState = IntroCameraState.FadedBlack; //the fade at load time is broken!

		introTimer = introKeepBlackTime;
		introSpeed = 5f;
		introEndTriggered = false;

		//look for the start position
		GameObject startObj = GameObject.Find("OculusStart");

		if (startObj) {

			introSpeed = 0f;
			introState = IntroCameraState.MenuFalling;
			
			introLocation = startObj.transform.position;
			introRotationEuler = startObj.transform.rotation.eulerAngles;
		}
		else {

			if ((singleton.levelCameraData != null) && (singleton.levelCameraData.oculusStart != null)) {

				introLocation = singleton.levelCameraData.oculusStart.transform.position;
				introRotationEuler = singleton.levelCameraData.oculusStart.transform.rotation.eulerAngles;

				CameraManager.SetSpawnCamera(singleton.levelCameraData.oculusStart.transform);
			}
		}

		mainCameraTransform.position = introLocation;
		mainCameraTransform.rotation = Quaternion.Euler(introRotationEuler);

        if( oculusActive ) 
        {
            OVRCameraParent.position = introLocation;
			OVRCameraParent.rotation = Quaternion.Euler(introRotationEuler);

			//uh....
			OVRCameraController ovcc  = GameObject.Find("OVRCameraController").GetComponent<OVRCameraController>();

			if (startObj != null)
				ovcc.FollowOrientation = startObj.transform;
		}

//		introCurrentSphereDist = GlobalOceanShaderAdjust.CurrentDistance();
//		GlobalOceanShaderAdjust.SetDist(introDesiredSphereDist);
	}

	public static void SetSpawnCamera(Transform startObj) {

		//this doesn't do anything :(
		CameraManager.singleton.introLocation = startObj.position;
		CameraManager.singleton.introRotationEuler = startObj.rotation.eulerAngles;

		CameraManager.singleton.mainCameraTransform.position = CameraManager.singleton.introLocation;
		CameraManager.singleton.mainCameraTransform.rotation = Quaternion.Euler(CameraManager.singleton.introRotationEuler);
		
		if( IsInOculusMode() ) 
		{
			Debug.Log("Set orientation....??!");

			CameraManager.singleton.OVRCameraParent.position = CameraManager.singleton.introLocation;
			CameraManager.singleton.OVRCameraParent.rotation = Quaternion.Euler(CameraManager.singleton.introRotationEuler);

			CameraManager.singleton.SetYRotation(CameraManager.singleton.introRotationEuler.y);

			Vector3 eulers = startObj.rotation.eulerAngles;

			Debug.Log("Eulers of start position: " + eulers.ToString());

			CameraManager.singleton.SetYRotation(eulers.y);

			OVRCameraController ovcc  = GameObject.Find("OVRCameraController").GetComponent<OVRCameraController>();
			ovcc.FollowOrientation = startObj;
		}
	}   

	public static void IntroCameraExit() 
    {
        if (singleton.ovrRB != null &&
            !singleton.ovrRB.isKinematic)
        {
            singleton.ovrRB.velocity = Vector3.zero;
        }

		cameraState = CameraState.Normal;
	}
	
	public static bool IsInIntroCamMode() {
		return (cameraState == CameraState.Intro);
	}

	public static bool IsInMenuMode() {

		if (singleton.introState == IntroCameraState.MenuFalling)
			return true;

		return false;
	}
	
	public static void TriggerIntroCameraOkToEnd() {
//		log.Info("Level is loaded. Waiting for critters to finalize dive in.");

		singleton.introEndTriggered = true;

		//the intro camera may leave the parent with gravity on
		CameraManager.singleton.OVRCameraParent.gameObject.GetComponent<Rigidbody>().useGravity = false;
		//CameraManager.singleton.introState = IntroCameraState.FadedBlack; //the fade at load time is broken!
		CameraManager.singleton.introState = IntroCameraState.WaitingForLoad;

		FloatingMenuManager.SetMenuMode(FloatingMenuManager.MenuType.Travel);
		FloatingMenuManager.MakeMenuAvailable();
	}
		
	public static void SetIntroGodRayObjectActive( bool active ) {
		if( singleton.introGodRayObject != null ) {
			singleton.introGodRayObject.SetActive( active );
		}
	}
	
	void UpdateIntroCamera(float dt) {
		introTimer -= dt;
		// state changes
		if( introTimer < 0f ) {			
			switch( introState ) {
			case IntroCameraState.FadedBlack:
                OculusCameraFadeManager.StartCameraFadeFromBlack(introFadeTime, null, null);	
				introTimer = introStallMotionTime;
				introState = IntroCameraState.Stalling;
				break;
			case IntroCameraState.Stalling:
				introTimer = introMinMotionTime;
				introState = IntroCameraState.InMotion;
				break;
			case IntroCameraState.InMotion:
				if( cameras == null ) {
					introTimer = 0.5f;
				}
				else {
					introTimer = introDownTimeToFade;
					introState = IntroCameraState.DivingDown;
				}
				break;
			case IntroCameraState.DivingDown:
                OculusCameraFadeManager.StartCameraFadeToBlack(introFadeTime, IntroDownFadeFinished, null);	
				introState = IntroCameraState.FadingToGame;
				break;
			case IntroCameraState.SaverBlocked:
				
				break;

			case IntroCameraState.WaitingForLoad:
				introTimer = introStallMotionTime;
				introState = IntroCameraState.Stalling;
				break;
			}
		}
		
		// update chunk
		switch( introState ) {
		case IntroCameraState.FadedBlack:
		case IntroCameraState.Stalling:
		case IntroCameraState.WaitingForLoad:
			// do nothing
			break;
		case IntroCameraState.InMotion:
			if( introEndTriggered && introTimer < 0.5f ) {
				introAccel += 18f * dt;
				introMaxSpeed += 80f * dt;
			}
			
			introSpeed += introAccel * dt;
			if( introSpeed > introMaxSpeed ) {
				introSpeed = introMaxSpeed;
			}
			break;
		case IntroCameraState.DivingDown:
//			introAccel += 20f * dt;
//			introMaxSpeed += 100f * dt;
//			introSpeed += introAccel * dt;
//			if( introSpeed > introMaxSpeed ) {
//				introSpeed = introMaxSpeed;
//			}
			introDownPitchAccel += 3.8f * dt;
			MathfExt.AccelDampDelt_Angle(introDownPitch,introDownPitchAccel,introDownPitchDecel,dt,30f,ref introDownPitchSpeed,ref introPitch, ref introDownPitch, ref introPitchDownDecelActive);
			break;
		case IntroCameraState.FadingToGame:
			introDownPitchAccel += 3.8f * dt;
			MathfExt.AccelDampDelt_Angle(introDownPitch,introDownPitchAccel,introDownPitchDecel,dt,30f,ref introDownPitchSpeed,ref introPitch, ref introDownPitch, ref introPitchDownDecelActive);
			introTimer = 1f;
			break;

		case IntroCameraState.MenuFalling:
			break;
		}

        mainCameraTransform.rotation = Quaternion.Euler(introPitch,mainCameraTransform.rotation.eulerAngles.y,0f);
		if( introSpeed > 0f ) {
			Vector3 amt = mainCameraTransform.forward * ( introSpeed * dt );
			mainCameraTransform.position += amt;
			if( mainCameraTransform.position.y > introLocation.y ) {
				mainCameraTransform.position = new Vector3(mainCameraTransform.position.x, introLocation.y, mainCameraTransform.position.z);
			}
		}

        if( oculusActive ) {
			if (!ChooseVRSDK.SteamActive)
			{
	            if (!ovrPhysicsMove)
	    			OVRCameraParent.position = mainCameraTransform.position;
	            // in intro mode, rotation is coming through normally, so grab it directly from a VR cam
	            OVRCameraParent.rotation = ovrController.CameraLeft.transform.rotation;
			}
        }

//		DebugDisplay.AddDebugText("new cam " + mainCameraTransform.position);
	}
	
	public static void SetGlowEffect (float glowIntensity, int blurIterations, float blurSpread, Color glowTint)
	{
		GlowEffect ge = singleton.mainCamera.gameObject.GetComponent<GlowEffect>();
		if( ge != null ) {
			ge.glowIntensity = glowIntensity;
			ge.blurIterations = blurIterations;
			ge.blurSpread = blurSpread;
			ge.glowTint = glowTint;
		}
		
		ge = singleton.leftCamera.gameObject.GetComponent<GlowEffect>();
		if( ge != null ) {
			ge.glowIntensity = glowIntensity;
			ge.blurIterations = blurIterations;
			ge.blurSpread = blurSpread;
			ge.glowTint = glowTint;
		}

        if (singleton.rightCamera != null)
        {
		ge = singleton.rightCamera.gameObject.GetComponent<GlowEffect>();
		if( ge != null ) {
			ge.glowIntensity = glowIntensity;
			ge.blurIterations = blurIterations;
			ge.blurSpread = blurSpread;
			ge.glowTint = glowTint;
		}
	}
	}

	void Update () {
		float dt = Time.deltaTime;

		//Debug.Log("Update camera...? " + cameraState.ToString() + ", " + currentCamera.cameraType.ToString());

//		UpdateAspect(false);

		if( cameraState == CameraState.Intro ) 
        { 
			UpdateIntroCamera(dt);
            return;
		}
        			
        // old camera critter targeting
        if (!oculusActive)
        {
			if( currentTarget == null )
            {
				CritterInfo critter = SimInstance.Instance.GetCritterInfoFromIndex(0);
				if( critter != null ) 
                {
					currentTarget = critter.critterObject;
				}
			}
            else if( currentCamera != null &&
                     currentCamera.cameraBase != null &&
                     currentCamera.cameraBase.GetFollowsTargets() ) 
            {
				currentTarget = SimManager.GetCameraTarget();
			}
        }

        Vector3 oldPos = currentCamera.cameraTransform.position;
        prevCamPos = currentCamera.cameraTransform.position;

		//Debug.Log("Updating camera: " + currentCamera.cameraType.ToString());

		currentCamera.cameraBase.UpdateCameraMode();
		
		// run our camera collision
		if( currentCamera.cameraBase.runCollision != CameraCollisionType.None )
        {
            bool bWasCollisionOn = currentCamera.cameraCollider != null ? currentCamera.cameraCollider.enabled : false;
            CollisionEnable(false);

            int layermask;

            if( currentCamera.cameraBase.runCollision == CameraCollisionType.Ground ) 
            {
				layermask = 1<<14; // ground layer only
			}
			else 
            {
				layermask = 1<<14|1<<13|1<<12|1<<11; // ground layer and all of our fish
			}

            Vector3 move_amt = currentCamera.cameraTransform.position - oldPos;
			Vector3 norm = Vector3.zero;
            currentCamera.cameraTransform.position = oldPos;
            CollisionHelpers.SphereSlide(currentCamera.cameraTransform, move_amt, 5.0f, layermask, 2, ref norm); // groundlayer = 14

//			if( CollisionHelpers.SphereSlide(mainCameraTransform,move_amt,10.0f,layermask,2,ref norm)) { // groundlayer = 14
//				DebugDisplay.AddDebugText("camera collided");
//			}

            CollisionEnable(bWasCollisionOn);
		}

        Vector3 newPos = currentCamera.cameraTransform.position;
        Quaternion newRot = currentCamera.cameraTransform.rotation;

        if( oculusActive ) 
        {
            if (!ovrPhysicsMove) // if true, its a non-physics driven OVR cam
            {
                OVRCameraParent.position = newPos;
            }
            OVRCameraParent.rotation = newRot;
		}

        mainCameraTransform.position = newPos;
        mainCameraTransform.rotation = newRot;			

        AudioManager.UpdateAudioSourceTransform( newPos, newRot );

        cameraForwardSet = false;
        cameraRightSet = false;
        cameraPositionSet = false;
		cameraFlattenedRotationSet = false;
        cameraEyePositionSet = false;
    }

	void FixedUpdate()
	{
		if (currentCamera != null)
		{
			currentCamera.cameraBase.FixedUpdateCameraMode();
		}
    }

    public void CollisionEnable(bool bEnabled)
    {
        if (currentCamera!= null &&
            currentCamera.cameraCollider != null )
        {
            currentCamera.cameraCollider.enabled = bEnabled;
        }
    }
	
	public static void GotoNextCamera(bool from_gui){
		singleton.currentCamera.cameraBase.GotoNextCamera(from_gui);
	}

	public static bool IntroOceanOn() {

		if (!ocean_intro)
			return false;

		return (ocean_intro.activeSelf);
	}

	public void SetOrientationOffset(Quaternion rot)
    {
        if (svrCam != null)
        {
            //svrCam.transform
        }
        else if (ovrController != null)
        {
            ovrController.SetOrientationOffset(rot);
        }
    }

	public void SetYRotation(float rot)
    {
        if (svrCam != null)
        {
            //svrCam
        }
        else if (ovrController != null)
        {
            ovrController.SetYRotation(rot);
        }
    }

    public void GetYRotation(ref float yRot)
    {
        if (svrCam != null)
        {

        }
        else if (ovrController != null)
        {
            ovrController.GetYRotation(ref yRot);
        }
    }

    public SphereCollider GetDispersionCollider()
    {
        if (ovrController != null)
        {
            return ovrController.gameObject.GetComponent<Collider>() as SphereCollider;
        }

        return null;
    }

    public void ResetDeviceOrientation()
    {
        if (svrCam != null)
        {
            var hmd = SteamVR.instance.hmd;
            if (hmd != null)
            {
//                hmd.ZeroTracker();
            }
        }

        if (ovrController != null)
        {
            OVRDevice.ResetOrientation();
        }
    }

    public void GetCameraOrientation(ref Quaternion originalRotation)
    {
        if (svrCam != null)
        {
            originalRotation = svrCam.transform.rotation;
        }
        else if (ovrController)
        {
            ovrController.GetCameraOrientation(ref originalRotation);
        }
    }
}
