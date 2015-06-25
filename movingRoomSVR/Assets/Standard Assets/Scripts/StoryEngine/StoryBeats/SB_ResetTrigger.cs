using UnityEngine;
using System.Collections;

public class SB_ResetTrigger : SBBase {
	
    public StoryBeatTrigger triggerToReset = null;
    public StoryBeatTrigger[] triggersToReset = null;

	public override void BeginBeat() {
		base.BeginBeat();
		
        if( triggerToReset != null ) {
            triggerToReset.Reset();
        }

        if( triggersToReset != null ) {
            for( int i = 0; i < triggersToReset.Length; i++ ) {
                StoryBeatTrigger tr = triggersToReset[i];
                if( tr != null ) {
                    tr.Reset();
                }
            }
        }
    }
	
	public override bool IsComplete() { return true; }	
}
