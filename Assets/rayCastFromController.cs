using UnityEngine;
using System.Collections;

public class rayCastFromController : MonoBehaviour {

	private Vector3 raycastOrigin;
	private Vector3 raycastFwd;
	private GameObject lastHitObject;
	private GameObject lastHitRotController;
	private bool canClick;
	private bool triggerIsDown;

	private float lastFramePos;
	private float thisFramePos;
	private Vector3 lastFrameVec;
	private Vector3 thisFrameVec;
	private Vector3 thisFrameRot;
	private Vector3 lastFrameRot;

	public float contVel;
	public Vector3 contVec;
	public float totalVel = 0f;
	public float cVelMult = 20f;
	public int controllerIndex = 1;
	public TextMesh cVelMesh;
	public TextMesh vMesh;
	private float lastMaxCVel;
	private float curVel;
	private float dampFactor = .025f;
	// Use this for initialization
	void Start () {

		Debug.Log("Casting from:" + transform.gameObject);
		cVelMesh = GameObject.Find ("cVelMesh").GetComponent<TextMesh> ();
		vMesh = GameObject.Find ("vecMesh").GetComponent<TextMesh> ();

	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hit;
		raycastOrigin = transform.position;
		raycastFwd = transform.forward;

		Debug.DrawRay (raycastOrigin, raycastFwd,Color.red);

		if (Physics.Raycast (raycastOrigin, raycastFwd, out hit, 100.0F)) {
			if (hit.collider.gameObject.tag == "raycastable" && hit.collider.gameObject != lastHitObject) {
				Debug.Log ("Ray hit:" + hit.collider.gameObject.name);
				canClick = true;
				hit.collider.gameObject.BroadcastMessage ("rayEnter");
				lastHitObject = hit.collider.gameObject;
				lastHitRotController = hit.collider.gameObject;
			}
		} 
		else if(lastHitObject != null && !triggerIsDown) {
			Debug.Log("Ray missed objects");
			lastHitObject.BroadcastMessage("rayExit");
			lastHitObject = null;
			canClick = false;
		}

		if (canClick && !triggerIsDown)
			checkInputDown ();
		if (triggerIsDown) {
			checkInputUp();
			calcVel();
		}

		if (Input.GetKey (KeyCode.P)) {
			calcVel();
		}


	}

	private void calcVel(){ //I WANT THE DELTA IN ROTATION! duhhh. 
		thisFrameVec = transform.position;

		thisFrameRot = transform.rotation.eulerAngles;
		contVel = getVelByY (thisFrameRot.y, lastFrameRot.y);
		//contVel *= dampFactor;

	//	if(Mathf.Abs(contVel) > .3)
		lastHitObject.GetComponent<raycastTargetManager> ().rotateObj (contVel);

		lastFrameVec = thisFrameVec;
		lastFrameRot = thisFrameRot;

	}

	private float getVelByY(float curFrame, float lastFrame){
		float cVX = curFrame - lastFrame;
		vMesh.text = "vMesh:" + curFrame + "/" + lastFrame;
		cVelMesh.text = "cVel: " + cVX;
		return cVX;
	}

	private float getVelByMag(Vector3 curFrame, Vector3 lastFrame){
		contVec = curFrame - lastFrame;
		vMesh.text = "vMesh:" + curFrame + "/" + lastFrame;
		return contVec.magnitude;
	}



	private void checkInputDown(){
		if (SteamVR_Controller.Input (controllerIndex).GetPressDown (SteamVR_Controller.ButtonMask.Trigger)) {
			//MONITOR CONTROLLER X VEL
			Debug.Log(gameObject.name + " has cont down");
			lastHitObject.BroadcastMessage ("onTriggerDown");
			triggerIsDown = true;

			lastFrameVec = transform.position;
			lastFrameRot = transform.rotation.eulerAngles;

			StopCoroutine("decayRotSpeed");
		} 
	}

	private void checkInputUp(){
		if (SteamVR_Controller.Input (controllerIndex).GetPressUp (SteamVR_Controller.ButtonMask.Trigger)) {
			lastHitObject.BroadcastMessage ("onTriggerUp");
			triggerIsDown = false;
			StartCoroutine("decayRotSpeed");
		}
	}

	IEnumerator decayRotSpeed(){
		float speedDecay;
		if (contVel > 0) {
			speedDecay = -.01f;
			while (contVel > 0) {
				contVel += speedDecay;
				lastHitRotController.GetComponent<raycastTargetManager> ().rotateObj (contVel);
				yield return null;
			}
		} 
		else { 
			speedDecay = .01f;
			while (contVel < 0) {
				contVel += speedDecay;
				lastHitRotController.GetComponent<raycastTargetManager> ().rotateObj (contVel);
				yield return null;
			}
		}
		contVel = 0f;
	}
}
