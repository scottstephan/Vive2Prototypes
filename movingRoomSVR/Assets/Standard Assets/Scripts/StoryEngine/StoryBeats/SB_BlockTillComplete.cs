using UnityEngine;
using System.Collections;

public class SB_BlockTillComplete : SBBase {
	
	public SBBase[]	blockers;
	
	// this story beat waits until the blockers are all finished before trigger it's next beats.
	public override bool IsComplete() {
		if( blockers != null && blockers.Length > 0 ) {
            for( int i = 0; i < blockers.Length; i++ ) {
                SBBase bk = blockers[i];
				if( bk != null && !bk.hasFinished ) {
					return false;
				}
			}
//			Debug.Log("BLOCKER IS COMPLETE!");
			return true;		
		}
		
		return false;
	}
}