using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_Teleport : SBBase {
	
    public string moveDesignGroupName;
    public Transform moveTransform;
    public string moveTransformName;
    public Transform teleportToTransform;
    public bool matchOrientation = true;
    public bool relativeToCamera;

    public override bool ContainsDesignGroup( string design_group ) { 
        if (string.IsNullOrEmpty( moveDesignGroupName ) ) {
            return false;
        }
        
        return ( moveDesignGroupName.ToUpper().Equals( design_group ) );
    }
    
    public override void BeginBeat() 
    {
        base.BeginBeat();

        if (teleportToTransform == null)
        {
            Debug.LogError("SB_Teleport " + gameObject.name + " has no teleportToTransform!");
            return;
        }

        CritterInfo c = null;
        Transform t = moveTransform;
        bool bHandled = false;
        bool bApplyRot = matchOrientation;
        Vector3 teleportPos = teleportToTransform.transform.position;

        if (!string.IsNullOrEmpty(moveDesignGroupName))
        {
            c = GetCritterInDesignGroup(moveDesignGroupName);

            if (c != null &&
                c.critterTransform != null)
            {
                t = c.critterTransform;
            }
        }
        else if (moveTransform == null &&
                 !string.IsNullOrEmpty(moveTransformName))
        {
            GameObject moveObj = GameObject.Find (moveTransformName);
            if (moveObj != null)
            {
                t = moveObj.transform;
            }
        }
                
        if (t == CameraManager.singleton.OVRCameraParent)
        {
            if (CameraManager.GetActiveCameraType() == CameraType.OculusFollowCamera)
            {
                OculusFollowCameraMode camFollow = CameraManager.GetActiveCameraMode() as OculusFollowCameraMode;
                if (camFollow != null)
                {
                    camFollow.exitTeleportTransform = t;
                    bHandled = true;
                }
            }
            else
            {
                if (FloatingMenuManager.IsMenuUp())
                {
                    bHandled = true;
                    FloatingMenuManager.TeleportCameraWhenDone = true;
                    FloatingMenuManager.TeleportCameraPosition = teleportPos;

                    if (matchOrientation)
                    {
                        FloatingMenuManager.TeleportCameraRotateY = true;
                        FloatingMenuManager.TeleportCameraYaw = teleportToTransform.transform.eulerAngles.y;
                    }
                }
                else
                {
                    CameraManager.GetCurrentCameraTransform().position = teleportPos;

                    if (matchOrientation)
                    {
                        CameraManager.singleton.SetYRotation(teleportToTransform.transform.eulerAngles.y);
                        bApplyRot = false;
                    }
                }
            }
        }

        if (t != null && 
            !bHandled)
        {
            t.position = teleportPos;

            if (bApplyRot)
            {
                t.rotation = teleportToTransform.transform.rotation;
            }

            if (c != null && c.animBase != null)
            {
                c.animBase.Teleport();
            }

#if UNITY_EDITOR
            Debug.Log("SB_Teleport " + gameObject.name + " teleporting target: " + t.name + " to " + teleportToTransform.name);
#endif
        }
        else
        {
            Debug.LogError("SB_Teleport " + gameObject.name + " foudn no object to teleport!");
        }
    }


	
	public override bool IsComplete()
    {
        return true;
	}			
}
