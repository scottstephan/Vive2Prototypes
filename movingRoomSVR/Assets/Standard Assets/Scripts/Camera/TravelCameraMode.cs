using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void TravelFinishedDelegate();

public class TravelCameraMode : BaseCameraMode
{
	private static GameObject introOcean;
	private static TravelCameraMode singleton;
    public static TravelCameraMode Singleton { 
        get {
            return singleton; 
        } 
    }
    
	Vector3 _travelDir;

	private Vector3 introLocation = new Vector3( 0f, -75f, -20000f );

	private GameObject introGodRayObject;
	private Vector3 introRotationEuler = new Vector3( -6f, 90f, 0f );
	private float speed = 150f;
	private float travelTime = 0f;
	private float minTravelTime = 0f;
	private bool fadeTriggered = false;
 
    const float MIN_TRAVEL_TIME_NORMAL = 6f;
    const float MIN_TRAVEL_TIME_BYPASS = 0.3f;

    private bool reloadingSameSphere = false;
	
    public bool bypassTravel = false;

	public TravelFinishedDelegate TravelFinished;
    
    public void ReloadingSameSphere()
    {
        reloadingSameSphere = true;
    }
    
    void Awake()
    {
		singleton = this;
        cameraType = CameraType.TravelCamera;
        myTransform = transform;
        cameraName = "Travel Camera";
		introOcean = GameObject.Find("ocean_intro");
    }
		
	public override void InitCameraMode(){
        if( inited ) {
            return;
        }
        
        base.InitCameraMode();        
        
		fadeTriggered = false;
		myTransform.position = introLocation;
		myTransform.rotation = Quaternion.Euler(introRotationEuler);		
    }
	
    void FadeUpFinished(object arg) {
        FloatingMenuManager.SetPlayerForward(_travelDir);
        FloatingMenuManager.ShowTravelTextOverlay(SphereManager.destinationName, _travelDir);
        TutorialManager.StartTutorial();
    }

	public override void StartCameraMode()
    {		
        if (introOcean == null)
        {
            // try to find intro ocean again in case it was loaded after CameraManager
            introOcean = GameObject.Find("ocean_intro");
        }

        if (bypassTravel)
        {
            myTransform.position = CameraManager.singleton.OVRCameraParent.position;
            myTransform.rotation = CameraManager.singleton.OVRCameraParent.rotation;    

            minTravelTime = MIN_TRAVEL_TIME_BYPASS;
        }
        else
        {
            myTransform.position = introLocation;
            myTransform.rotation = Quaternion.Euler(introRotationEuler);    
            CameraManager.singleton.SetYRotation( introRotationEuler.y );

            minTravelTime = MIN_TRAVEL_TIME_NORMAL;
            OculusCameraFadeManager.StartCameraFadeFromBlack( SphereManager.TRAVEL_FADE_TIME, FadeUpFinished, null );   
            _travelDir = myTransform.forward;
            _travelDir.y = 0f;
            _travelDir.Normalize();
            _travelDir *= speed;
            
            CameraManager.singleton.ovrPhysicsMove = true;
            CameraManager.singleton.ovrRB.isKinematic = false;
            CameraManager.singleton.CollisionEnable(true);
            CameraManager.UpdateCurrentCameraTransform();
            OVRDevice.ResetOrientation();
        }

		fadeTriggered = false;
		travelTime = 0f;
		speed = 150f;

        if( !reloadingSameSphere ) {
			SphereInstance.Instance.DestroyLevelObject();
        }
		CameraManager.SetIntroGodRayObjectActive( true );		

		if (introOcean)
			introOcean.SetActive(true);

		TravelFadeFinished();

		OculusFPSCameraMode.disableMovement = true;

        GlobalOceanShaderAdjust.UseCamPlaneHiRes(false);
		CameraManager.singleton.AttachDefaultPlankton();
	}

	void TravelFadeFinished() 
    {
        if (!bypassTravel)
        {
    		//reposition the camera so that it's in the travel map area
    		Vector3 newPos = CameraManager.singleton.OVRCameraParent.position;
    		newPos.x += 25000f;
    		newPos.z += 25000f;
    		newPos.y = 100f;

    		if (introOcean) 
            {
    			Vector3 oceanPos = introOcean.transform.position;
    			oceanPos.x = newPos.x;
    			oceanPos.z = newPos.z;
    		
    			newPos.y = (oceanPos.y - 500f/*710f*/);

    			introOcean.transform.position = oceanPos;
    		}

		    CameraManager.singleton.OVRCameraParent.position = newPos;
        }

        GlobalOceanShaderAdjust.ResetToOG();
	}
	
    public override void EndCameraMode() {
        reloadingSameSphere = false;

        if (!bypassTravel)
        {
            CameraManager.singleton.ovrPhysicsMove = false;
            CameraManager.singleton.ovrRB.isKinematic = true;
            CameraManager.singleton.CollisionEnable(false);
        }
    }

	void InstantiateFadeFinished(object arg) {

		OculusFPSCameraMode.disableMovement = false;
	}
	
	void ArrivalFadeFinished( object arg ) {

        //hide overlay graphics
        FloatingMenuManager.HideMenu(false);
        FloatingMenuManager.HideLoading();

		SphereManager.InstantiateLoadedAssets(PostInstantiate); //load while the screen is faded

		AudioManager.DetectSettings();
	}

	public void PostInstantiate() {

//		Debug.Log("arrrivalfadefinished");

//		int sphere_id = App.SphereManager.LEGACY_GetCurrentSphere();

		GlobalOceanShaderAdjust.ResetToLevel();

		if (introOcean)
			introOcean.SetActive(false);

		//if (introOcean)
		//	introOcean.renderer.enabled = false;
		
		CameraManager.SetIntroGodRayObjectActive( false );
//		OceanSphereController.StaticCreateLevelObject();
        if( !reloadingSameSphere ) {
    		CameraManager.StaticPostLoadSetup( true );
        }
		SphereInstance.PostSphereLoadSetup( true );

		CameraManager.SwitchToCamera( CameraType.OculusCamera );
  
		//FloatingMenuManager.HideMenu(false); //Why must this be called???
		FloatingMenuManager.HideText();

		if (TravelFinished != null)
			TravelFinished();
				
        	
        App.MetricsManager.Stage("habitat_load_name", App.SphereManager.currentSphereName, true);
        App.MetricsManager.TrackStaged("habitat_load_finish");

		SphereManager.habitatPlayStart = Time.time;
   
        Shader.WarmupAllShaders();

        OculusCameraFadeManager.StartCameraFadeFromBlack( SphereManager.TRAVEL_FADE_TIME+1.8f, InstantiateFadeFinished, null);
    }	

    public override void UpdateCameraMode()
    {
		//Debug.Log("TravelCamera UPDATE!");

        float dt = Time.deltaTime;
		runCollision = CameraCollisionType.None;
		
		bool ok_to_end = !App.SphereManager.LEGACY_IsLoadingSphere();
		
		travelTime += dt;
		if( !fadeTriggered 
		   && travelTime > minTravelTime 
		   && ok_to_end )
        { 
			fadeTriggered = true;

			//if (OculusCameraFadeManager.IsFaded())
			//	ArrivalFadeFinished(null);
			//else
            if (bypassTravel)
            {
                ArrivalFadeFinished(null);
            }
            else
            {
                OculusCameraFadeManager.StartCameraFadeToBlack( SphereManager.TRAVEL_FADE_TIME, ArrivalFadeFinished, null );
            }
		}

		//Vector3 amt = Vector3.forward * speed;
		Vector3 amt = _travelDir;

        OculusFPSCameraMode.singleton.UpdateMovementSound( amt.sqrMagnitude );

        if (!bypassTravel)
        {
            CameraManager.singleton.ovrRB.velocity = amt;
        }
	}
}
