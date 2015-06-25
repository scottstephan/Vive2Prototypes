using UnityEngine;
using System.Collections;

public class TextOverlay : MonoBehaviour {

	public TextMesh textMesh;
    public bool hoverFade;

    bool highlighted;
    float curAlpha = 1.0f;
    float targetAlpha = 1.0f;

	FloatingMenuManager _fmm;

	void Awake() 
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMesh>();
        }

        if (hoverFade)
        {
            Highlight(false, true);
        }
	}

	void OnEnable() 
    {

	}

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        bool bChanged = false;
        if(curAlpha < targetAlpha)
        {
            bChanged = true;
            curAlpha += Time.deltaTime / 0.35f;
            if (curAlpha > targetAlpha)
            {
                curAlpha = targetAlpha;
            }
        }
        else if(curAlpha > targetAlpha)
        {
            bChanged = true;
            curAlpha -= Time.deltaTime / 0.35f;
            if (curAlpha < targetAlpha)
            {
                curAlpha = targetAlpha;
            }
        }
        
        if (bChanged)
        {
            SetColor();
        }	
	}

    void SetColor()
    {
        if (textMesh  == null)
        {
            return;
        }

        Color c = textMesh.color;
        textMesh.color = new Color(c.r, c.g, c.b, curAlpha);
    }

    public void Highlight(bool bSet, bool bForce)
    {
        if (!hoverFade)
        {
            return;
        }

        targetAlpha = bSet ? 1f : 0f;

        if (bForce)
        {
            curAlpha = targetAlpha;
            SetColor ();
        }
    }

	public void SetText(string text) {

		textMesh.text = text;
	}
}
