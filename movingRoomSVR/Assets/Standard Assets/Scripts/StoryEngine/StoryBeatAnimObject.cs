using UnityEngine;
using System.Collections;


public class StoryBeatAnimObject : MonoBehaviour {
    public SBBase[] AnimationEvents;

    public void TriggerStoryBeat(int animEventIndex)
    {
        if (animEventIndex < 0 || animEventIndex >= AnimationEvents.Length)
        {
            Debug.LogError("StoryBeatAnimObject " + gameObject.name + " bad index in animation event: " + animEventIndex);
            return;
        }

        if( AnimationEvents[animEventIndex] == null ) {
            return;
        }

#if UNITY_EDITOR
        Debug.Log ("StoryBeatAnimObject anim event " + gameObject.name + " trigger " + AnimationEvents[animEventIndex].gameObject.name);
#endif

        if( StoryManager.Instance != null 
            && StoryManager.Instance.storyBeatsContainer != null )
        {
            SBBase[] beginBeats = new SBBase[1];
            beginBeats[0] = AnimationEvents[animEventIndex];
            StoryManager.Instance.storyBeatsContainer.BeginBeats( beginBeats );
        }
    }
}
