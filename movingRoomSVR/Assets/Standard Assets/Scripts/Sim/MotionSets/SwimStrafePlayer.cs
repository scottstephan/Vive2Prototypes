using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SwimStrafePlayer : BehaviorBase {
	// Use this for initialization

    // ensures nothing can automatically change this.
    public override bool IsSingletonBehavior() { return true; }

	public override void Start(CritterInfo critter_info)
	{
		GeneralMotionData gmd = critter_info.generalMotionData;
//		GeneralSpeciesData gsd  = critter_info.generalSpeciesData;
        SwimStrafePlayerData sd = critter_info.swimStrafePlayerData;
		gmd.lockVelocityToHeading = true;

        if (critter_info.swimDisperseData!= null)
        {
            critter_info.swimDisperseData.playerDisperseDisableCount++;
        }

        sd.prevPosition = CameraManager.GetCurrentCameraPosition();
        sd.prevSpeed = 0f;
        sd.strafeState = SwimStrafePlayerData.StrafePlayerState.Approach;

        sd.curTime = 0.1f;
        sd.desiredDirection = CameraManager.GetCurrentCameraForward();
        sd.bStrafeRight = RandomExt.CoinFlip();

        ThrottledFishSteering tfs = critter_info.critterSteering as ThrottledFishSteering;
        tfs.yawMaxSpeed *= 2f;
        tfs.yawAccel *= 2f;
        tfs.yawDecel *= 2f;

        gmd.critterBoxColliderRadius *= sd.colliderBoxRadiusMult;
    }

	public override void End(CritterInfo critter_info)
	{
        ThrottledFishSteering tfs = critter_info.critterSteering as ThrottledFishSteering;
        tfs.yawMaxSpeed /= 2f;
        tfs.yawAccel /= 2f;
        tfs.yawDecel /= 2f;

        SwimStrafePlayerData sd = critter_info.swimStrafePlayerData;
        GeneralMotionData gmd = critter_info.generalMotionData;
        gmd.critterBoxColliderRadius /= sd.colliderBoxRadiusMult;

        if (critter_info.swimDisperseData!= null)
        {
            critter_info.swimDisperseData.playerDisperseDisableCount--;
        }
    }

	// Update is called once per frame
	public override void Update (CritterInfo critter_info) 
	{
		SwimStrafePlayerData sd = critter_info.swimStrafePlayerData;
				
        // targeted motion only has a constant steering throttle for now.
		critter_info.critterSteering.desiredSteeringThrottle = sd.desiredThrottle;

        float dt = Time.deltaTime;
        sd.curTime -= dt;
        
        Vector3 camPos = CameraManager.GetCurrentCameraPosition();
        Vector3 critterPos = critter_info.critterTransform.position;
        Vector3 camToCritterDir = critterPos - camPos;
        float camToCritterDist = camToCritterDir.magnitude;

        if (camToCritterDist > 0f)
        {
            camToCritterDir /= camToCritterDist;
        }

        if (sd.strafeState == SwimStrafePlayerData.StrafePlayerState.Switching)
        {
            sd.desiredDirection.z = MathfExt.Fit(sd.curTime, 0f, sd.curTimeLimit, sd.curForwardDistance, sd.curForwardDistance+critter_info.generalMotionData.critterBoxColliderSize.z*sd.switchFwdColliderDistMult);
        }
        else if (sd.curTime <= 0.0f)
        {
            // switch the relative position.
            float randStrafeX = (sd.bStrafeRight ? 1 : -1) * RandomExt.FloatRange(sd.horizDistanceMin, sd.horizDistanceMax);
            bool bSwitchHoriz = (randStrafeX < 0f && sd.curHorizDistance > 0f) || (randStrafeX > 0f && sd.curHorizDistance < 0f);
            sd.curHorizDistance = randStrafeX;
            sd.curForwardDistance = RandomExt.FloatRange(sd.forwardDistanceMin, sd.forwardDistanceMax);
            if ((sd.curVertDistance <= 0f && !bSwitchHoriz) ||
                (sd.curVertDistance >= 0f && bSwitchHoriz))
            {
                sd.curVertDistance = RandomExt.FloatRange(sd.vertDistanceMin, sd.vertDistanceMax);
            }
            else
            {
                sd.curVertDistance = RandomExt.FloatRange(-sd.vertDistanceMax, sd.vertDistanceMin);
            }

            sd.desiredDirection  = new Vector3(sd.curHorizDistance, sd.curVertDistance, sd.curForwardDistance);
            if (Random.value < sd.switchSideChance)
            {
                sd.bStrafeRight = !sd.bStrafeRight;
            }

            if (sd.strafeState == SwimStrafePlayerData.StrafePlayerState.Locked)
            {
                sd.strafeState = SwimStrafePlayerData.StrafePlayerState.Switching;
                sd.curTime = sd.curTimeLimit = RandomExt.FloatRange(sd.positioningTimeMin, sd.positioningTimeMax);
            }
            else
            {
                sd.curTime = sd.curTimeLimit = RandomExt.FloatRange(sd.switchLocationTimeMin, sd.switchLocationTimeMax);
            }
        }

        // figure out our current rotation basd on the cameras path of motion and speed
        Vector3 camMoveDir = camPos - sd.prevPosition;
        float camMoveDist = camMoveDir.magnitude;

        if (dt < 0f)
        {
            dt = 1f;
        }

        float camSpeed = camMoveDist / dt;
        float camAccel = (camSpeed-sd.prevSpeed) / dt;

        if( camMoveDist < 0.001f ) 
        {
            camMoveDir = camToCritterDir * -1f;
            camMoveDist = camToCritterDist;
        }
        else
        {
            camMoveDir /= camMoveDist;
        }

        Vector3 desiredMoveDir = Quaternion.LookRotation( camMoveDir ) * sd.desiredDirection;
        Vector3 targetPos = camPos + desiredMoveDir;

        Vector3 targetDir = targetPos - critterPos;
        float targetDist = targetDir.magnitude;

        if (targetDist > 0f)
        {
            targetDir /= targetDist;
        }

#if UNITY_EDITOR
        Debug.DrawLine( camPos, targetDir, Color.white );
        Debug.DrawLine( camPos, camPos+desiredMoveDir, Color.green );
        Debug.DrawLine( camPos, camPos + camMoveDir * 300f, Color.blue );
#endif

        bool isBehind = false;
        isBehind = Vector3.Dot (targetDir, critter_info.critterTransform.forward) > 0f;

        if (sd.strafeState == SwimStrafePlayerData.StrafePlayerState.Approach ||
            sd.strafeState == SwimStrafePlayerData.StrafePlayerState.Switching)
        {
            critter_info.generalMotionData.desiredVelocityDirection = targetDir;

            // in between goal and player
            if (targetDist < sd.lockDistance)
            {
                sd.strafeState = SwimStrafePlayerData.StrafePlayerState.Locked;
                sd.curTime = sd.curTimeLimit = RandomExt.FloatRange(sd.switchLocationTimeMin, sd.switchLocationTimeMax);
            }
        }
        else
        {
            critter_info.generalMotionData.desiredVelocityDirection = camMoveDir;
        }

        if (sd.strafeState == SwimStrafePlayerData.StrafePlayerState.Locked)
        {
            if (isBehind)
            {
                critter_info.generalMotionData.desiredSpeed = MathfExt.Fit(targetDist, 0f, sd.lockDistance, camSpeed, camSpeed * sd.approachSpeedMult);
            }
            else
            {
                critter_info.generalMotionData.currentAcc = camAccel;
                critter_info.generalMotionData.currentSpeed = camSpeed;
            }
        }
        else if (sd.strafeState == SwimStrafePlayerData.StrafePlayerState.Switching)
        {
            float dot = Mathf.Clamp(Vector3.Dot (critter_info.generalMotionData.desiredVelocityDirection.normalized, critter_info.critterTransform.forward), -1f, 1f);
            float velDeltaAngle = Mathf.Acos (dot);
            float straightAheadMaxSpeed = isBehind ? camSpeed * sd.approachSpeedMult : camSpeed;
            critter_info.generalMotionData.desiredSpeed = MathfExt.Fit (Mathf.Abs (velDeltaAngle), Mathf.PI/18f, Mathf.PI*0.25f, straightAheadMaxSpeed, camSpeed*0.5f);
        }
        else
        {           
            if (isBehind)
            {
                critter_info.generalMotionData.desiredSpeed = Mathf.Max(camSpeed, critter_info.generalMotionData.swimSpeed) * (sd.approachSpeedMult *2f);
            }
            else if (camSpeed < 1f)
            {
                critter_info.generalMotionData.desiredSpeed = critter_info.generalMotionData.swimSpeed;
            }
            else if (sd.strafeState == SwimStrafePlayerData.StrafePlayerState.Approach)
            {
                critter_info.generalMotionData.desiredSpeed = camSpeed * sd.approachSpeedMult;
            }
        }

        sd.prevPosition = camPos;
        sd.prevSpeed = camSpeed;
    }

    public override float EvaluatePriority(CritterInfo critter_info)
	{
        return 0f;
	}
}