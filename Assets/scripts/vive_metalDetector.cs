using UnityEngine;
using System.Collections;

public class vive_metalDetector : MonoBehaviour {
    private Color originColor;
	private Color destColor; 
    private Rigidbody thisRigidbody;
    private controllerListener.svrController activatingController;
    public float lerpMod = .25f;
	public bool isOn = false;
	public TextMesh debugMesh;
	public TextMesh winMesh;

	public GameObject treasure;
	private float treasureDistance;

    void Start()
    {
		winMesh.text = "";

        thisRigidbody = gameObject.GetComponent<Rigidbody>();
        originColor = gameObject.GetComponent<MeshRenderer>().material.color;
		destColor = Color.red;

        Material cloneMat = gameObject.GetComponent<MeshRenderer>().material;
        gameObject.GetComponent<MeshRenderer>().material = cloneMat; 
    }

	void Update(){
		if (isOn) 
		{
			updateTreasureDistance();
		}
	}

	private void updateTreasureDistance(){
		Vector2 mdPos = new Vector2 (gameObject.transform.position.x, gameObject.transform.position.z);
		Vector2 treasurePos = new Vector2(treasure.transform.position.x, treasure.transform.position.z);

		treasureDistance = Vector2.Distance (mdPos, treasurePos);
        float lerpAmt = treasureDistance - lerpMod;
		Color tempColor = Color.Lerp (destColor, originColor, lerpAmt); //Reverse so that as we get closer to 0, we get more red. 
		gameObject.GetComponent<MeshRenderer> ().material.color = tempColor;

		//float hapticLength = 1000 - (500 * treasureDistance);

        if (treasureDistance < .5) SteamVR_Controller.Input(activatingController.index).TriggerHapticPulse(1000);
        debugMesh.text = "TD: " + treasureDistance;
		if (treasureDistance < .3) winMesh.text = "YOU FOUND TREASURE. so happy 4 u, baby bro. <3 mom & dad :)";
	}

    public void svrControllerDown(controllerListener.svrController controllerThatBroadcasted)
    {
        Debug.Log(gameObject.name + "has heard the svrDown Broadcast");
        activatingController = controllerThatBroadcasted;
        objectIsPickedUp(); //Could be toggled on flag
		toggleOnOff ();
    }

    public void svrControllerUp(controllerListener.svrController controllerThatBroadcasted)
    {
        Debug.Log(gameObject.name + "has heard the svr Up Broadcast");
        objectIsDropped();
        activatingController = null;
    }

    private void objectIsPickedUp()
    {
        thisRigidbody.isKinematic = true;
        gameObject.transform.parent = activatingController.controllerObject.transform;
    }

    private void objectIsDropped()
    {
        gameObject.transform.parent = null;

        Vector3 controllerVelocity = activatingController.curVelocity;
        Debug.Log("At button up, svrControllers velocity is: " + controllerVelocity);

        thisRigidbody.isKinematic = false;
        thisRigidbody.velocity = controllerVelocity; 

        activatingController = null;
    }

	private void toggleOnOff(){
		isOn = !isOn;
	}
}
