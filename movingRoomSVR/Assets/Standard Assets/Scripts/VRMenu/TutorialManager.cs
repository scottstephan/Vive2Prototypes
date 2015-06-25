using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour {
	
	bool _inputSelect;
//	FloatingMenuManager _fmm;

	static ScreenMode _currentMode;
	static GameObject _currentScreen;
	
	public enum ScreenMode
    {
        Move = 0,
        Upsell = 1,
        Badges = 2,
        Num = 3
    }   

	public GameObject[] tutorialScreens;
	static TutorialManager _singleton;

	static bool _lastTravel;
	static float _travelTimestamp;
//	static float _travelTimeMax = 2f;
	static float _minShow = 100f;
	static float _showTime;

	static float _showTimestamp;
//	static bool _showing;
	static int _tutorialStep;
//    static int _lastTutorialStep;

//    static bool bShowedMoveTutorial = false;

	static ScreenMode[] _tutorialSteps;

	void Awake() {

		_singleton = this;
//		_fmm = this.transform.parent.gameObject.GetComponent<FloatingMenuManager>();
	}

	static void SetupTutorialArray() {

//		_showing = false;
		_tutorialStep = 0;

		_tutorialSteps = new ScreenMode[_singleton.tutorialScreens.Length + 1];

        _tutorialSteps[0] = ScreenMode.Move;
        _tutorialSteps[1] = ScreenMode.Num;
//		_tutorialSteps[1] = ScreenMode.Upsell;
//        _tutorialSteps[2] = ScreenMode.Badges;
//        _tutorialSteps[3] = ScreenMode.Num;
	}

	// Use this for initialization
	void Start () {
	}

	public static void SetPage(ScreenMode mode) {

		if (_currentScreen)
			_currentScreen.SetActive(false);

		if ((int)mode >= _singleton.tutorialScreens.Length)
			return;

		_currentScreen = _singleton.tutorialScreens[(int)mode];

		if (_currentScreen)
			_currentScreen.SetActive(true);
	}

	public static void StartTutorial() {

		SetupTutorialArray();

		_showTimestamp = Time.time;
//		_showing = true;

        _tutorialStep = (int)ScreenMode.Move;

//        if (!bShowedMoveTutorial)
//        {
//            _tutorialStep = (int)ScreenMode.Move;
//            bShowedMoveTutorial = true;
//        }
//        else
//        {
//            _tutorialStep = _lastTutorialStep == 1 ? 2 : 1;
//        }

		SetPage(_tutorialSteps[_tutorialStep]);
		FloatingMenuManager.ShowTutorial(_tutorialSteps[_tutorialStep]);
		_showTime = _minShow;
//        _lastTutorialStep = _tutorialStep;
	}

	void Update() {

		if (_tutorialSteps == null)
			return;

		//if the tutorial is over, skip
		if (_tutorialStep >= _tutorialSteps.Length)
			return;

		//if it's time to move to the next step in the tutorial
		if ((Time.time - _showTimestamp) >= _showTime) {

			_currentScreen.SetActive(false);

			if (++_tutorialStep < _tutorialSteps.Length) {

				_currentScreen.SetActive(false);

				//1 second of blank time
				if (_tutorialSteps[_tutorialStep] == ScreenMode.Num) {
					_currentScreen.SetActive(false);
					_showTime = 1f;
				}
				else { //3 seconds for normal
					SetPage(_tutorialSteps[_tutorialStep]);
					_currentScreen.SetActive(true);
					_showTime = _minShow;
				}

				_showTimestamp = Time.time;
			}
		}
	}

	void OnEnable() {
		
		if (_currentScreen)
			_currentScreen.SetActive(true);
	}

	void UpdateInput() {

		_inputSelect = false;

		if (Input.GetKeyUp(KeyCode.Mouse0))
			_inputSelect = true;
	}

	bool ProcessInput() {

		if (_inputSelect)
			return true;

		return false;
	}
}
