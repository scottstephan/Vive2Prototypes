using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StaticCameraMode : BaseCameraMode
{
    void Awake()
    {
        cameraType = CameraType.StaticCamera;
        myTransform = transform;
        cameraName = "Static Camera";
    }
	
    public override void StartCameraMode()
    {
        myTransform = CameraManager.GetCurrentCameraTransform();
    }
	
    public override void UpdateCameraMode()
    {
	}
}
