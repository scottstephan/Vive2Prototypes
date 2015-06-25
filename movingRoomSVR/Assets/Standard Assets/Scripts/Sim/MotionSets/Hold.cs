using UnityEngine;
using System.Collections;

public class Hold : BehaviorBase {

	// ensures nothing can automatically change this.
	public override bool IsSingletonBehavior() { return true; }

    public override void Start (CritterInfo critter_info)
    {
        GeneralMotionData gmd = critter_info.generalMotionData;
        HoldData hd = critter_info.holdData;
        hd.ogUseAvoidance = gmd.useAvoidance;

        gmd.lockVelocityToHeading = true;
        gmd.useAvoidance = false;
    }

    public override void End (CritterInfo critter_info)
    {
        GeneralMotionData gmd = critter_info.generalMotionData;
        gmd.desiredSpeed = gmd.swimSpeed;

        HoldData hd = critter_info.holdData;
        gmd.useAvoidance = hd.ogUseAvoidance;
    }

    // yup. do nothing.
    public override void Update(CritterInfo critter_info)
    {
        GeneralMotionData gmd = critter_info.generalMotionData;
        HoldData hd = critter_info.holdData;

        Vector3 lookAtDir = critter_info.critterTransform.forward;
        if (hd != null &&
            hd.lookAtTransform != null)
        {
            lookAtDir = (hd.lookAtTransform.position - critter_info.critterTransform.position).normalized;
        }

        critter_info.critterSteering.desiredSteeringThrottle = 1f;
        gmd.desiredVelocityDirection = lookAtDir;
        gmd.desiredRotation = Quaternion.LookRotation(lookAtDir);
        gmd.desiredSpeed = 0f;
    }
}
