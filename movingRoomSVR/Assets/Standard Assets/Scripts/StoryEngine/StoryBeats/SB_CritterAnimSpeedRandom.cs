using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterAnimSpeedRandom : SBBase {
	
    public string designGroupName;
    public float randAnimSpeedVariance = 0.2f;

    private int designGroupHash;
    private bool bDone;

    static List<CritterInfo> searchCritters = new List<CritterInfo>();

    public override bool ContainsDesignGroup( string design_group ) { 
        if (string.IsNullOrEmpty( designGroupName ) ) {
            return false;
        }
        
        return ( designGroupName.ToUpper().Equals( design_group ) );
    }
    
    public override void BeginBeat() 
    {
        bDone = false;

        if (string.IsNullOrEmpty(designGroupName))
        {
            Debug.LogError("SB_CritterAnimSpeedRandom " + gameObject.name + "has empty designGroupName!");
        }
        else
        {
            designGroupHash = designGroupName.ToUpper().GetHashCode();
        }

        base.BeginBeat();
    }

    public override void UpdateBeat() 
    {	        
        base.UpdateBeat();

        if (IsAddingCritters(designGroupHash) || SimInstance.Instance.IsSimPaused() )
        {
            return;
        }

        GetCrittersByDesignGroup(designGroupHash, searchCritters);

#if UNITY_EDITOR
        Debug.Log("SB_CritterAnimSpeedRandom " + gameObject.name + " Group: " + designGroupName + " Count: " + searchCritters.Count);
#endif

        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if (c == null ||
                c.critterAnimation == null)
            {
                continue;
            }

            float randVariance = RandomExt.FloatRange(0f,randAnimSpeedVariance);
            if (SimInstance.Instance.slowdownActive)
            {
                randVariance /= SimInstance.slowdownMultiplier;
            }

            foreach (AnimationState anim in c.critterAnimation)
            {
                anim.speed *= 1.0f + randVariance;
            }

        }

        bDone = true;

        searchCritters.Clear();
	}
	
	
	public override bool IsComplete()
    {
        return bDone;
	}			
}
