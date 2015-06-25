using UnityEngine;
using System.Collections;

public class BootSceneLoadTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider col) {
		
		Debug.Log("TRIGGERED LOAD!");
		
		//start load!
		//CameraManager.StopFly();
		TravelMenuManager.StartTravel(null);
	}
}
