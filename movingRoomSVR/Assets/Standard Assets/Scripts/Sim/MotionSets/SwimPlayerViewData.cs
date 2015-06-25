using UnityEngine;
using System.Collections;

public class SwimPlayerViewData : BehaviorDataBase 
{
    public float goalDistance = 35f;
    public float moveInDirTimeMin = 3.5f;
    public float moveInDirTimeMax = 7f;
    public float dotStay = 0.9f;
    public bool allowRotate = true;

    [HideInInspector]
    public float timer = 0f;
    
    [HideInInspector]
    public bool startMe = false;

    [HideInInspector]
    public bool moveInDirStarted;
    [HideInInspector]
    public bool moveInViewRight;
    [HideInInspector]
    public float moveInDirTimer;
    [HideInInspector]
    public Vector3 moveDir;

    [HideInInspector]
    public int prevForceLOD = -1;

    [HideInInspector]
    public bool started;
    [HideInInspector]
    public int prevSingletonBehavior = -1;
}
