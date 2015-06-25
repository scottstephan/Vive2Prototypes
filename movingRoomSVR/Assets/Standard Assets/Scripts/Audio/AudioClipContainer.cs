using UnityEngine;
using System.Collections;

public class AudioClipContainer : MonoBehaviour 
{
    public AudioClip[] RandomClips;

    public bool cycleBreathAndBubbles;
    public AudioClip[] breaths;
    public AudioClip[] bubbles;

    int lastSelected = -1;
    public bool HasRandomClips
    {
        get
        {
            return RandomClips != null && RandomClips.Length > 0;
        }
    }

    public AudioClip GetRandomClip()
    {
        int randIndex;

        if (lastSelected >= 0 && RandomClips.Length > 1 && !cycleBreathAndBubbles)
        {
            randIndex = UnityEngine.Random.Range(0, RandomClips.Length-1);
            if (randIndex == lastSelected)
            {
                ++randIndex;
            }
        }
        else
        {
            randIndex = UnityEngine.Random.Range(0, RandomClips.Length);
        }

        lastSelected = randIndex;
        return RandomClips[randIndex];
    }


}
