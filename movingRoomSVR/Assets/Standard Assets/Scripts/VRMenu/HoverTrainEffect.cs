using UnityEngine;
using System.Collections;

public class HoverTrainEffect : MonoBehaviour {

	WemoItemData _wd;
	
	public GameObject glowRing;
	public GameObject centerObject;

	void Awake() {

		_wd = this.gameObject.GetComponent<WemoItemData>();
		glowRing.SetActive(false);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if (_wd.hoveringActive)
			glowRing.SetActive(true);
		else
			glowRing.SetActive(false);
	}
}
