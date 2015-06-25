using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour {

	Transform _bod;

	// Use this for initialization
	void Start () {
//		Attach ();
	}

	void LateUpdate() {

		//this.transform.position = _bod.position;

	}

	public void Attach() {

		_bod = GameObject.Find("OVRPlayerBody").transform;
		this.transform.parent = _bod;
	}
}
