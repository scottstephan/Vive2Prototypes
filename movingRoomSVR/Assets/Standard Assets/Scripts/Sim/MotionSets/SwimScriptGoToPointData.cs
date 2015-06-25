using UnityEngine;
using System.Collections;

public class SwimScriptGoToPointData : BehaviorDataBase 
{
    public enum State 
    {
        Arriving,
        Following
    }

    public float priorityValue = 200f; //Value used by AI to determine priority of behaviors

    [HideInInspector]
    public Transform goToTransform;
    [HideInInspector]
    public float arrivalDistance;
    [HideInInspector]
    public float swimSpeedMult;

    [HideInInspector]
    public bool enableAvoidFish;
    [HideInInspector]
    public bool enableAvoidGround;
    [HideInInspector]
    public bool enableCritterDisperse;
    [HideInInspector]
    public bool enablePlayerDisperse;
    [HideInInspector]
    public bool disableCritterGroundCollision;
}

