using UnityEngine;
using System.Collections;

public class svr_birdFeeder : MonoBehaviour {
    private controllerListener.svrController activatingController;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void svrControllerDown(controllerListener.svrController controllerThatBroadcasted)
    {
        Debug.Log(gameObject.name + "has heard the svrDown Broadcast");
        activatingController = controllerThatBroadcasted;
        
    }

    public void svrControllerUp(controllerListener.svrController controllerThatBroadcasted)
    {
        Debug.Log(gameObject.name + "has heard the svr Up Broadcast");
       
        activatingController = null;
    }

    private void objectIsActivated()
    {

    }

    private void objectIsDeactivated()
    {

    }
}
