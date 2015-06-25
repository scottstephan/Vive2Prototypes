using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_WaitForCount : SBBase {
	
    public int count = 2;

    int triggered = 0;
    bool bDone = false;
    bool isRunning = false;

    public override void Reset ()
    {
        base.Reset ();

        if (!isRunning)
        {
            triggered = 0;
            bDone = false;
        }
    }

    public override bool AllowAdd ()
    {
        return !isRunning && triggered < count;
    }

    public override void BeginBeat() 
    {
        if (triggered >= count)
        {
            return;
        }

        ++triggered;

        isRunning = true;

        if (triggered == count)
        {
            bDone = true;
        }

#if UNITY_EDITOR
        if (bDone)
        {
            Debug.Log("SB_WaitForCount " + gameObject.name + " triggering! " + triggered + " out of " + count);
        }
        else
        {
            Debug.Log("SB_WaitForCount " + gameObject.name + " hit but not triggered, waiting: " + triggered + " out of " + count);

        }
#endif

        base.BeginBeat();
    }

    public override void EndBeat ()
    {
        base.EndBeat ();
        isRunning = false;
        bDone = false;
    }

    public override bool IsComplete()
    {
        return bDone;
	}			
}
