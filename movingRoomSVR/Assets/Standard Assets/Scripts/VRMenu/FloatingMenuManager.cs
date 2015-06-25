using UnityEngine;
using System.Collections;
using System;

public class FloatingMenuManager : MonoBehaviour {

	public enum FollowType {FollowCamera, DontFollowCamera, FollowPlayer}

	static FloatingMenuManager	_singleton = null;
	static TravelMenuManager _bootMenu = null;

	static GameObject _introFloor;

    const float SELECTOR_DIST = 315f;

    static public FloatingMenuManager Instance
    {
        get
        {
            return _singleton;
        }
    }

    static public float menuOceanAdjustSize = -1f;
    static public float menuOceanAdjustDist = 2000f;
    static public GameObject menuOcean;
    static public GameObject[] normalOceans;


	GameObject _uiPoint;
	GameObject _playerBody;
	GameObject _orbitUIPoint;
    OrbitPlayer _orbitPlayerComponent;

	public AudioClip acMenuStart;
	public AudioClip acMenuEnd;
	public AudioClip acMenuHighlight;
	public AudioClip acMenuSelect;
    public AudioClip acMenuMusic;

	public MenuSelector selectorDot;

	GameObject[] _menuObjects;

	static GameObject _menuObject;
	static GameObject _selectedBlock;
	static int _vrLayerMask;

	static bool _menuVisible;

	static bool _inputBack;
	static bool _inputSelect;
	static bool _inputDebug;

    static bool _tourEnd = false;

    bool _firstFrame = false;
	bool _firstFrameSet = false;
    bool _firstAppRun = false;
    public const string FIRSTAPPRUNPREFSTR = "AppWasRun";

	public bool startInSelectMode = false;
	public bool menuAvailable = true;
	public bool visibleAtStart = false;
	public bool autoPodSelect = true;
//    bool dropped = false;
    bool platformUIStarted;

	public GameObject introOcean = null;

	public enum MenuType 
    {
        Travel = 0,
        Intro = 1,
        Tutorial = 2,
        Store = 3,
        Text = 4,
        WorldText = 5,
        Settings = 6,
        Video = 7,
        TourComplete = 8,
        PauseText = 9,
        ConfirmQuit = 10,
        IntroFirstTime = 11,
        DemoEnd = 12,
        Num = 13
    }

    enum MenuState
    {
        Hide,
        FadeInShow,
        Show,
        FadeInHide,
        FadeInShowDouble,
    }

    Vector3 playerDisplaceOldPos;
    bool bPlayerDisplaceApplied = false;
    Transform playerDisplaceMenuTransform;
	Transform playerDisplaceIntroTransform;

    static public bool TeleportCameraWhenDone;
    static public bool TeleportCameraRotateY;
    static public Vector3 TeleportCameraPosition;
    static public float TeleportCameraYaw;
    static public bool TransitionWhite;
    static public bool TransitionResumePlay;

	static MenuType _menuType;
    static MenuState _menuState;

	static GameObject _textObject;
	static GameObject _worldTextObject;
    static GameObject _travelTextObject;
    static GameObject _curTravelTextObject;
    static GameObject _pauseTextObject;
//    static GameObject _confirmQuitObject;
    static GameObject _travelGeoObject;
    static GameObject _loadingDisplay;
    static bool _bShowLoading;

	[HideInInspector]
	public bool spawnObject = false;

	static bool _runningAsPreview;
	static float _showTimestamp;
//	static float _lastTutorialShown;
//	static float _tutorialCooldown = 10f;

	static bool _tutorialOn;

    static Vector3 _playerForward = Vector3.forward;

	public TranslationUI translationUI;

    public bool runFirstTimeSequence = true;

    public bool showDemoEnd = false;
    public float demoEndTime = 5f;

    AudioClip unpauseMusicClip;

	void Awake() {

        platformUIStarted = false;

        if( runFirstTimeSequence ) {
            _firstAppRun = PlayerPrefs.GetInt(FIRSTAPPRUNPREFSTR, 0) == 0;
        }

		_introFloor = GameObject.Find("IntroFloor");

		if (_introFloor)
			_bootMenu = _introFloor.GetComponentInChildren<TravelMenuManager>();

		_singleton = this;
		_selectedBlock = null;
//		_lastTutorialShown = 0;
		_tutorialOn = false;

		_vrLayerMask = MakeLayerMask(false);

		_menuObjects = new GameObject[(int)MenuType.Num];
		_menuObjects[0] = this.transform.FindChild("TravelMenuParent").gameObject;
		_menuObjects[1] = this.transform.FindChild("StartScreenParent").gameObject;
//		_menuObjects[2] = this.transform.FindChild("TutorialParent").gameObject;
//		_menuObjects[3] = this.transform.FindChild("StoreParent").gameObject;
		_textObject = _menuObjects[4] = this.transform.FindChild("TextOverlayParent").gameObject;
        _travelTextObject = this.transform.FindChild("TravelTextOverlayParent").gameObject;
        _worldTextObject = _menuObjects[5] = this.transform.FindChild("TravelTextParent").gameObject;
//		_menuObjects[6] = this.transform.FindChild("SettingsMenuParent").gameObject;
//		_menuObjects[7] = this.transform.FindChild("VideoPassThroughParent").gameObject;
//        _menuObjects[8] = this.transform.FindChild("TourCompleteParent").gameObject;
        _pauseTextObject = _menuObjects[9] = this.transform.FindChild("PauseTextOverlayParent").gameObject;
//        _confirmQuitObject = _menuObjects[10] = this.transform.FindChild("ConfirmQuitParent").gameObject;
        _menuObjects[11] = this.transform.FindChild("IntroFirstTimeParent").gameObject;
        _menuObjects[12] = this.transform.FindChild("DemoEndParent").gameObject;

//        TourCompleteMenuManager tcmm = _menuObjects[8].GetComponent<TourCompleteMenuManager>();
//        tcmm.Init();

		_menuObject = (GameObject)_menuObjects[(int)_menuType];

		_menuVisible = false;

		GameObject menuStartTransform = GameObject.Find ("OculusMenuCameraPos");
        if (menuStartTransform != null)
        {
            playerDisplaceMenuTransform = menuStartTransform.transform;
        }

		GameObject introStartTransform = GameObject.Find ("OculusIntroCameraPos");
		if (introStartTransform != null)
		{
			playerDisplaceIntroTransform = introStartTransform.transform;
		}
	}

    static public bool IsFirstRun()
    {
        return _singleton._firstAppRun;
    }

	static int MakeLayerMask(bool dynamicOnly = false) {
		
        int vrLayerMask = 1 << LayerMask.NameToLayer("VRUI") | 1 << LayerMask.NameToLayer("VRUI");

		if (!dynamicOnly)
			vrLayerMask |= 1 << LayerMask.NameToLayer("VisibleVRUI");

		return (vrLayerMask);
	}

    public static void SetPlayerForward( Vector3 fwd ) {
        _playerForward = fwd;
    }
    
    public static void SetMenuMode(MenuType type) {

		_menuType = type;

        _menuObject = (GameObject)_singleton._menuObjects[(int)_menuType];

		switch (type) {

		case MenuType.Travel:
		    TravelMenuManager tmm = _menuObject.GetComponent<TravelMenuManager>();
		    tmm.UpdateTravelStatus();
			break;

		case MenuType.Intro:
//			FloatingMenuManager.HideSelector();
			break;
		}
	}

	// Use this for initialization
	void Start () {

		Shader.WarmupAllShaders ();
		
		_singleton = this;
		GameObject mo = GameObject.Find("MasterOceanObject");

		if (mo) 
        {
			TheBluApp tba = (TheBluApp)mo.GetComponent<TheBluApp>();

			if (tba)
				_runningAsPreview = false;
			else
				_runningAsPreview = true;

			CameraManager.StaticPostLoadSetup(true);
		}
		else
        {
			_runningAsPreview = true;
        }
		
		if (_runningAsPreview) 
        {
			menuAvailable = false;
		}

        SetMenuMode(MenuType.Intro);

		//for the main menu
		if (startInSelectMode)
        {
			selectorDot.gameObject.SetActive(true);
        }
		
		GameObject cman = GameObject.Find("CameraManager");
		
		if (!cman)
			return;
		
		_playerBody = cman.transform.FindChild("OVRPlayerBody").gameObject;

//		Transform cc = _playerBody.transform.Find("OVRCameraController");

        _uiPoint = CameraManager.singleton.rightCamera.transform.Find("UI Point").gameObject;

		GameObject go = GameObject.Find("UIFollower");

        if (go != null)
        {
            _orbitPlayerComponent = go.GetComponent<OrbitPlayer>();
		    _orbitUIPoint = go.transform.FindChild("OrbitUIPoint").gameObject;
        }

		_singleton.PlaceForCamera(_menuObjects[(int)MenuType.Video], FollowType.FollowCamera);

		//_steadyUIPoint = GameObject.Find("SteadyUIPoint").gameObject;
	}
	
    IEnumerator StartEndLevelMenuFade()
    {
        yield return new WaitForSeconds( 0.5f );
        OculusCameraFadeManager.FadeToBlack(0.3f);
        TransitionWhite = false;
    }
    
	void Update() {
		if (!_firstFrameSet) {
			_firstFrameSet = true;
			_firstFrame = true;
		}

	}

    // Update is called once per frame
	void LateUpdate () {

        if ((_menuState == MenuState.FadeInShow || _menuState == MenuState.FadeInShowDouble) &&
            OculusCameraFadeManager.IsFaded())
        {
            if (TransitionWhite)
            {
                if (_menuState != MenuState.FadeInShowDouble)
                {
                    StartCoroutine(StartEndLevelMenuFade());
                    _menuState = MenuState.FadeInShowDouble;
                }
            }
            else
            {
                ShowMenu(false, true, AllowMaskSet(_menuType));
            }
        }
        else if (_menuState == MenuState.FadeInHide && 
                OculusCameraFadeManager.IsFaded())
        {
            HideMenu(false);
        }

		if (_firstFrame) {

			_firstFrame = false;

            _loadingDisplay = GameObject.Find("LoadingDisplay") as GameObject;
            if (_loadingDisplay != null)
            {
                _loadingDisplay.SetActive(false);
            }

			if (visibleAtStart)
            {
                OVRDevice.ResetOrientation();
                StartCoroutine(SingleFrameDelayShowMenu( false, true, true ) );
            }
//			else if (!_runningAsPreview)
//				CameraManager.DropIntroCamera();
		}

		//this only does one thing, checks for the 'back' button
		ButtonInput();

		if (ProcessInput())
        {
			//if we've hit the menu button then toggle it
			if (_menuObject.activeSelf || isBootMenu)
            {
                if (AllowExitOnBack(_menuType)) 
                {
                    ShowConfirmQuitMenu();
			        return;
			    }
                else
                {
                    if (_menuType == MenuType.TourComplete)
                    {
                        HideMenu(true, false, true);
                    }
                    else
                    {
                        HideMenu(true);
                    }
                }
            }
			else
            {
				ShowMenu(true, false);
            }
		}

		UpdateSelector();
        if (_pauseTextObject.activeSelf)
        {
            Vector3 diff = CameraManager.GetCurrentCameraPosition() - CameraManager.singleton.prevCamPos;
            _pauseTextObject.transform.position += diff;
        }
	}

    void ShowConfirmQuitMenu()
    {
        if (platformUIStarted)
        {
            return;
        }
        // HideMenu (false, true, true);
        // ShowMenu(MenuType.ConfirmQuit);

#if UNITY_ANDROID && !UNITY_EDITOR
        OVRPluginEvent.Issue( RenderEventType.PlatformUIConfirmQuit );
        platformUIStarted = true;
#endif
    }

    void ShowGlobalMenu() 
    {
        if (platformUIStarted)
        {
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        OVRPluginEvent.Issue( RenderEventType.PlatformUI );
        platformUIStarted = true;
#endif
    }


    void OnApplicationPause( bool pause ) {
        // we need to reset our state if coming back from the oculus menu.
        if( !pause ) {
            platformUIStarted = false;
        }
    }

	void UpdateSelector() {

		//if we have no selector, we don't need to select
		if (!selectorDot.gameObject.activeSelf)
			return;
		
		//we should show some kind of crosshair?
		GameObject selBlock = CheckForSelection();
		//FloatingMenuItem fmi;
		VRButton vrb;

		//update our currently selected menu item
		if (selBlock != _selectedBlock) {
			
			if (_selectedBlock != null ) {

				vrb = _selectedBlock.GetComponent<VRButton>();

				if (vrb != null)
					vrb.Highlight(false);
				else
					selBlock = null;

				/*
				fmi = _selectedBlock.GetComponent<FloatingMenuItem>();
				
				if (fmi != null)
					fmi.Highlight(false);
				else {

					StoreButton sb = _selectedBlock.GetComponent<StoreButton>();

					if (sb)
						sb.Highlight(false);
					else {

						ToggleButton tb = _selectedBlock.GetComponent<ToggleButton>();

						if (tb)
							tb.Highlight(false);
						else
							selBlock = null; //why does this happen? Some objects have my new layer??
					}
				}*/
			}
			
			_selectedBlock = selBlock;
			
			if (_selectedBlock != null) 
            {
				vrb = _selectedBlock.GetComponent<VRButton>();
				
				if (vrb != null && vrb.isEnabled)
                {
                    _singleton.selectorDot.SetHot(true);
                    vrb.Highlight(true);
					AudioManager.Instance.PlayOneShot(acMenuHighlight);
				}
			}
			else
				_singleton.selectorDot.SetHot(false);
		}
		else if (selBlock == null)
			_singleton.selectorDot.SetHot(false);

	}

	public static void HideSelector() {

		_singleton.selectorDot.gameObject.SetActive(false);
	}

	public void HideTutorial() {

		TutorialFades tf = _menuObject.GetComponent<TutorialFades>();

		if (!tf)
			return;

		tf.FadeDisable();
	}
	
	public static void HideMenu(bool playSound, bool noMaskRestore = false, bool bImmediate = false) 
    {
//        Debug.Log ("FMM:HideMenu imm: "+bImmediate);

        if (playSound)
            AudioManager.Instance.PlayOneShot(_singleton.acMenuEnd);

        if (!bImmediate &&
            AllowMenuFade(_menuType) &&
            !OculusCameraFadeManager.IsFading())
        {
            if (_menuState != MenuState.FadeInHide)
            {
//                Debug.Log ("FMM:HideMenu FadeInHide");

                OculusCameraFadeManager.StartCameraFadeToBlack(GetMenuFadeOutTime(_menuType, _menuState), null, null);
                _menuState = MenuState.FadeInHide;
                return;
            }

            // travel code handles fade from black
            if (_menuType != MenuType.Travel ||
                TransitionResumePlay)
            {
                TransitionResumePlay = false;
                OculusCameraFadeManager.StartCameraFadeFromBlack(GetMenuFadeOutTime(_menuType, _menuState), null, null);
            }
        }

//        Debug.Log ("FMM:HideMenu Hide");

        _menuState = MenuState.Hide;
        OculusFPSCameraMode.disableMovement = false;
        ParticleKillRange.paused = false;
        SimInstance.Instance.DidPause( false );
        FloatingMenuManager.ShowPause( false );
        GlobalOceanShaderAdjust.Instance.SetClickDarkEffect( false );
        AudioManager.Instance.FadeOutAllMusic();
        ShowMenuOcean(false);

        if (!SimInstance.Instance.slowdownActive && !SimInstance.Instance.IsSimPaused() )
        {
            // only resume if not paused;
            AudioManager.Instance.ResumeInfoVoiceClip();
        }

        if (CameraManager.singleton.directionalLight != null)
        {
            CameraManager.singleton.directionalLight.enabled = true;
        }

        if (_singleton.bPlayerDisplaceApplied)
        {
            _singleton.bPlayerDisplaceApplied = false;
            _singleton._playerBody.transform.position = _singleton.playerDisplaceOldPos;
            CameraManager.UpdateCameraPositionFromParent();
            if (_singleton._orbitPlayerComponent != null)
            {
                _singleton._orbitPlayerComponent.UpdatePlayer();
            }

            UpdateLoading ();
        }

        if (TeleportCameraWhenDone)
        {
            TeleportCameraWhenDone = false;
            _singleton._playerBody.transform.position = TeleportCameraPosition;
            CameraManager.UpdateCameraPositionFromParent();
            if (_singleton._orbitPlayerComponent != null)
            {
                _singleton._orbitPlayerComponent.UpdatePlayer();
            }

            if (TeleportCameraRotateY)
            {
                TeleportCameraRotateY = false;
                CameraManager.singleton.SetYRotation(TeleportCameraYaw);
            }
        }

        TranslateOcean.UpdateMenu(false);

        _tutorialOn = false;
		_menuVisible = false;
		_selectedBlock = null;
		_menuObject.SetActive(false);
		_singleton.selectorDot.gameObject.SetActive(false);
		_menuObject.transform.parent = _singleton.transform;

		if ((_menuType != MenuType.Tutorial) || noMaskRestore) //to prevent flashes in transitions between menus
			GlobalOceanShaderAdjust.ClearMenuMask();

		if(!_runningAsPreview)
			HideAction(_menuType);

		_vrLayerMask = MakeLayerMask(false);

		//the non-floating boot menu can be in an inconsistent state
		if (isBootMenu)
			if (_bootMenu) {
				_bootMenu.ResetAllButtons();
				_bootMenu.UpdateTravelStatus();
			}
	}

	static void HideAction(MenuType state) {

		switch (state) {

		case MenuType.Intro:
			//we've hidden the intro
			//start state?
			if (_singleton.menuAvailable) {
				SetMenuMode(MenuType.Travel);

				if (_singleton.autoPodSelect)
                {
					ShowMenu(false, false);
                }
//                else if (!_singleton.dropped)
//                {
//                    _singleton.dropped = true;
//					  CameraManager.DropIntroCamera();
//                }
			}
//            else if (!_singleton.dropped)
//            {
//                _singleton.dropped = true;
//				  CameraManager.DropIntroCamera();
//            }

			break;

		case MenuType.Travel:
			SetMenuMode(MenuType.Travel);
			break;
		}
	}

	public static void ShowSelectionDot() {

		if (_singleton == null)
			return;

		_singleton.selectorDot.gameObject.SetActive(true); //only show this for certain menus!
		_singleton.selectorDot.SetHot(false);
	}

    public static void ShowMenuDelayed(MenuType type, float delay)
    {
        _singleton.StartCoroutine(_singleton._ShowMenuDelayed (type, delay));
    }

    
    IEnumerator _ShowMenuDelayed(MenuType type, float delay)
    {
        yield return new WaitForSeconds( delay );
        ShowMenu( type );
    }

	public static bool ShowMenu(MenuType type, bool bImmediate = false, bool tour_end = false ) {

//        Debug.Log("FMM:ShowMenu");

		if (IsMenuUp())
			return false;

        if( type == MenuType.Travel && tour_end ) {
            _tourEnd = tour_end;
            if( _singleton.showDemoEnd ) {
                type = MenuType.DemoEnd;
                ShowMenuDelayed( MenuType.Travel, _singleton.demoEndTime );
            }
        }

        if (_menuType != MenuType.IntroFirstTime &&
            _menuType != MenuType.Intro)
        {
    		if (OculusCameraFadeManager.IsFaded())
	    		return false;

    		if (OculusCameraFadeManager.IsFading())
	    		return false;
        }

        bool useMask = AllowMaskSet(type);

		SetMenuMode(type);

        return(ShowMenu(true, false, useMask, bImmediate));
	}

    static bool AdjustPlayerY(MenuType m)
    {
        return m == MenuType.Travel ||
               m == MenuType.DemoEnd ||
               m == MenuType.TourComplete ||
               m == MenuType.IntroFirstTime ||
               m == MenuType.Store;
    }

    static bool AllowMaskSet(MenuType m)
    {
        return m != MenuType.Tutorial &&
               m != MenuType.Text &&
               m != MenuType.IntroFirstTime;
    }

    static bool AllowExitOnBack(MenuType m)
    {
        return m == MenuType.Travel || (_menuType == MenuType.Intro && _singleton.selectorDot.gameObject.activeSelf);
    }

    static bool AllowPause(MenuType m)
    {
        return m != MenuType.IntroFirstTime;
    }

    static bool AllowMenuFade(MenuType m)
    {
        return m == MenuType.DemoEnd || 
               m == MenuType.TourComplete ||
               m == MenuType.Travel;
    }

    static bool AllowBack(MenuType m)
    {
        return m != MenuType.Intro && 
               m != MenuType.IntroFirstTime &&
               CameraManager.GetActiveCameraType() != CameraType.TravelCamera;
    }

    static bool AllowCameraLight(MenuType m)
    {
        return m != MenuType.Travel;
    }

    static float GetMenuFadeOutTime(MenuType m, MenuState s)
    {
        return 0.3f;
    }

    static float GetMenuFadeInTime(MenuType m, MenuState s)
    {
        if( m == MenuType.Intro ) {
            return 0.3f;
        }

        if( m == MenuType.Travel ) {
            return ( _tourEnd ? 1f : 0.3f );
        }

        if (s == MenuState.FadeInShowDouble)
        {
            return 1f;
        }

        return 1f;
    }

    public static void ShowMenuOcean(bool on)
    {
        if (menuOcean == null ||
            normalOceans == null)
        {
            return;
        }

        menuOcean.SetActive(on);

        if (on)
        {
            Vector3 camPos = CameraManager.GetCurrentCameraPosition();
            menuOcean.transform.position = new Vector3(camPos.x, menuOcean.transform.position.y, camPos.z);
        }

        for (int i=0; i<normalOceans.Length; ++i)
        {
            normalOceans[i].SetActive(!on);
        }
    }

    IEnumerator SingleFrameDelayShowMenu(bool playSound, bool bPlayMusic, bool useMask = true, bool bImmediate = false) {
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		ShowMenu( playSound, bPlayMusic, useMask, bImmediate );
    }

    public static bool ShowMenu(bool playSound, bool bPlayMusic, bool useMask = true, bool bImmediate = false) {
        if (playSound && !TransitionWhite) // brought up in-game
            AudioManager.Instance.PlayOneShot(_singleton.acMenuStart);

        if (!bImmediate && AllowMenuFade(_menuType))
        {
            if (_menuState != MenuState.FadeInShow)
            {
                _menuState = MenuState.FadeInShow;
                if (TransitionWhite)
                {
                    // set color to start fade from white alpha clear correctly
                    OculusCameraFadeManager.SetColor(new Color(0.9f,0.9f,0.95f,0f));
                    OculusCameraFadeManager.StartCameraFadeToColor(new Color(0.9f,0.9f,0.95f,1f), 4f, null, null);
                }
                else
                {
                    OculusCameraFadeManager.StartCameraFadeToBlack(GetMenuFadeInTime(_menuType, _menuState), null, null);
                }

//                Debug.Log("FMM:ShowMenu FadeInShow" + _menuType);

                return true;
            }

            OculusCameraFadeManager.StartCameraFadeFromBlack(GetMenuFadeInTime(_menuType, _menuState), null, null);
            _tourEnd = false;
        }

  //      Debug.Log("FMM:ShowMenu Show" + _menuType);

        _menuState = MenuState.Show;
        OculusFPSCameraMode.disableMovement = true;
        ParticleKillRange.paused = false;
        SimInstance.Instance.DidPause(AllowPause(_menuType));
        ShowMenuOcean(true);
        AudioManager.Instance.PauseInfoVoiceClip();

        if (!AllowCameraLight(_menuType) &&
            CameraManager.singleton.directionalLight != null)
        {
            CameraManager.singleton.directionalLight.enabled = false;
        }

        if (!_menuVisible) 
        {
//            OVRDevice.ResetOrientation();

            if (bPlayMusic && !AudioManager.Instance.IsPlaying( _singleton.acMenuMusic, WemoAudioTrackLogic.Ambient )) {
                // brought up at start, use music for the first menu use, otherwise use level's background music
                AudioManager.Instance.PlayTrack(_singleton.acMenuMusic, WemoAudioTrackLogic.Ambient, 0.5f, 1.0f);
            }
        }

		if (AdjustPlayerY(_menuType) &&
		    _menuType == MenuType.IntroFirstTime &&
		    !_singleton.bPlayerDisplaceApplied &&
		    _singleton.playerDisplaceIntroTransform != null)
		{
			Vector3 playerPos = _singleton._playerBody.transform.position;
			_singleton.bPlayerDisplaceApplied = true;
			_singleton.playerDisplaceOldPos = playerPos;
			_singleton._playerBody.transform.position = new Vector3(playerPos.x, _singleton.playerDisplaceIntroTransform.position.y, playerPos.z);
			//_singleton._playerBody.transform.position = new Vector3(playerPos.x, _singleton.playerDisplaceIntroTransform.position.y, _singleton.playerDisplaceIntroTransform.position.z);
			CameraManager.UpdateCameraPositionFromParent();
			if (_singleton._orbitPlayerComponent != null)
			{
				_singleton._orbitPlayerComponent.UpdatePlayer();
			}
		}


        if (AdjustPlayerY(_menuType) &&
            !_singleton.bPlayerDisplaceApplied &&
            _singleton.playerDisplaceMenuTransform != null)
        {
            Vector3 playerPos = _singleton._playerBody.transform.position;
            _singleton.bPlayerDisplaceApplied = true;
            _singleton.playerDisplaceOldPos = playerPos;
            _singleton._playerBody.transform.position = new Vector3(playerPos.x, _singleton.playerDisplaceMenuTransform.position.y, playerPos.z);
            CameraManager.UpdateCameraPositionFromParent();
            if (_singleton._orbitPlayerComponent != null)
            {
                _singleton._orbitPlayerComponent.UpdatePlayer();
            }
        }


        TranslateOcean.UpdateMenu(true);

        bool bOnlyDynamic = true;

        if (_menuType == MenuType.Travel && isBootMenu)
        {
            bOnlyDynamic = false;
        }

        _vrLayerMask = MakeLayerMask(bOnlyDynamic);
		
		_menuVisible = true;
		_selectedBlock = null;

		FollowType follow = FollowType.FollowCamera;

		if (_menuObject.GetComponent<DontFollowCamera>() != null)
			follow = FollowType.DontFollowCamera;

		if (_menuObject.GetComponent<FollowPlayer>() != null)
			follow = FollowType.FollowPlayer;

        if (_menuObject == _singleton._menuObjects[(int)MenuType.Travel] || 
            _menuObject == _singleton._menuObjects[(int)MenuType.DemoEnd] || 
            _menuObject == _singleton._menuObjects[(int)MenuType.Store] ||
            _menuObject == _singleton._menuObjects[(int)MenuType.TourComplete] ||
            _menuObject == _singleton._menuObjects[(int)MenuType.ConfirmQuit] ||
            _menuObject == _singleton._menuObjects[(int)MenuType.IntroFirstTime] ||
            _menuObject == _singleton._menuObjects[(int)MenuType.Intro]) 
        {
			_singleton.PlaceForCamera(_menuObject, follow, false);
		}
		else if (follow == FollowType.FollowPlayer)
        {
            _singleton.PlaceInWorld(_menuObject, _playerForward, true);
        }
		else
        {
			_singleton.PlaceForCamera(_menuObject, follow);
        }

        ////////////////////////
        // super demo hack.
        _singleton._menuObjects[(int)MenuType.DemoEnd].SetActive(false);
        ////////////////////////

        _menuObject.SetActive(true);

		//tutorial isn't masked out
		if (useMask)
        {
			GlobalOceanShaderAdjust.SetMenuMask();
        }

		_singleton.DisableButtonWithScene(App.SphereManager.currentSphereName);

		_showTimestamp = Time.time;

        if( _menuType == MenuType.Travel ) {
            App.MetricsManager.Track( "main_menu_loaded" );
        }

		return true;
	}

	public void PlaceAtUIPoint(GameObject menuObject) {

		//menuObject.transform.position = translationUI.transform.position;//_playerBody.transform.position;
	}

    public static void ShowPause( bool active, bool play_sfx = false ) 
    {
        if( play_sfx ) {
            AudioManager.Instance.PlayOneShot(_singleton.acMenuHighlight);
        }

        _pauseTextObject.SetActive( active );

        if( active ) 
        {
            _singleton.PlaceForCamera( _pauseTextObject, FollowType.DontFollowCamera, false );
            _singleton.unpauseMusicClip = AudioManager.Instance.GetCurrentMusicClip(WemoAudioTrackLogic.Ambient);
            if( _singleton.unpauseMusicClip == _singleton.acMenuMusic ) {
                _singleton.unpauseMusicClip = null;
            }
            AudioManager.Instance.PlayTrack(_singleton.acMenuMusic, WemoAudioTrackLogic.Ambient, 0.5f, 1.0f);
        }
        else
        {
//            if (_singleton.unpauseMusicClip != null)
            {
                AudioManager.Instance.PlayTrack(_singleton.unpauseMusicClip, WemoAudioTrackLogic.Ambient, 0.5f, 1.0f);
                _singleton.unpauseMusicClip = null;
            }
        }
    }

 	public void PlaceInWorld(GameObject menuObject, Vector3 fwd, bool noRotate = false, float spawnDistance = 0f) {

		menuObject.transform.parent = _playerBody.transform;

		//place it in front of the person, in the world
		Vector3 camPos = CameraManager.GetEyePosition();

		if (noRotate)
			fwd.y = 0;

		fwd.Normalize();
		
		fwd *= spawnDistance;
		camPos += fwd;

		menuObject.transform.position = camPos;
        menuObject.transform.LookAt( CameraManager.GetEyePosition() );
	}
	
	public void PlaceForCamera(GameObject guiObject = null, FollowType follow = FollowType.DontFollowCamera, bool facing = true) {

		if (guiObject == null)
			guiObject = _menuObject;

		this.transform.position = _playerBody.transform.position;

		if (!facing) {

			//this UI point does not pitch or roll.  This is for UIs that always
			//face 'forward'
			guiObject.transform.rotation = _orbitUIPoint.transform.rotation;
			guiObject.transform.position = _orbitUIPoint.transform.position;
		}
		else {
			guiObject.transform.rotation = _uiPoint.transform.rotation;
			guiObject.transform.position = _uiPoint.transform.position;
		}

		if (follow == FollowType.FollowCamera) {

			FollowCamera fc = guiObject.GetComponent<FollowCamera>();

			if ((fc == null) || (!fc.enabled))
				guiObject.transform.parent = _uiPoint.transform;
		}
		else if (follow == FollowType.FollowPlayer) 
			guiObject.transform.parent = this.transform;
		else
			guiObject.transform.parent = this.transform;
	}
		
	public GameObject CheckForSelection(bool moveToPoint = false) {

		//get camera midpoint
		Vector3 camCenter = CameraManager.GetEyePosition( true );
        Vector3 camFwd = CameraManager.GetCurrentCameraForward( true );
        Vector3 pushOut = camFwd * SELECTOR_DIST;

		RaycastHit hit;

        if (Physics.SphereCast(camCenter, 10f, camFwd, out hit, 10000f, _vrLayerMask))
        {          
            if (moveToPoint &&
                _selectedBlock != null)
            {     
                //if we have a selected block--update our dot
                selectorDot.SetTarget(hit.point);
            }
            else
            {
                selectorDot.SetTarget(camCenter + pushOut);
            }
            
            FloatingMenuItem fmi = hit.collider.gameObject.GetComponent<FloatingMenuItem>();
            
            if (fmi == null || fmi.enabled)
            {           
                return (hit.collider.gameObject);
            }
        }
        else
        {
            selectorDot.SetTarget(camCenter + pushOut);
        }

        return null;
	}

	void ButtonInput() {

		_inputBack = false;

        if (AllowBack(_menuType))
        {
    		if (Input.GetButtonUp("BackButton"))
            {
		    	_inputBack = true;
            }

    		if (InputManager.MoonlighBackTap() == InputManager.MoonlightBackInputType.ShortTap)
            {
	    		_inputBack = true;
            }
            else if (InputManager.MoonlighBackTap() == InputManager.MoonlightBackInputType.LongTap)
            {
                ShowGlobalMenu();
            }
        }

		/*
        if (!_menuVisible)
        {
            // bring up menu for short and long taps per oculus
            if (InputManager.MoonlighBackTap() == InputManager.MoonlightBackInputType.ShortTap || InputManager.MoonlighBackTap() == InputManager.MoonlightBackInputType.LongTap)
                _inputBack = true;
        }
        */

        if (_inputBack && !IsMenuUp() && _menuType != MenuType.Intro)
			SetMenuMode(MenuType.Travel);
	}

	bool ProcessInput() {

//		if (!menuAvailable)
//			return false;

		if (!_inputBack) //only if the back button is available! (We need to put this in the menu objects)
			return false;

        if( CameraManager.IsInTravelCamera() ) {
            return false;
        }

		if (_menuType == MenuType.Travel) {
	
			if (OculusCameraFadeManager.IsFaded())
				 return(false);
			
			if (OculusCameraFadeManager.IsFading())
				return(false);
		}

		return true;
	}

	public void DisableButtonWithScene(string sceneName) {
        /*
		FloatingMenuItem[] items = this.gameObject.GetComponentsInChildren<FloatingMenuItem>();

		foreach (FloatingMenuItem fmi in items) {
			if (sceneName == fmi.goToScene)
				fmi.enabled = false;
			else
				fmi.enabled = true;
		}*/
	}

	public void DisableMenu() {

		this.gameObject.SetActive(false);
	}

	public static bool IsMenuUp() {

		if (_menuObject == null)
			return false;

        if (_menuType == MenuType.DemoEnd ) {
            return false;
        }

		if (_menuObject.activeSelf)
			return true;

		return false;
	}

    public static bool IsEndMenuFadingIn()
    {
        return TransitionWhite && _menuState == MenuState.FadeInShow;
    }

    public static bool IsInConfirmQuitMenu()
    {       
        return _menuType == MenuType.ConfirmQuit;
    }

	public static void MakeMenuAvailable() {

		_singleton.menuAvailable = true;
	}

	public static void ShowTextOverlay(string text, Vector3 fwd, bool placeInWorld = false) {

		GameObject textObject = _textObject;

		if (placeInWorld)
        {
			textObject = _worldTextObject;
        }

		TextOverlay tt = textObject.GetComponent<TextOverlay>();
		tt.SetText(text);

		if (placeInWorld)
        {
            _singleton.PlaceInWorld(textObject, fwd, true);
        }
		else
        {
			_singleton.PlaceForCamera(textObject, FollowType.FollowCamera);
        }

		textObject.SetActive(true);
	}

/*    public static void SetTravelPreviewActive( bool on )
    {
        Transform menuChild = _travelTextObject.transform.FindChild("FloatingMenu");
        Transform textChild = menuChild.FindChild("TextPreview");
        textChild.gameObject.SetActive( on );
    }*/

    public static void ShowLoading()
    {
        if (_loadingDisplay != null)
        {
//			Debug.Log ("SHOW LOADING ON" + Time.realtimeSinceStartup);
            _bShowLoading = true;
            _loadingDisplay.SetActive(true);
            _loadingDisplay.transform.position = CameraManager.singleton.OVRCameraParent.position;
            _loadingDisplay.transform.rotation = CameraManager.GetCurrentCameraFlattenedRotation();

            if (CameraManager.singleton.directionalLight != null)
            {
                CameraManager.singleton.directionalLight.enabled = true;
            }
        }
    }

    // This should only get called when there is truely a big change in camera position.
    // if called everyframe on device, the neckposition code moves the camera position as you rotate your head, which causes popping.
    public static void UpdateLoading()
    {
        if (!_bShowLoading)
        {
            return;
        }

//        Debug.Log("ltp :: " + _loadingDisplay.transform.position + " :: " + CameraManager.singleton.OVRCameraParent.position);
        _loadingDisplay.transform.position = CameraManager.singleton.OVRCameraParent.position;
    }

    public static void HideLoading()
    {
        if (_loadingDisplay != null)
        {
//            Debug.Log ("SHOW LOADING OFF");
            _bShowLoading = false;
            _loadingDisplay.SetActive(false);
        }
    }

    public static void ShowTravelTextOverlay(string scene, Vector3 fwd)
    {        
        Transform menuChild = _travelTextObject.transform.FindChild("FloatingMenu");
        Transform textChild = menuChild.transform.FindChild(scene);

        if (textChild != null)
        {
            _curTravelTextObject = textChild.gameObject;
            _singleton.PlaceInWorld(_travelTextObject, fwd, true);
            _travelTextObject.SetActive(true);
            _curTravelTextObject.SetActive(true);
        }
        else
        {
            Debug.LogError("FloatingMenuManager could not find TravelTextOverlayParent child for scene: " + scene);
        }
    }

	public static void HideText() {
		HideMenu(false);

		//really hide text--I don't know why but on the device the text
		//is always visible after a load, it really doesn't make sense
		//if (_singleton._menuObjects[4] != null)
		//	_singleton._menuObjects[4].SetActive(false);

		if (_textObject != null)
        {
			_textObject.SetActive(false);
        }

		if (_worldTextObject != null)
        {
			_worldTextObject.SetActive(false);
        }

        if (_curTravelTextObject != null)
        {
            _curTravelTextObject.SetActive(false);
            _curTravelTextObject = null;
        }

        if (_travelTextObject != null)
        {
            _travelTextObject.SetActive(false);
        }
	}

	public static bool ShowTutorial(TutorialManager.ScreenMode page) {

		if (_tutorialOn)
			return false;

		TutorialManager.SetPage(page);
		SetMenuMode(MenuType.Tutorial);
		ShowMenu(false, false, false);

		return true;
	}

	/*
	public static bool ShowTutorial(TutorialManager.ScreenMode page) {

		if (CameraManager.IsInMenuMode())
			return false;

		if (CameraManager.IsInTravelCamera())
			return false;


		//this is to make longer delays on tutorial attempts after
		//a tutorial is shown.  Don't make the tutorial available
		//for awhile.
		if ((Time.time - _lastTutorialShown) < _tutorialCooldown)
			return false;

		//no tutorial in boot scene
		if (AppBase.Instance.RunningAsPreview())
			return false;

		//no tutorial when menu is up
		if (IsMenuUp())
			return false;

		//no tutorial in or during fades
		if (OculusCameraFadeManager.IsFaded())
			return false;

		if (OculusCameraFadeManager.IsFading())
			return false;

		//see if we've already seen this
		int shown = PlayerPrefs.GetInt(page.ToString(), -1);

		shown = -1; //never disable it for testing purposes!

		if (shown != -1)
			return false;

		TutorialManager.SetPage(page);
		
		if (ShowMenu(MenuType.Tutorial)) {

			switch (page) {

			case TutorialManager.ScreenMode.Move:
				_showedMove = true;
				break;

			case TutorialManager.ScreenMode.Select:
				_showedInteract = true;
				break;
			}
		}

#if !UNITY_EDITOR
		//don't set it right now
		PlayerPrefs.SetInt(page.ToString(), 1);
#endif

		_tutorialOn = true;
		_lastTutorialShown = Time.time;

		return true;
	}
	*/

	public static bool HasShownTutorial(TutorialManager.ScreenMode mode) {

//		if (mode == TutorialManager.ScreenMode.Move)
//			return(_showedMove);

//		if (mode == TutorialManager.ScreenMode.Select)
//			return(_showedInteract);

		return false;
	}

	public static void SetupShopScreen(FloatingMenuItem fmi) {
#if UNITY_ANDROID
		StoreMenuManager smm = _singleton._menuObjects[(int)MenuType.Store].GetComponent<StoreMenuManager>();

		if (!fmi)
			return;

		InventoryItem i = StoreManager.GetItemById(fmi.inventoryId);
		smm.InitWithItem(i);
		smm.SetPreviewIcon(fmi.gameObject);
#endif
	}

	public static bool isBootMenu {get {if (_introFloor && _introFloor.activeSelf) return true; else return false;}}

	public static bool tutorialOn { get {return(_tutorialOn);}}
	public static float showTime { get {return(_showTimestamp);}}
	public static GameObject currentMenuObject {get {return(_menuObject);}}

}
