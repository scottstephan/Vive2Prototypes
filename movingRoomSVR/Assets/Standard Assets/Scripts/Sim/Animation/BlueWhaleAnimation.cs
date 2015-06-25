using UnityEngine;
using System.Collections;

public class BlueWhaleAnimation : CritterAnimationBase {
	public AnimationClip forwardSwimClip;
    public AnimationClip forwardSwimSlowClip;
    public AnimationClip forwardSwimFastClip;
    public AnimationClip hoverClip;

    public bool useProceduralBlink;
    public bool useProceduralLook;

    public AnimationClip blinkClip; 
    public float blinkTimeRandMin = 5f; 
    public float blinkTimeRandMax = 9f; 

    public float lookChangeTimeMin = 3f; 
    public float lookChangeTimeMax = 10f; 

    public float interpLookTimeMin = 0.2f; 
    public float interpLookTimeMax = 0.4f; 

    public float playerLookDist = 1450f;
    public float playerlookTime = 3.25f;

    bool bLookAtPlayer;
    float blinkTimer;
    float lookTimer;
    float interpLookTime;
    Vector3 goalLookDirLeft;
    Vector3 goalLookDirRight;
    Vector3 startLookDirLeft;
    Vector3 startLookDirRight;

    Vector3 lastTrackingPosition;
    float animTrackingSpeed = 0.0f;  
    float animNotOkToSwitch = 0.0f;  

    float saccadeTime = 0.0f;  
    float saccadeTimer = 0.0f;  
    Vector3 saccadeDir;
    Vector3 prevSaccadeDir;

    public override string GetAnimationsPlayingString()
    {
        string ret_str = "";

        if (critterInfo.critterAnimation.IsPlaying("_forwardSwim"))
        {
            ret_str = ret_str + "_forwardSwim ";
        }

        if (critterInfo.critterAnimation.IsPlaying("_introClip"))
        {
            ret_str = ret_str + "_introClip ";
        }

        if (critterInfo.critterAnimation.IsPlaying("_outroClip"))
        {
            ret_str = ret_str + "_outroClip ";
        }
        return ret_str;
    }
	
	public override void Init()
    {
        base.Init ();

        Animation anim = critterInfo.critterAnimation;
        if (anim)
        {
            anim.wrapMode = WrapMode.Loop;

            if (forwardSwimClip)
            {
                anim.AddClip(forwardSwimClip, "_forwardSwim");
                anim.clip = forwardSwimClip;
                anim["_forwardSwim"].time = RandomExt.FloatRange(0.0f, forwardSwimClip.length);
                anim["_forwardSwim"].layer = 0;
            }

            anim.Play("_forwardSwim");

//            if (forwardSwimFastClip)
//            {
//                anim.AddClip(forwardSwimFastClip, "_forwardSwimFast");
//                anim["_forwardSwimFast"].layer = 0;
//            }

            if (forwardSwimSlowClip)
            {
                anim.AddClip(forwardSwimSlowClip, "_forwardSwimSlow");
                anim["_forwardSwimSlow"].layer = 0;
            }
            
            if (hoverClip)
            {
                anim.AddClip(hoverClip, "_hover");
                anim["_hover"].layer = 0;
            }
        }

        if (blinkClip != null)
        {
            AddAnimation(blinkClip);
            blinkTimer = RandomExt.FloatRange(blinkTimeRandMin, blinkTimeRandMax);
        }

        lookTimer = -1f;

        if (critterInfo.critterEyeData != null)
        {
            critterInfo.critterEyeData.OFF = !useProceduralLook;
        }
    }

    public override void UpdateAnimation()
    {
        Animation anim = critterInfo.critterAnimation;
       	if (anim == null)
        {
            return;
        }

		if (!playingOneOff)
        {
			if (!anim.IsPlaying("_forwardSwim"))
            {
                anim.CrossFade("_forwardSwim");
            }

            if (useProceduralBlink && blinkClip != null)
            {
                blinkTimer -= Time.deltaTime;

                if (blinkTimer < 0f)
                {
                    PlayAnimation(blinkClip, false, true);
                    blinkTimer = RandomExt.FloatRange(blinkTimeRandMin, blinkTimeRandMax);

                    float centerYaw = 82.5f * Mathf.Deg2Rad;
                    startLookDirLeft = critterInfo.critterEyeData.lookAtDirectionLeft;
                    startLookDirRight = critterInfo.critterEyeData.lookAtDirectionRight;
                    goalLookDirLeft = MathfExt.YawVector(critterInfo.cachedForward, centerYaw);
                    goalLookDirRight = MathfExt.YawVector(critterInfo.cachedForward, -centerYaw);
                    lookTimer = interpLookTime = blinkClip.length * 0.23f;
                }
            }

            float dt = Time.deltaTime;
            
            if (SimInstance.Instance.slowdownActive)
            {
                dt *= SimInstance.slowdownMultiplierInv;
            }
            //          GeneralMotionData sd = critter_info.generalMotionData;
            float cs = (lastTrackingPosition - critterInfo.cachedPosition).magnitude * (1.0f / dt);
            
            if (animNotOkToSwitch > 0.0f)
            {
                animNotOkToSwitch -= dt;
            }
            if (animNotOkToSwitch <= 0.0f)
            {
                if ( cs == 0.0f
                    || critterInfo.generalSpeciesData.myCurrentBehaviorType == SwimBehaviorType.SWIM_IDLE)
                {
                    anim.CrossFade("_hover", 1f);
                }
                else
                {
                    animNotOkToSwitch = 10.0f;
                    
                    float animationTime = 0.0f;
//                    if(anim.IsPlaying("_forwardSwimFast"))
//                    {
//                        animationTime = anim["_forwardSwimFast"].normalizedTime;
//                    }
                    if(anim.IsPlaying("_forwardSwimSlow"))
                    {
                        animationTime = anim["_forwardSwimSlow"].normalizedTime;
                    }
                    if(anim.IsPlaying("_forwardSwim"))
                    {
                        animationTime = anim["_forwardSwim"].normalizedTime;
                    }
                    
                    if (!MathfExt.Approx(animTrackingSpeed, cs, 0.001f))
                    {
                        GeneralMotionData gmd = critterInfo.generalMotionData;  

                        /*if ( gmd.desiredSpeed >= gmd.maxSpeed * 0.95f ) 
                        {
                            anim["_forwardSwimFast"].normalizedTime = animationTime;
                            anim.CrossFade("_forwardSwimFast", anim["_forwardSwimFast"].length);
                        }
                        else */if (gmd.desiredSpeed <= gmd.swimSpeed * 0.25f ) 
                        {
                            anim["_forwardSwimSlow"].normalizedTime = animationTime;
                            anim.CrossFade("_forwardSwimSlow", anim["_forwardSwimSlow"].length);
                        }
                        else
                        {
                            anim["_forwardSwim"].normalizedTime = animationTime;
                            anim.CrossFade("_forwardSwim", anim["_forwardSwim"].length);
                        }
                    }
                }
                animTrackingSpeed = cs;
            }
            

            lastTrackingPosition = critterInfo.cachedPosition;
        }

        if (useProceduralLook &&
            critterInfo.critterEyeData != null)
        {
            if (interpLookTime > 0f)
            {
                lookTimer -= Time.deltaTime;

                if (lookTimer < 0f)
                {
                    lookTimer = RandomExt.FloatRange(lookChangeTimeMin, lookChangeTimeMax);
                    interpLookTime = -1f;
                }
                else
                {
                    if (bLookAtPlayer)
                    {
                        LookAtPlayer();
                    }

                    float blend = 1f-lookTimer/interpLookTime;
                    critterInfo.critterEyeData.lookAtDirectionLeft.x = Mathf.SmoothStep(startLookDirLeft.x, goalLookDirLeft.x, blend);
                    critterInfo.critterEyeData.lookAtDirectionLeft.y = Mathf.SmoothStep(startLookDirLeft.y, goalLookDirLeft.y, blend);
                    critterInfo.critterEyeData.lookAtDirectionLeft.z = Mathf.SmoothStep(startLookDirLeft.z, goalLookDirLeft.z, blend);
                    critterInfo.critterEyeData.lookAtDirectionRight.x = Mathf.SmoothStep(startLookDirRight.x, goalLookDirRight.x, blend);
                    critterInfo.critterEyeData.lookAtDirectionRight.y = Mathf.SmoothStep(startLookDirRight.y, goalLookDirRight.y, blend);
                    critterInfo.critterEyeData.lookAtDirectionRight.z = Mathf.SmoothStep(startLookDirRight.z, goalLookDirRight.z, blend);
                }
            }
            else
            {
                lookTimer -= Time.deltaTime;

                if (lookTimer < 0f)
                {
                    SetLook();
                }
                else 
                {
                    if (bLookAtPlayer)
                    {
                        LookAtPlayer();
                    }

                    saccadeTimer -= Time.deltaTime;

                    if (saccadeTimer < 0f)
                    {
                        saccadeTime = saccadeTimer = RandomExt.FloatRange(0.15f, 0.25f);
                        prevSaccadeDir = saccadeDir;
                        goalLookDirLeft += prevSaccadeDir;
                        goalLookDirRight += prevSaccadeDir;
                        saccadeDir = new Vector3(RandomExt.FloatRange(0.01f, 0.02f), RandomExt.FloatRange(0.01f, 0.02f), RandomExt.FloatRange(0.01f, 0.02f));
                    }
                    else if (bLookAtPlayer)
                    {
                        goalLookDirLeft += prevSaccadeDir;
                        goalLookDirRight += prevSaccadeDir;
                    }

                    float blend = 1f-saccadeTimer/saccadeTime;
                    critterInfo.critterEyeData.lookAtDirectionRight.x = Mathf.SmoothStep(goalLookDirRight.x, goalLookDirRight.x + saccadeDir.x, blend);
                    critterInfo.critterEyeData.lookAtDirectionRight.y = Mathf.SmoothStep(goalLookDirRight.y, goalLookDirRight.y + saccadeDir.y, blend);
                    critterInfo.critterEyeData.lookAtDirectionRight.z = Mathf.SmoothStep(goalLookDirRight.z, goalLookDirRight.z + saccadeDir.z, blend);
                    critterInfo.critterEyeData.lookAtDirectionLeft.x = Mathf.SmoothStep(goalLookDirLeft.x, goalLookDirLeft.x + saccadeDir.x, blend);
                    critterInfo.critterEyeData.lookAtDirectionLeft.y = Mathf.SmoothStep(goalLookDirLeft.y, goalLookDirLeft.y + saccadeDir.y, blend);
                    critterInfo.critterEyeData.lookAtDirectionLeft.z = Mathf.SmoothStep(goalLookDirLeft.z, goalLookDirLeft.z + saccadeDir.z, blend);
                }
            }
        }
    }

    public void LookAtPlayer()
    {
        Vector3 playerPos = CameraManager.GetEyePosition();
        goalLookDirLeft = (playerPos - critterInfo.critterEyeData.leftEye.position).normalized;
        goalLookDirLeft = (playerPos - critterInfo.critterEyeData.rightEye.position).normalized;
    }

    public void SetLook()
    {
        critterInfo.critterEyeData.useDirection = true;
        startLookDirLeft = critterInfo.critterEyeData.lookAtDirectionLeft;
        startLookDirRight = critterInfo.critterEyeData.lookAtDirectionRight;

        // alternate looking at player
        if (bLookAtPlayer)
        {
            bLookAtPlayer = false;
        }
        else
        {
            Vector3 leftEyePos = critterInfo.critterEyeData.leftEye.position;

            float PLAYERDISTSQR = playerLookDist*playerLookDist;
            bLookAtPlayer = (CameraManager.GetEyePosition() - leftEyePos).sqrMagnitude < PLAYERDISTSQR;
            if (!bLookAtPlayer)
            {
                Vector3 rightEyePos = critterInfo.critterEyeData.rightEye.position;
                bLookAtPlayer = (CameraManager.GetEyePosition() - rightEyePos).sqrMagnitude < PLAYERDISTSQR;
            }
        }

        if (bLookAtPlayer)
        {
            LookAtPlayer();
            lookTimer = interpLookTime = playerlookTime;
        }
        else
        {
            float randYaw = RandomExt.FloatRange(71f, 87f) * Mathf.Deg2Rad;
            float randPitch = RandomExt.FloatRange(-5f, 5f) * Mathf.Deg2Rad;
            goalLookDirLeft = MathfExt.PitchVector (MathfExt.YawVector(critterInfo.critterTransform.forward, randYaw), randPitch);
            goalLookDirRight = MathfExt.PitchVector (MathfExt.YawVector(critterInfo.critterTransform.forward, -randYaw), randPitch);
            lookTimer = interpLookTime = RandomExt.FloatRange(interpLookTimeMin, interpLookTimeMax);
        }

        saccadeTimer = 0f;
    }
    
    public override void Teleport()
    {
        SetLook ();
        critterInfo.critterEyeData.lookAtDirectionLeft = goalLookDirLeft;
        critterInfo.critterEyeData.lookAtDirectionRight = goalLookDirRight;
        lookTimer = RandomExt.FloatRange(lookChangeTimeMin, lookChangeTimeMax);
        interpLookTime = -1f;
    }    
}
