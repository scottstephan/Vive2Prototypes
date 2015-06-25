using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_WaitPlayerLookTransform : SBBase {
	
    public Transform lookAtTransform;
    public float requireDistance = -1f;
    public float passTimeout = -1f;
    public bool passTriggerNextBeats;

    bool bDone;
    float timer;

    public override void BeginBeat() 
    {
        bDone = false;
        timer = 0f;

        if (lookAtTransform == null)
        {
            Debug.LogError("SB_WaitPlayerLookTransform " + gameObject.name + "has empty lookAtTransform!");
        }

        base.BeginBeat();
    }

    public override void UpdateBeat() 
    {	        
        base.UpdateBeat();

        if (!gameObject.activeSelf || lookAtTransform == null)
        {
            bDone = true;
#if UNITY_EDITOR
            Debug.Log("SB_WaitPlayerLookTransform " + gameObject.name + " complete because not active/no transform");
#endif
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

        Vector3 camFwd = CameraManager.GetCurrentCameraForward();
        Vector3 toPos = lookAtTransform.position - CameraManager.GetCurrentCameraPosition();
        float dist = toPos.magnitude;

        if (requireDistance > 0f && 
            dist > requireDistance)
        {
            return;
        }

        if (dist > 0f)
        {
            toPos /= dist;
        }

        if (Vector3.Dot(toPos, camFwd) > 0.65f)
        {
#if UNITY_EDITOR
            Debug.Log("SB_WaitPlayerLookTransform " + gameObject.name + " complete because lookign at " + lookAtTransform.name);
#endif
           
            bDone = true;
        }
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
