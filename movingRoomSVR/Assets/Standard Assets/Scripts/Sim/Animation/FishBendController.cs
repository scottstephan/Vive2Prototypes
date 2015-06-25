
using UnityEngine;
using System.Collections;

public class FishBendController {

	public static void Init( CritterInfo critter_info ) {
		FishBendControllerData data = critter_info.critterBendData;
	
		if( data == null 
			|| data.spineJointChain.firstTransform == null
			|| data.spineJointChain.lastTransform == null ) {
			return;
		}
		
        if (data.rootNode == null) {
            data.rootNode = critter_info.critterTransform;
        }
        
		data.maxSpeedAddition = critter_info.generalMotionData.swimSpeed * data.maxSpeedAddPercentage;
		data.spineJointChain.chainLength = 1;
		Transform t = data.spineJointChain.lastTransform;		
		while (t != data.spineJointChain.firstTransform && t != t.root) {
			data.spineJointChain.chainLength++;
			t = t.parent;
		}
		
		data.spineJointChain.transforms = new Transform[data.spineJointChain.chainLength];
		data.spineJointChain.lastLocalAngles = new float[data.spineJointChain.chainLength];
		data.spineJointChain.distanceToParent = new float[data.spineJointChain.chainLength];
		data.spineJointChain.localRotations = new Quaternion[data.spineJointChain.chainLength];
		data.spineJointChain.origLocalRotations = new Quaternion[data.spineJointChain.chainLength];
		t = data.spineJointChain.lastTransform;
		for (int i=data.spineJointChain.chainLength-1; i>=0; i--) {
			data.spineJointChain.transforms[i] = t;
			data.spineJointChain.distanceToParent[i] = t.position.magnitude;
			data.spineJointChain.localRotations[i] = Quaternion.identity;
			data.spineJointChain.lastLocalAngles[i] = 0f;
			data.spineJointChain.origLocalRotations[i] = t.localRotation;
			t = t.parent;
		}
		
		data.runtimeInitialized = false;
	}
	
	private static void RuntimeInit( CritterInfo critter_info, FishBendControllerData data ) {
    	data.lastRotation = critter_info.critterTransform.rotation;
//		WemoLog.Scott(data.lastRotation.eulerAngles);
		data.runtimeInitialized = true;		
	}

    public static void UpdateBend( CritterInfo critter_info ) {
		// check to see if you are using a static mesh instead of skinning and if so return
		if(critter_info.critterLODData.staticMesh) return;
		
		FishBendControllerData data = critter_info.critterBendData;
		 
		if( data == null 
			|| data.spineJointChain.firstTransform == null
			|| data.spineJointChain.lastTransform == null ) {
			return;
		}
		
		if( data.OFF )
        {
			return;
		}
		
		if( !data.runtimeInitialized ) {
			RuntimeInit( critter_info, data );
			return;
		}
			
        float dt = Time.deltaTime;
        if( critter_info.animBase.pauseActive ) {
            dt = 0f;
        }
		float abs_yaw_speed = Mathf.Abs(critter_info.critterSteering.GetMaxYawSpeed());
		data.accumTurningAngle += abs_yaw_speed * dt;
		if( data.forceStraightSpringRate
		   || data.accumTurningAngle > data.maxTurningSpringRateAngle 
		   || abs_yaw_speed <= 0.05f 
           || data.zeroOut) {
			data.forceStraightSpringRate = true;
			data.accumTurningAngle = 0f;
			data.desiredSpringRate = data.straightSpringRate;
//			DebugDisplay.AddDebugText("on");
		}
		else {
//			DebugDisplay.AddDebugText("turn");
			data.desiredSpringRate = data.turningSpringRate;
		}

		float inc = data.springRateSpeed * dt;
		if( data.springRate > data.desiredSpringRate ) {
			data.springRate -= inc;
			if( data.springRate < data.desiredSpringRate ) {
				data.springRate = data.desiredSpringRate;
			}
		}
		else if( data.springRate < data.desiredSpringRate ) {
			data.springRate += inc;			
			if( data.springRate > data.desiredSpringRate ) {
				data.springRate = data.desiredSpringRate;
			}
		}
		
/*		if( Input.anyKey ) {
			DebugDisplay.AddDebugText("s " + gmd.steeringYawSpeed + " : d " + data.desiredSpringRate + " :: r " + data.springRate);
		}*/

		// what is our frame difference..
        Quaternion cur = critter_info.cachedRotation;
        Quaternion diff = Quaternion.Inverse( Quaternion.Inverse( data.lastRotation ) * cur );
		bool all_ready = data.springRate == data.desiredSpringRate;
		critter_info.generalMotionData.turningSpeedAdjust = 0f;
		for( int i = 0; i < data.spineJointChain.chainLength; i++ ) {
			// diff gets recalculated below.
            Quaternion lr = data.spineJointChain.localRotations[i];
            lr = diff * lr;
	
            float angle = Quaternion.Angle(Quaternion.identity, lr);
            float new_angle = angle - (angle * data.springRate * dt);

			if( angle > data.forceStraightUnlockAngle ) {
				all_ready = false;
			}
			if( angle <= 0.0f || new_angle < 0.0f ) {
				angle = 0f;
                lr = Quaternion.identity;
			}
			else {

				if( new_angle > data.maxJointAngle ) {
					new_angle = data.maxJointAngle;
				}
				float ratio = 1.0f - (new_angle / angle);
                Quaternion new_rot = Quaternion.Slerp(lr, Quaternion.identity, ratio);
				if( i < ( data.spineJointChain.chainLength - 1 ) ) {
                    diff = Quaternion.Inverse( Quaternion.Inverse( lr ) * new_rot );
				}
                lr = new_rot;
			}
			
            float angle_diff = data.spineJointChain.lastLocalAngles[i] - angle;
			if( angle_diff > 0f ) { // we are decreasing our angle apply a forward motion to the critter.
				critter_info.generalMotionData.turningSpeedAdjust += ( angle_diff * data.perBoneDrivingForce * data.spineJointChain.distanceToParent[i] );
			}
			data.spineJointChain.lastLocalAngles[i] = angle;
	
//			WemoLog.Scott("post " + Quaternion.Angle(Quaternion.identity, data.spineJointChain.localRotations[i]));
			if( critter_info.critterAnimation == null ) {
                data.spineJointChain.transforms[i].localRotation = lr * data.spineJointChain.origLocalRotations[i];
			}
			else {
                data.spineJointChain.transforms[i].localRotation = lr * data.spineJointChain.transforms[i].localRotation;
			}

            data.spineJointChain.localRotations[i] = lr;
		}		
		
		if( critter_info.generalMotionData.turningSpeedAdjust > data.maxSpeedAddition ) {
			critter_info.generalMotionData.turningSpeedAdjust = data.maxSpeedAddition;
		}
			
		if( all_ready && data.forceStraightSpringRate ) {
//			DebugDisplay.AddDebugText("force straight off");
			data.forceStraightSpringRate = false;
			data.accumTurningAngle = 0f;
			
		}

		// update to our current rotation for the next frame
    	data.lastRotation = cur;
    }
    
    // The angle between dirA and dirB around axis
/*    public static float AngleAroundAxis (Vector3 dirA, Vector3 dirB, Vector3 axis) {
        // Project A and B onto the plane orthogonal target axis
        dirA = dirA - Vector3.Project(dirA, axis);
        dirB = dirB - Vector3.Project(dirB, axis);
        
        // Find (positive) angle between A and B
        float angle = Vector3.Angle(dirA, dirB);
        
        // Return angle multiplied with 1 or -1
        return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
    }*/
}
 