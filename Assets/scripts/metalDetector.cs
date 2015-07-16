using UnityEngine;
using System.Collections;

public class metalDetector : MonoBehaviour {
    public Transform metalDetectorObject; //Should be R hand'
    public Transform[] treasureVectors;
    public float treasureDetectionThreshhold;
    public float dist = 0;
    public bool isOn = false;
    private int lastHeldIndex = -1;
  
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(isOn){}
            //updateDistanceFromTreasure();
	}

    public void turnOnMD()
    {
        Debug.Log("Metal detector is on");
        isOn  = true;
    }

    public void turnOffMD()
    {
        Debug.Log("Metal detector is off");

        isOn = false;
    }

    public void toggleMD(int index)
    {
        isOn = !isOn;
        lastHeldIndex = index;
        Debug.Log("isOn is: " + isOn);
    }

    /*
    public void svrControllerDown()
    {
        Debug.Log(gameObject.name + "has heard the svrDown Broadcast");
    }

    public void svrControllerUp()
    {
        Debug.Log(gameObject.name + "has heard the svr Up Broadcast");
    } */
}
