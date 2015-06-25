using UnityEngine;
using System.Collections;
/// <summary>
///this is an independent camera mode for turntable preview scene only.
/// </summary>
public class TurnTableCameraMode : MonoBehaviour
{
	private Transform spawnPoint;
	private float initialPositionOffset = 100f;
	private float rotateAroundPointToCameraDistance = 0f;
	private float speed = 20f;
	
	// Use this for initialization
	void Start ()
	{
		spawnPoint = GameObject.Find("sbpoint").transform;
		Vector3 spawnPointPosition = spawnPoint.position;
		transform.position = spawnPoint.right * initialPositionOffset + spawnPointPosition;
		transform.LookAt(spawnPointPosition);
		rotateAroundPointToCameraDistance = initialPositionOffset;
	}

	// Update is called once per frame
	void Update ()
	{
		transform.position =Vector3.Lerp(transform.position, spawnPoint.position + (-transform .forward) * rotateAroundPointToCameraDistance, Time.deltaTime * 5.0f);
		MouseControl();
	}
	private void MouseControl()
    {
		Vector3 rotateAroundPoint = spawnPoint.position;
		if(Input.GetMouseButton(0))
		{
			transform.RotateAround(rotateAroundPoint,Vector3.up,Input.GetAxis("Mouse X")*600.0f * Time.deltaTime);
			transform.RotateAround(rotateAroundPoint,transform.right,-Input.GetAxis("Mouse Y")*600.0f * Time.deltaTime);
		}
		rotateAroundPointToCameraDistance -= Input.GetAxis("Mouse ScrollWheel") * speed * Time.deltaTime * 100;
		if(Input.GetKey(KeyCode.UpArrow))
		{
			rotateAroundPointToCameraDistance -=  10.0f* Time.deltaTime * 100;
		}
		if(Input.GetKey(KeyCode.DownArrow))
		{
			rotateAroundPointToCameraDistance +=  10.0f * Time.deltaTime * 100;
			
		}
	}}

