using UnityEngine;
using System.Collections;

public class InteractionData : BehaviorDataBase {
	
	public float priorityValue = 1000f; //Value used by AI to determine priority of behaviors
	
	public float swimSpeedMult = 1.0f;	    
	public float swimAccelOverride = 400.0f;	    
	public float steeringThrottle = 0.2f;    
	public float touchDelayToRotate = 4.0f;
	
	[HideInInspector]
	public float delayToRotate = 0.0f;
	
	[HideInInspector]
	public float swimSpeed;
	[HideInInspector]
	public Vector3 desiredDirection;	

	public float startledTimePerTap = 0.4f;
	public float startledSpeedMult = 6f;
	public float startledMaxRadFactor = 90f;
	[HideInInspector]
	public bool startled = false;
	[HideInInspector]
	public int startledTapCount = 0;
	[HideInInspector]
	public int startledTapIndex = 0;
	[HideInInspector]
	public float startledTime;
	[HideInInspector]
	public float savedMaxAccel;
		
	// throttled steering overrides.
	public float throttleMaxSpeed = 2f;
	public float throttleSpeedAccel = 8f;
	public float throttleSpeedDecel = 8f;
	[HideInInspector]
	public float savedThrottleMaxSpeed;
	[HideInInspector]
	public float savedThrottleSpeedAccel;
	[HideInInspector]
	public float savedThrottleSpeedDecel;
		
	public float yawAccel = 180f;
	public float yawDecel = 180f;
	public float yawMaxSpeed = 270.0f;
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
	
	
	public int happynessInc = 2;
	public float happynessHoldTime = 0.5f;
	[HideInInspector]
	public float happynessHoldTimer = 0f;
}
