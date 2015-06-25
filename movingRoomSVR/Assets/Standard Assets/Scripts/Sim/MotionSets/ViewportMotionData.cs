using UnityEngine;
using System.Collections;


public class ViewportMotionData : BehaviorDataBase {
	public GenericDelegate MoveComplete = null;
	
	// this behavior is self contained and thus does not currently use the priority value.
	public float priorityValue = 0f; //Value used by AI to determine priority of behaviors
		
	[HideInInspector]
	public Vector3 desiredViewportPosition;
	[HideInInspector]
	public Vector3 currentViewportPosition;
	[HideInInspector]
	public Quaternion desiredEndRotation;
	
	[HideInInspector]
	public float delayForceTime = -1f;
	
	[HideInInspector]
	public float moveTime = -1f;
	[HideInInspector]
	public float moveTimeTotal;
	[HideInInspector]
	public Vector3 moveOGPosition;
	[HideInInspector]
	public Quaternion moveOGRotation;
	
	[HideInInspector]
	public Vector3 regularMoveDir;
	[HideInInspector]
	public float regularMoveTime = -1f;
	[HideInInspector]
	public float regularMoveSpeed = 4f;
	[HideInInspector]
	public float regularMoveDecay = 0f;
	[HideInInspector]
	public bool regularMoveDoRotation = true;
	
	[HideInInspector]
	public float stalledTime = -1f;
	
	[HideInInspector]
	public bool stalledForAnim = false;
}
