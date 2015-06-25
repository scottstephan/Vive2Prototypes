using UnityEngine;
using System.Collections;

public class SB_ActivateObject : SBBase {
	
	public GameObject gameObjectToActivate = null;
	
	public bool deactivateObject = false;
	
    bool resetActiveInit;
    bool resetActive;

    public override void Start ()
    {
        base.Start ();

        if (gameObjectToActivate != null)
        {
            resetActive = gameObjectToActivate.activeSelf;
            resetActiveInit = true;
        }
    }

    public override void Reset ()
    {
        base.Reset ();

        if (gameObjectToActivate != null &&
            resetActiveInit)
        {
            gameObjectToActivate.SetActive(resetActive);
        }
    }

	public override void BeginBeat() 
    {
		base.BeginBeat();
		
		if (gameObjectToActivate != null)
        {
            gameObjectToActivate.SetActive (!deactivateObject);
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
