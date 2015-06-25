using UnityEngine;
using System.Collections;

public class TranslateXZRotateWithObject : MonoBehaviour {

    public GameObject Anchor;
    public string AnchorName;

    Animation myAnim;
    Transform myTransform;
    Transform anchorTransform;
    Vector3 anchorLastPos;
    Vector3 startDir;
    float startY;
    FishBowl fishBowl;

	// Use this for initialization
	void Start ()
    {
        if (!string.IsNullOrEmpty(AnchorName))
        {
            Anchor = GameObject.Find (AnchorName);
        }

        myTransform = gameObject.transform;
        startDir = myTransform.position;
        startY = startDir.y;
        startDir.y = 0f;
        myAnim = gameObject.GetComponent<Animation>();
        anchorTransform = Anchor.transform;
        anchorLastPos = GetPos();
        myTransform.position = anchorLastPos;

        fishBowl = gameObject.GetComponent<FishBowl>();

        if (gameObject.isStatic)
        {
            Debug.LogError ("TranslateWithObject attached to static object " + gameObject.name + " -  will not be able to move object!");
        }
	}
	
    Vector3 GetPos()
    {
        Vector3 fwd = anchorTransform.forward;
        fwd.y = 0f;
        Vector3 pos = Quaternion.LookRotation(fwd) * startDir;
        pos += anchorTransform.position;
        pos.y = startY;
        return pos;
    }

	// Update is called once per frame
	void LateUpdate ()
    {
        Vector3 newPos = GetPos();

        if (myAnim != null && myAnim.isPlaying)
        {
            myTransform.position += newPos;
        }
        else
        {
            myTransform.position += newPos - anchorLastPos;
        }

        anchorLastPos = newPos;

        if (fishBowl != null)
        {
            fishBowl.UpdateTransform(myTransform);
        }
	}
}
