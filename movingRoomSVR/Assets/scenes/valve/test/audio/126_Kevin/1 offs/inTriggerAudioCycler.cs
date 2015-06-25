using UnityEngine;
using System.Collections;

public class inTriggerAudioCycler : MonoBehaviour {
    private bool isCounting;
    public float maxTimeBetweenPlays;
    public float minTimeBetweenPlays;
    private float currentPlayInterval;
    private float curTimeCounter;
    public AudioClip[] soundsToPlayWhileInTrigger;
    public AudioSource soundSource;

    public Transform playerTransform;
    public float moveVelocityThresholdToPlay;
    private Vector3 lastPlayerPos;
    public float moveVelocity;
    private float dt;
    public string objectNameToLookFor;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (isCounting)
        {
            dt = Time.deltaTime;
            moveVelocity = (playerTransform.position - lastPlayerPos).magnitude / dt;
            lastPlayerPos = playerTransform.position;
            curTimeCounter += dt;
        }

        if (moveVelocity > moveVelocityThresholdToPlay && isCounting)
        {
            if (curTimeCounter >= currentPlayInterval)
            {
                playSound();
            }
        }
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.name == objectNameToLookFor)
        {
            Debug.Log("Deck audio is counting");
            isCounting = true;
            // curTimeCounter = 0;
            currentPlayInterval = Random.Range(minTimeBetweenPlays, maxTimeBetweenPlays);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.name == objectNameToLookFor)
        {
            Debug.Log("Deck audio is exited");
            isCounting = false;
            moveVelocity = 0;
            //curTimeCounter = 0;\
        }
    
    }

    private void playSound()
    {
        Debug.Log("Deck audio is playing");
        if (!soundSource.isPlaying)
        {
            AudioClip newClip = soundsToPlayWhileInTrigger[Random.Range(0, soundsToPlayWhileInTrigger.GetUpperBound(0))];
            soundSource.clip = newClip;
            soundSource.Play();

            curTimeCounter = 0f;

           currentPlayInterval = Random.Range(minTimeBetweenPlays, maxTimeBetweenPlays) + newClip.length;
        }
    }
}
