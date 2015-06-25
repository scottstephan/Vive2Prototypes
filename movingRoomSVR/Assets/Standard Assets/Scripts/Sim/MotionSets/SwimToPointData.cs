using UnityEngine;
using System.Collections;

public enum PointReachedType {
	EnterTargetedMotion,
	ExitScene,
}

public class SwimToPointData : BehaviorDataBase 
{
	public int priorityValue = 90; //Value used by AI to determine priority of behaviors
	[HideInInspector]
	public float currentPriorityValue = 0;
	
	public float swimSpeedMult = 1.0f;
	public float smoothRotateMult = 1.0f;

	public float speedMultDistMin = 0f;
	public float speedMultDistMax = 150f;
	public float speedMultMin = 0.1f;
	public float speedMultMax = 1f;
	
	public float desiredSteeringThrottle = 0.4f;
	
	[HideInInspector]
	public bool exitDirSet = false;
	
	[HideInInspector]
	public float swimSpeed;
	[HideInInspector]
	public float smoothRotate;

	[HideInInspector]
	public Vector3 targetPosition;
	
	[HideInInspector]
	public PointReachedType pointReachedType;	

	[HideInInspector]
	public Vector3 savedTargetDirection;

	[HideInInspector]
	public float blockCameraSwitchTimer = 10f;
	
	[HideInInspector]
	public bool cameraFadeStarted = false;	
	
	[HideInInspector]
	public bool staticCameraSwap = false;	

	[HideInInspector]
	public float staticCameraSwapTimer = 1f;
	
}


