using UnityEngine;
using System.Collections;

public class birdManager : MonoBehaviour {
	public Vector3 travelToFoodDest;
	private Vector3 originPos;
	private Vector3 departurePos;
	private GameObject curFoodTarget;
	public float birdFlySpeed;
	public float feedTime;
	public float bobSmooth = .25f;
	public float maxBobHeight = 1.5f;
	//Roll a random offset for the arrival to avoid stacks
	private float yOffset = 1.5f;
	private float xOffset = 1.5f;
	private float zOffset = 1.5f;
	public float randomPosMod = .5f;
	// Use this for initialization
	void Start () {
		originPos = gameObject.transform.position;
	}

	void OnEnable()
	{
		playerInput.foodOut += birdSeesFood;
	}
	
	
	void OnDisable()
	{
		playerInput.foodOut -= birdSeesFood;
	}

	// Update is called once per frame
	void Update () {
	
	}

	private void birdSeesFood(){ //every bird will converge. Maybe have a high-level bird manager that picks a few. 
		Debug.Log (gameObject.name + " sees the food"); 

		if (travelToFoodDest == Vector3.zero) 
		{ 
			StopAllCoroutines(); // In case a travel back is running
			curFoodTarget = playerInput.lastThrownFoodObject;
			departurePos = gameObject.transform.position;
			travelToFoodDest = curFoodTarget.transform.position + rollRandomOffsets(); // will only eat a new food object. could use a list to keep track of multiples. 
			StartCoroutine("travelToFood");
		}
		else if(travelToFoodDest != Vector3.zero)
		{
			//Potential behavior re-eval for a bird already feeding. 
		}
	}

	IEnumerator travelToFood(){
		float i = 0f;
		float rate = 1/birdFlySpeed; //Always the same. Should add a dist. mod.
		Vector3 actualDest = travelToFoodDest + rollRandomOffsets ();
		bool destSettled = false;

		while (i < 1f) {
			i += Time.deltaTime * rate;

			if(curFoodTarget.transform.position != travelToFoodDest)
			{ 
				travelToFoodDest = curFoodTarget.transform.position ; //update the travel-to as the food settles
			}
			else if(destSettled == false)//i.e: wait until the dust settles to establish the new pos
			{
				actualDest = travelToFoodDest + rollRandomOffsets(); 
				destSettled = true; //this has to be a better way to do this
			}
			gameObject.transform.position = Vector3.Lerp(departurePos, actualDest, i);
			yield return null; 
		}
		StartCoroutine ("feed");
	}

	IEnumerator feed(){
		float i = 0f;
		float time = 0f;
		float initY = gameObject.transform.position.y; //y offset for the ping-pong bob behavior. Keeps him from resetting to the min value. I guess I could just add it to i? Oh well.

		while (time < feedTime) {
			i += Time.deltaTime * bobSmooth;
			time += Time.deltaTime;
			gameObject.transform.position = new Vector3(gameObject.transform.position.x, Mathf.PingPong(i,maxBobHeight) + initY, gameObject.transform.position.z);
			yield return null; 
		}

		StartCoroutine ("travelToOrigin");
	}

	IEnumerator travelToOrigin(){
		float i = 0f;
		float rate = 1/birdFlySpeed; //Always the same. Should add a dist. mod.
		travelToFoodDest = Vector3.zero;// allows for a new feeding. Could do this up front to allow for a re-pos while traveling home. 
		Vector3 exitPos = gameObject.transform.position;

		while (i < 1f) {
			i += Time.deltaTime * rate;
			gameObject.transform.position = Vector3.Lerp(exitPos, originPos, i);
			yield return null; 
		}
	
	}

	private Vector3 rollRandomOffsets(){
		Vector3 randomRoll = new Vector3(Random.Range (xOffset - randomPosMod, xOffset + randomPosMod), Random.Range (yOffset - randomPosMod * .5f, yOffset + randomPosMod * .5f), Random.Range (zOffset - randomPosMod, zOffset + randomPosMod));
		if(Random.Range(-1,1) == -1) randomRoll.x *= -1;
		if(Random.Range(-1,1) == -1) randomRoll.z *= -1;
		return randomRoll;
	}
}
