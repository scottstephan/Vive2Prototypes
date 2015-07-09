using UnityEngine;
using System.Collections;

/*THIS SHOULD JUST HANDLE LOGIC DIRECTLY. NO NEED FOR OBJ_ACTIVE CLASS??? */
public class object_Activatable: MonoBehaviour {
  
    private Color originColor;
    private Rigidbody thisRigidbody;
    private controllerListener.svrController activatingController;
    private bool isHeld = false;

    void Start()
    {
        if (!gameObject.GetComponent<Rigidbody>()) gameObject.AddComponent<Rigidbody>();
        gameObject.tag = "svrInteractableObject";
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
        objectIsActivated(); //Could be toggled on flag
    }

    public void svrControllerUp(controllerListener.svrController controllerThatBroadcasted)
    {
        Debug.Log(gameObject.name + "has heard the svr Up Broadcast");
        objectIsDeactivated();
        activatingController = null;
    }

    private void objectIsActivated()
    {
       
    }

    private void objectIsDeactivated()
    {
      
    }
}
