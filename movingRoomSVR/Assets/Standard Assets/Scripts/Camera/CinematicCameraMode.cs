using UnityEngine;
using System.Collections;

public delegate void CinematicCameraFinished( BaseCameraMode my_camera );

public class CinematicCameraMode : BaseCameraMode {
 
    public AnimationClip theClip;
    
    [HideInInspector]
    public float switchFadeTime = 1.5f;

    private int currentSphereID = 0;
    
    public BaseCameraMode endCamera;
    public CameraType endCameraType;
    
    CinematicCameraFinished finishedCallback;   
    
    public void SetFinishedCallback( CinematicCameraFinished callback )
    {
        finishedCallback = callback;
    }
    
    public override bool CameraModeIsLoadable()
    {
        if( theClip == null ) {
            return false;
        }
        
        if( gameObject.GetComponent<Animation>() == null ) {
            gameObject.AddComponent<Animation>();
        }
            
        return true;
    }
    

    void Awake()
    {
        myTransform = transform;
        cameraType = CameraType.CinematicCamera;
        runCollision = CameraCollisionType.None;
        
        finishedCallback = null;

        cameraName = "Cinematic Camera";
        if( theClip != null ) {
            cameraName += " :: " + theClip.name;
        }
    }
    
    public override void InitCameraMode()
    {
        if( inited ) {
            return;
        }
        
        base.InitCameraMode();
        currentSphereID = App.SphereManager.LEGACY_GetCurrentSphere();        
    }
    
    public override void StartCameraMode() 
    {
//        Debug.Log("Starting " + gameObject.name );
        runCollision = CameraCollisionType.None;
        myTransform.position = Vector3.zero;
        myTransform.rotation = Quaternion.identity;
        gameObject.GetComponent<Animation>()[theClip.name].normalizedTime = 0f;
        gameObject.GetComponent<Animation>().Play(theClip.name);        
    }
    
    public override void EndCameraMode() 
    {
//        Debug.Log("ENDING! " + gameObject.name );        
    }
    
    public override void UpdateCameraMode() 
    {
//        Debug.Log("cam pos : " + myTransform.position);
/*        Vector3 tmp = myTransform.position;
        tmp.x *= -1;
        myTransform.position = tmp;*/
    }

    void SwitchToEndCamera()
    {
        if( endCamera != null && endCamera.masterIndex >= 0 ) {
            CameraManager.JumpToCameraOrder( endCamera.masterIndex );
        }
        else if ( endCameraType != CameraType.None ) {
            CameraManager.SwitchToCamera( endCameraType );
        }
        else {
            CameraManager.JumpToCameraOrder( 0 );
        }
        
        // do this last because we may be setting a camera mode in here!
        if( finishedCallback != null ) {
            finishedCallback( this );
        }          
    }
 
    void FadeFinished( object arg )
    {
        SwitchToEndCamera();
        OculusCameraFadeManager.StartCameraFadeFromBlack( switchFadeTime, null, null );        
    }
    
    void ClipFinished()
    {
        // we are either transitioning to another sphere, or we have already transitioned.
        // do not trigger any post cinematic setup.
        if( App.SphereManager.LEGACY_GetCurrentSphere() != currentSphereID || App.SphereManager.LEGACY_IsLoadingSphere() ) {
            return;
        }
        
        float fade_time = ( theClip.length - GetComponent<Animation>()[theClip.name].time );
//        Debug.Log("FADE TIME = " + fade_time);
        if( fade_time > 0f ) {
            switchFadeTime = fade_time;
            OculusCameraFadeManager.StartCameraFadeToBlack( switchFadeTime, FadeFinished, null );
        }
        else {
            SwitchToEndCamera();
        }
    }    
}
