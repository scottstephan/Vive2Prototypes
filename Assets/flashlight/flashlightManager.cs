using UnityEngine;
using System.Collections;

public class flashlightManager : MonoBehaviour {
    public int deviceIndex;
    public GameObject flashlight;
    private bool flashlightState = false;
	// Use this for initialization
	void Start () {
      //  deviceIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
	}
	
	// Update is called once per frame
	void Update () {
	    if(SteamVR_Controller.Input(deviceIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            Debug.Log("SVR Controller trigger down");
            flashlightState = !flashlightState;
            flashlight.SetActive(flashlightState);
            SteamVR_Controller.Input(deviceIndex).TriggerHapticPulse(100);
        }
	}
}
