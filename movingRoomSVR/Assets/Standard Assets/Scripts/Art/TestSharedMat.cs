using UnityEngine;
using System.Collections;

public class TestSharedMat : MonoBehaviour {
	public bool switchNow;
	public Material origMat;
	public Material newMat;
	public bool isOrig = true;

	void Start () {
		origMat = GetComponent<Renderer>().sharedMaterial;
	}
	
	void Update () {
		if(switchNow){
			switchNow = false;
			if(isOrig)
				GetComponent<Renderer>().material = newMat;
			else
				GetComponent<Renderer>().sharedMaterial = origMat;
			isOrig = !isOrig;

		}
	}
}
