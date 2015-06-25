using UnityEngine;
using System;
using System.Collections;

//TODO: re-enable warnings
#pragma warning disable 0414 // private variable assigned but not used.

public class TapGesture : GestureBase {
	
    public int numberOfTaps = 1;
	
	public bool tapOnGestureCritter = false;
	
	[HideInInspector] 
	public int currentTapCount = 0;
	[HideInInspector] 
	public int currentTapIndex = -1;

	public override void StartTrace ( Action<float> finishedFunc )
	{				
		base.StartTrace( finishedFunc );		
		
		currentTapCount = 0;
		currentTapIndex = -1;
	}

    private bool mouseIsDown = false;
    private Vector3 mouseDownPosition;

	public override void Evaluate() {
		if( base.traceActive ) {
            if( InputManager.tapCount > 0 ) {
//                    Debug.Log ("TAPS " + InputManager.tapCount + "," + currentTapCount );
                if( currentTapIndex != InputManager.uniqueTapIndex ) {
                    currentTapCount += 1;
                    currentTapIndex = InputManager.uniqueTapIndex;
                    if( currentTapCount > numberOfTaps ) {
                        base.tpFinishedFunc(0);
                        feedbackIsVisible = false;
                        return;
                    }
                }
            }
            
			if( currentTapCount == numberOfTaps ) {
                base.tpFinishedFunc(1);
                feedbackIsVisible = false;
                return;
            }
		}
	}	

    public override void EndTrace() {
        base.EndTrace();
        if(currentTapCount != numberOfTaps) {
            base.tpFinishedFunc(0);
        }
    }
}
