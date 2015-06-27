using UnityEngine;
using System.Collections;

public class controllerCollisionManager : MonoBehaviour {
    public bool canPickUp;
    public controllerFinder controllers;
    private GameObject poObject;
	// Use this for initialization
	void Start () {
        controllers = GameObject.Find("treasureManager").GetComponent<controllerFinder>();
	}
	
	// Update is called once per frame
	void Update () {
        if (canPickUp && controllers.rightControllerObject != null)
        {
           if(SteamVR_Controller.Input(controllers.rightController).GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
           {
               Debug.Log("Should be picking object up");
               pickUpObject();
           }
        }
	}

    void OnCollisionEnter(Collision col)
    {
        Debug.Log("Controller hit an object: " + col.gameObject.name);

        if (col.gameObject.tag == "liftable")
        {
            canPickUp = true;
            poObject = col.gameObject;
            Debug.Log("Controller hit an object" + col.gameObject.name);
            //listen for input to pick up
            //highlight colliding object
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.tag == "liftable")
        {
            poObject = null;
            //undo those
            //listen for input to pick up
            //highlight colliding object
        }
    }

    private void pickUpObject()
    {
        poObject.transform.parent = controllers.rightControllerObject.transform;
    }
}
