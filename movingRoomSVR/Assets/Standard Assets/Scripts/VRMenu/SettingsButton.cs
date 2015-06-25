using UnityEngine;
using System.Collections;
using System;
using Holoville.HOTween;

public class SettingsButton : VRButton {

	int _index;
	bool _on;
	
	public string prefName = UserManager.USER_EDUCATION_MODE;
	public int[] prefValues;
	
	public Color[] valueColors;
	public string[] valueNames;

	bool _toggle;
	
	override public void Awake() {

		_localScale = this.transform.localScale;
		_isEnabled = true;

		_index = 0;
		
		if (prefValues == null) {
			prefValues = new int[1];
			prefValues[0] = 0;
		}

		if (valueColors == null) {
			valueColors = new Color[1];
			valueColors[0] = Color.red;
		}

		if (valueNames == null) {
			valueNames = new string[1];
			valueNames[0] = "Uh.";
		}
	}
	
	
	// Use this for initialization
	void Start () {
		
		int edu = PlayerPrefs.GetInt(prefName, prefValues[0]);
		int idx = FindValue(edu);
		
		SetValueByIndex(idx);

		Highlight(false);
	}
	
	void SetValueByIndex(int idx) {
		
		PlayerPrefs.SetInt(prefName, prefValues[idx]);
		GetComponent<Renderer>().material.SetColor("_Color", valueColors[idx % valueColors.Length]);
		
	}

	public void Set() {

		_index++;
		_index %= prefValues.Length;

		SetValueByIndex(_index);

        if (prefName == "controltype")
        {
            switch (prefValues[_index])
            {
            case 0:
            default:
                OculusFPSCameraMode.singleton.TouchPadMove = OculusFPSCameraMode.TouchPadMoveMode.ToggleTap;
                break;
            case 1:
                OculusFPSCameraMode.singleton.TouchPadMove = OculusFPSCameraMode.TouchPadMoveMode.Hold;
                break;
            }
        }
	}
	
	int FindValue(int val) {
		
		for (int i = 0; i < prefValues.Length; i++) {
			
			if (prefValues[i] == val)
				return i;
		}

		return -1;
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

        //we're in fade--already selected
        if (!_highlighted)
            return false;
        
        if (!_isEnabled)
            return false;
        
        if (clickFunction != null)
            clickFunction();
                
        Set();

        PlayClickSound();

		return true;
	}
}

