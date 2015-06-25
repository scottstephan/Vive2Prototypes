using UnityEngine;
using System.Collections;

public class pointConstraint : MonoBehaviour
{
    public Transform xform;
    private Transform myTransform;
    public Vector3 offset = new Vector3(0f, 0f, 100f);
    public float oceanCurrentForce = 15.0f;
    public bool useOceanCurrentForce = true;
    public bool useRotation = false;
    public Transform rotXform;
    public bool modifyOffsetByMovement;
    public float moveSpeedMin = 10f;
    public float moveSpeedMax = 150f;
    public float moveOffsetMin = 50f;
    public float moveOffsetMax = 260f;
    public bool RotateOffsetPosition = true;

    [HideInInspector]
    public bool useLockDepth = false;
    [HideInInspector]
    public float lockDepth = -1045f;

    bool isCamera;

    Vector3 lastPosition;

    private ParticleAnimator[] particleAnimators;

    // Use this for initialization
    void Start()
    {
        myTransform = transform;

        if (myTransform == CameraManager.singleton.OVRCameraParent ||
            (CameraManager.singleton.HasVR && CameraManager.singleton.IsCamera(myTransform)))
        {
            isCamera = true;
        }

        //get all particle animators
        particleAnimators = gameObject.GetComponentsInChildren<ParticleAnimator>() as ParticleAnimator[];
    }

    // Update is called once per frame
    void Update()
    {
        if (xform == null)
        {
            return;
        }

        Vector3 cur_dir = Vector3.zero;
        if (OceanCurrents.Singleton != null)
        {
            cur_dir = OceanCurrents.Singleton.currentDirection;
        }

        Vector3 pos;
        Quaternion rot;

        if (isCamera)
        {
            pos = CameraManager.GetCurrentCameraPosition();
            rot = CameraManager.GetCurrentCameraRotation();
        }
        else
        {
            pos = xform.position;
            rot = xform.rotation;
        }

        if (RotateOffsetPosition)
        {
            myTransform.position = pos + (xform.rotation * offset);
        }
        else
        {
            myTransform.position = pos + offset;
        }

        if (modifyOffsetByMovement)
        {
            Vector3 delta = pos - lastPosition;
            float distSq = delta.sqrMagnitude;

            if (distSq > 0.001f)
            {
                float dist = Mathf.Sqrt(distSq);
                float speed = 0f;
                if (Time.deltaTime > 0f)
                {
                    speed = dist / Time.deltaTime;
                }

                float offsetDist = MathfExt.Fit(speed, moveSpeedMin, moveSpeedMax, moveOffsetMin, moveOffsetMax);
                myTransform.position += (offsetDist / dist) * delta;
            }
        }

        if (useLockDepth)
        {
            Vector3 pp = myTransform.position;
            pp.y = lockDepth;
            myTransform.position = pp;
        }

        //check ocean current force
        if (useOceanCurrentForce)
        {

            //loop through apply force to all particles
            foreach (ParticleAnimator particleAnimator in particleAnimators)
            {
                particleAnimator.force = cur_dir * oceanCurrentForce;
            }
        }

        if (useRotation)
        {
            if (rotXform)
            {
                myTransform.rotation = rotXform.rotation;
            }
            else
            {
                myTransform.rotation = rot;
            }
        }

        lastPosition = pos;
    }

}
