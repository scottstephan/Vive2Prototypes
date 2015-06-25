using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_SetTriggerTransform : SBBase {
    
    public string designGroupName;
    public StoryBeatTrigger triggerToSet = null;

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
        bDone = false;
        
        if (string.IsNullOrEmpty(designGroupName))
        {
            Debug.LogError("SB_SetTriggerTransform " + gameObject.name + "has empty designGroupName!");
            bDone = true;
        }
        
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
        Debug.Log("SB_SetTriggerTransform " + gameObject.name + " Group: " + designGroupName + " Trigger: " + triggerToSet.name);
#endif

        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            triggerToSet.Reset();
            triggerToSet.triggerTransform = c.critterTransform;
        }
        
        searchCritters.Clear();
        bDone = true;
    }
    
    
    public override bool IsComplete()
    {
        return bDone;
    }           
}
