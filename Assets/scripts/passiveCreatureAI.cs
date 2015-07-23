using UnityEngine;
using System.Collections;
//TO-DO: Don't need the rigidbody physics 95% of the time- Only when thrown. Need a grounded protocol to tell when they're on the ground. When they are, we can turn the physics off again. 

public class passiveCreatureAI : passiveCreatureBase {
	private Transform curTransform;
	private Transform destTransform;
	private Quaternion destRot;

	public float moveSpeed;
	public float rotationSpeed;

	public float maxWaitTime;
	public float minWaitTime;

	public float groundedY;
	private int frameCounter;
	private int freqToPollCreature = 60; //num of update iterations btw. poking the creature 
	public float curTime;


	private Rigidbody thisRigidbody;
	public bool isWalkingToNode = false;
	// Use this for initialization
	public override void Start () {
		base.Start ();

		thisRigidbody = gameObject.GetComponent<Rigidbody>();
		groundedY = gameObject.transform.position.y; //gotta find a better way to do this. Could just calc x/z and comp. into a new V3. Could lock y in transit? Could raycast at ground, but that seems like a waste

		curTransform = walkToNodes [Random.Range (0, walkToNodes.Count)];

		Vector3 tempVector = curTransform.position;
		tempVector.y = groundedY;

		gameObject.transform.position = tempVector;
	}
	
	// Update is called once per frame
	void Update (){ 
		if (!isWalkingToNode) 
		{
			curTime += Time.deltaTime;
			frameCounter++;

			if (curTime > minWaitTime && curTime < maxWaitTime && frameCounter % freqToPollCreature == 0) 
			{
				float randomRoll = Random.Range (0, maxWaitTime - curTime); 
//				Debug.Log("Random roll is: " + randomRoll);
				if (randomRoll < 1) {
					isWalkingToNode = true;
					findNode ();
					curTime = 0;
				}
			} else if (curTime > maxWaitTime) {
				isWalkingToNode = true;
				findNode ();
				curTime = 0;
			}
		}
	}

	private void findNode(){

		destTransform = returnNewNode();
		destRot = Quaternion.LookRotation (gameObject.transform.position - destTransform.position); //will prob want to lerp this over time
		isWalkingToNode = true;

		StartCoroutine ("rotateToNode");
	}

	private Transform returnNewNode(){
		Transform specNode = walkToNodes[Random.Range (0, walkToNodes.Count)];
		walkToNodes.Remove (specNode); //remove the new home from a list of possibles
		return specNode;
	}

	IEnumerator rotateToNode(){ //crab turns to face its dest node. TO-DO: Crab's fwd Z is wrong! Node Z's should all be sync'ed
		float i = 0;

		while (i < 1) 
		{
			i += rotationSpeed;
			
			Quaternion newRot = Quaternion.Lerp(curTransform.rotation, destRot,i);
			newRot.x = 0; newRot.z = 0;
			gameObject.transform.rotation = newRot;

			yield return null;
		}

		StartCoroutine ("walkToNode");
	}

	IEnumerator walkToNode(){
		float i = 0;

		walkToNodes.Add (curTransform); //add the current node back to the list of possibles for other crabs
		while (i < 1) 
		{
			i += moveSpeed;

			Vector3 newPos = Vector3.Lerp(curTransform.position,destTransform.position,i);
			newPos.y = groundedY;

			gameObject.transform.position = newPos; //This could cause some hell vs. rigidbody.MovePositon, but at 90fps, should be okay?
			yield return null;
		}
		curTransform = destTransform;

		Debug.Log ("Done walking");
		isWalkingToNode = false;
	}
	
}
