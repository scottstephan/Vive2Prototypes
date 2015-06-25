using UnityEngine;
using System.Collections;

public class IntroFirstTimeSceneLoadTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider col) {

        PlayerPrefs.SetInt(FloatingMenuManager.FIRSTAPPRUNPREFSTR, 1);
		TravelMenuManager.StartTravel("whale");
	}
}
