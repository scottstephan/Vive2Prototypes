using UnityEngine;
using System.Collections;

public class SortingLayer : MonoBehaviour {
    public string layerName;

	// Use this for initialization
	void Start () {
        Renderer myRenderer = GetComponent<Renderer>();
        if( myRenderer != null ) {
            myRenderer.sortingLayerName = layerName;
        }
	}
}

