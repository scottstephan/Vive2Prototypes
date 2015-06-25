using UnityEngine;
using System.Collections;

public class GodrayCS : MonoBehaviour {
	public GameObject lightBeam ;
	public int beamCount  = 30;
	public float lightRadius = 100f;
	public Transform sun;
	public float rotAngle;

	// Use this for initialization
	void Start () {
		for(int i = 0; i < beamCount; i++){
			Vector3 randomPos = transform.position + lightRadius*(new Vector3(Random.value-0.5f,0f,Random.value-0.5f));
			Vector3 norm = sun.forward + new Vector3(-0.5f + Random.value, -0.5f + Random.value, -0.5f + Random.value) * rotAngle;
			Quaternion rot = Quaternion.LookRotation(norm.normalized);
			GameObject ray = Instantiate(lightBeam, randomPos, rot) as GameObject; 
			ray.transform.SetParent(transform, true);
		}
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
