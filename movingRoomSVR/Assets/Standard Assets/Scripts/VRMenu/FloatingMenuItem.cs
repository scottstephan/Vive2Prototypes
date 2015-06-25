using UnityEngine;
using System.Collections;

public class FloatingMenuItem : VRButton {
	
	public GameObject lockSprite;
    public GameObject lockText;
    public string goToScene;

	public string inventoryId; //this is the acutal ID of the scene in the inventory
//	GameObject _glowRing;
	
    public Animation animatedObject;
    public AnimationClip animClipNormal;
    public AnimationClip animClipHover;
    bool _selected;
	bool _isLocked;
	bool _isNew;
    Renderer _renderer;

	public Material hoverMaterial;
	Color _startingColor;
	Material _defaultMaterial;

    public GameObject ContinueObject = null;
    public GameObject ContinueHideObject = null;
    public GameObject PreviewObject = null;

    MeshRenderer[] cartIcons;

	public override void Awake() 
    {
        base.Awake ();

        _renderer = GetComponent<Renderer>();

        if (_renderer != null)
        {
            _defaultMaterial = this.gameObject.GetComponent<Renderer>().material;

            if (GetComponent<Renderer>().material != null &&
                GetComponent<Renderer>().material.HasProperty("_Color"))
            {
	    	    _startingColor = GetComponent<Renderer>().material.color;
            }
            else
            {
                _startingColor = Color.white;
            }
        }

		_localScale = myTransform.localScale;
//		_glowRing = this.transform.FindChild("glowRing").gameObject;

        if (lockSprite != null)
        {
            cartIcons = lockSprite.GetComponentsInChildren<MeshRenderer>();
        }

        if (ContinueObject != null)
        {
            ContinueObject.SetActive(false);
        }
    }

	// Use this for initialization
	void Start () {
	
		_selected = false;

		if ((goToScene == null) || (goToScene == ""))
        {
			enabled = false;
        }
		else
        {
			enabled = true;
        }
	}

    public override void OnEnable()
    {
        base.OnEnable ();

        if (ContinueObject != null)
        {
            bool cont_active = goToScene == App.SphereManager.currentSphereName && !TravelMenuManager.ForceCurrentSphereTravel;

            ContinueObject.SetActive( cont_active );
            if( PreviewObject != null ) {
                PreviewObject.SetActive( !cont_active );
            }

            if( ContinueHideObject != null ) {
                ContinueHideObject.SetActive( !cont_active );               
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
	
		ButtonInput();

		if (ProcessInput())
			OnClick();
	}

	public override bool OnClick() {

		//we're in fade--already selected
		if (!_highlightedRotation)
			return false;

		if (!_isEnabled ||
            FloatingMenuManager.IsInConfirmQuitMenu())
        {
			return false;
        }

		if (_isLocked)
			SendMessageUpwards("StartBuy", this); //locked scenes go to the purchase flow
		else
			SendMessageUpwards("ClickedBox", this);

        PlayClickSound();

		return true;
	}

    // HACKORIFIC!
    public override void CheckHighlightRotation( bool force_off = false) {
        if( !_highlightedTranslation && !force_off ) {
            return;
        }

 
        Vector3 camCenter = CameraManager.GetEyePosition();
        Vector3 camFwd = CameraManager.GetCurrentCameraForward();

        RaycastHit hit;
        
        bool result = Physics.SphereCast(camCenter, 10f, camFwd, out hit, 10000f, 1<<31);
        if( force_off ) {
            result = false;
        }

        bool set = false;
        if (result && !_highlightedRotation) 
        {
            set = true;
//            Debug.Log("Rotate SOME SHIT -- ONNN");
            
            containerTargetRotation = containerHoverRotation;
            containerRotTimer = 0f;
            
            if (_renderer != null &&
                _renderer.material != null)
            {
                _renderer.material.color = Color.white;
            }
            
            if (animatedObject != null)
            {
                animatedObject.CrossFade(animClipHover.name, 0.4f);
            }
        }
        else if( !result && _highlightedRotation ) {
            
            set = true;
//            Debug.Log("Rotate SOME SHIT -- OFF");

            containerTargetRotation = containerNormalRotation;
            containerRotTimer = 0f;
            
            if (_renderer != null &&
                _renderer.material != null)
            {
                GetComponent<Renderer>().material.color = _startingColor;
            }
            
            if (animatedObject != null)
            {
                animatedObject.CrossFade(animClipNormal.name, 0.8f);
            }
        }

        if( set ) {
            _highlightedRotation = result;
            
            if (cartIcons != null)
            {
                for (int i=0; i<cartIcons.Length; ++i)
                {
                    cartIcons[i].material.SetFloat("Opacity Mult", _highlightedRotation ? 2.5f : 1f);
                }
            }
            
            if (_isLocked)
            {
                if (lockText != null)
                {
                    lockText.SetActive(_highlightedRotation);
                }
            }

            // hacky to get the text to turn off when we are no longer hovering. we want the rotation to complete before hover objects turn on for these, but not when stopping hover.
            if( !_highlightedRotation ) {
                if (activeNormal != null)
                {
                    activeNormal.SetActive(!_highlightedRotation);
                }
                
                if (activeHover != null)
                {
                    activeHover.SetActive(_highlightedRotation);
                }
            }

            SwapMaterial();
            SetContainerhighlightShaders(false);
        }
    }

	public override void Highlight(bool set, bool on_enable = false) {

//		Debug.Log("FMI - HIGHLIGHT SOME SHIT");
//		Debug.Log("FMI - HIGHLIGHT = " + set);

		//can't highlight disabled buttons
		if (!_isEnabled)
			return;

        if (set && !_highlightedTranslation) 
        {
            if (_tween != null)
            {
                _tween.Kill();
            }
            
            //          Debug.Log("ZOOM HIGHLIGHT -- ONNN");
            
            containerTargetPos = containerHoverPos;
            containerTransTimer = 0f;
            
        }
        else {            
            //          Debug.Log("ZOOM HIGHLIGHT -- OFF");
            
            if (_tween != null)
            {
                _tween.Kill();
            }
            
            if( !MathfExt.Approx( containerNormalPos, containerTargetPos, 0.001f ) ) {
                containerTargetPos = containerNormalPos;
                containerTransTimer = 0f;
            }            
        }

        // make sure the rotational highlight state is handled.
        CheckHighlightRotation( true );

        if( on_enable ) {
            containerTransform.localRotation = containerTargetRotation;
            containerPositionalTransform.localPosition = containerTargetPos;
            containerTransTimer = -1f;
            containerRotTimer = -1f;
        }
        
        _highlightedTranslation = set;       
	}

	public bool highlighted {

		get {return(_highlightedRotation);}
	}

	void ToggleEnabled(bool en) {

		_isEnabled = en;
	}

	public bool selected {
		get {return(_selected);}
	}

	public bool isNew {

		get {return(_isNew);}
	}

	public bool isLocked {

		get {return(_isLocked);}
	}

	public void SetNew() {

		_isNew = true;

        if (lockSprite != null)
        {
    		//lockSprite.renderer.material.SetColor("_TintColor", Color.yellow);
		    lockSprite.SetActive(true);
        }

        if (lockText != null)
        {
			lockText.SetActive(_highlightedRotation);
        }
    }

	public void SetLocked() {

		_isLocked = true;
		//lockSprite.renderer.material.SetColor("_TintColor", Color.red);
        if (lockSprite != null)
        {
		    lockSprite.SetActive(true);
        }

        if (lockText != null)
        {
			lockText.SetActive(_highlightedRotation);
        }
    }

	public void ClearStatus() {

//		Debug.Log("CLEAR STATUS");

		_isNew = false;
		_isLocked = false;

        if (lockSprite != null)
        {
		    lockSprite.SetActive(false);
        }

        if (lockText != null)
        {
            lockText.SetActive(false);
        }
	}

	public void ResetScale() {

		myTransform.localScale = _localScale;
	}

	public void SwapMaterial() {

		if (hoverMaterial != null &&
            _renderer != null)
        {
    		if (highlighted)
            {
                _renderer.material = hoverMaterial;
            }
		    else 
            {
                _renderer.material = _defaultMaterial;
            }
        }
	}

}
