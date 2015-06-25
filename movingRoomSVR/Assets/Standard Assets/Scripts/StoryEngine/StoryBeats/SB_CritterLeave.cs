using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterLeave : SBBase
{
    public string designGroupName;
    public bool waitUntilAllLeave;

	public Transform leaveDirection = null;
	public bool leaveDirectionIsCameraRelative = false;

    public float offscreenMinDist = 1000f;
    public float offscreenFogDistMult = 0.5f;

    static List<CritterInfo> searchCritters = new List<CritterInfo>();
    bool allLeft;


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
            Debug.LogError("SB_CritterLeave " + gameObject.name + "has empty designGroupName!");
        }

        GetCrittersByDesignGroup(designGroupName, searchCritters);

#if UNITY_EDITOR
        Debug.Log("SB_CritterLeave Start " + gameObject.name + " Group: " + designGroupName + " Count: " + searchCritters.Count);
#endif
		Vector3 dir = Vector3.zero;
		
		if( leaveDirection != null ) {
			if( leaveDirectionIsCameraRelative ) {
				Quaternion cq = CameraManager.GetCurrentCameraFlattenedRotation();
				dir = cq * leaveDirection.forward;
			}
			else {
				dir = leaveDirection.forward;
			}
		}

		for (int i=0; i<searchCritters.Count; ++i)
        {
            float colliderSize = 0f;

            CritterInfo c = searchCritters[i];

            if (c.generalMotionData != null)
            {
                colliderSize += c.generalMotionData.critterBoxColliderRadius;
            }

            SimInstance.Instance.LeaveSceneMinDist(c, Mathf.Min (offscreenMinDist, GlobalOceanShaderAdjust.Instance.dist*offscreenFogDistMult) + colliderSize, dir );
        }

        searchCritters.Clear ();

		base.BeginBeat();
	}
	
    public override void UpdateBeat()
    {
        base.UpdateBeat ();

        if (waitUntilAllLeave)
        {
            GetCrittersByDesignGroup(designGroupName, searchCritters);

            if (searchCritters.Count == 0)
            {
                allLeft = true;
#if UNITY_EDITOR
                Debug.Log("SB_CritterLeave " + gameObject.name + " Group: "  + designGroupName + " ALL LEFT");
#endif
            }
       
            searchCritters.Clear ();
        }
    }
	
	public override bool IsComplete()
    {
        if (waitUntilAllLeave)
        {
            return allLeft;
        }

		return true;
	}			
}
