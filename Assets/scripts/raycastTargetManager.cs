using UnityEngine;
using System.Collections;

public class raycastTargetManager : MonoBehaviour {
	public Color originColor;
	public Color hitColor;
	public Color clickColor;

	public GameObject objToRotate;
	// Use this for initialization
	void Start () {
		originColor = gameObject.GetComponent<Renderer> ().material.color;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void rayEnter(){
		gameObject.GetComponent<Renderer> ().material.color = hitColor;
	}

	public void rayExit(){
		gameObject.GetComponent<Renderer> ().material.color = originColor;
	}

	public void onTriggerDown(){
		gameObject.GetComponent<Renderer> ().material.color = clickColor;
		//objToRotate.GetComponent<rotateableObjectsCreator> ().rotSpeed = .3f;
	}

	public void onTriggerUp(){
		gameObject.GetComponent<Renderer> ().material.color = originColor;
		objToRotate.GetComponent<rotateableObjectsCreator> ().rotSpeed = 0f;
	}

	public void rotateObj(float speed){
		objToRotate.GetComponent<rotateableObjectsCreator> ().rotSpeed = speed;
	}


}
