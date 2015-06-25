using UnityEngine;
using System.Collections;

public class OculusAreaFadeTrigger : MonoBehaviour {

    public Transform respawnMarker;

    Transform myTransform;


    void Awake()
    {
        myTransform = transform;
    }
	// Use this for initialization
	void Start () 
    {
	    if (GetComponent<Collider>() == null)
        {
            Debug.LogError("OculusAreaFadeTrigger " +gameObject.name + " has no collider!");
        }
        else if (!GetComponent<Collider>().isTrigger)
        {
            Debug.LogError("OculusAreaFadeTrigger " +gameObject.name + " collider must have isTrigger = true!");
        }

        gameObject.layer = 27;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter (Collider other) 
    {
		if (OculusCameraFadeManager.IsFaded())
			return;

        if (CameraManager.GetActiveCameraType() == CameraType.OculusCamera)
        {
            OculusFPSCameraMode.singleton.StopAreaFade();
        }
    }

    void OnTriggerExit (Collider other) 
    {
        if (CameraManager.GetActiveCameraType() == CameraType.OculusCamera)
        {
            OculusFPSCameraMode.singleton.StartAreaFade(this, myTransform.position);
        }
    }

    public Vector3 GetRespawnPosition()
    {
        if (respawnMarker != null)
        {
            return respawnMarker.position;
        }

        return myTransform.position;
    }

    public Quaternion GetRespawnRotation()
    {
        if (respawnMarker != null)
        {
            return respawnMarker.rotation;
        }
        
        return myTransform.rotation;
    }

}

