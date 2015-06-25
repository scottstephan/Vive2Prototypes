using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterFollowPath : SBBase {
	
    public string designGroupName;
    public GameObject followObject;
    public AnimationClip followAnimClip;
    public float arrivalDistance = 100f;
    public float followDistance = 100f;
    public bool enableAvoidFish = false;
    public bool enableAvoidGround = false;
    public bool enablePlayerDisperse = false;
    public bool enableCritterDisperse = false;

    public bool disableCritterGroundCollision = false;

    public bool blockUntilComplete = true;

    private int designGroupHash;

    List<CritterInfo> searchCritters = new List<CritterInfo>();

    public enum FollowState {
        None,
        Triggered,
        InFollow,
        Finished
    }

    FollowState eState = FollowState.None;

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

    public override void BeginBeat() 
    {
        eState = FollowState.None;;

        if (string.IsNullOrEmpty(designGroupName))
        {
            Debug.LogError("SB_CritterFollowPath " + gameObject.name + "has empty designGroupName!");
            eState = FollowState.Finished;
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
        case FollowState.None:
        {
#if UNITY_EDITOR
            if (followObject != null)
            {
                Debug.Log("SB_CritterFollowPath " + gameObject.name + " START Group: " + designGroupName + " Follow: " + followObject.name + " Anim: " + (followAnimClip!=null?followAnimClip.name:"NULL") + " Count: " + searchCritters.Count);
            }
            else
            {
                Debug.Log("SB_CritterFollowPath " + gameObject.name + " START Group: " + designGroupName + " Follow STOP (no object) Count: " + searchCritters.Count);
            }
#endif

            for (int i=0; i<searchCritters.Count; ++i)
            {
                CritterInfo c = searchCritters[i];
                if (c == null ||
                    c.swimFollowPathData == null)
                {
                    continue;
                }

                // clear out current behavior
//                if (c.generalSpeciesData.myCurrentBehaviorType == SwimBehaviorType.SWIM_FOLLOWPATH)
//                {
//                    AI.ForceSwitchToBehavior(c, SwimBehaviorType.SWIM_FREE);
//                }

                c.critterSteering.desiredSteeringThrottle = 1f;
                c.swimFollowPathData.followObject = followObject;
                c.swimFollowPathData.followAnimClip = followAnimClip;
                c.swimFollowPathData.arrivalDistance = arrivalDistance;
                c.swimFollowPathData.followDistance = followDistance;
                c.swimFollowPathData.enableAvoidFish = enableAvoidFish;
                c.swimFollowPathData.enableAvoidGround = enableAvoidGround;
                c.swimFollowPathData.enablePlayerDisperse = enablePlayerDisperse;
                c.swimFollowPathData.enableCritterDisperse = enableCritterDisperse;
                c.swimFollowPathData.disableCritterGroundCollision = disableCritterGroundCollision;
                c.generalSpeciesData.switchBehavior = true;
            }

            if (followObject == null || !blockUntilComplete)
            {
                eState = FollowState.Finished;
            }
            else
            {
                eState = FollowState.Triggered;
            }

            break;
        }
        case FollowState.Triggered:
        {
            for (int i=0; i<searchCritters.Count; ++i)
            {
                CritterInfo c = searchCritters[i];
                if (c == null)
                {
                    continue;
                }
                
                if (c.generalSpeciesData.myCurrentBehaviorType == SwimBehaviorType.SWIM_FOLLOWPATH)
                {
                    eState = FollowState.InFollow;
                    break;
                }
            }
            break;
        }
        case FollowState.InFollow:
        {
            for (int i=0; i<searchCritters.Count; ++i)
            {
                CritterInfo c = searchCritters[i];
                if (c == null)
                {
                    continue;
                }
                
                if (c.generalSpeciesData.myCurrentBehaviorType != SwimBehaviorType.SWIM_FOLLOWPATH)
                {
                    eState = FollowState.Finished;

#if UNITY_EDITOR
                    if (followObject != null)
                    {
                        Debug.Log("SB_CritterFollowPath " + gameObject.name + " FINISH Group: " + designGroupName + " Follow: " + followObject.name + " Anim: " + (followAnimClip!=null?followAnimClip.name:"NULL") + " Count: " + searchCritters.Count);
                    }
                    else
                    {
                        Debug.Log("SB_CritterFollowPath " + gameObject.name + " FINISH Group: " + designGroupName + " Follow STOP (no object) Count: " + searchCritters.Count);
                    }
#endif

                    break;
                }
            }
            break;
        }
        }

	}
	

    public override void EndBeat() {
        searchCritters.Clear();
        base.EndBeat();
    }

	public override bool IsComplete()
    {
        return (eState == FollowState.Finished);
	}			
}
