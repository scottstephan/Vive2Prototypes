using UnityEngine;
using System.Collections;

public class sphereRotator : MonoBehaviour {
	public Transform rotateTarget;
	public float rotateAmt = 180;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround (rotateTarget.position, Vector3.up, rotateAmt * Time.deltaTime);
		transform.LookAt (rotateTarget.position);
		if (Input.GetKeyDown (KeyCode.N)) {
			//StartCoroutine("rotateAround");

		}
	}

	IEnumerator rotateAround(){
		float i = 0;
		while (i < rotateAmt) {
			i++;
			yield return null;
		}

	}
}
