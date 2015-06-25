using UnityEngine;
using System.Collections;
using System;
using Holoville.HOTween;

public class SelectableModeButton : VRButton {

    public GameObject activeNormalSelected;
    public GameObject activeHoverSelected;

	public string prefName = UserManager.USER_EDUCATION_MODE;
	public int prefValue;
    public int defaultValue;
    public bool bootMenuOnly = true;

    bool _selected;

    public SelectableModeButton[] unselectButtons;
			
	// Use this for initialization
	void Start () {
		
		Highlight(false);

        int pref = PlayerPrefs.GetInt(prefName, defaultValue);
        _selected = pref != prefValue;
        Select (pref == prefValue);
	}
	
    public override void OnEnable()
    {
        base.OnEnable();

        if (!FloatingMenuManager.isBootMenu)
        {
            gameObject.SetActive(false);
        }
    }

	void SetValueByIndex(int idx) {
		
	}

	// Update is called once per frame
	void Update () {
		
		if (!_isEnabled)
			return;
		
		ButtonInput();

		if (ProcessInput())
			OnClick();
	}

	public override bool OnClick() {
		Debug.Log("SELECTABLE MODE BUTTON HAS BEEN CLICKED");
		
		//we're in fade--already selected
		if (!_highlighted)
			return false;
		
		if (!_isEnabled)
			return false;
		
		if (clickFunction != null)
			clickFunction();
		
        Select(true);

		return true;
	}

    public override void Highlight (bool set, bool on_enable = false)
    {
        //can't highlight disabled buttons
        if (!_isEnabled)
            return;
        
        if (_scaleHighlight) {
            if (set && !_highlighted)
                _tween = HOTween.To(this.transform, .5f, new TweenParms().Prop("localScale", new Vector3(_localScale.x * 1.25f, _localScale.y * 1.25f, _localScale.z * 1.25f)));
            else { //tween down to 1
                
                if (_tween != null)
                    _tween.Kill();
                
                _tween = HOTween.To(this.transform, .5f, new TweenParms().Prop("localScale", _localScale));
            }
        }
        
        _highlighted = set;
        Select (_selected);
    }

    void SetPref()
    {
        if (string.IsNullOrEmpty(prefName))
        {
            return;
        }

        PlayerPrefs.SetInt(prefName, prefValue);

        if (prefName == UserManager.USER_EDUCATION_MODE)
        {
            App.UserManager.educationalMode = prefValue;
        }
    }

    public void Select(bool selected)
    {
        if (_selected != selected &&
            selected)
        {
            if (unselectButtons != null)
            {
                for (int i=0; i<unselectButtons.Length; ++i)
                {
                    unselectButtons[i].Select (false);
                }
            }

            SetPref();
        }

        _selected = selected;

        if (_selected)
        {
            if (activeNormalSelected != null)
            {
                activeNormalSelected.SetActive(!_highlighted);
            }
            
            if (activeHoverSelected != null)
            {
                activeHoverSelected.SetActive(_highlighted);
            }

            if (activeNormal != null)
            {
                activeNormal.SetActive(false);
            }
            
            if (activeHover != null)
            {
                activeHover.SetActive(false);
            }
        }
        else
        {
            if (activeNormalSelected != null)
            {
                activeNormalSelected.SetActive(false);
            }
            
            if (activeHoverSelected != null)
            {
                activeHoverSelected.SetActive(false);
            }
            
            if (activeNormal != null)
            {
                activeNormal.SetActive(!_highlighted);
            }
            
            if (activeHover != null)
            {
                activeHover.SetActive(_highlighted);
            }
        }
    }
}

