
using UnityEngine;
using System.Collections;

public class EyeControllerData : MonoBehaviour {
    public enum EyeForwardAxis
    {
        ZForward,
        XForward
    }

    public bool OFF;

	public Transform rootNode;

	public Transform lookAt;

	public Transform leftEye;
	public Transform rightEye;

    public EyeForwardAxis forwardAxis = EyeForwardAxis.ZForward;
	public float maxYawDegrees = 60f;
	public float maxPitchDegrees = 60f;
	
	[HideInInspector]
	public Quaternion prevRotation;
	[HideInInspector]
	public Quaternion prevLeftRotation;
	[HideInInspector]
	public Quaternion prevRightRotation;
	
	[HideInInspector]
	public Quaternion ogLeftLocalRotation;
	[HideInInspector]
	public Quaternion ogLeftRotation;
	[HideInInspector]
	public Quaternion ogLeftLocalInvRotation;
	[HideInInspector]
	public Quaternion ogRightLocalRotation;
	[HideInInspector]
	public Quaternion ogRightRotation;
	[HideInInspector]
	public Quaternion ogRightLocalInvRotation;

	[HideInInspector]
	public bool runtimeInitialized;

    [HideInInspector]
    public bool useDirection;
    [HideInInspector]
    public Vector3 lookAtDirectionLeft;
    [HideInInspector]
    public Vector3 lookAtDirectionRight;
}
 