using UnityEngine;
using System.Collections;

public class PauseControl : MonoBehaviour {

	private bool pause_mode_active = false;
	private static PauseControl singleton;	

	void Awake()
	{
		singleton = this;
		pause_mode_active = false;
	}

	void SetPause( bool pause )
	{
		GameObject[] game_objects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach (GameObject game_object in game_objects) {
			game_object.SendMessage("DidPause", pause, SendMessageOptions.DontRequireReceiver);
		}
		
		pause_mode_active = pause;	
	}
	
	public static void PublicSetPause (bool pause ) {
		singleton.SetPause (pause);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (InputManager.debugKeysActive && InputManager.GetKeyDown ("p")) {
	        SetPause(!pause_mode_active);
	    }	
	}
}
