using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TravelMenuManager : MonoBehaviour {

	bool _inputBack;
	//bool _inputSelect;
//	bool _inputDebug;

	FloatingMenuManager _fmm;

	public static string _currentTravelTarget;
    public static bool ForceCurrentSphereTravel;

	public GameObject blocker;

    float _oldCullDist;
    static public bool HasTraveled;

	void Awake() {

		_fmm = GameObject.Find("FloatingMenuManager").GetComponent<FloatingMenuManager>();
	}

	// Use this for initialization
	void Start () {

		UpdateTravelStatus();
	}
	
	// Update is called once per frame
	void Update () {

		ButtonInput();
		ProcessInput();
	}

	void ButtonInput() {
		
		_inputBack = false;
//		_inputDebug = false;

        if (CameraManager.IsInTravelCamera())
            return;

		if (SphereManager.IsInBoot())
			return;

#if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.Tab))
			_inputBack = true;
		
		if (Input.GetKeyUp(KeyCode.Mouse0))
			_inputBack = true;
#endif	

		//if (Input.GetKeyUp(KeyCode.JoystickButton0))
		if (Input.GetButtonUp("BackButton"))
			_inputBack = true;
		
		if (InputManager.AnyMoonlightBackTap())
			_inputBack = true;
	}
	
	bool ProcessInput() {

		if (!_inputBack)
			return false;
		
		return true;
	}

	public void ResetAllButtons() {
		
		FloatingMenuItem[] items = this.gameObject.GetComponentsInChildren<FloatingMenuItem>();
		
		foreach (FloatingMenuItem fmi in items) {
			fmi.Highlight(false);
		}
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

	void StartBuy(object fmi) {

		FloatingMenuItem f = (FloatingMenuItem)fmi;
		FloatingMenuManager.SetupShopScreen(f);

		FloatingMenuManager.HideMenu(false);
		FloatingMenuManager.ShowMenu(FloatingMenuManager.MenuType.Store);

        App.MetricsManager.Stage("menu_habitat_clicked", f.goToScene, true);				
		App.MetricsManager.TrackStaged("habitat_menu_click");
	}

	void ClickedBox(object fmi) {

		FloatingMenuItem f = (FloatingMenuItem)fmi;
		
		FloatingMenuManager.HideSelector();

		if (_fmm != null) {

			//if (SphereManager.destinationName != "sphere") {
			if (!SphereManager.IsInBoot()){
				FloatingMenuManager.HideMenu(false);
				_fmm.visibleAtStart = false; //we might just make this prefab non-destroyable
			}
		}

//        FloatingMenuManager.SetTravelPreviewActive( f.PreviewObject != null );

		_currentTravelTarget = f.goToScene;

        StartTravel(_currentTravelTarget);

        if (FloatingMenuManager.isBootMenu)
        {
		    AudioManager.PlayInGameAudio(SoundFXID.MenuSelect);
        }

		SphereManager.destinationName = f.goToScene;

		if (f.inventoryId != null) {

			InventoryItem i = StoreManager.GetItemById(f.inventoryId);

			if (i != null)
				SphereManager.destinationName = i.itemName;

		}
        	
        App.MetricsManager.Stage("menu_habitat_clicked", f.goToScene, true);                
        App.MetricsManager.TrackStaged("habitat_menu_click");
	}


	public static void StartTravel(string sceneName, bool force=false) 
    {
        if (!ForceCurrentSphereTravel &&
            !force && 
            sceneName == App.SphereManager.currentSphereName)
        {
            FloatingMenuManager.TransitionResumePlay = true;
            return;
        }

        HasTraveled = true;
        ForceCurrentSphereTravel = false;
        AudioManager.Instance.StopInfoVoiceClip();

        if( SimInstance.Instance.slowdownActive ) 
        {
            FloatingMenuManager.ShowPause( false );
            SimInstance.Instance.SlowdownCritters( false, true );
        }

        if( SimInstance.Instance.IsSimPaused() ) {
            FloatingMenuManager.ShowPause( false );
            SimInstance.Instance.DidPause( false );
            ParticleKillRange.paused = false;
        }

		if (sceneName == null)
        {
			sceneName = _currentTravelTarget;
        }

        SphereManager.destinationName = sceneName;

        InventoryItem item = StoreManager.GetItemByAssetBundleName(sceneName);
        if (item != null && item.itemName != null)
        {
            SphereManager.destinationName = item.itemName;
        }
        	
		AudioManager.Instance.PlayTrack(SoundFXID.TravelAmbient, WemoAudioTrackLogic.Ambient, 0.5f, 1.0f);
		App.SphereManager.TravelToSphere( sceneName );
	}

	void OnEnable() {
        HasTraveled = false;
		ResetAllButtons();
		UpdateTravelStatus();

		FloatingMenuManager.ShowSelectionDot();

        if (GlobalOceanShaderAdjust.Instance != null && GlobalOceanShaderAdjust.Instance.dist < FloatingMenuManager.menuOceanAdjustDist)
        {
            _oldCullDist = GlobalOceanShaderAdjust.Instance.dist;
            GlobalOceanShaderAdjust.SetDist(FloatingMenuManager.menuOceanAdjustDist);
        }
        else
        {
            _oldCullDist = -1f; // default?
        }

        // do this after shader adjust b/c that resets color
        if (CameraManager.singleton != null)
        {
            CameraManager.singleton.SetBackgroundColor(Color.black);
        }
	}

    void OnDisable()
    {
        // will reset dist and background color to shallow color
        if (!HasTraveled &&
            GlobalOceanShaderAdjust.Instance != null &&
            _oldCullDist >= 0f)
        {
            GlobalOceanShaderAdjust.SetDist(_oldCullDist);
        }
    }

	public void UpdateTravelStatus() {

		//go through each habitat and see if we need to buy it, or it's new
		FloatingMenuItem[] items = this.gameObject.GetComponentsInChildren<FloatingMenuItem>();
		
		foreach (FloatingMenuItem fmi in items) {

			if ((fmi.inventoryId != null) && (fmi.inventoryId != "")){

				if (!PlayerInventory.HasItem(fmi.inventoryId))
					fmi.SetLocked();
				else
					fmi.ClearStatus();
			}
		}
  	}
}
