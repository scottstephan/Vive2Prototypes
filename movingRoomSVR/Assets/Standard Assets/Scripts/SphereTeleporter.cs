using UnityEngine;
using System.Collections;

public class SphereTeleporter : MonoBehaviour {
	
	public string levelString;
	public Vector3 destinationPoint;
	
//	private GameObject camera;
//	private SphereCollider collider;
	
	// Use this for initialization
	void Start () {
	
		levelString = "sandycliff";
		
		destinationPoint = new Vector3(-210,-285,-250);
		
//		camera = GameObject.Find("OculusCameraMode");
//		collider = GetComponentInChildren<SphereCollider>();
	}
	
	// Update is called once per frame
	void Update () {
		
/*		float dist = (camera.transform.position - transform.position).magnitude;
		float radius = collider.radius * transform.localScale.x;
		
		if (dist > radius)
		{			
			//App.SphereManager.TravelToSphere("scripps-kelp", null);
			//App.SphereManager.TravelToSphere("coral-garden", null);
			
			camera.transform.position = new Vector3(-210,-285,-250);
		}*/
	}
}
