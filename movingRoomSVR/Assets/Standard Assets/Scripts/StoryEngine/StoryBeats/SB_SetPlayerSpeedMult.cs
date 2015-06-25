using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_SetPlayerSpeedMult : SBBase {	

    public float speedModifier = 1f;


    public override void BeginBeat() 
    {
        CameraManager.SetCameraExternalSpeedModifier( speedModifier );
        base.BeginBeat();
    }

    public override void Reset ()
    {
        base.Reset ();
        CameraManager.SetCameraExternalSpeedModifier( 1f );
    }

	public override bool IsComplete()
    {
        return true;
	}			
}
