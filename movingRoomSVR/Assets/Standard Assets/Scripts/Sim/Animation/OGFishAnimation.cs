using UnityEngine;
using System.Collections;

public class OGFishAnimation : CritterAnimationBase {
	public AnimationClip forwardSwimClip;
	public AnimationClip forwardSwimSlowClip;
	public AnimationClip forwardSwimFastClip;
	public AnimationClip hoverClip;
	public AnimationClip breatheClip;
	public Transform breatheTransform;
	
	public AnimationClip biteClip;
    public bool biteIsFullBody;
	
	[HideInInspector]
	public float feedTime = 0f;
	
	public float feedTimeMin = 1.5f;
	public float feedTimeMax = 3.5f;

	public bool canBitePlayer = false;
	public float bitePlayerMaxDistance = 200f;
	[HideInInspector]
	public float bitePlayerMaxDistanceSqrd;
	public float bitePlayerMaxDot = 0.866f;
	public float bitePlayerCoolDownTime = 8f;
	[HideInInspector]
	public float bitePlayerCoolDownTimer;

    public float bitePlayerTurnDot = 0.866f;
    public float bitePlayerTurnDist = 1000f;
    public float bitePlayerHeightOffset = 95f;
    public Transform biteProximityTransform;
    public bool biteKillsPlayer = false;
    
    public float breatheTimeMin = 1.5f;
    public float breatheTimeMax = 3.5f;
	[HideInInspector]
	public float breatheTime = -1.0f;	
	
    public float swimCrossFadeTime = 0.3f;
    public float swimFastNoSwitch = 0.4f;

	[HideInInspector]
	public Vector3 lastTrackingPosition;
	[HideInInspector]
	public float animTrackingSpeed = 0.0f;	
	[HideInInspector]
	public float animNotOkToSwitch = 0.0f;	
	
	[HideInInspector]
	public bool hasAllClips;
	[HideInInspector]
	public bool hasFeedClip;
	[HideInInspector]
	public bool hasBreatheClip;
	
	public static void CreateFromDeprecatedData( CritterInfo critter_info ) {
		OGFishAnimation new_data = critter_info.critterObject.AddComponent<OGFishAnimation>();
		critter_info.animBase = new_data;

		FishAnimationData og_data = critter_info.critterObject.GetComponent<FishAnimationData>();
		if( og_data == null ) {
			return;
		}
		
		new_data.forwardSwimClip = og_data.forwardSwimClip;
		new_data.forwardSwimSlowClip = og_data.forwardSwimSlowClip;
		new_data.forwardSwimFastClip = og_data.forwardSwimFastClip;
		new_data.hoverClip = og_data.hoverClip;
		new_data.breatheClip = og_data.breatheClip;
		new_data.breatheTransform = og_data.breatheTransform;	
		new_data.biteClip = og_data.biteClip;
		new_data.feedTimeMin = og_data.feedTimeMin;
		new_data.feedTimeMax = og_data.feedTimeMax;		
		new_data.breatheTimeMin = og_data.breatheTimeMin;
		new_data.breatheTimeMax = og_data.breatheTimeMax;		
	}

    public static void CreateFromDeprecatedData( GameObject critter, CritterInfoData critter_info ) {
        OGFishAnimation new_data = critter.AddComponent<OGFishAnimation>();
        critter_info.animBase = new_data;
        
        FishAnimationData og_data = critter.GetComponent<FishAnimationData>();
        if( og_data == null ) {
            return;
        }
        
        new_data.forwardSwimClip = og_data.forwardSwimClip;
        new_data.forwardSwimSlowClip = og_data.forwardSwimSlowClip;
        new_data.forwardSwimFastClip = og_data.forwardSwimFastClip;
        new_data.hoverClip = og_data.hoverClip;
        new_data.breatheClip = og_data.breatheClip;
        new_data.breatheTransform = og_data.breatheTransform;   
        new_data.biteClip = og_data.biteClip;
        new_data.feedTimeMin = og_data.feedTimeMin;
        new_data.feedTimeMax = og_data.feedTimeMax;     
        new_data.breatheTimeMin = og_data.breatheTimeMin;
        new_data.breatheTimeMax = og_data.breatheTimeMax;       
    }
    
    public override string GetAnimationsPlayingString()
    {
        string ret_str = "";

        if (critterInfo.critterAnimation.IsPlaying("_forwardSwim"))
        {
            ret_str = ret_str + "_forwardSwim ";
        }

        if (critterInfo.critterAnimation.IsPlaying("_forwardSwimSlow"))
        {
            ret_str = ret_str + "_forwardSwimSlow ";
        }

        if (critterInfo.critterAnimation.IsPlaying("_forwardSwimFast"))
        {
            ret_str = ret_str + "_forwardSwimFast ";
        }

        if (critterInfo.critterAnimation.IsPlaying("_hover"))
        {
            ret_str = ret_str + "_hover ";
        }

        if (critterInfo.critterAnimation.IsPlaying("_breathe"))
        {
            ret_str = ret_str + "_breathe ";
        }

        if (critterInfo.critterAnimation.IsPlaying("_bite"))
        {
            ret_str = ret_str + "_bite ";
        }

        return ret_str;
    }
	
    public void OnScriptAnimEnd()
    {        
        Animation anim = critterInfo.critterAnimation;
        if( forwardSwimClip ) {
            anim.CrossFade("_forwardSwim");
        }
    }

	public override void Init()
    {
        base.Init();

        Animation anim = critterInfo.critterAnimation;
        if (anim)
        {
            lastTrackingPosition = critterInfo.critterTransform.position;
            anim.wrapMode = WrapMode.Loop;

            bool has_all_clips = true;
            if (forwardSwimClip)
            {
                anim.AddClip(forwardSwimClip, "_forwardSwim");
                anim.clip = forwardSwimClip;
                anim["_forwardSwim"].time = RandomExt.FloatRange(0.0f, forwardSwimClip.length);
                anim["_forwardSwim"].layer = 0;
            }
            else
            {
                has_all_clips = false;
            }

            if (forwardSwimFastClip)
            {
                anim.AddClip(forwardSwimFastClip, "_forwardSwimFast");
                anim["_forwardSwimFast"].layer = 0;
            }
            else
            {
                has_all_clips = false;
            }

            if (forwardSwimSlowClip)
            {
                anim.AddClip(forwardSwimSlowClip, "_forwardSwimSlow");
                anim["_forwardSwimSlow"].layer = 0;
            }
            else
            {
                has_all_clips = false;
            }

            if (hoverClip)
            {
                anim.AddClip(hoverClip, "_hover");
                anim["_hover"].layer = 0;
            }
            else
            {
                has_all_clips = false;
            }

            if (breatheClip)
            {
                anim.AddClip(breatheClip, "_breathe");
                anim["_breathe"].wrapMode = WrapMode.Once;
                anim["_breathe"].layer = 1;
                anim["_breathe"].weight = 1.0f;
                anim["_breathe"].AddMixingTransform(breatheTransform);
                anim["_breathe"].blendMode = AnimationBlendMode.Additive;
                breatheTime = Random.Range(breatheTimeMin, breatheTimeMax);
                hasBreatheClip = true;
            }
            else
            {
                hasBreatheClip = false;
            }

			if (biteClip)
            {
                anim.AddClip(biteClip, "_bite");
                anim["_bite"].wrapMode = WrapMode.Once;
                anim["_bite"].layer = 2;
                anim["_bite"].weight = 1.0f;
                if( !biteIsFullBody ) {
                    //anim["_bite"].AddMixingTransform(breatheTransform);
                    anim["_bite"].blendMode = AnimationBlendMode.Additive;
                    //anim["_bite"].blendMode = AnimationBlendMode.Blend;
                }
				hasFeedClip = true;

				bitePlayerMaxDistanceSqrd = bitePlayerMaxDistance * bitePlayerMaxDistance;
            }
            else
            {
                hasFeedClip = false;
            }
			
            hasAllClips = has_all_clips;

			if( forwardSwimClip ) {
	            anim.Play("_forwardSwim");
			}
        }

        AllowDisperse = !biteKillsPlayer;
    }
	
    public override void CleanUp() {
        Animation anim = critterInfo.critterAnimation;

        AnimationState astate = anim["_forwardSwim"];
        if( astate != null ) {
            GameObject.Destroy(astate.clip);
            anim.RemoveClip("_forwardSwim");
        }

        astate = anim["_forwardSwimSlow"];
        if( astate != null ) {
            GameObject.Destroy(astate.clip);
            anim.RemoveClip("_forwardSwimSlow");
        }        

        astate = anim["_forwardSwimFast"];
        if( astate != null ) {
            GameObject.Destroy(astate.clip);
            anim.RemoveClip("_forwardSwimFast");
        }        

        astate = anim["_hover"];
        if( astate != null ) {
            GameObject.Destroy(astate.clip);
            anim.RemoveClip("_hover");
        }        

        astate = anim["_breathe"];
        if( astate != null ) {
            GameObject.Destroy(astate.clip);
            anim.RemoveClip("_breathe");
        }        

        astate = anim["_bite"];
        if( astate != null ) {
            GameObject.Destroy(astate.clip);
            anim.RemoveClip("_bite");
        }        
    }

    public override void UpdateAnimation()
    {
//		Profiler.BeginSample("UpdateAnimation2");
        Animation anim = critterInfo.critterAnimation;

        if (anim == null)
        {
            return;
        }

        GeneralSpeciesData gsd = critterInfo.generalSpeciesData;	
		GeneralMotionData gmd = critterInfo.generalMotionData;	
		if (!playingOneOff)
        {
            if (hasAllClips)
            {
                float dt = Time.deltaTime;

                if (SimInstance.Instance.slowdownActive)
                {
                    dt *= SimInstance.slowdownMultiplierInv;
                }
                //			GeneralMotionData sd = critter_info.generalMotionData;
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
                        if(anim.IsPlaying("_hover")) {
                            anim.CrossFade("_hover", swimCrossFadeTime);
                        }
                    }
                    else
                    {
                        animNotOkToSwitch = 2.0f;
    								
    					float animationTime = 0.0f;
    					if(anim.IsPlaying("_forwardSwimFast"))
    					{
    						animationTime = anim["_forwardSwimFast"].normalizedTime;
    					}
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
                            if (animTrackingSpeed < cs + 00 && ( gmd.desiredSpeed >= gmd.swimSpeed * 0.95f ) )
                            {
                                anim["_forwardSwimFast"].normalizedTime = animationTime;
                                anim.CrossFade("_forwardSwimFast", swimCrossFadeTime);
                                animNotOkToSwitch = swimFastNoSwitch;
                                //						WemoLog.Scott("fast");
                            }
                            else if (animTrackingSpeed > cs && ( gmd.desiredSpeed <= gmd.swimSpeed * 0.25f ) )
                            {
                                anim["_forwardSwimSlow"].normalizedTime = animationTime;
                                anim.CrossFade("_forwardSwimSlow", swimCrossFadeTime);
                                //						WemoLog.Scott("slow");
                            }
                            else
                            {
                                anim["_forwardSwim"].normalizedTime = animationTime;
                                anim.CrossFade("_forwardSwim", swimCrossFadeTime);
                                //						WemoLog.Scott("swim");
                            }
                        }
                    }
                    animTrackingSpeed = cs;
                }
    			
    			if( hasBreatheClip ) {
    	            breatheTime -= dt;
    	            if (breatheTime <= 0.0f)
    	            {
    	                anim["_breathe"].time = 0.0f;
    	                breatheTime = Random.Range(breatheTimeMin, breatheTimeMax);
    	                anim.CrossFade("_breathe", 0.3f);
                    }
    			}
    			
    			if( hasFeedClip ) {
    				if( canBitePlayer && !anim.IsPlaying("_bite") ) {
    					bitePlayerCoolDownTimer -= dt;
    					if( bitePlayerCoolDownTimer <= 0f ) {
    						// get relative position and direction to player.
                            Vector3 bitePos;
                            Vector3 biteFwd;
                            if (biteProximityTransform != null)
                            {
                                bitePos = biteProximityTransform.position;
                                biteFwd = biteProximityTransform.forward;
                            }
                            else
                            {
                                bitePos = critterInfo.cachedPosition;
                                biteFwd = critterInfo.cachedForward;
                            }

                            Vector3 og_dir = CameraManager.GetCurrentCameraPosition() - bitePos;
    						float mag = og_dir.magnitude;
    						Vector3 dir = og_dir * ( 1f / mag );
                            float d = Vector3.Dot( dir, biteFwd );

    						if( d > bitePlayerTurnDot && mag < bitePlayerTurnDist ) {
    							SwimFreeData sd = critterInfo.swimFreeData;
    							og_dir.y += bitePlayerHeightOffset;
    							sd.desiredDirection = og_dir.normalized;
    						}

    						if( mag < bitePlayerMaxDistance && d > bitePlayerMaxDot ) {
    							anim["_bite"].time = 0.0f;
    							bitePlayerCoolDownTimer = bitePlayerCoolDownTime;
    							feedTime = Random.Range(feedTimeMin, feedTimeMax);
    							anim.CrossFade("_bite",0.3f);
                            }
    					}
    				}

                    if (biteKillsPlayer)
                    {
                        // shark demo specific, fade to black.
                        if (!OculusCameraFadeManager.IsFading() 
                            && !AppBase.reloadActive 
                            && anim.IsPlaying("_bite"))
                        {
                            if (anim["_bite"].normalizedTime > 0.55f)
                            {
                                Debug.Log ("FADING TO BLACK!!!");
                                OculusCameraFadeManager.FadeToBlack(0.1f);
                                AppBase.Instance.DelayedReload(3f);
                            }
                        }
                    }

    				feedTime -= dt;
    				if(feedTime < 0f && isFeeding && gsd.isHungry)
    				{
    //					WemoLog.Eyal("play shark feed animation");
    					anim["_bite"].time = 0.0f;
    					feedTime = Random.Range(feedTimeMin, feedTimeMax);
    					anim.CrossFade("_bite",0.3f);
                    }
    			}
    			
                lastTrackingPosition = critterInfo.critterTransform.position;
            }
            else 
            {
                if (forwardSwimClip != null &&
                    !anim.isPlaying)
                {
                    anim.Play ("_forwardSwim");
                }
            }
        }
//		Profiler.EndSample();
    }
}
