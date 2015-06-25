using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterFishBowl : SBBase {
	
    public string designGroupName;
    public GameObject fishBowlObject;

    static List<CritterInfo> searchCritters = new List<CritterInfo>();

    bool bDone;
    
    public override bool ContainsDesignGroup( string design_group ) { 
        if (string.IsNullOrEmpty( designGroupName ) ) {
            return false;
        }
        
        return ( designGroupName.ToUpper().Equals( design_group ) );
    }
    
    public override void BeginBeat() 
    {
        if (string.IsNullOrEmpty(designGroupName))
        {
            Debug.LogError("SB_CritterFishBowl " + gameObject.name + "has empty designGroupName!");
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

        if (fishBowlObject == null)
        {
            Debug.LogError("SB_CritterFishBowl NULL FISHBOWL " + gameObject.name);
            bDone = true;
            return;
        }

        GetCrittersByDesignGroup(designGroupName, searchCritters);

        FishBowl fb = fishBowlObject.GetComponent<FishBowl>();

        if (fb == null)
        {
            Debug.LogError("SB_CritterFishBowl NO FISHBOWL COMPONENT on " + fishBowlObject.name);
            bDone = true;
            return;
        }

        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if (c == null ||
                c.generalSpeciesData == null)
            {
                continue;
            }

            c.generalSpeciesData.fishBowlData = fb.fishBowlData;
            c.generalSpeciesData.switchBehavior = true;

            if (c.swimFreeData != null)
            {
                c.swimFreeData.outside = false;
                // needs to get cleared to be reset by the behavior
                c.swimFreeData.outsideBowlDirectionSet = false;
            }
        }

#if UNITY_EDITOR
//        Debug.Log("SB_CritterFishBowl " + gameObject.name + " Group: " + designGroupName + " Fishbowl: " + fishBowlObject.name +" Count: " + searchCritters.Count);
#endif
        searchCritters.Clear();
        bDone = true;
	}
	
	
	public override bool IsComplete()
    {
        return bDone;
	}			
}
