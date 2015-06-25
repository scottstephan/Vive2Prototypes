using UnityEngine;
using System.Collections;

public enum CameraType {
    None,
	FPSCamera,
	OLDFollowBehindCamera,
	FollowBehindCamera,
	FollowPlanarCamera,
	DriftCamera,
	PathCamera,
	RevealCamera,
	DynamicDriftCamera,
	PreviewSceneMakerDebugCamera,
	RotateAroundObjectCamera,
	TravelCamera,
	StaticCamera,
	StaticFollowCamera,
	MigrationCamera,
    CinematicCamera,
	OculusCamera,
	OculusFollowCamera,
    OculusTourCamera
}

public class BaseCameraMode : MonoBehaviour {
	
	public float maxVelocity = 100f;
	public float accelaration = 0.5f;
	[HideInInspector]
	protected Vector3 direction;
	[HideInInspector]
	protected float velocity = 0f;
	[HideInInspector]
	protected Transform cameraTransform;
	
    [HideInInspector]
    public float externalSpeedModifier = 1f;    

	public bool primaryCamera = false;
	
	public bool unlocked = false;
	
	[HideInInspector]
	public bool modeActive = false;

	[HideInInspector]
	public CameraCollisionType runCollision = CameraCollisionType.None;

	[HideInInspector]
	public bool blockSwitch = false;

	[HideInInspector]
	public BaseCameraMode cameraReveal = null;

	[HideInInspector]
	public CameraType cameraType;
	
	[HideInInspector]
	public int masterIndex;

	[HideInInspector]
	public int cameraOrderIndex;

	[HideInInspector]
	public Transform myTransform;	
	
	[HideInInspector]
	public bool drawGizmos = false;
	
	[HideInInspector]
	public string cameraName;
    
    [HideInInspector]
    public bool inited = false;
	
    public virtual bool CameraModeIsLoadable() { return true; } // called prior to adding a reference to this camera into the camera manager. verifies that all camera information is properly setup.
	public virtual void InitCameraMode() { inited = true; }		// called at initialization of unity
    public virtual void IntroDownFadeFinished() {}
	public virtual void StartCameraMode() {} 	// called right before we switch to this camera
	public virtual void EndCameraMode() {}		// called right before we call start of the new camera
	public virtual void UpdateCameraMode() {}	// called during the cameras update.	
	public virtual void FixedUpdateCameraMode() {}
	public virtual void GotoNextCamera(bool from_gui){}		// called when user hits "space" or clicks on camera button
	
	public virtual bool GetFollowsTargets() { return false; }
	public virtual int GetCameraOrderIndex() { return cameraOrderIndex; }

	public void SetDrawGizmos( bool new_value ) {
		drawGizmos = new_value;
	}
}
