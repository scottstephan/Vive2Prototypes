using UnityEngine;
using System.Collections;

public class spherePointChoose : MonoBehaviour {
	public float maxDelayBtwMoves = 3f;
	private float timeCount;
	public float moveSpdPerSec;
	private Vector3 oldPos;
	private Vector3 newPosOnSphere;
	public Transform sphereTarget;
	private bool isInRotateMode = false;
	public float rotateAmt;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		timeCount += Time.deltaTime;
		if (Input.GetKeyDown (KeyCode.M))moveToNewSpherePt ();
		if (isInRotateMode) {
			transform.RotateAround (sphereTarget.position, Vector3.up, rotateAmt * Time.deltaTime);
			transform.LookAt (sphereTarget.position);

		}

	/*	else if (timeCount > maxDelayBtwMoves) {
			moveToNewSpherePt();
		}*/
	}

	private void moveToNewSpherePt(){
		newPosOnSphere = Random.insideUnitSphere + sphereTarget.position;
		Debug.Log("Moving to " + newPosOnSphere);
		oldPos = gameObject.transform.position;
		StopAllCoroutines ();
		StartCoroutine ("lerpMove");
	}

	IEnumerator lerpMove(){
		float rate = 1 / moveSpdPerSec;
		float i = 0;
		Vector3 newPos = new Vector3(0,0,0);

		while (i < 1) {
			i += rate * Time.deltaTime;
			gameObject.transform.position = Vector3.Lerp(oldPos,newPosOnSphere,i);
		//	Debug.Log ("NewPos:" + newPos);
			//gameObject.transform.position = newPos;
			yield return null;
		}

		isInRotateMode = true;


	}
}
