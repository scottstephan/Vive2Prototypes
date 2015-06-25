using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterTriggerInteract : SBBase {
	
    public string designGroupName;
    public float maxInteractTime = 0f;
    public bool waitUntilFinished = false;

    CritterInfo interactCritter;

    public override bool ContainsDesignGroup( string design_group ) {
        if (string.IsNullOrEmpty( designGroupName ) ) {
            return false;
        }

        return ( designGroupName.ToUpper().Equals( design_group ) );
    }

    public override void BeginBeat() 
    {	
        interactCritter = null;
        base.BeginBeat();
	}

    public override void EndBeat()
    {
        if (string.IsNullOrEmpty(designGroupName))
        {
            Debug.LogError("SB_CritterTriggerInteract " + gameObject.name + "has empty designGroupName!");
        }

        base.EndBeat ();
        interactCritter = null;
    }

    public override void UpdateBeat()
    {
        if (interactCritter == null)
        {
            interactCritter = GetCritterInDesignGroup(designGroupName);

            if (interactCritter != null && 
                interactCritter.critterObject != null &&
                interactCritter.generalSpeciesData != null)
            {         
#if UNITY_EDITOR
                Debug.Log("SB_CritterTriggerInteract " + gameObject.name + " Group: " + designGroupName + " " + interactCritter.critterObject);
#endif
                if (interactCritter.swimPlayerInteractData != null)
                {
                    interactCritter.generalSpeciesData.switchBehavior = true;
                    interactCritter.generalSpeciesData.isPlayerInteract = true;
                    interactCritter.swimPlayerInteractData.maxInteractTime = maxInteractTime;
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogError("SB_CritterTriggerInteract " + gameObject.name + " Group: " + designGroupName + " does not have a SwimPlayerInteractData component!");
                }
#endif
            }
        }
        
        base.UpdateBeat ();
    }
	
	public override bool IsComplete()
    {
        if (interactCritter != null)
        {
            // critter got deleted
            if (interactCritter.critterObject == null ||
                interactCritter.generalSpeciesData == null)
            {
                return true;
            }
        }

        if (waitUntilFinished)
        {
            if (interactCritter != null &&
                !interactCritter.generalSpeciesData.isPlayerInteract)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return interactCritter != null;
        }
	}			
}
