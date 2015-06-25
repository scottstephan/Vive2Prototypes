using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_PlayerTourMode : SBBase {

    public GameObject tourStart;
    public GameObject tourStop;
    public GameObject[] deactivateObjects;
    public bool openMenuOnComplete;

    public override void BeginBeat() 
    {
        bool bTour = App.UserManager.educationalMode == 0 && tourStart != null;

        OculusTourCameraMode.openMenuOnComplete = openMenuOnComplete;
        OculusTourCameraMode.tourStart = tourStart;
        OculusTourCameraMode.tourStop = tourStop;
        OculusTourCameraMode.deactivateObjects = deactivateObjects;

        if (OculusTourCameraMode.init_tourStart == null)
        {
            OculusTourCameraMode.init_openMenuOnComplete = openMenuOnComplete;
            OculusTourCameraMode.init_tourStart = tourStart;
            OculusTourCameraMode.init_tourStop = tourStop;
            OculusTourCameraMode.init_deactivateObjects = deactivateObjects;
        }

        if (bTour)
        {
            if( CameraManager.IsInOculusMode() ) {
                CameraManager.SwitchToCamera(CameraType.OculusTourCamera);
            }

            #if UNITY_EDITOR
            Debug.Log("SB_PlayerTourMode ON " + gameObject.name + " TourStart: " + tourStart.name);
            #endif
        }
        else
        {
            #if UNITY_EDITOR
            Debug.Log("SB_PlayerTourMode off, in edu mode " + gameObject.name + " TourStart: " + tourStart.name);
            #endif
        }

        base.BeginBeat();
    }

	public override bool IsComplete()
    {
        return true;
	}			
}
