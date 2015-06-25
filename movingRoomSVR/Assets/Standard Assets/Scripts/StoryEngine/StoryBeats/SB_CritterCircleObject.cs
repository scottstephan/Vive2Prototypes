using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public enum CircleBeatType {
	StartCircle,
	StopCircle,
	SetData
}

public class SB_CritterCircleObject : SBBase {

	public CircleBeatType beatType = CircleBeatType.StartCircle;

	public string designGroupName;
//	public bool chooseOne = false;
	public GameObject circleObject = null;
    public float radialDistance = 1000f;
	public float swimSpeed = 160f;

	public bool useSwimTime = false;
	public float swimTime = 45f;

	private bool beat_active = false;

    List<CritterInfo> searchCritters = new List<CritterInfo>();
    
    public override bool ContainsDesignGroup( string design_group ) { 
        if (string.IsNullOrEmpty( designGroupName ) ) {
            return false;
        }
        
        return ( designGroupName.ToUpper().Equals( design_group ) );
    }

    void CircleTimeUp()
	{
#if UNITY_EDITOR
        Debug.Log("SB_CritterCircleObject " + gameObject.name + " swimtime of " + swimTime + " is complete.");
#endif
		GetCrittersByDesignGroup(designGroupName, searchCritters);

		for (int i=0; i<searchCritters.Count; ++i) {
			CritterInfo c = searchCritters[i];
			if (c == null ||
			    c.circleAroundObjectData == null)
			{
				continue;
			}

			AI.ForceSwitchToBehavior( c, SwimBehaviorType.SWIM_FREE );
		}
		searchCritters.Clear();

		beat_active = false;
	}

    public override void BeginBeat() 
    {	
        if (string.IsNullOrEmpty(designGroupName))
        {
            Debug.LogError("SB_CritterFishBowl " + gameObject.name + " has empty designGroupName!");
        }

        GetCrittersByDesignGroup(designGroupName, searchCritters);
#if UNITY_EDITOR
        Debug.Log("SB_CritterCircleObject " + gameObject.name + " Group: " + designGroupName + " Count: " + searchCritters.Count);
#endif
		bool callback_set = false;
        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if (c == null)
            {
                continue;
            }

            if (c.circleAroundObjectData == null)
            {
                Debug.LogError("SB_CritterCircleObject " + gameObject.name + " critter " + c.critterObject.name + " has no CircleAroundObjectData in prefab!");
                continue;
            }

			if( beatType == CircleBeatType.StopCircle ) {
				AI.ForceSwitchToBehavior( c, SwimBehaviorType.SWIM_FREE );
			}
			else {
				CircleAroundObjectData cd = c.circleAroundObjectData;
				cd.circleRadius = radialDistance;
				cd.swimSpeed = swimSpeed;
                cd.circleTime = useSwimTime ? swimTime : -1;
				if( circleObject != null ) {
					cd.targetTransform = circleObject.transform;
				}
				else {
					// will automaticaly chose the camera to circle around.
					cd.targetTransform = CameraManager.GetCurrentCameraTransform();
				}


				if( beatType == CircleBeatType.StartCircle &&
                   (c.swimToPointData == null || c.swimToPointData.pointReachedType != PointReachedType.ExitScene))
                {
					// only the useSwimTime method blocks advancement.
					if( !callback_set && useSwimTime ) 
                    {
						cd.CircleTimeExpired += CircleTimeUp;
						callback_set = true;
						beat_active = true;
					}

					AI.ForceSwitchToBehavior( c, SwimBehaviorType.CIRCLE_AROUND_OBJECT );
				}
			}
        }

        searchCritters.Clear();

        base.BeginBeat();
	}
	
	
	public override bool IsComplete()
    {
		return !beat_active;
	}			
}
