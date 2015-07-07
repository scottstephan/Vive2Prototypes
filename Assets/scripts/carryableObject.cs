using UnityEngine;
using System.Collections;
/*THIS HAS BEEN DEPRECATED BY listenForSVCCOllision !!! */
public class carryableObject : MonoBehaviour {
	public enum carryableTypes{metalDetetctor, food};
	public carryableTypes thisObjectType;

    public bool debugMode = false; //should be a sim-wide flag or, at least, a delegate
    private bool canBePickedup;
    private GameObject curHandTouching;
    private GameObject lastTouchedByController;
    public int lastTouchedIndex = -1;
    private Color originalColor;
    private controllerFinder controllers;
    private bool isHeld;
    public bool hasOnOffState = false;
    private bool isPhysicsObject = false;
	// Use this for initialization

    //A goofy shortcut limitation here is that only the first controller to touch
    //the object will be able to pick it up. I should put valid touches into a <List> or something
    //lol. 

	void Start () {
        originalColor = gameObject.GetComponent<MeshRenderer>().material.color;

	}
	
	// Update is called once per frame
	void Update () {
        if (canBePickedup && !isHeld && lastTouchedIndex != -1)
        {
            if (SteamVR_Controller.Input(lastTouchedIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) //Maybe this ought to be handed back to controllerListener?
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
					//Here we'd check for whether or not it picks up vel from controller arc etc etc.
                dropObject();
            }
        }
	}

    void OnTriggerEnter(Collider col) //Maybe add classes- Interactable vs. Holdable etc. TGhe core logic here is identical. 
    {
        //Debug.Log("Somethings in pick up collider");
        if (col.gameObject.tag == "controller" && !canBePickedup && !isHeld)
        {
            if (debugMode)
            {
                Debug.Log("Pick up object being tapped by controller");
                gameObject.GetComponent<MeshRenderer>().material.color = Color.red; 
            }
            canBePickedup = true;
            lastTouchedByController = col.gameObject;
            
            lastTouchedIndex = controllerListener.returnIndexByName(lastTouchedByController.name); //Should be a list of valid touches
            Debug.Log("Last touched index is: " + lastTouchedIndex);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "controller" && canBePickedup && !isHeld)
        {
            if (debugMode)
            {
                Debug.Log("Controller left my trigger");
                gameObject.GetComponent<MeshRenderer>().material.color = originalColor;
            }
            canBePickedup = false;
            lastTouchedByController = null; //rmv from list

        }
    }

    private void pickUpObject()
    {
        transform.parent = lastTouchedByController.transform;
		gameObject.transform.rotation= Quaternion.Euler(new Vector3(0,0,0));
        isHeld = true; //switch bases on type
		if(thisObjectType == carryableTypes.metalDetetctor)gameObject.GetComponent<metalDetector>().toggleMD(lastTouchedIndex);
        
    }

    private void dropObject()
    {
        transform.parent = null;
        isHeld = false;
		if(thisObjectType == carryableTypes.metalDetetctor) gameObject.GetComponent<metalDetector>().toggleMD(lastTouchedIndex);
    }
}
