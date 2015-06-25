using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RotateAroundObjectCamera : BaseCameraMode
{
	Vector3 rotateAroundPoint;
	float rotateAroundPointToCameraDistance = 0f;
	public float speed = 400f;
	
    public override void InitCameraMode()
    {
        if( inited ) {
            return;
        }
        
        base.InitCameraMode();        
        
		unlocked = true;
        cameraType = CameraType.RotateAroundObjectCamera;
		myTransform = gameObject.transform;
		rotateAroundPoint = myTransform.position;
    }
    public override void StartCameraMode()
    {
   		gameObject.transform.position = myTransform.position;
		gameObject.transform.rotation = myTransform.rotation;
		gameObject.transform.up = Vector3.up;
		rotateAroundPointToCameraDistance = 100;
		myTransform.position = rotateAroundPoint + (-myTransform .forward) * rotateAroundPointToCameraDistance;
		
		
    }
	
    public override void UpdateCameraMode()
    {
		runCollision = CameraCollisionType.None;

		myTransform.position =Vector3.Lerp(myTransform.position, rotateAroundPoint + (-myTransform .forward) * rotateAroundPointToCameraDistance, Time.deltaTime * 5.0f);
		MouseControl();

      
    }


    private void MouseControl()
    {
		if(Input.GetMouseButton(0))
		{
			myTransform.RotateAround(rotateAroundPoint,Vector3.up,Input.GetAxis("Mouse X")*600.0f * Time.deltaTime);
			myTransform.RotateAround(rotateAroundPoint,myTransform.right,-Input.GetAxis("Mouse Y")*600.0f * Time.deltaTime);
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
	}
}
