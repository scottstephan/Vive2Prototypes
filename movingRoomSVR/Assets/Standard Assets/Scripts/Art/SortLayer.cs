using UnityEngine;
using System.Collections;

public class SortLayer : MonoBehaviour {

    public string sortingLayer;
	void Start () {
        if(GetComponent<Renderer>() != null)
            GetComponent<Renderer>().sortingLayerName = sortingLayer;

	}
	

}
