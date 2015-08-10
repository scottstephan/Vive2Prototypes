using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class navAgentTest : MonoBehaviour {
	private NavMeshAgent thisAgent;
	public List<Transform> walkToNodes = new List<Transform>();
	private GameObject[] walkNodeObjects;
	private float groundY;
	// Use this for initialization
	void Start () {
		thisAgent = gameObject.GetComponent<NavMeshAgent> ();
		groundY = transform.position.y;
		if (walkToNodes.Count == 0) 
		{
			walkNodeObjects = GameObject.FindGameObjectsWithTag ("walkNode");
			
			foreach (GameObject go in walkNodeObjects) {
				walkToNodes.Add (go.GetComponent<Transform> ()); //This should be a static that comes from somewhere else?
			}
			
			Debug.Log ("WalkToNode has " + walkToNodes.Count + " items");
			
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.N) && thisAgent.isOnNavMesh) {
		//	thisAgent.acceleration = 10;
			Vector3 dest = walkToNodes[Random.Range(0,walkToNodes.Count)].position;
			dest.y = groundY;
			Debug.Log("Heading to:" + dest);
			thisAgent.SetDestination(dest);
		}
	}
}
