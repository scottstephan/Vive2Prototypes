using UnityEngine;
using System.Collections;

[System.Serializable]
public enum RepeatType {
	None,
	Cooldown
}

[System.Serializable]
public enum TriggerType {
	OnEnter,
	OnExit,
	IsInside,
	IsOutside
}

[System.Serializable]
public enum TriggerMode {
    AllModes,
    TourModeOnly,
    ExploreModeOnly,
}

public class StoryBeatTrigger : MonoBehaviour {

	public SBBase[] triggerBeats;

	public RepeatType repeatType = RepeatType.None;
	public TriggerType triggerType = TriggerType.OnEnter;
    public TriggerMode triggerMode = TriggerMode.AllModes;
	public Transform triggerTransform = null;	// if null gets set to the player.
	
	private Collider myCollider = null;
	private Vector3 colliderCenter;
	private bool isInside = false;
	private bool triggered = false;

	public float coolDownTime = 10f;
	private float coolDownTimer = 0f;

	public void Reset() {
//		Debug.Log("RESETING STORY BEAT TRIGGER ");
		if( triggerBeats != null && triggerBeats.Length > 0 ) {
            for( int i = 0; i < triggerBeats.Length; i++ ) {
                SBBase sb = triggerBeats[i];
				if( sb != null ) {
					sb.Reset();
				}
			}
		}

        // allow triggers to re-trigger on reset
        isInside = false;
		triggered = false;
        coolDownTimer = 0f;
	}

	// Use this for initialization
	void Start () 
    {
        if( GetComponent<Collider>() == null ) 
        {
			Debug.LogError("StoryBeatTrigger " + gameObject.name + " requires a Collider to function. Turning Self Off.");
			gameObject.SetActive( false );
			return;
		}
	}

    void OnEnable()
    {
        myCollider = GetComponent<Collider>();
		colliderCenter = myCollider.bounds.center;

        Reset();
    }

	void TriggerMe()
    {
#if UNITY_EDITOR
        Debug.Log ("StoryBeatTrigger " + gameObject.name + " " + triggerType + " object: " + GetTestTransform().name);
#endif
		if( StoryManager.Instance != null 
		   && StoryManager.Instance.storyBeatsContainer != null )
        {
			StoryManager.Instance.storyBeatsContainer.BeginBeats( triggerBeats );
		}

		coolDownTimer = coolDownTime;
		triggered = true;
	}

    Transform GetTestTransform()
    {
        Transform testTransform = triggerTransform;
        
        if( testTransform == null ) 
        {
            testTransform = CameraManager.GetCurrentCameraTransform();
        }

        return testTransform;
    }

	// Update is called once per frame
	void Update ()
    {	
        if ((triggerMode == TriggerMode.ExploreModeOnly && App.UserManager.educationalMode == 0) ||
            (triggerMode == TriggerMode.TourModeOnly && App.UserManager.educationalMode == 1))
        {
            return;
        }

        Transform testTransform = GetTestTransform();

		if( testTransform == null ) 
		{
			return;
		}
		
		if( !triggered ) 
        {
			bool is_inside_now = CollisionHelpers.IsInside( myCollider, testTransform.position, colliderCenter );
			switch( triggerType ) {
				case TriggerType.OnEnter:
				{
					if( !isInside && is_inside_now ) {
						TriggerMe();
					}
					break;
				}
				case TriggerType.OnExit:
				{
					if( !isInside && is_inside_now ) {
						TriggerMe();
					}
					break;
				}
				case TriggerType.IsInside:
				{
					if( is_inside_now ) {
						TriggerMe();
					}
					break;
				}
				case TriggerType.IsOutside:
				{
					if( !is_inside_now ) {
						TriggerMe();
					}
					break;
				}
			}
			isInside = is_inside_now;
		}
		
		if( triggered ) {
			switch( repeatType ) {
				case RepeatType.Cooldown:
				{
					coolDownTimer -= Time.deltaTime;
					if( coolDownTimer <= 0f ) 
                    {
                        bool wasInside = isInside;
						Reset();
                        isInside = wasInside;
					}
					break;
				}
				case RepeatType.None:
				default:
				{
					break;
				}
			}
		}
	}
}
