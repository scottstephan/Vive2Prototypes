using UnityEngine;
using System.Collections;

public class SinMotion : MonoBehaviour {
	public Vector3 amp;
	public Vector3 offset;
	public Vector3 freq;

	private Vector3 origLocalPos;
	private Transform myXform;

	// Use this for initialization
	void Start () {
		myXform = transform;
		origLocalPos = myXform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 tmp =  new Vector3(Mathf.Sin(Time.time * freq.x + offset.x) * amp.x , Mathf.Sin(Time.time * freq.y + offset.y) * amp.y, Mathf.Sin( Time.time * freq.z + offset.z) * amp.z);
		myXform.localPosition = origLocalPos + myXform.right * tmp.x + myXform.up * tmp.y + myXform.forward * tmp.z;
	
	}
}
