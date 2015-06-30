using UnityEngine;
using System.Collections;

public class playerInput : MonoBehaviour {
	public bool hasFoodReady = false;
	private Vector3 foodSpawnPt;
	public GameObject foodArm;
	public GameObject food;
	private GameObject thisFood;
	public float foodThrowForce;
	// Use this for initialization
	void Start () {
		foodSpawnPt = GameObject.Find ("holdSpawnPoint").transform.position;
		foodArm.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (1) && !hasFoodReady) 
		{
			createFood();
		}
		if (Input.GetMouseButton (0) && hasFoodReady) 
		{
			dropFood();
		}
	}

	private void createFood(){
		hasFoodReady = true;
		foodArm.SetActive (true);
		thisFood = GameObject.Instantiate (food, foodSpawnPt, new Quaternion (0, 0, 0, 0)) as GameObject;
		thisFood.transform.parent = foodArm.transform;
		thisFood.GetComponent<Rigidbody> ().useGravity = false;
		thisFood.GetComponent<Rigidbody> ().isKinematic = true;


	}

	private void dropFood()
	{
		thisFood.transform.parent = null;
		thisFood.GetComponent<Rigidbody> ().useGravity = true;
		thisFood.GetComponent<Rigidbody> ().isKinematic = false;
		thisFood.GetComponent<Rigidbody> ().AddForce (Vector3.forward * foodThrowForce);
		//deparent and turn on food object
		//send message that food is out
		hasFoodReady = false;
		foodArm.SetActive (false);
	}
}
