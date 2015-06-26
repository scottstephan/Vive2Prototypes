using UnityEngine;
using System.Collections;

/// <summary>
/// Bigmouse Studio, by Richard Zhang
/// Free camera in 3D world
/// Please use this script together with MouseLook.js from Standard Assets
/// </summary>

public class CameraMotion : MonoBehaviour {
	public float moveSpeed = 10.0f;

	// Update is called once per frame
	void FixedUpdate () {
		float moveForward = Input.GetAxis("Vertical");
		
		if(Mathf.Abs(moveForward) > 0.01f)
		{
			transform.position += transform.TransformDirection(new Vector3(0, 0, moveForward)).normalized * moveSpeed * Time.deltaTime;
		}
	}
}
