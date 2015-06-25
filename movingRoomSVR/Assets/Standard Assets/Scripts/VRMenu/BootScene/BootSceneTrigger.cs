using UnityEngine;
using System.Collections;

public class BootSceneTrigger : MonoBehaviour {

	//public CopyBootMenuToPopup cbp;

	bool _triggered = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision col) {

		//bring up menu!

		if (_triggered)
			return;

		_triggered = true;

		//FloatingMenuManager.ShowMenu(FloatingMenuManager.MenuType.Travel);
		//bringup selection dot

		CameraManager.IntroCameraExit();

		FloatingMenuManager.ShowSelectionDot();

		//cbp.CopyBootMenu(); //copy at this position so the popup version is at the same position
	}
}
