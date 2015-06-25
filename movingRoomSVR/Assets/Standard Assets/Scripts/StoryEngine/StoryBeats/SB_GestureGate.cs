using UnityEngine;
using System.Collections;

public class SB_GestureGate : SBBase {
	
	enum GateState {
		NONE,
		WAITING,
	}
	
	public GestureBase gesture;

	private bool gestureSuccess = false;

	private GateState gState = GateState.NONE;
	public override void Reset() {
		gState = GateState.NONE;
		gestureSuccess = false;
		
		base.Reset();
	}
	
	public override void BeginBeat() {	
		base.BeginBeat();
		gState = GateState.WAITING;
		gesture.StartTrace( TracePatternFinished );		
	}
			
	void TracePatternFinished( float out_score ) {
		Debug.Log("Trace Finished");
		gestureSuccess = out_score > 0f ? true : false;
		gesture.EndTrace();
        if( gestureSuccess ) {
//			AudioManager.PlayInGameAudio(SoundFXID.GestureSuccess);
//			LevelUIControl.Instance.TriggerGreatJobLocation( GestureFlowManager.Instance.gestureCritter.critterTransform.position );
			gState = GateState.NONE;
        }
		else {
//			AudioManager.PlayInGameAudio(SoundFXID.GestureFail);
			gesture.StartTrace( TracePatternFinished );
		}
	}
	
	public override bool IsComplete() {
		return gState == GateState.NONE;
	}
	
	public override void UpdateBeat() {
		if( gState == GateState.WAITING ) {
			gesture.Evaluate();
		}
	}
}
