using UnityEngine;
using System.Collections;

public class svrControllerTets : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.Log (gameObject.name + "is on!");
		addRaycast ();
	}

	private void addRaycast(){
		gameObject.AddComponent<rayCastFromController> ();
	}

	// Update is called once per frame
	void Update () {
	
	}
}
