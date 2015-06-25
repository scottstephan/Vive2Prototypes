using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;


public class StoryManager : MonoBehaviour {
	
	private static StoryManager singleton = null;
	public static StoryManager Instance {
		get {
			return singleton;
		}
	}

    public bool DEBUG_BEATS = false;

    [HideInInspector]
	public string storyState = "NONE";
	
	[HideInInspector]
	public StoryBeatsContainer storyBeatsContainer;

	[HideInInspector]
	public bool storyBeatsActive = false;
	
	void Awake() {
		singleton = this;
#if !UNITY_EDITOR
        DEBUG_BEATS = false;
#endif
    }
	
	// Use this for initialization
	void Start () {
	}

	public void Reset() {
		if( storyBeatsContainer != null ) {
			storyBeatsContainer.StartSequence();
			storyBeatsActive = true;
		}		
	}

	// Update is called once per frame
	void Update() {
		if( CameraManager.IsInIntroCamMode() ||
           CameraManager.IsInTravelCamera()) {
			return;
		}

		if( storyBeatsActive && storyBeatsContainer != null ) {
			storyBeatsContainer.UpdateSB();
		}
		else if( !storyBeatsActive && storyBeatsContainer != null ) {
			storyBeatsContainer.StartSequence();
			storyBeatsActive = true;
		}


	}
}
