using UnityEngine;
using System.Collections;

public class controllerFinder : MonoBehaviour {
    public int leftController = -1;
    public int rightController = -1;
    public GameObject leftControllerObject;
    public GameObject rightControllerObject;
    
	// Use this for initialization
	void Start () {
	    //TO-DO:
        //Write the finals into a static method
        //Write a controller assignment f(x)
	}
	
	// Update is called once per frame
	void Update () {
        if (leftController == -1) { 
            leftController = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
            Debug.Log("Left controller is: " + leftController);
            if (leftController != -1 || leftController != 0)
            {
                leftControllerObject = GameObject.Find("Device" + leftController.ToString());
                leftControllerObject.AddComponent<BoxCollider>();
                leftControllerObject.AddComponent<controllerIndexKeeper>().controllerIndex = leftController;
                leftControllerObject.tag = "controller";

             //   leftControllerObject.AddComponent<controllerCollisionManager>();
            }
            
        }
        if (rightController == -1)
        {
            rightController = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
            if (rightController != -1 || rightController != 0)
            {
                rightControllerObject = GameObject.Find("Device" + rightController.ToString());
                rightControllerObject.AddComponent<BoxCollider>();
                rightControllerObject.AddComponent<controllerIndexKeeper>().controllerIndex = rightController;
                rightControllerObject.tag = "controller";
             //   rightControllerObject.AddComponent<controllerCollisionManager>();
            }
            
        }
	}

}



