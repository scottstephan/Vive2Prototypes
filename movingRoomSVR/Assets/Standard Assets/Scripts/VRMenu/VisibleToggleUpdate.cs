using UnityEngine;
using System.Collections;

public class VisibleToggleUpdate : MonoBehaviour {

	public ToggleButton tb;

	void OnBecameVisible() {

		Debug.Log("BECAME VISIBLE TOGGLE!");

		tb.DetectPref();
	}
}
