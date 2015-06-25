using UnityEngine;
using System.Collections;

public class OGFishSteering : SteeringBase {
	
	private float desiredPitch;
	private float prevDesiredPitch;
	private bool pitchDecelActive;
	private float desiredYaw;
	private float prevDesiredYaw;
	private bool yawDecelActive;
	private float steeringPitchSpeed = 0f;
	private float steeringYawSpeed = 0f;

	public float steeringYawAccel = 180f;
	public float steeringYawDecel = 2160f;
	public float steeringYawMaxSpeed = 270.0f;
	public float steeringPitchAccel = 180f;
	public float steeringPitchDecel = 2160f;
	public float steeringPitchMaxSpeed = 270.0f;
	public float steeringRollAccel = 220f;
	public float steeringRollDecel = 220f;
	public float steeringRollMaxSpeed = 270.0f;
	
	// Roll vars
//	private float steeringRoll = 0f;
	private float desiredRoll;
	private float prevDesiredRoll;
	private bool rollDecelActive;
	private float steeringRollSpeed = 0f;
	public float steeringRollMult = 0f;
	public float steeringRollStrafingMult = 0f;

	public override float GetYawSpeed() {
		return steeringYawSpeed;
	}
	
	public override float GetPitchSpeed() {
		return steeringPitchSpeed;
	}
	
	public override float GetMaxYawSpeed() {
		return steeringYawMaxSpeed;
	}
	
	public override float GetMaxPitchSpeed() {
		return steeringPitchMaxSpeed;
	}
	
	public override float GetMaxRollSpeed() {
		return steeringRollMaxSpeed;
	}
	
	public static void CreateFromGeneralMotion( CritterInfo critter_info ) {
		OGFishSteering og_steer = critter_info.critterObject.AddComponent<OGFishSteering>();

		GeneralMotionData gmd = critter_info.generalMotionData;
		
		og_steer.steeringYawAccel = gmd.steeringYawAccel;
		og_steer.steeringYawDecel = gmd.steeringYawDecel;
		og_steer.steeringYawMaxSpeed = gmd.steeringYawMaxSpeed;
		og_steer.steeringPitchAccel = gmd.steeringPitchAccel;
		og_steer.steeringPitchDecel = gmd.steeringPitchDecel;
		og_steer.steeringPitchMaxSpeed = gmd.steeringPitchMaxSpeed;
		og_steer.steeringRollAccel = gmd.steeringRollAccel;
		og_steer.steeringRollDecel = gmd.steeringRollDecel;
		og_steer.steeringRollMaxSpeed = gmd.steeringRollMaxSpeed;
		og_steer.steeringRollMult = gmd.steeringRollMult;
		og_steer.steeringRollStrafingMult = gmd.steeringRollStrafingMult;

		critter_info.critterSteering = og_steer;
	}
	
    public static void CreateFromGeneralMotion( GameObject critter, CritterInfoData critter_info ) {
        OGFishSteering og_steer = critter.AddComponent<OGFishSteering>();
        
        GeneralMotionData gmd = critter_info.generalMotionData;        
        og_steer.steeringYawAccel = gmd.steeringYawAccel;
        og_steer.steeringYawDecel = gmd.steeringYawDecel;
        og_steer.steeringYawMaxSpeed = gmd.steeringYawMaxSpeed;
        og_steer.steeringPitchAccel = gmd.steeringPitchAccel;
        og_steer.steeringPitchDecel = gmd.steeringPitchDecel;
        og_steer.steeringPitchMaxSpeed = gmd.steeringPitchMaxSpeed;
        og_steer.steeringRollAccel = gmd.steeringRollAccel;
        og_steer.steeringRollDecel = gmd.steeringRollDecel;
        og_steer.steeringRollMaxSpeed = gmd.steeringRollMaxSpeed;
        og_steer.steeringRollMult = gmd.steeringRollMult;
        og_steer.steeringRollStrafingMult = gmd.steeringRollStrafingMult;
        
        critter_info.critterSteering = og_steer;
    }

    // not sure init is needed yet.
	public override void Init() {
		
	}
	
	public override void SteerUpdate(float dt) {
//		Profiler.BeginSample("OGFishSteeringControl");
		
		CritterInfo critter = critterInfo;
        GeneralMotionData gmd = critter.generalMotionData;
		float raw_cur_yaw = critter.critterTransform.rotation.eulerAngles.y;
		float raw_cur_pitch = critter.critterTransform.rotation.eulerAngles.x;
		float cur_yaw = MathfExt.RegularEuler(raw_cur_yaw);
		float cur_pitch = MathfExt.RegularEuler(raw_cur_pitch);
		float raw_desiredYaw = desiredRotation.eulerAngles.y;
		float raw_desiredPitch = desiredRotation.eulerAngles.x;
		desiredYaw = MathfExt.RegularEuler(raw_desiredYaw);
		desiredPitch = Mathf.Clamp(MathfExt.RegularEuler(raw_desiredPitch),-70f,70f);
		
		float use_pa = steeringPitchAccel;
		float use_pd = steeringPitchDecel;
		float use_ya = steeringYawAccel;
		float use_yd = steeringYawDecel;
//		if( gmd.isDispersed || gmd.avoidFish ) {
		if( desiredSteeringThrottle > 0.75f ) {
			use_pa *= 3f;
			use_pd *= 3f;
			use_ya *= 3f;
			use_yd *= 3f;
		}
		
		MathfExt.AccelDampDelt_Angle(desiredPitch,use_pa,use_pd,dt,steeringPitchMaxSpeed,ref steeringPitchSpeed,ref cur_pitch, ref prevDesiredPitch, ref pitchDecelActive);
		MathfExt.AccelDampDelt_Angle(desiredYaw,use_ya,use_yd,dt,steeringYawMaxSpeed,ref steeringYawSpeed,ref cur_yaw, ref prevDesiredYaw, ref yawDecelActive);
		
		if( cur_pitch < -70f ) {
			if( cur_pitch < -80f ) {
				cur_pitch = -80f;
			}

			if( steeringPitchSpeed < 0f ) {
				steeringPitchSpeed *= 0.9f;
			}
		}
		else if( cur_pitch > 70f ) {
			if( cur_pitch > 80f ) {
				cur_pitch = 80f;
			}
			if( steeringPitchSpeed > 0f ) {
				steeringPitchSpeed *= 0.9f;
			}
		}
		
		float final_yaw = MathfExt.UnityEuler(cur_yaw);
		float final_pitch = MathfExt.UnityEuler(cur_pitch);
		
		desiredRoll = steeringRollMult * (raw_cur_yaw - final_yaw);
		float raw_cur_roll = critter.critterTransform.rotation.eulerAngles.z;
		float cur_roll = MathfExt.RegularEuler(raw_cur_roll);
			
		float strafingRoll = 0f;
		if(critter.generalSpeciesData.isStrafing)
		{
			//float strafingRoll = Vector3.Dot(gmd.desiredVelocityDirection.normalized, critter.critterTransform.forward) * 10f * gmd.steeringRollMult;
			strafingRoll = Vector3.Dot( gmd.desiredVelocityDirection, critter.critterTransform.right) * steeringRollStrafingMult;
			desiredRoll += strafingRoll;
		}	
		
		MathfExt.AccelDampDelt_Angle(desiredRoll,steeringRollAccel,steeringRollDecel,dt,steeringRollMaxSpeed,ref steeringRollSpeed,ref cur_roll, ref prevDesiredRoll, ref rollDecelActive);
		
		float final_roll = MathfExt.UnityEuler(cur_roll);
		//final_roll = Mathf.Clamp(final_roll,-90f,90f);
		

		critter.critterTransform.rotation = Quaternion.Euler( new Vector3( final_pitch, final_yaw, final_roll ) );
		
/*		string massive_output = critter.critterObject.name + ":: fy " + final_yaw + " : fp " + final_pitch;
		massive_output += " cy " + cur_yaw + " : cp " + cur_pitch;
		massive_output += " rcy " + raw_cur_yaw + " : rcp " + raw_cur_pitch;
		massive_output += " ogy " + og_cur_yaw + " : ogp " + og_cur_pitch;
		massive_output += " rdy " + raw_desiredYaw + " : rdp " + raw_desiredPitch;
		massive_output += " dy " + gmd.desiredYaw + " : dp " + gmd.desiredPitch;
		Debug.Log(massive_output);*/
//		Profiler.EndSample();

	}

}
