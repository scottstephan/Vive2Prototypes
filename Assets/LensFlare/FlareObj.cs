using UnityEngine;
using System.Collections;

public class FlareObj : MonoBehaviour {
	public float dist;

	private LensFlareManager manager;
	private Transform myXform;
	private Material mat;

	void Start () {
		myXform = transform;
		mat =  GetComponent<Renderer>().material;
	}

	public void SetManager(LensFlareManager lfm){
		manager = lfm;
	}
	
	public void SetPosition(Vector3 pos){
		myXform.position = pos;
	}

	public void SetRotation(Quaternion rot){
		myXform.rotation = rot;
	}

	void Update () {
	
	}
}
