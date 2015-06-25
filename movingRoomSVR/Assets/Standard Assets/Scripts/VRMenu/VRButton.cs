using UnityEngine;
using System.Collections;
using System;
using Holoville.HOTween;

public class VRButton : MonoBehaviour {

	protected Tweener _tween;
	protected bool _highlighted;
	protected bool _highlightedTranslation = false;
	protected bool _highlightedRotation = false;
	protected bool _isEnabled = true;
	protected Vector3 _localScale;
	protected bool _inputSelect;

	protected bool _scaleHighlight = false;
	public bool _toRotate = false;
	public bool _toTranslate = false;

	public Action clickFunction;
    public AudioClip clickSound;

    public GameObject activeNormal;
    public GameObject activeHover;

    protected Transform myTransform = null;
    public GameObject containerObject;
    protected Transform containerTransform = null;
    public Vector3 containerNormalAngle = Vector3.zero;
	public Vector3 containerHoverAngle = Vector3.zero;
    protected Quaternion containerNormalRotation = Quaternion.identity;
    protected Quaternion containerHoverRotation = Quaternion.identity;
    protected Quaternion containerTargetRotation = Quaternion.identity;
    protected float containerRotTimer = -1f;


    public GameObject containerPositionalObject;
    protected Transform containerPositionalTransform = null;
    public Vector3 containerNormalPos = Vector3.zero;
	public Vector3 containerHoverPos = Vector3.zero;
	public Vector3 containerTargetPos = Vector3.zero;
	protected float containerTransTimer = -1f;

    protected HighlightShader[] highlightShaders;

    TextOverlay[] textItems;

	virtual public void Awake() 
    {       
        myTransform = transform;
		_localScale = myTransform.localScale;
		_isEnabled = true;

		highlightShaders = gameObject.GetComponentsInChildren<HighlightShader>();
		_scaleHighlight = highlightShaders == null;

        textItems = gameObject.GetComponentsInChildren<TextOverlay>();

        containerNormalRotation = Quaternion.Euler (containerNormalAngle);
        containerHoverRotation = Quaternion.Euler (containerHoverAngle);

        if( containerObject != null ) {
            containerTransform = containerObject.transform;
        }
        if( containerPositionalObject != null ) {
            containerPositionalTransform = containerPositionalObject.transform;

            containerTargetPos = containerNormalPos;
            containerPositionalTransform.localPosition = containerNormalPos;
//            Debug.Log("AWAKE " + gameObject.name + " pos update " + containerPositionalTransform.localPosition);
        }
    }

    public virtual void OnEnable()
    {
        if( myTransform == null ) {
            Awake();
        }

        bool highlighted = FloatingMenuManager.Instance.CheckForSelection() == this.gameObject;
        Highlight( highlighted, true );
        SetContainerhighlightShaders(true);
    }
	
    public virtual void CheckHighlightRotation( bool force_off = false ) {}

	public virtual void Highlight(bool set, bool on_enable = false) {
//		Debug.Log("HIGHLIGHT HIGHLIGHT HIGHLIGHT HIGHLIGHT HIGHLIGHT HIGHLIGHT HIGHLIGHT ");
		//can't highlight disabled buttons
		if (!_isEnabled)
			return;

		bool set_rotate = false;
		bool set_translate = false;

		if (containerObject != null && _toRotate)
        {
			set_rotate = true;
            if (set && !_highlightedRotation) 
            {
//				Debug.Log("ROTATE TRIGGER START");
                containerTargetRotation = containerHoverRotation;
                containerRotTimer = 0f;
            }
            else 
            {   
//				Debug.Log("rotate trigger stop");
                containerTargetRotation = containerNormalRotation;
                containerRotTimer = 0f;
            }
        }

		if (containerObject != null && _toTranslate)
		{
			set_translate = true;
			if (set && !_highlightedTranslation) 
			{		
//				Debug.Log("ZOOM TRIGGER START");
				containerTargetPos = containerHoverPos;
				containerTransTimer = 0f;
			}
			else if( !MathfExt.Approx( containerNormalPos, containerTargetPos, 0.001f ) )
			{   
//				Debug.Log("ZOOM TRIGGER stoooooooop");
				containerTargetPos = containerNormalPos;
				containerTransTimer = 0f;
			}
		}

        if( on_enable ) {
            if( containerTransform != null ) {
                containerTransform.localRotation = containerTargetRotation;
            }
            if( containerPositionalTransform != null ) {
                containerPositionalTransform.localPosition = containerTargetPos;
            }
            containerTransTimer = -1f;
            containerRotTimer = -1f;
        }

		if (set_rotate) {
			_highlightedRotation = set;
			//if (set == true) _highlightedTranslation = set;
		}
		else if (set_translate) {
			_highlightedTranslation = set;
		}


		if (_toRotate){
	        if (activeNormal != null)
	        {
				activeNormal.SetActive(!_highlightedRotation);
	        }

	        if (activeHover != null)
	        {
				activeHover.SetActive(_highlightedRotation);
	        }

	        SetContainerhighlightShaders(false);
		}
	}

	protected void UpdateContainerPosition()
	{
		//Debug.Log("ZOOM TRIGGER UPDATE POSITION - START");
		if (containerPositionalObject == null ||
		    containerTransTimer < -0f)
		{
			return;
		}
		
		const float CONTAINER_TRANS_TIME = 0.35f;
		containerTransTimer += Time.deltaTime;
		
		float blend = 1f;
		//Debug.Log("ZOOM TRIGGER UPDATE POSITION - STEP2");

		if (containerTransTimer < CONTAINER_TRANS_TIME)
		{
			blend = containerTransTimer/0.35f;
		}
		else
		{
			containerTransTimer = -1f;
		}
		
		Vector3 oldPos;
		if (MathfExt.Approx(containerTargetPos, containerHoverPos, 0.001f))
		{
			oldPos = containerNormalPos;
		}
		else
		{
//            Debug.Log("HOEVERING?????");
			oldPos = containerHoverPos;
		}
		
        containerPositionalTransform.localPosition = Vector3.Lerp(oldPos, containerTargetPos, blend);
//        Debug.Log(gameObject.name + " pos update " + containerPositionalTransform.localPosition + " blend " + blend);
	}


	protected void UpdateContainerRotation()
    {
		//Debug.Log("ZOOM TRIGGER UPDATE ROTATION - START");

        if (containerObject == null ||
            containerRotTimer < -0f)
        {
            return;
        }

        const float CONTAINER_ROTATE_TIME = 0.35f;
        containerRotTimer += Time.deltaTime;

        float blend = 1f;
		//Debug.Log("ZOOM TRIGGER UPDATE ROTATION - STEP2");
        if (containerRotTimer < CONTAINER_ROTATE_TIME)
        {
            blend = containerRotTimer/0.35f;
        }
        else
        {
            if (activeNormal != null)
            {
                activeNormal.SetActive(!_highlightedRotation);
            }
            
            if (activeHover != null)
            {
                activeHover.SetActive(_highlightedRotation);
            }

            containerRotTimer = -1f;
        }

        Quaternion oldRot;
        if (containerTargetRotation == containerHoverRotation)
        {
            oldRot = containerNormalRotation;
        }
        else
        {
            oldRot = containerHoverRotation;
        }

        containerTransform.localRotation = Quaternion.Lerp(oldRot, containerTargetRotation, blend);
		//Debug.Log("ZOOM TRIGGER UPDATE ROTATION");
    }

	protected void ButtonInput() {

        CheckHighlightRotation();
       	UpdateContainerRotation();
		UpdateContainerPosition();


		_inputSelect = false;
		
		if (Input.GetKeyUp(KeyCode.Space))
			_inputSelect = true;
		
		//if (Input.GetKeyUp(KeyCode.JoystickButton0))
		if (Input.GetButtonUp("Interact"))
			_inputSelect = true;
		
		#if UNITY_ANDROID && !UNITY_EDITOR
		if (Input.GetKeyUp(KeyCode.Mouse0))
			_inputSelect = true;
		#endif
	}

	public virtual bool OnClick() {
		
		//we're in fade--already selected
		if (!_highlightedRotation && _toRotate)
			return false;
		
		if (!_isEnabled)
			return false;
		
		if (clickFunction != null)
			clickFunction();

        PlayClickSound();

		return true;
	}

    protected void PlayClickSound()
    {
        if (clickSound != null)
        {
            AudioManager.Instance.PlayOneShot(clickSound);
        }
        else
        {
            AudioManager.Instance.PlayOneShot(FloatingMenuManager.Instance.acMenuSelect);
        }
    }

	protected bool ProcessInput() {

		return (_inputSelect);
	}

    protected void SetContainerhighlightShaders(bool bForce)
    {
        if (highlightShaders != null)
        {
            if (_toRotate && _highlightedRotation)
            {
				foreach(HighlightShader hs in highlightShaders){
                	hs.Highlight(1.0f, bForce);
				}
            }
            else
			{
				foreach(HighlightShader hs in highlightShaders){
					hs.Highlight(0f, bForce);
				}
            }
        }

        if (textItems != null)
        {
            for (int i =0; i<textItems.Length; ++i)
            {
				textItems[i].Highlight(_highlightedRotation, bForce);
            }
        }

        if (containerObject != null &&
            bForce)
        {
            containerTransform.localRotation = containerTargetRotation;
            containerRotTimer = -1f;
        }
    }
    
    public bool isEnabled { get{return(_isEnabled);} set{_isEnabled = value;}}
}
