using UnityEngine;
using System.Collections;

// select between OculusSDK and SteamVR
// should always be set first in script order execution.
public class ChooseVRSDK : MonoBehaviour 
{
    public enum SDKType
    {
        OCULUS,
        STEAM
    }

    public SDKType sdkType;

    static public bool SteamActive = false;

	void Awake() 
    {
        CameraManager camMgr = FindObjectOfType<CameraManager>(); // can't use singleton because not setup yet (this Awake() should happen before everything else)

        bool HMDActive = false;
        HMDActive = camMgr.startupOculusActive;
#if UNITY_EDITOR
        HMDActive = camMgr.startupEditorOculusActive;
#endif
        if (HMDActive)
        {
            camMgr.mainCamera.gameObject.SetActive(false);

            SteamVR_Camera svrcam = camMgr.OVRCameraParent.GetComponentInChildren<SteamVR_Camera>();
            bool forceSteam = SteamVROverride();
            if (svrcam == null || (sdkType == SDKType.OCULUS && !forceSteam))
            {
                if (svrcam == null)
                    Debug.Log("Could not find SteamVR component. Oculus SDK will be used.");
                else
                    Debug.Log("Oculus SDK chosen.  SteamVR not enabled in ChooseVRSDK component and not forced via command line option -steamVR.");

                camMgr.leftCamera.gameObject.SetActive(true);
                camMgr.rightCamera.gameObject.SetActive(true);

                if (svrcam != null)
                {
                    if (svrcam.head != null && svrcam.head.gameObject != null)
                    {
                        DestroyImmediate(svrcam.head.gameObject);
                    }
                }

                SteamVRScaler scaler = camMgr.GetComponentInChildren<SteamVRScaler>();
                if (scaler != null)
                {
                    DestroyImmediate(scaler);
                }

                GameObject steamStatus = GameObject.Find("[Status]");
                if (steamStatus != null)
                {
                    DestroyImmediate(steamStatus);
                }
            }
            else
            {
                SteamActive = true;
                svrcam.enabled = true;

				// BV: cannot nuke the cameracontroller because it is cached in the CameraManager singleton and various scripts access it
                //OVRCameraController cc = camMgr.OVRCameraParent.GetComponentInChildren<OVRCameraController>();
                //if (cc != null)
                //    DestroyImmediate(cc);

                OVRDevice dev = camMgr.OVRCameraParent.GetComponentInChildren<OVRDevice>();
                if (dev != null)
					DestroyImmediate(dev);

                OVRCamera[] ovrCams = camMgr.OVRCameraParent.GetComponentsInChildren<OVRCamera>();
                if (ovrCams != null)
                {
                    for (int i = ovrCams.Length - 1; i >= 0; --i)
                    {
						DestroyImmediate(ovrCams[i]);
                    }
                }

                OVRMainMenu menu = camMgr.OVRCameraParent.GetComponentInChildren<OVRMainMenu>();
                if (menu != null)
                {
                    DestroyImmediate(menu);
                }

                // don't nuke yet bc Steam cameras haven't been created and need to grab their data
                camMgr.leftCamera.gameObject.SetActive(false);
                camMgr.rightCamera.gameObject.SetActive(false);

                OVRCameraController occ = camMgr.OVRCameraParent.GetComponentInChildren<OVRCameraController>();
                if (occ != null)
                {
                    occ.enabled = false;
                }

                // BV: don't migrate parameters, they aren't compatible with the way Steam camera works
                //svrcam.camera.CopyFrom(camMgr.leftCamera);

                OculusScaler scaler = camMgr.GetComponentInChildren<OculusScaler>();
                if (scaler != null)
                {
                    DestroyImmediate(scaler);
                }

                // emulate height of a person if headset is not being used
                SteamVR vr = SteamVR.instance;
                if (vr == null || vr.hmd == null)
				{
#if UNITY_EDITOR
                    camMgr.OVRCameraParent.transform.position = camMgr.OVRCameraParent.transform.position + new Vector3(0, 175, 0);
#endif
				}
				else
				{
					Component mouseCtrl = camMgr.OVRCameraParent.GetComponent("PanWithMouse");
					if (mouseCtrl != null)
					{
						DestroyImmediate(mouseCtrl);
					}                  
				}
            }
        }
        else
        {
            // non-VR config
            camMgr.mainCamera.gameObject.SetActive(true);

            camMgr.leftCamera.gameObject.SetActive(false);
            camMgr.rightCamera.gameObject.SetActive(false);

            if (camMgr.OVRCameraParent != null)
                camMgr.OVRCameraParent.gameObject.SetActive(false);
        }
	}

    public static bool SteamVROverride()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; ++i)
        {
            if (string.Compare(args[i], "-steamvr", true) == 0)
            {
                return true;
            }
        }

        return false;
    }
}
