using UnityEngine;
using System.Collections;
/*This script only handles one object at a time in order to prevent multiple pickups with one hand.
 * We'll probably need smarter filtering based on dist() in the future */

public class controllerCollisionManager : MonoBehaviour {
    public controllerListener.svrController thisController;
    private GameObject lastTouchedInteractableObject;
    private bool isTouchingActivatableObject = false;
    private bool isTouchingInteractableObject = false;
    private bool isUsingInteractableObject = false;
    private Vector3 cVel;
    private Vector3 oldPos;
    private bool objInCollider = false;
	// Use this for initialization
	void Start () {
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Rigidbody>().useGravity = false;
        gameObject.GetComponent<BoxCollider>().isTrigger = true; //Will need to resize to about multi-touch false psoitives
	}
	
	// Update is called once per frame
	void Update () { //this is getting busy. With so many cases, maybe check IP and switch?

	    if(isTouchingInteractableObject)
        {
            if (SteamVR_Controller.Input(thisController.index).GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                Debug.Log(thisController.index + "is pressing down");
                isUsingInteractableObject = true; //THIS PRESUMES A "HOLD" OPERATION NOT A "USE" OPERATION. WILL NEED TO SORT BASED ON OBJECT TYPE. This happens in ad-hoc way in the object code.
				Debug.Log("svrController is touching: " + lastTouchedInteractableObject.name + "and is broadcasting down");

                lastTouchedInteractableObject.BroadcastMessage("svrControllerDown",thisController);
                if (oldPos == Vector3.zero) oldPos = thisController.controllerObject.transform.position; //Involuntary for now. for cVel
            }
        }

        if(isUsingInteractableObject)
        {
            thisController.curVelocity = getControllerVelocity();

            if (SteamVR_Controller.Input(thisController.index).GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                Debug.Log(thisController.index + "is pressind up");

                isUsingInteractableObject = false;
                oldPos = Vector3.zero;
                lastTouchedInteractableObject.BroadcastMessage("svrControllerUp",thisController); //Should limit scope of broadcast
            }
        }
	}

    public void idController(controllerListener.svrController controller)
    {
        thisController = controller;
    }

    private Vector3 getControllerVelocity()
    {
        cVel = (thisController.controllerObject.transform.position - oldPos) / Time.deltaTime;
        oldPos = thisController.controllerObject.transform.position;
        return cVel;
    }

    void OnTriggerEnter(Collider col)
    {
        if(!objInCollider)
        {
            if (col.gameObject.tag == "svrInteractableObject")
            {
				Debug.Log("Controller has entered collider of" + col.gameObject.name);
                objInCollider = true;
                lastTouchedInteractableObject = col.gameObject;
                isTouchingInteractableObject = true;
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (lastTouchedInteractableObject != null && col.gameObject.name == lastTouchedInteractableObject.name) 
        {
			Debug.Log("Controller has exited collider of" + col.gameObject.name);
            objInCollider = false;
			isUsingInteractableObject = false;
			lastTouchedInteractableObject = null;
            isTouchingInteractableObject = false;
        }
    }
}
