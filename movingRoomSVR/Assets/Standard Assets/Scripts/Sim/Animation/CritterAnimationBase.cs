using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CritterAnimationBase : MonoBehaviour {
		
	[HideInInspector]
	public CritterInfo critterInfo;

	// TODO> need to figure out how to refactor this out. its a bad hook from our behaviors.
	// maybe move into a 'feelings' and 'tag' based system ala LMNO?
	[HideInInspector]
	public bool isFeeding = false;
	
	[HideInInspector]
	public bool playingOneOff = false;
    [HideInInspector]
    public bool pauseSim = false;

    [HideInInspector]
    public bool slowdownActive = false;
    
    [HideInInspector]
    public bool pauseActive = false;
    
    public Transform bodyTransform = null;
//    Vector3 bodyStartPos;
//    Quaternion bodyStartRot;

    List<string> playingOneOffNames;

    public bool AllowDisperse = true;

	#region VIRTUAL FUNCTIONS

	public virtual void Init() 
    {
//        if (bodyTransform != null)
//        {
//            bodyStartPos = bodyTransform.localPosition;
//            bodyStartRot = bodyTransform.localRotation;
//        }
    }	

    public virtual void CleanUp() {}

	public virtual void UpdateAnimation() {}
	
	public virtual string GetAnimationsPlayingString() { return "NO ANIMS PLAYING"; }

	#endregion
	
	#region BASIC ONE OFF SUPPORT

	void OneOffClipFinished()
    {
		playingOneOff = false;
        pauseSim = false;
	}

    public virtual void Teleport()
    {
    }

	// TODO>generalize the clip being passed in to include anim state machines and other types of animation controllers a la ANT.
	public virtual void AddAnimation( AnimationClip new_clip ) 
	{
        Animation anim = critterInfo.critterAnimation;
        if (anim)
		{
	        anim.AddClip(new_clip, new_clip.name);
	        anim[new_clip.name].wrapMode = WrapMode.Once;
	        anim[new_clip.name].layer = 10;
            anim[new_clip.name].normalizedTime = 0f;
		}				
	}	

    public virtual void PlayAnimation( AnimationClip new_clip, bool stop_sim, bool cross_fade ) 
    {
        Animation anim = critterInfo.critterAnimation;
        if (anim)
        {
            playingOneOff = true;
            anim[new_clip.name].normalizedTime = 0f;
            if( cross_fade )
            {             
                anim.CrossFade(new_clip.name);
            }
            else 
            {
                anim.Play(new_clip.name);
            }

            if (playingOneOffNames == null)
            {
                playingOneOffNames = new List<string>();
            }

            playingOneOffNames.Add(new_clip.name);

            pauseSim = stop_sim;
        }               
    }   

    public virtual void LateAnimationUpdate() 
    {
        if( playingOneOff )
        {
            if ( bodyTransform != null )
            {
//                Vector3 pos = bodyTransform.position;
//                Quaternion rot = bodyTransform.rotation;

//                critterInfo.critterTransform.position = pos;
//                critterInfo.critterTransform.rotation = rot;
//                bodyTransform.localPosition = bodyStartPos;
//                bodyTransform.localRotation = bodyStartRot;
            }

            Animation anim = critterInfo.critterAnimation;
            if (anim)
            {
                for (int i=0; i<playingOneOffNames.Count; ++i)
                {
                    if (!anim.IsPlaying(playingOneOffNames[i]))
                    {
                        playingOneOffNames.RemoveAt(i--);
                    }
                }

                if (playingOneOffNames.Count == 0)
                {
                    playingOneOff = false;
                }
            }
        }        
    }    

	#endregion
		
	#region DEBUG ANIMATION PLAYER

	private static int animationCycleThroughIndex = 0;
	public static string animationString = string.Empty;
	public static string animationTime = string.Empty;
	private static float currentAnimationTime = 0.0f;	
		
	private static ArrayList GetAllAnimationNames( Animation anim )
	{
		ArrayList result = new ArrayList();
		foreach(AnimationState state in anim)
		{
			result.Add(state.name);
		}
		return result;
		
	}
    	
	public static string CycleThroughAnimations( CritterInfo critter_info )
	{
		Animation anim = critter_info.critterAnimation;
		ArrayList listOfAnimations = GetAllAnimationNames(anim);
		string currentPlayingAnimation = string.Empty;
		if (anim && AppBase.Instance.RunningAsPreview())
        {
			if(InputManager.GetKeyDown(KeyCode.A))
			{
				//anim.Stop();
				animationCycleThroughIndex+=1;
				if(animationCycleThroughIndex > listOfAnimations.Count-1)
				{
					animationCycleThroughIndex = 0;
				}
			}
			anim.wrapMode = WrapMode.Loop;
			
			if(!anim.IsPlaying((string)listOfAnimations[animationCycleThroughIndex]))
				anim.Play((string)listOfAnimations[animationCycleThroughIndex]);
			animationString = "Name : " +(string)listOfAnimations[animationCycleThroughIndex];
			currentPlayingAnimation = (string)listOfAnimations[animationCycleThroughIndex];
			currentAnimationTime = anim[(string)listOfAnimations[animationCycleThroughIndex]].time;
			while(currentAnimationTime > anim[(string)listOfAnimations[animationCycleThroughIndex]].length)
			{
				currentAnimationTime -= anim[(string)listOfAnimations[animationCycleThroughIndex]].length;
			}
			animationTime = (currentAnimationTime).ToString("0.0000") + "/"+ anim[(string)listOfAnimations[animationCycleThroughIndex]].length.ToString("0.0000");
			
		}
		return currentPlayingAnimation;
	}

	#endregion

}
