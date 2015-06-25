using UnityEngine;
using System.Collections;

public class timedAudioDeploys : MonoBehaviour
{

    public float[] timesToPlay;
    public AudioSource[] AudioSourceToActivate;
    private int soundIndex = 0;
    private bool isCheckingForSounds = true;

    // Use this for initialization
    void Start()
    {
        if (timesToPlay.Length == 0) isCheckingForSounds = false;
    }

    // Update is called once per frame
    void Update() {
        if(isCheckingForSounds){
            if (Time.time > timesToPlay[soundIndex]) {
                Debug.Log("Playing: " + AudioSourceToActivate[soundIndex].name);
                //AudioSourceToActivate[soundIndex].SetActive(true);
                AudioSourceToActivate[soundIndex].Play();
	            soundIndex++;
                if(soundIndex == timesToPlay.Length) isCheckingForSounds = false;
            }
        }
    }
}
