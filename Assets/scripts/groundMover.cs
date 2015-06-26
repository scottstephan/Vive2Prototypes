using UnityEngine;
using System.Collections;

public class groundMover : MonoBehaviour {
	public Rigidbody thisRigidbody;
	// Use this for initialization
	void Start () {
		thisRigidbody = this.gameObject.GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void moveByForce(Vector3 force){
		thisRigidbody.AddForce(force);
	}

	public void setVelocity(Vector3 vel){
		thisRigidbody.velocity = vel;
	}

	public void setRotation(Vector3 rot){
		gameObject.transform.rotation = Quaternion.Euler (rot);
	}
}
