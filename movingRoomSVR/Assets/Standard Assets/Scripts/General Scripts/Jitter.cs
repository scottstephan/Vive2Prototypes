using UnityEngine;
using System.Collections;

public class Jitter : MonoBehaviour {

    public bool cumulative;
    public Vector3 amount = Vector3.one;

    Vector3 startPos;

	void Start () 
    {
        startPos = transform.localPosition;	
	}
	
	void Update () 
    {
        Vector3 jitter = new Vector3((-0.5f + Random.value) * amount.x, (-0.5f + Random.value) * amount.y, (-0.5f + Random.value) * amount.z);
        if (cumulative)
        {
            transform.localPosition += jitter;
        }
        else
        {
            transform.localPosition = startPos + jitter;
        }
	}
}
