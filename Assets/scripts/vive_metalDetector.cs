using UnityEngine;
using System.Collections;

public class vive_metalDetector : MonoBehaviour {
    private Color originColor;
    private Rigidbody thisRigidbody;
    private controllerListener.svrController activatingController;
	public bool isOn = false;
	public TextMesh debugMesh;

	public GameObject treasure;
	private float treasureDistance;

    void Start()
    {
        thisRigidbody = gameObject.GetComponent<Rigidbody>();
        originColor = gameObject.GetComponent<MeshRenderer>().material.color;
            //Instance the mat to avoid overwriting the original material. 
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
		debugMesh.text = "TD: " + treasureDistance;
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
     //   gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

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

    //    gameObject.GetComponent<MeshRenderer>().material.color = originColor;
        activatingController = null;
    }

	private void toggleOnOff(){
		isOn = !isOn;
	}
}
