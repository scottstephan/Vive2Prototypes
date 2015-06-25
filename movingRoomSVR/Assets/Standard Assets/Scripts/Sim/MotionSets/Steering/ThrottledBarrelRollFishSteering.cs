using UnityEngine;
using System.Collections;

public class ThrottledBarrelRollFishSteering : ThrottledFishSteering {

    public float barrelRollSpeed = 15f;
    public float barrelRollAccel = 10f;
    public float barrelRollDecel = 10f;
    private float barrelRollStart;
    private float barrelRollAccum;

    [HideInInspector]
    public bool barrelRoll;
    private bool wasBarrelRoll;

	public override void SteerUpdate(float dt) {
		
		CritterInfo critter = critterInfo;
		if( critter == null || critter.critterTransform == null ) {
			return;
		}
        
        GeneralMotionData gmd = critter.generalMotionData;

//		Profiler.BeginSample("ThrottledFishSteering");
        Vector3 eulerAngles = critter.critterTransform.rotation.eulerAngles;
        Vector3 desiredEulers = desiredRotation.eulerAngles;
		float raw_cur_yaw = eulerAngles.y;
		float raw_cur_pitch = eulerAngles.x;
		float cur_yaw = MathfExt.RegularEuler(raw_cur_yaw);
		float cur_pitch = MathfExt.RegularEuler(raw_cur_pitch);
        float raw_desiredYaw = desiredEulers.y;
        float raw_desiredPitch = desiredEulers.x;

		desiredYaw = MathfExt.RegularEuler(raw_desiredYaw);
		desiredPitch = Mathf.Clamp(MathfExt.RegularEuler(raw_desiredPitch),-70f,70f);
		
		MathfExt.AccelDampDelt(desiredSteeringThrottle,throttleSpeedAccel,throttleSpeedDecel,dt, throttleMaxSpeed,ref throttleSpeed, ref throttle, ref throttleDecelActive);
		if( throttle < 0f || throttle > 1f ) {
			throttle = Mathf.Clamp01(throttle);
			throttleSpeed = 0f;
			throttleDecelActive = false;
		}
		
		float max_pitch_speed = pitchMaxSpeed * throttle;
		MathfExt.AccelDampDelt_Angle(desiredPitch,pitchAccel,pitchDecel,dt,max_pitch_speed,ref pitchSpeed,ref cur_pitch, ref prevDesiredPitch, ref pitchDecelActive);
//		Debug.Log("desiredPitch " + desiredPitch + " :: cur_pitch " + cur_pitch + " :: mxpitchspd " + max_pitch_speed + " :: pitchspd " + pitchSpeed );

		float max_yaw_speed = yawMaxSpeed * throttle;
		MathfExt.AccelDampDelt_Angle(desiredYaw,yawAccel,yawDecel,dt,max_yaw_speed,ref yawSpeed,ref cur_yaw, ref prevDesiredYaw, ref yawDecelActive);
//		Debug.Log("desiredYaw " + desiredYaw + " :: cur_yaw " + cur_yaw + " :: mxyawspd " + max_yaw_speed + " :: yawspd " + yawSpeed );
		if( cur_pitch < -70f ) {
			if( cur_pitch < -80f ) {
				cur_pitch = -80f;
			}

			if( pitchSpeed < 0f ) {
				pitchSpeed *= 0.9f;
			}
		}
		else if( cur_pitch > 70f ) {
			if( cur_pitch > 80f ) {
				cur_pitch = 80f;
			}
			if( pitchSpeed > 0f ) {
				pitchSpeed *= 0.9f;
			}
		}
		
		float final_yaw = MathfExt.UnityEuler(cur_yaw);
		float final_pitch = MathfExt.UnityEuler(cur_pitch);
		
		desiredRoll = rollOnYawMult * (raw_cur_yaw - final_yaw);

		float raw_cur_roll = eulerAngles.z;
		float cur_roll = MathfExt.RegularEuler(raw_cur_roll);
			
		float strafingRoll = 0f;
		if(critter.generalSpeciesData.isStrafing)
		{
			//float strafingRoll = Vector3.Dot(gmd.desiredVelocityDirection.normalized, critter.critterTransform.forward) * 10f * gmd.steeringRollMult;
            strafingRoll = Vector3.Dot( gmd.desiredVelocityDirection, critter.critterTransform.right) * rollStrafingMult;
			desiredRoll -= strafingRoll;
		}
		
        float desiredRollAccel = rollAccel;
        float desiredRollDecel = rollDecel;
        float desiredRollMaxSpeed = rollMaxSpeed;

        if (barrelRoll)
        {
            if (!wasBarrelRoll)
            {
                wasBarrelRoll = true;
                barrelRollStart = cur_roll;
                barrelRollAccum = 0f;

                if (critter.critterBendData != null)
                {
                    critter.critterBendData.zeroOut = true;
                }
            }
            
            desiredRoll = cur_roll + ((barrelRollStart > 0f) ? barrelRollSpeed : -barrelRollSpeed) * dt;
            desiredRollMaxSpeed = barrelRollSpeed;
            desiredRollAccel = barrelRollAccel;
            desiredRollDecel = barrelRollDecel;
        }
        else if (wasBarrelRoll)
        {
            wasBarrelRoll = false;
            desiredRoll = Mathf.Clamp( desiredRoll, -60f, 60f );
            if (critter.critterBendData != null)
            {
                critter.critterBendData.zeroOut = false;
            }
        }
        
        float prev_roll = cur_roll;
        MathfExt.AccelDampDelt_Angle(desiredRoll,desiredRollAccel,desiredRollDecel,dt,desiredRollMaxSpeed,ref rollSpeed,ref cur_roll,ref prevDesiredRoll,ref rollDecelActive);
		
		float final_roll = MathfExt.UnityEuler(cur_roll);
		critter.critterTransform.rotation = Quaternion.Euler( new Vector3( final_pitch, final_yaw, final_roll ) );
		
        if (barrelRoll)
        {
            barrelRollAccum += Mathf.Abs (Mathf.DeltaAngle(prev_roll,cur_roll));
            if (barrelRollAccum >= 360f)
            {
                barrelRoll = false;
            }
        }

        //		Profiler.EndSample();
	}

}
