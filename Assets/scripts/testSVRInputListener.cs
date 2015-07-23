using UnityEngine;
using System.Collections;
//This is the most basic version of an SVR interactable object
public class testSVRInputListener : svrInteractableBase {
    private Color originColor;
    private Rigidbody thisRigidbody;
   
    void Start()
    {
        thisRigidbody = gameObject.GetComponent<Rigidbody>();
        originColor = gameObject.GetComponent<MeshRenderer>().material.color;
            //Instance the mat to avoid overwriting the original material. 
        Material cloneMat = gameObject.GetComponent<MeshRenderer>().material;
        gameObject.GetComponent<MeshRenderer>().material = cloneMat; 
    }

	public override void controllerInputTriggerDown()
    {
		base.controllerInputTriggerDown ();
		Debug.Log(gameObject.name + "has heard the svrDown Broadcast");

		objectIsPickedUp(); 
    }

	public override void controllerInputTriggerUp()
    {
		base.controllerInputTriggerUp ();
		Debug.Log(gameObject.name + "has heard the svUp Broadcast");

        objectIsDropped();
    }

}
