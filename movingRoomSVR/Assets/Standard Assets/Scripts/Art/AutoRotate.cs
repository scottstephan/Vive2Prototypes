using UnityEngine;
using System.Collections;

public class AutoRotate : MonoBehaviour {

	public Vector3 rotSpeed = new Vector3(0f, 0f, 5f);
	private Transform myXform;

	void Start () {
		myXform = transform;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 rot =myXform.localEulerAngles ;
		myXform.localEulerAngles = new Vector3(rot.x + rotSpeed.x * Time.deltaTime, rot.y + rotSpeed.y * Time.deltaTime, rot.z + rotSpeed.z * Time.deltaTime);
	
	}
}
