﻿using UnityEngine;
using System.Collections;

public class svr_shotgunTrigger : MonoBehaviour {
	public GameObject bullet;
	public GameObject muzzlePt;
    public float bulletForce = 1000f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void svrControllerDown()
	{
		Debug.Log (gameObject.name + "has gotten broadcast from controller");
		fireBullet ();
	}

	private void fireBullet()
	{
		GameObject tempBullet = Instantiate (bullet,muzzlePt.transform.position,new Quaternion(0,0,0,0)) as GameObject;
		tempBullet.GetComponent<Rigidbody> ().AddForce(muzzlePt.transform.forward * bulletForce);
	}
}
