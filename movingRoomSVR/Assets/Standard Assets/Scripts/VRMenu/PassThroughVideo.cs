using UnityEngine;
using System.Collections;

public class PassThroughVideo : MonoBehaviour {

	int _cameraDeviceIndex;
	WebCamTexture _wcTexture;
	WebCamDevice[] _devices;
	GameObject _videoQuad;

	void Awake() {

		_cameraDeviceIndex = GetFrontCamera();
		_videoQuad = this.transform.FindChild("VideoDisplayQuad").gameObject;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnEnable() {

		PlayWebCam();
	}

	void OnDisable() {
		StopWebCam();
	}



	public int GetFrontCamera() {

		_devices = WebCamTexture.devices;

		int cameraIdx = -1;
		for (int i = 0; i < _devices.Length; i++)
		{
			if (!_devices[i].isFrontFacing)
				cameraIdx = i;
		}

		return cameraIdx;
	}

	public void PlayWebCam() {

		if (_cameraDeviceIndex == -1)
			return;

		_wcTexture = new WebCamTexture(_devices[_cameraDeviceIndex].name);
		_wcTexture.Play();

        _videoQuad.GetComponent<Renderer>().sharedMaterial.mainTexture = _wcTexture;

		_videoQuad.SetActive(true);
	}

	public void StopWebCam() {

		if (_cameraDeviceIndex == -1)
			return;

		_videoQuad.SetActive(false);

		_wcTexture.Stop();
		_wcTexture = null;

		_videoQuad.GetComponent<Renderer>().sharedMaterial.mainTexture = null;
	}
}
