using UnityEngine;
using System.Collections;

public class fps : MonoBehaviour {
	
	public GUIText textObj;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(textObj) textObj.text = "fdsfsdfsd";
	
	}
}
