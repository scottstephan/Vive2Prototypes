using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (BoxCollider))]

public class listenForSVCCollision : MonoBehaviour {

	public delegate void svrValidTriggerPull();
	public static event svrValidTriggerPull svrTriggerPulled_Valid;

	private GameObject lastTouchedByController;
	private int lastTouchedIndex;
	private bool isBeingTouched = false;
	private bool isCurrentlyActivatedByController = false;

	public static List<controllerListener.svrController> validTouchingControllers = new List<controllerListener.svrController>();


	// Use this for initialization
	void Start () {
		gameObject.GetComponent<BoxCollider> ().isTrigger = true;
		gameObject.GetComponent<Rigidbody> ().isKinematic = true; //Not always true. True for now. 
	}

	// Update is called once per frame
	void Update () {
		if (!isCurrentlyActivatedByController)
		{

			for (int i = 0; i < validTouchingControllers.Count; i++) 
			{
				if (SteamVR_Controller.Input(validTouchingControllers[i].index).GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
				{
					//Broadcast down object from here
				}
			}

		}
		
		if (isCurrentlyActivatedByController)
		{
			for (int i = 0; i < validTouchingControllers.Count; i++) 
			{
				if (SteamVR_Controller.Input(validTouchingControllers[i].index).GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
				{
					//Broadcast down object from here
				}
			}
		}
	}
	
	void OnTriggerEnter(Collider col)  
	{
		if (col.gameObject.tag == "controller")
		{
			Debug.Log("I'm being touched by index " + controllerListener.returnIndexByName(col.gameObject.name));
			validTouchingControllers.Add(controllerListener.returnSVRObjectByName(col.gameObject.name));
			gameObject.GetComponent<MeshRenderer>().material.color = Color.red; 
		
		}
	}
	
	void OnTriggerExit(Collider col)
	{
		if (col.gameObject.tag == "controller") 
		{
			validTouchingControllers.Remove (controllerListener.returnSVRObjectByName (col.gameObject.name));
			gameObject.GetComponent<MeshRenderer> ().material.color = Color.green;
		}
	}
}
