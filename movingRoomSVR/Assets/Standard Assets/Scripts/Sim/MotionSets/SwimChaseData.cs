using UnityEngine;
using System.Collections;


public class SwimChaseData : BehaviorDataBase 
{
	public float priorityValue = 30f; //Value used by AI to determine priority of behaviors
	[HideInInspector]
	public float currentPriorityValue = 0;
	
	public float searchForVictim = 40f;
	public float chaseRadius = 100f;
	public float relativeSizeToAttack = 2f;
	
	public float swimSpeedMult = 1.0f;
	public float speedMultDistMin = 0f;
	public float speedMultDistMax = 150f;
	public float speedMultMin = 0.1f;
	public float speedMultMax = 1f;
	public float maxChaseTime = 3f;
	public float desiredSteeringThrottle = 0.9f;

    [HideInInspector]
    public string chaseDesignGroupName;

	[HideInInspector]
	public float swimSpeed;
	[HideInInspector]
	public Vector3 pointOrig;
	[HideInInspector]
	public int checkCounter;
	[HideInInspector]
	public CritterInfo victim = null;
	[HideInInspector]
	public bool returnToPointOrig = false;
	[HideInInspector]
	public float chaseStartTime = 0f;
}


