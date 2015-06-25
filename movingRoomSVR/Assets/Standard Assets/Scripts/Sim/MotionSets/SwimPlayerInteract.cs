using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwimPlayerInteract : BehaviorBase
{
    List<CritterInfo> disperseCritters = new List<CritterInfo>();
    GameObject disperseObject;
    Transform disperseTransform;

    public override void OneTimeStart(CritterInfo critter_info)
    {
        base.OneTimeStart (critter_info);
       
        SwimPlayerInteractData spid = critter_info.swimPlayerInteractData;

        if (spid != null)
        {
            if (spid.noticeAnim != null)
            {
                critter_info.animBase.AddAnimation(spid.noticeAnim);
            }

            if (spid.inspectAnim != null)
            {
                critter_info.animBase.AddAnimation(spid.inspectAnim);
            }

            if (spid.curiousAnim != null)
            {
                critter_info.animBase.AddAnimation(spid.curiousAnim);
            }

            if (spid.scaredAnim != null)
            {
                critter_info.animBase.AddAnimation(spid.scaredAnim);
            }

            if (spid.seenReactionAnim != null)
            {
                critter_info.animBase.AddAnimation(spid.seenReactionAnim);
            }

            if (spid.pokeAnim != null)
            {
                critter_info.animBase.AddAnimation(spid.pokeAnim);
            }
        }
    }

    public override void Start(CritterInfo critter_info)
    {
        base.Start(critter_info);

        GeneralMotionData gmd = critter_info.generalMotionData;
        gmd.lockVelocityToHeading = true;
//        gmd.desiredVelocityDirection = critter_info.critterTransform.forward;
//        gmd.desiredVelocityDirection.y = 0f;
//        gmd.desiredSpeed = 0f;
        
        SwimPlayerInteractData spid = critter_info.swimPlayerInteractData;
        critter_info.critterSteering.desiredSteeringThrottle = spid.steeringThrottle;

        if (critter_info.swimDisperseData != null)
        {
            critter_info.swimDisperseData.playerDisperseDisableCount++;
        }

        ApplyThrottleOverrides( critter_info, spid );

        spid.totalInteractTime = 0f;
        spid.totalVisibleTime = 0f;
        spid.moveInViewFlipCount = 0;
        spid.moveInViewStartLeft = RandomExt.CoinFlip();
        critter_info.critterBoxCollider.size *= 0.5f;

        // turn our eyes on!
        if( critter_info.critterEyeData ) 
        {
            critter_info.critterEyeData.OFF = false;
        }

        if (spid.behaviorExitTime > 0f &&
            Time.time - spid.behaviorExitTime < 90f)
        {
            GoToState(critter_info, SwimPlayerInteractData.State.MoveToPlayer);           
        }
        else
        {
            spid.scareCount = 0;
            if (spid.noticeAnim == null)
            {
                GoToState(critter_info, SwimPlayerInteractData.State.MoveToPlayer);           
            }
            else
            {
                GoToState(critter_info, SwimPlayerInteractData.State.MoveNotice);
            }

            if (disperseObject == null)
            {
                disperseObject = new GameObject();
                disperseTransform = disperseObject.transform;
            }

            if (critter_info.swimSchoolFollowData.leaderTransform != null)
            {
                SimInstance.Instance.GetSchoolCritters(critter_info, disperseCritters);

                Vector3 centerPos = critter_info.critterTransform.position;
                
                for (int i=0; i<disperseCritters.Count; ++i)
                {
                    centerPos += disperseCritters[i].critterTransform.position;
                }

                centerPos /= disperseCritters.Count + 1;

                // Get camera look direction, but adjust y so it's mostly in the same xz plane as player, 
                // but height adjusted a little if looking up/down
                Vector3 playerDir = CameraManager.GetCurrentCameraTransform().position - critter_info.critterTransform.position;
                playerDir.y = 0f;
                playerDir.Normalize();

                //use desiredVel instead??
                Vector3 fishDir = critter_info.critterTransform.forward;
                fishDir.y = 0f;
                fishDir.Normalize();

                Vector3 disperseDir = -fishDir + (-playerDir);
                float dist = 300f;

                // disperse between cam dir and fish move dir, so fish doesn't turn 90 degrees toward/away from cam (avoid smallest silhoutte)
                disperseTransform.position = centerPos + disperseDir.normalized * dist;
                disperseTransform.rotation = Quaternion.LookRotation(disperseDir);

                for (int i=0; i<disperseCritters.Count; ++i)
                {
                    CritterInfo sc = disperseCritters[i];
                    DisperseCollision.Disperse(sc.generalSpeciesData, null, disperseTransform, 100f, true);
                }

                disperseCritters.Clear ();
            }
        }
    }

    public override void End(CritterInfo critter_info)
    {
        critter_info.critterBoxCollider.size *= 2f;
        RestoreThrottleOverrides( critter_info, critter_info.swimPlayerInteractData );

        // turn our eyes on!
        if( critter_info.critterEyeData ) 
        {
            critter_info.critterEyeData.OFF = true;
        }

        critter_info.swimPlayerInteractData.behaviorExitTime = Time.time;

        if (critter_info.swimDisperseData != null)
        {
            critter_info.swimDisperseData.playerDisperseDisableCount--;
        }
    }

    void ApplyThrottleOverrides( CritterInfo critter_info, SwimPlayerInteractData spid )
    {
        ThrottledFishSteering s = (ThrottledFishSteering)critter_info.critterSteering;
        
        // save the current steering values
        spid.savedThrottleMaxSpeed = s.throttleMaxSpeed;
        spid.savedThrottleSpeedAccel = s.throttleSpeedAccel;
        spid.savedThrottleSpeedDecel = s.throttleSpeedDecel;
        spid.savedYawAccel = s.yawAccel;
        spid.savedYawDecel = s.yawDecel;
        spid.savedYawMaxSpeed = s.yawMaxSpeed;   
        spid.savedPitchAccel = s.pitchAccel;
        spid.savedPitchDecel = s.pitchDecel;
        spid.savedPitchMaxSpeed = s.pitchMaxSpeed;
        spid.savedRollAccel = s.rollAccel;
        spid.savedRollDecel = s.rollDecel;
        spid.savedRollMaxSpeed = s.rollMaxSpeed;
        spid.savedRollOnYawMult = s.rollOnYawMult;
        spid.savedRollStrafingMult = s.rollStrafingMult; 
        
        // apply the overrides
        s.throttleMaxSpeed = spid.throttleMaxSpeed;
        s.throttleSpeedAccel = spid.throttleSpeedAccel;
        s.throttleSpeedDecel = spid.throttleSpeedDecel;
        s.yawAccel = spid.yawAccel;
        s.yawDecel = spid.yawDecel;
        s.yawMaxSpeed = spid.yawMaxSpeed;
        s.pitchAccel = spid.pitchAccel;
        s.pitchDecel = spid.pitchDecel;
        s.pitchMaxSpeed = spid.pitchMaxSpeed;
        s.rollAccel = spid.rollAccel;
        s.rollDecel = spid.rollDecel;
        s.rollMaxSpeed = spid.rollMaxSpeed;
        s.rollOnYawMult = spid.rollOnYawMult;
        s.rollStrafingMult = spid.rollStrafingMult;
    }
    
    void RestoreThrottleOverrides( CritterInfo critter_info, SwimPlayerInteractData spid )
    {
        ThrottledFishSteering s = (ThrottledFishSteering)critter_info.critterSteering;
        
        s.throttleMaxSpeed = spid.savedThrottleMaxSpeed;
        s.throttleSpeedAccel = spid.savedThrottleSpeedAccel;
        s.throttleSpeedDecel = spid.savedThrottleSpeedDecel;
        s.yawAccel = spid.savedYawAccel;
        s.yawDecel = spid.savedYawDecel;
        s.yawMaxSpeed = spid.savedYawMaxSpeed;
        s.pitchAccel = spid.savedPitchAccel;
        s.pitchDecel = spid.savedPitchDecel;
        s.pitchMaxSpeed = spid.savedPitchMaxSpeed;
        s.rollAccel = spid.savedRollAccel;
        s.rollDecel = spid.savedRollDecel;
        s.rollMaxSpeed = spid.savedRollMaxSpeed;
        s.rollOnYawMult = spid.savedRollOnYawMult;
        s.rollStrafingMult = spid.savedRollStrafingMult;
    }

    // Update is called once per frame
    public override void Update (CritterInfo critter_info) 
    {
        float dt = Time.deltaTime;

        base.Update( critter_info );
        
        GeneralMotionData gmd = critter_info.generalMotionData;
        SwimPlayerInteractData spid = critter_info.swimPlayerInteractData;
        Transform camTransform = CameraManager.GetCurrentCameraTransform();
        Vector3 camEyePos = CameraManager.GetEyePosition();

        // Get camera look direction, but adjust y so it's mostly in the same xz plane as player, 
        // but height adjusted a little if looking up/down
        Vector3 forwardAdjustedY = camTransform.forward;
        forwardAdjustedY.y *= 0.3f; 
        forwardAdjustedY.Normalize();

        Vector3 playerMoveDir = camTransform.position - spid.playerLastPos;      
        float playerMoveDist = playerMoveDir.magnitude;
        bool bPlayerMoving = playerMoveDist > 0.01f;
        Vector3 playerFishDir = critter_info.critterTransform.position - camEyePos;
        float playerFishDist = playerFishDir.magnitude;

        if (playerMoveDist > 0f)
        {
            playerMoveDir /= playerMoveDist;
        }

        if (playerFishDist > 0f)
        {
            playerFishDir /= playerFishDist;
        }

        if (spid.state != SwimPlayerInteractData.State.MoveToPlayer &&
            spid.state != SwimPlayerInteractData.State.MoveNotice)
        {
            spid.totalInteractTime += dt;

            if (spid.maxInteractTime > 0f &&
                spid.totalInteractTime > spid.maxInteractTime)
            {
                Disperse(critter_info);
                return;
            }
        }

        bool bVisible = critter_info.isVisible;
        bool bSlowOnArrival = true;

        if (spid.state == SwimPlayerInteractData.State.MoveInView ||
            spid.state == SwimPlayerInteractData.State.Spin)
        {
            bSlowOnArrival = false;
        }
        else if ((spid.state == SwimPlayerInteractData.State.MoveToPlayer 
                  || spid.state == SwimPlayerInteractData.State.MoveCurious 
                  || spid.state == SwimPlayerInteractData.State.MoveScared)
                 && !bVisible)
        {
            bSlowOnArrival = false;
        }

        if (bVisible)
        {
            if (spid.stateNotVisibleTime > 0f &&
                spid.seenReactionAnim != null &&
                !critter_info.animBase.playingOneOff &&
                (spid.state == SwimPlayerInteractData.State.LookScared
                 || spid.state == SwimPlayerInteractData.State.LookCurious
                 || spid.state == SwimPlayerInteractData.State.LookInspect
                 || spid.state == SwimPlayerInteractData.State.MoveCurious
                 || spid.state == SwimPlayerInteractData.State.MoveToPlayer 
                 || spid.state == SwimPlayerInteractData.State.MoveInView))
            {
                critter_info.animBase.PlayAnimation(spid.seenReactionAnim, false, true);
            }

            spid.stateVisibleTime += dt;
            spid.stateNotVisibleTime = 0f;
            spid.totalVisibleTime += dt;
        }
        else
        {
            spid.stateNotVisibleTime += dt;
            spid.stateVisibleTime = 0f;
            spid.totalVisibleTime = 0f;
        }

        spid.stateTime += dt;

        // update target distance for moving states
        bool bUpdateSpeed = true;

        if (spid.state == SwimPlayerInteractData.State.MoveNotice)
        {
            if (spid.stateTime < 0.5f)
            {
                spid.stateTargetPos = critter_info.critterTransform.position + critter_info.critterTransform.forward * 50f;
                bUpdateSpeed = false;
            }
            else
            {
                Vector3 fishPlayerDirXZ = new Vector3(-playerFishDir.x, 0f, -playerFishDir.z).normalized;
                Vector3 fishPlayerRight = Vector3.Cross(fishPlayerDirXZ, Vector3.up);
                float fishPlayerRightDot = Vector3.Dot (fishPlayerRight, critter_info.critterTransform.forward);

                if (fishPlayerRightDot > 0f)
                {
                    spid.stateTargetPos = camEyePos + MathfExt.YawVector(forwardAdjustedY, 10f*Mathf.Deg2Rad) * spid.inspectDistance;
                }
                else
                {
                    spid.stateTargetPos = camEyePos + MathfExt.YawVector(forwardAdjustedY, -10f*Mathf.Deg2Rad) * spid.inspectDistance;
                }
            }
        }                                                    
        else if (spid.state == SwimPlayerInteractData.State.LookNotice ||
                 spid.state == SwimPlayerInteractData.State.MoveToPlayer)
        {
            spid.stateTargetPos = camEyePos + (forwardAdjustedY * spid.inspectDistance) + spid.randomMoveOffset;
        }
        else if (spid.state == SwimPlayerInteractData.State.MoveCurious)
        {
            spid.stateTargetPos = camEyePos + (forwardAdjustedY * spid.curiousDistance) + spid.randomMoveOffset;
        }
        else if (spid.state == SwimPlayerInteractData.State.MoveScared)
        {
            spid.stateTargetPos = camEyePos + (forwardAdjustedY * spid.scaredDistance) + spid.randomMoveOffset;
        }
        else if (spid.state == SwimPlayerInteractData.State.MoveInView)
        {
            float goalDist = (spid.curiousDistance + spid.inspectDistance) * 0.5f;
            float lookYaw = spid.moveInViewFlipCount % 2 == (spid.moveInViewStartLeft ? 0 : 1) ? Mathf.Deg2Rad * 90f : Mathf.Deg2Rad * -90f;
            spid.stateTargetPos = camEyePos
                                  + MathfExt.YawVector(forwardAdjustedY, lookYaw) * spid.curiousDistance*2f
                                  + forwardAdjustedY * goalDist;
        }
        else if (spid.state == SwimPlayerInteractData.State.Spin)
        {
            // move behind curious dist
            spid.stateTargetPos = camEyePos + (forwardAdjustedY * (spid.curiousDistance+(spid.turnDistance*1.25f))) + spid.randomMoveOffset;
        }
        else if (spid.state == SwimPlayerInteractData.State.LookCurious ||
                 spid.state == SwimPlayerInteractData.State.LookScared ||
                 spid.state == SwimPlayerInteractData.State.LookInspect)
        {
            UpdatePlayerLookOffset(critter_info);
        }

        Vector3 desiredVelDir = spid.stateTargetPos - critter_info.critterTransform.position;
        float distToTarget = desiredVelDir.magnitude;
        Vector3 lookDir = MathfExt.PitchVector(MathfExt.YawVector(camEyePos - critter_info.critterTransform.position, spid.lookOffsetYaw), spid.lookOffsetPitch);

        if (!gmd.avoidGround)
        {
            gmd.desiredVelocityDirection = desiredVelDir;       

            if (bUpdateSpeed)
            {
                if (bSlowOnArrival)
                {
                    float minSpeedMult = MathfExt.Fit(Vector3.Angle(lookDir, critter_info.critterTransform.forward), 0f, 360f, 0.0f, 1f);
                    gmd.desiredSpeed = spid.swimSpeed * MathfExt.Fit(distToTarget,spid.turnDistance,spid.turnDistance*10f,minSpeedMult,1f);
                }
                else
                {
                    gmd.desiredSpeed = spid.swimSpeed;           
                }
            }
        }

        float inspectVisibleTimeLimit = 3f + (spid.inspectAnim == null ? 2.5f : 0f);
        float curiousVisibleTimeLimit = 5f + (spid.curiousAnim == null ? 2.5f : 0f);
        float scaredVisibleTimeLimit = 5f + (spid.inspectAnim == null ? 2.5f : 0f);

        // Update state transition logic
        if (spid.state == SwimPlayerInteractData.State.MoveNotice)
        {
            if (Vector3.Dot (playerFishDir, critter_info.critterTransform.forward) < -0.8f &&
                (spid.stateVisibleTime > 0f || spid.stateNotVisibleTime > 5f))
            {
                GoToState(critter_info, SwimPlayerInteractData.State.LookNotice);
            }
        }
        else if (spid.state == SwimPlayerInteractData.State.LookNotice &&
                 (spid.noticeAnim == null || (spid.statePlayedLookAnim && !critter_info.animBase.playingOneOff)))
        {
            GoToState(critter_info, SwimPlayerInteractData.State.MoveToPlayer);
        }
        else if (spid.state == SwimPlayerInteractData.State.MoveToPlayer)
        {
            if (distToTarget < spid.turnDistance && !bPlayerMoving)
            {
                GoToState(critter_info, SwimPlayerInteractData.State.LookInspect);
            }
        }
        else if (spid.state == SwimPlayerInteractData.State.LookInspect &&
                 spid.stateVisibleTime > inspectVisibleTimeLimit &&
                 (spid.inspectAnim == null || (spid.statePlayedLookAnim && !critter_info.animBase.playingOneOff)))
        {
            GoToState(critter_info, SwimPlayerInteractData.State.MoveCurious);
        }
        else if (spid.state == SwimPlayerInteractData.State.MoveCurious)
        {
            if ((distToTarget < spid.turnDistance && !bPlayerMoving) || gmd.avoidGround)
            {
                GoToState(critter_info, SwimPlayerInteractData.State.LookCurious);
            }
        }
        else if (spid.state == SwimPlayerInteractData.State.LookCurious &&
                 (spid.stateTime > (curiousVisibleTimeLimit + 2f) || spid.stateVisibleTime > curiousVisibleTimeLimit) &&
                 (spid.curiousAnim == null || (spid.statePlayedLookAnim && !critter_info.animBase.playingOneOff)))
        {
            if (spid.curiousBehavior == SwimPlayerInteractData.CuriousBehavior.None)
            {
                GoToState(critter_info, SwimPlayerInteractData.State.Poke);
            }
            else if (spid.curiousBehavior == SwimPlayerInteractData.CuriousBehavior.SwimBackAndForth)
            {
                GoToState(critter_info, SwimPlayerInteractData.State.MoveInView);
            }
            else if (spid.curiousBehavior == SwimPlayerInteractData.CuriousBehavior.Spin)
            {
                if (spid.moveInViewFlipCount == 0)
                {
                    GoToState(critter_info, SwimPlayerInteractData.State.Spin);
                }
                else
                {
                    GoToState(critter_info, SwimPlayerInteractData.State.Poke);
                }
            }
        }
        else if (spid.state == SwimPlayerInteractData.State.MoveInView)
        {
            if (distToTarget < spid.turnDistance || gmd.avoidGround)
            {
                gmd.avoidGround = false;
                if (spid.moveInViewFlipCount > 4)
                {
                    GoToState(critter_info, SwimPlayerInteractData.State.Poke);
                }
                else
                {
                    GoToState(critter_info, SwimPlayerInteractData.State.MoveInView);
                }
            }
        }
        else if (spid.state == SwimPlayerInteractData.State.Spin)
        {
            if (spid.stateTime > 4f)
            {
                gmd.desiredVelocityDirection = lookDir;
                gmd.desiredRotation = Quaternion.LookRotation(lookDir);
                gmd.desiredSpeed = 0f;

                if (Vector3.Dot (playerFishDir, critter_info.critterTransform.forward) < -0.9f)
                {
                    spid.moveInViewFlipCount++;
                    GoToState(critter_info, SwimPlayerInteractData.State.LookCurious);
                }
            }
        }
        else if (spid.state == SwimPlayerInteractData.State.MoveScared)
        {
            if ((distToTarget < spid.turnDistance || gmd.avoidGround)
                 && !bPlayerMoving)
            {
                GoToState(critter_info, SwimPlayerInteractData.State.LookScared);
            }
        }
        else if (spid.state == SwimPlayerInteractData.State.LookScared)
        {
            if (spid.stateNotVisibleTime > 1.5f ||
                spid.stateVisibleTime > scaredVisibleTimeLimit)
            {
                GoToState(critter_info, SwimPlayerInteractData.State.MoveToPlayer);
            }
        }
        else if (spid.state == SwimPlayerInteractData.State.Poke &&
                 (spid.pokeAnim == null || (spid.statePlayedLookAnim &&  !critter_info.animBase.playingOneOff)))
        {
            Disperse(critter_info);
        }

        // allow get scared
        if (AllowMoveScared(spid.state) &&
            spid.totalVisibleTime > 0.5f &&
            bPlayerMoving)
        {
            if (playerFishDist < spid.inspectDistance * 0.9f)
            {
                if (Vector3.Dot (playerMoveDir, playerFishDir) > 0.25f &&
                    Vector3.Dot (playerFishDir, critter_info.critterTransform.forward) < -0.25f)
                {
                    GoToState(critter_info, SwimPlayerInteractData.State.MoveScared);
                }
            }
            else if ((spid.state == SwimPlayerInteractData.State.LookCurious || spid.state == SwimPlayerInteractData.State.LookScared) 
                     && playerFishDist > spid.inspectDistance * 1.1f)
            {
                if (Vector3.Dot (playerMoveDir, playerFishDir) < -0.25f)
                {
                    GoToState(critter_info, SwimPlayerInteractData.State.MoveCurious);
                }
            }
        }

        // play look anim
        AnimationClip lookAnim = GetStateLookAnim(spid);

        if (lookAnim != null &&
            !spid.statePlayedLookAnim &&
            !critter_info.animBase.playingOneOff)
        {
            if (Vector3.Dot (playerFishDir, critter_info.critterTransform.forward) < -0.9f)
            {
                spid.statePlayedLookAnim = true;
                critter_info.animBase.PlayAnimation(lookAnim, false, true);
                PlayStateLookSound(critter_info);
            }
        }

        // Update fish look direction and fish look timers
        if (AllowTurnLook(spid.state))
        {
            gmd.desiredVelocityDirection = lookDir;
            gmd.desiredRotation = Quaternion.LookRotation(lookDir);
            gmd.desiredSpeed = 0f;
        }

        spid.playerLastPos = camTransform.position;
    }

    bool AllowMoveScared(SwimPlayerInteractData.State state)
    {
        return state == SwimPlayerInteractData.State.LookInspect ||
               state == SwimPlayerInteractData.State.LookCurious ||
               state == SwimPlayerInteractData.State.MoveCurious ||
               state == SwimPlayerInteractData.State.LookScared;
    }

    bool AllowTurnLook(SwimPlayerInteractData.State state)
    {
        return state == SwimPlayerInteractData.State.LookNotice ||
               state == SwimPlayerInteractData.State.LookInspect ||
               state == SwimPlayerInteractData.State.LookCurious ||
               state == SwimPlayerInteractData.State.LookScared ||
               state == SwimPlayerInteractData.State.Poke;
    }

    AnimationClip GetStateLookAnim(SwimPlayerInteractData spid)
    {
        if (spid.state == SwimPlayerInteractData.State.LookNotice)
        {
            return spid.noticeAnim;
        }
        if (spid.state == SwimPlayerInteractData.State.LookInspect)
        {
            return spid.inspectAnim;
        }
        else if (spid.state == SwimPlayerInteractData.State.LookCurious)
        {
            return spid.curiousAnim;
        }
        else if (spid.state == SwimPlayerInteractData.State.Poke)
        {
            return spid.pokeAnim;
        }

        return null;
    }

    void PlayStateLookSound(CritterInfo critter_info)
    {
        SwimPlayerInteractData spid = critter_info.swimPlayerInteractData;

        if (!spid.sfxEnabled)
        {
            return;
        }

        if (critter_info.swimPlayerInteractData.state == SwimPlayerInteractData.State.MoveScared)
        {
            AudioManager.PlayCritterSwimAway(critter_info);
        }
        else if (critter_info.swimPlayerInteractData.state == SwimPlayerInteractData.State.LookNotice)
        {
            AudioManager.PlaySFXAtObject(critter_info.critterObject, Vector3.zero, spid.sfxLookNotice);
        }
        else if (critter_info.swimPlayerInteractData.state == SwimPlayerInteractData.State.LookInspect)
        {
            AudioManager.PlaySFXAtObject(critter_info.critterObject, Vector3.zero, spid.sfxLookInspect);
        }
        else if (critter_info.swimPlayerInteractData.state == SwimPlayerInteractData.State.LookCurious)
        {
            AudioManager.PlaySFXAtObject(critter_info.critterObject, Vector3.zero, spid.sfxLookInspect);
        }
    }

    public override float EvaluatePriority(CritterInfo critter_info)
    {
        GeneralSpeciesData gsd = critter_info.generalSpeciesData;
        SwimPlayerInteractData sd = critter_info.swimPlayerInteractData;

        if(sd)
        {
            if(gsd.isPlayerInteract)
            {
                sd.currentPriorityValue = sd.priorityValue;
            }
            else
            {
                sd.currentPriorityValue = 0f;
            }

            return sd.currentPriorityValue;
        }
        else
        {
            return 0f;
        }
    }

    static void UpdatePlayerLookOffset(CritterInfo critter_info)
    {
        SwimPlayerInteractData spid = critter_info.swimPlayerInteractData;

        if (spid.stateTime > spid.lookSwitchEyeNextTime)
        {
            spid.lookSwitchEyeNextTime = spid.stateTime + RandomExt.FloatRange(spid.lookSwitchDirTimeMin, spid.lookSwitchDirTimeMax);

            if (critter_info.critterEyeData != null)
            {
                if (critter_info.critterEyeData == CameraManager.singleton.leftCamera)
                {
                    critter_info.critterEyeData.lookAt = CameraManager.singleton.rightCamera.transform;
                }
                else
                {
                    critter_info.critterEyeData.lookAt = CameraManager.singleton.leftCamera.transform;
                }
            }
        }

        if (spid.stateTime > spid.lookOffsetNextTime)
        {
            // default curious settings
            float yawMin = 4f;
            float yawMax = 22f;
            float pitchMin = 0f;
            float pitchMax = 3.5f;
            float nextTimeMin = 3.5f;
            float nextTimeMax = 6f;

            if (spid.state == SwimPlayerInteractData.State.LookScared)
            {
                // update for scared settings
                yawMin = 10f;
                yawMax = 28f;
                pitchMin = 2f;
                pitchMax = 5f;
                nextTimeMin = 0.175f;
                nextTimeMax = 0.65f;
            }

            spid.lookOffsetNextTime = spid.stateTime + RandomExt.FloatRange(nextTimeMin, nextTimeMax);
            bool bFlipYaw = RandomExt.CoinFlip();
            bool bFlipPitch = !bFlipYaw || RandomExt.CoinFlip();

            if ((spid.lookOffsetYaw > 0f && bFlipYaw) || (spid.lookOffsetYaw < 0f && !bFlipYaw))
            {
                spid.lookOffsetYaw = Mathf.Deg2Rad * RandomExt.FloatRange(-yawMax, -yawMin);
            }
            else if ((spid.lookOffsetYaw < 0f && bFlipYaw) || (spid.lookOffsetYaw > 0f && !bFlipYaw))
            {
                spid.lookOffsetYaw = Mathf.Deg2Rad * RandomExt.FloatRange(yawMin, yawMax);
            }
            else
            {
                spid.lookOffsetYaw = Mathf.Deg2Rad * RandomExt.FloatRange(-yawMax, yawMax);
            }

            if ((spid.lookOffsetPitch > 0f && bFlipPitch) || (spid.lookOffsetPitch < 0f && !bFlipPitch))
            {
                spid.lookOffsetPitch = Mathf.Deg2Rad * RandomExt.FloatRange(-pitchMax, -pitchMin);
            }
            else if ((spid.lookOffsetPitch < 0f && bFlipPitch) || (spid.lookOffsetPitch > 0f && !bFlipPitch))
            {
                spid.lookOffsetPitch = Mathf.Deg2Rad * RandomExt.FloatRange(pitchMin, pitchMax);
            }
            else
            {
                spid.lookOffsetPitch = Mathf.Deg2Rad * RandomExt.FloatRange(-pitchMax, pitchMax);
            }
        }
    }

    public static void GoToState(CritterInfo critter_info, SwimPlayerInteractData.State state)
    {
        GeneralMotionData gmd = critter_info.generalMotionData;
        SwimPlayerInteractData spid = critter_info.swimPlayerInteractData;
        ThrottledFishSteering tfs = (ThrottledFishSteering)critter_info.critterSteering;

        spid.state = state;
        spid.stateTime = 0f;
        spid.stateVisibleTime = 0f;
        spid.stateNotVisibleTime = 0f;

        float stateSpeedMult = 1.0f;
        float stateYawMult = 1.0f;

        switch (state)
        {
            case SwimPlayerInteractData.State.MoveNotice:
                stateSpeedMult = 1.2f;
                stateYawMult = 1.2f;
                spid.playerLastPos = spid.playerStartPos = CameraManager.GetCurrentCameraPosition();
                break;
            case SwimPlayerInteractData.State.MoveToPlayer:
                stateSpeedMult = spid.maxSpeedMult;
                spid.randomMoveOffset = new Vector3(RandomExt.FloatRangeAbs(spid.inspectDistance*0.1f, spid.inspectDistance*0.5f), RandomExt.FloatRangeAbs(spid.inspectDistance*0.02f, spid.inspectDistance*0.2f), 0f);
                spid.playerLastPos = spid.playerStartPos = CameraManager.GetCurrentCameraPosition();
//                AudioManager.PlayAudioSourceAtObject(critter_info.critterObject, Vector3.zero, (int)SoundFXID.ClownfishPlayInitial); 
                break;
            case SwimPlayerInteractData.State.MoveScared:
                if (spid.scaredAnim != null &&
                    !critter_info.animBase.playingOneOff)
                {
                    critter_info.animBase.PlayAnimation(spid.scaredAnim, false, true);
                    AudioManager.PlaySFXAtObject(critter_info.critterObject, Vector3.zero, SoundFXID.ClownfishInteractScared);
                }

                spid.randomMoveOffset = new Vector3(RandomExt.FloatRangeAbs(spid.inspectDistance*0.1f, spid.inspectDistance*0.5f), RandomExt.FloatRangeAbs(spid.inspectDistance*0.02f, spid.inspectDistance*0.2f), 0f);
                stateSpeedMult = spid.maxSpeedMult;
                break;
            case SwimPlayerInteractData.State.MoveCurious:
                stateSpeedMult = 1.25f;               
                spid.randomMoveOffset = new Vector3(RandomExt.FloatRangeAbs(spid.curiousDistance*0.1f, spid.curiousDistance*0.5f), RandomExt.FloatRangeAbs(spid.curiousDistance*0.02f, spid.curiousDistance*0.2f), 0f);
//                AudioManager.PlayAudioSourceAtObject(critter_info.critterObject, Vector3.zero, (int)SoundFXID.ClownfishPlayOffscreen); 
                break;
            case SwimPlayerInteractData.State.MoveInView:
                stateSpeedMult = 1.25f;
                spid.moveInViewFlipCount++;
                AudioManager.PlayCritterSwimAway(critter_info);
                break;
            case SwimPlayerInteractData.State.Spin:
                AudioManager.PlayCritterSwimAway(critter_info);
                break;
            case SwimPlayerInteractData.State.LookCurious:
                stateYawMult = 0.25f;
                break;
            case SwimPlayerInteractData.State.LookScared:
                stateYawMult = 0.5f;
                break;
            case SwimPlayerInteractData.State.LookInspect:
                stateYawMult = 0.5f;
                spid.playerLastPos = spid.playerStartPos = CameraManager.GetCurrentCameraPosition();
                break;
            default:
                break;
        }

        spid.statePlayedLookAnim = false;
        spid.lookOffsetYaw = 0f;
        spid.lookOffsetPitch = 0f;
        spid.lookOffsetNextTime = 0f;
        spid.lookSwitchEyeNextTime = 0f;
        spid.swimSpeed = spid.swimSpeedMult * gmd.swimSpeed * stateSpeedMult;

        tfs.throttleMaxSpeed = spid.throttleMaxSpeed * stateSpeedMult;
        tfs.throttleSpeedAccel = spid.throttleSpeedAccel * stateSpeedMult;
        tfs.throttleSpeedDecel = spid.throttleSpeedDecel * stateSpeedMult;

        tfs.yawAccel = spid.yawAccel * stateYawMult;
        tfs.yawDecel = spid.yawDecel * stateYawMult;
        tfs.yawMaxSpeed = spid.yawMaxSpeed * stateYawMult;

        if (state == SwimPlayerInteractData.State.MoveScared)
        {
            ++spid.scareCount;

            // exit behavior after enough scares
            if (spid.scareCount >= 4)
            {
                Disperse(critter_info);
            }
        }
    }

    static void Disperse(CritterInfo critter_info)
    {
        DisperseCollision.Disperse(critter_info.generalSpeciesData, null, CameraManager.GetCurrentCameraTransform(), critter_info.swimPlayerInteractData.scaredDistance * 1.5f, true);
        critter_info.generalSpeciesData.isPlayerInteract = false;
    }
}
