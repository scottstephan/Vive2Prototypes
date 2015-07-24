using UnityEngine;
using System.Collections;
//This is the most basic version of an SVR interactable object
public class testSVRInputListener : svrInteractableBase {
    
    void Start()
    {
		base.Start ();
    }

	public override void controllerInputTriggerDown()
    {
		base.controllerInputTriggerDown ();

		objectIsPickedUp(); 
    }

	public override void controllerInputTriggerUp()
    {
		base.controllerInputTriggerUp ();

        objectIsDropped();
    }

}
