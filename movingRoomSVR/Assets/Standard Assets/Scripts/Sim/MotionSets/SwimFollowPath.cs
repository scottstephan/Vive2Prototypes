using UnityEngine;
using System.Collections;

[System.Serializable]
public class SwimFollowPath : BehaviorBase {

	public override void Start(CritterInfo critter_info)
	{
		GeneralMotionData gmd = critter_info.generalMotionData;
		gmd.lockVelocityToHeading = true;

        SwimDisperseData disperseData = critter_info.swimDisperseData;
        SwimFollowPathData sd = critter_info.swimFollowPathData;
        sd.followTransform = sd.followObject.transform;
        sd.lastFollowPos = sd.followTransform.position;
        sd.followAnim = sd.followObject.GetComponent<Animation>();
        sd.state = SwimFollowPathData.State.Arriving;
        sd.lastMoveDir = Vector3.zero;

        sd.ogUseAvoidance = gmd.useAvoidance;

        if (sd.followAnim == null)
        {
            sd.followAnim = sd.followObject.AddComponent<Animation>();
        }

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
        SwimFollowPathData sd = critter_info.swimFollowPathData;
        GeneralMotionData gmd = critter_info.generalMotionData;
        gmd.useAvoidance = sd.ogUseAvoidance;
        gmd.useAvoidanceGround = true;

        SwimDisperseData disperseData = critter_info.swimDisperseData;
        sd.followObject = null;
        sd.followTransform = null;
        sd.followAnim = null;
        sd.followAnimClip = null;

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
        SwimFollowPathData sd = critter_info.swimFollowPathData;
        GeneralSpeciesData gsd = critter_info.generalSpeciesData;
        GeneralMotionData gmd = critter_info.generalMotionData;

        Vector3 targetDir = sd.followTransform.position - critter_info.cachedPosition;
        float targetDist = targetDir.magnitude;
//        float maxDim = Mathf.Max(gmd.critterBoxColliderSize.x, Mathf.Max (gmd.critterBoxColliderSize.y, gmd.critterBoxColliderSize.z));

        if (sd.state == SwimFollowPathData.State.Arriving)
        {
            if (targetDist < sd.arrivalDistance)
            {
                if (sd.followAnimClip != null)
                {
                    if (sd.followAnim.GetClip(sd.followAnimClip.name) == null)
                    {
                        sd.followAnim.AddClip(sd.followAnimClip, sd.followAnimClip.name);
                    }

                    sd.followAnim[sd.followAnimClip.name].time = 0f;
                    sd.followAnim.Play(sd.followAnimClip.name);
                }

                sd.state = SwimFollowPathData.State.Following;
            }

            gmd.desiredSpeed = gmd.swimSpeed;
        }
        else if (sd.state == SwimFollowPathData.State.Following)
        {
            Vector3 deltaTargetPos = sd.followTransform.position - sd.lastFollowPos;
            float deltaDist = deltaTargetPos.magnitude;

            if (deltaDist <= 0.01f)
            {
                gmd.desiredSpeed = MathfExt.Fit(targetDist, 0f, sd.followDistance, 0f, gmd.swimSpeed);
            }
            else
            {
                float followSpeed = Mathf.Max(deltaDist/Time.deltaTime, 1f);
                if (targetDist < sd.followDistance)
                {
                    gmd.desiredSpeed = MathfExt.Fit(targetDist, 0f, sd.followDistance, 0f, followSpeed);
                }
                else
                {
                    gmd.desiredSpeed = MathfExt.Fit(targetDist, sd.followDistance, sd.followDistance * 3f, followSpeed, followSpeed*2f);
                }
            }

            if (targetDist < sd.arrivalDistance &&
                !sd.followAnim.isPlaying)
            {
                // follow anim may be removed if objects are deactivated as critter is being told to leave
                if (sd.followAnimClip != null &&
                    sd.followAnim[sd.followAnimClip.name] != null)
                {
                    sd.followAnim[sd.followAnimClip.name].time = 0f;
                    sd.followAnim.Play(sd.followAnimClip.name);
                    sd.followAnim.Sample();
                    sd.followAnim.Stop();
                }

                sd.followObject = null;
                gsd.switchBehavior = true;
                return;
            }
        }
        
        gmd.desiredVelocityDirection = targetDir;

        sd.lastMoveDir = sd.followTransform.position - sd.lastFollowPos;
        sd.lastFollowPos = sd.followTransform.position;
#if UNITY_EDITOR
        Debug.DrawLine(critter_info.cachedPosition,sd.followTransform.position,Color.yellow);
#endif
	}
	
	public override float EvaluatePriority(CritterInfo critter_info)
	{
        SwimFollowPathData sfpd = critter_info.swimFollowPathData;

        if(sfpd != null &&
           sfpd.followObject != null)
        {
            return sfpd.priorityValue;
		}

		return 0f;
	}
}



