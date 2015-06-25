using UnityEngine;
using UnityEditor;
//using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class StoryBeatTools : ScriptableWizard
{
    public string designGroup = "DESIGN GROUP";

    [MenuItem("WEMOTools/StoryBeats/Design Group : Select All")]
    static void CreateWizard() {
        ScriptableWizard.DisplayWizard("Design Group : Select All", typeof(StoryBeatTools), "SELECT THEM");
    }
    
    void OnWizardCreate() {
        designGroup = designGroup.ToUpper();

        // Attempt to cut LevelStreamingData, save as a prefab.
        GameObject streamingData = GameObject.Find("LevelStreamingData");
        if (streamingData == null) {            
            Debug.LogError("Select All cannot find the LevelStreamingData game object.");
            return;
        }

        List<Object> found_objects = new List<Object>();
        SBBase[] story_beats = streamingData.GetComponentsInChildren<SBBase>(true);

        if( story_beats == null || story_beats.Length <= 0 ) {
            Debug.Log("No basic story beats found.");
        }
        else {
            Debug.Log("Story beats found. [" + story_beats.Length + "]");
            for( int i = 0; i < story_beats.Length; i++ ) {
                SBBase sb = story_beats[i];
                if( sb == null ) {
                    continue;
                }
                if( sb.ContainsDesignGroup( designGroup ) ) {
                    Object ob = sb.gameObject as Object;

                    if( !found_objects.Contains(ob) ) {
                        found_objects.Add(ob);
                    }
                }
            }
        }

        StoryBeatDesignGroupAddOn[] story_beat_addons = streamingData.GetComponentsInChildren<StoryBeatDesignGroupAddOn>(true);
        
        if( story_beat_addons == null || story_beat_addons.Length <= 0 ) {
            Debug.Log("No StoryBeatDesignGroupAddOn found.");
        }
        else {
            Debug.Log("StoryBeatDesignGroupAddOn found. [" + story_beat_addons.Length + "]");
            for( int i = 0; i < story_beat_addons.Length; i++ ) {
                StoryBeatDesignGroupAddOn sba = story_beat_addons[i];
                if( sba == null ) {
                    continue;
                }

                if( sba.designGroupNames != null 
                   && sba.designGroupNames.Length > 0 ) {
                    for( int j = 0; j < sba.designGroupNames.Length; j++ ) {
                        if( sba.designGroupNames[j].ToUpper().Equals( designGroup ) ) {
                            Object ob = sba.gameObject as Object;
                            
                            if( !found_objects.Contains(ob) ) {
                                found_objects.Add(ob);
                            }
                        }
                    }
                }
            }
        }
        
        StoryBeatInteract[] story_beat_interacts = streamingData.GetComponentsInChildren<StoryBeatInteract>(true);
        
        if( story_beat_interacts == null || story_beat_interacts.Length <= 0 ) {
            Debug.Log("No StoryBeatInteracts found.");
        }
        else {
            Debug.Log("StoryBeatInteracts found. [" + story_beat_interacts.Length + "]");
            for( int i = 0; i < story_beat_interacts.Length; i++ ) {
                StoryBeatInteract sbi = story_beat_interacts[i];
                if( sbi == null ) {
                    continue;
                }
                
                if( sbi.designGroupName != null 
                   && sbi.designGroupName.Length > 0 ) {
                    if( sbi.designGroupName.ToUpper().Equals( designGroup ) ) {
                        Object ob = sbi.gameObject as Object;
                        
                        if( !found_objects.Contains(ob) ) {
                            found_objects.Add(ob);
                        }
                    }
                }
            }
        }
        Selection.objects = found_objects.ToArray();
        Debug.Log("Select Design Group references is complete.");
    }

    [MenuItem("WEMOTools/StoryBeats/Select Trigger Parents &m")]
    static void SelectParents() {
        if( Selection.objects.Length != 1 ) {
            Debug.LogError("Select Trigger Parents requires a single story beat to be selected." );
            return;
        }

        // Attempt to cut LevelStreamingData, save as a prefab.
        GameObject streamingData = GameObject.Find("LevelStreamingData");
        if (streamingData == null) {            
            Debug.LogError("Select Trigger Parents cannot find the LevelStreamingData game object.");
            return;
        }
        
        SBBase check_sb = Selection.activeGameObject.GetComponent<SBBase>();
        if( check_sb == null ) {
            Debug.LogError("Select Trigger Parents requires a single story beat to be selected. " );
            return;
        }

        List<Object> found_objects = new List<Object>();
        found_objects.Add(Selection.activeObject);

        // are we in any containers..
        StoryBeatsContainer[] story_containers = streamingData.GetComponentsInChildren<StoryBeatsContainer>();
        if( story_containers == null || story_containers.Length <= 0 ) {
            Debug.Log("No StoryBeatsContainer found.");
        }
        else {
            Debug.Log("StoryBeatsContainer found. [" + story_containers.Length + "]");
            for( int i = 0; i < story_containers.Length; i++ ) {
                StoryBeatsContainer sbc = story_containers[i];
                if( sbc == null ) {
                    continue;
                }
                if( sbc.startBeats != null && sbc.startBeats.Length > 0 ) {
                    for( int j = 0; j < sbc.startBeats.Length; j++ ) {
                        if( sbc.startBeats[j] == check_sb ) {
                            Object ob = sbc.gameObject as Object;
                            
                            if( !found_objects.Contains(ob) ) {
                                found_objects.Add(ob);
                            }
                        }
                    }
                }
            }
        }

        // are we in any next beats..
        SBBase[] story_beats = streamingData.GetComponentsInChildren<SBBase>(true);
        
        if( story_beats == null || story_beats.Length <= 0 ) {
            Debug.Log("No basic story beats found.");
        }
        else {
            Debug.Log("Story beats found. [" + story_beats.Length + "]");
            for( int i = 0; i < story_beats.Length; i++ ) {
                SBBase sb = story_beats[i];
                if( sb == null ) {
                    continue;
                }
                if( sb.TriggersStoryBeat( check_sb ) ) {
                    Object ob = sb.gameObject as Object;
                    
                    if( !found_objects.Contains(ob) ) {
                        found_objects.Add(ob);
                    }
                }
            }
        }

        // are we in any anim objects
        StoryBeatAnimObject[] story_anim = streamingData.GetComponentsInChildren<StoryBeatAnimObject>();
        if( story_anim == null || story_anim.Length <= 0 ) {
            Debug.Log("No StoryBeatAnimObject found.");
        }
        else {
            Debug.Log("StoryBeatAnimObject found. [" + story_anim.Length + "]");
            for( int i = 0; i < story_anim.Length; i++ ) {
                StoryBeatAnimObject sba = story_anim[i];
                if( sba == null ) {
                    continue;
                }
                if( sba.AnimationEvents != null && sba.AnimationEvents.Length > 0 ) {
                    for( int j = 0; j < sba.AnimationEvents.Length; j++ ) {
                        if( sba.AnimationEvents[j] == check_sb ) {
                            Object ob = sba.gameObject as Object;
                            
                            if( !found_objects.Contains(ob) ) {
                                found_objects.Add(ob);
                            }
                        }
                    }
                }
            }
        }

        // are we in any interact beats
        StoryBeatInteract[] story_interact = streamingData.GetComponentsInChildren<StoryBeatInteract>();
        if( story_interact == null || story_interact.Length <= 0 ) {
            Debug.Log("No StoryBeatInteract found.");
        }
        else {
            Debug.Log("StoryBeatInteract found. [" + story_interact.Length + "]");
            for( int i = 0; i < story_interact.Length; i++ ) {
                StoryBeatInteract sbi = story_interact[i];
                if( sbi == null ) {
                    continue;
                }
                if( sbi.triggerBeats != null && sbi.triggerBeats.Length > 0 ) {
                    for( int j = 0; j < sbi.triggerBeats.Length; j++ ) {
                        if( sbi.triggerBeats[j] == check_sb ) {
                            Object ob = sbi.gameObject as Object;
                            
                            if( !found_objects.Contains(ob) ) {
                                found_objects.Add(ob);
                            }
                        }
                    }
                }
            }
        }

        // are we in any trigger beats
        StoryBeatTrigger[] story_triggers = streamingData.GetComponentsInChildren<StoryBeatTrigger>();
        if( story_triggers == null || story_triggers.Length <= 0 ) {
            Debug.Log("No StoryBeatTrigger found.");
        }
        else {
            Debug.Log("StoryBeatTrigger found. [" + story_triggers.Length + "]");
            for( int i = 0; i < story_triggers.Length; i++ ) {
                StoryBeatTrigger sbt = story_triggers[i];
                if( sbt == null ) {
                    continue;
                }

                if( sbt.triggerBeats != null && sbt.triggerBeats.Length > 0 ) {
                    for( int j = 0; j < sbt.triggerBeats.Length; j++ ) {
                        if( sbt.triggerBeats[j] == check_sb ) {
                            Object ob = sbt.gameObject as Object;
                            
                            if( !found_objects.Contains(ob) ) {
                                found_objects.Add(ob);
                            }
                        }
                    }
                }
            }
        }

        Selection.objects = found_objects.ToArray();
        Debug.Log ("All parent triggers are selected.");
    }
}
