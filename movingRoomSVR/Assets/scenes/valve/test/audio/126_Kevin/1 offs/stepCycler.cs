using UnityEngine;
using System.Collections;

public class stepCycler : MonoBehaviour {
    public AudioClip[] creakSteps;
    public float stochasticModifier = 1f;
    public AudioSource stepSource;
    public float timeOfLastPlay;
    public float timeSinceLastPlay;
    public float minCreakDelay = 0;
    public float maxCreakDelay;
    public float curDelay = 5f; //need an intro delay

    public bool playAtFixedTime;
    public float fixedTimeInterval;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	    timeSinceLastPlay = Time.timeSinceLevelLoad - timeOfLastPlay;

	    if (curDelay < timeSinceLastPlay && !playAtFixedTime && !stepSource.isPlaying) {
            playRandomClip();
		}
        else if (playAtFixedTime && timeSinceLastPlay >= fixedTimeInterval && !stepSource.isPlaying)
        {
            playRandomClip();
        }
	}

    private void playRandomClip()
    {
        Debug.Log("Playing foot creak");
        AudioClip randClip = creakSteps[Random.Range(0, creakSteps.Length)];
        stepSource.clip = randClip;
        stepSource.Play();
            //stepSource.PlayOneShot(randClip);
        minCreakDelay = randClip.length;
        curDelay = minCreakDelay + Random.RandomRange(1, maxCreakDelay);
        timeOfLastPlay = Time.timeSinceLevelLoad;
    }
}
