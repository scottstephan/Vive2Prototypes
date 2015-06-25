using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterSchool : SBBase {
	
    public SwimSchoolFollowData.SchoolState SchoolEnable = SwimSchoolFollowData.SchoolState.Manual;
    public string designGroupName;
    public string followDesignGroupName;
    public GameObject followObject;
    public Vector3 followOffset;

    public bool useOverrideFollowRadius;
    public float overrideFollowRadius;

    private CritterInfo leaderCritterInfo;

    static List<CritterInfo> searchCritters = new List<CritterInfo>();

    bool bDone;
    	
    public override bool ContainsDesignGroup( string design_group ) { 
        bool first = ( !string.IsNullOrEmpty( designGroupName ) 
                      && designGroupName.ToUpper().Equals( design_group ));
        bool second = ( !string.IsNullOrEmpty( followDesignGroupName ) 
                       && followDesignGroupName.ToUpper().Equals( design_group ) );
        
        return ( first || second );
    }
    

    public override void BeginBeat() 
    {
        if (string.IsNullOrEmpty(designGroupName))
        {
            Debug.LogError("SB_CritterSchool " + gameObject.name + "has empty designGroupName!");
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
//        Debug.Log("SB_CritterSchool " + gameObject.name + " Group: " + designGroupName + " Count: " + searchCritters.Count);
#endif
        if (SchoolEnable == SwimSchoolFollowData.SchoolState.Manual)
        {
            if (followObject == null &&
                string.IsNullOrEmpty(followDesignGroupName))
            {
                followObject = GameObject.Find("OVRCameraController");
            }

            if (followObject != null)
            {
                if (leaderCritterInfo == null)
                {
                    leaderCritterInfo = new CritterInfo();
                }
                leaderCritterInfo.isPlayer = followObject.name == "OVRCameraController";
                leaderCritterInfo.critterObject = followObject;
                leaderCritterInfo.critterTransform = leaderCritterInfo.critterObject.transform;
                leaderCritterInfo.cachedPosition = leaderCritterInfo.critterTransform.position;
                leaderCritterInfo.critterCollider = leaderCritterInfo.critterObject.GetComponent<Collider>();
                leaderCritterInfo.critterAnimation = leaderCritterInfo.critterObject.GetComponent<Animation>();
                leaderCritterInfo.followIgnoreFishBowl = true;
            }
            else
            {
                leaderCritterInfo = SimInstance.Instance.GetCritterInDesignGroup(followDesignGroupName);
            }
        }

        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if (c == null ||
                c.swimSchoolFollowData == null)
            {
                Debug.LogError("SB_CritterSchool " + gameObject.name + " critter " + c.critterObject.name + " has no SwimSchoolFollowData in prefab!");

                continue;
            }

            c.swimSchoolFollowData.state = SchoolEnable;
            c.swimSchoolFollowData.scriptFollowRadius = useOverrideFollowRadius ? overrideFollowRadius : -1;

            if (SchoolEnable == SwimSchoolFollowData.SchoolState.Manual)
            {
                c.ignoreAiAutoTriggers = true;
                c.swimSchoolFollowData.leaderOffset = followOffset;
                AI.SetNewLeader(c, leaderCritterInfo);
            }
            else
            {
                c.ignoreAiAutoTriggers = false;
                c.followIgnoreFishBowl = false;
                AI.SetNewLeader(c, null);
            }
        }

        searchCritters.Clear();
        bDone = true;
	}
	
	
	public override bool IsComplete()
    {
        return bDone;
	}			
}
