using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterDisperseSpeed : SBBase {
	
    public string designGroupName;
    public float extraSpeedMult = 1f;
    public float extraColliderDistMult = 1f;

    bool bDone;
    int designGroupHash;

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
            Debug.LogError("SB_CritterDisperseSpeed " + gameObject.name + "has empty designGroupName!");
        }
        else
        {
            designGroupHash = designGroupName.ToUpper().GetHashCode();
        }

        bDone = false;
        base.BeginBeat();
    }
    
    public override void UpdateBeat() 
    {           
        base.UpdateBeat();
        
        if (IsAddingCritters(designGroupHash))
        {
            return;
        }

        GetCrittersByDesignGroup(designGroupHash, searchCritters);

        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if (c == null ||
                c.swimDisperseData == null)
            {
                continue;
            }

            c.swimDisperseData.extraColliderDistMult = extraColliderDistMult;
            c.swimDisperseData.extraSpeedMult = extraSpeedMult;
        }

#if UNITY_EDITOR
        Debug.Log("SB_CritterDisperseSpeed " + gameObject.name + " Group: " + designGroupName + " extraSpeedMult: " + extraSpeedMult + " extraColliderMult " + extraColliderDistMult + " Count: " + searchCritters.Count);
#endif
        searchCritters.Clear();
        bDone = true;
	}
	
	
	public override bool IsComplete()
    {
        return bDone;
	}			
}
