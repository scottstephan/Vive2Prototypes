using UnityEngine;
using System.Collections;
using System;
using Holoville.HOTween;

public class SettingsToggleButton : VRButton {
	
	bool _selected;
	bool _on;
	
	TextMesh _offText;
	TextMesh _onText;
	
	Color _onColor;
	Color _offColor;

	public string prefName;
    public int defaultValue = 0;
	public Vector3 settingsIconScaleValue;

	public Action toggleCallback;

	public bool visibleUpdate = false;

	public GameObject checkOn;
	public GameObject boxOn;
	public GameObject boxOff;

	public override void Awake() {
        base.Awake ();

		_localScale = this.transform.localScale;
		_isEnabled = true;

		Transform txt = this.transform.FindChild("Side_Toggle");

		_offText = txt.FindChild("Text_Off").GetComponent<TextMesh>();
		_onText = txt.FindChild("Text_On").GetComponent<TextMesh>();
		
		_onColor = _onText.color;
		_offColor = _offText.color;

		_scaleHighlight = false;
	}

	// Use this for initialization
	void Start () {
		
		DetectPref();
		
		Highlight(false);
	}
	
	// Update is called once per frame
	void Update () {
		
		if (!_isEnabled)
			return;
		
		ButtonInput();
		
		if (ProcessInput())
			OnClick();
	}

	public void DetectPref() {

        int val = PlayerPrefs.GetInt(prefName, defaultValue);
		
		if (val == 0)
			Toggle(false);
		else
			Toggle(true);
	}
	
	public void Toggle(bool on) {
		
		_on = on;
		
		SetToggleState();

		if (toggleCallback != null)
			toggleCallback();
	}
	
	void SetToggleState() {
		//Debug.Log("SetToggleState " + _on);
		boxOn.SetActive(_on);
		boxOff.SetActive(!_on);
		if (_on) {
			_onText.color = _onColor;
			_offText.color = _offColor;
			if(checkOn != null){
				HOTween.To(checkOn.transform, 0.3f, "localScale", settingsIconScaleValue );
			}
			
			PlayerPrefs.SetInt(prefName, 1);
		}
		else {
			_onText.color = _offColor;
			_offText.color = _onColor;
			if(checkOn != null){
				HOTween.To(checkOn.transform, 0.3f, "localScale", settingsIconScaleValue * 0.001f);
			}
			
			PlayerPrefs.SetInt(prefName, 0);
		}

		//we should probalby have a static global to refresh 
		//all settings etc.
		AudioManager.DetectSettings();
	}

	void OnBecameVisible() {

		if (visibleUpdate)
			DetectPref();

	}
	
	public void Toggle() {
		
		if (_on)
			_on = false;
		else
			_on = true;
		
		SetToggleState();
	}
	
	public override bool OnClick ()
	{
        //we're in fade--already selected
        if (!_highlightedRotation)
            return false;
        
        if (!_isEnabled)
            return false;
        
        if (clickFunction != null)
            clickFunction();
        
        Toggle();

        PlayClickSound();
		
		return true;
	}
	
	public bool toggleState {get{return(_on);} set{Toggle(value);}}
}

