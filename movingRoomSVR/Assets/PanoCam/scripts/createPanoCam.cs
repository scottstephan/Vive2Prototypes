using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class createPanoCam : MonoBehaviour {
	
	void Update () {
		panoCamScript scr = transform.GetComponent<panoCamScript> ();
		scr.Init();
		
	}
}