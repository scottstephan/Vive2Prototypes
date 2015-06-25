using UnityEngine;
using System.Collections.Generic;

public class SB_PlaySFX : SBBase {
	
    AudioSource myAudioSource;
    public Transform SFX3DTransform;
    public string SFX3DDesignGroupName;
    public bool DimsForInfoBeat = false;

    public bool Follow3DTransform = false;

    public float dimmedVolume = 0.1f;

    private bool done;
    private bool wasMuted;
    private bool wasPaused;
    private bool is3D;

    private bool fadeActive = false;
    private float ogVolume = 0f;
    private float curVolume = 0f;
    private float fadeRate = 0.75f;
    private float fadeDir = 1f;

    public override bool ContainsDesignGroup( string design_group ) { 
        if (string.IsNullOrEmpty( SFX3DDesignGroupName ) ) {
            return false;
        }
        
        return ( SFX3DDesignGroupName.ToUpper().Equals( design_group ) );
    }
    
    public override void Start()
    {
        myAudioSource = GetComponent<AudioSource>();

        if (myAudioSource == null)
        {
            Debug.LogError ("SB_PlaySFX " + gameObject.name + " has no audioSource!");
        }

        base.Start ();
    }

	public override void BeginBeat()
    {	
        done = true;
        is3D = false;
        if (myAudioSource != null)
        {
            ogVolume = myAudioSource.volume;

            if (SFX3DTransform != null)
            {
                is3D = true;
                gameObject.transform.position = SFX3DTransform.position;
                gameObject.transform.rotation = SFX3DTransform.rotation;
            }
            else if (!string.IsNullOrEmpty(SFX3DDesignGroupName))
            {
                CritterInfo c = GetCritterInDesignGroup(SFX3DDesignGroupName);

                if (c != null)
                {
                    is3D = true;
                    gameObject.transform.position = c.critterTransform.position;
                    gameObject.transform.rotation = c.critterTransform.rotation;
                }
            }

            done = false;
            wasMuted = IsMuted();
            wasPaused = SimInstance.Instance.IsSimPaused();
            myAudioSource.mute = wasMuted;
            if( ( SB_PlayInfoVoice.isPlaying && DimsForInfoBeat ) 
                 || SimInstance.Instance.slowdownActive ) {
                myAudioSource.volume = dimmedVolume;
                curVolume = dimmedVolume;
            }
            else {
                curVolume = myAudioSource.volume;
            }

            if( !wasPaused ) {
                myAudioSource.Play();
            }

/*            if (DimMusic)
            {
                AudioManager.Instance.DimTrack(WemoAudioTrackLogic.Ambient, myAudioSource.clip.length);
            }*/

#if UNITY_EDITOR
            Debug.Log ("SB_PlaySFX " + gameObject.name + " playing clip " + ((myAudioSource == null || myAudioSource.clip == null) ? "null" : myAudioSource.clip.name));
#endif
        }

		base.BeginBeat();
	}
	
    bool IsMuted()
    {
        if (is3D)
        {
            return !AudioManager.soundFXOn;
        }
        else
        {
            return !AudioManager.musicOn;
        }
    }

    public override void UpdateBeat()
    {
        if( wasPaused != SimInstance.Instance.IsSimPaused() ) {
            wasPaused = SimInstance.Instance.IsSimPaused();
            if( wasPaused ) {
                myAudioSource.Pause();
            }
            else if( curVolume > 0f ) {
                myAudioSource.Play();
            }
        }

        if( wasPaused ) {
            return;
        }

        if (wasMuted != IsMuted())
        {
            wasMuted = IsMuted();
            myAudioSource.mute = wasMuted;
        }

        if( fadeActive ) 
        {
            float inc = Time.deltaTime * fadeRate * fadeDir;
            curVolume += inc;
            if( fadeDir < 0f && curVolume <= dimmedVolume ) 
            {
                curVolume = dimmedVolume;
                if( curVolume <= 0f ) {
                    myAudioSource.Pause();
                }
                fadeActive = false;
            }
            else if( fadeDir > 0f && curVolume >= ogVolume ) 
            {
                curVolume = ogVolume;
                fadeActive = false;
            }
            myAudioSource.volume = curVolume;
        }
        if( myAudioSource.time > ( myAudioSource.clip.length * 0.9f ) ) {
            done = true;
        }
        if( !fadeActive && !done ) {
            if( ( ( SB_PlayInfoVoice.isPlaying && DimsForInfoBeat ) 
                 || SimInstance.Instance.slowdownActive ) 
               && myAudioSource.isPlaying ) {
                curVolume = myAudioSource.volume;
                fadeActive = true;
                fadeDir = -1f;
            }
            else if( !SB_PlayInfoVoice.isPlaying 
                    && DimsForInfoBeat
                    && !SimInstance.Instance.slowdownActive ) {
                curVolume = myAudioSource.volume;
                if( !myAudioSource.isPlaying ) {
                    myAudioSource.Play();
                }
                fadeActive = true;
                fadeDir = 1f;
            }
        }

        if( Follow3DTransform ) {
            if (SFX3DTransform != null)
            {
                gameObject.transform.position = SFX3DTransform.position;
                gameObject.transform.rotation = SFX3DTransform.rotation;
            }
            else if (!string.IsNullOrEmpty(SFX3DDesignGroupName))
            {
                CritterInfo c = GetCritterInDesignGroup(SFX3DDesignGroupName);
                
                if (c != null)
                {
                    gameObject.transform.position = c.critterTransform.position;
                    gameObject.transform.rotation = c.critterTransform.rotation;
                }
            }
        }

        base.UpdateBeat();
    }
	
	public override bool IsComplete() {
        return ( done );
	}			
}
