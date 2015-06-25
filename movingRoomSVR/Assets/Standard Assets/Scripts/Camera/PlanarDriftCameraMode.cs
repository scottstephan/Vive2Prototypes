using UnityEngine;
using System.Collections;

public class PlanarDriftCameraMode : BaseCameraMode
{
    private Vector3[] planarDriftLocations;

    public float useDriftLocationRatio = 1.0f;

    public float xRandomBounds = 200.0f;
    public float yRandomBounds = 200.0f;

    public float switchLocationTimeMin = 2.0f;
    public float switchLocationTimeMax = 10.0f;
    public float switchLocationTimeRawBias = 4.0f;
    public float switchLocationTimePower = 1.5f;

    private float currentSmoothTime;
    public float smoothTimeMin = 2.0f;
    public float smoothTimeMax = 15.0f;
    public float smoothTimeRawBias = 5.0f;
    public float smoothTimePower = 1.5f;

    public float speedMax = 60.0f;

    private float vertical_dead_zone = 0.15f;
    private float horizontal_dead_zone = 0.1f;
    private float normalized_centerX = 0.5f;
    private float normalized_centerY = 0.5f;
	private float mouse_normalized_y = 0.0f;
	private float mouse_normalized_x = 0.0f;
	
    private float timeToSwitchLocation = 10.0f;
	private float rotationTimer = 0.0f;

    private Vector3 currentVelocity;
    //	private Vector3 currentRotationSpeed;
//    private Vector3 boundryDirection = Vector3.zero;
    //	private Vector3 camForward;
//    private Vector3 camRight;
//    private Vector3 camUp;
    private Vector3 desiredLocation;

    private Vector3 ogPosition;
    private Quaternion ogRotation;
    //	private Vector3 originalCameraForward;
    private Color mainColor = new Color(1F, 0F, 0F, 0.6F);
    private Color otherColor = new Color(0F, 1F, 0F, 0.6F);
	
	private bool _bAbleToRotate = false;
    private bool bAbleToRotate {
		get{
            // App.UIManager doesnt yet exist in BluVR.
/*            if(App.UIManager != null && App.UIManager.anyNonMenuOpen){
                return false;
            }*/
			return _bAbleToRotate;
		}
	}
	

//    private Vector3 originalCameraForward = Vector3.zero;
//    private Vector3 desiredCameraForward = Vector3.zero;
    private Quaternion originalCameraRotation = Quaternion.identity;
    private Quaternion desiredCameraRotation = Quaternion.identity;
    private Vector3 originalCameraUp = Vector3.zero;
    private Vector3 originalCameraRight = Vector3.zero;
    private Quaternion yaw = Quaternion.identity;
    private Quaternion pitch = Quaternion.identity;
    private float yawAngle = 0.0f;
    private float pitchAngle = 0.0f;
    private float actualYawAngle = 0.0f;
    private float actualPitchAngle = 0.0f;
    private float pitchSpeed = 0.0f;
    private float yawSpeed = 0.0f;

    public float maxCameraHeight = -10.0f;
	
	private bool bHasMovedMouse = false;
	
	//Change Camera Controls
	//1 = Original Scheme -> move mouse, camera moves in that direction until gradually coming to a stop
	//2 = New Scheme -> move mouse and camera will continue to move in that direction until the mouse is manually moved back to center
	private int controlSchemeIndex = 2;
		
    public override void InitCameraMode()
    {
        if( inited ) {
            return;
        }
        
        base.InitCameraMode();        
        
        cameraType = CameraType.DriftCamera;
        myTransform = gameObject.transform;

        Transform[] drift_transforms = gameObject.GetComponentsInChildren<Transform>();
        planarDriftLocations = new Vector3[drift_transforms.Length];
        int i = 0;
        foreach (Transform drift_transform in drift_transforms)
        {
            planarDriftLocations[i++] = drift_transform.position;
        }

        //		camForward = transform.forward;
//        camRight = myTransform.right;
//        camUp = myTransform.up;
        ogPosition = myTransform.position;
		ogRotation = myTransform.rotation;
        desiredLocation = myTransform.position;

        timeToSwitchLocation = 0.0f;

        currentVelocity = Vector3.zero;
        //	currentRotationSpeed = Vector3.zero;
        cameraName = "Progression Camera";

        //originalCameraForward = myTransform.forward;
        myTransform.forward = transform.forward;
    }
    public override void StartCameraMode()
    {
        myTransform.position = ogPosition;
		myTransform.rotation = ogRotation;
		
		bHasMovedMouse = false;
        _bAbleToRotate = false;
//        originalCameraForward = myTransform.forward;
//        desiredCameraForward = originalCameraForward;
        originalCameraRotation = myTransform.rotation;
        desiredCameraRotation = originalCameraRotation;
        originalCameraUp = myTransform.up;
        originalCameraRight = myTransform.right;
//        Quaternion yaw = Quaternion.identity;
//        Quaternion pitch = Quaternion.identity;
        yawAngle = 0.0f;
        pitchAngle = 0.0f;
        actualPitchAngle = 0.0f;
        actualYawAngle = 0.0f;
		rotationTimer = 0.0f;
    }
	void UpdatingRotationTimer()
	{
		if( InputManager.timeSinceMouseMovement < 0.25f ) {
			bHasMovedMouse = true;
		}
		
		rotationTimer += Time.deltaTime;
		if(rotationTimer > 2.0f && bHasMovedMouse)
		{
			_bAbleToRotate = true;
		}
	}
    public override void UpdateCameraMode()
    {
		runCollision = CameraCollisionType.None;

        float dt = Time.deltaTime;
        timeToSwitchLocation -= dt;

        if( bAbleToRotate 
		   && AppBase.inFocus )
        {
            MouseControl();
        }
        else
        {
			UpdatingRotationTimer();
          //  DeadZoneDetection();
        }

        if (timeToSwitchLocation <= 0.0f)
        {
            float tmp = Random.value;
            if (planarDriftLocations != null && planarDriftLocations.Length > 0 && tmp < useDriftLocationRatio)
            {
                int new_idx = Random.Range(0, planarDriftLocations.Length);
                desiredLocation = planarDriftLocations[new_idx];
            }
            else
            {
                float x_offset = RandomExt.FloatRange(-xRandomBounds, xRandomBounds);
                float y_offset = RandomExt.FloatRange(-yRandomBounds, yRandomBounds);
                desiredLocation = ogPosition + (originalCameraUp * y_offset) + (originalCameraRight * x_offset);
            }

            currentSmoothTime = RandomExt.FloatWithBiasPower(smoothTimeMin, smoothTimeMax, smoothTimeRawBias, smoothTimePower);
            timeToSwitchLocation = RandomExt.FloatWithBiasPower(switchLocationTimeMin, switchLocationTimeMax, switchLocationTimeRawBias, switchLocationTimePower);
        }

        myTransform.position = Vector3.SmoothDamp(myTransform.position, desiredLocation, ref currentVelocity, currentSmoothTime, speedMax);

        if (myTransform.position.y > maxCameraHeight)
        {
            myTransform.position = new Vector3(myTransform.position.x, maxCameraHeight, myTransform.position.z);
        }
    }
    private void DeadZoneDetection()
    {
        mouse_normalized_y = Input.mousePosition.y / Screen.height;
        mouse_normalized_x = Input.mousePosition.x / Screen.width;

        if (Mathf.Abs(mouse_normalized_x - normalized_centerX) <= horizontal_dead_zone && Mathf.Abs(normalized_centerY - mouse_normalized_y) <= vertical_dead_zone)
        {
            _bAbleToRotate = true;
        }
    }
    bool MouseCanRotate()
    {
		switch (controlSchemeIndex)
		{
		case 1:
			//Debug.Log("controlSchemeIndex " + controlSchemeIndex);
		    mouse_normalized_y = Input.mousePosition.y / Screen.height;
	        mouse_normalized_x = Input.mousePosition.x / Screen.width;
/*	        if (App.UIManager.menu.isOpen)
	        {
				//Debug.Log("reset1.1");
				yawAngle = actualYawAngle;
				pitchAngle = actualPitchAngle;
	            return false;
	        }
			else */
			if (Mathf.Abs(mouse_normalized_x - normalized_centerX) > 0.5f || Mathf.Abs(mouse_normalized_y - normalized_centerY) > 0.5f)
	        {
				//Debug.Log("reset1.2");
				yawAngle = actualYawAngle;
				pitchAngle = actualPitchAngle;
	            return false;
			}
	        else
	        {
				//Debug.Log("pass1");
	            return true; 
	        }
		case 2:
			mouse_normalized_y = Input.mousePosition.y / Screen.height;
	        mouse_normalized_x = Input.mousePosition.x / Screen.width;
//	        if (GUIMasterManager.isCursorOverUIButton() || GUIStateManager.IsAnyNonUpdatePanelOpen() /*|| App.UIManager.menu.isOpen*/)
//	        {
//				//Debug.Log("reset2.1");
//				yawAngle = actualYawAngle;
//				pitchAngle = actualPitchAngle;
//	            return false;
//	        }
//			else 
            if (Mathf.Abs(mouse_normalized_x - normalized_centerX) > 0.5f || Mathf.Abs(mouse_normalized_y - normalized_centerY) > 0.5f)
	        {
				//Debug.Log("reset2.2");
				yawAngle = actualYawAngle;
				pitchAngle = actualPitchAngle;
	            return false;
			}
			else if (Mathf.Abs(mouse_normalized_x - normalized_centerX) <= horizontal_dead_zone && Mathf.Abs(normalized_centerY - mouse_normalized_y) <= vertical_dead_zone)
	        {
				//Debug.Log("reset2.3");
				yawAngle = actualYawAngle;
				pitchAngle = actualPitchAngle;
	            return false;
			}
	        else
	        {
				//Debug.Log("pass2");
	            return true; 
	        }
		}
	//	Debug.LogWarning("You must select a control scheme : 1 or 2");
		return false;
     
    }

    private void MouseControl()
    {
		switch (controlSchemeIndex)
		{
		case 1:
			mouse_normalized_y = Input.mousePosition.y / Screen.height;
	        mouse_normalized_x = Input.mousePosition.x / Screen.width;
			
			yawAngle = 0;
			pitchAngle = 0;
	        if (Mathf.Abs(mouse_normalized_x - normalized_centerX) > horizontal_dead_zone && MouseCanRotate())
	        {
				float x = mouse_normalized_x - normalized_centerX;
	            yawAngle = x * ((0.5f-horizontal_dead_zone)/0.5f) * 120.0f;
	            yawAngle = Mathf.Clamp(yawAngle, -30, 30);
	        }
		
	        actualYawAngle = Mathf.SmoothDamp(actualYawAngle, yawAngle, ref yawSpeed, 3.0f);
	        yaw = Quaternion.AngleAxis(actualYawAngle, Vector3.up);
	        if (Mathf.Abs(mouse_normalized_y - normalized_centerY) > vertical_dead_zone && MouseCanRotate())
	        {
				float y = mouse_normalized_y - normalized_centerY;
				pitchAngle = -y  * ((0.5f-vertical_dead_zone)/0.5f) * 100.0f;
	            pitchAngle = Mathf.Clamp(pitchAngle, -20, 20);
	        }
			
	      	actualPitchAngle = Mathf.SmoothDamp(actualPitchAngle, pitchAngle, ref pitchSpeed, 3.0f);
	        pitch = Quaternion.Euler(actualPitchAngle, 0, 0);
	        desiredCameraRotation = yaw * originalCameraRotation * pitch;
	        myTransform.rotation = Quaternion.RotateTowards(myTransform.rotation, desiredCameraRotation, 0.3f);
			break;
		case 2:
			//Debug.Log("case 2");
	        mouse_normalized_y = Input.mousePosition.y / Screen.height;
	        mouse_normalized_x = Input.mousePosition.x / Screen.width;
			
	        if (Mathf.Abs(mouse_normalized_x - normalized_centerX) > horizontal_dead_zone && MouseCanRotate())
	        {
				float x = mouse_normalized_x - normalized_centerX;
				yawAngle += x*70 * Time.deltaTime;  //x * ((0.5f-horizontal_dead_zone)/0.5f) * 50.0f;
	            yawAngle = Mathf.Clamp(yawAngle, -30, 30);
	        }
			else
			{
				yawAngle = Mathf.MoveTowards(yawAngle, actualYawAngle,1.0f);
				yawSpeed =Mathf.MoveTowards (yawSpeed,0.0f,0.1f);
			}
	        actualYawAngle = Mathf.SmoothDamp(actualYawAngle, yawAngle, ref yawSpeed, 3.0f);
	        yaw = Quaternion.AngleAxis(actualYawAngle, Vector3.up);// .Euler(0, actualYawAngle, 0);
			
			
	        if (Mathf.Abs(mouse_normalized_y - normalized_centerY) > vertical_dead_zone && MouseCanRotate())
	        {
				float y = mouse_normalized_y - normalized_centerY;
				pitchAngle += -y * 70 * Time.deltaTime;// * ((0.5f-vertical_dead_zone)/0.5f) * 40.0f;
	            pitchAngle = Mathf.Clamp(pitchAngle, -20, 20);
	        }
			else
			{
				//pitchAngle = actualPitchAngle3
				pitchAngle = Mathf.MoveTowards(pitchAngle, actualPitchAngle,1.0f);
				pitchSpeed = Mathf.MoveTowards (pitchSpeed,0.0f,0.1f);
			}
	      	actualPitchAngle = Mathf.SmoothDamp(actualPitchAngle, pitchAngle, ref pitchSpeed, 3.0f);
	        pitch = Quaternion.Euler(actualPitchAngle, 0, 0);
	        desiredCameraRotation = yaw * originalCameraRotation * pitch;
	        myTransform.rotation = Quaternion.RotateTowards(myTransform.rotation, desiredCameraRotation, 0.3f);
			break;
		}
	}

    void OnDrawGizmos()
    {
        if (!drawGizmos)
        {
            return;
        }

        Transform[] drift_transforms = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform drift_transform in drift_transforms)
        {
            Color use_color = otherColor;
            float use_radius = 3.4f;
            if (drift_transform.position == transform.position)
            {
                use_color = mainColor;
                use_radius = 6.8f;
            }

            Gizmos.color = use_color;
            Gizmos.DrawSphere(drift_transform.position, use_radius);
            Gizmos.DrawLine(drift_transform.position, drift_transform.position + (transform.forward * 100.0f));// TODO>pivot
        }
    }
}