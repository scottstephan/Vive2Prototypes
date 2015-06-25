using UnityEngine;
using System.Collections;

public class ParkingSpot : MonoBehaviour {
		
	[HideInInspector]
	public bool available = true;
	[HideInInspector]
	public Vector3 pos;
	[HideInInspector]
	public Vector3 up;
	[HideInInspector]
	public SpeciesSize maxSize;
	[HideInInspector]
	public Mesh spotMesh;
	

	// Use this for initialization
	void Start () {
		pos = transform.position;
		up = transform.up;
		MeshFilter mf = GetComponent<MeshFilter>();
		if(mf) spotMesh = mf.mesh;
	}
	
}
