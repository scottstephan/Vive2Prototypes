using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SBBase : MonoBehaviour {
	[HideInInspector]
	public StoryBeatsContainer mySBContainer;
	
	public SBBase[] nextBeats;
	
    StoryBeatDesignGroupAddOn extraDesignGroups;

    public string comment; // for designer purposes to keep track of why things changed

	[HideInInspector]
	public bool markedForRemoval = false;
	[HideInInspector]
	public bool hasFinished = false;

    protected bool hasBeenReset = false;
	
    protected bool skip;

    public virtual void Start()
    {
        extraDesignGroups = GetComponent<StoryBeatDesignGroupAddOn>();
    }

    public virtual bool TriggerNextBeats()
    {
        return true;
    }

    public virtual bool AllowAdd()
    {
        return true;
    }

	public virtual void Reset()
	{
		if(hasBeenReset)
		{
			// This node has already been reset -- nothing to do here.
			return;
		}
		
        skip = false;
		hasFinished = false;
		hasBeenReset = true;
		
		if( nextBeats != null && nextBeats.Length > 0 ) {
            for( int i = 0; i < nextBeats.Length; i++ ) {
                SBBase nb = nextBeats[i];
				if( nb != null ) {
					nb.Reset();
				}
			}
		}
	}

    public bool IsSkipped()
    {
        return this.skip;
    }

    public void Skip(bool bSkip)
    {
        this.skip = bSkip;
    }

	public virtual void BeginBeat() 
	{
		hasBeenReset = false;
		hasFinished = false;

#if UNITY_EDITOR
        if (nextBeats != null)
        {
            for (int i=0; i<nextBeats.Length; ++i)
            {
                if (nextBeats[i] == gameObject)
                {
                    Debug.LogError("StoryBeat Object " + gameObject.name + " contains itself in Next Beats");
                }
            }
        }
#endif
	}
	
    protected float GetDeltaTime()
    {
        float dt = Time.deltaTime;
        
        if (SimInstance.Instance.IsSimPaused())
        {
            dt = 0f;
        }
        else if (SimInstance.Instance.slowdownActive)
        {
            dt *= SimInstance.slowdownMultiplierInv;
        }

        return dt;
    }

	public virtual void EndBeat() { 
		markedForRemoval = true;
		hasFinished = true;
	}
	
	public virtual void UpdateBeat() {}
	
	public virtual bool IsComplete() { return false; }

    public virtual bool ContainsDesignGroup( string design_group ) { return false; }

    public virtual bool TriggersStoryBeat( SBBase trigger_beat ) {
        if( trigger_beat == null ) {
            return false;
        }

        if( nextBeats != null && nextBeats.Length > 0 ) {
            for( int i = 0; i < nextBeats.Length; i++ ) {
                if (nextBeats[i] == trigger_beat) {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsAddingCritters( string designGroupName ) 
    {
        int designGroupHash = designGroupName.ToUpper().GetHashCode();
        return IsAddingCritters(designGroupHash);
    }

    public bool IsAddingCritters( int designGroupHash ) 
    {
        if (SimInstance.Instance.IsAddingCritters(designGroupHash))
        {
            return true;
        }
        
        if (extraDesignGroups != null &&
            extraDesignGroups.designGroupHashes != null)
        {
            for (int i=0; i<extraDesignGroups.designGroupHashes.Length; ++i)
            {
                if (SimInstance.Instance.IsAddingCritters(extraDesignGroups.designGroupHashes[i]))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void GetCrittersByDesignGroup( string designGroupName, List<CritterInfo> results ) 
    {
        int designGroupHash = designGroupName.ToUpper().GetHashCode();
        GetCrittersByDesignGroup(designGroupHash, results);
    }

    public void GetCrittersByDesignGroup( int designGroupHash, List<CritterInfo> results ) 
    {
        SimInstance.Instance.GetCrittersByDesignGroup(designGroupHash, results);

        if (extraDesignGroups != null &&
            extraDesignGroups.designGroupHashes != null)
        {
            for (int i=0; i<extraDesignGroups.designGroupHashes.Length; ++i)
            {
                SimInstance.Instance.GetCrittersByDesignGroup(extraDesignGroups.designGroupHashes[i], results);
            }
        }
    }

    public CritterInfo GetCritterInDesignGroup( string designGroupName ) 
    {
        int designGroupHash = designGroupName.ToUpper().GetHashCode();
        return GetCritterInDesignGroup(designGroupHash);
    }
    
    public CritterInfo GetCritterInDesignGroup( int designGroupHash ) 
    {
        CritterInfo critter_info = SimInstance.Instance.GetCritterInDesignGroup(designGroupHash);

        if (critter_info != null)
        {
            return critter_info;
        }

        if (extraDesignGroups != null &&
            extraDesignGroups.designGroupHashes != null)
        {
            for (int i=0; i<extraDesignGroups.designGroupHashes.Length; ++i)
            {
                critter_info = SimInstance.Instance.GetCritterInDesignGroup(designGroupHash);
                if (critter_info != null)
                {
                    return critter_info;
                }
            }
        }

        return null;
    }

}
