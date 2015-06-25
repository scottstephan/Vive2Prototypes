using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Prime31;

public class MetricsManager : MonoBehaviour {

	/*******************************************
	* CONSTS
	********************************************/
 //       private const string FLURRY_APP_KEY = "WWYV3TWYFXMCMQJPT8NR";   // BluVR : Development
        private const string FLURRY_APP_KEY = "FKTB9KGDZ4MWVC6DJ4PB";   // BluVR : Production
//    private const string FLURRY_APP_KEY = "FVYZKY4SSZZYDFPF6TS2";     // BluVR : Retail Demo 1

	
	/*******************************************
	* VARIABLES
	********************************************/

	// State
	[HideInInspector] public bool initialized = false;
    
	/*******************************************
	* UNITY METHODS
	********************************************/
	public void Awake () {
		
//		Debug.Log ("METRICS :::: MetricsManager Awake");
		// Set static singleton reference
		App.MetricsManager = this;
	}
	
	public void Start() {
		Init();

//      Debug.Log ("METRICS :::: MetricsManager Start");
	}

	public void OnApplicationPause( bool paused ) {
		if (!paused) {
//            Debug.Log ("Metrics :::: OnApplicationPause - UNPAUSED");
			Init ();
			return;
		}
//		Debug.Log ("Metrics :::: OnApplicationPause - PAUSED");
		Disable ();
	}

	public void OnApplicationFocus( bool resumed ) {
		if ( resumed ) {
//			Debug.Log ("Metrics :::: OnApplicationFocus - FOCUSED");
			Init ();
			return;
		}
//		Debug.Log ("Metrics :::: OnApplicationFocus - UNFOCUSED");
		Disable ();
	}

	public void OnDestroy () {
	}
	
	public void Update() {
	}

	/*******************************************
	* MAIN METHODS
	********************************************/
	private void Init() {
		if (App.MetricsManager.initialized)
			return;

		// Start Flurry tracking session
        #if UNITY_ANDROID
		FlurryAndroid.onStartSession (FLURRY_APP_KEY, false, true);
		FlurryAndroid.setLogEnabled( true );
        #endif
		App.MetricsManager.initialized = true;
	}

	private void Disable() {
		if (!App.MetricsManager.initialized)
			return;

		// Start Flurry tracking session
		#if UNITY_ANDROID
		FlurryAndroid.onEndSession();
		#endif
		App.MetricsManager.initialized = false;
	}

    Dictionary<string,string> dict = new Dictionary<string, string>();

    public void Stage(string key, string val, bool bClear = false)
    {
        if (bClear)
        {
            dict.Clear();
        }

        dict.Add(key, val);
    }

    public void TrackStaged(string eventName) 
    {
        Track(eventName, dict);
    }

    public void Track( string eventName, Dictionary<string,string> parameters=null ) {
        // Track without params
        if ( parameters == null ) {
            #if UNITY_ANDROID
            FlurryAndroid.logEvent( eventName, false );
            #endif
            //          Debug.Log( "METRICS: Tracking Event: " + eventName );
            return;
        }
        
        // Track with params
        #if UNITY_ANDROID
        FlurryAndroid.logEvent( eventName, parameters, false );     
        #endif  
        DebugEventWithParameters( eventName, parameters );
    }

	public void StartTimedEvent( string eventName, Dictionary<string,string> parameters=null ) {
		// Track without params
		if ( parameters == null ) {
            #if UNITY_ANDROID
			FlurryAndroid.logEvent( eventName, true );
            #endif
//			Debug.Log( "METRICS: Tracking TIMED Event: " + eventName );
			return;
		}
		
		// Track with params
        #if UNITY_ANDROID
		FlurryAndroid.logEvent( eventName, parameters, true );
        #endif
		DebugEventWithParameters( eventName, parameters, true );
	}

	public void EndTimedEvent( string eventName, Dictionary<string,string> parameters=null ) {
		// Track without params
		if ( parameters == null ) {
#if UNITY_ANDROID
			FlurryAndroid.endTimedEvent( eventName );
#endif
//            Debug.Log( "METRICS: ENDING TIMED Event: " + eventName );
			return;
		}
		
		// Track with params
        #if UNITY_ANDROID
		FlurryAndroid.endTimedEvent( eventName, parameters );
        #endif
        DebugEventWithParameters( eventName, parameters, true, true );
	}

	private void DebugEventWithParameters( string eventName, Dictionary<string,string> parameters, bool timed=false, bool ending=false ) {
		#if !UNITY_EDITOR
			return;
		#endif
		Debug.Log( "METRICS: " + ( timed ? "ENDING" : "Tracking" ) + " " + ( timed ? "TIMED" : "" ) + " Event With Parameters: " + eventName );		
		foreach( KeyValuePair<string,string> param in parameters ) {
			Debug.Log( "METRICS: Event (" + eventName + ") param: " + param.Key + ", value: " + param.Value );
		}
	}
}
