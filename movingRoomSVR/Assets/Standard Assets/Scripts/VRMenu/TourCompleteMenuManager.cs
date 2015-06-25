using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OculusIAPAndroid;
using OculusIAPAndroid.Model;

public class TourCompleteMenuManager : MonoBehaviour {

//	FloatingMenuManager _fmm;

    static TourCompleteMenuManager _singleton;

    public TourCompleteButton exploreButton;
    public TourCompleteButton tourButton;
    public TourCompleteButton giantsButton;
    public TourCompleteButton arcticButton;
    public TextMesh habitatTitle;
    public TextMesh badgeProgress;

	public GameObject previewDisc;

	GameObject _newPreviewDisc;
    float _oldCullDist;
    bool bHandled;

    AllFishInSceneBadge allFishBadge;

    public void Init()
    {
        _singleton = this;
        allFishBadge = GetComponent<AllFishInSceneBadge>();
    }

	void Awake() {

//		_fmm = this.transform.parent.gameObject.GetComponent<FloatingMenuManager>();
	}

	// Use this for initialization
	void Start () {
        exploreButton.clickFunction = ExploreClicked;
        tourButton.clickFunction = TourClicked;
        giantsButton.clickFunction = TravelGiantsClicked;
        arcticButton.clickFunction = TravelArcticClicked;
    }


    public static void StartScene()
    {
        if (_singleton != null &&
            _singleton.allFishBadge != null)
        {
            _singleton.allFishBadge.StartScene();
        }
    }

    public static void SpeciesAdd(string speciesName)
    {
        if (_singleton != null &&
            _singleton.allFishBadge != null)
        {
            _singleton.allFishBadge.SpeciesAdd(speciesName);
        }
    }

    public static bool SpeciesInteract(string speciesName)
    {
        if (_singleton != null &&
            _singleton.allFishBadge != null)
        {
            return _singleton.allFishBadge.SpeciesInteract(speciesName);
        }

        return false;
    }

	void OnEnable() {

        bHandled = false;
		FloatingMenuManager.ShowSelectionDot();

        if (habitatTitle != null)
        {
            string title = App.SphereManager.currentSphereName;
            InventoryItem item = StoreManager.GetItemByAssetBundleName(title);
            if (item != null)
            {
                title = item.itemName;
            }

            habitatTitle.text = title;
        }

        if( App.SphereManager.currentSphereName.Equals("whale") ) {
            arcticButton.gameObject.SetActive( true );
            giantsButton.gameObject.SetActive( false );
        }
        else {
            giantsButton.gameObject.SetActive( true );
            arcticButton.gameObject.SetActive( false );
        }
//        if (App.UserManager.educationalMode == 1)
//        {
//            exploreButton.Highlight(false);
//            exploreButton.gameObject.SetActive(true);
//            tourButton.gameObject.SetActive(false);
//        }
//        else
//        {
//            tourButton.Highlight(false);
//            exploreButton.gameObject.SetActive(false);
//            tourButton.gameObject.SetActive(true);
//        }

        if (badgeProgress != null &&
            allFishBadge != null)
        {
            // todo fill w/number
            badgeProgress.text = allFishBadge.GetNumSpeciesInteracted() + " / " + allFishBadge.GetNumSpeciesTotal();
        }

        if (GlobalOceanShaderAdjust.Instance != null && GlobalOceanShaderAdjust.Instance.dist < FloatingMenuManager.menuOceanAdjustDist)
        {
            _oldCullDist = GlobalOceanShaderAdjust.Instance.dist;
            GlobalOceanShaderAdjust.SetDist(FloatingMenuManager.menuOceanAdjustDist);
        }
        else
        {
            _oldCullDist = -1f; // default?
        }
	}
	
    void OnDisable()
    {
        // will reset dist and background color to shallow color
        if (GlobalOceanShaderAdjust.Instance != null && _oldCullDist >= 0f)
        {
            GlobalOceanShaderAdjust.SetDist(_oldCullDist);
        }

        if (!bHandled)
        {
            TravelMenuManager.ForceCurrentSphereTravel = true;
            bHandled = true;
            FloatingMenuManager.ShowMenuDelayed(FloatingMenuManager.MenuType.Travel, 0.05f);
        }
    }

    void TravelClicked() {
//		Dictionary<string,string>dict = new Dictionary<string,string>();
//		dict.Add("purchase_habitatname", _itemToBuy.Id);

//		App.MetricsManager.Track("purchase_screen_no", dict);
        OculusTourCameraMode.tourStop = null;
        FloatingMenuManager.HideMenu(false, true, true);
        TravelMenuManager.ForceCurrentSphereTravel = true;
        FloatingMenuManager.ShowMenu(FloatingMenuManager.MenuType.Travel);
        bHandled = true;
	}

    void TravelGiantsClicked() {
        OculusTourCameraMode.tourStop = null;
        FloatingMenuManager.HideMenu(false, true);
        TravelMenuManager.StartTravel("whale", true);
        bHandled = true;
    }
    
    void TravelArcticClicked() {
        OculusTourCameraMode.tourStop = null;
        FloatingMenuManager.HideMenu(false, true);
        TravelMenuManager.StartTravel("arctic", true);
        bHandled = true;
    }
    
    void TourClicked()
    {
//        if( giantsButton.gameObject.activeSelf ) {
//            TravelArcticClicked();
//        }
//        else {
//            TravelGiantsClicked();
//        }
        bHandled = true;
        PlayerPrefs.SetInt(UserManager.USER_EDUCATION_MODE, 0);
        App.UserManager.educationalMode = 0;

        OculusTourCameraMode.ResetTour();

        OculusTourCameraMode.openMenuOnComplete = OculusTourCameraMode.init_openMenuOnComplete;
        OculusTourCameraMode.tourStart = OculusTourCameraMode.init_tourStart;
        OculusTourCameraMode.tourStop = OculusTourCameraMode.init_tourStop;
        OculusTourCameraMode.deactivateObjects = OculusTourCameraMode.init_deactivateObjects;
        FloatingMenuManager.HideMenu(false, true);

        AppBase.Instance.RestartApp();
    }


	public void ExploreClicked() {

// Educational mode is currently disabled.
//        PlayerPrefs.SetInt(UserManager.USER_EDUCATION_MODE, 1);
//        App.UserManager.educationalMode = 1;

//		Dictionary<string,string>dict = new Dictionary<string,string>();
//		dict.Add("dont_purchase_habitatname", _itemToBuy.Id);
		
//		App.MetricsManager.Track("purchase_screen_yes", dict);

        FloatingMenuManager.HideMenu(false, true);
        CameraManager.SwitchToCamera(CameraType.OculusCamera);
	}
}
