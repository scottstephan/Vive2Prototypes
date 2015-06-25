using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterHungerLevel : SBBase {
	
    public string designGroupName;
    public float hungerLevel;
    public bool canGetHungry = true;
    public string targetDesignGroupName;

    static List<CritterInfo> searchCritters = new List<CritterInfo>();

//    bool bDone;

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
            Debug.LogError("SB_CritterHungerLevel " + gameObject.name + "has empty designGroupName!");
        }

//        bDone = false;
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
       // Debug.Log("SB_CritterHungerLevel " + gameObject.name + " Group: "  + designGroupName + " Count: " + searchCritters.Count);
#endif
        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if (c != null &&
                c.generalSpeciesData != null)
            {
                c.generalSpeciesData.hungerLevel = hungerLevel;
                c.generalSpeciesData.canGetHungry = canGetHungry;
                c.generalSpeciesData.switchBehavior = true;
                if (!string.IsNullOrEmpty(targetDesignGroupName))
                {
                    c.targetDesignGroupName = targetDesignGroupName;
                }
            }
        }

        searchCritters.Clear();
//        bDone = true;
	}
	
	
	public override bool IsComplete()
    {
        return true;
	}			
}
