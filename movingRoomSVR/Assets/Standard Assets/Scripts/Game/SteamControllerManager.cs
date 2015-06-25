using UnityEngine;
using System.Collections;
using System.IO;

public class SteamControllerManager : MonoBehaviour 
{
    bool steamVRinitialized = false;
    bool buttonTrackersBound;
    const int NUM_CONTROLLERS = 2;

    public struct ButtonTrackerBinding
    {
        public uint index;
//        public SteamVR_Controller.SteamControllerState_t state;
        public Transform tracker;
        public bool bound;
    }

    ButtonTrackerBinding [] controllers = new ButtonTrackerBinding[NUM_CONTROLLERS];

    SteamVR_TrackedObject [] tracked;
    float[] DEBUGdot;

    public float dotThreshold = 0.8f;

    GameObject instructions;

    static private SteamControllerManager instance;

    void OnEnable()
    {
        steamVRinitialized = SteamVR.instance != null;

        instance = this;

        for (int i = 0; i< NUM_CONTROLLERS; ++i)
        {
            controllers[i] = new ButtonTrackerBinding();
//            controllers[i].state = new SteamVR_Controller.SteamControllerState_t();
        }

        tracked = FindObjectsOfType<SteamVR_TrackedObject>();
        DEBUGdot = new float[tracked.Length];
    }

    void OnDisable()
    {
    }

    void MaybeBind()
    {
		if (!steamVRinitialized)
			return;
        if (buttonTrackersBound)
            return;
        ////////////////////////

        // find correct tracked object based on look direction
        if (tracked != null)
        {
            int controllerWithButtonDown = -1;
            if (!controllers[0].bound)
            {
                for (uint c = 0; c < NUM_CONTROLLERS; ++c)
                {
//                    SteamVR_Controller.SteamControllerState_t state = new SteamVR_Controller.SteamControllerState_t();
//                    if (SteamVR_Controller.GetControllerState(c, ref state))
                    //{
                    //    if ((state.ulButtons & SteamVR_Controller.STEAM_RIGHT_TRIGGER_MASK) > 0
                    //        || (state.ulButtons & SteamVR_Controller.STEAM_BUTTON_RIGHTPAD_CLICKED_MASK) > 0)
                    //    {
                    //        controllerWithButtonDown = (int)c;
                    //    }
                    //}
                }

                int best = -1;
                float bestDot = -1;
                for (int i = 0; i < tracked.Length; ++i)
                {
                    // only process if real data present
                    if (tracked[i].index != SteamVR_TrackedObject.EIndex.Hmd && tracked[i].transform.localPosition != Vector3.zero)
                    {
                        if (instructions == null)
                        {
                            instructions = new GameObject("instructions", typeof(TextMesh));
                            TextMesh tm = instructions.GetComponent<TextMesh>();
                            tm.text = "Look at me and press any button.";
                            tm.characterSize = 0.02f * transform.lossyScale.x;
                            tm.color = Color.red;
                            tm.transform.parent = tracked[i].transform;
                            tm.transform.localPosition = Vector3.zero;
                        }

                        float dot = Vector3.Dot(transform.forward, (tracked[i].transform.position - transform.position).normalized);
                        Debug.DrawLine(transform.position, tracked[i].transform.position, new Color(0.5f + 0.5f * dot, 0f, 0.5f + 0.5f * dot, 1f));
                        Debug.DrawRay(transform.position, transform.forward, Color.magenta);

                        DEBUGdot[i] = dot;

                        if (dot > dotThreshold && dot >= bestDot)
                        {
                            bestDot = dot;
                            best = i;
                        }
                    }
                }

                if (controllerWithButtonDown >= 0)
                {
                    if (best >= 0)
                    {
                        // associate button input with tracked transform
                        controllers[0].tracker = tracked[best].transform;
                        controllers[0].index = (uint)controllerWithButtonDown;
                        controllers[0].bound = true;

                        buttonTrackersBound = true;

                        if (instructions != null)
                        {
                            GameObject.Destroy(instructions);
                            instructions = null;
                        }
                    }
                }
            }

            // once we've associated one controller, we can do the other
            if (controllers[0].bound)
            {
                for (int i = 0; i < tracked.Length; ++i)
                {
                    // don't allow already associated tracker
                    if (tracked[i].index != SteamVR_TrackedObject.EIndex.Hmd                         
                        && tracked[i].transform != controllers[0].tracker 
                        && tracked[i].transform.localPosition != Vector3.zero)
                    {
                        // associate button input with tracked transform
                        controllers[1].tracker = tracked[i].transform;
                        controllers[1].index = 1 - controllers[0].index;
                        controllers[1].bound = true;
                    }
                }

                buttonTrackersBound = true;
            }
        }
    }

	// Update is called once per frame
	void Update () 
    {
        MaybeBind();

        if (steamVRinitialized)
        {
//            SteamVR.SteamAPI_RunCallbacks();

            for (uint i = 0; i < NUM_CONTROLLERS; ++i)
            {
                if (controllers[i].bound)
                {
//                    SteamVR_Controller.SteamControllerState_t state = controllers[i].state;
//                    if (SteamVR_Controller.GetControllerState(controllers[i].index, ref state))
                    {
//                        controllers[i].state = state;
                        //Debug.Log(string.Format("Controller {0}: PacketNum: {1} Buttons: {2} LeftPad: {3},{4} RightPad: {5},{6}",
                        //    controllers[i].index, state.unPacketNum, state.ulButtons, state.sLeftPadX, state.sLeftPadY, state.sRightPadX, state.sRightPadY));
                    }
                }
            }
        }
	}

    // debug rendering hack
    //void OnGUI()
    //{
    //    Rect r = new Rect();
    //    for (int i = 0; i < tracked.Length; ++i)
    //    {
    //        if (tracked[i].index != SteamVR_TrackedObject.EIndex.Hmd && tracked[i].transform.localPosition != Vector3.zero)
    //        {
    //            Debug.Log(string.Format("{0}: {1}", i, tracked[i].transform.position));
    //            Vector3 pos = Camera.main.WorldToScreenPoint(tracked[i].transform.position);
    //            if (pos.z > 0f)
    //            {
    //                r.x = pos.x;
    //                r.y = Screen.height - pos.y;
    //                r.width = 200f;
    //                r.height = 100f;
    //                GUI.Label(r, DEBUGdot[i].ToString());
    //            }
    //        }
    //    }
    //}

    void LateUpdate()
    {
        if (instructions != null)
        {
            instructions.transform.rotation = Quaternion.LookRotation((instructions.transform.position - transform.position).normalized, Vector3.up);
        }
    }

    static public bool Initialized(uint idx)
    {
        if (idx < NUM_CONTROLLERS)
        {
            return instance.controllers[idx].bound;
        }

        return false;
    }

    static public bool GetButton(uint idx, ulong button)
    {
        if (idx < NUM_CONTROLLERS)
        {
            if (instance.controllers[idx].bound)
            {
                return false;
//                return (instance.controllers[idx].state.ulButtons & button) > 0;
            }
        }

        Debug.LogWarning(string.Format("Controller {0} not found or intialized.", idx));
        return false;
    }

    static public bool AnyButton(uint idx)
    {
        if (idx < NUM_CONTROLLERS)
        {
            if (instance.controllers[idx].bound)
            {
                return false;
                //SteamVR_Controller.SteamControllerState_t state = instance.controllers[idx].state;
                //return ((state.ulButtons & SteamVR_Controller.STEAM_RIGHT_TRIGGER_MASK) > 0
                //    || (state.ulButtons & SteamVR_Controller.STEAM_RIGHT_BUMPER_MASK) > 0
                //    || (state.ulButtons & SteamVR_Controller.STEAM_BUTTON_RIGHTPAD_CLICKED_MASK) > 0);
            }
        }
        return false;
    }

    //static public SteamVR_Controller.SteamControllerState_t GetState(uint idx)
    //{
    //    if (idx < NUM_CONTROLLERS)
    //    {
    //        if (instance.controllers[idx].bound)
    //        {
    //            return instance.controllers[idx].state;
    //        }
    //    }

    //    Debug.LogWarning(string.Format("Controller {0} not found or intialized.", idx));
    //    return new SteamVR_Controller.SteamControllerState_t();
    //}

    static public Transform GetTransform(uint idx)
    {
        if (idx < NUM_CONTROLLERS)
        {
            if (instance.controllers[idx].bound)
            {
                return instance.controllers[idx].tracker;
            }
        }

        Debug.LogWarning(string.Format("Controller {0} not found or intialized.", idx));
        return null;
    }
}
