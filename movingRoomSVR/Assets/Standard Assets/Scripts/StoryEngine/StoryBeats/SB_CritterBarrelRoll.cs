using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterBarrelRoll : SBBase {
	
    public string designGroupName;

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
            Debug.LogError("SB_CritterBarrelRoll " + gameObject.name + "has empty designGroupName!");
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
            if (c == null)
            {
                continue;
            }

            ThrottledBarrelRollFishSteering tbrfs = c.critterSteering as ThrottledBarrelRollFishSteering;

            if (tbrfs != null)
            {
                tbrfs.barrelRoll = true;
            }
        }

#if UNITY_EDITOR
        Debug.Log("SB_CritterBarrelRoll " + gameObject.name + " Group: " + designGroupName);
#endif
        searchCritters.Clear();
        bDone = true;
	}
	
	
	public override bool IsComplete()
    {
        return bDone;
	}			
}
