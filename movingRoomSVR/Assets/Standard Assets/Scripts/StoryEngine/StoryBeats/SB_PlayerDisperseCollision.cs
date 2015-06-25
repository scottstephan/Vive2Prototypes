using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_PlayerDisperseCollision : SBBase {
	
    public float collisionRadius;

    public override void BeginBeat() 
    {
		SphereCollider sphere = CameraManager.singleton.GetDispersionCollider();

        if (sphere != null)
        {
            sphere.radius = collisionRadius;
#if UNITY_EDITOR
            Debug.Log("SB_PlayerDisperseCollision " + gameObject.name + " New Radius: " + collisionRadius);
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("SB_PlayerDisperseCollision OVRCameraController has no disperse SphereCollider");
#endif
        }

        base.BeginBeat();
    }

	public override bool IsComplete()
    {
        return true;
	}			
}
