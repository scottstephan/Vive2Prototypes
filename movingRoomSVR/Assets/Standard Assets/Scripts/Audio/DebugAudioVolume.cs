using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DebugAudioVolume : MonoBehaviour {
    private static bool adjustActive = false;

    private static List<DebugAudioVolume> audioSources = null;

    private static int activeIndex = -1;

    [HideInInspector]
    public AudioSource myAudio = null;

    public static TextMesh dname = null;
    public static TextMesh dvolume = null;

    public static float smoothMove = 0f;

	// Use this for initialization
	void Start () {
	    if( audioSources == null ) {
            audioSources = new List<DebugAudioVolume>();
        }

        myAudio = GetComponent<AudioSource>();
        audioSources.Add(this);

        if( activeIndex < 0 ) {
            activeIndex = 0;
        }

        if( dname == null ) {
            GameObject go = GameObject.Find("AudioDebugName");
            if( go != null ) {
                dname = go.GetComponent<TextMesh>();
                go = GameObject.Find("AudioDebugVolume");
                dvolume = go.GetComponent<TextMesh>();
            }
        }
	}
	
    public static void ActiveSourceAdjust( bool on ) {
        adjustActive = on;
    }

	// Update is called once per frame
	void Update () {
        if( !adjustActive ) {
            return;
        }

        int myIndex = audioSources.IndexOf(this);

        if( myIndex != activeIndex ) {
            return;
        }

        smoothMove -= Time.deltaTime;
        if( smoothMove <= 0f ) {
            if (Input.GetAxis("Left_Right") > 0.4f) {
                activeIndex++;
                if( activeIndex >= audioSources.Count ) {
                    activeIndex = 0;
                }

                smoothMove = 0.4f;
                return;
            }
            else if (Input.GetAxis("Left_Right") < -0.4f) {
                activeIndex--;
                if( activeIndex < 0 ) {
                    activeIndex = audioSources.Count;
                }

                smoothMove = 0.4f;
                return;
            }
        }


        if( Input.GetAxis("Forward_Back") > 0.4f ) {
            myAudio.volume += 0.005f;
        }
        else if( Input.GetAxis("Forward_Back") < -0.4f ) {
            myAudio.volume -= 0.005f;
        }

        if( dname != null ) {
            string t = gameObject.name;
            if( myAudio.clip != null ) {
                t += " " + myAudio.clip.name;
            }
            dname.text = t;
            dvolume.text = myAudio.volume.ToString();
        }
    }
    
    void OnDestroy() {
        audioSources.Remove(this);

        if( audioSources.Count == 0 ) {
            audioSources = null;
        }
    }
}
