using UnityEngine;
using System;
using System.Collections;

public class navLine_ObjBehavior : MonoBehaviour {
	private Vector3 originScale;
	private float dist;
	public float per;
	private float newScale;

	public float maxScale;
	public float minScale;
	public bool lookAtUpdate = false;
	private Vector3 lookAtPos;
	private Quaternion lookAtDir;
	// Use this for initialization
	void Start () {
		originScale = transform.localScale;
		lookAtPos = GameObject.Find ("centerRot").transform.position;

	}
	
	// Update is called once per frame
	void Update () {
		dist = Math.Abs(lineNav.fwdViewPt.x - transform.position.x); //cvt -* to +*
		per = dist / lineNav.navRadius;
		newScale = Mathf.Lerp (maxScale, minScale, per);
		transform.localScale = new Vector3(newScale,newScale,newScale);


		if (lookAtUpdate) {
			lookAtDir = Quaternion.LookRotation (lookAtPos - transform.position);
			transform.rotation = lookAtDir;
		}

	}
}
