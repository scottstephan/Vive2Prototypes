using UnityEngine;
using System.Collections;

[System.Serializable]
public class SwimScriptGoToPoint : BehaviorBase {

	public override void Start(CritterInfo critter_info)
	{
		GeneralMotionData gmd = critter_info.generalMotionData;
		gmd.lockVelocityToHeading = true;

        SwimScriptGoToPointData sd = critter_info.swimScriptGoToPointData;
        SwimDisperseData disperseData = critter_info.swimDisperseData;

        if (!sd.enableAvoidFish)
        {
            gmd.useAvoidanceFish = false;
        }

        if (!sd.enableAvoidGround)
        {
            gmd.useAvoidanceGround = false;
        }

        if (disperseData != null)
        {
            if (!sd.enableCritterDisperse)
            {
                disperseData.critterDisperseDisableCount++;
            }

            if (!sd.enablePlayerDisperse)
            {
                disperseData.playerDisperseDisableCount++;
            }
        }

        if (sd.disableCritterGroundCollision)
        {
            gmd.useGroundSlide = false;
        }
    }

    public override void End (CritterInfo critter_info)
    {
        GeneralMotionData gmd = critter_info.generalMotionData;
        SwimScriptGoToPointData sd = critter_info.swimScriptGoToPointData;
        SwimDisperseData disperseData = critter_info.swimDisperseData;

        sd.goToTransform = null;

        if (!sd.enableAvoidFish)
        {
            gmd.useAvoidanceFish = true;
        }
        
        if (!sd.enableAvoidGround)
        {
            gmd.useAvoidanceGround = true;
        }
        
        if (disperseData != null)
        {
            if (!sd.enableCritterDisperse)
            {
                disperseData.critterDisperseDisableCount--;
            }
            
            if (!sd.enablePlayerDisperse)
            {
                disperseData.playerDisperseDisableCount--;
            }
        }

        if (sd.disableCritterGroundCollision)
        {
            gmd.useGroundSlide = true;
        }
    }

	// Update is called once per frame
	public override void Update (CritterInfo critter_info) 
	{
        SwimScriptGoToPointData sd = critter_info.swimScriptGoToPointData;
        GeneralSpeciesData gsd = critter_info.generalSpeciesData;
        GeneralMotionData gmd = critter_info.generalMotionData;

        Vector3 targetDir = sd.goToTransform.position - critter_info.cachedPosition;
        float targetDist = targetDir.magnitude;

        if (targetDist < sd.arrivalDistance)
        {
            sd.goToTransform = null;
            gsd.switchBehavior = true;
        }

        gmd.desiredVelocityDirection = targetDir;
        gmd.desiredSpeed = gmd.swimSpeed * sd.swimSpeedMult;
#if UNITY_EDITOR
        Debug.DrawLine(critter_info.cachedPosition,critter_info.cachedPosition+targetDir,Color.yellow);
#endif
	}
	
	public override float EvaluatePriority(CritterInfo critter_info)
	{
        SwimScriptGoToPointData sgtpd = critter_info.swimScriptGoToPointData;

        if(sgtpd != null &&
           sgtpd.goToTransform != null)
        {
            SwimDisperseData sdd = critter_info.swimDisperseData;
            if (sdd != null && sdd.useBounds)
            {
                return sdd.priorityValue - 1f;
                // scripted disperse
            }
            else
            {
                return sgtpd.priorityValue;
            }
		}

		return 0f;
	}
}



