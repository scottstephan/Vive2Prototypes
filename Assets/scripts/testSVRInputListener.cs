using UnityEngine;
using System.Collections;

public class testSVRInputListener : MonoBehaviour {
    private Color originColor;
    private Rigidbody thisRigidbody;
    private controllerListener.svrController activatingController;
    void Start()
    {
        thisRigidbody = gameObject.GetComponent<Rigidbody>();
        originColor = gameObject.GetComponent<MeshRenderer>().material.color;
            //Instance the mat to avoid overwriting the original material. 
        Material cloneMat = gameObject.GetComponent<MeshRenderer>().material;
        gameObject.GetComponent<MeshRenderer>().material = cloneMat; 
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
        objectIsDropped();
        activatingController = null;
    }

    private void objectIsPickedUp()
    {
        gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

        thisRigidbody.isKinematic = true;
        gameObject.transform.parent = activatingController.controllerObject.transform;
    }

    private void objectIsDropped()
    {
        gameObject.transform.parent = null;

        Vector3 controllerVelocity = activatingController.curVelocity;
        Debug.Log("At button up, svrControllers velocity is: " + controllerVelocity);

        thisRigidbody.isKinematic = false;
        thisRigidbody.velocity = controllerVelocity; 

        gameObject.GetComponent<MeshRenderer>().material.color = originColor;
        activatingController = null;
    }
}
