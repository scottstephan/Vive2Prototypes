using UnityEngine;
using System.Collections;

public class TranslateWithObject : MonoBehaviour {

    public GameObject Anchor;
    public string AnchorName;

    Animation myAnim;
    Transform myTransform;
    Transform anchorTransform;
    Vector3 anchorLastPos;
    FishBowl fishBowl;

	// Use this for initialization
	void Start ()
    {
        if (!string.IsNullOrEmpty(AnchorName))
        {
            Anchor = GameObject.Find (AnchorName);
        }

        myTransform = gameObject.transform;
        myAnim = gameObject.GetComponent<Animation>();
        anchorTransform = Anchor.transform;
        anchorLastPos = Vector3.zero;

        if (gameObject.isStatic)
        {
            Debug.LogError ("TranslateWithObject attached to static object " + gameObject.name + " -  will not be able to move object!");
        }

        fishBowl = gameObject.GetComponent<FishBowl>();
	}
	

	// Update is called once per frame
	void LateUpdate ()
    {
        if (myAnim != null && myAnim.isPlaying)
        {
            myTransform.position += anchorTransform.position;
        }
        else
        {
            myTransform.position += anchorTransform.position - anchorLastPos;
        }

        anchorLastPos = anchorTransform.position;

        if (fishBowl != null)
        {
            fishBowl.UpdateTransform(myTransform);
        }
	}
}
