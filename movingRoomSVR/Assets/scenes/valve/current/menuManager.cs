using UnityEngine;
using System.Collections;

public class menuManager : MonoBehaviour {
   // public int sceneIndexToLoad;
    public float timeToFadeToBlack;
    public float whenToBeginFade = 120f;
    public GameObject fadeObject;
    private float lerpRate;
    public bool isLerping = false; //moar bad code! Just toggle to true for a fade up.
    private float totalLerpAmt = 0;
    public bool fadeUp = true;
	// Use this for initialization
	void Start () {
        Color setColor = fadeObject.GetComponent<Renderer>().material.color;

        if (fadeUp)
        {
            setColor.a = 0;
            totalLerpAmt = 0;
        }
        else
        {
            setColor.a = 1;
        }
        fadeObject.GetComponent<Renderer>().material.color = setColor;

        lerpRate = 1 / timeToFadeToBlack;
	}
	
	// This code can be crazy condensed.
	void Update () {
        if (Time.timeSinceLevelLoad > whenToBeginFade)
        {
            isLerping = true;
        }

        if (isLerping)
        {
            if (fadeUp)
            {
                float lerpAmtThisFrame = lerpRate * Time.deltaTime;
                totalLerpAmt += lerpAmtThisFrame;
                float newAlpha = Mathf.Lerp(0, 1, totalLerpAmt);
                Color newColor = fadeObject.GetComponent<Renderer>().material.color;
                newColor.a = newAlpha;
                fadeObject.GetComponent<Renderer>().material.color = newColor;
            }
            else
            {
                float lerpAmtThisFrame = lerpRate * Time.deltaTime;
                totalLerpAmt += lerpAmtThisFrame;
                float newAlpha = Mathf.Lerp(1, 0, totalLerpAmt);
                Color newColor = fadeObject.GetComponent<Renderer>().material.color;
                newColor.a = newAlpha;
                fadeObject.GetComponent<Renderer>().material.color = newColor;
                if (newColor.a == 0) { isLerping = false; }
            }
        }
	}

}
