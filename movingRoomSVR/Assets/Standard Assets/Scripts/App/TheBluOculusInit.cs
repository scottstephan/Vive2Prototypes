using UnityEngine;
using System.Collections;

using System.Runtime.InteropServices;   // required for DllImport

public class TheBluOculusInit : MonoBehaviour {
    public int  DEBUG_maxFPS = 60;

    static public TheBluOculusInit Instance;

    // Occulus interface calls necessary to set up the rendering/quality parameters
    [DllImport("OculusPlugin")]
    private static extern void OVR_VrModeParms_SetCpuLevel( int cpuLvl );
    [DllImport("OculusPlugin")]
    private static extern void OVR_VrModeParms_SetGpuLevel( int gpuLvl );
    [DllImport("OculusPlugin")]
    private static extern void OVR_TW_SetMinimumVsyncs( int mode );
    [DllImport("OculusPlugin")]
    private static extern void OVR_TW_SetDebugMode( int mode, int value );
//    [DllImport("OculusPlugin")]
//    private static extern void OVR_VrModeParms_SetAllowPowerSave( bool allow );
    [DllImport("OculusPlugin")]
    private static extern bool OVR_IsPowerSaveActive();

    public virtual void Awake() 
    {
        Instance = this;

        SetData( true );

        GameObject.DontDestroyOnLoad(this);

        Shader.SetGlobalColor("_DeepColor", OculusCameraFadeManager.MoonlightBlack);
        Shader.SetGlobalColor("_MiddleColor", OculusCameraFadeManager.MoonlightBlack);
        Shader.SetGlobalColor("_ShallowColor", OculusCameraFadeManager.MoonlightBlack);
        Shader.SetGlobalColor("_ShallowColorMinusDeep",Color.clear);
        RenderSettings.fogColor = OculusCameraFadeManager.MoonlightBlack;
    }

    void SetData( bool first_time ) {
        //oculus says use this to help get rid of judder. Changing it
        // once the application has been started does not seem to work.
        Application.targetFrameRate = DEBUG_maxFPS;
        try
        {
            OVR_TW_SetMinimumVsyncs( DEBUG_maxFPS == 60 ? 1 : 2 );
            
            // The functionality of these is unclear and experimentally it
            // seems that they behave differently than the Oculus tech notes
            // and forums indicate. Setting them low and letting the system
            // ramp them up, as recommended in tech notes appears to lead to
            // a "pulsing" framerate that has a great degree of variability.
            // Experimentally, it seems that the best results have been 
            // achieved by setting them to the max (which is the default in the
            // current version of the SDK, as of July 2014
            OVR_VrModeParms_SetCpuLevel( 1 );
            OVR_VrModeParms_SetGpuLevel( 3 );
            if( first_time ) {
                OVRPluginEvent.Issue( RenderEventType.ResetVrModeParms );
            }
        }
        catch
        {
            // handle exceptions runnng on pc
        }
                
        // Enable Power Save Mode Handling
//        OVR_VrModeParms_SetAllowPowerSave( true );
    }

    void OnApplicationPause( bool pause ) {
        if( !pause ) {
            // this should be reseting the cpu levels from when we come back from the oculus menu, but it is not working as intended, but staying here incase it does start to work as it should.
            SetData( false );            
        }
    }
}
