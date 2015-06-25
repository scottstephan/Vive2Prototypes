using UnityEngine;
using System.Collections;

public class Dead : BehaviorBase {

	// ensures nothing can automatically change this.
	public override bool IsSingletonBehavior() { return true; }

	// allows this to be automatically changed to..
	public override bool SingletonAllowsSwitchTo() { return true; }

	public override float EvaluatePriority(CritterInfo critter_info)
	{
		if (critter_info.deadData != null && critter_info.generalSpeciesData.isDead)
		{
			return 100000f;
		}
		else
		{
			return 0f;
		}
	}

    public override void Update(CritterInfo critter_info)
    {
//        GeneralSpeciesData gsd = critter_info.generalSpeciesData;
        GeneralMotionData gmd = critter_info.generalMotionData;
        DeadData dd = critter_info.deadData;

        gmd.desiredSpeed = dd.DyingSwimSpeed;

        if (dd.SpawnGibs)
        {
            GibManager.SpawnGibs(critter_info);
            critter_info.markedForRemove = true;
        }
    }
}
