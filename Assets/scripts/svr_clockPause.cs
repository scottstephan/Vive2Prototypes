using UnityEngine;
using System.Collections;

public class svr_clockPause : MonoBehaviour {
	private bool timeIsPaused = false;
	// Use this for initialization
	void Start () {
	
	}
	
	void svrControllerDown()
	{
		Debug.Log (gameObject.name + "has gotten broadcast from controller");
		timeIsPaused = !timeIsPaused;
		Time.timeScale = timeIsPaused ? 1 : 0;

		Debug.Log ("Timescale is:" + Time.timeScale);

		/*IF WE PAUSE TIME, NO LONGER TRACKING COLLISIONS, NO EXIT INFO FOR CONTROLLER FROM COLLIDER. Need to void it BEFORE the freeze */
	}

	void svrControllerUp(){

	}


}
