using UnityEngine;
using System.Collections;

public enum LODStyle{
	Default = 0,
	S4 = 1
}
[System.Serializable]
public class LODStyleData{
	public LODStyle style;
	public Shader shader;
	public Shader fishShader;
	public Shader fishFlashShader;
}

public class GlobalOceanShaderAdjust : MonoBehaviour {
	public static GlobalOceanShaderAdjust Instance;

	static GlobalOceanShaderAdjust ogInstance;
	static GlobalOceanShaderAdjust levelInstance;	
	static GlobalOceanShaderAdjust instance;

	static Color _oldBGColor;

	static int _oldLMask;
	static int _oldRMask;
    static int _menuLMask;
    static int _menuRMask;

	public LODStyle lodStyle;
	public LODStyleData[] lodStyleData;
	public bool forceLODShaders = false;
	public bool unityFog = false;
	public float dist = 1000;
	public float shallowPoint = 0;
	public float deepestPoint = 2600;
	public float deepestPointBrightness = 0;
	public Color deepColor = new Color(0.003f,0.01f,0.125f,0f); // 24M is deep
	public Color middleColor = new Color(0.015f,0.2f,0.4f,0f); 
	public Color shallowColor = new Color(0.03f,0.386f,0.8f,0f);

    public Color darkClickColor = new Color( 0.01f, 0.01f, 0.01f, 1f );
    [HideInInspector]
    public Color deepLerpedColor;
    [HideInInspector]
    public Color middleLerpedColor;
    [HideInInspector]
    public Color shallowLerpedColor;
    [HideInInspector]
    public Color keyLightLerpedColor;
    [HideInInspector]
    public bool useDarkClickColor = false;
    [HideInInspector]
    public float darkLerpTime = 0.25f;
    [HideInInspector]
    public float darkLerpTimer = -1f;


    [HideInInspector]
    public float clickDist = 250f;
    [HideInInspector]
    public float distLerped = 250f;

//    [HideInInspector]
//    private Color mainTextureColor = new Color( 1f, 1f, 1f, 1f );
    [HideInInspector]
    private Color mainTextureLerpedColor;

    public float depthPower = 1.0f;
    public float depthClickPower = 10.0f;
    [HideInInspector]
    public float depthLerpedPower = 1.0f;
    public float fishDepthPower = 1.0f;
	public float depthStartVal = 0.1f;
	public Vector4 keyLightDir = new Vector4(0f,1f,0f,1f);
	public Color keyLightColor = new Color(1f,1f,1f,1f);
	public float keyLightIntensity = 1.5f;
	public Color ambLightColor = new Color(0.1f,0.1f,0.1f,1f);
	public float ambLightMult = 2.5f;
	public float causticsTileMult = 0.003f;
	public float causticsMult = 0.5f;
	public float causticsSpeed = 9.6f;
	public float fishCausticMult = 3.0f;
	public Transform lightTransform = null;		
	public Transform oceanTransform = null;
	
    public float frameMaxAlpha = 0.25f;

	public float glow_glowIntensity = 1.6f;
	public int glow_blurIterations = 5;
	public float glow_blurSpread = 1f;
	public Color glow_glowTint = new Color(1f,1f,1f,0f);
	
	
	private float newDist;
	private float lerpTime;
	private float lerpTimeLeft;

	float camYA;
	float camYB;
	
	static GameObject[] camPlanes;
    static GameObject[] camPlanesHiRes;
    static bool bUseCamPlanesHiRes;

	//this is for the menu darkening--HACK??

	static bool _oldFog;
	static Color _oldFogColor;
	static FogMode _oldFogMode;
	static float _oldFogStartDistance;
	static float _oldFogEndDistance;
	
    public bool useDomeLockDepth = false;

	// class log
//	private static Log log = Log.GetLog(typeof(GlobalOceanShaderAdjust).FullName);

	void Awake(){
		Instance = this;
        if (_oldLMask == 0)
        {
            _oldLMask = CameraManager.singleton.leftCamera.cullingMask;
			if (CameraManager.singleton.rightCamera != null)
	            _oldRMask = CameraManager.singleton.rightCamera.cullingMask;
        
            int menuMask = 0;
            menuMask |= (1 << LayerMask.NameToLayer("VRUI"));
            menuMask |= (1 << LayerMask.NameToLayer("VRCursor"));
            menuMask |= (1 << LayerMask.NameToLayer("Water"));
            menuMask |= (1 << LayerMask.NameToLayer("VideoBox"));
        
            _menuLMask = menuMask | (1 << LayerMask.NameToLayer("cameraLeftEye"));
            _menuRMask = menuMask | (1 << LayerMask.NameToLayer("cameraRightEye"));
        }

		if (oceanTransform == null)
			oceanTransform = new GameObject ().transform;
	}

    public void DoDarkLerp() {
        darkLerpTimer -= Time.deltaTime;
        float rat = darkLerpTimer / darkLerpTime;

        if( rat < 0f ) {
            rat = 0f;
        }

        if( useDarkClickColor ) {
            keyLightLerpedColor = Color.Lerp( darkClickColor, keyLightColor, rat );
        }
        else {
            keyLightLerpedColor = Color.Lerp( keyLightColor, darkClickColor, rat );
        }

//        Shader.SetGlobalColor("_DeepColor", deepLerpedColor );
//        Shader.SetGlobalColor("_MiddleColor", middleLerpedColor );
//        Shader.SetGlobalColor("_ShallowColor", shallowLerpedColor );
//        Shader.SetGlobalFloat("_DepthPower", depthLerpedPower );
//        Shader.SetGlobalFloat("_DepthMax", distLerped );
        Shader.SetGlobalColor("_keyLightContrib", keyLightIntensity * keyLightLerpedColor);
    }

	public void AdjustToParams(bool do_planes) {
//        if (CameraManager.singleton.fxFrame != null)
//        {
//            CameraManager.singleton.fxFrame.maxAlpha = frameMaxAlpha;
//        }

        Shader.SetGlobalFloat("_DepthMax",dist );
		Shader.SetGlobalFloat("_ShallowPoint",shallowPoint);
		Shader.SetGlobalFloat("_DeepestPoint",deepestPoint);
		Shader.SetGlobalFloat("_DeepestPointBrightness",deepestPointBrightness);
		Shader.SetGlobalColor("_DeepColor",deepColor);
		Shader.SetGlobalColor("_MiddleColor",middleColor);
		Shader.SetGlobalColor("_ShallowColor",shallowColor);
        Shader.SetGlobalColor("_ShallowColorMinusDeep",shallowColor-deepColor);
        Shader.SetGlobalFloat("_ShallowOverDeepestPoint",shallowPoint/deepestPoint);
        Shader.SetGlobalFloat("_DepthPower",depthPower);
		Shader.SetGlobalFloat("_fishDepthPower",fishDepthPower);
		Shader.SetGlobalFloat("_DepthStartVal",depthStartVal);	
		Vector3 keyLightDirNorm = new Vector3(keyLightDir.x,keyLightDir.y,keyLightDir.z).normalized;
		if(lightTransform != null) 
		{
			//keyLightDirNorm = lightTransform.forward;
			Shader.SetGlobalMatrix("_lightMatrix", lightTransform.localToWorldMatrix);
			Shader.SetGlobalVector("_lightDir",lightTransform.forward);
       	}
		else
			Shader.SetGlobalVector("_lightDir",keyLightDirNorm);
		Shader.SetGlobalColor("_keyLightColor",keyLightColor);
		Shader.SetGlobalFloat("_keyLightIntensity",keyLightIntensity);
		Shader.SetGlobalFloat("_keyLightMult",keyLightIntensity);
		Shader.SetGlobalColor("_ambLightColor",ambLightColor);
        Shader.SetGlobalColor("_keyLightContrib", keyLightIntensity * keyLightColor);
        Shader.SetGlobalColor("_ambLightContrib", ambLightColor * ambLightMult);
		Shader.SetGlobalFloat("_causticsTileMult",causticsTileMult);
		Shader.SetGlobalFloat("_causticsMult",causticsMult);
		Shader.SetGlobalFloat("_CausticSpeed",10f*causticsSpeed);
		Shader.SetGlobalFloat("_CausticMult",fishCausticMult);
		Shader.SetGlobalFloat("_ambLightMult",ambLightMult);

		if( do_planes ) 
        {
            UpdatePlanes(dist, useDomeLockDepth);

			CameraManager.singleton.farClipPlane = dist + 250f;
		}

		//XCamera.farClipPlane = dist + 100;
		
		
		//Glow adjust
		/*log.Debug("#############################     glow adjust        ###############################");
		log.Debug(Camera.main.gameObject.name + " glowIntensity " + glow_glowIntensity + " blurIterations " + glow_blurIterations + " blurSpread " + glow_blurSpread + " glowTint " + glow_glowTint);
		GlowEffect ge = Camera.main.gameObject.GetComponent<GlowEffect>();
		ge.glowIntensity = glow_glowIntensity;
		ge.blurIterations = glow_blurIterations;
		ge.blurSpread = glow_blurSpread;
		ge.glowTint = glow_glowTint;
		XCamera.SetGlowEffect(glow_glowIntensity, glow_blurIterations, glow_blurSpread, glow_glowTint );
		*/

//		Debug.Log("GLOBAL SHADER ADJUST CALLED: " + shallowColor.ToString() + " : " + this.gameObject.name);
		RenderSettings.fogDensity = 1f;
		RenderSettings.fogEndDistance = dist;// - 500;
        RenderSettings.fogStartDistance = depthStartVal;
		RenderSettings.fogColor = shallowColor;
//		Debug.LogError("fog color " + shallowColor + " " + shallowColor.r * 256 + " " + shallowColor.g * 256 + " " + shallowColor.b * 256);
	}

    static void UpdatePlanes(float d, bool domeLockDepth )
    {
		CameraManager.singleton.dome.localScale = Vector3.one * d ;
        CameraManager.singleton.domePointConstraint.useLockDepth = domeLockDepth;

        float mult = d/1500;

        GameObject[] planes = bUseCamPlanesHiRes ? camPlanesHiRes : camPlanes;

        for( int i = 0; i < planes.Length; i++ )
        {
            GameObject camPlane = planes[i];
			// temporarily disable all plans to test dome
			camPlane.SetActive(false);

			if (camPlane == null)
				continue;

            float parentScale = camPlane.transform.parent.lossyScale.z;
			//camPlane.transform.localPosition = new Vector3(0, 0, ( ( d - 0.5f ) / parentScale) );
			camPlane.transform.localPosition = new Vector3(0, 0, ( ( d + 8.5f ) / parentScale) );
            //camPlane.transform.localScale = new Vector3(720*mult,180*mult,180*mult);
            // ee overide  --  added * 0.2f  since I'm trying a much bigger mesh for the planes (ocean25*25) instead of (ocean 5*5)
            camPlane.transform.localScale = new Vector3(900 * mult / parentScale, 900 * mult / parentScale, 900 * mult / parentScale) * 0.2f;
            //sp.transform.localScale = Vector3(mult,mult,mult);
            //var spMesh : Mesh = sp.GetComponent(MeshFilter).mesh;
        }
    }

    public static void ResetToOG() {
        
        /*
		Debug.Log("OG INSTANCE IS... " + ogInstance.gameObject.name);
		Debug.Log("OTHER INSTANCE IS... " + instance.gameObject.name);

		if( instance != ogInstance ) {

			Debug.Log("SHADERADJUST: Reset to OG");

			GameObject.DestroyImmediate(instance);
			instance = ogInstance;
			instance.AdjustToParams( true );
		}
		*/

		if (ogInstance == null)
			return;

//        Debug.LogError("Reset to OG INSTANCE: " + ogInstance.gameObject.name + " : " + ogInstance.shallowColor.ToString());

		ogInstance.AdjustToParams(true);
	}
	
	public static void ResetToLevel() {

//		Debug.LogError("SHADERADJUST: Reset to level.");

		if( levelInstance != null) {
			// added sun
			GameObject sun = GameObject.Find("Sun");
			if( sun != null ) {
                levelInstance.lightTransform = sun.transform;
			}
			else {
                levelInstance.lightTransform = null;
			}
			
            levelInstance.AdjustToParams( true );

			RenderSettings.fog = true; //fog is ALWAYS ON PEOPLE!??
		}
	}

	/*
	 * Ok the flags aren't getting stomped by ME--I commented this out and
	 * it still stomps out the righteye flags on the right camera after a scene load.  
	 * I think the new oculus SDK might somehow duplicate the flags on each camera?
	 * I can't find any reference to the cullingmask in any code at all, so I have no
	 * idea where this is happening--I think maybe because there is an oculus camera 
	 * in both scenes there's some problem..??
	 */

	/*
	public static void RightEyeMaskHack(bool set = false) {

		_oldRMask &= ~(1 << LayerMask.NameToLayer("cameraLeftEye"));
		_oldRMask |= (1 << LayerMask.NameToLayer("cameraRightEye"));

		if (set)
			CameraManager.singleton.rightCamera.cullingMask = _oldRMask;
	}
	*/

	public static void RestoreMasks() {

		//return;

		//RightEyeMaskHack();

		CameraManager.singleton.leftCamera.cullingMask = _oldLMask;
		CameraManager.singleton.rightCamera.cullingMask = _oldRMask;

		//Debug.Log("RESTORE MASKS??? " + _oldLMask + " , " + _oldRMask);
	}

	public static void SetFadeMask() {

		if (!CameraManager.singleton)
			return;
		
		if (!CameraManager.singleton.HasVR)
			return;
		
		//this is only here because the fade to black doesn't work.
#if UNITY_ANDROID
		//CameraManager.singleton.ovrController.CullingMask = 0;
#endif
	}

	public static void ClearFadeMask() {

		ClearMenuMask();
	}

	public static void SetMenuMask() {

		if (!CameraManager.singleton)
			return;

		if (!CameraManager.singleton.HasVR)
			return;


        CameraManager.singleton.leftCamera.cullingMask = _menuLMask;
        CameraManager.singleton.rightCamera.cullingMask = _menuRMask;


        CameraManager.singleton.domePointConstraint.useLockDepth = false;
    }

	public static void ClearMenuMask() {

		if (!CameraManager.singleton)
			return;
		
		if (!CameraManager.singleton.HasVR)
			return;

		RestoreMasks();

        CameraManager.singleton.domePointConstraint.useLockDepth = instance.useDomeLockDepth;
    }
	
	public static void StaticAdjustToParams( bool do_planes ) {
		instance.AdjustToParams( do_planes );
	}
	
	public static float CurrentDistance() {
		return instance.dist;
	}

	public static void SetDeepColor( Color new_color ) {
		instance.deepColor = new_color; 
	}

	public static void SetShallowColor( Color new_color ) {
		instance.shallowColor = new_color; 
	}


    static public void UseCamPlaneHiRes(bool bHiRes)
    {
        for (int i=0; i < camPlanes.Length; ++i)
        {
            camPlanes[i].SetActive(!bHiRes);
        }

        for (int i=0; i < camPlanesHiRes.Length; ++i)
        {
            camPlanesHiRes[i].SetActive(bHiRes);
        }

        bUseCamPlanesHiRes = bHiRes;

        if (levelInstance != null)
        {
            UpdatePlanes(levelInstance.dist, levelInstance.useDomeLockDepth);
        }
        else if (ogInstance != null)
        {
            UpdatePlanes(ogInstance.dist, ogInstance.useDomeLockDepth);
        }
    }

	public static void SetDist( float new_dist ) 
    {
        if (instance == null)
        {
            return;
        }

		instance.dist = new_dist;
		instance.newDist = new_dist;
			
		instance.lerpTimeLeft = 0f;
		instance.AdjustToParams(true);
	}
	
	public static void ExpandSphere( float amt, float tm ) {
		instance.lerpTime = tm;
		instance.lerpTimeLeft = tm;
		instance.newDist = instance.dist + amt;
//		Debug.Log("						debug Adam: ExpandSphere - Amt=" + amt + " Tm=" + tm);
	}

    public static void LerpSphere( float newDist, float tm ) {
        instance.lerpTime = tm;
        instance.lerpTimeLeft = tm;
        instance.newDist = newDist;
//        Debug.Log("                     debug Adam: LerpSphere - Amt=" + amt + " Tm=" + tm);
    }

	public Color updateWater( float camY ) {
		float tint =  Mathf.Clamp(1+camY/deepestPoint,0,1);
//		Debug.Log("changed: "+camY);
		float fogRamp = Mathf.Clamp(-1+tint*2,0,1);
		
		Color waterColor = Color.Lerp(deepColor,middleColor,2 * tint);
		waterColor = Color.Lerp(waterColor,shallowColor,2 * Mathf.Pow(fogRamp,2));
		
		return waterColor;
		//RenderSettings.fogColor = waterColor;
		//RenderSettings.fog = true;
	}
		
	void Start() {
		bool first_time = false;
		camYB = 10000;
        if (ogInstance == null) 
		{
//            Debug.LogError("GlobalShader OG Set. " + gameObject.name);
			first_time = true;
			instance = this;
			ogInstance = this;
		}
		else
		{
//            Debug.LogError("GlobalShader Level Set. " + gameObject.name);
            levelInstance = this;
            instance = this;
			
			// refresh instance. Ensure the latest instance should be from streaming data.
/*			if( instance != ogInstance )
			{
                Debug.LogError("GlobalShader Destroyed . " + instance.name);
                GameObject.DestroyImmediate(instance);
				instance = this;
			}*/
		}

		newDist = dist;

        if (camPlanes == null)
        {
    		camPlanes = GameObject.FindGameObjectsWithTag("CamPlane");
        }

        if (camPlanesHiRes == null)
        {
            camPlanesHiRes = GameObject.FindGameObjectsWithTag("CamPlaneHiRes");
        }

        UseCamPlaneHiRes(true);

		//ASK EYAL ABOUT THIS--the fog and ocean top are not being set to shallow color

        if(AppBase.Instance == null || AppBase.Instance.RunningAsPreview())
		{
			AdjustToParams( true );
		}
		else
		{
			if( first_time && !CameraManager.IsInTravelCamera() ) {
//                Debug.LogError("START ADJUST PARAMS. " + gameObject.name);
                AdjustToParams( true );
			}
		}

		//THIS IS GETTING CALLED!
		if (unityFog){

//            Debug.LogError("UNITY FOG ADJUST PARAMS .  FALSE!! " + gameObject.name);
            AdjustToParams(false);

			//Debug.Log("****** change fog color " + shallowColor * 256);
			RenderSettings.fogMode = FogMode.Linear;
			//RenderSettings.fogColor = updateWater(CameraManager.GetCurrentCameraPosition().y);
			RenderSettings.fogColor = shallowColor;
			RenderSettings.fog = true;
		}
	}



    public void SetClickDarkEffect( bool on ) {
        useDarkClickColor = on;
        if( darkLerpTimer < 0f ) {
            darkLerpTimer = darkLerpTime;
        }
        else {
            darkLerpTimer = darkLerpTime - darkLerpTimer; // inverse the time so the lerp doesnt pop.
        }

    }

	void Update() {
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			Application.LoadLevel(Application.loadedLevel); // reload
		}

		if (Input.GetKeyDown (KeyCode.F1)) 
		{
			int lvl = Application.loadedLevel-1;
			if (lvl < 0)
				lvl = Application.levelCount-1;

			Application.LoadLevel(lvl);
		}

		if (Input.GetKeyDown (KeyCode.F2)) 
		{
			int lvl = Application.loadedLevel+1;
			if (lvl >= Application.levelCount)
				lvl = 0;
			
			Application.LoadLevel(lvl);
		}

		if(Input.GetKeyDown(KeyCode.S)){
			if(Time.timeScale == 1f)
			   Time.timeScale = 4f;
			else
				Time.timeScale = 1f;
		}

		camYA = CameraManager.GetCurrentCameraPosition().y;
		float d = Mathf.Abs(camYA-camYB);
		
		//
		if (unityFog && (d>1.5) && lodStyle == LODStyle.Default){
			RenderSettings.fogColor = updateWater(camYA);
//			Debug.Log("changing again " + RenderSettings.fogColor);
		}
		
		camYB = camYA;
		
	
		/*if (Input.GetKey(KeyCode.E))
		{
			Debug.Log("						debug Adam: dist = " + dist);
			//Debug.Log("						debug Adam: New camPlane scale is " + camPlane.transform.localScale);
			
			// make the cam plane wider and closer to fix slipping in the distance.
			float mult = dist/1500;
			foreach( GameObject camPlane in camPlanes )
			{
				camPlane.transform.localPosition = new Vector3(0,0,dist);
				camPlane.transform.localScale = new Vector3(1000*mult,550*mult,100*mult);
			}
		}*/
		
		if( dist != newDist)
        {
            lerpTimeLeft -= Time.deltaTime;

            if (lerpTime > 0)
            {
    			float ratio = 1.0f - (lerpTimeLeft/lerpTime);
    			dist = Mathf.Lerp(dist,newDist,ratio);
            }
            else
            {
                dist = newDist;
            }

//            Debug.LogError("DIST CHANGED AdjustParams! " + gameObject.name);
            AdjustToParams( true );
		}

		//WHAT IS THIS? RAB
		/*
        if( Input.GetKeyDown( KeyCode.G ) || Input.GetKeyDown(KeyCode.JoystickButton3 ) ) {
            SetClickDarkEffect( !useDarkClickColor );
        }
        */

        if( darkLerpTimer >= 0f ) {
            DoDarkLerp();
        }
	}
}
