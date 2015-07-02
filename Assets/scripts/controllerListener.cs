﻿using UnityEngine;
using System.Collections.Generic;
using Valve.VR;
//TO-DO: Write a L/R assignation via SteamVR_Controller(leftmost) etc etc.
public class controllerListener : MonoBehaviour {

    public class svrController
    {
       public GameObject controllerObject;
       public int index;
       public bool isSet = false;
    }

    public static svrController controller1 = new svrController();
    public static svrController controller2 = new svrController();
    public static List<svrController> svrControllers = new List<svrController>();

    public bool addCollidersAndTagsToControllers = false;
	// Use this for initialization

    
	void Start () {
	
	}

	private void OnDeviceConnected(params object[] args)
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
	
	void OnEnable()
	{
		SteamVR_Utils.Event.Listen("device_connected", OnDeviceConnected);
	}
	
	void OnDisable()
	{
		SteamVR_Utils.Event.Remove("device_connected", OnDeviceConnected);
	}

    private void writeControllerRef(int index)
    {
       // GameObject Controller = controller1 == null ? controller1 : controller2;
        GameObject Controller = GameObject.Find("Device" + index);
        Debug.Log("Controller with index" + index + " assigned to gameobject " + Controller.name);
      
        if (!controller1.isSet)
        {
            controller1.isSet = true;
            controller1.controllerObject = Controller;
            controller1.index = index;
            svrControllers.Add(controller1);
        //    readControllerList();
        }
        else if(!controller2.isSet)
        {
            controller2.isSet = true;
            controller2 .controllerObject= Controller;
            controller2.index = index;
            svrControllers.Add(controller2);
        //    readControllerList();
        }
        
        if (addCollidersAndTagsToControllers) addCollisionSetupToController(Controller);
    }

    private void eraseControllerRef(int index)
    {
        
    }

    private void addCollisionSetupToController(GameObject controller)
    {
        controller.AddComponent<BoxCollider>();
        controller.tag = "controller";
    }

    public static int returnIndexByName(string controllerName)
    {

        for (int i = 0; i < svrControllers.Count; i++) // Loop with for.
        {
          //  Debug.Log("In returnIndexByName loop. Looking for " + controllerName + " currently at " + svrControllers[i].controllerObject.name);
            if (controllerName == svrControllers[i].controllerObject.name)
            {
              return svrControllers[i].index;
            }
        }

        return -1;
    }

    public static void readControllerList()
    {
        Debug.Log("Reading controller list");
        for (int i = 0; i < svrControllers.Count; i++) // Loop with for.
        {
            Debug.Log("svrControllers[" + i + "] is named: " + svrControllers[i].controllerObject.name);
        }
    }
}
