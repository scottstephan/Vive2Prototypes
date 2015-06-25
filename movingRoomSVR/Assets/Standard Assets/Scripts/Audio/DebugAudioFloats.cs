using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum AdjustFloats {
    PlayerFeel = 0,
    BubbleMin = 1,
    BubbleMax = 2,
}

public class DebugAudioFloats : MonoBehaviour {

    private static AdjustFloats activeFloat = AdjustFloats.PlayerFeel;
    private static bool adjustActive = true;

    private float smoothMove = 0.5f;
    public static TextMesh dname = null;
    public static TextMesh dvolume = null;

	// Use this for initialization
	void Start() {

        if( dname == null ) {
            GameObject go = GameObject.Find("AudioDebugName");
            dname = go.GetComponent<TextMesh>();
            go = GameObject.Find("AudioDebugVolume");
            dvolume = go.GetComponent<TextMesh>();
        }
	}
	
	// Update is called once per frame
	void Update () {
        // toggle between floats and sources
        if(Input.GetButtonDown("ToggleDebugHUD")) {
            adjustActive = !adjustActive;
            DebugAudioVolume.ActiveSourceAdjust( !adjustActive );
        }

        if( !adjustActive ) {
            return;
        }

        smoothMove -= Time.deltaTime;
        if( smoothMove <= 0f ) {
            if (Input.GetAxis("Left_Right") > 0.4f) {
                switch( activeFloat ) {
                    case AdjustFloats.PlayerFeel: {
                        activeFloat = AdjustFloats.BubbleMin;
                        break;
                    }
                    case AdjustFloats.BubbleMin: {
                        activeFloat = AdjustFloats.BubbleMax;
                        break;
                    }
                    case AdjustFloats.BubbleMax:{
                        activeFloat = AdjustFloats.PlayerFeel;
                        break;
                    }
                }
                smoothMove = 0.4f;
            }
            else if (Input.GetAxis("Left_Right") < -0.4f) {
                switch( activeFloat ) {
                    case AdjustFloats.PlayerFeel: {
                        activeFloat = AdjustFloats.BubbleMax;
                        break;
                    }
                    case AdjustFloats.BubbleMin: {
                        activeFloat = AdjustFloats.PlayerFeel;
                        break;
                        }
                    case AdjustFloats.BubbleMax:{
                        activeFloat = AdjustFloats.BubbleMin;
                        break;
                    }
                }

                smoothMove = 0.4f;
            }
        }
        
        
        if( Input.GetAxis("Forward_Back") > 0.4f ) {
            switch( activeFloat ) {
                case AdjustFloats.PlayerFeel: {
                    AudioManager.Instance.maxPlayerFeelVolume += 0.005f;
                    AudioManager.Instance.maxPlayerFeelVolume = Mathf.Clamp01( AudioManager.Instance.maxPlayerFeelVolume );
                    break;
                }
                case AdjustFloats.BubbleMin: {
                    AudioManager.Instance.bubbleMinVolume += 0.005f;
                    AudioManager.Instance.bubbleMinVolume = Mathf.Clamp01( AudioManager.Instance.bubbleMinVolume );
                    break;
                }
                case AdjustFloats.BubbleMax:{
                    AudioManager.Instance.bubbleMaxVolume += 0.005f;
                    AudioManager.Instance.bubbleMaxVolume = Mathf.Clamp01( AudioManager.Instance.bubbleMaxVolume );
                    break;
                }
            }
        }
        else if( Input.GetAxis("Forward_Back") < -0.4f ) {
            switch( activeFloat ) {
            case AdjustFloats.PlayerFeel: {
                AudioManager.Instance.maxPlayerFeelVolume -= 0.005f;
                AudioManager.Instance.maxPlayerFeelVolume = Mathf.Clamp01( AudioManager.Instance.maxPlayerFeelVolume );
                break;
            }
            case AdjustFloats.BubbleMin: {
                AudioManager.Instance.bubbleMinVolume -= 0.005f;
                AudioManager.Instance.bubbleMinVolume = Mathf.Clamp01( AudioManager.Instance.bubbleMinVolume );
                break;
            }
            case AdjustFloats.BubbleMax:{
                AudioManager.Instance.bubbleMaxVolume -= 0.005f;
                AudioManager.Instance.bubbleMaxVolume = Mathf.Clamp01( AudioManager.Instance.bubbleMaxVolume );
                break;
            }
            }
        }
        
        switch( activeFloat ) {
        case AdjustFloats.PlayerFeel: {
            dname.text = "PlayerFeel";
            dvolume.text = AudioManager.Instance.maxPlayerFeelVolume.ToString();
            break;
        }
        case AdjustFloats.BubbleMin: {
            dname.text = "BubbleMin";
            dvolume.text = AudioManager.Instance.bubbleMinVolume.ToString();
            break;
        }
        case AdjustFloats.BubbleMax:{
            dname.text = "BubbleMax";
            dvolume.text = AudioManager.Instance.bubbleMaxVolume.ToString();
            break;
        }
        }
    }    
}
