using UnityEngine;
using System.Collections;

public class CircleAroundObjectData : BehaviorDataBase {
	
	public float priorityValue = 1000f;
	
	public float swimSpeed = 60f;
	public float swimAccelOverride = 400.0f;	    
	public float steeringThrottle = 1f;  

    [HideInInspector]
    public Transform targetTransform = null;

    [HideInInspector]
	public bool heightAdjustActive = false;
    public bool adjustsToHeight = true;
    public float allowedHeightDifference = 10f;

    [HideInInspector]
	public Vector3 prevTargetPosition = Vector3.zero;

	[HideInInspector]
	public float circleTime = -1f;
	[HideInInspector]
	public float circleDir = 1f;
	
	public float circleRadius = 15f;
	
	public float radialSpeed = 15f;

	public GenericDelegate CircleTimeExpired;	
	
	[HideInInspector]
	public float savedMaxAccel;

	public bool useSteeringOverrides = false;

	// throttled steering overrides.
	public float throttleMaxSpeed = 16f;
	public float throttleSpeedAccel = 48f;
	public float throttleSpeedDecel = 48f;
	[HideInInspector]
	public float savedThrottleMaxSpeed;
	[HideInInspector]
	public float savedThrottleSpeedAccel;
	[HideInInspector]
	public float savedThrottleSpeedDecel;
		
	public float yawAccel = 6048f;
	public float yawDecel = 6048f;
	public float yawMaxSpeed = 2048.0f;
	[HideInInspector]
	public float savedYawAccel;
	[HideInInspector]
	public float savedYawDecel;
	[HideInInspector]
	public float savedYawMaxSpeed;

	public float pitchAccel = 180f;
	public float pitchDecel = 180f;
	public float pitchMaxSpeed = 110.0f;
	[HideInInspector]
	public float savedPitchAccel;
	[HideInInspector]
	public float savedPitchDecel;
	[HideInInspector]
	public float savedPitchMaxSpeed;

	public float rollAccel = 220f;
	public float rollDecel = 220f;
	public float rollMaxSpeed = 270.0f;
	public float rollOnYawMult = 0f;
	public float rollStrafingMult = 0f;
	[HideInInspector]
	public float savedRollAccel;
	[HideInInspector]
	public float savedRollDecel;
	[HideInInspector]
	public float savedRollMaxSpeed;
	[HideInInspector]
	public float savedRollOnYawMult;
	[HideInInspector]
	public float savedRollStrafingMult;	
}
