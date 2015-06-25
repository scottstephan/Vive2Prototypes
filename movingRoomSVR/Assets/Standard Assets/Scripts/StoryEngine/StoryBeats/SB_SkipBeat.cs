using UnityEngine;
using System.Collections;

public class SB_SkipBeat : SBBase {
	
	public SBBase storyBeat;
	
	public bool skipBeat = true;
	
	public override void BeginBeat() 
    {
		base.BeginBeat();
		
        if (storyBeat != null)
        {
            storyBeat.Skip(skip);

#if UNITY_EDITOR           
            Debug.Log("SB_SkipBeat " + gameObject.name + (skipBeat ? " set to skip " : " set to NOT skip ") + storyBeat.gameObject.name);
#endif
        }
        else 
        {
#if UNITY_EDITOR
            Debug.LogError("SB_SkipBeat " + gameObject.name + " has NULL storybeat to " + (skipBeat ? "skip" : "unskip"));
#endif
        }
	}
	
    public override void Reset ()
    {
        base.Reset ();
        if (storyBeat != null)
        {
            storyBeat.Skip(false);
        }
    }

	public override bool IsComplete()
    { 
        return true;
    }	
}
