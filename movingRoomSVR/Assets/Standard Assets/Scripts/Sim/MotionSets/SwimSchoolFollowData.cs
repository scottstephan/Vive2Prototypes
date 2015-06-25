using UnityEngine;
using System.Collections;

public class SwimSchoolFollowData : BehaviorDataBase 
{
	public enum SchoolState
    {
        Automatic,
        Manual,
        None
    }

	public float priorityValue = 50f; //Value used by AI to determine priority of behaviors
	[HideInInspector]
	public float currentPriorityValue = 0;
	
	public float followRadiusMult = 1.0f;
    public float swimSpeedMult = 1.2f;
	public float smoothRotateMult = 1.0f;
	public float randOffsetMult = 1.0f;
	public float offsetMultY = 0.35f;
    public float followColliderRadiusMult = 1.2f;

    //public bool useAvoidance = false;
    public float schoolAvoidPushMult = 1.0f;

	public float sinMotionFreqMult = 1.0f;
	
	public float desiredSteeringThrottle = 0.5f;
		
	[HideInInspector]
	public float followRadius;

    [HideInInspector]
    public float scriptFollowRadius = -1;
    [HideInInspector]
    public float oldFollowRadius = 0f;

	[HideInInspector]
	public float swimSpeed;
	[HideInInspector]
	public float smoothRotate;
	
	
	[HideInInspector]
	public Vector3 posOffset;
	[HideInInspector]
	public Vector3 posLast;
	[HideInInspector]
	public Vector3 randOffsetSpeed;

    [HideInInspector]
    public SchoolState state = SchoolState.Automatic;

	[HideInInspector]
	public Transform leaderTransform;
    [HideInInspector]
    public Vector3 leaderOffset;

    //[HideInInspector]
	//public int isLeaderInt = 0;
	
	[HideInInspector]
	public CritterInfo leaderCritterInfo;

}

