using UnityEngine;
using System.Collections;

public class carryableObject : MonoBehaviour {
    public bool debugMode = false;
    private bool canBePickedup;
    private GameObject curHandTouching;
    private GameObject lastTouchedByController;
    public int lastTouchedIndex = -1;
    private Color originalColor;
    private controllerFinder controllers;
    private bool isHeld;
	// Use this for initialization

    //A goofy shortcut limitation here is that only the first controller to touch
    //the object will be able to pick it up. I should put valid touches into a <List> or something
    //lol. 

	void Start () {
        originalColor = gameObject.GetComponent<MeshRenderer>().material.color;
	}
	
	// Update is called once per frame
	void Update () {
        if (canBePickedup && !isHeld)
        {
            if (SteamVR_Controller.Input(lastTouchedIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                Debug.Log("Picking up object");
                pickUpObject();
            }
        }

        if (isHeld)
        {
            if (SteamVR_Controller.Input(lastTouchedIndex).GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                Debug.Log("Dropping object");
                dropObject();
            }
        }
	}

    void OnTriggerEnter(Collider col)
    {
        Debug.Log("Somethings in pick up collider");
        if (col.gameObject.tag == "controller" && !canBePickedup && !isHeld)
        {
            if (debugMode) Debug.Log("Pick up object being tapped by controller");
            canBePickedup = true;
            lastTouchedByController = col.gameObject;
            gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
            controllers = col.gameObject.GetComponent<controllerFinder>();
            lastTouchedIndex = col.gameObject.GetComponent<controllerIndexKeeper>().controllerIndex;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "controller" && canBePickedup && !isHeld)
        {
            if(debugMode) Debug.Log("Controller left my trigger");
            canBePickedup = false;
            lastTouchedByController = null;
            gameObject.GetComponent<MeshRenderer>().material.color = originalColor;

        }
    }

    private void pickUpObject()
    {
        transform.parent = lastTouchedByController.transform;
        isHeld = true;
        gameObject.transform.position = lastTouchedByController.transform.forward;
    }

    private void dropObject()
    {
        transform.parent = null;
        isHeld = false;
        //Unparent child
    }
}
