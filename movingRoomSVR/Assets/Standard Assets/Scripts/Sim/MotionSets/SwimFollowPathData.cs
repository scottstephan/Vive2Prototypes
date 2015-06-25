using UnityEngine;
using System.Collections;

public class SwimFollowPathData : BehaviorDataBase 
{
    public enum State 
    {
        Arriving,
        Following
    }

    public float priorityValue = 200f; //Value used by AI to determine priority of behaviors

    [HideInInspector]
    public GameObject followObject;
    [HideInInspector]
    public AnimationClip followAnimClip;
    [HideInInspector]
    public Animation followAnim;
    [HideInInspector]
    public Transform followTransform;
    [HideInInspector]
    public State state;
    [HideInInspector]
    public float arrivalDistance;
    [HideInInspector]
    public float followDistance;
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
    [HideInInspector]
    public Vector3 lastFollowPos;
    [HideInInspector]
    public Vector3 lastMoveDir;
}

