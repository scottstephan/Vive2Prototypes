using UnityEngine;
using System.Collections;
//SHOULD REQUIRE RBODY etc
public class svrInteractableBase : MonoBehaviour {

	private controllerListener.svrController activatingController;
	private Rigidbody thisRigidbody;
	private Color originColor;

	// Use this for initialization
	public virtual void Start () {
		thisRigidbody = gameObject.GetComponent<Rigidbody>();
		originColor = gameObject.GetComponent<MeshRenderer>().material.color;
		//Instance the mat to avoid overwriting the original material. 
		Material cloneMat = gameObject.GetComponent<MeshRenderer>().material;
		gameObject.GetComponent<MeshRenderer>().material = cloneMat; 
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void svrControllerDown(controllerListener.svrController controllerThatBroadcasted)
	{
		Debug.Log(gameObject.name + "has heard the svrDown Broadcast");
		activatingController = controllerThatBroadcasted;
		Debug.Log ("It was touched by cotnroller" + activatingController.index);
		controllerInputTriggerDown();

	}
	
	private void svrControllerUp(controllerListener.svrController controllerThatBroadcasted)
	{
		Debug.Log(gameObject.name + "has heard the svr Up Broadcast");
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
