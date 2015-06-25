using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterGoToPoint : SBBase {
	
    public string designGroupName;
    public Transform goToTransform;
    public float arrivalDistance = 100f;
    public float swimSpeedMult = 1f;
    public bool enableAvoidFish = true;
    public bool enableAvoidGround = true;
    public bool enablePlayerDisperse = true;
    public bool enableCritterDisperse = true;
    public bool disableCritterGroundCollision = false;

    public bool blockUntilComplete = true;
    private int designGroupHash;

    List<CritterInfo> searchCritters = new List<CritterInfo>();

    public enum GoToState {
        None,
        Triggered,
        InGoTo,
        Finished
    }

    GoToState eState = GoToState.None;

    public override bool ContainsDesignGroup( string design_group ) { 
        if (string.IsNullOrEmpty( designGroupName ) ) {
            return false;
        }
        
        return ( designGroupName.ToUpper().Equals( design_group ) );
    }
    
    public override void Reset() {
        searchCritters.Clear();
        base.Reset();
    }

    public override void EndBeat() {
        searchCritters.Clear();
        base.EndBeat();
    }

    public override void BeginBeat() 
    {
        eState = GoToState.None;;

        if (string.IsNullOrEmpty(designGroupName))
        {
            Debug.LogError("SB_CritterGoToPoint " + gameObject.name + "has empty designGroupName!");
            eState = GoToState.Finished;
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

        if (IsAddingCritters(designGroupHash))
        {
            return;
        }

        if( searchCritters.Count <= 0 ) {
            GetCrittersByDesignGroup(designGroupHash, searchCritters);
        }
        
        switch( eState ) {
        case GoToState.None:
        {
#if UNITY_EDITOR
            if (goToTransform != null)
            {
//                Debug.Log("SB_CritterGoToPoint " + gameObject.name + " START Group: " + designGroupName + " GoTo: " + goToTransform.name + " Count: " + searchCritters.Count);
            }
            else
            {
//                Debug.Log("SB_CritterGoToPoint " + gameObject.name + " START Group: " + designGroupName + " GoTo STOP (no object) Count: " + searchCritters.Count);
            }
#endif

            for (int i=0; i<searchCritters.Count; ++i)
            {
                CritterInfo c = searchCritters[i];
                if (c == null ||
                    c.swimScriptGoToPointData == null)
                {
                    continue;
                }

                // clear out current behavior
//                if (c.generalSpeciesData.myCurrentBehaviorType == SwimBehaviorType.SWIM_SCRIPT_GOTO)
//                {
//                    AI.ForceSwitchToBehavior(c, SwimBehaviorType.SWIM_FREE);
//                }

                c.critterSteering.desiredSteeringThrottle = 1f;
                c.swimScriptGoToPointData.goToTransform = goToTransform;
                c.swimScriptGoToPointData.arrivalDistance = arrivalDistance;
                c.swimScriptGoToPointData.swimSpeedMult = swimSpeedMult;
                c.swimScriptGoToPointData.enableAvoidFish = enableAvoidFish;
                c.swimScriptGoToPointData.enableAvoidGround = enableAvoidGround;
                c.swimScriptGoToPointData.enableCritterDisperse = enableCritterDisperse;
                c.swimScriptGoToPointData.enablePlayerDisperse = enablePlayerDisperse;
                c.swimScriptGoToPointData.disableCritterGroundCollision = disableCritterGroundCollision;
                c.generalSpeciesData.switchBehavior = true;
            }

            if (goToTransform == null || !blockUntilComplete)
            {
                eState = GoToState.Finished;
            }
            else
            {
                eState = GoToState.Triggered;
            }

            break;
        }
        case GoToState.Triggered:
        {
            for (int i=0; i<searchCritters.Count; ++i)
            {
                CritterInfo c = searchCritters[i];
                if (c == null)
                {
                    continue;
                }
                
                if (c.generalSpeciesData.myCurrentBehaviorType == SwimBehaviorType.SWIM_SCRIPT_GOTO)
                {
                    eState = GoToState.InGoTo;
                    break;
                }
            }
            break;
        }
        case GoToState.InGoTo:
        {
            for (int i=0; i<searchCritters.Count; ++i)
            {
                CritterInfo c = searchCritters[i];
                if (c == null)
                {
                    continue;
                }
                
                if (c.generalSpeciesData.myCurrentBehaviorType != SwimBehaviorType.SWIM_SCRIPT_GOTO &&
                    c.swimScriptGoToPointData.goToTransform != goToTransform)
                {
                    eState = GoToState.Finished;

#if UNITY_EDITOR
                    if (goToTransform != null)
                    {
//                        Debug.Log("SB_CritterGoToPoint " + gameObject.name + " FINISH Group: " + designGroupName + " GoTo: " + goToTransform.name + " Count: " + searchCritters.Count);
                    }
                    else
                    {
  //                      Debug.Log("SB_CritterGoToPoint " + gameObject.name + " FINISH Group: " + designGroupName + " GoTo STOP (no object) Count: " + searchCritters.Count);
                    }
#endif

                    break;
                }
            }
            break;
        }
        }
	}
	
	
	public override bool IsComplete()
    {
        return (eState == GoToState.Finished);
	}			
}
