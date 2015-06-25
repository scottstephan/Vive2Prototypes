using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_PlayInfoVoice : SBBase {

    public AudioClip infoClip;

    public static bool isPlaying = false;

    private float pauseBeforePlayTime = 0.5f;

    public override void BeginBeat()
    {   
        if (infoClip != null)
        {
            StartCoroutine( InfoClipHandler() );
            Debug.Log ("SB_PlayInfoVoice " + gameObject.name + " playing clip " + infoClip.name);
        }
        
        base.BeginBeat();
    }
    
    IEnumerator InfoClipHandler() {
        yield return new WaitForSeconds( pauseBeforePlayTime );

        if( AudioManager.Instance.PlayInfoVoiceClip(infoClip) ) {
			isPlaying = true;

			yield return new WaitForSeconds (infoClip.length);

			isPlaying = false;
		}

		yield break;
	}

    public override bool IsComplete() {
        return true;
    }           

    void OnDestroy() {
        StopCoroutine( InfoClipHandler() );
        isPlaying = false;
    }
}
