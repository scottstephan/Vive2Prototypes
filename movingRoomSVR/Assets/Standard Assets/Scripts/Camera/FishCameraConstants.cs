using UnityEngine;
using System.Collections;

public class FishCameraConstants : MonoBehaviour {

	// planar camera constants
	public float planar_Distance = 250.0f;
	public float planar_LateralClampDistance = 40.0f;
	public float planar_InterpolationTime = 7.0f;
	
	public float ropeAndStick_minDistance = 25.0f;
	public float ropeAndStick_desiredDistance = 35.0f;
	public float ropeAndStick_maxDistance = 45.0f;
	public float ropeAndStick_movementTime = 1.0f;
	public float ropeAndStick_heightOffset = 0.0f;
	public float ropeAndStick_lookAtDeadSpace = 10.0f;
	public float ropeAndStick_lookAtDelay = 0.3f;
	public float ropeAndStick_lookAtDistClamp = 20.0f;
	public float ropeAndStick_panelOpenRightOffset = 0.50f;

    public Vector3 oculusFollowOffset = new Vector3(0f, 50f, -100f);

	public float OLDFollow_distance = 50.0f;
	public float OLDFollow_height = 0.0f;
	public float OLDFollow_damping = 2.0f;
	public bool OLDFollow_smoothRotation = true;
	public float OLDFollow_rotationDamping = 4.0f;
}
