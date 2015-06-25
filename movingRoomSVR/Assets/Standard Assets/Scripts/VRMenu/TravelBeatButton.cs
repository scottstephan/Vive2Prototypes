using UnityEngine;
using System.Collections;

public class TravelBeatButton : MonoBehaviour {

	public Material[] buttonMaterials;
	public int materialToUse;

//	GameObject glow;

	// Use this for initialization
	void Start () {

		if (buttonMaterials == null)
			return;

		this.GetComponent<Renderer>().material = buttonMaterials[materialToUse];
	}
	
	public void SetGlow(bool glowOn) {

/*		if (glow == null)
			return;

		if (glowOn)
			glow.SetActive(true);
		else
			glow.SetActive(false);*/
	}
}
