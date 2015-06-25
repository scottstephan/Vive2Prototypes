using UnityEngine;
using System.Collections;

public class SwimTargetedData : BehaviorDataBase 
{
	public float priorityValue = 80f; //Value used by AI to determine priority of behaviors
	[HideInInspector]
	public float currentPriorityValue = 0;
	
	public float followRadiusMult = 1.0f;
	public float swimSpeedMult = 1.0f;
	public float smoothRotateMult = 1.0f;

	//public Vector3 randomTargetCenter = new Vector3(700,-400,-1000);
	public Vector3 randomTargetOffset = new Vector3(600,400,600);
	public float randomTargetCameraDistanceMin = 200f;
	public float randomTargetCameraDistanceMax = 600f;
	
	
	public float speedMultDistMin = 0f;
	public float speedMultDistMax = 150f;
	public float speedMultMin = 0.1f;
	public float speedMultMax = 1f;
	
	
	public float radiusMultDistMin = 100f;
	public float radiusMultDistMax = 1500f;
	public float radiusMultMin = 1f;
	public float radiusMultMax = 5f;
	
	public float sinMotionFreqMult = 1f;
	public bool doSinMotion = false;

    public float EatTargetSeqStartDist = 400f;
    public float EatTargetLerpDist = 200f;
    public float EatTargetLerpTime = 0.5f;

	public float desiredSteeringThrottle = 0.6f;
	[HideInInspector]
	public float swimToSurfaceSpeedMult = 1f;
	
	[HideInInspector]
	public float followRadius;
	[HideInInspector]
	public float swimSpeed;
	[HideInInspector]
	public float smoothRotate;

	[HideInInspector]
	public Vector3 targetPosition;
	[HideInInspector]
	public Vector3 savedTargetDirection;
	[HideInInspector]
	public bool isTargetFood;
	[HideInInspector]
	public Transform movingTarget;
	[HideInInspector]
	public GeneralSpeciesData targetGSD;
	[HideInInspector]
	public DeadData targetDeadData;
	[HideInInspector]
	public Transform myBitePosition;
    [HideInInspector]
    public float targetLerp;

	public bool HasScriptTarget() { return targetGSD != null; }

	[HideInInspector]
	public float lastDistToTarget;	
	[HideInInspector]
	public Vector3 posOffset;
	[HideInInspector]
	public Vector3 posLast;
	[HideInInspector]
	public Vector3 randOffsetSpeed;
	
}

