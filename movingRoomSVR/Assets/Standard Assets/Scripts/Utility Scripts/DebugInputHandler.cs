using UnityEngine;
using System.Collections;

public class DebugInputHandler : MonoBehaviour {

	public static bool environmentDeleteObjectActive = false;

	// bool to toggle on/off staticMesh for fish (no deformation)
	public bool critterStaticMesh = false;

    public static bool DO_AVOIDANCE_DEBUG_OUTPUT = false;
	public static bool DO_GOD_RAYS = true;
    public static bool DO_NEW_RANDOM_TARGETING = false;
	public static bool FORCE_LOD0_OFF = false;

    void Awake() {
	}

	// Use this for initialization
	void Start () {
#if UNITY_EDITOR
        InputManager.debugKeysActive = true;
#endif
	}

	// Update is called once per frame
	void Update () {

		if( !InputManager.debugKeysActive )
		{
            return;
        }

        //Menubar info

        if(InputManager.GetKeyDown(",")) {
            FORCE_LOD0_OFF = !FORCE_LOD0_OFF;
            DebugDisplay.AddDebugText("FORCE_LOD0_OFF : " + FORCE_LOD0_OFF);
        }

        if (InputManager.GetKeyDown("[")){
            AdjustFOV (-5);
        }
        if (InputManager.GetKeyDown("]")){
            AdjustFOV (5);
        }

/* More involved than these calls unfortunately - CameraManager currently assumes OVRCameraController is primary,
 * need to make switchable w/MainCamera
 * 
        if (InputManager.GetKeyDown("c")){
            if (CameraManager.IsInOculusMode())
            {
                CameraManager.SwitchToCamera(CameraType.FPSCamera);
            }
            else
            {
                CameraManager.SwitchToCamera(CameraType.OculusCamera);
            }
        }
*/
/*        if( InputManager.GetKeyDown("t" ) ) {
            environmentDeleteObjectActive = !environmentDeleteObjectActive;
            DebugDisplay.AddDebugText("Environment Delete Object is :: " + environmentDeleteObjectActive );
            if( !environmentDeleteObjectActive ) {
                SphereInstance.Instance.ResetEnvironmentDeletedObjects();
            }
        }
        if( environmentDeleteObjectActive ) {
            if( InputManager.GetKeyDown("r") ) {
                SphereInstance.Instance.ResetEnvironmentDeletedObjects();
            }
        }*/
		

//        if( InputManager.GetKeyDown("v" ) ) {
//            CameraManager.SwitchToCamera(CameraType.CinematicCamera);
//		}
        //EXTERNAL HOTKEYS
        //"space" == change camera
        //"p" == pause

        //Testing Event Stream	
        if (InputManager.GetKeyDown("m")){
			//GUIFacebookConnectPrompt.StartDrawingFacebookConnectPrompt();
        }
		
        if (InputManager.GetKeyDown("f")){
			//MessageManager.FacebookPostPromptGeneric("fishpurchased",30);
			//MessageManager.FacebookPostPromptGeneric("fishpurchased",30);
			//MessageManager.SpherePurchased(1);
			//MessageManager.FacebookPostPromptGeneric("giftsent", 30);
			//GUIStateManager.OpenFacebookConnectPanel( true );			
        }
		if(InputManager.GetKeyDown("q"))
		{
			//GUIFacebookConnectPrompt.Singleton.FacebookMergeSuccess();	
		}


/*        if(InputManager.GetKeyDown("q"))
        {
            OceanSphereController.Instance.aaMode++;
            if( OceanSphereController.Instance.aaMode > 3 )
            {
                OceanSphereController.Instance.aaMode = 0;
            }

            //WemoLog.Eyal("aaMode " + aaMode);
            switch( OceanSphereController.Instance.aaMode )
            {
                case 0: QualitySettings.antiAliasing = 0;
                        DebugDisplay.AddDebugText("antiAliasing = 0");
                        break;
                case 1: QualitySettings.antiAliasing = 2;
                        DebugDisplay.AddDebugText("antiAliasing = 2");

                        break;
                case 2: QualitySettings.antiAliasing = 4;
                        DebugDisplay.AddDebugText("antiAliasing = 4");

                        break;
                case 3: QualitySettings.antiAliasing = 8;
                        DebugDisplay.AddDebugText("antiAliasing = 8");
                        break;
            }

        }*/

/*        if(InputManager.GetKeyDown("o"))
        {
            OceanSphereController.Instance.profOn = !OceanSphereController.Instance.profOn;
            DebugDisplay.AddDebugText("start profiling " + OceanSphereController.Instance.profOn);
//            Profiler.logFile = "profile.log";
//            Profiler.enabled = OceanSphereController.Instance.profOn;
        }*/

/*        if(InputManager.GetKeyDown("n"))
        {
            critterStaticMesh = !critterStaticMesh;
            for(int i=0; i < OceanSphereController.GetCrittersInPopulation(); i++){
                CritterInfo critter_info = OceanSphereController.Instance.critters[i];
                LODModelData.SetStaticMesh(critter_info,critterStaticMesh);
            }
        }*/

/*        if(InputManager.GetKeyDown("b"))
        {
            critterStaticMesh = !critterStaticMesh;
            for(int i=0; i < OceanSphereController.GetCrittersInPopulation(); i++){
                CritterInfo critter_info = OceanSphereController.Instance.critters[i];
                critter_info.critterLODData.flashMaterial(critter_info);
            }
        }*/
		
        // update all fish.
//		if( InputManager.GetKeyDown("f") ) {
//			DebugDisplay.AddDebugText("updating " + crittersInPopulation + " critters!");
//		}

/*        if (InputManager.GetKeyDown("l")){
            DebugDisplay.AddDebugText("UI TRANSPARENCY DATA");
            GUIGraffitiManager.DisplayTransDebugInfo();			
        }

        if(InputManager.GetKeyDown("j")) {
            DO_AVOIDANCE_DEBUG_OUTPUT = !DO_AVOIDANCE_DEBUG_OUTPUT;
            DebugDisplay.AddDebugText("DO_AVOIDANCE_DEBUG_OUTPUT : " + DO_AVOIDANCE_DEBUG_OUTPUT);
        }
        if(InputManager.GetKeyDown("g")) {
            DO_GOD_RAYS = !DO_GOD_RAYS;
//            GameObject.Find("godrays").SetActive( DO_GOD_RAYS );
//            DebugDisplay.AddDebugText("DO_GOD_RAYS : " + DO_GOD_RAYS);
        }
        if(InputManager.GetKeyDown("k")) {
            DO_NEW_RANDOM_TARGETING = !DO_NEW_RANDOM_TARGETING;
            DebugDisplay.AddDebugText("DO_NEW_RANDOM_TARGETING : " + DO_NEW_RANDOM_TARGETING);
        }

#if UNITY_EDITOR
		if (InputManager.GetKeyDown("l")) {
			//NotificationManager.CreateClickSharedRoamingNotification("bmillz", "notification-connection-profileimg.png");
			GlowScreen gs = GameObject.Find("PlaneGlow").GetComponent<GlowScreen>();
			if(gs.isOn) gs.SetOff();
			else gs.SetOn();
		}
#endif
*/

// travel to spheres

        string levelStr = null;
        if (InputManager.GetKeyDown("1"))
        {
            levelStr = "open-water-shark";
        }
        else if (InputManager.GetKeyDown("2"))
        {
            levelStr = "coral-garden";
        }
        else if (InputManager.GetKeyDown("2"))
        {
			levelStr = "scripps-kelp";
        }

        if (levelStr != null)
        {
			App.SphereManager.TravelToSphere( levelStr );
        }
    }

    static void AdjustFOV(float f)
    {
        GameObject camCtrlObj = GameObject.Find ("OVRCameraController");
        
        OVRCameraController camCtrl = camCtrlObj != null ? camCtrlObj.GetComponent<OVRCameraController>() : null;
        
        if (camCtrl == null)
        {
            return;
        }
        
        float fov = 0.0f;
        camCtrl.GetVerticalFOV(ref fov);
        
        fov = Mathf.Clamp (fov+f, 60, 180);
        camCtrl.SetVerticalFOV(fov);
        
        Debug.Log("FOV Vertical = " + fov);
    }
}
