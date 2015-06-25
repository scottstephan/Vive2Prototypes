using UnityEngine;
using System.Collections;

public class ActivateButton : VRButton {
    public GameObject deactivate;
    public GameObject activate;

    public Collider[] colliders = null;

    public override void Awake() 
	{       
        base.Awake();

		myTransform = transform;
		//_isEnabled = true;
		
//		highlightShaders = gameObject.GetComponentsInChildren<HighlightShader>();
//		_scaleHighlight = highlightShaders == null;
		
//		textItems = gameObject.GetComponentsInChildren<TextOverlay>();
		
		containerNormalRotation = Quaternion.Euler (containerNormalAngle);
		containerHoverRotation = Quaternion.Euler (containerHoverAngle);
        containerTargetRotation = containerNormalRotation;

		if( containerObject != null ) {
			containerTransform = containerObject.transform;
		}
		if( containerPositionalObject != null ) {
			containerPositionalTransform = containerPositionalObject.transform;
		}
	}


    public override void OnEnable ()
    {
        base.OnEnable ();
        _isEnabled = true;
        
        if (deactivate != null)
        {
            deactivate.SetActive(true);
        }
        
        if (activate != null)
        {
            activate.SetActive(false);
        }

        if( colliders != null && colliders.Length > 0 ) {
            for(int i = 0; i < colliders.Length; i++ ) {
                if( colliders[i] != null ) {
                    colliders[i].enabled = true;
                }
            }
        }
    }
    
    // Update is called once per frame
    void Update () {
        
        if (!_isEnabled)
            return;
        
        ButtonInput();
        
        if (ProcessInput())
            OnClick();
    }
    
    public override bool OnClick ()
    {

        if (!base.OnClick ())
            return false;
        
        if (deactivate != null)
        {
            deactivate.SetActive(false);
        }
        
        if (activate != null)
        {
            activate.SetActive(true);
        }

        if( colliders != null && colliders.Length > 0 ) {
            for(int i = 0; i < colliders.Length; i++ ) {
                if( colliders[i] != null ) {
                    colliders[i].enabled = false;
                }
            }
        }

        Highlight (false);
        _isEnabled = false;

        FloatingMenuManager.ShowSelectionDot();

        return true;
    }

	public override void CheckHighlightRotation( bool force_off = false ) {
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
			
		}
		else if( !result && _highlightedRotation ) {
			
			set = true;
			//            Debug.Log("Rotate SOME SHIT -- OFF");
			
			containerTargetRotation = containerNormalRotation;
			containerRotTimer = 0f;

		}
		
		if( set ) {
			_highlightedRotation = result;

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
			
            SetContainerhighlightShaders(false);
        }
	}
	
    public override void Highlight(bool set, bool on_enable = false) {
		
		//Debug.Log("FMI - HIGHLIGHT SOME SHIT");
		//Debug.Log("FMI - HIGHLIGHT = " + set);
		
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
		else { //tween down to 1
			
			
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
		
        CheckHighlightRotation( true );
        _highlightedTranslation = set;       
	}
	
	public bool highlighted {
		
		get {return(_highlightedRotation);}
	}

//	public bool selected {
//		get {return(_selected);}
//	}


}