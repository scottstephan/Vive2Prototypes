using UnityEngine;
using System.Collections.Generic;
using Valve.VR;

public class controllerListener : MonoBehaviour {

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
}
