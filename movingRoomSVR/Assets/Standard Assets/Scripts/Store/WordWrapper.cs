using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WordWrapper : MonoBehaviour {

	public float wantedWidth;
	TextMesh textMesh;

	void Awake() {

		textMesh = this.gameObject.GetComponent<TextMesh>();

	}

	// Use this for initialization
	void Start () {

		Wrap();
	}
	
	void OnEnable() {

		Wrap();
	}

	public void Wrap() {

		textMesh = this.gameObject.GetComponent<TextMesh>();

		//process linefeeds
		textMesh.text = textMesh.text.Replace("\\n","\n");

		TextSize ts = new TextSize(textMesh);
		ts.FitToWidth(wantedWidth);
	}

}
