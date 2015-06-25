using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwimPlayerView : BehaviorBase
{
    public override void Start(CritterInfo critter_info)
    {
        base.Start(critter_info);

        
        GeneralMotionData gmd = critter_info.generalMotionData;
        SwimPlayerViewData sd = critter_info.swimPlayerViewData;

        sd.prevForceLOD = critter_info.critterLODData.ForceLOD;
        LODManager.ForceLOD( critter_info.critterLODData, 0 );
        critter_info.critterLODData.ForceLOD = 0;
        critter_info.critterLODData.SetupLitPieces( false );
        critter_info.critterItemData.SetFishNameUI();

        SimInstance.Instance.SlowdownCritters( true, false );

        sd.ogUseAvoidance = gmd.useAvoidance;
        gmd.lockVelocityToHeading = true;
        gmd.useAvoidance = false;
//        gmd.desiredVelocityDirection = critter_info.critterTransform.forward;
//        gmd.desiredVelocityDirection.y = 0f;
//        gmd.desiredSpeed = 0f;

        if (sd.allowRotate)
        {
            Vector3 camFwdXZ = CameraManager.GetCurrentCameraForward();
            camFwdXZ.y = 0f; 
            camFwdXZ.Normalize();
            Vector3 camRight = Vector3.Cross(camFwdXZ, Vector3.up);
            sd.moveInViewRight = Vector3.Dot (camRight, critter_info.critterTransform.forward) > 0f;
            gmd.desiredVelocityDirection = sd.moveDir = sd.moveInViewRight ? camRight : -camRight;
        }
        else
        {
            gmd.desiredVelocityDirection = sd.moveDir = critter_info.critterTransform.forward;
        }

        if (!sd.started)
        {
            sd.moveInDirStarted  = false;

            if (critter_info.swimDisperseData != null)
            {
                critter_info.swimDisperseData.playerDisperseDisableCount++;
            }

            critter_info.critterBoxCollider.size *= 0.5f;

            // turn our eyes on!
            if( critter_info.critterEyeData ) 
            {
                critter_info.critterEyeData.OFF = false;
            }

            critter_info.critterSteering.desiredSteeringThrottle = 1f;
    //        critter_info.critterSteering.desiredSteeringThrottle = 0.05f;


            sd.started = true;
        }
    }

    public override void End(CritterInfo critter_info)
    {
        SwimPlayerViewData sd = critter_info.swimPlayerViewData;
        
        LODManager.ForceLOD( critter_info.critterLODData, sd.prevForceLOD );
        sd.prevForceLOD = -1;
        critter_info.critterLODData.SetupLitPieces( true, 0.3f );
        critter_info.critterItemData.TurnFishNameUIOff();

        SimInstance.Instance.SlowdownCritters( false, false );

        GeneralMotionData gmd = critter_info.generalMotionData;
        gmd.useAvoidance = sd.ogUseAvoidance;

        sd.timer = 0f;

        if (sd.started)
        {
            sd.started = false;

            critter_info.critterBoxCollider.size *= 2f;
    //        critter_info.critterSteering.desiredSteeringThrottle = 1f;

            if( critter_info.critterEyeData ) 
            {
                critter_info.critterEyeData.OFF = true;
            }

            if (critter_info.swimDisperseData != null)
            {
                critter_info.swimDisperseData.playerDisperseDisableCount--;
            }

            if (sd.prevSingletonBehavior != -1)
            {
                SwimBehaviorType prevBehavior = (SwimBehaviorType)sd.prevSingletonBehavior;
                sd.prevSingletonBehavior = -1;
                AI.ForceSwitchToBehavior(critter_info, prevBehavior);
            }
        }
    }

    // Update is called once per frame
    public override void Update( CritterInfo critter_info )
    {
        float dt = Time.deltaTime;

        GeneralMotionData gmd = critter_info.generalMotionData;
        SwimPlayerViewData spvd = critter_info.swimPlayerViewData;
        /*
        Vector3 camEyePos = CameraManager.GetEyePosition();
        Vector3 critterPos = critter_info.critterTransform.position;
        bool isVisible = critter_info.isVisible;


        // Get camera look direction, but adjust y so it's mostly in the same xz plane as player, 
        // but height adjusted a little if looking up/down
        Vector3 camFwdXZ = CameraManager.GetCurrentCameraForward();
        camFwdXZ.y = 0f; 
        camFwdXZ.Normalize();

        Vector3 camToCritterDirXZ = critterPos - camEyePos;
        camToCritterDirXZ.y = 0f;
        float camToCritterDistXZ = camToCritterDirXZ.magnitude;

        if (camToCritterDistXZ > 0f)
        {
            camToCritterDirXZ /= camToCritterDistXZ;
        }

        float dot = Vector3.Dot (camFwdXZ, camToCritterDirXZ);

        if (!isVisible)
        {
            spvd.moveInDirStarted = false;
        }

        if (!spvd.moveInDirStarted)
        {
            if (isVisible && dot > 0.5f)
            {
                spvd.moveInDirStarted = true;
                spvd.moveInDirTimer = RandomExt.FloatRange(spvd.moveInDirTimeMin, spvd.moveInDirTimeMax);
                spvd.moveInViewRight = Vector3.Dot (Vector3.Cross(camFwdXZ, Vector3.up), camToCritterDirXZ) < 0f; // start moving right if we're on the left of the camera

                if (critter_info.swimFreeData.lookAtAnim != null)
                {
                    critter_info.animBase.PlayAnimation(critter_info.swimFreeData.lookAtAnim, false, true);
                }
            }
        }

        if (spvd.moveInDirStarted)
        {
            spvd.moveInDirTimer -= dt;

            if (spvd.moveInDirTimer <= 0f)
            {
                spvd.moveInViewRight = !spvd.moveInViewRight;
                spvd.moveInDirTimer = RandomExt.FloatRange(spvd.moveInDirTimeMin, spvd.moveInDirTimeMax);

                if (critter_info.swimFreeData.lookAtAnim != null)
                {
                    critter_info.animBase.PlayAnimation(critter_info.swimFreeData.lookAtAnim, false, true);
                }
            }

            Vector3 right = Vector3.Cross(camFwdXZ, Vector3.up);
            gmd.desiredVelocityDirection = spvd.moveInViewRight ? right : -right;
            gmd.currentSpeed = gmd.desiredSpeed = 0.5f;           
        }
        else
        {
            Vector3 targetPos = camEyePos + camFwdXZ * spvd.goalDistance;
            gmd.desiredVelocityDirection = targetPos - critterPos;       
            gmd.desiredSpeed = gmd.swimSpeed;           
        }
        */

        gmd.desiredVelocityDirection = spvd.moveDir;
        gmd.currentSpeed = gmd.desiredSpeed = 0.5f;           

        Vector3 cam_fwd = CameraManager.GetCurrentCameraForward();
        Vector3 to_me = critter_info.cachedPosition - CameraManager.GetCurrentCameraPosition();
        to_me.Normalize();
        float d = Vector3.Dot( cam_fwd, to_me );
//        Debug.Log(d + "  :: " + spvd.timer + " :: " + dt);
        if( d < spvd.dotStay ) {
            spvd.timer += dt;
            if (spvd.timer >= 0.62f)
            {
                critter_info.generalSpeciesData.switchBehavior = true;
                spvd.startMe = false;
            }
        }
        else {
//            Debug.Log ("OFF!");
            spvd.timer = 0f;
        }

        if( OculusFPSCameraMode.singleton.IsInteractButtonDown() || CameraManager.GetActiveCameraType() != CameraType.OculusCamera ) {
//            Debug.Log ("Interact OFF!");
            critter_info.generalSpeciesData.switchBehavior = true;
            spvd.startMe = false;
        }
    }

    public override void LateUpdate( CritterInfo critter_info ) {
        critter_info.critterItemData.UpdateFishNameUI();
    }

    public override float EvaluatePriority(CritterInfo critter_info)
    {
        SwimPlayerViewData sd = critter_info.swimPlayerViewData;

        if(sd && sd.startMe)
        {
            return 500f;
        }

        return 0f;
    }
}
