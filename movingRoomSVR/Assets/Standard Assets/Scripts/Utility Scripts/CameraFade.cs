using UnityEngine;
using System.Collections;

public delegate void FadeFinishedFunc( object arg );

// simple fading script
// A texture is stretched over the entire screen. The color of the pixel is set each frame until it reaches its target color.
public class CameraFade : MonoBehaviour {
	private GUIStyle m_BackgroundStyle = new GUIStyle();        // Style for background tiling
	private Texture2D m_FadeTexture;                // 1x1 pixel texture used for fading
	private Color m_OriginalScreenOverlayColor = new Color( 0, 0, 0, 0f ); // default starting color: black and fully transparrent
	private Color m_CurrentScreenOverlayColor = new Color( 0, 0, 0, 0f ); // default starting color: black and fully transparrent
	private Color m_TargetScreenOverlayColor = new Color( 0, 0, 0, 0f );  // default target color: black and fully opaque
	private Color m_DeltaColor = new Color( 0, 0, 0, 0 );        // the delta-color is basically the "speed / second" at which the current color should change
	//private int m_FadeGUIDepth = -1000;    // make sure this texture is drawn on top of everything
	private int m_FadeGUIDepth = 2;    // make sure this texture is drawn on top of everything


	private float fadeTime = 0f;
	private float fadeTimeTotal = 0f;

	private FadeFinishedFunc fadeFinishedFunc = null;
	private object fadeFinishedFuncArg = null;

	public UISprite black;
	public bool isCameraFading = false;

	// initialize the texture, background-style and initial color:
	private void Awake() {
		m_FadeTexture = new Texture2D( 1, 1 );
		m_BackgroundStyle.normal.background = m_FadeTexture;
		SetScreenOverlayColor( m_CurrentScreenOverlayColor );

		// TEMP:
		// usage: use "SetScreenOverlayColor" to set the initial color, then use "StartFade" to set the desired color & fade duration and start the fade
		//SetScreenOverlayColor(new Color(0,0,0,1));
		//StartFade(new Color(1,0,0,1), 5);
		transform.position = new Vector3( transform.position.x, transform.position.y, 2 );
	}

	public void SetBackgroundTransparent() {
		m_OriginalScreenOverlayColor = new Color( 0, 0, 0, 0 );
		m_CurrentScreenOverlayColor = new Color( 0, 0, 0, 0 );
		m_TargetScreenOverlayColor = new Color( 0, 0, 0, 0 );
		SetScreenOverlayColor( m_CurrentScreenOverlayColor );
		fadeTime = 0f;
		fadeTimeTotal = 0f;
	}

	//	void OnGUI() {
	//		
	//		//DebugDisplay.AddDebugText("CAMERA FADE ON_GUI");
	//		
	//		DoOnGUI();
	//		
	//	}
	public bool IsCameraFadeActive() {
		return (fadeTime > 0f);
	}

	// draw the texture and perform the fade:
	public void DoOnGUI() {
		// if the current color of the screen is not equal to the desired color: keep fading!
		if ( fadeTime > 0f ) {
			fadeTime -= Time.deltaTime;
			if ( fadeTime < 0f ) {
				fadeTime = 0f;
			}
			float ratio = 1.0f - (fadeTime / fadeTimeTotal);

			SetScreenOverlayColor( m_OriginalScreenOverlayColor + m_DeltaColor * ratio );
		} else if ( fadeFinishedFunc != null ) {
			isCameraFading = false;
			FadeFinishedFunc tmp = fadeFinishedFunc;
			fadeFinishedFunc = null;
			tmp( fadeFinishedFuncArg );
			fadeFinishedFuncArg = null;
		}
		black.alpha = m_CurrentScreenOverlayColor.a;

		// only draw the texture when the alpha value is greater than 0:
		if ( m_CurrentScreenOverlayColor.a > 0 ) {
			GUI.depth = m_FadeGUIDepth;
			// adjust fade area
			GUI.Label(new Rect(CameraManager.ScreenLeft - 10, CameraManager.ScreenVOffset - 10, CameraManager.ScreenWidth + 10 + 1, CameraManager.ScreenHeight + 10 + 1), m_FadeTexture, m_BackgroundStyle);
		}
	}


	// instantly set the current color of the screen-texture to "newScreenOverlayColor"
	// can be usefull if you want to start a scene fully black and then fade to opague
	public void SetScreenOverlayColor( Color newScreenOverlayColor ) {
		m_CurrentScreenOverlayColor = newScreenOverlayColor;
		//DebugDisplay.AddDebugText("SET FULL SCREEN TO ALPHA OF  = " + m_CurrentScreenOverlayColor.a);
		m_FadeTexture.SetPixel( 0, 0, m_CurrentScreenOverlayColor );
		m_FadeTexture.Apply();
	}


	// initiate a fade from the current screen color (set using "SetScreenOverlayColor") towards "newScreenOverlayColor" taking "fadeDuration" seconds
	public void StartFade( Color newScreenOverlayColor, float fadeDuration, FadeFinishedFunc finishedFunc, object arg ) {
		if ( fadeFinishedFunc != null ) {
			return;
		}

		if ( fadeDuration <= 0.0f )      // can't have a fade last -2455.05 seconds!
		{
			SetScreenOverlayColor( newScreenOverlayColor );
		} else                    // initiate the fade: set the target-color and the delta-color
        {
			fadeTime = fadeDuration;
			fadeTimeTotal = fadeDuration;
			m_OriginalScreenOverlayColor = m_CurrentScreenOverlayColor;
			m_TargetScreenOverlayColor = newScreenOverlayColor;
			m_DeltaColor = m_TargetScreenOverlayColor - m_OriginalScreenOverlayColor;
		}

		isCameraFading = true;
		fadeFinishedFunc = finishedFunc;
		fadeFinishedFuncArg = arg;
	}
}
