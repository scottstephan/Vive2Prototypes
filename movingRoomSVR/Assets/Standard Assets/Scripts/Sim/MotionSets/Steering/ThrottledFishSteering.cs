using UnityEngine;
using System.Collections;

public class ThrottledFishSteering : SteeringBase {
	
    protected float throttle;
    protected float throttleSpeed;
    protected bool throttleDecelActive = false;
	public float throttleMaxSpeed = 2f;
	public float throttleSpeedAccel = 8f;
	public float throttleSpeedDecel = 8f;
	
	
    protected float desiredYaw;
    protected float yawSpeed = 0f;
    protected float prevDesiredYaw;
    protected bool yawDecelActive;
	public float yawAccel = 180f;
	public float yawDecel = 180f;
	public float yawMaxSpeed = 270.0f;

    protected float desiredPitch;
    protected float pitchSpeed = 0f;
    protected float prevDesiredPitch;
    protected bool pitchDecelActive;
	public float pitchAccel = 180f;
	public float pitchDecel = 180f;
	public float pitchMaxSpeed = 110.0f;

    protected float desiredRoll;
    protected float prevDesiredRoll;

    protected float rollSpeed = 0f;
    protected bool rollDecelActive;	
	public float rollAccel = 220f;
	public float rollDecel = 220f;
	public float rollMaxSpeed = 270.0f;
	public float rollOnYawMult = 0f;
	public float rollStrafingMult = 0f;
	
    [HideInInspector]
    public bool scriptOverride;
    [HideInInspector]
    public float scriptOverrideTime;
    [HideInInspector]
    public float scriptThrottleMult;
    [HideInInspector]
    public float scriptYawMult;
    [HideInInspector]
    public float scriptPitchMult;
    [HideInInspector]
    public float scriptRollMult;


    public override void Reset()
    {
        Vector3 eulerAngles = critterInfo.critterTransform.rotation.eulerAngles;
        desiredRotation = critterInfo.critterTransform.rotation;
        pitchSpeed = 0f;
        yawSpeed = 0f;
        rollSpeed = 0f;
        desiredPitch = prevDesiredPitch = eulerAngles.x;
        desiredYaw = prevDesiredYaw = eulerAngles.y;
        desiredRoll = prevDesiredRoll = eulerAngles.z;
    }

	public override float GetYawSpeed() {
		return yawSpeed;
	}
	
	public override float GetPitchSpeed() {
		return pitchSpeed;
	}
	
	public override float GetMaxYawSpeed() {
		return yawMaxSpeed;
	}
	
	public override float GetMaxPitchSpeed() {
		return pitchMaxSpeed;
	}
	
	public override float GetMaxRollSpeed() {
		return rollMaxSpeed;
	}
	
	public override void SteerUpdate(float dt) {
		
        if( critterInfo == null) {
			return;
		}
        
        GeneralMotionData gmd = critterInfo.generalMotionData;

//		Profiler.BeginSample("ThrottledFishSteering");
        Vector3 eulerAngles = critterInfo.cachedEulers;
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
        if(critterInfo.generalSpeciesData.isStrafing)
		{
			//float strafingRoll = Vector3.Dot(gmd.desiredVelocityDirection.normalized, critter.critterTransform.forward) * 10f * gmd.steeringRollMult;
            strafingRoll = Vector3.Dot( gmd.desiredVelocityDirection, critterInfo.cachedRight) * rollStrafingMult;
			desiredRoll -= strafingRoll;
		}
		
        desiredRoll = Mathf.Clamp( desiredRoll, -60f, 60f );
        
		MathfExt.AccelDampDelt_Angle(desiredRoll,rollAccel,rollDecel,dt,rollMaxSpeed,ref rollSpeed,ref cur_roll,ref prevDesiredRoll,ref rollDecelActive);
		
		float final_roll = MathfExt.UnityEuler(cur_roll);
        critterInfo.critterTransform.rotation = Quaternion.Euler( final_pitch, final_yaw, final_roll  );
		
//		Profiler.EndSample();
	}

}
