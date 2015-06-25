using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_EndLevel : SBBase {

    bool bDone;

    public override void BeginBeat() 
    {
        bDone = false;

        base.BeginBeat();
    }

    public override void UpdateBeat ()
    {
        base.UpdateBeat ();

        if (FloatingMenuManager.IsMenuUp() || 
            OculusCameraFadeManager.IsFaded() ||
            OculusCameraFadeManager.IsFading())
        {
            return;
        }

        bDone = true;
        FloatingMenuManager.ShowMenu(FloatingMenuManager.MenuType.TourComplete);
        
        #if UNITY_EDITOR
        Debug.Log("SB_EndLevel " + gameObject.name );
        #endif
    }

	public override bool IsComplete()
    {
        return bDone;
	}			
}
