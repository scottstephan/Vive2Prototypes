using UnityEngine;
using System;
using System.Collections;

//TODO: re-enable warnings
#pragma warning disable 0414 // private variable assigned but not used.

public class GestureBase : MonoBehaviour {

	[HideInInspector]
	public bool traceActive = false;

	[HideInInspector]
	public Action<float> tpFinishedFunc = null;

    protected bool feedbackIsVisible;

	public virtual void StartTrace( Action<float> finishedFunc ) {
		tpFinishedFunc = finishedFunc;
		traceActive = true;
        feedbackIsVisible = true;
	}

	public virtual void EndTrace() {
		traceActive = false;
        feedbackIsVisible = false;
	}
	
	public virtual void Evaluate() {
	}
	
	// Use this for initialization
    void Start () {
    }
	
    // Update is called once per frame
//  void Update () {
//  }

}
