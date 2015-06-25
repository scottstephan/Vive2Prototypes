using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class SB_ActivateTravelObject : SBBase {

//	Tweener _tween;
	Vector3 _localScale;
    bool resetActive;

	public GameObject gameObjectToActivate = null;
	public float scaleTime = .5f;
	public bool deactivateObject = false;
	
    public override void Start ()
    {
        base.Start ();
        
        if (gameObjectToActivate != null)
        {
            resetActive = gameObjectToActivate.activeSelf;
        }
    }
    
    public override void Reset ()
    {
        base.Reset ();
        
        if (gameObjectToActivate != null)
        {
            gameObjectToActivate.SetActive(resetActive);
        }
    }

	public override void BeginBeat() 
    {
		base.BeginBeat();
		
		if (gameObjectToActivate != null)
        {
            bool bTour = App.UserManager.educationalMode == 0;

            if (bTour)
            {
                gameObjectToActivate.SetActive (false);
            }
            else
            {
                gameObjectToActivate.SetActive (!deactivateObject);
                _localScale = gameObjectToActivate.transform.localScale;
                gameObjectToActivate.transform.localScale = Vector3.zero;               
                HOTween.To(gameObjectToActivate.transform, scaleTime, new TweenParms().Prop("localScale", _localScale));
            }

			gameObjectToActivate.GetComponent<Collider>().enabled = true;

#if UNITY_EDITOR           
            Debug.Log("SB_ActivateObject " + gameObject.name + (deactivateObject ? " deactivating " : " activating ") + gameObjectToActivate.name);
#endif
        }
        else 
        {
#if UNITY_EDITOR
            Debug.LogError("SB_ActivateObject " + gameObject.name + " has NULL gameObject to " + (deactivateObject ? "deactivate" : "activate"));
#endif
        }
	}
	
	public override bool IsComplete()
    { 
        return true;
    }	
}
