using UnityEngine;
using System.Collections;

public class SB_Visible : SBBase {
	
	public GameObject gameObjectToToggleVisible = null;	
	public bool isVisible = false;
    public bool allChildren = true;

	public override void BeginBeat() 
    {
		base.BeginBeat();

        SetVisible(isVisible);
#if UNITY_EDITOR           
        if (gameObjectToToggleVisible != null)
        {
            Debug.Log("SB_ActivateObject " + gameObject.name + (isVisible ? " setting visible " : " setting NOT visible ") + gameObjectToToggleVisible.name);
        }
        else 
        {
            Debug.LogError("SB_ActivateObject " + gameObject.name + " has NULL gameObject to " + (isVisible ? "set visible" : "set not visible"));
        }
#endif
    }

    void SetVisible(bool b)
    {
        if (gameObjectToToggleVisible == null)
        {
            return;
        }

        Renderer r = gameObjectToToggleVisible.GetComponent<Renderer>();

        if (r != null)
        {
            r.enabled = b;
        }
        
        if (allChildren)
        {
            Renderer[] children = gameObjectToToggleVisible.GetComponentsInChildren<Renderer>();
            
            if (children != null)
            {
                for (int i=0; i<children.Length; ++i)
                {
                    r = children[i];
                    if (r != null)
                    {
                        r.enabled = b;
                    }
                }
            }               
        }
    }

    public override void Reset ()
    {
        base.Reset ();
        SetVisible(true);
    }
	
	public override bool IsComplete()
    { 
        return true;
    }	
}
