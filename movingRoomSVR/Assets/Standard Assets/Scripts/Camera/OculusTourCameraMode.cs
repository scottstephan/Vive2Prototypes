using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OculusTourCameraMode : BaseCameraMode {
    static public OculusTourCameraMode singleton;

    bool bUpdated;

    bool isPlaying;
    bool wasMenuUp;

    static public bool openMenuOnComplete;
    static public GameObject tourStart;
    static public GameObject tourStop;
    static public GameObject[] deactivateObjects;

    static public bool init_openMenuOnComplete;
    static public GameObject init_tourStart;
    static public GameObject init_tourStop;
    static public GameObject[] init_deactivateObjects;

    Transform tourStartTransform;
    Animation tourStartAnim;
    AnimationClip tourStartClip;

    Collider myCollider = null;

//    Vector3 lastPos;
    Vector3 lastMove;

/*    enum SoundState
    {
        Init,
        Still,
        Moving
    }

    SoundState soundState;*/

    Quaternion originalRotation;
	
    static public void Init()
    {
        init_openMenuOnComplete = openMenuOnComplete = false;
        init_tourStart = tourStart = null;
        init_tourStop = tourStop = null;
        init_deactivateObjects = deactivateObjects = null;
    }

    public void Awake()
    {
        singleton = this;

        myCollider = GetComponent<Collider>();
    }

    public void ToggleColliderCollision( bool on ) {
        if( myCollider == null ) {
            return;
        }

        myCollider.enabled = on;
    }

	public override void InitCameraMode() 
	{
        if( inited ) 
		{
            return;
        }
        
        base.InitCameraMode();
         
        myTransform = transform;

        cameraType = CameraType.OculusTourCamera;

        if (GetComponent<Rigidbody>())
		{
            GetComponent<Rigidbody>().freezeRotation = true;
		}

        originalRotation = myTransform.localRotation;
		
		cameraName = "Oculus Tour Camera";

        runCollision = CameraCollisionType.None;
                       		
        ToggleColliderCollision(false);
	}
	
    bool IsMoveTouchDown()
    {
        return Input.GetMouseButtonDown(0);
    }

	public override void StartCameraMode()
    {
		//temp
		Cursor.visible = false;

        tourStartTransform = tourStart.transform;
        tourStartAnim = tourStart.GetComponent<Animation>();
        tourStartClip = tourStartAnim.clip;
        tourStartAnim.enabled = true;
        tourStartAnim[tourStartClip.name].time = 0f;
        tourStartAnim[tourStartClip.name].normalizedTime = 0f;
        tourStartAnim[tourStartClip.name].speed = 1f;

        tourStartAnim.Play( tourStartClip.name );
        tourStartAnim.Sample();

        isPlaying = true;
        wasMenuUp = false;

        lastMove = Vector3.zero;
//        lastPos = tourStartTransform.position;

        bUpdated = false;
//        soundState = SoundState.Init;
        CameraManager.singleton.ovrPhysicsMove = false;
		if(CameraManager.singleton.ovrRB != null)
        {
			CameraManager.singleton.ovrRB.isKinematic = true;
        }

        CameraManager.singleton.CollisionEnable(false);

        ToggleColliderCollision(false);
        SetPositionOrientation(tourStartTransform.position, tourStartTransform.rotation, true);

        if (tourStop != null)
        {
            Renderer[] renderers = tourStop.GetComponentsInChildren<Renderer>();
            if (renderers != null)
            {
                for (int i=0; i< renderers.Length; ++i)
                {
                    renderers[i].enabled = false;
                }
            }
        }
        
        if (deactivateObjects != null)
        {
            for (int i=0; i<deactivateObjects.Length; ++i)
            {
                if (deactivateObjects[i] != null)
                {
                    deactivateObjects[i].SetActive(false);
                }
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

        CameraManager.UpdateCurrentCameraTransform();
    }

    public static void ResetTour()
    {
        if (tourStop != null)
        {
            Renderer[] renderers = tourStop.GetComponentsInChildren<Renderer>();
            if (renderers != null)
            {
                for (int i=0; i< renderers.Length; ++i)
                {
                    renderers[i].enabled = true;
                }
            }
        }
        
        if (tourStop == null && deactivateObjects != null)
        {
            for (int i=0; i<deactivateObjects.Length; ++i)
            {
                if (deactivateObjects[i] != null)
                {
                    deactivateObjects[i].SetActive(true);
                }
            }
        }
    }

	public override void EndCameraMode() 
    {
        ResetTour();

        ToggleColliderCollision(false);

        if (!isPlaying)
        {
            SimInstance.Instance.SlowdownCritters( false, true );
        }

        CameraManager.singleton.CollisionEnable(true);
	}

    bool IsPauseButtonDown()
    {
        if (FloatingMenuManager.IsMenuUp())
        {
            return false;
        }

        if (Input.GetButtonDown("Interact"))
        {
            return true;
        }

#if UNITY_EDITOR
        return Input.GetKeyDown(KeyCode.Space);
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

	public override void UpdateCameraMode() 
    {	
        bool bMenu = FloatingMenuManager.IsMenuUp();
        bool bDone = tourStartAnim.enabled && !tourStartAnim.isPlaying;

        if (bMenu)
        {
            if (!wasMenuUp)
            {
                tourStartAnim[tourStartClip.name].speed = 0f;
                wasMenuUp = true;
            }
        }
        else if (wasMenuUp)
        {
            tourStartAnim[tourStartClip.name].speed = 1f;
//            tourStartAnim[tourStartClip.name].speed = isPlaying ? 1f : SimInstance.slowdownMultiplierInv;
            wasMenuUp = false;
            isPlaying = true;
        }

        if (!bDone 
            && bUpdated 
            && IsPauseButtonDown())
        {
            isPlaying = !isPlaying;
            if (isPlaying)
            {
                ParticleKillRange.paused = false;
                FloatingMenuManager.ShowPause( false, true );
                OculusFPSCameraMode.singleton.PauseMovementSound( false );
                SimInstance.Instance.DidPause( false );
                GlobalOceanShaderAdjust.Instance.SetClickDarkEffect( false );
                //                SimInstance.Instance.SlowdownCritters( false, true );
                tourStartAnim[tourStartClip.name].speed = 1f;
                AudioManager.Instance.ResumeInfoVoiceClip();
            }
            else
            {
                ParticleKillRange.paused = true;
                FloatingMenuManager.ShowPause( true, true );
                OculusFPSCameraMode.singleton.PauseMovementSound( true );
                SimInstance.Instance.DidPause( true );
                GlobalOceanShaderAdjust.Instance.SetClickDarkEffect( true );
                //                SimInstance.Instance.SlowdownCritters( true, true );
                tourStartAnim[tourStartClip.name].speed = 0f;
//                tourStartAnim[tourStartClip.name].speed *= SimInstance.slowdownMultiplierInv;
                AudioManager.Instance.PauseInfoVoiceClip();
            }
        }       

        if (!bMenu)
        {
            Vector3 curPos = tourStartTransform.position;
            Vector3 delta;

            if (bDone &&
                FloatingMenuManager.IsEndMenuFadingIn())
            {
                delta = lastMove;
                myTransform.position = CameraManager.singleton.OVRCameraParent.position;
            }
            else
            {
                delta = curPos - myTransform.position;
            }

//            lastPos = curPos;

            float deltaSqrMag = delta.sqrMagnitude;

            OculusFPSCameraMode.singleton.UpdateMovementSound( deltaSqrMag );

            CameraManager.singleton.ovrCharCtrl.Move(delta);
            myTransform.position = CameraManager.singleton.OVRCameraParent.position;

            if (deltaSqrMag > 0.001f)
            {
                lastMove = delta;
            }
        }

		// pull orientation from Oculus and apply it to this camera
		CameraManager.singleton.GetCameraOrientation(ref originalRotation);
		myTransform.rotation = originalRotation;

        if (Input.GetButtonDown("ResetOrientation"))
        {
            // Reset tracker position. 
            // We assume that the CameraController is at the desired neck location
// this spins the camera.
//            CameraManager.singleton.ovrController.SetYRotation(0f);
            OVRDevice.ResetOrientation();
        }

		bUpdated = true;

        if (bDone)
        {
            if (openMenuOnComplete)
            {
                openMenuOnComplete = false;
                TrackSphereEndStats();
//                FloatingMenuManager.ShowMenu(FloatingMenuManager.MenuType.TourComplete);
                TravelMenuManager.ForceCurrentSphereTravel = true;
                FloatingMenuManager.TransitionWhite = true;
                FloatingMenuManager.ShowMenu(FloatingMenuManager.MenuType.Travel, false, true );
            }
            else if (tourStop != null)
            {
                StoryBeatInteract sb = tourStop.GetComponent<StoryBeatInteract>();
                if (sb != null)
                {
                    sb.TriggerInteract();
                }
            }
        }

        CameraManager.UpdateCurrentCameraTransform();
	} 

    void TrackSphereEndStats()
    {
        string playTimeSpherePrefName = "PLAYTIME_"+App.SphereManager.currentSphereName;
        float habitatPlayTime = (Time.time - SphereManager.habitatPlayStart);
        float accumTime = PlayerPrefs.GetFloat(playTimeSpherePrefName,0f);
        accumTime += habitatPlayTime;
        PlayerPrefs.SetFloat(playTimeSpherePrefName, accumTime);
        int agg = 1;
        // between 1 and 5 minutes
        if( accumTime >= 60f && accumTime < 300f ) {
            agg = 2;
        }
        // between 5 and 10 minutes
        else if( accumTime >= 300f && accumTime < 600f ) {
            agg = 3;
        }
        // between 10 and 60 minutes
        else if( accumTime >= 600f && accumTime < 3600f ) {
            agg = 4;
        }
        // more than 60 minutes
        else if( accumTime >= 3600f ) {
            agg = 5;
        }
        App.MetricsManager.Stage("habitat_play_time", agg.ToString() );
        
        string playCountSpherePrefName = "PLAYCOUNT_"+App.SphereManager.currentSphereName;
//        string playCountTotalPrefName = "PLAYCOUNT_TOTAL";
        int numSpherePlays = PlayerPrefs.GetInt (playCountSpherePrefName, 0);
        ++numSpherePlays;
        PlayerPrefs.SetInt (playCountSpherePrefName, numSpherePlays);
        agg = 1;
        if( numSpherePlays > 5 && numSpherePlays <= 10 ) {
            agg = 2;
        }
        else if( numSpherePlays > 10 && numSpherePlays <= 50 ) {
            agg = 3;
        }
        else if( numSpherePlays > 50 ) {
            agg = 4;
        }
        App.MetricsManager.Stage("habitat_playthrough_num", agg.ToString ());
        string event_name = "habitat_play_"+App.SphereManager.currentSphereName;
        App.MetricsManager.TrackStaged(event_name);

        // track global aggregate
        int numTotalPlays = PlayerPrefs.GetInt ("PLAYCOUNT_TOTAL", 0);
        ++numTotalPlays;
        PlayerPrefs.SetInt ("PLAYCOUNT_TOTAL", numTotalPlays);
        agg = 1;
        if( numTotalPlays > 5 && numTotalPlays <= 10 ) {
            agg = 2;
        }
        else if( numTotalPlays > 10 && numTotalPlays <= 50 ) {
            agg = 3;
        }
        else if( numTotalPlays > 50 ) {
            agg = 4;
        }       
        App.MetricsManager.Stage("habitats_played", agg.ToString (), true);

        float totalPlayTime = PlayerPrefs.GetFloat ("PLAYTIME_TOTAL", 0f);
        totalPlayTime += habitatPlayTime;
        PlayerPrefs.SetFloat ("PLAYTIME_TOTAL", 0f);
        agg = 1;
        // between 1 and 5 minutes
        if( totalPlayTime >= 60f && totalPlayTime < 300f ) {
            agg = 2;
        }
        // between 5 and 10 minutes
        else if( totalPlayTime >= 300f && totalPlayTime < 600f ) {
            agg = 3;
        }
        // between 10 and 60 minutes
        else if( totalPlayTime >= 600f && totalPlayTime < 3600f ) {
            agg = 4;
        }
        // more than 60 minutes
        else if( totalPlayTime >= 3600f ) {
            agg = 5;
        }
        App.MetricsManager.Stage("play_time_global", agg.ToString ("0.00"));
        App.MetricsManager.TrackStaged("global_play");
    }
       
}
