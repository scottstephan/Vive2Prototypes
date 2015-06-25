using UnityEngine;
using System.Collections;

public class BackToMenuButton : VRButton {

	// Use this for initialization
	void Start () {
	
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

		//we need a reliable way to detect if we are in the boot menu
		//if (SphereManager.destinationName == "sphere")
		if (SphereManager.IsInBoot())
			FloatingMenuManager.HideMenu(true);
		else
        {
            FloatingMenuManager.HideMenu(false);
            FloatingMenuManager.ShowMenu(FloatingMenuManager.MenuType.Travel);
        }

		FloatingMenuManager.ShowSelectionDot();
		
		return true;
	}
}
