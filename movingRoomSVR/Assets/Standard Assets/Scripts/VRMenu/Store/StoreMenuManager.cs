using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OculusIAPAndroid;
using OculusIAPAndroid.Model;

public class StoreMenuManager : MonoBehaviour {
#if UNITY_ANDROID

	public string sceneToBuy;

//	FloatingMenuManager _fmm;

	InventoryItem _itemToBuy;

	static StoreMenuManager _singleton;

	public TextMesh descriptionText;
	public TextMesh titleText;
	public TextMesh priceText;

	public GameObject previewDisc;

	GameObject _newPreviewDisc;
    float _oldCullDist;

	//we need to grab the boot travel menu too
	static TravelMenuManager _bootTMM;

	void Awake() {

		_singleton = this;
//		_fmm = this.transform.parent.gameObject.GetComponent<FloatingMenuManager>();

        // find the root object of the StaticTravelMenuParent because the StaticTravelMenuParent may never have been initialized
        // if we only entered the store for the first time in a scene/habitat
        GameObject introFloor =  GameObject.Find("IntroFloor");
        if (introFloor != null)
        {
            _bootTMM = introFloor.GetComponentInChildren<TravelMenuManager>();
        }
	}

	// Use this for initialization
	void Start () {

		//hook up buttons
		StoreButton[] items = this.gameObject.GetComponentsInChildren<StoreButton>();
		
		foreach (StoreButton sb in items) {
			
			if (sb.gameObject.name == "BuyButton")
				sb.clickFunction = OKClicked;

			if (sb.gameObject.name == "CancelButton")
				sb.clickFunction = CancelPurchase;
		}
	
		//pull up info about scene (image, descriptoin, etc.)
	}

	void OnEnable() {

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
	}
	
    void OnDisable()
    {
        // will reset dist and background color to shallow color
        if (GlobalOceanShaderAdjust.Instance != null && _oldCullDist >= 0f)
        {
            GlobalOceanShaderAdjust.SetDist(_oldCullDist);
        }
    }

	// Update is called once per frame
	void Update () {
	
        if (InputManager.MoonlighBackTap() == InputManager.MoonlightBackInputType.ShortTap)
            CancelPurchase();

		if (Input.GetButtonUp("BackButton"))
			CancelPurchase();
	}

	public static void EndPurchaseInterface() {

		if (SphereManager.IsInBoot()) {
			FloatingMenuManager.HideMenu(false);
			FloatingMenuManager.ShowSelectionDot();
		}
		else {
			FloatingMenuManager.HideMenu(false, true);
			_singleton.Invoke("ShowTravel", .01f);
		}
	}

	void ShowTravel() {
		FloatingMenuManager.ShowMenu(FloatingMenuManager.MenuType.Travel);
	}

	public static void PurchaseScene() {

		//fake the purchase if were not using an oculus offer
		if (!Purchase.PurchaseItem(_singleton._itemToBuy))
			CompletePurchase();
	}

	public static void CompletePurchase() {

		//unlock the map--put it in some file (set the prefs for now)
		PlayerInventory.AddItem(_singleton._itemToBuy);
		PlayerInventory.SaveUser();
		
		if (_bootTMM != null)
			_bootTMM.UpdateTravelStatus();

	}

	public void InitWithOffer(Offer o) {

		descriptionText.text = o.Description;
		priceText.text = o.Price;
	}

	public void InitWithItem(InventoryItem item) {

		_itemToBuy = item;

		if (item.oculusOffer != null) {
			InitWithOffer(item.oculusOffer);
		}
		else {

			descriptionText.text = item.storeDescription;
			priceText.text = "$" + item.price;
		}
		
		//word wrap it
		WordWrapper ww = descriptionText.gameObject.GetComponent<WordWrapper>();
		ww.Wrap();

//		Dictionary<string,string>dict = new Dictionary<string,string>();
//		dict.Add("purchase_screen_habitatname", _itemToBuy.Id);
//		App.MetricsManager.Track("purchase_screen_load", dict);
	}

	void CancelPurchase() {

		//don't allow interface clicks if we're in the middle of buying something
		if (OculusIAPController.buying)
			return;

//		Dictionary<string,string>dict = new Dictionary<string,string>();
//		dict.Add("purchase_habitatname", _itemToBuy.Id);
//		App.MetricsManager.Track("purchase_screen_no", dict);

		EndPurchaseInterface();

		/*

		//hide the menu
		FloatingMenuManager.HideMenu(false);

		if (FloatingMenuManager.isBootMenu)
			FloatingMenuManager.ShowSelectionDot();
			*/
	}


	public void OKClicked() {

		//don't allow interface clicks if we're in the middle of buying something
		if (OculusIAPController.buying)
			return;

//		Dictionary<string,string>dict = new Dictionary<string,string>();
//		dict.Add("dont_purchase_habitatname", _itemToBuy.Id);	
//		App.MetricsManager.Track("purchase_screen_yes", dict);

		PurchaseScene();
		EndPurchaseInterface();
	}

	public void SetPreviewIcon(GameObject asset) {

		previewDisc.SetActive(false);

		//we need to clone this item, then parent it to the
		//icon and hide its parent

		if (_newPreviewDisc != null) {
			GameObject.DestroyImmediate(_newPreviewDisc);
			_newPreviewDisc = null;
		}

		_newPreviewDisc = GameObject.Instantiate(asset) as GameObject;

		_newPreviewDisc.transform.parent = previewDisc.transform.parent;
		_newPreviewDisc.transform.localScale = previewDisc.transform.localScale;
		_newPreviewDisc.transform.position = previewDisc.transform.position;
		_newPreviewDisc.transform.rotation = previewDisc.transform.rotation;

		_newPreviewDisc.layer = LayerMask.NameToLayer("VRUI");

		int numChildren = _newPreviewDisc.transform.childCount;

		for (int i = 0; i < numChildren; i++) {
			GameObject g = _newPreviewDisc.transform.GetChild(i).gameObject;
			g.layer = LayerMask.NameToLayer("VRUI");

			int subChildren = g.transform.childCount;

			for (int j = 0; j < subChildren; j++) {
				GameObject gg = g.transform.GetChild(j).gameObject;
				gg.layer = LayerMask.NameToLayer("VRUI");

				int subSubChildren = gg.transform.childCount;

				for (int k = 0; k < subSubChildren; k++) {

					GameObject ggg = gg.transform.GetChild(k).gameObject;
					ggg.layer = LayerMask.NameToLayer("VRUI");
				}
			}
		}


		_newPreviewDisc.SetActive(true);

		FloatingMenuItem fmi = _newPreviewDisc.GetComponent<FloatingMenuItem>();
        if (fmi != null && fmi.lockSprite != null)
        {
    		fmi.lockSprite.SetActive(true);
	    	fmi.lockSprite.layer = _newPreviewDisc.layer;
        }

		DestroyImmediate (_newPreviewDisc.GetComponent<FloatingMenuItem>());
		DestroyImmediate (_newPreviewDisc.GetComponent<BoxCollider>());
	}
#endif
}
