using UnityEngine;
using System.Collections;
using System;

// simple fading script using OVRMainMenu to draw over both eyes
public class OculusCameraFadeManager : MonoBehaviour {
	static OculusCameraFadeManager singleton;
	private Color m_OriginalScreenOverlayColor = new Color(0f, 0f, 0f, 0f); // default starting color: black and fully transparrent
	private static Color m_CurrentScreenOverlayColor = new Color(0f, 0f, 0f, 0f); // default starting color: black and fully transparrent
	private Color m_TargetScreenOverlayColor = new Color(0f, 0f, 0f, 0f);  // default target color: black and fully opaque
	private Color m_DeltaColor = new Color(0, 0, 0, 0);        // the delta-color is basically the "speed / second" at which the current color should change
	
	private float fadeTime = 0f;
	private float fadeTimeTotal = 0f;
	
	public float    FadeInTime      = 2.0f;
	public Texture  FadeInTexture   = null;
	public static Color OverlayColorOverride = new Color(0f, 0f, 0f, 0f);
	public float    startDelayTime   = 3.0f;
	private float    startDelayTimer  = 3.0f;
	
	private FadeFinishedFunc fadeFinishedFunc = null;
	private object fadeFinishedFuncArg = null;
	
	[HideInInspector]
	public bool isCameraFading = false;
	
	public GameObject faderMesh = null;
	
    static public Color MoonlightBlack = new Color(0.0f, 0.0f, 0.0f, 1f); // this color is recommend for black for the samsung devices
	static public Color MoonlightBlue = new Color(0.47f, 0.78f, 1f, 1f); // this color is used for loading screen tests

	// initialize the texture, background-style and initial color:
	private void Awake()
	{
		singleton = this;
		startDelayTimer = startDelayTime;
        SetScreenOverlayColor(MoonlightBlack);
		
		#if (!UNITY_ANDROID || UNITY_EDITOR)
		/*
		if (faderMesh)
			faderMesh.SetActive(false);
		
		faderMesh = null; //dont use this on other platforms
		*/
		#endif
	}
	
	public void SetBackgroundTransparent()
	{
		//Debug.Log ("SetBackgroundTransparent : Fade Manager : " );
		m_OriginalScreenOverlayColor = new Color(0f, 0f, 0f, 0f);
		m_CurrentScreenOverlayColor = new Color(0f, 0f, 0f, 0f);
		m_TargetScreenOverlayColor = new Color(0f, 0f, 0f, 0f);
		m_DeltaColor = new Color(0f, 0f, 0f, 0f);        // the delta-color is basically the "speed / second" at which the current color should change
		SetScreenOverlayColor(m_CurrentScreenOverlayColor);
		fadeTime = 0f;
		fadeTimeTotal = 0f;
		
		if (faderMesh) {
			//faderMesh.renderer.material.color = new Color(0f, 0f, 0f, 0f);
			faderMesh.SetActive(false);
		}
	}
	
	static public void Reset()
	{
		//Debug.Log ("OculusCameraFadeManager : Reset : " );

		singleton.startDelayTimer = singleton.startDelayTime;
        singleton.SetScreenOverlayColor(MoonlightBlack);
		singleton.StartFade(Color.clear, singleton.FadeInTime, null, null);
	}
	
	public void DoOnGUI()
	{
	}
	
/*	void OnGUI() {
        // Important to keep from skipping render events
		if (Event.current.type != EventType.Repaint)
			return;
		
		if (faderMesh)
			return;
	
		// Fade in screen
		
		// only draw the texture when the alpha value is greater than 0:
		if (m_CurrentScreenOverlayColor.a > 0)
		{
			GUI.color = m_CurrentScreenOverlayColor;
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), FadeInTexture);
		}
	}*/
	
	public bool IsCameraFadeActive()
	{
		return (fadeTime > 0f);
	}
	
	// draw the texture and perform the fade:
	public void Update()
	{
		// if the current color of the screen is not equal to the desired color: keep fading!
		float dt = Time.deltaTime;
		if( startDelayTimer > 0f ) 
		{
			startDelayTimer -= dt;
			if (startDelayTimer <= 0f)
			{
				StartFade(Color.clear, FadeInTime, null, null);       
			}
		}
		else if (fadeTime > 0f)
		{
			fadeTime -= Time.deltaTime;
			if (fadeTime < 0f)
			{
				fadeTime = 0f;
			}
			float ratio = 1.0f - (fadeTime / fadeTimeTotal);
			
			SetScreenOverlayColor(m_OriginalScreenOverlayColor + m_DeltaColor * ratio);
		} else if ( isCameraFading )
		{
			//Debug.Log ("Used to be - SetBackgroundTransparent : Fade Manager : FADE OVER" );
			//Debug.Log ("SetBackgroundTransparent : Fade Manager : " );
			isCameraFading = false;
			//SetScreenOverlayColor(Color.clear);

//			Debug.Log("Stopped fading...");

			if (m_TargetScreenOverlayColor.a >= 1f)
            {
                CameraManager.singleton.RenderPlankton(false);
                MemoryManager.RunGarbageCollection();
            }
            else
            {
                CameraManager.singleton.RenderPlankton(true);               
            }

			if( fadeFinishedFunc != null ) 
			{
				FadeFinishedFunc tmp = fadeFinishedFunc;
				fadeFinishedFunc = null;
				tmp(fadeFinishedFuncArg);
				fadeFinishedFuncArg = null;
			}
		}
		
		//Debug.Log ("update : Fade Manager : " + m_CurrentScreenOverlayColor );
	}
	
	
	// instantly set the current color of the screen-texture to "newScreenOverlayColor"
	// can be usefull if you want to start a scene fully black and then fade to opague
	public void SetScreenOverlayColor(Color newScreenOverlayColor)
	{
		//Debug.Log("SET SCREEN OVERLAY COLOR: " + newScreenOverlayColor.ToString());

		if (m_CurrentScreenOverlayColor == newScreenOverlayColor)
			return;

		m_CurrentScreenOverlayColor = newScreenOverlayColor;
		
		if (faderMesh) {
			faderMesh.GetComponent<Renderer>().material.color = m_CurrentScreenOverlayColor;
			
			if (newScreenOverlayColor.a <= 0)
				faderMesh.SetActive(false);
			else {
				faderMesh.SetActive(true);
			}
		}
	}

    public static void StartCameraFadeToColor(Color c, float fade_time, FadeFinishedFunc finished_func, object arg)
    {      
        if (singleton == null) return;

        singleton.StartFade(c, fade_time, finished_func, arg);    
    }

	public static void StartCameraFadeToBlue(float fade_time, FadeFinishedFunc finished_func, object arg)
	{    
		//Debug.Log("START CAMERA FADE TO BLUE");
		if (singleton == null) return;
		
		singleton.StartFade(MoonlightBlue, fade_time, finished_func, arg);	
	}

	public static void StartCameraFadeToBlack(float fade_time, FadeFinishedFunc finished_func, object arg)
    {    
		//Debug.Log("START CAMERA FADE TO BLACK");
        if (singleton == null) return;

        singleton.StartFade(MoonlightBlack, fade_time, finished_func, arg);	
	}
	
	public static void StartCameraFadeFromBlack(float fade_time, FadeFinishedFunc finished_func, object arg)
    {      
        if (singleton == null) return;

		singleton.StartFade(Color.clear, fade_time, finished_func, arg);       
	}

    // initiate a fade from the current screen color (set using "SetScreenOverlayColor") towards "newScreenOverlayColor" taking "fadeDuration" seconds
    static public void SetColor(Color newScreenOverlayColor)
    {
        if (singleton == null) return;

		singleton.SetScreenOverlayColor(newScreenOverlayColor); //why??
        //singleton.StartFade(newScreenOverlayColor, 0f, null, null);
    }

	// initiate a fade from the current screen color (set using "SetScreenOverlayColor") towards "newScreenOverlayColor" taking "fadeDuration" seconds
	public void StartFade(Color newScreenOverlayColor, float fadeDuration, FadeFinishedFunc finishedFunc, object arg)
	{
		if (fadeFinishedFunc != null)
			return;
		
//		Debug.Log ("Starting Fade : " + newScreenOverlayColor + "FD: " + fadeDuration );
		if (fadeDuration <= 0.0f)      // can't have a fade last -2455.05 seconds!
		{
//			Debug.Log ("SETTING IT" );
			SetScreenOverlayColor(newScreenOverlayColor);
		}
		else                    // initiate the fade: set the target-color and the delta-color
		{
//			Debug.Log ("Starting it legit" );
			fadeTime = fadeDuration;
			fadeTimeTotal = fadeDuration;
			m_OriginalScreenOverlayColor = m_CurrentScreenOverlayColor;
			m_TargetScreenOverlayColor = newScreenOverlayColor;
			m_DeltaColor = m_TargetScreenOverlayColor - m_OriginalScreenOverlayColor;
			SetScreenOverlayColor(m_CurrentScreenOverlayColor);
		}
		
		isCameraFading = true;
		fadeFinishedFunc = finishedFunc;
		fadeFinishedFuncArg = arg; 
	}

	static public void FadeToTransparent(float time)
	{
		if (singleton == null) return;
		
		singleton.StartFade(new Color(0f,0f,0f,0f), time, null, null);
	}

	static public void FadeToBlack(float time)
	{
		if (singleton == null) return;

		//Debug.Log("OculusCameraFade -- FadeToBlack");
        singleton.StartFade(MoonlightBlack, time, null, null);
	}
	
	static public bool IsFading()
	{
		if (singleton == null) return false;
		
		return singleton.IsCameraFadeActive();
	}

	static public bool IsFaded() {

		if (m_CurrentScreenOverlayColor.a >= 1f && !singleton.isCameraFading)
			return true;
		
		return false;

	}
}
