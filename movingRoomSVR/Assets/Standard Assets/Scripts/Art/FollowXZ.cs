using UnityEngine;
using System.Collections;

public class FollowXZ : MonoBehaviour {
	public Transform target;
	private Transform myXform;
	void Start () {
		myXform = transform;
		GameObject cam = GameObject.Find("OVRPlayerBody");
		target = cam.transform;
	}
	
	// Update is called once per frame
	void Update () {
		if(target != null){
			myXform.position = new Vector3(target.position.x, 0f, target.position.z);
		}
	}
}
