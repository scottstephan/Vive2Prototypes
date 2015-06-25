using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterSpeed : SBBase
{
    public string designGroupName;

    public float duration = -1f;

    // throttled steering overrides.
    public float throttleMult = 1f;
    public float yawMult = 1f;
    public float pitchMult = 1f;
    public float rollMult = 1f;

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
            Debug.LogError("SB_CritterSpeed " + gameObject.name + "has empty designGroupName!");
        }

        GetCrittersByDesignGroup(designGroupName, searchCritters);

#if UNITY_EDITOR
        Debug.Log("SB_CritterSpeed " + gameObject.name + " Group: " + designGroupName + " Count: " + searchCritters.Count);
#endif
        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            ThrottledFishSteering s = (ThrottledFishSteering)c.critterSteering;
            float revertThrottle = 1f;
            float revertYaw = 1f;
            float revertPitch = 1f;
            float revertRoll = 1f;

            if (s.scriptOverride)
            {
                revertThrottle = s.scriptThrottleMult; 
                revertYaw = s.scriptYawMult; 
                revertPitch = s.scriptPitchMult; 
                revertRoll = s.scriptRollMult; 
            }

            // apply the overrides
            float mult = throttleMult / revertThrottle;

            s.throttleMaxSpeed *= mult;
            s.throttleSpeedAccel *= mult;
            s.throttleSpeedDecel *= mult;

            mult = yawMult / revertYaw;
            s.yawAccel *= mult;
            s.yawDecel *= mult;
            s.yawMaxSpeed *= mult;

            mult = pitchMult / revertPitch;
            s.pitchAccel *= mult;
            s.pitchDecel *= mult;
            s.pitchMaxSpeed *= mult;

            mult = rollMult / revertRoll;
            s.rollAccel *= mult;
            s.rollDecel *= mult;
            s.rollMaxSpeed *= mult;

            s.scriptOverride = throttleMult != 1f || yawMult != 1f || pitchMult != 1f || rollMult != 1f;
            s.scriptOverrideTime = duration;
            s.scriptThrottleMult = throttleMult; 
            s.scriptYawMult = yawMult; 
            s.scriptPitchMult = pitchMult; 
            s.scriptRollMult = rollMult; 
        }

        searchCritters.Clear ();

		base.BeginBeat();
	}
	
	public override bool IsComplete()
    {
		return true;
	}			
}
