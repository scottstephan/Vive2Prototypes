using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

	GameObject _uiPoint;

	void Awake() {

		_uiPoint = GameObject.Find("UI Point");
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void LateUpdate () 
	{
		if (CameraManager.singleton == null)
			return;

		transform.position = Vector3.Lerp(transform.position, _uiPoint.transform.position, Time.deltaTime * 50.0f);
		transform.rotation = _uiPoint.transform.rotation;
	}
}
