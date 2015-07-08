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
	private float yOffset = 1f;
	private float xOffset = 1.5f;
	private float zOffset = 1.5f;
	public float randomPosMod = .5f;
	//For random circle pos
	public float circleRadiusX = 5;
	public float circleRadiusZ = 5;
	public float posTheta = 360;
	//AI stuff
	public float birdHunger = 0; //At 100, it is 100% sure to go get food.
	private bool isEating = false;
	private Color originColor;
	// Use this for initialization
	void Start () {
		originPos = gameObject.transform.position;
		Material clone = gameObject.GetComponent<MeshRenderer> ().material; //instance and assign
		gameObject.GetComponent<MeshRenderer> ().material = clone; //material to avoid perma changes.
		birdHunger = Random.Range (0, 100);
		originColor = gameObject.GetComponent<MeshRenderer> ().material.color;
	}

	void OnEnable()
	{
	    svr_birdFeeder.foodOut += birdSeesFood;
	}
	
	
	void OnDisable()
	{
        svr_birdFeeder.foodOut -= birdSeesFood;
	}

	// Update is called once per frame
	void Update () {
		if (!isEating) 
		{ 
			birdHunger += 1 * Time.deltaTime;
			Color c = Color.Lerp(originColor,Color.red,birdHunger / 100);
			gameObject.GetComponent<MeshRenderer> ().material.color = c;
		}
	}

	private void birdSeesFood(){ 
		Debug.Log (gameObject.name + " sees the food"); 
		isEating = Random.Range (0, 100) < birdHunger ? true : false;

		if (travelToFoodDest == Vector3.zero && isEating) 
		{ 
			StopAllCoroutines(); // In case a travel back is running
            curFoodTarget = birdSimManager.lastTossedFood;
			//curFoodTarget.GetComponent<drawCircle> ().isDrawingCircle = true;

            travelToFoodDest = getPositionAroundCircle(birdSimManager.lastTossedFood.transform.position, Random.Range(0, posTheta), circleRadiusX, circleRadiusZ); //get a random position in a circle around wherever the food is. Probs wanna wait a few frames or update dest after food has settled
			departurePos = gameObject.transform.position;
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

		while (i < 1f) {
			i += Time.deltaTime * rate;
			gameObject.transform.position = Vector3.Lerp(departurePos, travelToFoodDest, i);
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
		Destroy (curFoodTarget);
		isEating = false;
		birdHunger = 0;
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

	private Vector3 getPositionAroundCircle(Vector3 circleCenter, float theta, float radiusX, float radiusZ){
		Vector3 circlePos = Vector3.zero;
		circlePos.x = circleCenter.x + Mathf.Cos(theta) * radiusX;
		circlePos.z = circleCenter.z + Mathf.Sin(theta) * radiusZ;
		circlePos.y = yOffset;
		return circlePos;
	}
}
