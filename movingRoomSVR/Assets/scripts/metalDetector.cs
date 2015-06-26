using UnityEngine;
using System.Collections;

public class metalDetector : MonoBehaviour {
    public Transform controller; //Should be R hand'
    public Transform[] treasureVectors;
    public float treasureDetectionThreshhold;
    public float dist = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        updateDistanceFromTreasure();
	}

    private void updateDistanceFromTreasure()
    {
        foreach(Transform treasureVector in treasureVectors)
        {
            dist = Vector3.Distance(treasureVector.position, controller.position);
            if (dist < treasureDetectionThreshhold)
            {
                Debug.Log("Close to treasure!");
            }
        }
    

    }
}
