
using UnityEngine;
using System.Collections;

[System.Serializable]
public class BendingJointChain {
    public Transform firstTransform;
    public Transform lastTransform;

	internal int chainLength;
    internal Transform[] transforms;
    internal float[] distanceToParent;
    internal float[] lastLocalAngles;
	internal Quaternion[] localRotations;
    internal Quaternion[] origLocalRotations;
}

public class FishBendControllerData : MonoBehaviour {
    public bool OFF;

    [HideInInspector]
    public bool zeroOut;

	public Transform rootNode;
    public BendingJointChain spineJointChain;
	public float maxJointAngle = 40.0f;
	public float springRate = 10.0f;
	public float turningSpringRate = 4.0f;
	public float straightSpringRate = 15.0f;
	public float springRateSpeed = 100f;
	
	[HideInInspector]
	public float accumTurningAngle = 0f;
	[HideInInspector]
	public bool forceStraightSpringRate = false;
	
	public float maxTurningSpringRateAngle = 30f;
	public float forceStraightUnlockAngle = 5f;
		
	[HideInInspector]
	public float desiredSpringRate = 15f;
	
	[HideInInspector]
	public Quaternion lastRotation;
	
	public float perBoneDrivingForce = 0.17f;
	public float maxSpeedAddPercentage = 0.25f;
	[HideInInspector]
	public float maxSpeedAddition;
	
	[HideInInspector]
	public bool runtimeInitialized;
}
 