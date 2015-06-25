using UnityEngine;
using System.Collections;

public class panoCamScript : MonoBehaviour {
	
	public int camCount = 16;
    public float degreesToCover = 360f;

	public void Init() {

		if (transform.childCount == 0) {
			for (int i=0; i < camCount; i++) {
				GameObject go = new GameObject ();
				go.transform.parent = transform;
				go.transform.localPosition = new Vector3(0,0,0);
				go.name = "subCam"+i;
				go.AddComponent<Camera> ();
			}
		} else if (transform.childCount != camCount) {
			Debug.LogError ("In order to re-creating all child cameras, please manually delete all the child game objects of panoCam.");
			return;
		} else {
			// update all cameras
			prepSubCameras();
		}

	}
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	void prepSubCameras() {
		float hwratio = (float)Screen.height/(float)Screen.width*(float)transform.childCount;

		for(int i = 0; i < transform.childCount; i++) {
			Transform child = transform.GetChild(i);
			float onepart = 1.0f/transform.childCount;
			child.GetComponent<Camera>().fieldOfView = 2*Mathf.Atan(hwratio*Mathf.Tan ((degreesToCover/2)*Mathf.Deg2Rad/transform.childCount))*Mathf.Rad2Deg;
			child.localEulerAngles = new Vector3(0, degreesToCover/transform.childCount * i, 0);
			child.GetComponent<Camera>().rect = new Rect(onepart * i, 0, onepart, 1);
		}
	}
}
