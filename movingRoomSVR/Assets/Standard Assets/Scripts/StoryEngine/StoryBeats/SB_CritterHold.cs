using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public enum HoldBeatType {
	StartHold,
	StopHold
}

public class SB_CritterHold : SBBase {

    public HoldBeatType beatType = HoldBeatType.StopHold;

	public string designGroupName;

    public Transform lookAtTransform;

    static List<CritterInfo> searchCritters = new List<CritterInfo>();
    
    public override bool ContainsDesignGroup( string design_group ) { 
        if (string.IsNullOrEmpty( designGroupName ) ) {
            return false;
        }
        
        return ( designGroupName.ToUpper().Equals( design_group ) );
    }
    
    public override void BeginBeat() 
    {
        GetCrittersByDesignGroup(designGroupName, searchCritters);
#if UNITY_EDITOR
        Debug.Log("SB_CritterHold " + gameObject.name + " " + beatType + " Group:  " + designGroupName + " Count: " + searchCritters.Count);
#endif
        SwimBehaviorType newb = ( beatType == HoldBeatType.StartHold ) ? SwimBehaviorType.HOLD : SwimBehaviorType.SWIM_FREE;
        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if( c == null ) {
                continue;
            }

            if (c.holdData != null)
            {
                c.holdData.lookAtTransform = lookAtTransform;
            }

            AI.ForceSwitchToBehavior( c, newb );
        }

        searchCritters.Clear();

        base.BeginBeat();
	}	
	
	public override bool IsComplete()
    {
		return true;
	}			
}
