using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PreviewSceneMakerDebugCameraMode : BaseCameraMode
{
    private GameObject internalTarget;
    private CritterInfo internalCritter;
    private Transform internalTargetTransform;

    public float minDistance = 25.0f;
    public float desiredDistance = 35.0f;
    public float maxDistance = 45.0f;
    public float movementTime = 1.0f;
    private float movementTimer;

    public float heightOffset = 0.0f;
    // based on the speed of the fish, we recalculate our acceleration.
    public float lookAtDeadSpace = 10.0f;
    public float lookAtDelay = 0.3f;
    public float lookAtDistClamp = 20.0f;
//    private float lookAtDeadSpaceSqrd;
    //	private float lookAtDistClampSqrd;
    private float lookAtTimer;
    private Vector3 lookAtTarget;
    private Vector3 desiredLookAtTarget;
    public float maxCameraHeight = -10.0f;
	public float speed = 400f;
	
	private float fishToCameraDistance;
	
    private float GetAccelForSpeed(float time, float speed, float cur_speed)
    {
        return (Mathf.Abs((speed - cur_speed) / time));
    }
	
    void Awake()
    {
        internalTarget = null;
    }
	
    public override void InitCameraMode()
    {
        if( inited ) {
            return;
        }
        
        base.InitCameraMode();        
        
        cameraType = CameraType.PreviewSceneMakerDebugCamera;
        myTransform = transform;
        cameraName = "PreviewSceneMakerDebug Camera";
    }
	
    public override void StartCameraMode()
    {
        internalTarget = null;
    }
	
    public override void UpdateCameraMode()
    {
		runCollision = CameraCollisionType.None;
        float dot;

        if (internalTarget != CameraManager.currentTarget)
        {
            internalTarget = CameraManager.currentTarget;
            internalTargetTransform = internalTarget.transform;
            internalCritter = SimManager.GetCritterForCameraTarget();
            if (internalCritter == null)
            {
                CameraManager.JumpToCameraOrder(0);
                return;
            }

            FishCameraConstants constants = internalTarget.GetComponent<FishCameraConstants>();
            minDistance = constants.ropeAndStick_minDistance;
            desiredDistance = constants.ropeAndStick_desiredDistance;
            maxDistance = constants.ropeAndStick_maxDistance;
            movementTime = constants.ropeAndStick_movementTime;
            heightOffset = constants.ropeAndStick_heightOffset;
            lookAtDeadSpace = constants.ropeAndStick_lookAtDeadSpace;
            lookAtDelay = constants.ropeAndStick_lookAtDelay;
            lookAtDistClamp = constants.ropeAndStick_lookAtDistClamp;

            RaycastHit hit;
            Vector3 xdiff = myTransform.position - internalTargetTransform.position;
            Ray new_ray = new Ray(internalTargetTransform.position, xdiff.normalized);
            dot = Vector3.Dot(new_ray.direction, Vector3.up);
            if (Mathf.Abs(dot) > 0.4f)
            {
                xdiff[0] += 10f;
                xdiff[1] *= 0.4f;
                xdiff[2] += 10f;
                new_ray.direction = xdiff;
            }
            bool done = false;
            float inc = MathfExt.PI_4;
            float total_inc = 0;
            Vector3 og_ray = xdiff;
            while (!done)
            {
                if (Physics.SphereCast(new_ray, 5.0f, out hit, desiredDistance - 15f, 1 << 14))
                {
                    myTransform.position = internalTargetTransform.position + (new_ray.direction * (hit.distance - 20));
                    if (hit.distance > (desiredDistance * 0.75))
                    {
                        myTransform.position += new Vector3(0f, 250f, 0);
                        done = true;
                    }
                    else
                    {
                        total_inc += inc;
                        if (total_inc >= MathfExt.TWO_PI)
                        {
                            myTransform.position += new Vector3(0f, 250f, 0);
                            done = true;
                        }
                        new_ray.direction = MathfExt.YawVector(og_ray, total_inc);
                    }
                }
                else
                {
                    myTransform.position = internalTargetTransform.position + (new_ray.direction * desiredDistance);
                    done = true;
                }
            }
            myTransform.LookAt(internalTargetTransform.position, Vector3.up);
			fishToCameraDistance = (myTransform.position - internalTargetTransform.position) .magnitude;
        }
		if(internalTarget)
		{
			myTransform.position =Vector3.Lerp(myTransform.position, internalTargetTransform.position + (-myTransform .forward) * fishToCameraDistance, Time.deltaTime * 5.0f);
			MouseControl();
			if(myTransform.position.y > maxCameraHeight)
			{
				myTransform.position = new Vector3(myTransform.position.x,-10,myTransform.position.z );
			}
		}
    }
	void MouseControl()
	{
		if(Input.GetMouseButton(0))
		{
			myTransform.RotateAround(internalTargetTransform.position,Vector3.up,Input.GetAxis("Mouse X")*600.0f * Time.deltaTime);
			myTransform.RotateAround(internalTargetTransform.position,myTransform.right,-Input.GetAxis("Mouse Y")*600.0f * Time.deltaTime);
		}
		fishToCameraDistance -= Input.GetAxis("Mouse ScrollWheel") * speed * Time.deltaTime * internalCritter.generalMotionData.critterBoxColliderRadius;
		if(Input.GetKey(KeyCode.UpArrow))
		{
			fishToCameraDistance -=  10.0f* Time.deltaTime * internalCritter.generalMotionData.critterBoxColliderRadius;
		}
		if(Input.GetKey(KeyCode.DownArrow))
		{
			fishToCameraDistance +=  10.0f * Time.deltaTime * internalCritter.generalMotionData.critterBoxColliderRadius;
			
		}
	}
}
