using UnityEngine;
using System.Collections;

//
// DEPRECATED! 3/21/2012!
// NEW ANIMATION HIERARCHY HAS BEEN IMPLEMENTED.
// THIS PARTICULAR CLASS HAS BEEN KEPT FOR BACKWARDS COMPATIBILITY 
// AND GETS REDIRECTED INTO THE OGFishAnimation CLASS AT LOAD TIME.
//

public class FishAudioData : MonoBehaviour {
    public enum FishSizeCategory
    {
        Small,
        Medium,
        Large
    }

    public FishSizeCategory fishSize = FishSizeCategory.Small;

    public AudioClip passbyClip;
    public AudioClip passbyClipAlt1;
    public AudioClip passbyClipAlt2;
    public float passbyDistMax = 1000f;

    public AudioClip swimAwayClip;
    public AudioClip swimAwayClipAlt1;
    public AudioClip swimAwayClipAlt2;
    public float swimAwayDistMax = 500f;

    [HideInInspector]
    public float swimAwayLastPlayTime = -10; // set negative so first time check will pass without app needing to be running for a few seconds

    public GameObject strokeAudioObject = null;
    private AudioSource[] strokeAudioSources = null;
    public AudioClip[] strokes = null;
    public AudioClip[] upStrokes = null;
    public AudioClip[] downStrokes = null;

    public GameObject sfxAudioObject = null;
    private AudioSource sfxAudioSource = null;

    void Awake() {
        if( sfxAudioObject != null ) {
            sfxAudioSource = sfxAudioObject.GetComponent<AudioSource>();
        }
        if( strokeAudioObject != null ) {
            strokeAudioSources = strokeAudioObject.GetComponents<AudioSource>();
#if UNITY_ANDROID && !UNITY_EDITOR
            if( strokeAudioSources != null && strokeAudioSources.Length > 0) {
                for(int i = 0; i < strokeAudioSources.Length; i++ ) {
                    strokeAudioSources[i].volume *= AudioManager.AndroidVolumeBoost;
                    strokeAudioSources[i].volume = Mathf.Clamp01(strokeAudioSources[i].volume);
                }
            }
#endif
        }
    }

    public void PlaySFXClip() {
        if( FloatingMenuManager.IsMenuUp() ||  sfxAudioSource == null || SimInstance.Instance.slowdownActive || SimInstance.Instance.IsSimPaused() ) {
            return;
        }

        sfxAudioSource.time = 0f;
        sfxAudioSource.Play();
    }

    public void PlayUpStrokeClip() {
        if (strokeAudioSources == null || strokeAudioSources.Length <= 0)
        {
            return;
        }

        AudioClip p = null;
        AudioSource src = null;
        for(int i = 0; i < strokeAudioSources.Length; i++ ) {
            if( !strokeAudioSources[i].isPlaying ) {
                src = strokeAudioSources[i];
                i = strokeAudioSources.Length;
            }
        }

        if( src == null ) {
            return;
        }        

        if( upStrokes == null ) {
            if( strokes == null ) {
                return;
            }
            
            p = strokes[Random.Range(0,strokes.Length)];
        }
        else {
            p = upStrokes[Random.Range(0,upStrokes.Length)];
        }

        if( p == null ) {
            return;
        }

        Debug.Log("Playing upstroke" + p.name + " at " + Time.timeSinceLevelLoad);

        src.clip = p;
        src.Play();
    }
    
    public void PlayDownStrokeClip() {
        
        if(strokeAudioSources == null || strokeAudioSources.Length <= 0) {
            return;
        }

        AudioClip p = null;
        AudioSource src = null;
        for(int i = 0; i < strokeAudioSources.Length; i++ ) {
            if( !strokeAudioSources[i].isPlaying ) {
                src = strokeAudioSources[i];
                i = strokeAudioSources.Length;
            }
        }

        if( src == null ) {
            return;
        }

        if( downStrokes == null ) {
            if( strokes == null ) {
                return;
            }
            
            p = strokes[Random.Range(0,strokes.Length)];
        }
        else {
            p = downStrokes[Random.Range(0,downStrokes.Length)];
        }
        
        if( p == null ) {
            return;
        }

        src.clip = p;
        src.Play();
        Debug.Log("Playing downstroke" + p.name + " at " + Time.timeSinceLevelLoad);

    }

    public void PlayStrokeClip() {
        if( FloatingMenuManager.IsMenuUp() || strokeAudioSources == null || strokeAudioSources.Length <= 0 || SimInstance.Instance.slowdownActive || SimInstance.Instance.IsSimPaused() ) {
            return;
        }
       
        AudioClip p = null;
        AudioSource src = null;
        for(int i = 0; i < strokeAudioSources.Length; i++ ) {
            if( !strokeAudioSources[i].isPlaying ) {
                src = strokeAudioSources[i];
                i = strokeAudioSources.Length;
            }
        }
        
        if( strokes == null || src == null ) {
            return;
        }
        
        p = strokes[Random.Range(0,strokes.Length)];

        if( p == null ) {
            return;
        }

        src.clip = p;
        StartCoroutine("rampUpSound", src);
        src.Play();
        Debug.Log("Playing STROKE" + p.name + " at " + Time.timeSinceLevelLoad);
    }

    public AudioClip GetPassbyClip()
    {
        int max = (passbyClip != null ? 1 : 0) + (passbyClipAlt1 != null ? 1 : 0) + (passbyClipAlt2 != null ? 1 : 0);

        int index = Random.Range(0, max);

        if (index == 2)
        {
            return passbyClipAlt2;
        }
        else if (index == 1)
        {
            if (passbyClipAlt1 != null)
            {
                return passbyClipAlt1;
            }
            else
            {
                return passbyClipAlt2;
            }
        }

        return passbyClip;
    }

    public AudioClip GetSwimAwayClip()
    {
        int max = (swimAwayClip != null ? 1 : 0) + (swimAwayClipAlt1 != null ? 1 : 0) + (swimAwayClipAlt2 != null ? 1 : 0);
        
        int index = Random.Range(0, max);
        
        if (index == 2)
        {
            return swimAwayClipAlt2;
        }
        else if (index == 1)
        {
            if (swimAwayClipAlt1 != null)
            {
                return swimAwayClipAlt1;
            }
            else
            {
                return swimAwayClipAlt2;
            }
        }
        
        return swimAwayClip;
    }

    IEnumerator rampUpSound(AudioSource src)
    {
        float duration = 0.5f;
        float targetVol = src.volume;
        float vol = 0;
        float t = 0;
        src.volume = 0;

        while(vol < targetVol){
            vol = Mathf.Lerp(0,targetVol,t);
            t += Time.deltaTime / duration;
            src.volume = vol;
        }

        yield return new WaitForSeconds(0);
    }
}

