using UnityEngine;
using System.Collections;

using System.Runtime.InteropServices;   // required for DllImport

public delegate void GenericDelegate();

public class AppBase : MonoBehaviour {
	protected bool firstUpdate;
	
	public bool DEBUG_muteMusic = false;
	
	private static AppBase singleton;	
	
	public static bool doveIn = false;
	
    public static bool reloadActive = false;
    public static bool restartActive = false;

	public static bool inFocus = true;

    public bool usingBuiltData = false;

    public static AppBase Instance {
        get {
            return singleton;
        }
    }
	
	public virtual bool Initialized() {
		return false;
	}

	public virtual bool RunningAsPreview() {
		return false;
	}
		
	public bool FirstUpdateHappened() {
		return firstUpdate;
	}

	void Awake() {
		singleton = this;
	}

    public virtual void Start() {
        App.DataManager.Init();

        InputManager.InitInput();

        SphereInstance.Instance.Construct();
			
		SimManager.Construct();
	}
	
	void OnApplicationFocus( bool focus ) {
		inFocus = focus;
	}

    void onApplicationExitEvent( string adSpace )
    {
        App.MetricsManager.Stage("exit_habitat", App.SphereManager != null ? App.SphereManager.currentSphereName :  "", true);
        App.MetricsManager.Stage("session_playtime", Time.realtimeSinceStartup.ToString ("0.00"));
        App.MetricsManager.TrackStaged("exit");
    }

	public virtual void OnFirstDiveIn() 
	{ 
		doveIn = true; 
	}

    IEnumerator _delayedReload( float in_time ) {
        yield return new WaitForSeconds( in_time );

/*        if( AudioManager.Instance != null ) {
            AudioManager.Instance.MusicFullStop();
        }
        
        if( SimInstance.Instance != null ) {
            SimInstance.Instance.ForceRemoveCritters();
        }
        
        if( StoryManager.Instance != null ) {
            StoryManager.Instance.Reset();
        }
        
        if( CameraManager.singleton != null ) {
            CameraManager.singleton.RestartCurrentCamera();
        }
        
        OculusCameraFadeManager.Reset();
        
        yield return null;*/

        Application.LoadLevel(Application.loadedLevel);
        reloadActive = false;
    }
    
	public void DelayedReload( float in_time ) {
        reloadActive = true;
		StartCoroutine( _delayedReload( in_time ) );
    }    

    IEnumerator _delayedRestartApp( float in_time ) {
        yield return new WaitForSeconds( in_time );
        RestartApp();
    }

    public void DelayedRestartApp( float in_time ) {
        restartActive = true;
        StartCoroutine( _delayedRestartApp( in_time ) );
    }

	public virtual void RestartApp() {
        if( AudioManager.Instance != null ) {
            AudioManager.Instance.MusicFullStop();
        }
        
        if( SimInstance.Instance != null ) {
            SimInstance.Instance.ForceRemoveCritters();
        }

        OculusCameraFadeManager.Reset();

        if( CameraManager.singleton != null ) {
            CameraManager.singleton.RestartCurrentCamera();
        }

        if( StoryManager.Instance != null ) {
            StoryManager.Instance.Reset();
        }
            
        restartActive = false;
    }

	void Update() {}
}
