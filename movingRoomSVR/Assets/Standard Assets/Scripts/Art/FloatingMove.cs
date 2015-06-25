using UnityEngine;
using System.Collections;

public class FloatingMove : MonoBehaviour {
    public float waveHeight = 0.05f; 
	public float waveSpeed = 1.0f; 
	float seed = 40.0f;
    // Use this for initialization
	void Start () {
		seed = Random.Range(100,5000);
	}
	
	// Update is called once per frame
	void Update () {
		
  		float diffHeight = Mathf.Sin((Time.time+seed)*waveSpeed)*waveHeight;
		transform.Translate(0,diffHeight,0);
		//Debug.Log(transform +": "+transform.position);
    }
}
