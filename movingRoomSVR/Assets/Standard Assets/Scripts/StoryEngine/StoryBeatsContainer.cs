using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StoryBeatsContainer : MonoBehaviour {
	public SBBase[] startBeats;
	
	[HideInInspector]
	public List<SBBase> activeStoryBeats;
	
	private bool sequenceStarted = false;
    private StoryBeatTrigger[] triggers;

	void Start () {
		activeStoryBeats = new List<SBBase>();
		
		// setup our reference in the story manager... this may not be needed..
		if( StoryManager.Instance ) {
			StoryManager.Instance.storyBeatsContainer = this;
		}

        GameObject lvlStrData = GameObject.Find (SphereInstance.levelStreamingDataObjName) as GameObject;
        if (lvlStrData != null)
        {
            triggers = lvlStrData.GetComponentsInChildren<StoryBeatTrigger>(true);
        }
    }

	void BeginNextBeats( SBBase sb ) {
#if UNITY_EDITOR
        string beat_names = "";
#endif
        if( sb.nextBeats != null &&
            sb.nextBeats.Length > 0 &&
            sb.TriggerNextBeats() ) 
        {
            for( int i = 0; i < sb.nextBeats.Length; i++ ) 
            {
                SBBase nb = sb.nextBeats[i];
				if( nb != null )
                {
                    if (nb.IsSkipped())
                    {
                        #if UNITY_EDITOR
                        Debug.Log("SB Skipping " + nb.gameObject.name);
                        #endif

                        BeginNextBeats(nb);
                    }
                    else
                    {
#if UNITY_EDITOR
                        if( StoryManager.Instance.DEBUG_BEATS ) {
                            beat_names += nb.name + " :: ";
                        }
#endif
                        AddBeat (nb);
                    }
				}
			}
		}

#if UNITY_EDITOR
        if( StoryManager.Instance.DEBUG_BEATS ) {
            Debug.Log("NEXT BEATS for " + sb.name + " starting :: " + beat_names);
        }
#endif      
    }
	
	public void ResetBeats() {
		if( startBeats != null && startBeats.Length > 0 ) {
            for( int i = 0; i < startBeats.Length; i++ ) {
                SBBase sb = startBeats[i];
               	if( sb != null ) {
					sb.Reset();
				}
			}
		}
	}

	public void BeginBeats( SBBase[] beats ) {
#if UNITY_EDITOR
        if( StoryManager.Instance.DEBUG_BEATS ) {
            string beat_names = "BEGINBEATS :: ";
            if( beats != null && beats.Length > 0 ) {
                for( int i = 0; i < beats.Length; i++ ) {
                    if( beats[i] != null ) {
                        beat_names += beats[i].name + " :: ";
                    }
                }
            }
            Debug.Log(beat_names);
        }
#endif		

        if( beats != null && beats.Length > 0 ) {
            for( int i = 0; i < beats.Length; i++ ) {
                SBBase sb = beats[i];
				if( sb != null ) 
                {
                    if (sb.IsSkipped())
                    {
                        #if UNITY_EDITOR
                        Debug.Log("SB Skipping " + sb.gameObject.name);
                        #endif

                        BeginNextBeats(sb);
                    }
                    else
                    {
                        AddBeat(sb);
                    }
				}
			}
		}
	}

	public void StartSequence() {

        activeStoryBeats.Clear();

		sequenceStarted = true;
		InputManager.tapCount = 0;	// force the tap count to zero.
		InputManager.SetNoTapTillNextTouch();

        if (triggers != null)
        {
            for (int i=0; i<triggers.Length; ++i)
            {
                triggers[i].Reset ();
            }
        }

		ResetBeats();

#if UNITY_EDITOR
        if( StoryManager.Instance.DEBUG_BEATS ) {
            Debug.Log(gameObject.name + " is starting up ::");
        }
#endif      
        BeginBeats( startBeats );
	}
	
	public void UpdateSB() {
		if( !sequenceStarted ) {
			StartSequence();
			return;
		}
		
		if( activeStoryBeats != null && activeStoryBeats.Count > 0 ) {
			// update all the beats.
            for( int i = 0; i < activeStoryBeats.Count; i++ ) {
                SBBase sb = activeStoryBeats[i];
				if( sb != null ) {
					sb.UpdateBeat();
					if( sb.IsComplete() ) {
						sb.EndBeat();
					}
				}
			}
			
			// remove any beats that ended and startup any new ones.
			bool done = false;
			SBBase remove_me = null;
			while( !done ) 
            {
				remove_me = null;
                for( int i = 0; i < activeStoryBeats.Count; i++ ) 
                {
                    SBBase sb = activeStoryBeats[i];
                    if( remove_me == null && sb != null ) 
                    {
						if( sb.markedForRemoval) 
                        {
							remove_me = sb;
							sb.markedForRemoval = false;
                            break;
						}
					}
				}

				if( remove_me != null ) 
                {
					activeStoryBeats.Remove( remove_me );
				    BeginNextBeats( remove_me );			// adds any new story beats into the list.
				}
				else 
                {
					done = true;
				}				
			}
		}		
	}	

    void AddBeat(SBBase sb)
    {
        sb.BeginBeat();
        if (!sb.AllowAdd() ||
            !activeStoryBeats.Contains (sb))
        {
            activeStoryBeats.Add( sb );
        }
    }
}
