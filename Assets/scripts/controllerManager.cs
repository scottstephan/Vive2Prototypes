using UnityEngine;
using System.Collections;

public class controllerManager : MonoBehaviour {
//	public int leftControlIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.Leftmost);
//	public int rightControlIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.Rightmost);
	public int lookForIndex = 3;
	public float speed = 4f;
	public GameObject controller;
	private Vector3 fwdVelocity;
	public groundMover mover;
	private float lerpTime = 5f;
	public float t = 0;
	private Vector3 oldRot;
	private Vector3 deltaRot;
	private GameObject vehicle;
	private bool rotationLocked = false;
    public bool useViewToControl = false;
    public GameObject headCam;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (SteamVR_Controller.Input (lookForIndex).GetPress (SteamVR_Controller.ButtonMask.Trigger)) 
		{
				Debug.Log ("SteamVR Controller Trigger Pressed");
                if(useViewToControl)
                {
                    fwdVelocity = headCam.transform.forward;

                }
                else
                {
                    fwdVelocity = controller.transform.forward;
                }
				mover.setVelocity(fwdVelocity * -speed);
		//, m,		if(!rotationLocked) lockRotation();
		//		getRotationDelta();
		//		setNewRotation();
		}
		if (SteamVR_Controller.Input (lookForIndex).GetPressUp (SteamVR_Controller.ButtonMask.Trigger)) {
			Debug.Log ("SteamVR Controller Trigger Lifted");
			mover.setVelocity(new Vector3(0,0,0));
			rotationLocked = false;
			t = 0; //reset lerp
		}

	//	if(rotationLocked) lerpToRot();
	}

	private void lockRotation(){
		oldRot.x = controller.transform.rotation.x;
		oldRot.y = controller.transform.rotation.y;
		oldRot.z = controller.transform.rotation.z;
		rotationLocked = true;
	}

	private void getRotationDelta(){
		deltaRot.x = oldRot.x - controller.transform.rotation.x;
		deltaRot.y = oldRot.y - controller.transform.rotation.y;
		deltaRot.z = oldRot.z - controller.transform.rotation.z;
	}

	private void setNewRotation(){
		Vector3 newRot = controller.transform.eulerAngles + deltaRot;
		mover.setRotation(newRot);
	}

	private void lerpToRot(){

		float amountPerFrame = 1 / lerpTime;

		if (t <= 1) {
			Vector3 tempRot = controller.transform.eulerAngles + deltaRot;
			Vector3 newRot = Vector3.Lerp (oldRot, tempRot, t);
			mover.setRotation(newRot); 
			t += amountPerFrame * Time.deltaTime;
		}

		oldRot = controller.transform.eulerAngles;

	}


}
