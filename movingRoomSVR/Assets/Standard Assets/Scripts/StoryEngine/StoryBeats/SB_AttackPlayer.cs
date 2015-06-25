using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_AttackPlayer : SBBase 
{
    public SwimSchoolFollowData.SchoolState SchoolEnable = SwimSchoolFollowData.SchoolState.Manual;
    public string designGroupName;
    public GameObject followObject;
    //public Vector3 followOffset;

    public bool UseAvoidance = false;
    public float DesiredSpeed = 300f;

    private CritterInfo leaderCritterInfo;

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
        if (string.IsNullOrEmpty(designGroupName))
        {
            Debug.LogError("SB_AttackPlayer " + gameObject.name + "has empty designGroupName!");
        }

        bDone = false;
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
        Debug.Log("SB_CritterSchool " + gameObject.name + " Group: " + designGroupName + " Count: " + searchCritters.Count);
#endif

        if (SchoolEnable == SwimSchoolFollowData.SchoolState.Manual)
        {
            followObject = GameObject.Find("OVRCameraController");
            if (followObject != null &&
                leaderCritterInfo == null)
            {
                leaderCritterInfo = new CritterInfo();
                leaderCritterInfo.isPlayer = followObject.name == "OVRCameraController";
                leaderCritterInfo.critterObject = followObject;
                leaderCritterInfo.critterTransform = leaderCritterInfo.critterObject.transform;
                leaderCritterInfo.critterCollider = leaderCritterInfo.critterObject.GetComponent<Collider>();
                leaderCritterInfo.critterAnimation = leaderCritterInfo.critterObject.GetComponent<Animation>();
                leaderCritterInfo.followIgnoreFishBowl = true;
            }
        }

        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if (c == null ||
                c.swimSchoolFollowData == null)
            {
                continue;
            }

            GeneralMotionData gmd = c.generalMotionData;
//            GeneralSpeciesData gsd = c.generalSpeciesData;

            //TODO: behavior to drive attack
            gmd.useAvoidance = UseAvoidance;
            gmd.desiredSpeed = DesiredSpeed;
            c.ignoreAiAutoTriggers = true;
        }

        searchCritters.Clear();
        bDone = true;
	}
	
	
	public override bool IsComplete()
    {
        return bDone;
	}			
}
