using UnityEngine;
using System.Collections;

public class SwimStrafePlayerData : BehaviorDataBase 
{
    public enum StrafePlayerState
    {
        Approach,
        Switching,
        Locked
    }

	public float priorityValue = 80f; //Value used by AI to determine priority of behaviors

    [HideInInspector]
	public float currentPriorityValue = 0;
	
    public float desiredThrottle = 1f;
    public float approachSpeedMult = 2.0f;

    [HideInInspector]
    public StrafePlayerState strafeState = StrafePlayerState.Approach;
	
    public float switchLocationTimeMin = 2.0f;
    public float switchLocationTimeMax = 10.0f;
    public float switchSideChance = 0.5f;
    public float switchFwdColliderDistMult = 30f;

    public float positioningTimeMin = 8.0f;
    public float positioningTimeMax = 10.0f;

    public float colliderBoxRadiusMult = 2.0f;

    [HideInInspector]
    public float curTime = 10.0f;
    [HideInInspector]
    public float curTimeLimit = 10.0f;
    [HideInInspector]
    public float curForwardDistance = 50.0f;
    [HideInInspector]
    public float curHorizDistance = 50.0f;
    [HideInInspector]
    public float curVertDistance = 20.0f;

    [HideInInspector]
    public float forwardDistanceMin = 50f;
    [HideInInspector]
    public float forwardDistanceMax = 250f;
    [HideInInspector]
    public float horizDistanceMin = 50f;
    [HideInInspector]
    public float horizDistanceMax = 200f;
    [HideInInspector]
    public float vertDistanceMin = 20f;
    [HideInInspector]
    public float vertDistanceMax = 100f;

    public float lockDistance = 100.0f;

    [HideInInspector]
    public Vector3 desiredDirection;
    [HideInInspector]
    public bool bStrafeRight;

    [HideInInspector]
    public Vector3 prevPosition;
    [HideInInspector]
    public float prevSpeed;


}

