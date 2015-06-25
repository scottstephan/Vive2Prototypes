using UnityEngine;
using System.Collections;

public class SwimIdleData : BehaviorDataBase 
{
	public float priorityValue = 40f; //Value used by AI to determine priority of behaviors
	[HideInInspector]
	public float currentPriorityValue = 0;
	
	public float followRadiusMult = 0.4f;
	public float swimSpeedMult = 0.8f;
	public float smoothRotateMult = 0.2f;
	public float idleTime = 10.0f;
	public float idleTimeRandom = 4.0f;
	public float switchPosFreq = 25.0f;
	public float switchPosRandom = 10.0f;

	public float desiredSteeringThrottle = 0.65f;
	
	public int stopIdleWhenPoked = 30;
	[HideInInspector]
	public int poked;
	
	public float speedMultDistMin = 0f;
	public float speedMultDistMax = 150f;
	public float speedMultMin = 0.1f;
	public float speedMultMax = 1f;
		
	public Vector3 randOffsetMult = new Vector3(0.4f,1f,0.4f);
	public float sinMotionFreqMult = 1.0f;
	
	[HideInInspector]
	public float followRadius;
	[HideInInspector]
	public float swimSpeed;
	[HideInInspector]
	public float smoothRotate;

	[HideInInspector]
	public Vector3 targetPosition;
	[HideInInspector]
	public Vector3 desiredDir;
	
	[HideInInspector]
	public Vector3 posOffset;
	[HideInInspector]
	public Vector3 posLast;
	[HideInInspector]
	public Vector3 randOffsetSpeed;
	
	
	[HideInInspector]
	public float idleTimer;
	[HideInInspector]
	public float switchPosTimer;
}

