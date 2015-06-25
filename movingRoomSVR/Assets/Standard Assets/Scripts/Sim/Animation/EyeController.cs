
using UnityEngine;
using System.Collections;

public class EyeController {

	public static void Init( CritterInfo critter_info ) {
		EyeControllerData data = critter_info.critterEyeData;
	
		if( data == null 
		   || data.leftEye == null 
		   || data.rightEye == null ) {
			return;
		}
		
        if (data.rootNode == null) {
            data.rootNode = critter_info.critterTransform;
        }
        
		data.ogLeftLocalRotation = data.leftEye.localRotation;
		data.ogLeftRotation = data.leftEye.rotation;
		data.ogLeftLocalInvRotation = Quaternion.Inverse( data.ogLeftLocalRotation );

		data.ogRightLocalRotation = data.rightEye.localRotation;
		data.ogRightRotation = data.rightEye.rotation;
		data.ogRightLocalInvRotation = Quaternion.Inverse( data.ogRightLocalRotation );

		data.runtimeInitialized = false;

		// we default off.
		data.OFF = true;
	}
	
	private static void RuntimeInit( CritterInfo critter_info, EyeControllerData data ) {
    	data.prevRotation = critter_info.critterTransform.rotation;

		if( data.lookAt == null ) {
			data.lookAt = CameraManager.GetCurrentCameraTransform();
		}

		data.runtimeInitialized = true;		
	}
	
    public static void UpdateEyes( CritterInfo critter_info ) {

		EyeControllerData data = critter_info.critterEyeData;

		if( data.leftEye == null 
		   || data.rightEye == null ) {
			return;
		}

		if( data.OFF ) {
			return;
		}
		
		if( !data.runtimeInitialized ) {
			RuntimeInit( critter_info, data );
			return;
		}
	
//		float dt = Time.deltaTime;
		
		Quaternion cur = critter_info.critterTransform.rotation;
//		Quaternion inv_cur = Quaternion.Inverse( cur );

        Vector3 left_eye_forward_world;
        Vector3 right_eye_forward_world;

        if (data.useDirection)
        {
            left_eye_forward_world = data.lookAtDirectionLeft;
            right_eye_forward_world = data.lookAtDirectionRight;
        }
        else
        {
            left_eye_forward_world = data.lookAt.position - data.leftEye.position;
            right_eye_forward_world = data.lookAt.position - data.rightEye.position;
        }

		Quaternion eye_space = cur * data.ogLeftRotation;
		Vector3 left_local = Quaternion.Inverse( eye_space ) * left_eye_forward_world;

		eye_space = cur * data.ogRightRotation;
		Vector3 right_local = Quaternion.Inverse( eye_space ) * right_eye_forward_world;

//		Debug.Log( "Left Eye local " + left_local );
//		Debug.Log( "Right Eye local " + right_local );
		
		// extract yaw and pitch
		float left_yaw = Mathf.Clamp( Mathf.Atan2( left_local.x, left_local.z ) * Mathf.Rad2Deg, -data.maxYawDegrees, data.maxYawDegrees );
		float right_yaw = Mathf.Clamp( Mathf.Atan2( right_local.x, right_local.z ) * Mathf.Rad2Deg, -data.maxYawDegrees, data.maxYawDegrees );

		float left_pitch = Mathf.Clamp( Mathf.Atan2( left_local.y, Mathf.Sqrt(left_local.x * left_local.x + left_local.z * left_local.z )) * Mathf.Rad2Deg, -data.maxPitchDegrees, data.maxPitchDegrees);
		float right_pitch = Mathf.Clamp( Mathf.Atan2( right_local.y,Mathf.Sqrt(right_local.x * right_local.x + right_local.z * right_local.z )) * Mathf.Rad2Deg, -data.maxPitchDegrees, data.maxPitchDegrees);

        if (data.forwardAxis == EyeControllerData.EyeForwardAxis.ZForward)
        {
    		data.leftEye.localRotation = Quaternion.Euler( left_yaw, -left_pitch, 0f ) * data.ogLeftLocalRotation;
	    	data.rightEye.localRotation = Quaternion.Euler( right_yaw, right_pitch, 0f ) * data.ogRightLocalRotation;
        }
        else
        {
            data.leftEye.localRotation = Quaternion.Euler( 0f, left_yaw, -left_pitch ) * data.ogLeftLocalRotation;
            data.rightEye.localRotation = Quaternion.Euler( 0f, right_yaw, right_pitch ) * data.ogRightLocalRotation;
        }
	}
}
 