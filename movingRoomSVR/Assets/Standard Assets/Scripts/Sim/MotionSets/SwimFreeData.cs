using UnityEngine;
using System.Collections;

public class SwimFreeData : BehaviorDataBase {
	
	public float priorityValue = 40f; //Value used by AI to determine priority of behaviors
	[HideInInspector]
	public float currentPriorityValue = 0f;
	
	public float swimSpeedMult = 1.0f;
	public float headingTimeMin = 4f;
	public float headingTimeMax = 15f;
	
	public float steeringThrottleMin = 0.01f;
	public float steeringThrottleMax = 0.7f;
	public float steeringThrottleBiasedValue = 0.05f;
	public float steeringThrottlePower = 3.0f;

	public float outsideBowlThrottleMin = 0.5f;
	public float outsideBowlThrottleMax = 0.8f;

	public float dirChangeMinYaw = 0f;
	public float dirChangeMaxYaw = 180f;
	public float dirChangeYawBiasedValue = 20f;
	public float dirChangeYawPower = 5f;
	
	public float dirChangeMinPitch = 0f;
	public float dirChangeMaxPitch = 45f;
	public float dirChangePitchBiasedValue = 0f;
	public float dirChangePitchPower = 5f;

    public AnimationClip lookAtAnim;
    [HideInInspector]
    public float lookAtTimer = 0f;   

    [HideInInspector]
    public float savedDirYValue = 0f;
    [HideInInspector]
    public Vector3 lastFishBowlPos;

	[HideInInspector]
	public float swimSpeed;
	[HideInInspector]
	public bool outside;
	[HideInInspector]
	public Vector3 desiredDirection;
	[HideInInspector]
	public float headingTimer = 0f;
	[HideInInspector]
	public bool outsideBowlDirectionSet = false;
}
