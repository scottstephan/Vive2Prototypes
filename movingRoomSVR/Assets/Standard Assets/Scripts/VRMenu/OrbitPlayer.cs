using UnityEngine;
using System.Collections;

public class OrbitPlayer : MonoBehaviour {

	Transform _bod;
    Transform myTransform;
	// Use this for initialization
	void Start () {
        myTransform = transform;
		_bod = GameObject.Find("OVRPlayerBody").transform;		
	}
	
    public void UpdatePlayer()
    {
        myTransform.position = _bod.position;

        myTransform.rotation = Quaternion.LookRotation( CameraManager.GetCurrentCameraFlattenedForward() );

    }
	// Update is called once per frame
	void Update () {
        UpdatePlayer ();
	}
}
