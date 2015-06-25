
using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public enum DebugTextDisplay {
    OFF,
    FPS,
    POSITION,
    FPS_POSITION,
    SCALE
}

public class DebugDisplay : MonoBehaviour 
{
	public static DebugDisplay singleton = null;
	public int DEBUG_maxFPS = 90;
	
	public GameObject fpsTextObject;
    private TextMesh fpsTextMesh = null;
    
    public GameObject lowFpsTextObject;
    private TextMesh lowFpsTextMesh = null;
    private Renderer lowFpsTextRenderer = null;

    public GameObject tempTextObject;
    private TextMesh tempTextMesh = null;

    public GameObject positionTextObject;
    private TextMesh positionTextMesh = null;
    
    public GameObject playerScaleTextObject;
    private TextMesh playerScaleTextMesh = null;

    private float lowFps = 30;
    private float lowFpsTimer = 0f;
    private float lowFpsTime = 1f;

    private int framesSinceSpike = 0;
    private int framesSinceSpikeSmoothed = 0;
    private int framesSinceSpikeAccumTotal = 0;
    static private int framesSinceSpikeMAparm = 50;

    private int [] spikeAccumulator = new int[framesSinceSpikeMAparm];
    private int spikeAccumulatorCount = 0;


    private float temperature = 0.0f;               // last temperature
    static private int   temperatureGetFrequency = 15;     // get the temp every N frames
    private int   temperatureFrameCount = 0;
    
	public GameObject buildNumberTextObject;
	
// Attach this to a GUIText to make a frames/second indicator.
//
// It calculates frames/second over each updateInterval,
// so the display does not keep changing wildly.
//
// It is also fairly accurate at very low FPS counts (<10).
// We do this not by simply counting frames per interval, but
// by accumulating FPS for each frame. This way we end up with
// correct overall FPS even if the interval renders something like
// 5.5 frames.

    public bool fpsOverride = false;
 
	public  float updateInterval = 0.1F;
 
	private float accum   = 0; // FPS accumulated over the interval
	private int   frames  = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval

	private float lowFPSMessageCoolDownTimer = 0f;
//	private float lowFPSMessageCoolDownTime = 1f;
	
	private bool paused = false;
    private DebugTextDisplay debugStep = DebugTextDisplay.OFF;

	private float lastTime;	
	
//unused	private static bool DEBUG_DISPLAY_OFF = false;
	//public bool LOG_THE_TEXT = false;

	public bool logFPSData;
	public float logFrequency = 1f;
	public int maxLogEntries = 0;

    bool firstToggle;

	float _lastFPSLog = 0f;
//	bool _firstWrite = true;
	string _logText;
	int _numEntries;

//	private bool requestedVersionFile = false;

	// be careful when recursive logging, yo
	private static Log log = Log.GetLog(typeof(DebugDisplay).FullName);

	public void UpdateBuildTextWithPlatform() {
        return;

/*		if( AppBase.Instance.RunningAsPreview() ) {
			return;
		}
		
		// first add mac vs pc suffix
#if UNITY_STANDALONE_OSX
		buildNumberTextObject.guiText.text = buildNumberTextObject.guiText.text + " OSX";
#endif

#if UNITY_STANDALONE_WIN
		buildNumberTextObject.guiText.text = buildNumberTextObject.guiText.text + " WIN";
#endif
        string serverName = RuntimeID.ServerName;
		buildNumberTextObject.guiText.text = serverName + " " + buildNumberTextObject.guiText.text;*/
	}
	
	public static void OutputBuildNumber() 
    {
        if (singleton == null)
        {
            return;
        }

		log.Trace(singleton.buildNumberTextObject.GetComponent<GUIText>().text);
	}
	
	public static bool DebugHUDActive() {
	    // use debugStep instead of hidden
        if (singleton != null && singleton.debugStep != DebugTextDisplay.OFF)
        {
            return true;
        }
        else
        {
            return false;
        }
		//return !singleton.hidden;
	}
	
	public void toggleHUD(bool shouldResetToInitState) {
        if (shouldResetToInitState) 
        {
            // this sets the build string on by default
            // debugStep = 1;

            // this sets the build string off by default
            debugStep = DebugTextDisplay.OFF;
        }
        else
        {
            if (!firstToggle)
            {
                firstToggle = true;
                lastTime = Time.time;
                StartCoroutine(UpdateFPS());           
            }

            switch ( debugStep ) {
            case DebugTextDisplay.OFF:
                debugStep = DebugTextDisplay.FPS;
                break;
            case DebugTextDisplay.FPS:
                debugStep = DebugTextDisplay.POSITION;
                break;
            case DebugTextDisplay.POSITION:
                debugStep = DebugTextDisplay.FPS_POSITION;
                break;
            case DebugTextDisplay.FPS_POSITION:
                debugStep = DebugTextDisplay.SCALE;
                break;
            case DebugTextDisplay.SCALE:
                debugStep = DebugTextDisplay.OFF;
                break;
            }
        }

        // let's first turn everything off
        fpsTextObject.SetActive( false );
        tempTextObject.SetActive( false );
        lowFpsTextObject.SetActive( false );
        playerScaleTextObject.SetActive( false );

        if( positionTextObject != null ) {
            positionTextObject.SetActive( debugStep == DebugTextDisplay.FPS_POSITION || debugStep == DebugTextDisplay.POSITION );
        }

        // now we'll just turn back on what we need
        if( debugStep == DebugTextDisplay.FPS || debugStep == DebugTextDisplay.FPS_POSITION ) 
        {
            fpsTextObject.SetActive( true );
            tempTextObject.SetActive( true );
            lowFpsTextObject.SetActive( true );
			StartLog(); //reset the log
        }
        else if (debugStep == DebugTextDisplay.SCALE)
        {
            //OculusScaler scaler = CameraManager.singleton.GetComponentInChildren<OculusScaler>();
            //playerScaleTextMesh.text = "Scale: " + scaler.Scale;
            //playerScaleTextObject.SetActive( true );
        }
		else if (logFPSData) {
			SaveLog();
		}			 
	}
	
	IEnumerator UpdateFPS()
	{
		while( true ) {
			float new_time = Time.time;
			float dt = new_time - lastTime;
			lastTime = Time.time;

			float incremental_fps = 1f/dt;
		    timeleft -= dt;
			accum += incremental_fps;
	    	++frames;

			// ee edit we don't have theBlueOcullusInit anymore
            //if (incremental_fps < TheBluOculusInit.Instance.DEBUG_maxFPS - 5) {
			if (incremental_fps < DEBUG_maxFPS - 5) {
					int spikeIndex = spikeAccumulatorCount % framesSinceSpikeMAparm;
                if (spikeAccumulatorCount >= framesSinceSpikeMAparm){
                    framesSinceSpikeAccumTotal -= spikeAccumulator[spikeIndex];
                }
                framesSinceSpikeAccumTotal += framesSinceSpike;
                spikeAccumulator[spikeIndex] = framesSinceSpike;
                spikeAccumulatorCount++;

                framesSinceSpike = 0;
                framesSinceSpikeSmoothed = framesSinceSpikeAccumTotal / Math.Min(spikeAccumulatorCount,framesSinceSpikeMAparm);
            }
            else {
                framesSinceSpike++;

            }
#if UNITY_ANDROID
            // get the temperature 
            if ((temperatureFrameCount % temperatureGetFrequency) == 0) {
                temperature = OVRDevice.GetBatteryTemperature();
                if (tempTextObject.activeSelf)
                {
                    tempTextMesh.text = System.String.Format("{0:F2} Deg {1} SSG {2}", temperature, framesSinceSpikeSmoothed, framesSinceSpike);
                }
                tempTextMesh.color = new Color(100, 0, 200);;
            }
            temperatureFrameCount++;
#endif
			//            if( incremental_fps < TheBluOculusInit.Instance.DEBUG_maxFPS || 
			if( incremental_fps < DEBUG_maxFPS || 
               incremental_fps < ((accum/frames) - 5))
            {
                if (incremental_fps < lowFps)
                {
                    lowFps = incremental_fps;
                    lowFpsTimer = lowFpsTime;
                }

				if (logFPSData)
                    AppendLog(incremental_fps, SphereManager.destinationName);
			}
            
            if( lowFpsTimer > 0f &&
                lowFpsTextObject != null &&
                fpsTextObject.activeSelf) 
            {
                lowFpsTimer -= dt;
                lowFpsTextRenderer.enabled = true;
                if (lowFpsTextObject.activeSelf)
                {
                    lowFpsTextMesh.text = System.String.Format("{0:F2} FPS", lowFps);
                }

				//if(lowFps < TheBluOculusInit.Instance.DEBUG_maxFPS)
				if(lowFps < DEBUG_maxFPS)
                    lowFpsTextMesh.color = Color.Lerp (Color.red, new Color(255, 153, 0), MathfExt.Fit(lowFps, 10f, DEBUG_maxFPS / 2.0f, 0f, 1f));
                else 
                    lowFpsTextMesh.color = Color.Lerp (new Color(255, 153, 0), Color.green, MathfExt.Fit(lowFps, DEBUG_maxFPS / 2.0f, DEBUG_maxFPS, 0f, 1f));

                if( lowFpsTimer <= 0f ) 
                {
                    lowFps = DEBUG_maxFPS;
                    lowFpsTextRenderer.enabled = false;
                }
            }
            
			// track low frame rate spikes.
			lowFPSMessageCoolDownTimer -= dt;
			
	    	// Interval ended - update GUI text and start new interval
	    	if( timeleft <= 0.0 )
	    	{
                if (!fpsOverride) {
    	        	// display two fractional digits (f2 format)
    	    		float fps = accum/frames;
                    if (fpsTextObject.activeSelf)
                    {
    	    		    string format = System.String.Format("{0:F2} FPS",fps);
                        fpsTextMesh.text = format;
                    }
    	
    	    		if(fps < 30)
                        fpsTextMesh.color = Color.yellow;
    	    		else if(fps < 10)
                        fpsTextMesh.color = Color.red;
    	       		else
                        fpsTextMesh.color = Color.green;
                }
	    //  DebugConsole.Log(format,level);
	       		timeleft = updateInterval;
	    	    accum = 0.0F;
	    	    frames = 0;
		    }
			yield return new WaitForEndOfFrame();
		}
		
	}

	void StartLog() {

		//write down the map name, date, time, etc.

		_numEntries = 0;
		_logText = "";

		//put in headers
		_logText += "Time,FPS,Note\n";
	
		string note = "Log Started: " + System.DateTime.Now.ToString() + " ";

		if (App.SphereManager.currentSphere != null) {
			note += (App.SphereManager.currentSphere._id);
		}

		AppendLog(0f, note, true);
	}

	void AppendLog(float fps, string note = "none", bool force = false) {

		if (note == null)
			note = "none";

		//if we are capping the log
		if (maxLogEntries > 0) {
			if (_numEntries > maxLogEntries)
				return;
		}

		note = "\"" + note + "\"";

		if (force || ((Time.time - _lastFPSLog) >= logFrequency)) {
			_logText += (Time.time.ToString("0.0") + "," + fps.ToString() + "," + note + "\n");
			_lastFPSLog = Time.time;
		}
	}

	void SaveLog() {

		string printDate = System.DateTime.Now.ToString();
		printDate = printDate.Replace("/", "-");

		string logFileName = "PerfLog_" + SystemInfo.deviceUniqueIdentifier + "_" + printDate + ".csv";

		PlayerInventory.WriteToFile(logFileName, _logText);
	}
	
	void DidPause( bool new_value ) {
		paused = new_value;
	}

	/* temporary refactoring of DebugDisplay:
	   one version never goes to WemoLog. Why? So that the logger can get to the HUD only.
	   NOTE: when all logging goes through Log.cs then this [ie: the call to
	         WemoLog.Log] can go away.
	   */
	private static void AddDebugTextAndMaybeLog( string hud_text, bool attempt_log ) {
/* attempt_log no longer supported
		if( !(DEBUG_DISPLAY_OFF || !InputManager.debugKeysActive) ) {
			if( attempt_log && singleton.LOG_THE_TEXT ) {
				WemoLog.Log(hud_text);
			}
		}
*/
	}
	
	public static void AddDebugTextHudOnly( string hud_text ) {
		AddDebugTextAndMaybeLog (hud_text, false);
	}

	public static void AddDebugText( string hud_text ) {
		Log.Main.Debug (hud_text);
	}

	void Awake() {
		singleton = this;
		
        if( fpsTextObject != null ) {
            fpsTextMesh = fpsTextObject.GetComponent<TextMesh>();
        }

        if( tempTextObject != null ) 
        {
            tempTextMesh = tempTextObject.GetComponent<TextMesh>();
            tempTextObject.GetComponent<Renderer>().enabled = true;
        }

        if( lowFpsTextObject != null ) {
            lowFpsTextMesh = lowFpsTextObject.GetComponent<TextMesh>();
            lowFpsTextRenderer = lowFpsTextObject.GetComponent<Renderer>();
            lowFpsTextObject.SetActive( false );
        }

        if( playerScaleTextObject != null ) {
            playerScaleTextMesh = playerScaleTextObject.GetComponent<TextMesh>();
            playerScaleTextObject.SetActive( false );
        }
        
        if( positionTextObject != null ) {
            positionTextMesh = positionTextObject.GetComponent<TextMesh>();
        }
        
        timeleft = updateInterval;

        fpsTextObject.SetActive( false );
        tempTextObject.SetActive( false );
        lowFpsTextObject.SetActive( false );
        playerScaleTextObject.SetActive( false );        
        if( positionTextObject != null ) {
            positionTextObject.SetActive( false );
        }

        toggleHUD(true);
	}
 
/* 	IEnumerator GetVersionFile() {
#if UNITY_EDITOR
		yield break;
#else

		// then put on the unique user string (for support tracking)
		buildNumberTextObject.guiText.text = buildNumberTextObject.guiText.text + " / " + RuntimeID.UserString;

		WWW versionRequest = new WWW(NetworkManager.apiBaseURL + "/version.txt");
		yield return versionRequest;
		if( versionRequest.text != null ) {
			string build_date_text = buildNumberTextObject.guiText.text;
			buildNumberTextObject.guiText.text = versionRequest.text + " " + build_date_text;
		}
		else {
			log.Warning("NO VERSION TXT FOUND!");
		}
#endif
	}*/
	
	void Start() {
		UpdateBuildTextWithPlatform();

		if (logFPSData)
			StartLog();
	}
	
	void Update()
	{
		if( paused ||
           AppBase.Instance == null ||
           !AppBase.Instance.FirstUpdateHappened() ) 
        {
			return;
		}
		
/*		if( !requestedVersionFile ) {
			StartCoroutine(GetVersionFile());
			requestedVersionFile = true;
		}*/
				
        if(Input.GetButtonDown("ToggleDebugHUD")){
			toggleHUD(false);
		}

		/*
		if(Input.GetKeyDown(KeyCode.JoystickButton2)){
			//Application.LoadLevel(0);
		}
		*/

		if(Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            OculusScaler scaler = CameraManager.singleton.GetComponentInChildren<OculusScaler>();

            if (scaler != null)
            {
                if (scaler.Scale < 10)
                {
                    scaler.Scale += 1;
                }
                else if (scaler.Scale < 40)
                {
                    scaler.Scale += 5;
                }
                else if (scaler.Scale < 100)
                {
                    scaler.Scale += 10;
                }
                else 
                {
                    scaler.Scale = 1;
                }

                scaler.ApplyScale();

                if (playerScaleTextMesh != null)
                {
                    playerScaleTextMesh.text = "Scale: " + scaler.Scale;
                }
            }
		}        		


        if( debugStep == DebugTextDisplay.FPS_POSITION || debugStep == DebugTextDisplay.POSITION ) {
            positionTextMesh.text = CameraManager.GetCurrentCameraPosition().ToString();
        }
    }
	
    public static TextMesh getFPSGuiText()
    {
        if( singleton.debugStep == DebugTextDisplay.FPS || singleton.debugStep == DebugTextDisplay.FPS_POSITION )
        {
            return singleton.fpsTextMesh;
        }
        else
        {
            return null;
        } 
    }

	void OnApplicationPause(bool pauseStatus) {
	
		if (!logFPSData)
			return;

		if (pauseStatus)
			SaveLog();

	}

    void OnApplicationQuit() {
		
		if (!logFPSData)
			return;

		SaveLog();
	}


}
 
