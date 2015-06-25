using UnityEngine;
using System.Collections;

public class SB_TimedPause : SBBase {

	public float pauseTime = 1f;
	
	public bool useRandomRange = false;
	public float lowTime = 0.25f;
	public float highTime = 1f;

	private bool paused = false;
	

    float timer = 0f;

	public override void Reset()
    {
		paused = false;
        timer = 0f;

		base.Reset();
	}
	

	public override void BeginBeat()
    {
		base.BeginBeat();
		
		paused = true;
        timer = pauseTime;
        
        if( useRandomRange ) 
        {
            timer = RandomExt.FloatRange( lowTime, highTime );
        }

#if UNITY_EDITOR
        Debug.Log("SB_TimedPause " + gameObject.name + " pause start for " + timer + " seconds");
#endif
	}
	
    public override void UpdateBeat ()
    {
        base.UpdateBeat ();

        float dt = GetDeltaTime();

        timer -= dt;

        if (timer <= 0f)
        {
#if UNITY_EDITOR
            if (!paused)
            {
    //            Debug.Log("SB_TimedPause " + gameObject.name + " pause complete.");
            }
#endif

            paused = false;
        }
    }

	public override bool IsComplete() {
		return !paused;
	}
}
