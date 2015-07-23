using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class passiveCreatureBase : MonoBehaviour {
	public static List<Transform> walkToNodes = new List<Transform>();
	private GameObject[] walkNodeObjects;

	// Use this for initialization
	public virtual void Start () {

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
	
	}

	private void determineGrounded(){
		//raycast down.
		//if it hits ground && dist of ray is < threshold- We're grounded.
		//check the y to see if we're upside down!
		//if so, flip and set grounded
	}
}
