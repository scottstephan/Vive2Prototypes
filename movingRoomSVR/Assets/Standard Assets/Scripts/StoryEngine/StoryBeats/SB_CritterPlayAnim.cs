using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterPlayAnim : SBBase {

	public string designGroupName;
    public AnimationClip clip;
    public bool blockUntilComplete;
    public bool fullBody;

    List<CritterInfo> searchCritters = new List<CritterInfo>();

    bool bDone;

    Vector3 startBodyPos;
    Quaternion startBodyRot;

    Vector3 endBodyPos;
    Quaternion endBodyRot;
    float timer = 0f;
#if UNITY_EDITOR
    public override void Start ()
    {
        bDone = false;
        timer = 0f;
        base.Start ();

        if (fullBody && !blockUntilComplete)
        {
            Debug.LogError("SB_CritterPlayAnim " + gameObject.name + " setBodyEndPos=true but does not block until complete (set blockUntilComplete=true)");
        }
    }
#endif    

    public override bool ContainsDesignGroup( string design_group ) { 
        if (string.IsNullOrEmpty( designGroupName ) ) {
            return false;
        }
        
        return ( designGroupName.ToUpper().Equals( design_group ) );
    }
    
    public override void BeginBeat() 
    {
        if (clip == null)
        {
            Debug.LogError("SB_CritterPlayAnim " + gameObject.name + " has no animation clip!");
            return;
        }

        GetCrittersByDesignGroup(designGroupName, searchCritters);
        #if UNITY_EDITOR
        Debug.Log("SB_CritterPlayAnim " + gameObject.name + " Clip: " + clip.name + " Group:  " + designGroupName + " Count: " + searchCritters.Count);
        #endif

        clip.wrapMode = WrapMode.Once;

        bool bGotRot = false; 

        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if( c == null ||
                c.critterAnimation == null ||
                c.animBase == null)            
            {
                continue;
            }

            if (c.critterAnimation.GetClip (clip.name) == null)
            {
                c.animBase.AddAnimation(clip);
            }
            else
            {
                c.critterAnimation[clip.name].wrapMode = WrapMode.Once;
                c.critterAnimation[clip.name].layer = 10;
                c.critterAnimation[clip.name].normalizedTime = 0f;
            }

            if (fullBody)
            {
//                c.critterAnimation[clip.name].weight = 1f;
                c.critterBendData.OFF = true;
            }

            if (fullBody && 
                !bGotRot)
            {
                Transform body = c.animBase.bodyTransform;
                if (body != null)
                {
                    startBodyPos = body.localPosition;
                    startBodyRot = body.localRotation;    
                    c.critterAnimation.Play (clip.name);
                    c.critterAnimation[clip.name].normalizedTime = 1f;
                    c.critterAnimation.Sample();
                    endBodyPos = body.position;
                    endBodyRot = body.localRotation;
                    c.critterAnimation[clip.name].normalizedTime = 0f;
                    c.critterAnimation.Stop (clip.name);
                    bGotRot = true;
                }
            }

            c.animBase.PlayAnimation(clip, false, !fullBody);
        }

        if (blockUntilComplete)
        {
            timer = clip.length;
//            StartCoroutine(WaitForAnim());
        }
        else
        {
            bDone = true;
        }

        base.BeginBeat();
	}	

    public override void UpdateBeat ()
    {
        base.UpdateBeat ();

        if (timer > 0f)
        {
            float dt = GetDeltaTime();

            timer -= dt;

            if (timer <= 0)
            {
                ResetFullyBodyPositions();
                bDone = true;
            }
        }
    }

    IEnumerator WaitForAnim()
    {
        yield return new WaitForSeconds(clip.length - 0.03f);
        ResetFullyBodyPositions();
        bDone = true;
    }

    bool IsCritterPlayingAnim()
    {
        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if( c == null ||
               c.critterAnimation == null) 
            {
                continue;
            }

            if (c.critterAnimation.IsPlaying(clip.name))
//                if (c.critterAnimation.IsPlaying(clip.name) &
//                c.critterAnimation[clip.name].length - c.critterAnimation[clip.name].time > 0.04f)
            {
                return true;
            }
        }

        return false;
    }
	
    public override void EndBeat ()
    {
        base.EndBeat ();

        searchCritters.Clear();
    }

    void ResetFullyBodyPositions()
    {
        if (fullBody)
        {
            for (int i=0; i < searchCritters.Count; ++i)
            {
                CritterInfo c = searchCritters[i];

                c.critterAnimation.Stop (clip.name);

                Transform body = c.animBase.bodyTransform;
                
                if (body != null)
                {
                    body.localPosition = startBodyPos;
                    body.localRotation = startBodyRot;    
                    c.critterTransform.position = endBodyPos;
                    c.critterTransform.rotation = endBodyRot * Quaternion.Inverse(startBodyRot);
                    c.UpdateCachedTransformData();
                }

                c.critterBendData.OFF = false;
            }
        }
    }

	public override bool IsComplete()
    {
        return bDone;
//        if (blockUntilComplete)
//        {
//            return !IsCritterPlayingAnim();
//        }
//        else
//        {
//            return true;
//        }
	}			
}
