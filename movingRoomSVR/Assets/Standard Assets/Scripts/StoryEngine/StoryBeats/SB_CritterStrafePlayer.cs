using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public enum StrafeBeatType {
	StartStrafe,
	StopStrafe
}

public class SB_CritterStrafePlayer : SBBase {

    public StrafeBeatType beatType = StrafeBeatType.StartStrafe;

	public string designGroupName;

    public float forwardDistanceMin = 50f;
    public float forwardDistanceMax = 250f;
    public float horizDistanceMin = 50f;
    public float horizDistanceMax = 200f;
    public float vertDistanceMin = 0f;
    public float vertDistanceMax = 100f;

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
        Debug.Log("SB_CritterStrafePlayer " + gameObject.name + " Group: " + designGroupName + " Count: " + searchCritters.Count);
#endif
        SwimBehaviorType newb = ( beatType == StrafeBeatType.StartStrafe ) ? SwimBehaviorType.SWIM_STRAFE_PLAYER : SwimBehaviorType.SWIM_FREE;
        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if( c == null)
            {
                continue;
            }

            SwimStrafePlayerData sd = c.swimStrafePlayerData;

            if (sd == null ) 
            {
                Debug.LogError ("SB_CritterStrafePlayer" + gameObject.name + " critter has no SwimStrafePlayerData! Group: " + designGroupName);
                continue;
            }

            sd.forwardDistanceMin = forwardDistanceMin;
            sd.forwardDistanceMax = forwardDistanceMax;
            sd.horizDistanceMin = horizDistanceMin;
            sd.horizDistanceMax = horizDistanceMax;
            sd.vertDistanceMax = vertDistanceMax;
            sd.vertDistanceMax = vertDistanceMax;
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
