using UnityEngine;
using System.Collections;

/*POSSIBLE EDGE CASE WHERE IF YOU TOUCH 2 OBJECTS AT ONCE, IT DOWNS ONE AND UPS THE OTHER. Only allow controller trigger to take in one object at a time */

public class object_Pickupable: MonoBehaviour {
    public bool hasCustomBehaviorOnHold = false;
    public bool hasCustomeBehaviorOnDrop = false;

    private Color originColor;
    private Rigidbody thisRigidbody;
    private controllerListener.svrController activatingController;
    private bool isHeld = false;

    void Start()
    {
        if (!gameObject.GetComponent<Rigidbody>()) gameObject.AddComponent<Rigidbody>();
        gameObject.tag = "svrInteractableObject";
        thisRigidbody = gameObject.GetComponent<Rigidbody>();

        if (gameObject.GetComponent<MeshRenderer>())
        {
            originColor = gameObject.GetComponent<MeshRenderer>().material.color;
            //Instance the mat to avoid overwriting the original material. 
            Material cloneMat = gameObject.GetComponent<MeshRenderer>().material;
            gameObject.GetComponent<MeshRenderer>().material = cloneMat;
        }
    }

    public void svrControllerDown(controllerListener.svrController controllerThatBroadcasted)
    {
        Debug.Log(gameObject.name + "has heard the svrDown Broadcast");
        activatingController = controllerThatBroadcasted;
        objectIsPickedUp(); //Could be toggled on flag
    }

    public void svrControllerUp(controllerListener.svrController controllerThatBroadcasted)
    {
        Debug.Log(gameObject.name + "has heard the svr Up Broadcast");
        if (controllerThatBroadcasted.index == activatingController.index) //to avoid mis-fires from other controllers. Sometimes throws a null, which could also be a good indicator?j58
        {
            objectIsDropped();
            activatingController = null;
        }
     }

    private void objectIsPickedUp()
    {
        if (!isHeld)
        {
            isHeld = true;
            if (gameObject.GetComponent<MeshRenderer>()) gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

            thisRigidbody.isKinematic = true;
            gameObject.transform.parent = activatingController.controllerObject.transform;

            if (hasCustomBehaviorOnHold) gameObject.BroadcastMessage("objectIsBeingHeld");
        }
    }

    private void objectIsDropped()
    {
        if (isHeld)
        {
            isHeld = false;
            gameObject.transform.parent = null;

            Vector3 controllerVelocity = activatingController.curVelocity; //This occassionally throws an error and hangs the object. Why ??
            Debug.Log("At button up, svrControllers velocity is: " + controllerVelocity);

            thisRigidbody.isKinematic = false;
            thisRigidbody.velocity = controllerVelocity;

            if (gameObject.GetComponent<MeshRenderer>()) gameObject.GetComponent<MeshRenderer>().material.color = originColor;
            activatingController = null;

            if (hasCustomeBehaviorOnDrop) gameObject.BroadcastMessage("objectIsReleased");
        }
    }
}
