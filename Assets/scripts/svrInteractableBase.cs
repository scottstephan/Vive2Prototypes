using UnityEngine;
using System.Collections;

public class svrInteractableBase : MonoBehaviour {
	private controllerListener.svrController activatingController;
	private Rigidbody thisRigidbody;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void svrControllerDown(controllerListener.svrController controllerThatBroadcasted)
	{
		Debug.Log(gameObject.name + "has heard the svrDown Broadcast");
		activatingController = controllerThatBroadcasted;
		controllerInputTriggerDown();

	}
	
	private void svrControllerUp(controllerListener.svrController controllerThatBroadcasted)
	{
		Debug.Log(gameObject.name + "has heard the svr Up Broadcast");
		activatingController = null;

		controllerInputTriggerUp();
	}

	public virtual void controllerInputTriggerDown(){

	}

	public virtual void controllerInputTriggerUp(){
		
	}
//BELOW THIS POINT: Common functions for SVR Interactable Objects
	public void objectIsPickedUp() 
	{
		//     gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
		//Maybe add a short delay for feeding?
		StartCoroutine("feedTransformDelay");
		thisRigidbody.isKinematic = true;
		gameObject.transform.parent = activatingController.controllerObject.transform;
	}
	
	public void objectIsDropped() 
	{
		gameObject.transform.parent = null;
		
		Vector3 controllerVelocity = activatingController.curVelocity;
		Debug.Log("At button up, svrControllers velocity is: " + controllerVelocity);
		
		thisRigidbody.isKinematic = false;
		thisRigidbody.velocity = controllerVelocity;
		
		//  gameObject.GetComponent<MeshRenderer>().material.color = originColor;
		activatingController = null;
	}
}
