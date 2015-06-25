using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReparentObjectHierarchy : MonoBehaviour {

    public string gameObjectName = "OVRCameraController";

    List<Transform> children;

	// Use this for initialization
	void Start ()
    {
        GameObject newParent = GameObject.Find (gameObjectName);

        if (newParent == null)
        {
            Debug.LogError("ReparentObjectHierarchy " + gameObject.name + " cannot find new parent: " + gameObjectName);
            return;
        }

        Transform newTransform = newParent.transform;
        Transform myTransform = gameObject.transform;

        int numChildren = myTransform.childCount;

        if (numChildren <= 0)
        {
            return;
        }

        children = new List<Transform>();

        for (int i=0; i<numChildren; ++i)
        {
            Transform childTransform = myTransform.GetChild (i);
            if (childTransform != null)
            {
#if UNITY_EDITOR
                if (childTransform.gameObject.isStatic)
                {
                    Debug.LogError ("ReparentObjectHierarchy " + gameObject.name + " cannot reattach " + childTransform.gameObject.name + " because it is static!");
                }
#endif
                if (childTransform.gameObject.GetComponent<Collider>() != null)
                {
                    childTransform.gameObject.GetComponent<Collider>().enabled = false;
                }

                children.Add(childTransform);
            }
        }

        for (int i=0; i<children.Count; ++i)
        {
            Vector3 localPos = children[i].transform.position;
            children[i].parent = newTransform;
            children[i].transform.localPosition = localPos;

            if (children[i].gameObject.GetComponent<Collider>() != null)
            {
                children[i].gameObject.GetComponent<Collider>().enabled = false;
            }
        }
	}

    void OnDestroy()
    {
        if (AppBase.Instance.RunningAsPreview())
        {
            return;
        }

        if (children != null)
        {
            for (int i=0; i<children.Count; ++i)
            {
                if (children[i] != null)
                {
                    GameObject.Destroy(children[i].gameObject);
                }
            }
        }
    }
}
