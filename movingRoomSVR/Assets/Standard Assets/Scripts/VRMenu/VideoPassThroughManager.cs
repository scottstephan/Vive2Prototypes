using UnityEngine;
using System.Collections;

public class VideoPassThroughManager : MonoBehaviour {

//	bool _videoPlaying = false;
//	FloatingMenuManager _fmm;
	public PassThroughVideo ptv;

	void Awake() {
	}

	// Update is called once per frame
	void Update () {
// video pass through off in retail demo.
/*		if (Input.GetButtonUp("VideoButton"))
			ToggleVideo();*/
	}

	void ToggleVideo() {

		if (ptv.gameObject.activeSelf)
			StopVideo();
		else
			StartVideo();
	}

	void StopVideo() {

		ptv.gameObject.SetActive(false);

	}

	void StartVideo() {

		ptv.gameObject.SetActive(true);
	}

}
