using UnityEngine;
using System.Collections;

public class screenshotCap : MonoBehaviour {
    int screenshotIndex = 0;
    public int sizeMult = 1;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Taking a screenshot!");
            string shotString =  screenshotIndex + "bluSVR_panorama.png";
            Application.CaptureScreenshot(shotString,sizeMult);
            screenshotIndex++;
        }
	}
}
