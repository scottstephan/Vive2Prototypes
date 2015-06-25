using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OculusIAPAndroid;
using OculusIAPAndroid.Model;

public class ConfirmQuitMenuManager : MonoBehaviour {

    public VRButton continueButton;
    public VRButton quitButton;

    float _oldCullDist;

#if UNITY_ANDROID
	// Use this for initialization
	void Start ()
    {
        continueButton.clickFunction = ContinueClicked;
        quitButton.clickFunction = QuitClicked;
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

    void ContinueClicked() 
    {
        FloatingMenuManager.HideMenu(false, true, true);
        FloatingMenuManager.ShowMenu(FloatingMenuManager.MenuType.Travel);
        FloatingMenuManager.ShowSelectionDot();
	}

	void QuitClicked() 
    {
        CameraManager.singleton.ovrController.ReturnToLauncher();
    }    
#endif
}
