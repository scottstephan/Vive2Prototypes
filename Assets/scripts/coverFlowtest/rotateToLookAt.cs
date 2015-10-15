using UnityEngine;
using System;
using System.Collections;

public class rotateToLookAt : MonoBehaviour {
	private Quaternion lookAtDir;
	private Vector3 lookAtPos;
	private float originZ;
	private float originX;
	public float angle;
	public float per;
	public float maxScale;
	public float minScale;
	// Use this for initialization
	void Start () {
		lookAtPos = GameObject.Find ("centerRot").transform.position;
		originZ = transform.localRotation.z;
		originX = transform.localRotation.x;
	}
	
	// Update is called once per frame
	void Update () {
		updateLookRot ();
		updateScale ();
	}

	private void updateLookRot(){
		lookAtDir = Quaternion.LookRotation (lookAtPos - transform.position);
		lookAtDir.z = originZ;
		lookAtDir.x = originX;
		transform.rotation = lookAtDir;
		Debug.DrawLine (transform.position, lookAtPos);
	}

	private void updateScale(){
		angle = Vector3.Angle (rotateableObjectsCreator.fwdViewPt, transform.position);
		per = angle / 180;
		float newScale = Mathf.Lerp (maxScale, minScale, per);
		transform.localScale = new Vector3(newScale,newScale,newScale);

	}
}
