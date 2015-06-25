using UnityEngine;
using System.Collections;

public class RotateToCam : MonoBehaviour {


	private Transform myXform;
	void Start () {
		myXform = transform;
	}
	
	void Update () {
		myXform.rotation = Quaternion.LookRotation(CameraManager.GetCurrentCameraPosition() - myXform.position);
	}
}
