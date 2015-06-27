using UnityEngine;
using System.Collections;

/// <summary>
/// Bigmouse Studio, by Richard Zhang
/// Random play specified audio clips with specified interval
/// </summary>

public class RandomAudioPlayer : MonoBehaviour {
	public float interval;
	public AudioClip[] audioClips;
	
	private AudioSource audioSource;
	private float timeCounter = 0.0f;
	
	// Use this for initialization
	void Start () {
		audioSource = GetComponent<AudioSource>();
		
		audioSource.clip = audioClips[(int)Mathf.Floor(Random.value * audioClips.Length)];
		audioSource.Play();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if(!audioSource.isPlaying)
		{
			timeCounter += Time.fixedDeltaTime;
			
			if(timeCounter > interval)
			{
				timeCounter = 0.0f;
				
				audioSource.clip = audioClips[(int)Mathf.Floor(Random.value * audioClips.Length)];
				audioSource.Play();
			}
		}
	}
}
