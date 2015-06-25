using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterAggroLevel : SBBase {
	
    // These values override prefab values that don't otherwise change,
    // so if they ever need to be set to the prefab original value, 
    // that would need to get stored somewhere else, in the CritterInfo probably.
    public string designGroupName;
    public float aggressionLevel;
    public float searchForVictim = 40f;
    public float chaseRadius = 100f;
    public float maxChaseTime = 3f;
    public float relativeSizeToAttack = 20f;
    public string chaseDesignGroupName;

    static List<CritterInfo> searchCritters = new List<CritterInfo>();

    bool bDone;
    
    public override bool ContainsDesignGroup( string design_group ) { 
        bool first = ( !string.IsNullOrEmpty( designGroupName ) 
                      && designGroupName.ToUpper().Equals( design_group ));
        bool second = ( !string.IsNullOrEmpty( chaseDesignGroupName ) 
                       && chaseDesignGroupName.ToUpper().Equals( design_group ) );

        return ( first || second );
    }
    

    public override void BeginBeat() 
    {
        if (string.IsNullOrEmpty(designGroupName))
        {
            Debug.LogError("SB_CritterAggroLevel " + gameObject.name + "has empty designGroupName!");
        }

        bDone = false;
        base.BeginBeat();
    }
    
    public override void UpdateBeat() 
    {           
        base.UpdateBeat();
        
        if (IsAddingCritters(designGroupName))
        {
            return;
        }

        GetCrittersByDesignGroup(designGroupName, searchCritters);

#if UNITY_EDITOR
        Debug.Log("SB_CritterAggroLevel "  + gameObject.name + " Group: " + designGroupName + " Count: " + searchCritters.Count);
#endif

        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if (c != null &&
                c.generalSpeciesData != null)
            {
                c.generalSpeciesData.aggressionLevel = aggressionLevel;
                c.generalSpeciesData.switchBehavior = true;

                if (c.swimChaseData != null)
                {
                    c.swimChaseData.searchForVictim = searchForVictim;
                    c.swimChaseData.chaseRadius = chaseRadius;
                    c.swimChaseData.maxChaseTime = maxChaseTime;
                    c.swimChaseData.relativeSizeToAttack = relativeSizeToAttack;
                    c.swimChaseData.chaseDesignGroupName = chaseDesignGroupName;
                }
            }
        }

        searchCritters.Clear();
        bDone = true;
	}
	
	
	public override bool IsComplete()
    {
        return bDone;
	}			
}
