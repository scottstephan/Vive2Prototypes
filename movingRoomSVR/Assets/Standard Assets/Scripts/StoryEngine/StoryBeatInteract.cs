using UnityEngine;
using System.Collections;


public class StoryBeatInteract : MonoBehaviour {

    [System.Serializable]
    public enum RepeatInteractType 
    {
        None,
        Immediate,
        Cooldown
    }

    public SBBase[] triggerBeats;
    public RepeatInteractType repeatType = RepeatInteractType.Immediate;
    public string designGroupName;

    public float coolDownTime = 10f;
    private float coolDownTimer = 0f;

    private FxPortal portal;

    private WemoItemData myItemData;

	public void Reset() {
//		Debug.Log("RESETING STORY BEAT Interact ");
		if( triggerBeats != null && triggerBeats.Length > 0 ) {
            for( int i = 0; i < triggerBeats.Length; i++ ) {
                SBBase sb = triggerBeats[i];
				if( sb != null ) {
					sb.Reset();
				}
			}
		}
	}

	// Use this for initialization
	void Start () 
    {
        myItemData = GetComponent<WemoItemData>();
        portal = GetComponent<FxPortal>();

        if( myItemData != null ) 
        {
            myItemData.SetVRInteractable(true, TriggerInteract);
            gameObject.layer = 15;
        }
        else if (portal != null)
        {
            gameObject.layer = 14;
        }
        else
        {
            Debug.LogError("StoryBeatInteract Requires a WemoItemData or FXPortal to function. Turning Self Off.");
			gameObject.SetActive( false );
			return;
		}
	}

    void OnEnable()
    {
        Reset();
    }

	public void TriggerInteract()
    {
#if UNITY_EDITOR
        Debug.Log ("StoryBeatInteract " + gameObject.name + " clicked.");
#endif
		if( StoryManager.Instance != null 
		   && StoryManager.Instance.storyBeatsContainer != null )
        {
			StoryManager.Instance.storyBeatsContainer.BeginBeats( triggerBeats );
		}

        if (repeatType == RepeatInteractType.Cooldown)
        {
            coolDownTimer = coolDownTime;
        }
        else if (repeatType == RepeatInteractType.Immediate)
        {
            coolDownTimer = 0.001f;
        }

        if( myItemData == null )
        {
            CritterInfo c = SimInstance.Instance.GetCritterInDesignGroup(designGroupName);
            if (c != null)
            {
                OculusFollowCameraMode.singleton.SetTarget(c);
                CameraManager.SwitchToCamera(CameraType.OculusFollowCamera);                
            }
            else
            {
                Debug.LogError("StoryBeatInteract " + gameObject.name + " found no critter in " + designGroupName + "!");
            }
        }         
    }

    public void Update()
    {
        if (coolDownTimer > 0f)
        {
            coolDownTimer -= Time.deltaTime;
            if (coolDownTimer <= 0f &&
                myItemData != null)
            {
                myItemData.SetVRInteractable(true, TriggerInteract);
            }
        }

        if (portal != null &&
            portal.isTriggered)
        {
            TriggerInteract();
            portal.isTriggered = false;
        }

    }
}
