using UnityEngine;
using System.Collections;

public class CollectibleTextureOffset : MonoBehaviour {
	
	public float scrollSpeed = 0.25F;
	
	// Use this for initialization
	void Start () {
	

	}
	
	//The FixedUpdate() function is called a set number of times per second by Unity.
	void FixedUpdate()
		{
			//We use a short formula -­-­ multiply the Scroll Speed value by the current time -­-­to define a texture offset.
			float offset = Time.time * scrollSpeed;
			GetComponent<Renderer>().material.mainTextureOffset = new Vector2 (offset,offset);
		}
	
	// Update is called once per frame
	void Update () {
	
	}
}
