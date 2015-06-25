using UnityEngine;
using System.Collections;

public class SwimStrafingData : BehaviorDataBase {
	public float priorityValue = 40f; //Value used by AI to determine priority of behaviors
	[HideInInspector]
	public float currentPriorityValue = 0;

	public float desiredSteeringThrottle = 0.35f;
	public float swimSpeedMult = 1f;
	public float timerOffsetMin = 2f;
	public float timerOffsetMax = 5f;
    public float maxXDirOffset = 3f;
    public float maxXDirPower = 2f;
    public float xMaxSpeed = 10f;
    public float xAccel = 10f;
    public float xDecel = 10f;
    [HideInInspector]
    public float xSpeed;
    [HideInInspector]
    public bool xDecelActive;
	[HideInInspector]
	public float swimSpeed;
    [HideInInspector]
    public float desiredRandomX = 0f;
    [HideInInspector]
    public float randomX;
	[HideInInspector]
	public float randomXTimer;	
}
