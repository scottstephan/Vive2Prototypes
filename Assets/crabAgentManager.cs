using UnityEngine;
using System.Collections;

public class crabAgentManager : svrInteractableBase {
	private bool hasBeenTossed = false;
	public bool isOnNavMesh = true;
	private NavMeshAgent thisAgent;

	// Use this for initialization
	void Start () {
		thisAgent = gameObject.GetComponent<NavMeshAgent> ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void isPickedUp(){
		isOnNavMesh = false;

	}


}
