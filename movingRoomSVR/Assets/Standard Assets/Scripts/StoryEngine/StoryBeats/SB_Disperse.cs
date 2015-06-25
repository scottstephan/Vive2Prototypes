using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_Disperse : SBBase {
	
    public Transform DisperseVolume;
    public string designGroupName;
    public float extraSpeedMult = 1f;
    public float extraColliderDistMult = 1f;

    bool bDone;
    int designGroupHash;
    float radius;
    Bounds bounds;

    static List<CritterInfo> searchCritters = new List<CritterInfo>();

    public override bool ContainsDesignGroup( string design_group ) { 
        if (string.IsNullOrEmpty( designGroupName ) ) {
            return false;
        }
        return ( designGroupName.ToUpper().Equals( design_group ) );
    }

    public override void Start()
    {
        if (string.IsNullOrEmpty(designGroupName))
        {
            Debug.LogError("SB_Disperse " + gameObject.name + " has empty designGroupName!");
        }
        else
        {
            designGroupHash = designGroupName.ToUpper().GetHashCode();
        }
        
        if (DisperseVolume == null)
        {
            Debug.LogError("SB_Disperse " + gameObject.name + " has no DisperseVolume!");
        }
        else if (DisperseVolume.GetComponent<Collider>() != null)
        {
            bounds = DisperseVolume.GetComponent<Collider>().bounds;
        }
        else 
        {
            FishBowl fb = DisperseVolume.GetComponent<FishBowl>();
            if (fb != null && fb.fishBowlData != null)
            {
                bounds = new Bounds(fb.fishBowlData.position, fb.fishBowlData.size);
            }
            else
            {
                bounds = new Bounds(DisperseVolume.position, DisperseVolume.localScale);
            }
        }

        radius = Mathf.Max (bounds.extents.x, Mathf.Max(bounds.extents.y, bounds.extents.z)) * 0.5f;

        base.Start ();
    }

    public override void BeginBeat() 
    {
        bDone = false;


        base.BeginBeat();
    }
    
    public override void UpdateBeat() 
    {           
        base.UpdateBeat();
        
        if (DisperseVolume == null)
        {
            bDone = true;
            return;
        }

        if (IsAddingCritters(designGroupHash))
        {
            return;
        }

        GetCrittersByDesignGroup(designGroupHash, searchCritters);

        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if (c == null ||
                c.swimDisperseData == null)
            {
                continue;
            }

            if (!bounds.Intersects(c.critterCollider.bounds))
            {
                continue;
            }

            DisperseCollision.Disperse(c.generalSpeciesData, null, DisperseVolume, radius, true); 
            c.swimDisperseData.extraColliderDistMult = extraColliderDistMult;
            c.swimDisperseData.extraSpeedMult = extraSpeedMult;
            c.swimDisperseData.useBounds = true;
            c.swimDisperseData.bounds = bounds;
        }

#if UNITY_EDITOR
        Debug.Log("SB_Disperse " + gameObject.name + " DisperseVol: " + DisperseVolume + " Group: " + designGroupName + " extraSpeedMult: " + extraSpeedMult + " extraColliderMult " + extraColliderDistMult + " Count: " + searchCritters.Count);
#endif
        searchCritters.Clear();
        bDone = true;
	}
	
	
	public override bool IsComplete()
    {
        return bDone;
	}			
}
