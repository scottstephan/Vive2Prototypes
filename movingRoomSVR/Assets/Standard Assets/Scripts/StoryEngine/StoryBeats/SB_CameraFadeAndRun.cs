using UnityEngine;
using System.Collections;

public enum FadeAndRunState {
    READY,
    FADE_OUT,
    RUN_BEATS,
    FADE_IN,
    DONE
}

public class SB_CameraFadeAndRun : SBBase {
	
    public SBBase[] beatsToRunWhenFaded = null;

    public Color fadeColor = Color.black;

    public float fadeTime = 1.5f;

    FadeAndRunState state = FadeAndRunState.READY;

    public override bool TriggersStoryBeat( SBBase trigger_beat ) {
        if( base.TriggersStoryBeat( trigger_beat ) ) {
            return true;
        }

        if( beatsToRunWhenFaded != null && beatsToRunWhenFaded.Length > 0 ) {
            for( int i = 0; i < beatsToRunWhenFaded.Length; i++ ) {
                if (beatsToRunWhenFaded[i] == trigger_beat) {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    public override void Reset ()
    {
        state = FadeAndRunState.READY;

        base.Reset();
    }

    public override void BeginBeat ()
    {
        base.BeginBeat();

        state = FadeAndRunState.FADE_OUT;
        OculusCameraFadeManager.StartCameraFadeToColor( fadeColor, fadeTime, FadedOut, null );
    }

	public override bool IsComplete()
    {
        return ( state == FadeAndRunState.DONE );
	}

    void FadedOut( object arg ) {
        if( state != FadeAndRunState.FADE_OUT ) {
            // we got reset behind our backs. assume everything is ok.
            return;
        }
        if( beatsToRunWhenFaded != null && beatsToRunWhenFaded.Length > 0 ) {
            state = FadeAndRunState.RUN_BEATS;
            StoryManager.Instance.storyBeatsContainer.BeginBeats( beatsToRunWhenFaded );
        }
        else {
            state = FadeAndRunState.FADE_IN;
            OculusCameraFadeManager.StartCameraFadeFromBlack( fadeTime, FadedIn, null );
        }
    }
    
    void FadedIn( object arg ) {
        if( state != FadeAndRunState.FADE_IN ) {
            // we got reset behind our backs. assume everything is ok.
            return;
        }

        state = FadeAndRunState.DONE;
    }
    
    public override void UpdateBeat() {
        if( state != FadeAndRunState.RUN_BEATS ) {
            return;
        }

        bool done = true;
        for( int i = 0; i < beatsToRunWhenFaded.Length; i++ ) {
            SBBase sb = beatsToRunWhenFaded[i];
            if( sb != null && !sb.IsComplete() ) {
                done = false;
            }
        }

        if( done ) {
            state = FadeAndRunState.FADE_IN;
            OculusCameraFadeManager.StartCameraFadeFromBlack( fadeTime, FadedIn, null );
        }
    }
}

