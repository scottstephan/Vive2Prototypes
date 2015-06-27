using UnityEngine;
using System.Collections;

public class metalDetector : MonoBehaviour {
    public Transform metalDetectorObject; //Should be R hand'
    public Transform[] treasureVectors;
    public float treasureDetectionThreshhold;
    public float dist = 0;
    public bool isOn = false;
    private int lastHeldIndex = -1;
    public GameObject radarSphere;
    private float flashTime = 1f;
    private bool radarIsFlashing = false;
	// Use this for initialization
	void Start () {
	   
	}
	
	// Update is called once per frame
	void Update () {
        if(isOn) updateDistanceFromTreasure();
	}

    private void updateDistanceFromTreasure()
    {
        foreach(Transform treasureVector in treasureVectors)
        {
            dist = Vector3.Distance(treasureVector.position, metalDetectorObject.position);
            if (dist < treasureDetectionThreshhold)
            {
                Debug.Log("Close to treasure!");
                flashTime = .1f + dist;
                if(!radarIsFlashing) StartCoroutine("flashRadar");
                SteamVR_Controller.Input(lastHeldIndex).TriggerHapticPulse(1000);
            }
        }
    

    }

    public void turnOnMD()
    {
        isOn  = true;
    }

    public void turnOffMD()
    {
        isOn = false;
    }

    public void toggleMD(int index)
    {
        isOn = !isOn;
        Debug.Log("Toggled MD to: " + isOn);
        if (isOn) radarSphere.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
        if (!isOn) radarSphere.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        lastHeldIndex = index;
    }

    IEnumerator flashRadar()
    {
        radarIsFlashing = true;
        radarSphere.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        yield return new WaitForSeconds(flashTime);
        radarSphere.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
        radarIsFlashing = false;

    }
}
