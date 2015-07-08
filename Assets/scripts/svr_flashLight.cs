using UnityEngine;
using System.Collections;

public class svr_flashLight : MonoBehaviour {
    public Light flashLightBeam;
	// Use this for initialization
	void Start () {
        flashLightBeam.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void objectIsBeingHeld()
    {
        flashLightBeam.enabled = true;
    }

     public void objectIsReleased()
    {
        flashLightBeam.enabled = false;
    }
}
