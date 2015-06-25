using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public enum ScaleEffectState {
    None,
    Up,
    Peak,
    Down
}

public class CameraFacingScaleEffect : MonoBehaviour {

    private Transform myTransform = null;
//    private float localScale = 0f;

    public float scalePeak = 4f;
    public float scaleStart = 0f;
    public float scaleEnd = 0f;

    public float upTime = 0.8f;
    public float peakTime = 0.2f;
    public float downTime = 0.33f;

    public ScaleEffectState state = ScaleEffectState.None;
    public bool autoDeactivate = true;

    private Tweener tween;

    public void Reset() {
        myTransform.localScale = Vector3.one * scaleStart;
        state = ScaleEffectState.None;

        if( tween != null ) {
            tween.Kill();
            tween = null;
        }
    }

	// Use this for initialization
	void Start () {
        myTransform = transform;
        Reset();
	}
	
	// Update is called once per frame
	void Update () {
	    if( state == ScaleEffectState.None ) {

        }
	}
}
