using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_PlayerCameraMode : SBBase {
	
    public CameraType cameraMode = CameraType.OculusCamera; 
    public bool endOfLevel;

    bool bCheckForFollowFade = false;

    public override void BeginBeat() 
    {
        CameraType curCamType = CameraManager.GetActiveCameraType();

        if (curCamType != cameraMode)
        {
            if(curCamType == CameraType.OculusFollowCamera)
            {
                bCheckForFollowFade = true;
                OculusFollowCameraMode.singleton.Exit(endOfLevel);
            }
            else
            {
                bCheckForFollowFade = false;
                CameraManager.SwitchToCamera(cameraMode);
            }
        }

#if UNITY_EDITOR
        Debug.Log("SB_PlayerCameraMode " + gameObject.name + " CameraType: " + cameraMode);
#endif

        base.BeginBeat();
    }

	public override bool IsComplete()
    {
        if (bCheckForFollowFade)
        {
            return CameraManager.GetActiveCameraType() != CameraType.OculusFollowCamera;
        }
        else
        {
            return true;
        }
	}			
}
