using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_WaitPlayerNearCritter : SBBase {
	
    public string designGroupName;
    public float nearDistance = 100f;
    public bool requirePlayerFacing;
    public int testEveryNFrames = 4;
    public float passTimeout = -1f;
    public bool passTriggerNextBeats;

//    static List<CritterInfo> searchCritters = new List<CritterInfo>();

    bool bDone;
    float timer;
    int designGroupHash;
    int testFrame;

    public override bool ContainsDesignGroup( string design_group ) { 
        if (string.IsNullOrEmpty( designGroupName ) ) {
            return false;
        }
        
        return ( designGroupName.ToUpper().Equals( design_group ) );
    }
    
    public override void BeginBeat() 
    {
        bDone = false;
        timer = 0f;
        testFrame = 0;

        if (string.IsNullOrEmpty(designGroupName))
        {
            Debug.LogError("SB_WaitPlayerNearCritter " + gameObject.name + "has empty designGroupName!");
        }
        else
        {
            designGroupHash = designGroupName.ToUpper().GetHashCode();
        }

        base.BeginBeat();
    }

    public override void UpdateBeat() 
    {	        
        base.UpdateBeat();

        if (!gameObject.activeSelf)
        {
            bDone = true;
            return;
        }

        if (IsAddingCritters(designGroupHash))
        {
            return;
        }

        timer += GetDeltaTime();
        
        if (passTimeout > 0f && 
            timer > passTimeout)
        {
            #if UNITY_EDITOR
            Debug.Log("SB_WaitPlayerLookTransform " + gameObject.name + " complete because timeout passed: " + passTimeout);
            #endif
            bDone = true;
            return;
        }

        if ((testFrame % testEveryNFrames) == 0)
        {
            testFrame = 0;

            Vector3 camPos = CameraManager.GetCurrentCameraPosition();
            Vector3 camFwd = CameraManager.GetCurrentCameraForward();

            Collider[] colliders = Physics.OverlapSphere(camPos, nearDistance, 1 << 9);

            if (colliders != null)
            {
                for (int i=0; i<colliders.Length; ++i)
                {
                    GeneralSpeciesData hitgsd = colliders[i].gameObject.GetComponentInParent<GeneralSpeciesData>();

                    if (hitgsd != null &&
                        hitgsd.myCritterInfo != null &&
                        hitgsd.myCritterInfo.designGroupHash == designGroupHash)
                    {
                        if (requirePlayerFacing &&
                            Vector3.Dot (camFwd, (camPos-colliders[i].transform.position).normalized) < 0f)
                        {
                            continue;
                        }

                        bDone = true;
                        break;
                    }
                }
            }
        }

        ++testFrame;
    }	

    public override bool TriggerNextBeats()
    {
        if (passTimeout > 0f && 
            timer > passTimeout &&
            !passTriggerNextBeats)
        {
            return false;
        }
        
        return true;
    }

	
	public override bool IsComplete()
    {
        return bDone;
	}			
}
