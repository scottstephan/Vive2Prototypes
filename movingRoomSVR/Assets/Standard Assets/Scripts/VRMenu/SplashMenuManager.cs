using UnityEngine;
using System.Collections;

public class SplashMenuManager : MonoBehaviour {

	bool _inputSelect;
	bool _inputDebug;

	bool _plugged = false;

//	Quaternion _startCamAngle;
//	Transform _clt;

	void Awake() 
    {
	}

	// Use this for initialization
	void Start () 
    {	
//		_clt = ((GameObject)GameObject.Find("CameraLeft")).transform;
//		_startCamAngle = _clt.rotation;

        CameraManager.IntroCameraExit();
        _plugged = AreHeadphonesAttached();
        FloatingMenuManager.HideSelector();

        if (!_plugged)
        {
            App.MetricsManager.Track("headphone_prompt");
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (AppBase.Instance.RunningAsPreview())
        {
            // only hide don't show travel menu when running single level
            FloatingMenuManager.HideMenu(false, false, true);
            return;
        }

/*        if (_plugged)
        {
            OVRDevice.ResetOrientation();
        }*/

		ButtonInput();

		if (ProcessInput()) 
        {
            if (AreHeadphonesAttached())
            {                 
                App.MetricsManager.Track("headphone_inserted");
            }

            Exit();
            return;
		}

/*	Dont autoexit.
 * if (_plugged)
        {   
            Exit();
        }*/
	}

    void Exit()
    {
        bool bFirstRun = FloatingMenuManager.IsFirstRun();
        FloatingMenuManager.HideMenu(true, false, true);

        if (bFirstRun)
        {
            FloatingMenuManager.ShowMenu(FloatingMenuManager.MenuType.IntroFirstTime);
        }
        else
        {
            FloatingMenuManager.ShowMenu(FloatingMenuManager.MenuType.Travel, _plugged);
        }

        if (AreHeadphonesAttached())
        {                 
            App.MetricsManager.Track("headphone_active");
        }
    }

	void ButtonInput() {

		//none of this is actually used

		_inputSelect = false;
		_inputDebug = false;

		if (Input.GetKeyUp(KeyCode.JoystickButton3) || (Input.GetKeyUp(KeyCode.Q)))
			_inputDebug = true;

		if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.JoystickButton0) || Input.GetKeyUp(KeyCode.JoystickButton1) || Input.GetKeyUp(KeyCode.JoystickButton2))
		    _inputSelect = true;
	}
	
	bool ProcessInput() {

		if (_inputSelect)
			return true;

		if (_inputDebug)
			_plugged = true;

		return false;
	}


    bool AreHeadphonesAttached()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            return AndroidUtil.AreHeadphonesConnected();
        }
        catch
        {
            return false;
        }
#else
        return false;
#endif
    }
}
