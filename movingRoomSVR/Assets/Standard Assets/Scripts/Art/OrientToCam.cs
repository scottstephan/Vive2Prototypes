using UnityEngine;
using System.Collections;

public class OrientToCam : MonoBehaviour {

	private Transform myXform;
	void Start () {
		myXform = transform;
	}

	void Update () {
		Vector3 diff = Camera.main.transform.position - myXform.position;
		myXform.rotation = Quaternion.LookRotation(diff.normalized);
	}
}
