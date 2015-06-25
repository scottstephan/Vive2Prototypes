using UnityEngine;
using System.Collections;


public class SwimDisperseData : BehaviorDataBase 
{
	public float priorityValue = 100f; //Value used by AI to determine priority of behaviors

	[HideInInspector]
	public float currentPriorityValue = 0;

	public float desiredSpeedMult = 2.0f;
    public float desiredSteeringThrottle = 1.0f;
    public float thresholdColliderDistMult = 20f;
    public float maxPredatorSpeedMult = 3f;

	[HideInInspector]
	public Vector3 pointOrig;

    [HideInInspector]
    public float extraSpeedMult = 1f;
    [HideInInspector]
    public float extraColliderDistMult = 1f;

    [HideInInspector]
    public int playerDisperseDisableCount = 0;
    [HideInInspector]
    public int critterDisperseDisableCount = 0;

    public bool disperseFromMovingTarget;

    [HideInInspector]
    public bool checkMovingTarget;

    [HideInInspector]
    public bool useBounds;
    [HideInInspector]
    public Bounds bounds;
}


