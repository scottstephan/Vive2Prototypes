using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FPSCameraMode :  BaseCameraMode {
	
//	private GameObject boundingVolume;	
	public bool debugMode = false;
	
    public float sensitivityYaw = 15F;
    public float sensitivityPitch = 15F;

    public float minYaw = -360F;
    public float maxYaw = 360F;

    public float minPitch = -80F;
    public float maxPitch = 80F;
	
	public float xSpeedMul = 3.0f;
	public float ySpeedMul = 1.5f;
	public float zSpeedMul = 6.0f;
	
	public float maxCameraHeight = -10.0f;
	
    float rot_yaw = 0F;
    float rot_pitch = 0F;
    
    Quaternion originalRotation;
	
    public override void InitCameraMode() {    
        if( inited ) {
            return;
        }
        
        base.InitCameraMode();        
        
		myTransform = transform;
		cameraType = CameraType.FPSCamera;

/*		if( boundingVolume == null ) {
			boundingVolume = GameObject.Find("SwimBoundaryCollider");
		}*/

        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
        originalRotation = myTransform.localRotation;
		
		cameraName = "FPS Camera";
    }

    public override void StartCameraMode() {
		myTransform.position = CameraManager.GetCurrentCameraPosition();
		myTransform.rotation = CameraManager.GetCurrentCameraRotation();
	}
	
    public override void UpdateCameraMode() {
		if( debugMode ) {
			runCollision = CameraCollisionType.None;
		}
		else {
			runCollision = CameraCollisionType.GroundAndCritters;
		}
		
/*    	Vector3 from_center = transform.position - boundingVolume.transform.position;
		float tp = Mathf.Clamp(Mathf.Atan2(from_center.y,Mathf.Sqrt(from_center.x * from_center.x + from_center.z * from_center.z)) * Mathf.Rad2Deg, -45, 45);
		float ty = Mathf.Atan2(-from_center.x,-from_center.z) * Mathf.Rad2Deg;
		print(from_center);
		print(tp);
		print(ty);
		print(transform.rotation.eulerAngles[1]);*/

    	bool left_mouse = Input.GetMouseButton(0);

        float x = Input.GetAxis("Left_Right") * xSpeedMul;
        float y = Input.GetAxis("Up_Down") * ySpeedMul;
       	float z = Input.GetAxis("Forward_Back") * zSpeedMul;
		Vector3 move_amt = myTransform.rotation * new Vector3(x,y,z);
		myTransform.position += move_amt;
		
		// in debug mode we are not limited by height
        if ( !debugMode && myTransform.position.y > maxCameraHeight ) {
			myTransform.position = new Vector3(myTransform.position.x,maxCameraHeight,myTransform.position.z);
		}
        
		if (left_mouse)
		{
			rot_yaw += Input.GetAxis("Mouse X") * sensitivityYaw;
           	rot_pitch += Input.GetAxis("Mouse Y") * sensitivityPitch;
	    	rot_yaw = MathfExt.ClampAngle(rot_yaw,minYaw,maxYaw);
        	rot_pitch = MathfExt.ClampAngle(rot_pitch,minPitch,maxPitch);
        	Quaternion yawQuat = Quaternion.AngleAxis (rot_yaw, Vector3.up);
        	Quaternion pitchQuat = Quaternion.AngleAxis (rot_pitch, Vector3.left);
        
        	myTransform.rotation = originalRotation * yawQuat * pitchQuat;
		}
	}       
 }