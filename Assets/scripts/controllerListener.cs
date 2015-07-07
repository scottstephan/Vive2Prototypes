using UnityEngine;
using System.Collections.Generic;
using Valve.VR;
//TO-DO: Write a L/R assignation via SteamVR_Controller(leftmost) etc etc.
public class controllerListener : MonoBehaviour {

    public class svrController //Yes, SteamVR has one of these. This is easier and quicker for 90% of stuff we do with controllers.
    {
       public GameObject controllerObject;
       public int index;
       public bool isSet = false;
       public Vector3 curVelocity;
    }

    public static svrController controller1 = new svrController();
    public static svrController controller2 = new svrController();
    public static List<svrController> svrControllers = new List<svrController>();

    public bool addCollidersAndTagsToControllers = false; //If you're planning on using controllers as collision volumes, check this. It'll auto-add a box collider and tag the controller.
	// Use this for initialization
    
	void Start () {
	
	}

	private void OnDeviceConnected(params object[] args) //Listen to SteamVR for device connection messages
	{
		var index = (int)args[0];
		
		var vr = SteamVR.instance;
		if (vr.hmd.GetTrackedDeviceClass((uint)index) != TrackedDeviceClass.Controller)
			return;
		
		var connected = (bool)args[1];
		if (connected)
		{
			Debug.Log(string.Format("Controller {0} connected.", index));
            writeControllerRef(index);
		}
		else
		{
			Debug.Log(string.Format("Controller {0} disconnected.", index));

		}
	}
	    //std add/remove delegate listening
	void OnEnable()
	{
		SteamVR_Utils.Event.Listen("device_connected", OnDeviceConnected);
	}
	
	void OnDisable()
	{
		SteamVR_Utils.Event.Remove("device_connected", OnDeviceConnected);
	}

    private void writeControllerRef(int index) //Create a ref to the controller in the List<>
    {
             // GameObject Controller = controller1 == null ? controller1 : controller2;
        GameObject Controller = GameObject.Find("Device" + index);
        svrController tempController = null;
        Debug.Log("Controller with index" + index + " assigned to gameobject " + Controller.name);
      
        if (!controller1.isSet)
        {
            controller1.isSet = true;
            controller1.controllerObject = Controller;
            controller1.index = index;
            svrControllers.Add(controller1);
            tempController = controller1;
        }
        else if(!controller2.isSet)
        {
            controller2.isSet = true;
            controller2 .controllerObject= Controller;
            controller2.index = index;
            svrControllers.Add(controller2);
            tempController = controller2;
        }
       
        if (addCollidersAndTagsToControllers) addCollisionSetupToController(tempController);
    }

    private void eraseControllerRef(int index)
    {
        
    }

    private void addCollisionSetupToController(svrController controller)
    {
        controller.controllerObject.AddComponent<controllerCollisionManager>();
        controller.controllerObject.AddComponent<BoxCollider>();
        controller.controllerObject.AddComponent<Rigidbody>();

        controller.controllerObject.GetComponent<controllerCollisionManager>().idController(controller);
        controller.controllerObject.tag = "controller";
    }

    public static int returnIndexByName(string controllerName) //Returns the controllers index when provided with the name of the gameobject
    {

        for (int i = 0; i < svrControllers.Count; i++) 
        {
            if (controllerName == svrControllers[i].controllerObject.name)
            {
              return svrControllers[i].index;
            }
        }

        return -1;
    }

	public static svrController returnSVRObjectByName(string controllerName){
		for (int i = 0; i < svrControllers.Count; i++) 
		{
			if (controllerName == svrControllers[i].controllerObject.name)
			{
				return svrControllers[i];
			}
		}
		
		return null;
	}

    public static void readControllerList() //Lists all the controllerss
    {
        Debug.Log("Reading controller list");
        for (int i = 0; i < svrControllers.Count; i++) 
        {
            Debug.Log("svrControllers[" + i + "] is named: " + svrControllers[i].controllerObject.name);
        }
    }
}
