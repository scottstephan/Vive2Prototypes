using UnityEngine;
using System.Collections;

//
// DEPRECATED! 3/21/2012!
// NEW ANIMATION HIERARCHY HAS BEEN IMPLEMENTED.
// THIS PARTICULAR CLASS HAS BEEN KEPT FOR BACKWARDS COMPATIBILITY 
// AND GETS REDIRECTED INTO THE OGFishAnimation CLASS AT LOAD TIME.
//

public class FishAnimationData : MonoBehaviour {
	public AnimationClip forwardSwimClip;
	public AnimationClip forwardSwimSlowClip;
	public AnimationClip forwardSwimFastClip;
	public AnimationClip hoverClip;
	public AnimationClip breatheClip;
	public Transform breatheTransform;
	
	public AnimationClip biteClip;
	
	[HideInInspector]
	public float feedTime = 0f;
	[HideInInspector]
	public bool isFeeding = false;
	
	public float feedTimeMin = 1.5f;
	public float feedTimeMax = 3.5f;
	
	public float breatheTimeMin = 1.5f;
	public float breatheTimeMax = 3.5f;
	[HideInInspector]
	public float breatheTime = -1.0f;	
	
	[HideInInspector]
	public Vector3 lastTrackingPosition;
	[HideInInspector]
	public float anim_trackingSpeed = 0.0f;	
	[HideInInspector]
	public float animNotOkToSwitch = 0.0f;
	
	
	[HideInInspector]
	public bool hasAllClips;
	[HideInInspector]
	public bool hasFeedClip;
	[HideInInspector]
	public bool hasBreatheClip;
}

