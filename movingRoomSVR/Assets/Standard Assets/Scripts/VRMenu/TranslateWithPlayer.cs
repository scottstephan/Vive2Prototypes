using UnityEngine;
using System.Collections;

public class TranslateWithPlayer : MonoBehaviour {

	Transform _bod;
    Transform myTransform = null;

	// Use this for initialization
	void Start () {
        myTransform = transform;
		_bod = GameObject.Find("OVRPlayerBody").transform;	
	}
	
	// Update is called once per frame
	void Update () {

        myTransform.position = _bod.position;

		//perhaps not necessary?
		//this.transform.eulerAngles = Vector3.zero;
	
	}
}
