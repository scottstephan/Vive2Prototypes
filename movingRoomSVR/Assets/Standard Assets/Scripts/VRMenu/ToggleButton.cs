using UnityEngine;
using System.Collections;
using System;
using Holoville.HOTween;

public class ToggleButton : VRButton {
	
	bool _selected;
	bool _on;

	TextMesh _offText;
	TextMesh _onText;

	Color _onColor;
	Color _offColor;

	public ToggleButton syncedButton;

	public bool visibleUpdate = false;
	
	public override void Awake() {
        base.Awake();

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
		
		int val = PlayerPrefs.GetInt(UserManager.USER_EDUCATION_MODE, 1);
		
		if (val == 0)
			Toggle(false);
		else
			Toggle(true);
	}

	public void Toggle(bool on) {

		_on = on;

		SetToggleState();
	}

	void SetToggleState() {

		//change color

		if (_on) {
			//renderer.material.SetColor("_Color", Color.green);

			_onText.color = _onColor;
			_offText.color = _offColor;
            
/*	EDUCATIONAL MODE IS DISABLED.
 * PlayerPrefs.SetInt(UserManager.USER_EDUCATION_MODE, 1);
            if( App.UserManager != null ) {
                App.UserManager.educationalMode = 1;
            }*/
		}
		else {
			//renderer.material.SetColor("_Color", Color.red);
            
			_onText.color = _offColor;
			_offText.color = _onColor;

			PlayerPrefs.SetInt(UserManager.USER_EDUCATION_MODE, 0);
            if( App.UserManager != null ) {
                App.UserManager.educationalMode = 0;
            }
		}

		if (syncedButton && (syncedButton.toggleState != _on))
			syncedButton.Toggle(_on);
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
		if(base.OnClick ())
			Toggle();
		else
			return false;

		return true;
	}

	public bool toggleState {get{return(_on);} set{Toggle(value);}}
}

