using UnityEngine;
using System.Collections;

public class SwimParkingData : BehaviorDataBase {
	
	public float priorityValue = 70f; //Value used by AI to determine priority of behaviors
	[HideInInspector]
	public float currentPriorityValue = 0;
	
	public float parkingIncrement = 0f;
	public float parkingTimeMin = 5f;
	public float parkingTimeMax = 25f;
	
	public float swimSpeedMult = 1f;
	[HideInInspector]
	public float swimSpeed;
	[HideInInspector]
	public Vector3 targetPosition;

	
	[HideInInspector]
	public bool isParked = false;
	[HideInInspector]
	public float parkingTimer = 0f;
    [HideInInspector]
    public bool doneParking = false;
    [HideInInspector]
    public bool okToEnd = false;
	
	[HideInInspector]
	public ParkingSpot spot;
	
	
	[HideInInspector]
	public float parkingLevel = 1.0f;
	[HideInInspector]
	public bool parkingNeeded = false;
}
