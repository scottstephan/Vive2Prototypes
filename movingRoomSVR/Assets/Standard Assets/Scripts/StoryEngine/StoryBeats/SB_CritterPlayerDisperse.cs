using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterPlayerDisperse : SBBase {
	
    public string designGroupName;
    public bool enablePlayerDisperse;

    bool bDone;

    static List<CritterInfo> searchCritters = new List<CritterInfo>();

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
            Debug.LogError("SB_CritterPlayerDisperse " + gameObject.name + "has empty designGroupName!");
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

        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if (c == null ||
                c.swimDisperseData == null)
            {
                continue;
            }

            if (enablePlayerDisperse)
            {
                c.swimDisperseData.playerDisperseDisableCount--;
            }
            else
            {
                c.swimDisperseData.playerDisperseDisableCount++;
            }
        }

#if UNITY_EDITOR
        Debug.Log("SB_CritterPlayerDisperse " + gameObject.name + " Group: " + designGroupName + (enablePlayerDisperse ? " enabled " : " disabled ") + "Count: " + searchCritters.Count);
#endif
        searchCritters.Clear();
        bDone = true;
	}
	
	
	public override bool IsComplete()
    {
        return bDone;
	}			
}
