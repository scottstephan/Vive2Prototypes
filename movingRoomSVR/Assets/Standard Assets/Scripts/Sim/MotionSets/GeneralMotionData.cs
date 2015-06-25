using UnityEngine;
using System.Collections;

public class GeneralMotionData : MonoBehaviour 
{
	public float followRadius = 60.0f;
	public float swimSpeed = 30.0f;
	public float smoothRotate = 1.6f;
	public bool useAvoidance = false;
	public float avoidFishLargerThanMyVolumeRatio = 0.7f;
	public float maxAcc = 30f;
	public float maxSpeed = 60.0f;
	public float sinMotionFreq = 1.0f;
 
    public float oceanCurrentMassFactor = 10000f;
    
	public Vector3 spawnPointCameraSpace = new Vector3(30f,0f,0f);
	public float   spawnPointRandRaius = 3f;
	
	[HideInInspector]
	public int avoidanceDelay;
	public int avoidanceEveryNFrames = 4;
	[HideInInspector]
	public int avoidanceGroundDelay;
	public int avoidanceGroundEveryNFrames = 4;
	public int avoidanceForwardCheckFrames = 28;
	public int avoidanceRotationCheckFrames = 4;
    [HideInInspector]
    public float avoidanceBoxPreCalcRotationFactor = 0f;

	public float avoidanceBoxScaleTime = 3.25f;
	public float avoidanceBoxZMin = 1.2f;
	public float avoidanceBoxZMax = 3.1f;
    public float avoidanceGroundPushFrameMult = 0.1f;
    public float avoidanceFishPushFrameMult = 0.3f;
    public float avoidFishForwardDesiredSpeedMult = 85f;

	[HideInInspector]
	public bool avoidGround;
	[HideInInspector]
	public bool avoidFish;
	[HideInInspector]
	public float halfSizeZ;
	[HideInInspector]
	public int desiredDirFrameOutCount;
	[HideInInspector]
	public Vector3 lastPosition;
	[HideInInspector]
	public Vector3 desiredDirNorm;
	[HideInInspector]
	public Vector3 desiredDirNormGround;
	[HideInInspector]
	public Vector3 critterBoxColliderSize;  // raw xyz size of the box collider
	[HideInInspector]
	public float critterBoxColliderVolume;  // x * y * z
	[HideInInspector]
	public float critterBoxColliderRadius;  // cross-section of the fish radius  2f * max(x,y)
	[HideInInspector]
	public Transform fishAvoidanceXform = null; 
	
	[HideInInspector]
	public float predatorOffsetMult;	
    [HideInInspector]
    public Transform myDisperseXform;
    [HideInInspector]
    public CritterInfo myDisperseCritter;
    [HideInInspector]
	public bool isDispersed;
	[HideInInspector]
	public float disperseRadius;
	[HideInInspector]
	public bool isBeingChased = false;
	
	[HideInInspector]
	public float currentSpeed;
	[HideInInspector]
	public float turningSpeedAdjust;
	[HideInInspector]
	public float currentAcc;
	[HideInInspector]
	public Vector3 heading;
	
	[HideInInspector]
	public Vector3 desiredVelocityDirection; // this used to be called diff
	[HideInInspector]
	public float speedMult; // speed multiplier
	[HideInInspector]
	public float rotMult; // rot multiplier
	[HideInInspector]
	public float dist; // current dist
	[HideInInspector]
	public float desiredSpeed; // desiredSpeed
	[HideInInspector]
	public bool lockVelocityToHeading;
	[HideInInspector]
	public bool useVelocityDirection;

	[HideInInspector]
	public bool useAvoidanceGround = true;
    [HideInInspector]
    public bool useAvoidanceFish = true;

    [HideInInspector]
    public bool useGroundSlide = true;

	[HideInInspector]
	public float turnRight;
	[HideInInspector]
	public float turnUp;
	
	[HideInInspector]
	public Vector3 lastFrameVelocity;
	
	[HideInInspector]
	public int layerMask;

	[HideInInspector]
	public bool collidedWithGroundLastFrame = false;	
	
	[HideInInspector]
	public int stuckCount = 0;	
	
	[HideInInspector]
	public bool drawGizmos = false;
	
	//////////////////////////////////////////////////////// 
	//////////////////////////////////////////////////////// 
	/// DEPRECATED!
	[HideInInspector]
	public Quaternion desiredRotation;
	[HideInInspector]
	public float desiredPitch;
	[HideInInspector]
	public float desiredYaw;
	[HideInInspector]
	public float steeringPitchSpeed = 0f;
	[HideInInspector]
	public float steeringYawSpeed = 0f;
	public float steeringYawAccel = 180f;
	public float steeringYawDecel = 2160f;
	public float steeringYawMaxSpeed = 270.0f;
	public float steeringPitchAccel = 180f;
	public float steeringPitchDecel = 2160f;
	public float steeringPitchMaxSpeed = 270.0f;
	public float steeringRollAccel = 220f;
	public float steeringRollDecel = 220f;
	public float steeringRollMaxSpeed = 270.0f;
	[HideInInspector]
	public float steeringRoll = 0f;
	[HideInInspector]
	public float desiredRoll;
	[HideInInspector]
	public float steeringRollSpeed = 0f;
	public float steeringRollMult = 0f;
	public float steeringRollStrafingMult = 0f;
	//////////////////////////////////////////////////////// 
	//////////////////////////////////////////////////////// 
	
}

