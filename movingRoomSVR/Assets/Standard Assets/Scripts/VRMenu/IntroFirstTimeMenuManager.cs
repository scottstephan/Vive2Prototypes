using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OculusIAPAndroid;
using OculusIAPAndroid.Model;


public class IntroFirstTimeMenuManager : MonoBehaviour {

    public float startWaitTime = 0f;
    public float moveSpeed = 90f;
    public float fogDist = 600f;
    public float speedUpTime = 1f;

    StoryBeatsContainer _sb;

    float _oldCullDist;
    Vector3 _moveDir;
    Vector3 _curSpeed;
    enum MenuState
    {
        LogoStartWait,
        LogoMoveForward
    }

    MenuState _state;
    float _stateTimer;

    void Awake ()
    {
        _sb = GetComponentInChildren<StoryBeatsContainer>();
    }

	// Use this for initialization
	void Start ()
    {
    }


	void OnEnable() 
    {
        TravelMenuManager.HasTraveled = false;
        FloatingMenuManager.HideSelector();
        _oldCullDist = GlobalOceanShaderAdjust.Instance.dist;
        GlobalOceanShaderAdjust.SetDist(fogDist);

        CameraManager.IntroCameraExit(); // turn off intro cam state
        CameraManager.SwitchToCamera(CameraType.OculusCamera);

        _state = MenuState.LogoStartWait;
        _stateTimer = 0f;
        _curSpeed = Vector3.zero;
        _moveDir = CameraManager.singleton.rightCamera.transform.forward; // don't use cached for this one move dir get b/c of frame delay 
        _moveDir.y = 0f;        
        _moveDir.Normalize();
        _moveDir *= moveSpeed;

        if (_sb != null)
        {
            StoryManager.Instance.storyBeatsContainer = _sb;
            StoryManager.Instance.storyBeatsContainer.StartSequence();
        }

        App.MetricsManager.Track ("intro_sequence_start");
    }
	
    void OnDisable()
    {
        App.MetricsManager.Track ("intro_sequence_stop");

        // will reset dist and background color to shallow color
        if (!TravelMenuManager.HasTraveled)
        {
            GlobalOceanShaderAdjust.SetDist(_oldCullDist);
        }

        SimInstance.ForceRemoveCrittersIfInstance();
    }

    void Update()
    {
        _stateTimer += Time.deltaTime;

        if (_state == MenuState.LogoStartWait)
        {
            if (_stateTimer > startWaitTime)
            {
                _state = MenuState.LogoMoveForward;
                _stateTimer = 0f;
            }
        }

        if (_state == MenuState.LogoMoveForward)
        {
            if (_stateTimer < speedUpTime)
            {
                _curSpeed = Vector3.Lerp(Vector3.zero, _moveDir, _stateTimer/speedUpTime);
            }
            else
            {
                _curSpeed = _moveDir;
            }

            MoveForward();
        } 
    }

    void MoveForward()
    {
        //AudioManager.PlayInGameAudio(SoundFXID.TravelTransition);
        CameraManager.singleton.ovrCharCtrl.Move (_curSpeed* Time.deltaTime);
    }

    void Stop()
    {
        CameraManager.singleton.ovrRB.isKinematic = true;
        CameraManager.singleton.ovrRB.velocity = Vector3.zero;
    }
}
