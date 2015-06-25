using UnityEngine;
using System.Collections;

public class RenderQueue : MonoBehaviour {
	public int offset;
	// Use this for initialization
	void Start () {
		if(GetComponent<Renderer>() != null){
            GetComponent<Renderer>().sharedMaterial.renderQueue += offset;
//            Debug.Log("RenderQueue " + renderer.sharedMaterial.renderQueue);
		}
	}
}
