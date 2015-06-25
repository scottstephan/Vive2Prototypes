using UnityEngine;
using System.Collections;

[System.Serializable]
public class WeightedSB {
	public SBBase[] storyBeats = null;
	public float weight = 1f;
	[HideInInspector]
	public float percentage = 0f;
	[HideInInspector]
	public int satisfiedCnt = 0;
	[HideInInspector]
	public int cnt = 0;
}

public class SB_Random : SBBase {
	
	public WeightedSB[] weightedSBs;

	public bool ensureDistribution = false;
	public int totalDistribution = 10;

	private float totalWeight = 0;

    public override bool TriggersStoryBeat( SBBase trigger_beat ) {
        if( base.TriggersStoryBeat( trigger_beat) ) {
            return true;
        }
        
        if( weightedSBs != null && weightedSBs.Length > 0 ) {
            for( int i = 0; i < weightedSBs.Length; i++ ) {
                WeightedSB wb = weightedSBs[i];
                if( wb == null 
                   || wb.storyBeats == null 
                   || wb.storyBeats.Length <= 0 ) {
                    continue;
                }

                for( int j = 0; j < wb.storyBeats.Length; j++ ) {
                    if( wb.storyBeats[j] == trigger_beat ) {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    // called at scene initialization.
	public override void Reset()
	{
		if(hasBeenReset)
		{
			// This node has already been reset -- nothing to do here.
			return;
		}

		// ensure the next beats are null.
		nextBeats = null;

		// go through each of our chance trees and reset them.. add up our total weight too.
		totalWeight = 0;
		foreach( WeightedSB wsb in weightedSBs ) {
			if( wsb.weight < 0f ) {
				Debug.LogError(" WEIGHT ERROR: Random StoryBeat Weight is less than zero!");
			}
			totalWeight += wsb.weight;

			foreach( SBBase sb in wsb.storyBeats ) {
				sb.Reset();
			}
		}

		// weights and counts..
		foreach( WeightedSB wsb in weightedSBs ) {
			wsb.cnt = 0;
			wsb.satisfiedCnt = Mathf.Max(1,(int)(((float)totalDistribution) * (wsb.weight / totalWeight)));
			wsb.percentage = wsb.weight / totalWeight;
		}

		base.Reset();
	}

	public override void BeginBeat() 
	{
		// always reset the next beats when beginning this one.
		nextBeats = null;
        int pickIndex = -1;

		float rnd = Random.value;

		float accum_percentage = 0f;
        for(int iWSB=0; iWSB<weightedSBs.Length; ++iWSB )
        {
            WeightedSB wsb = weightedSBs[iWSB];

			accum_percentage += wsb.percentage;

			if( nextBeats == null && rnd < accum_percentage ) 
            {
				if( ensureDistribution ) 
                {
					if( wsb.cnt >= wsb.satisfiedCnt ) 
                    {
						int ri = Random.Range(0,weightedSBs.Length);
						int i;

						for( i = 0; i < weightedSBs.Length; i++ ) 
                        {
							int ii = ri + i;
							if( ii >= weightedSBs.Length )
                            {
								ii -= weightedSBs.Length;
							}
							WeightedSB sb = weightedSBs[ii];
							if( sb.cnt < sb.satisfiedCnt )
                            {
                                pickIndex = iWSB;
								nextBeats = sb.storyBeats;
								sb.cnt++;
								i = weightedSBs.Length;
							}
						}

						// reset the distribution. and choose the original hit.
						if( nextBeats == null ) 
                        {
							for( i = 0; i < weightedSBs.Length; i++ ) 
                            {
								WeightedSB sb = weightedSBs[i];
								sb.cnt = 0;
							}

                            pickIndex = iWSB;
							nextBeats = wsb.storyBeats;
							wsb.cnt++;
						}
					}
					else 
                    {
                        pickIndex = iWSB;
						nextBeats = wsb.storyBeats;
						wsb.cnt++;
					}
				}
				else
                {
                    pickIndex = iWSB;                   
					nextBeats = wsb.storyBeats;
				}
			}
		}

#if UNITY_EDITOR

        if (nextBeats == null)
        {
            Debug.Log ("SB_Random " + gameObject.name + " picked NO next beats");
        }
        else
        {
            Debug.Log ("SB_Random " + gameObject.name + " picked array index " + pickIndex);

        }
#endif
		base.BeginBeat();
	}
	
	public override bool IsComplete() { return true; }
}
