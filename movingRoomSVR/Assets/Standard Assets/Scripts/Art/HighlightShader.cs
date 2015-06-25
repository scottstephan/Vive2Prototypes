using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class HighlightShader : MonoBehaviour {

    public float highlightOnTime = 1f;
    public float highlightOffTime = 1f;
    private Material mat;
	private float highlightTarget;
	private float highlight;

    Renderer[] childRenderers;

	void Start () 
    {
        if (gameObject.GetComponent<Renderer>() != null)
        {
            mat = gameObject.GetComponent<Renderer>().material;
        }

        childRenderers = gameObject.GetComponentsInChildren<Renderer>();
	}

	void Update(){
//		if(Input.GetKeyDown(KeyCode.RightAlt))
//			Highlight(0f);
//		if(Input.GetKeyDown(KeyCode.LeftAlt))
///			Highlight(1f);

        bool bChanged = false;
		if(highlight < highlightTarget)
        {
            bChanged = true;
            highlight += Time.deltaTime * (1f/highlightOnTime);
            if (highlight > highlightTarget)
            {
                highlight = highlightTarget;
            }
        }
        else if(highlight > highlightTarget)
        {
            bChanged = true;
            highlight -= Time.deltaTime * (1f/highlightOffTime);
            if (highlight < highlightTarget)
            {
                highlight = highlightTarget;
            }
        }

		if (bChanged)
        {
            SetHighlight();
        }
	}

    void SetHighlight()
    {
        if (mat != null)
        {
            mat.SetFloat("_Highlight", highlight);
        }
        
        if (childRenderers != null)
        {
            for (int i=0; i<childRenderers.Length; ++i)
            {
                Renderer r = childRenderers[i];
                if (r != null &&
                    r.material != null)
                {
                    r.material .SetFloat("_Highlight", highlight);
                }
            }
        }
    }
    
    public void Highlight(float val, bool bForce = false)
    {
		highlightTarget = val;

        if (bForce)
        {
            highlight = highlightTarget;
            SetHighlight();
        }
	}
}
