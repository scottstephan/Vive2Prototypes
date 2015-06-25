using UnityEngine;
using System.Collections;

public class PointConstraintDirection : MonoBehaviour {

	public Transform rotation;
	public Transform parent;
	public float offset;

	private Transform myXform;

	void Start () {
		myXform= transform;
	}
	
	// Update is called once per frame
	void Update () {
		myXform.position = parent.position + rotation.forward * offset;
	}
}
