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
	private bool isPulsing = false;
	public ushort hapticPulseRate = 0;
    public float minPulseDelay = 1.5f;
    public float curPulseDelay = 0f;

	// Use this for initialization
	void Start () {
		StartCoroutine("pulseHaptic");
		StartCoroutine("flashRadar");
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
			ushort tempHaptic = (ushort)(1000 - dist * 100); //i.e, there's less of a mod as you get closer to a treasure
			hapticPulseRate = tempHaptic > hapticPulseRate ? tempHaptic : hapticPulseRate; //pulse according to the closest treasure
			curPulseDelay = minPulseDelay - dist * .25f;
			flashTime = .1f + dist;

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
        radarSphere.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        yield return new WaitForSeconds(flashTime);
        radarSphere.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
		yield return new WaitForSeconds(flashTime);
		StartCoroutine ("flashRadar");
    }

    IEnumerator pulseHaptic()
    {
        SteamVR_Controller.Input(lastHeldIndex).TriggerHapticPulse(hapticPulseRate);
        yield return new WaitForSeconds(curPulseDelay);
    }
}
