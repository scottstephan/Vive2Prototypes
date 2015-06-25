using UnityEngine;
using System.Collections;

public class SwimFreefallData : BehaviorDataBase {
	public int priorityValue = 120; //Value used by AI to determine priority of behaviors
	[HideInInspector]
	public float currentPriorityValue = 0;
	
	[HideInInspector]
	public bool becameAirborne = false;
}
