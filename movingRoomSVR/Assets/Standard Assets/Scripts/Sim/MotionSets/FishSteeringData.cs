using UnityEngine;
using System.Collections;

public class FishSteeringData : MonoBehaviour {
	[HideInInspector]
	public float desiredPitch;
	[HideInInspector]
	public float desiredYaw;
	[HideInInspector]
	public float steeringPitchSpeed = 0f;
	[HideInInspector]
	public float steeringYawSpeed = 0f;

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
	[HideInInspector]
	public float steeringRoll = 0f;
	[HideInInspector]
	public float desiredRoll;
	[HideInInspector]
	public float steeringRollSpeed = 0f;
	public float steeringRollMult = 0f;
	public float steeringRollStrafingMult = 0f;
}
