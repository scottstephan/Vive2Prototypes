using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public enum SoundFXID : int
{
    PanelClick = 0,
    GetInfo = 1, // not used
	Purchased = 2, // gone
	SetCompletion = 3, // gone
	FoundWaldo = 4, //gone
	WaldoBeep = 5, // gone
	NewWaldo = 6,
	TravelTransition = 7,
    OwnershipMoment = 8, // gone
	Roam1 = 9, // gone
    Roam2 = 10,// gone
    Roam3 = 11,// gone
	Migration = 12, //gone
    Schooling = 13,
    Still = 14,
    Moving = 15,
    ClownfishInteractInitial = 16,
    ClownfishInteractCurious = 17,
    ClownfishInteractScared = 18,
    Passby1 = 19,
    Passby2 = 20,
    Disperse1 = 21,
    Disperse2 = 22,
	IntroSplash = 23,
	MenuSelect = 24,
	TravelAmbient = 25,
    TravelMoving = 26,
}


public class AmbientAudioQueueData {
	public AudioClip audioClipToPlay;	// will replace the ambient track.
	public SoundFXID soundIDToPlay;
	public float fadeInTime;
    public float crossFadeOutTime;
	public int atBeat;
	public int offset;
	public int bpm;
	public WemoAudioTrackLogic trackLogic;
}

public class SFXData {
	public GameObject sfxObject;
    public AudioSource sfxSource;
    public Transform sfxTransform;
    public Transform attachTransform;
    public Vector3 attachToOffset;
    public float fade;
    public float maxVolume;
    public AudioClipContainer randomClips;
    public bool bFakeBinaural;
}

[System.Serializable]
public class CritterMusicData
{
    public CritterMusicType musicType;

    public GameObject critterMusicObject;
    [HideInInspector]
    public Transform critterMusicTransform;
    [HideInInspector]
    public AudioSource critterMusicSource;
	
	public float maxVolume = 0.5f;
    public float targetAmbientVolume = 0.5f;
	
	public float playTimeMin = 15f;
	public float playTimeMax = 30f;
	public float playTimeBias = 25f;
	public float playTimePower = 1.5f;
	public float dontPlayTimeMin = 45f;
	public float dontPlayTimeMax = 60f;
	public float dontPlayTimeBias = 55f;
	public float dontPlayTimePower = 1.5f;
	
	[HideInInspector]
	public float playTimer;
	[HideInInspector]
	public bool playing;
	[HideInInspector]
	public bool canPlay;
	
    [HideInInspector]
    public float distance;
    [HideInInspector]
    public float targetDistance;
    [HideInInspector]
    public float targetSqrdDistance;
}

public class AudioManager : MonoBehaviour
{
    private static AudioManager singleton;
    public static AudioManager Instance {
        get {
            return singleton;
        }
    }
	
	public List<AmbientAudioQueueData> ambientAudioQueue;

    public GameObject audioListenerObject;
    private Transform audioListenerTransform;
    
    public GameObject binauralAudioListenerObject;
    private Transform binauralAudioListenerTransform;
    
    public GameObject ambientMusicObject;
    private AudioSource ambientMusicSource;

    public float maxAmbientVolume = 1.0f;
    public float ambientVolumeSpeed = 0.5f;
    private float targetAmbientVolume = 0.0f;
    private float ambientVolume = 0.0f;

	private bool swappedIntroClip = false;
	
    public GameObject infoVoiceObject;
    private AudioSource infoVoiceSource;
    private bool infoVoicePaused;

    public float maxPlayerFeelVolume = 0.5f;

    private static bool globalMute = false;
	public static bool GlobalMute {
		get {
			return globalMute;
		}
	}

    public float fadeTime = 10.0f;

    public float passbySmallLimitTimeGlobal = 7f;
    public float passbyMediumLimitTimeGlobal = 7f;
    public float passbyLargeLimitTimeGlobal = 4f;
    public float swimAwayLimitTimeGlobal = 4f;
    public float swimAwayLimitTimePerFish = 10f;
    float swimAwayLastPlayTime = -10;
    float passbySmallLastPlayTime = -10;
    float passbyMediumLastPlayTime = -10;
    float passbyLargeLastPlayTime = -10;

    public float FakeBinauralPanScale = 1.5f;
    public float DimMusicVolume = 0.5f;

	bool enableLogic_Ambient = false;

	List<AudioClip> clips;
	List<AssetBundle> bundles;
	private int currentAudioSphereID = -1;

	private float volumeDropTimer;
	private float volumeDropScale = 1f;
	private float desiredVolumeDropScale = 1f;
	private float volumeDropScaleSpeed = 5.0f;
	
    public float globalMusicVolume = 1f;

	static bool _musicOn = true;
	static bool _soundFXOn = true;
    static bool _narrationOn = true;

    public AudioSource bubbleEffectSource = null;
    private AudioClipContainer bubbleClips = null;
    public float bubbleMinTime = 3.9f; // cant go lower without overlap of audio lengths.
    public float bubbleMaxTime = 6f;
    public float bubbleMinVolume = 0.25f;
    public float bubbleMaxVolume = 0.45f;
    public float bubbleTimer = 0f;

	public static bool musicOn { get{return(_musicOn);}}
	public static bool soundFXOn {get{return(_soundFXOn);}}
    public static bool narrationFXOn {get{return(_narrationOn);}}

    public const float AndroidVolumeBoost = 1f; // 2.5x is fullvolume match. need to be a bit more to account for volume range and actually hearing it at rest.

	// TODO> this directly matches the Enum defined above. THIS IS BAD. we need a better system. This is quick and dirty.
	List<SFXData> sfxData;
	
	public static void SetupVolumeDropScale( float desired_scale, float time ) {
		singleton.volumeDropTimer = time;
		singleton.desiredVolumeDropScale = desired_scale;
	}
	
	void UpdateVolumeDropScale( float dt ) {
		if( volumeDropScale != desiredVolumeDropScale ) {
			float amt = volumeDropScaleSpeed * dt;
			if( volumeDropScale < desiredVolumeDropScale ) {
				volumeDropScale += amt;
				if( volumeDropScale > desiredVolumeDropScale ) {
					volumeDropScale = desiredVolumeDropScale;
				}
			}
			else if( volumeDropScale > desiredVolumeDropScale ) {
				volumeDropScale -= amt;
				if( volumeDropScale < desiredVolumeDropScale ) {
					volumeDropScale = desiredVolumeDropScale;
				}
			}
		}
		
		if( volumeDropTimer > 0f ) {
			volumeDropTimer -= dt;
			if( volumeDropTimer <= 0f ) {
				desiredVolumeDropScale = 1f;
			}
		}
	}
	
	void SetupCritterMusicData( CritterMusicData md ) {
        md.critterMusicSource = md.critterMusicObject.GetComponent<AudioSource>();
        md.critterMusicTransform = md.critterMusicObject.transform;
        md.distance = md.critterMusicSource.maxDistance;
        md.targetDistance = md.critterMusicSource.maxDistance;
        md.targetSqrdDistance = md.targetDistance * md.targetDistance;
        Vector3 offset = audioListenerTransform.forward * md.distance;
        md.critterMusicTransform.position = audioListenerTransform.position + offset;
		md.playTimer = RandomExt.FloatWithRawBiasPower(md.dontPlayTimeMin,md.dontPlayTimeMax,md.dontPlayTimeBias,md.dontPlayTimePower);
		md.canPlay = false;
		md.playing = false;
	}
	
	void Awake() {
		singleton = this;
		swappedIntroClip = false;
		volumeDropTimer = 0f;
		volumeDropScale = 1f;
		desiredVolumeDropScale = 1f;
		ambientAudioQueue = new List<AmbientAudioQueueData>();
    
        audioListenerTransform = audioListenerObject.transform;
        
        if( binauralAudioListenerObject != null ) {
            binauralAudioListenerTransform = binauralAudioListenerObject.transform;
        }        

        if (CameraManager.singleton != null)
        {
			CameraManager.singleton.InitAudioMgr();
        }
    }

    void Start()
    {
        if( infoVoiceObject != null ) {
            infoVoiceSource = infoVoiceObject.GetComponent<AudioSource>();
        }

        ambientMusicSource = ambientMusicObject.GetComponent<AudioSource>();

//        ambientMusicSource.loop = true;
//        ambientMusicSource.volume = 0.0f;

        if( bubbleEffectSource != null ) {
            bubbleClips = bubbleEffectSource.gameObject.GetComponent<AudioClipContainer>();
        }
        
//        openMusicSource.volume = maxOpenVolume;
//        openMusicSource.loop = true;
//        openMusicSource.Play();
		
		int n = Enum.GetNames(typeof(SoundFXID)).Length;
		sfxData = new List<SFXData>();
		for( int i = 0; i < n; i++ )
        {
            SFXData new_data = new SFXData();
			new_data = new SFXData();
            string sfxObjName = ((SoundFXID)i).ToString()+"SFX";
            new_data.sfxObject = GameObject.Find(sfxObjName);
			if(new_data.sfxObject != null)
            {
                new_data.sfxSource = new_data.sfxObject.GetComponent<AudioSource>();
                new_data.sfxTransform = new_data.sfxObject.transform;
                new_data.randomClips = new_data.sfxObject.GetComponent<AudioClipContainer>();

                if (new_data.sfxSource != null)
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                    new_data.sfxSource.volume *= AndroidVolumeBoost;
                    new_data.sfxSource.volume = Mathf.Clamp01( new_data.sfxSource.volume );
#endif
                    new_data.maxVolume = new_data.sfxSource.volume;
                }
                else
                {
//                    Debug.LogError("AudioManager - SFX Object does not contain audio source component " + sfxObjName);
                }
            }
            else
            {
//                Debug.LogError("AudioManager - Could not find SFX object for audio source " + sfxObjName);
            }

            sfxData.Add( new_data );
		}
		
        fadeData = new List<FadeVolumeData>();
        swapFadeData = new List<FadeVolumeData>();
        n = Enum.GetNames(typeof(WemoAudioTrackLogic)).Length;
        for (int i = 0; i < n; i++)
        {
            fadeData.Add(new FadeVolumeData());
            swapFadeData.Add(new FadeVolumeData());
        }

        clips = new List<AudioClip>();
        n = Enum.GetNames(typeof(WemoAudioTracks)).Length;
        for (int i = 0; i < n; i++)
        {
            clips.Add(null);
        }

		bundles = new List<AssetBundle>();
		n = Enum.GetNames(typeof(WemoAudioTracks)).Length;
		for (int i = 0; i < n; i++)
		{
			bundles.Add(null);
		}

        fadeData[(int)WemoAudioTrackLogic.Intro].onFadeEffectFinished = new OnFadeEffectFinished(OnIntroFadedOut);
        fadeData[(int)WemoAudioTrackLogic.Ambient].onFadeEffectFinished = new OnFadeEffectFinished(OnAmbientFaded);
        swapFadeData[(int)WemoAudioTrackLogic.Intro].onFadeEffectFinished = new OnFadeEffectFinished(OnIntroFadedOut);
        swapFadeData[(int)WemoAudioTrackLogic.Ambient].onFadeEffectFinished = new OnFadeEffectFinished(OnAmbientFaded);

		shouldFadeOutIntro = false;
		PlayIntroAudio( true );
		
		SetGlobalMute( (PlayerPrefs.GetInt(UserManager.USER_AUDIO_MUTE,0) > 0) ? true : false);

		DetectSettings();

#if UNITY_ANDROID && !UNITY_EDITOR
        bubbleMinVolume *= AndroidVolumeBoost;
        bubbleMaxVolume *= AndroidVolumeBoost;
#endif
    }

	public bool IsPlayingIntro() {
		return !swappedIntroClip;
	}

	public bool AmbientAtBeat( int at_beat, int offset, int bpm ) {
		if( ambientMusicSource.isPlaying )
        {
			float bps = ( bpm / 60f );
			float offset_time = bps * offset;

			float t = ambientMusicSource.time;
			if( t <= offset_time )
            {
				return false;
			}
			
            int cur_beat = (int)( ( t - offset_time ) / bps );
            if ( cur_beat % at_beat == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
		}
        else
        {
		    return true; // if no ambient start immediately
        }
	}

	public void QueueSoundIDWithAmbientBeat( SoundFXID sound_id, int at_beat, int offset=0, int bpm = 103 ) {
		if( AmbientAtBeat( at_beat, offset, bpm ) )
        {
			PlayInGameAudio( sound_id );
			return;
		}
		
		AmbientAudioQueueData queue_data = new AmbientAudioQueueData();
		queue_data.audioClipToPlay = null;
		queue_data.soundIDToPlay = sound_id;
		queue_data.fadeInTime = 0f;
		queue_data.atBeat = at_beat;
		queue_data.offset = offset;
		queue_data.bpm = bpm;

		ambientAudioQueue.Add( queue_data );
	}
	
	public void QueueAudioClipWithAmbientBeat( AudioClip audio_clip, WemoAudioTrackLogic track_logic, float fadeInTime, float crossFadeOutTime, int at_beat, int offset=0, int bpm=103 ) {
		if( AmbientAtBeat( at_beat, offset, bpm ) ) 
        {
            PlayTrack(audio_clip, track_logic, fadeInTime, crossFadeOutTime);
			return;
		}

		//Debug.Log ("QUEUE AUDIO CLIP! " + audio_clip.name);
		AmbientAudioQueueData queue_data = new AmbientAudioQueueData();
		queue_data.audioClipToPlay = audio_clip;
		queue_data.trackLogic = track_logic;
		queue_data.fadeInTime = fadeInTime;
        queue_data.crossFadeOutTime = crossFadeOutTime;
        queue_data.atBeat = at_beat;
		queue_data.offset = offset;
		queue_data.bpm = bpm;
		ambientAudioQueue.Add( queue_data );
	}
	
	public void UpdateAmbientQueue() 
    {
		if( !swappedIntroClip ) 
        {
			return;
		}

        for( int i = 0; i < ambientAudioQueue.Count; i++ ) 
        {
            AmbientAudioQueueData ad = ambientAudioQueue[i];

			if( AmbientAtBeat( ad.atBeat, ad.offset, ad.bpm ) )
            {
				if( ad.audioClipToPlay != null ) 
                {
                    singleton.PlayTrack(ad.audioClipToPlay, ad.trackLogic, ad.fadeInTime, ad.crossFadeOutTime);
				}
				else
                {
					PlayInGameAudio( ad.soundIDToPlay );
				}

                ambientAudioQueue.RemoveAt(i--);
			}
		}
	}

    public void MusicFullStop() {
        ambientMusicSource.Stop();
        ambientMusicSource.clip = null;
        ambientMusicSource.time = 0;
    }

	public void PlayIntroAudio( bool full_init ) {
		startTime = DateTime.Now;
		if( full_init ) {
            MusicFullStop();
        }

        // turn this off for now
//      FadeIn(WemoAudioTrackLogic.Intro);
        swappedIntroClip = true; // and turn this on to allow audio
        enableLogic_Ambient = true;
	}
	
	public void StopIntroAudio() {
		shouldFadeOutIntro = true;
	}
	
    DateTime startTime;
    bool shouldFadeOutIntro = false;
    bool canFadeOutIntro
    {
        get
        {
            TimeSpan timePlayed = DateTime.Now - startTime;
			return ( timePlayed.TotalSeconds > 6f );
        }
    }	
	
    void UpdatePlayerBubbles( float dt ) {
        if( bubbleClips == null || SimInstance.Instance.slowdownActive || SimInstance.Instance.IsSimPaused() ) {
            return;
        }

        bubbleTimer -= dt;
        if( bubbleTimer > 0f ) {
            return;
        }

        AudioClip bc = bubbleClips.GetRandomClip();
        bubbleEffectSource.volume = UnityEngine.Random.Range( bubbleMinVolume, bubbleMaxVolume );
        bubbleEffectSource.PlayOneShot( bc );
        bubbleTimer = UnityEngine.Random.Range( bubbleMinTime, bubbleMaxTime );
    }

    void Update()
    {
        if (globalMute)
        {
            return;
        }

        if (!_soundFXOn && !_musicOn && !_narrationOn)
			return;

		if (shouldFadeOutIntro && canFadeOutIntro)
        {
            FadeOut(WemoAudioTrackLogic.Intro);
            shouldFadeOutIntro = false;
        }

        float dt = Time.deltaTime;

		UpdateVolumeDropScale( dt );

        CalculateFadeRatio(dt, WemoAudioTrackLogic.Intro, fadeData);
        if (enableLogic_Ambient)
        {
            //track our active ambient adjustments
            targetAmbientVolume = maxAmbientVolume;			
			
            ambientVolume = Mathf.MoveTowards(ambientVolume, targetAmbientVolume, ambientVolumeSpeed * dt);           
            CalculateFadeRatio(dt, WemoAudioTrackLogic.Ambient, fadeData);

            ambientMusicSource.volume = ambientVolume * fadeData[(int)WemoAudioTrackLogic.Ambient].fadeRatio * volumeDropScale * globalMusicVolume;
        }

		UpdateAmbientQueue();

	
        UpdatePlayerBubbles( dt );

        Vector3 camPos = CameraManager.GetEyePosition();
        int numSFXData = sfxData.Count;
        for (int i=0; i<numSFXData; ++i)
        {
            SFXData sfx = sfxData[i];
            if (sfx == null || sfx.sfxSource == null)
            {
                continue;
            }

            bool bIsPlaying = sfx.sfxSource.isPlaying;

            if (sfx.attachTransform != null)
            {
                if (bIsPlaying)
                {
                    sfx.sfxTransform.position = sfx.attachTransform.localToWorldMatrix.MultiplyPoint(sfx.attachToOffset);
                }
                else
                {
                    sfx.attachTransform = null;
                }
            }

            if (sfx.fade != 0f)
            {
                sfx.sfxSource.volume += sfx.fade * dt;
//                Debug.Log ("Fade " + sfx.sfxSource.clip.name + " " + sfx.sfxSource.volume);

                if (sfx.fade < 0f && sfx.sfxSource.volume <= 0f)
                {
                    Stop((SoundFXID)i);
                    sfx.sfxSource.volume = sfx.maxVolume;
                }
                else if (sfx.fade > 0f && sfx.sfxSource.volume >= sfx.maxVolume)
                {
                    sfx.sfxSource.volume = sfx.maxVolume;
                    sfx.fade = 0f;// stop fade
                }
            }

            if (bIsPlaying &&
                sfx.bFakeBinaural)
            {
				Vector3 camRight = CameraManager.GetCurrentCameraRight();
            	Vector3 sourceToCamDir = (sfx.sfxTransform.position - camPos).normalized;
            	float rightDot = Vector3.Dot (sourceToCamDir, camRight);
            	sfx.sfxSource.panStereo = Mathf.Clamp (rightDot * FakeBinauralPanScale, -1f, 1f);
            }
        }
    }
	
	public static void ToggleGlobalMute()
	{
		SetGlobalMute( !globalMute );
	}
	
    public static void SetGlobalMute(bool mute)
    {
		PlayerPrefs.SetInt( UserManager.USER_AUDIO_MUTE,(mute ? 1 : 0 ) );

        bool bWasMute = globalMute;

        //DebugDisplay.AddDebugText("Setting mute " + mute);
        globalMute = mute;

        singleton.ambientMusicSource.mute = mute;
		
		// make sure we are playing the correct audio when we come out of mute..
        if( bWasMute && !bWasMute)
        { 
            // somewhat of a hack to ensure our initial setting doesnt do this.
			singleton.SwitchAudioTracks();
		}
    }

	public static void SetMusicMute(bool mute) {

		if (singleton == null)
			return;

		if (singleton.ambientMusicSource == null)
			return;

		singleton.ambientMusicSource.mute = mute;
	}

	public static void SetSFXMute(bool mute) {

		if (singleton.sfxData == null)
			return;

		for (int i = 0; i < singleton.sfxData.Count; i++) {

			SFXData sfd = singleton.sfxData[i];

			if (sfd.sfxSource == null)
				continue;

			sfd.sfxSource.mute = mute;
		}

        if (singleton.bubbleEffectSource != null)
        {
            singleton.bubbleEffectSource.mute = mute;
        }

		if (OculusFPSCameraMode.singleton && OculusFPSCameraMode.singleton.playerFeelFX != null)
			OculusFPSCameraMode.singleton.playerFeelFX.GetComponent<AudioSource>().mute = mute;
	}

    public static void InitAudioSourceTransform(Transform audio_source)
    {
        if (singleton == null)
        {
            return;
        }

        singleton.audioListenerTransform.position = audio_source.position;
        singleton.audioListenerTransform.rotation = audio_source.rotation;

        if( singleton.binauralAudioListenerTransform != null ) {
            singleton.binauralAudioListenerTransform.position = audio_source.position;
            singleton.binauralAudioListenerTransform.rotation = audio_source.rotation;
        }
    }

    public static void UpdateAudioSourceTransform(Vector3 pos, Quaternion rot)
    {
        singleton.audioListenerTransform.position = Vector3.MoveTowards(singleton.audioListenerTransform.position, pos, 15.0f);
        singleton.audioListenerTransform.rotation = Quaternion.RotateTowards(singleton.audioListenerTransform.rotation, rot, 15.0f);
        
        if( singleton.binauralAudioListenerTransform != null ) {
            singleton.binauralAudioListenerTransform.position = pos;
            singleton.binauralAudioListenerTransform.rotation = rot;
        }
    }

    public static void UpdateAudioSourceTransform(Transform audio_source)
    {
        UpdateAudioSourceTransform(audio_source.position, audio_source.rotation);
    }
	
    public bool IsInfoVoiceClipPlaying() {
        return ( infoVoiceSource != null && infoVoiceSource.isPlaying );
    }

    public bool PlayInfoVoiceClip( AudioClip clip ) 
    {
        if (!_narrationOn)
        {
            return false;
        }

        if( infoVoiceSource != null && 
           !infoVoiceSource.isPlaying ) 
        {
            infoVoiceSource.clip = clip;
            infoVoiceSource.time = 0f;
            if( !infoVoicePaused ) {
                infoVoiceSource.Play();
            }
        }

		return true;
    }

    public void StopInfoVoiceClip( ) 
    {
        if( infoVoiceSource != null)
        {
            infoVoiceSource.Stop ();
            infoVoiceSource.clip = null;
        } 
    }

    public void PauseInfoVoiceClip( ) 
    {
        if( infoVoiceSource != null &&
            infoVoiceSource.isPlaying)
        {
            infoVoicePaused = true;
            infoVoiceSource.Pause();
        } 
    }

    public void ResumeInfoVoiceClip( ) 
    {
        if( infoVoiceSource != null &&
            infoVoicePaused )
        {
            infoVoicePaused = false;
            infoVoiceSource.Play();
        } 
    }

    public void PlayAudio(int audio_id)
	{
        if( globalMute ) {
			return;
		}

		if (!_soundFXOn)
			return;
			
		if( audio_id >= 0 
			&& audio_id < sfxData.Count 
			&& sfxData[audio_id].sfxSource != null ) 
        {
            sfxData[audio_id].bFakeBinaural = false;
            sfxData[audio_id].attachTransform = null;

            if (sfxData[audio_id].randomClips != null &&
                sfxData[audio_id].randomClips.HasRandomClips)
            {
                sfxData[audio_id].sfxSource.clip = sfxData[audio_id].randomClips.GetRandomClip();
            }

            sfxData[audio_id].sfxSource.Play();
		}
	}
	
    public void PlayAudio( AudioSource one_off_audio )
    {
        if( globalMute ) {
            return;
        }

		if (!_soundFXOn)
			return;
        
        if( one_off_audio != null ) {
            one_off_audio.Play();
        }
    }

	public void PlayOneShot(AudioClip clip) {

		if (globalMute)
			return;

		if (clip == null)
			return;

		if (!_soundFXOn)
			return;

		GetComponent<AudioSource>().PlayOneShot(clip); //fix this because it LEAKS!!!??
	}
 
	public AudioClip GetAudioClip(SoundFXID _audio_id) {
		int audio_id = (int)_audio_id;
		if( audio_id >= 0 
            && audio_id < sfxData.Count 
            && sfxData[audio_id] != null 
            && sfxData[audio_id].sfxSource != null ) {
			return sfxData[audio_id].sfxSource.clip;
		}
		return null;
	}
	
    public void PitchAudio( SoundFXID _audio_id, float new_pitch ) {
        int audio_id = (int)_audio_id;
        if( audio_id >= 0 
           && audio_id < sfxData.Count 
           && sfxData[audio_id] != null 
           && sfxData[audio_id].sfxSource != null ) {
            sfxData[audio_id].sfxSource.pitch = Mathf.SmoothStep( sfxData[audio_id].sfxSource.pitch, new_pitch, 2f * Time.deltaTime ); 
        }
    }

	public static void PlayInGameAudio(SoundFXID audio_id)
	{
		if( globalMute ) {
			return;
		}

		if (!_soundFXOn)
			return;
			
		singleton.PlayAudio((int)audio_id);
	}
	
	public static void PlayInGameAudio(int audio_id)
	{
        if( globalMute ) {
			return;
		}

		if (!_soundFXOn)
			return;

		singleton.PlayAudio(audio_id);
	}

	public static void PlaySFXAtPosition(Vector3 pos, SoundFXID sfx)
	{
        int audio_id = (int) sfx;
//        Debug.Log("AudioManager Play at pos" + pos + " " + ((SoundFXID)audio_id));
		if( audio_id >= 0 
           && audio_id < singleton.sfxData.Count 
		   && singleton.sfxData[audio_id].sfxSource != null )
        {
            singleton.PlayAudio(audio_id);
            singleton.sfxData[audio_id].bFakeBinaural = false;
            singleton.sfxData[audio_id].fade = 0f;           
            singleton.sfxData[audio_id].sfxTransform.position = pos;
        }
	}

    public static void PlaySFXAtObject(GameObject go, Vector3 offset, SoundFXID sfx)
    {
//        Debug.Log("AudioManager Play at object" + go.name + " " + go.transform.position + " " + offset + " " + ((SoundFXID)audio_id));
        int audio_id = (int) sfx;

        if( audio_id >= 0 
           && audio_id < singleton.sfxData.Count 
           && singleton.sfxData[audio_id].sfxSource != null )
        {
            singleton.PlayAudio(audio_id);
            singleton.sfxData[audio_id].bFakeBinaural = false;
            singleton.sfxData[audio_id].fade = 0f;           
            singleton.sfxData[audio_id].sfxTransform.position = go.transform.localToWorldMatrix * offset;
            singleton.sfxData[audio_id].attachTransform = go.transform;
            singleton.sfxData[audio_id].attachToOffset = offset;
        }
    }

    public static void PlayCritterPassby(CritterInfo critter_info)
    {
        if (critter_info == null ||
            critter_info.audioData == null ||
            critter_info.audioData.passbyClip == null)
        {
            return;
        }

        if ((critter_info.audioData.fishSize == FishAudioData.FishSizeCategory.Small && (Time.time - singleton.passbySmallLastPlayTime < singleton.passbySmallLimitTimeGlobal)) ||
            (critter_info.audioData.fishSize == FishAudioData.FishSizeCategory.Medium && (Time.time - singleton.passbyMediumLastPlayTime < singleton.passbyMediumLimitTimeGlobal)) ||
            (critter_info.audioData.fishSize == FishAudioData.FishSizeCategory.Large && (Time.time - singleton.passbyLargeLastPlayTime < singleton.passbyLargeLimitTimeGlobal))) 
        {
            return;
        }

        int audio_id = (int)SoundFXID.Passby1;

        if (singleton.sfxData[audio_id].sfxSource.isPlaying)
        {
            audio_id = (int)SoundFXID.Passby2;

            if (singleton.sfxData[audio_id].sfxSource.isPlaying)
            {
                return;
            }
        }

        if ((critter_info.cachedPosition-CameraManager.GetEyePosition()).magnitude > critter_info.audioData.passbyDistMax)
        {
            return;
        }

        if (critter_info.audioData.fishSize == FishAudioData.FishSizeCategory.Small)
        {
            singleton.passbySmallLastPlayTime = Time.time;
        }
        else if (critter_info.audioData.fishSize == FishAudioData.FishSizeCategory.Medium)
        {
            singleton.passbyMediumLastPlayTime = Time.time;
        }
        else
        {
            singleton.passbyLargeLastPlayTime = Time.time;
        }

        singleton.sfxData[audio_id].fade = 0f;           
        singleton.sfxData[audio_id].sfxTransform.position = critter_info.cachedPosition;
        singleton.sfxData[audio_id].attachTransform = critter_info.critterTransform;
        singleton.sfxData[audio_id].attachToOffset = Vector3.zero;
//        singleton.sfxData[audio_id].bFakeBinaural = true;
        singleton.sfxData[audio_id].sfxSource.maxDistance = critter_info.audioData.passbyDistMax;
        singleton.sfxData[audio_id].sfxSource.clip = critter_info.audioData.GetPassbyClip();
//        singleton.sfxData[audio_id].sfxSource.pan = Mathf.Clamp (Vector3.Dot ((singleton.sfxData[audio_id].sfxTransform.position - CameraManager.GetEyePosition()).normalized, CameraManager.GetCurrentCameraTransform().right) * singleton.FakeBinauralPanScale, -1f, 1f);
        singleton.sfxData[audio_id].sfxSource.Play();
    }

    public static void PlayCritterSwimAway(CritterInfo critter_info)
    {
        if (critter_info == null ||
            critter_info.audioData == null ||
            critter_info.audioData.swimAwayClip == null ||
            !critter_info.isVisible ||
            (Time.time - singleton.swimAwayLastPlayTime < singleton.swimAwayLimitTimeGlobal))
        {
            return;
        }

        if (Time.time - critter_info.audioData.swimAwayLastPlayTime < singleton.swimAwayLimitTimePerFish)
        {
            return;
        }

        int audio_id = (int)SoundFXID.Disperse1;
        
        if (singleton.sfxData[audio_id].sfxSource.isPlaying)
        {
            audio_id = (int)SoundFXID.Disperse2;
            
            if (singleton.sfxData[audio_id].sfxSource.isPlaying)
            {
                return;
            }
        }

        if ((critter_info.cachedPosition-CameraManager.GetEyePosition()).magnitude > critter_info.audioData.swimAwayDistMax)
        {
            return;
        }

        critter_info.audioData.swimAwayLastPlayTime = singleton.swimAwayLastPlayTime = Time.time;

        singleton.sfxData[audio_id].fade = 0f;           
        singleton.sfxData[audio_id].sfxTransform.position = critter_info.cachedPosition;
        singleton.sfxData[audio_id].attachTransform = critter_info.critterTransform;
        singleton.sfxData[audio_id].attachToOffset = Vector3.zero;
        singleton.sfxData[audio_id].bFakeBinaural = true;
        singleton.sfxData[audio_id].sfxSource.clip = critter_info.audioData.GetSwimAwayClip();    
        singleton.sfxData[audio_id].sfxSource.panStereo = Mathf.Clamp (Vector3.Dot ((singleton.sfxData[audio_id].sfxTransform.position - CameraManager.GetEyePosition()).normalized, CameraManager.GetCurrentCameraTransform().right) * singleton.FakeBinauralPanScale, -1f, 1f);
        singleton.sfxData[audio_id].sfxSource.maxDistance = critter_info.audioData.swimAwayDistMax;
//        singleton.sfxData[audio_id].sfxSource.PlayDelayed(0.1f);
    }

    public static void FadeInSFX(SoundFXID audio_id, float time)
    {
        if(singleton.sfxData[(int)audio_id].sfxSource != null )
        {
            singleton.sfxData[(int)audio_id].fade = singleton.sfxData[(int)audio_id].maxVolume/time;
            singleton.sfxData[(int)audio_id].sfxSource.volume = 0f;
        }
    }

    public static void FadeInSFXFromCurrent(SoundFXID audio_id, float fullTime)
    {
        if(singleton.sfxData[(int)audio_id].sfxSource != null )
        {
            float rate = singleton.sfxData[(int)audio_id].maxVolume/fullTime;
            float deltaVolume = singleton.sfxData[(int)audio_id].maxVolume - singleton.sfxData[(int)audio_id].sfxSource.volume;
            float time = deltaVolume/rate;
            singleton.sfxData[(int)audio_id].fade = 1.0f/time;
        }
    }

    public static void FadeOutSFX(SoundFXID audio_id, float time)
    {
        if(singleton.sfxData[(int)audio_id].sfxSource != null )
        {
            singleton.sfxData[(int)audio_id].fade = -singleton.sfxData[(int)audio_id].sfxSource.volume/time;
        }
    }

    public static void FadeOutAudioFromCurrent(SoundFXID audio_id, float fullTime)
    {
        if(singleton.sfxData[(int)audio_id].sfxSource != null )
        {
            float rate = singleton.sfxData[(int)audio_id].maxVolume/fullTime;
            singleton.sfxData[(int)audio_id].fade = -rate;
        }
    }

    public static void Stop(SoundFXID sfx)
    {
        int audio_id = (int) sfx;

        if( audio_id >= 0 
           && audio_id < singleton.sfxData.Count 
           && singleton.sfxData[audio_id].sfxSource != null ) 
        {
            singleton.sfxData[audio_id].fade = 0f;
            singleton.sfxData[audio_id].bFakeBinaural = false;
            singleton.sfxData[audio_id].attachTransform = null;
            singleton.sfxData[audio_id].sfxSource.Stop();
        }
    }
	
	public void FadeOutAllMusic() {
		FadeOut(WemoAudioTrackLogic.Ambient,1.5f);
	}
	
	bool IsCurrentSphereAmbientLocal() {
		if( CameraManager.IsInIntroCamMode() || CameraManager.IsInTravelCamera() ) {
			return false;
		}
		
		AudioClip clip = null;
		switch( currentAudioSphereID ) {
			case 1:
            default:
			{
				clip = clips[(int)WemoAudioTracks.Reef_Ambient];
				break;
			}
			case 2:
			{
				clip = clips[(int)WemoAudioTracks.Sandy_Ambient];
				break;
			}
			case 3:
			case 8:		
			{
				clip = clips[(int)WemoAudioTracks.Open_Ambient];
				break;
			}
			case 4:
			{
				clip = clips[(int)WemoAudioTracks.Cliff_Ambient];
				break;
			}
			case 5:
			{
				clip = clips[(int)WemoAudioTracks.MyGallery_Ambient];
				break;
			}
			case 6:
			{
				clip = clips[(int)WemoAudioTracks.Garden_Ambient];
				break;
			}
			case 7:
			{
				clip = clips[(int)WemoAudioTracks.DarkDepths_Ambient];
				break;
			}
            case 9:
            {
                clip = clips[(int)WemoAudioTracks.Seychelles_Ambient];
                break;
            }
            case 10:
            {
                clip = clips[(int)WemoAudioTracks.Kelp_Ambient];
                break;
            }
            case 15:
            {
                clip = clips[(int)WemoAudioTracks.BlueWhale_Ambient];
                break;
            }
            case 16:
            {
                clip = clips[(int)WemoAudioTracks.Prochloro_Ambient];
                break;
            }
            case 17:
            {
                clip = clips[(int)WemoAudioTracks.Iceberg_Ambient];
                break;
            }
		}
		return clip != null;
	}
	
	// TODO> gross and not scalable.
	public void SwitchAudioTracks() {

        return;

/*		int cur_sphere_id = App.SphereManager.currentSphere != null ? App.SphereManager.currentSphere.legacyid : 0;
        
//        Debug.Log("Switching to sphere audio " + cur_sphere_id);

		//if( currentAudioSphereID == cur_sphere_id && cur_sphere_id != prevSphereID) {
			//return;
		//}
		
//		if( currentAudioSphereID != -1 ) {
//			ClearAudioForLevel(currentAudioSphereID);
//		}

		currentAudioSphereID = cur_sphere_id;
		
		AudioClip ambient = null;
		AudioClip light = null;
		AudioClip deepwater = null;
		AudioClip predator = null;
		AudioClip reef = null;
		
		switch( cur_sphere_id ) {
			case 1:
            default:
			{
				ambient = clips[(int)WemoAudioTracks.Reef_Ambient];
				light = clips[(int)WemoAudioTracks.Reef_Light];
				deepwater = clips[(int)WemoAudioTracks.Reef_DeepWater];
				predator = clips[(int)WemoAudioTracks.Reef_Predator];
				reef = clips[(int)WemoAudioTracks.Reef_Reef];
				break;
			}
			case 2:
			{
				ambient = clips[(int)WemoAudioTracks.Sandy_Ambient];
				light = clips[(int)WemoAudioTracks.Sandy_Light];
				deepwater = clips[(int)WemoAudioTracks.Sandy_DeepWater];
				predator = clips[(int)WemoAudioTracks.Sandy_Predator];
				reef = clips[(int)WemoAudioTracks.Sandy_Reef];			
				break;
			}
			case 3:
			case 8:
			{
				ambient = clips[(int)WemoAudioTracks.Open_Ambient];
				light = clips[(int)WemoAudioTracks.Open_Light];
				deepwater = clips[(int)WemoAudioTracks.Open_DeepWater];
				predator = clips[(int)WemoAudioTracks.Open_Predator];
				reef = clips[(int)WemoAudioTracks.Open_Reef];
				break;
			}
			case 4:
			{
				ambient = clips[(int)WemoAudioTracks.Cliff_Ambient];
				light = clips[(int)WemoAudioTracks.Cliff_Light];
				deepwater = clips[(int)WemoAudioTracks.Cliff_DeepWater];
				predator = clips[(int)WemoAudioTracks.Cliff_Predator];
				reef = clips[(int)WemoAudioTracks.Cliff_Reef];
				break;
			}
			case 5:
			{
				ambient = clips[(int)WemoAudioTracks.MyGallery_Ambient];
				light = null;
				deepwater = null;
				predator = null;
				reef = null;
				break;
			}
			case 6:
			{
				ambient = clips[(int)WemoAudioTracks.Garden_Ambient];
				light = null;
				deepwater = null;
				predator = null;
				reef = null;
				break;
			}
			case 7:
			{
				ambient = clips[(int)WemoAudioTracks.DarkDepths_Ambient];
				light = null;
				deepwater = null;
				predator = null;
				reef = null;
				break;
			}
			case 9:
			{
				ambient = clips[(int)WemoAudioTracks.Seychelles_Ambient];
				light = null;
				deepwater = null;
				predator = null;
				reef = null;
				break;
			}
            case 10:
            {
                ambient = clips[(int)WemoAudioTracks.Kelp_Ambient];
                light = null;
                deepwater = null;
                predator = null;
                reef = null;
                break;
            }
            case 15:
            {
//            Debug.Log("WHALE AUDIO SET!");
                ambient = clips[(int)WemoAudioTracks.BlueWhale_Ambient];
                light = null;
                deepwater = null;
                predator = null;
                reef = null;
                break;
            }
            case 16:
            {
                ambient = clips[(int)WemoAudioTracks.Prochloro_Ambient];
                light = null;
                deepwater = null;
                predator = null;
                reef = null;
                break;
            }
            case 17:
            {
                ambient = clips[(int)WemoAudioTracks.Iceberg_Ambient];
                light = null;
                deepwater = null;
                predator = null;
                reef = null;
				break;
            }
		}

		// hook up all the tracks.
        ambientMusicSource.clip = ambient;
        ambientMusicSource.time = 0;
		
		deepWaterMusicSource.clip = deepwater;
        deepWaterMusicSource.time = 0;

		lightMusicSource.clip = light;
		lightMusicSource.time = 0;

		predatorMusicData.critterMusicSource.clip = predator;
		predatorMusicData.critterMusicSource.time = 0;

		reefMusicData.critterMusicSource.clip = reef;
		reefMusicData.critterMusicSource.time = 0;		
		
		FadeInSphereAudio();*/
	}
	
	// TODO> gross and not scalable.
	public void ClearAudioForLevel(int old_level) {
		switch( old_level ) {
			case 1:
            default:
			{
				clips[(int)WemoAudioTracks.Reef_Ambient] = null;
				UnloadAudioAssetBundle(WemoAudioTracks.Reef_Ambient);
				break;
			}
			case 2:
			{
				clips[(int)WemoAudioTracks.Sandy_Ambient] = null;
				UnloadAudioAssetBundle(WemoAudioTracks.Sandy_Ambient);
				break;
			}
			case 3:
			case 8:
			{
				clips[(int)WemoAudioTracks.Open_Ambient] = null;
				UnloadAudioAssetBundle(WemoAudioTracks.Open_Ambient);
				break;
			}
			case 4:
			{
				clips[(int)WemoAudioTracks.Cliff_Ambient] = null;
				UnloadAudioAssetBundle(WemoAudioTracks.Cliff_Ambient);
				break;
			}
			case 5:
			{
				clips[(int)WemoAudioTracks.MyGallery_Ambient] = null;
				UnloadAudioAssetBundle(WemoAudioTracks.MyGallery_Ambient);
				break;
			}
			case 6:
			{
				clips[(int)WemoAudioTracks.Garden_Ambient] = null;
				UnloadAudioAssetBundle(WemoAudioTracks.Garden_Ambient);
				break;
			}
			case 7:
			{
				clips[(int)WemoAudioTracks.DarkDepths_Ambient] = null;
				UnloadAudioAssetBundle(WemoAudioTracks.DarkDepths_Ambient);
				break;
			}
			case 9:
			{
				clips[(int)WemoAudioTracks.Seychelles_Ambient] = null;
				UnloadAudioAssetBundle(WemoAudioTracks.Seychelles_Ambient);
				break;
			}
            case 10:
            {
                clips[(int)WemoAudioTracks.Kelp_Ambient] = null;
                UnloadAudioAssetBundle(WemoAudioTracks.Kelp_Ambient);
                break;
            }
            case 15:
            {
                clips[(int)WemoAudioTracks.BlueWhale_Ambient] = null;
                UnloadAudioAssetBundle(WemoAudioTracks.BlueWhale_Ambient);
                break;
            }
            case 16:
            {
                clips[(int)WemoAudioTracks.Prochloro_Ambient] = null;
                UnloadAudioAssetBundle(WemoAudioTracks.Prochloro_Ambient);
                break;
            }
            case 17:
            {
                clips[(int)WemoAudioTracks.Iceberg_Ambient] = null;
                UnloadAudioAssetBundle(WemoAudioTracks.Iceberg_Ambient);
                break;
            }
		}
	}
	
	void UnloadAudioAssetBundle(WemoAudioTracks track) {
		if( bundles[(int)track] != null ) {
			bundles[(int)track].Unload(true);
			bundles[(int)track] = null;
		}
	}
	
	public bool IsAudioLoaded(WemoAudioTracks track) {
		return (bundles[(int)track] != null && clips[(int)track] != null );
	}
	
	public void FinishLoading(WemoAudioTracks track, WemoAudioTrackLogic track_logic, AssetBundle asset_bundle, AudioClip clip, bool assign_track_logic)
    {
        if (bundles[(int)track] != asset_bundle)
            bundles[(int)track] = asset_bundle;

        if (clips[(int)track] != clip)
            clips[(int)track] = clip;

        //      DebugDisplay.AddDebugText("finished loading music?");
        if( track_logic == WemoAudioTrackLogic.Ambient )
        {
            //          DebugDisplay.AddDebugText("finished loading ambient, fade out intro.");
            shouldFadeOutIntro = true;
        }

        if( !App.SphereManager.LEGACY_IsLoadingSphere() && assign_track_logic ) 
        {
            SetMusicClip(clip, track_logic);
        }
    }

    public void SetMusicClip(AudioClip clip, WemoAudioTrackLogic track_logic)
    {
        switch (track_logic)
        {
            case WemoAudioTrackLogic.Ambient:
                ambientMusicSource.clip = clip;
                break;
        }
    }

	public void PlayTrack(SoundFXID sfx, WemoAudioTrackLogic track_logic, float fadeIn, float crossFadeOutTime)
	{
		int audio_id = (int)sfx;
		SFXData sd = sfxData[audio_id];
		AudioClip clip = sd.sfxSource.clip;

		SetMusicClip(clip, track_logic);
		singleton.Fade(track_logic, fadeIn > 0f, fadeIn, crossFadeOutTime);
	}
     
    public AudioClip GetCurrentMusicClip(WemoAudioTrackLogic track_logic)
    {
        switch (track_logic)
        {
        case WemoAudioTrackLogic.Ambient:
            return ambientMusicSource.clip;
        }

        return null;
    }

    public void PlayTrack(AudioClip clip, WemoAudioTrackLogic track_logic, float fadeIn, float crossFadeOutTime)
    {
        SetMusicClip(clip, track_logic);
        singleton.Fade(track_logic, fadeIn > 0f, fadeIn, crossFadeOutTime);
    }

    public void DimTrack(WemoAudioTrackLogic track, float duration)
    {
        FadeVolumeData fd = fadeData[(int)track];
                
        // already dimmed)
        if (fd.dimReverseTimer > 0f)
        {
            fd.dimReverseTimer = Mathf.Max (fd.dimReverseTimer, duration);
        }
        else
        {
            fd.targetVolume = fd.maxVolume * DimMusicVolume;
            fd.volumeSpeed = -fd.targetVolume / Mathf.Min(0.5f, duration);
            fd.dimReverseTimer = duration;
        }
    }

    public bool IsPlaying(AudioClip clip, WemoAudioTrackLogic track_logic)
    {
        switch (track_logic)
        {
            case WemoAudioTrackLogic.Ambient:
                return ambientMusicSource.clip == clip && ambientMusicSource.isPlaying; 
        }

        return false;
    }

    public bool IsPlaying(SoundFXID sfx)
    {
        int audio_id = (int)sfx;
        
        if( audio_id >= 0 
           && audio_id < sfxData.Count 
           && sfxData[audio_id] != null 
           && sfxData[audio_id].sfxSource != null 
           && sfxData[audio_id].sfxSource.isPlaying)
        {
            return true;
        }
        
        return false;
    }
    
    public void PauseSFX( SoundFXID sfx, bool on )
    {
        int audio_id = (int)sfx;
        
        if( audio_id >= 0 
           && audio_id < sfxData.Count 
           && sfxData[audio_id] != null 
           && sfxData[audio_id].sfxSource != null )
        {
            if( on ) {
                sfxData[audio_id].sfxSource.Pause();
            }
            else {
                sfxData[audio_id].sfxSource.Play();
            }
        }        
    }
    
    void OnIntroFadedOut(FadeVolumeData data)
	{
		if( data.curVolume > 0f ) {
//            Debug.Log("Tried to end intro track!");
			return;
		}
		
//		Debug.Log("Ending the intro track");
	
		if( !swappedIntroClip ) {
            AudioClip new_clip = GetAudioClip(SoundFXID.TravelTransition);
            if( new_clip != null ) {
    			ambientMusicSource.clip = new_clip;
            }
			swappedIntroClip = true;
		}

		SwitchAudioTracks();
	}
	
	void OnAmbientFaded( FadeVolumeData data )
    {
        if (data.volumeSpeed < 0f)
        {
            ambientMusicSource.Stop();
        }
	}
	
	public void FadeInSphereAudio() {
		FadeIn(WemoAudioTrackLogic.Ambient);
		enableLogic_Ambient = true;
        shouldFadeOutIntro = true;
	}    	
	
	void CalculateFadeRatio(float dt, WemoAudioTrackLogic track, List<FadeVolumeData> fadeList)
	{
        FadeVolumeData fd = fadeList[(int)track];
		if (fd == null)
        {
            return;
        }

        if (fd.dimReverseTimer > 0f)
        {
            fd.dimReverseTimer -= dt;
            if (fd.dimReverseTimer <= 0f)
            {
                fd.volumeSpeed = -fd.volumeSpeed;
                fd.targetVolume = fd.maxVolume;
            }
        }

        if (fd.curVolume != fd.targetVolume)
		{
			fd.curVolume += fd.volumeSpeed * dt;

			if ((fd.volumeSpeed > 0 && fd.curVolume > fd.targetVolume) ||
                (fd.volumeSpeed < 0 && fd.curVolume < fd.targetVolume))
            {
				fd.curVolume = fd.targetVolume;
            }
			
			if (fd.volumeSpeed > 0) // Fade in
				fd.fadeRatio = fd.curVolume / fd.targetVolume; // --> 1
			else
				fd.fadeRatio = fd.curVolume; // --> 0

			if (fd.hasFinishedFading)
			{
                if (fd.onFadeEffectFinished != null)
				{
					fd.onFadeEffectFinished(fd);
				}
			}
		}
	}

    void Fade(WemoAudioTrackLogic track, bool fadeIn, float fadeInTime, float crossFadeOutTime)
    {
        AudioSource music = null;

//        float curVolume = 1.0f;
        float maxVolume = 1.0f;
        bool bSetToAmbientTime = false;

        switch (track)
        {
            case WemoAudioTrackLogic.Ambient:
                music = ambientMusicSource;
                maxVolume = maxAmbientVolume;
                break;
            case WemoAudioTrackLogic.Intro:
                music = ambientMusicSource;
                maxVolume = maxAmbientVolume;
                if (!ambientMusicSource.isPlaying)
				{
                    ambientMusicSource.time = 0f;
				}
                break;
            default:
                Debug.LogError("AudioManager - No handler for music track " + track);
                return;
        }

        if (!music.isPlaying || !fadeIn)
        {
            if (bSetToAmbientTime)
            {
                music.time = ambientMusicSource.time;
            }

            music.Play();
            fadeData[(int) track].curVolume = music.volume;
            fadeData[(int) track].maxVolume = maxVolume;
            fadeData[(int)track].dimReverseTimer = 0f;
        }
       
        if (fadeInTime <= 0)
        {
            fadeData[(int)track].targetVolume = fadeData[(int) track].curVolume = fadeData[(int)track].maxVolume;
            music.volume = fadeData[(int)track].targetVolume * globalMusicVolume;
            fadeData[(int)track].fadeRatio = 1f;
        }
        else 
        {
            if (fadeIn) // current --> Max
            {
                fadeData[(int)track].targetVolume = fadeData[(int)track].maxVolume;
                fadeData[(int)track].volumeSpeed = fadeData[(int)track].maxVolume / fadeInTime;
            }
            else // Fade out: current --> 0
            {
                fadeData[(int)track].targetVolume = 0.0f;
                fadeData[(int)track].volumeSpeed = -fadeData[(int)track].maxVolume / fadeInTime;
            }
        }

//        Debug.Log("Fading " + track + " from " + fadeData[(int)track].curVolume + " to " + fadeData[(int)track].targetVolume + " with speed of " + fadeData[(int)track].volumeSpeed + "\n FadeIn: " + fadeIn);
    }

    public static void FadeIn(WemoAudioTrackLogic track, float time)
    {
        singleton.Fade(track, true, time, 0f);
    }

    public static void FadeIn(WemoAudioTrackLogic track)
    {
        singleton.Fade(track, true, singleton.fadeTime, 0f);
    }

    public static void FadeOut(WemoAudioTrackLogic track, float time)
    {
        singleton.Fade(track, false, time, 0f);
    }

    public static void FadeOut(WemoAudioTrackLogic track)
    {
        singleton.Fade(track, false, singleton.fadeTime, 0f);
    }

    
    public static void CrossFadeOut(WemoAudioTrackLogic track, float crossFadeOutTime)
    {
        if (crossFadeOutTime > 0f)
        {
            singleton.fadeData[(int)track].targetVolume = 0.0f;
            singleton.fadeData[(int)track].dimReverseTimer = 0.0f;
            singleton.fadeData[(int)track].volumeSpeed = -singleton.fadeData[(int)track].maxVolume / crossFadeOutTime;
        }
    }
    
	public static void DetectSettings() {

		_musicOn = true;
		_soundFXOn = true;
        _narrationOn = true;

		int isset = PlayerPrefs.GetInt("music", 1);

		if (isset == 0)
			_musicOn = false;
		
		isset = PlayerPrefs.GetInt("soundfx", 1);

		if (isset == 0)
			_soundFXOn = false;

        isset = PlayerPrefs.GetInt("narration", 1);
        
        if (isset == 0)
            _narrationOn = false;

		//SetGlobalMute(!_musicOn);
		SetMusicMute(!_musicOn);
		SetSFXMute(!_soundFXOn);
	}

    List<FadeVolumeData> fadeData;
    List<FadeVolumeData> swapFadeData;

    delegate void OnFadeEffectFinished(FadeVolumeData data);
    class FadeVolumeData
    {
        public FadeVolumeData() : this(0, 0, 0, 0) { }
        public FadeVolumeData(float max, float target, float cur, float speed)
        {
            maxVolume = max;
            targetVolume = target;
            curVolume = cur;
            volumeSpeed = speed;
        }

        public float maxVolume;
        public float targetVolume;
        public float curVolume;
        public float volumeSpeed;
        public float fadeRatio;
        public float dimReverseTimer;

        public bool hasFinishedFading
        {
            get
            {
                return curVolume == targetVolume;
            }
        }

        public OnFadeEffectFinished onFadeEffectFinished;
    }
}

public enum WemoAudioTrackLogic : int
{
    None = 0,
    Intro = 1,
    Ambient = 2,
}

public enum WemoAudioTracks
{
	Intro,
	Reef_Ambient,
	Sandy_Ambient,
	Open_Ambient,
	Cliff_Ambient,
	MyGallery_Ambient,
	Garden_Ambient,
	DarkDepths_Ambient,
    Seychelles_Ambient,
    Prochloro_Ambient,
	Kelp_Ambient,
    BlueWhale_Ambient,
	Iceberg_Ambient,
}
