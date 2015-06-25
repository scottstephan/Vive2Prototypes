using UnityEngine;
using System.Collections;

public class AudioDirectionalHotspot : MonoBehaviour 
{
    public float cutoffFrequencyMin = 200;
    public float cutoffFrequencyMax = 5000;
    public float pitchDistMin = 0.75f;
    public float pitchDistMax = 1.1f;

    Transform myTransform;
//    AudioSource audioSrc;
    AudioLowPassFilter lowPassFilter;
    AudioReverbFilter reverb;


	// Use this for initialization
	void Start () 
    {
        myTransform = gameObject.transform;
//        audioSrc = gameObject.audio;
//        lowPassFilter = gameObject.GetComponent<AudioLowPassFilter>();
//        if audioassFilter == null)
//        {
//            lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
//        }

        reverb = gameObject.GetComponent<AudioReverbFilter>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        Vector3 camPos = CameraManager.GetCurrentCameraPosition();
        Vector3 camDir = CameraManager.GetCurrentCameraForward();

        Vector3 posToCamDir = camPos - myTransform.position;
        float posToCamDist = posToCamDir.magnitude;

        if (posToCamDist > 0f)
        {
            posToCamDir /= posToCamDist;
        }

        float angle = Vector3.Angle(new Vector3(-posToCamDir.x, -posToCamDir.y*0.2f, -posToCamDir.z), new Vector3(camDir.x, camDir.y*0.2f, camDir.z));

//        lowPassFilter.cutoffFrequency = MathfExt.Fit (dot, 1f, -1f, cutoffFrequencyMin, cutoffFrequencyMax);        
//        audio.pitch = MathfExt.Fit (posToCamDist, 0f, audio.maxDistance, pitchDistMin, pitchDistMax);

        reverb.dryLevel = MathfExt.Fit(angle, 0f, 180f, 0f, -700f);
        reverb.decayTime = MathfExt.Fit(angle, 0f, 180f, 4f, 1f);
        reverb.decayHFRatio = MathfExt.Fit(angle, 0f, 180f, 0.1f, 1f);
        reverb.reflectionsLevel = MathfExt.Fit(angle, 0f, 180f,-900f, -2607f);
        reverb.reflectionsDelay = MathfExt.Fit(angle, 0f, 180f, 1f, 0f);
        reverb.reverbLevel = MathfExt.Fit(angle, 0f, 180f, -300f, -10000f);
        reverb.reverbDelay = MathfExt.Fit(angle, 0f, 180f, 1000f, 0f);
        reverb.diffusion = MathfExt.Fit(angle, 0f, 180f, 90f, 0f);
        reverb.density = MathfExt.Fit(angle, 0f, 180f, 90f, 0f);
    }
}
