using UnityEngine;
using System.Collections;

public class SwimPlayerInteractData : BehaviorDataBase 
{
    public enum State
    {
        MoveNotice, // peel off super fast
        LookNotice, // play look
        MoveToPlayer, // get in initial distance range
        MoveCurious,
        MoveInView,
        MoveScared,
        LookInspect,
        LookCurious,
        LookScared,
        Spin,
        Poke,
    }

    public enum CuriousBehavior
    {
        None,
        SwimBackAndForth,
        Spin
    }

    public float priorityValue = 99f; //Value used by AI to determine priority of behaviors
    public float inspectDistance = 15f;
    public float curiousDistance = 4.3f;
    public float scaredDistance = 35f;
    public float turnDistance = 2f;
    public float maxInteractTime = 0f;
    public float maxSpeedMult = 4f;

    public AnimationClip noticeAnim;
    public AnimationClip inspectAnim;
    public AnimationClip curiousAnim;
    public AnimationClip scaredAnim;
    public AnimationClip seenReactionAnim;
    public AnimationClip pokeAnim;

    public CuriousBehavior curiousBehavior = CuriousBehavior.SwimBackAndForth;

    public bool sfxEnabled = true;
    public SoundFXID sfxLookNotice = SoundFXID.ClownfishInteractInitial;
    public SoundFXID sfxLookInspect = SoundFXID.ClownfishInteractCurious;
    public SoundFXID sfxLookCurious = SoundFXID.ClownfishInteractCurious;

    public float lookSwitchDirTimeMin = 1f;
    public float lookSwitchDirTimeMax = 2f;

    [HideInInspector]
    public float currentPriorityValue = 0;

    public float swimSpeedMult = 1.0f;      
    public float swimAccelOverride = 400.0f;        
    public float steeringThrottle = 1.0f;    

    [HideInInspector]
    public float swimSpeed;

    // throttled steering overrides.
    public float throttleMaxSpeed = 2f;
    public float throttleSpeedAccel = 8f;
    public float throttleSpeedDecel = 8f;

    [HideInInspector]
    public float savedThrottleMaxSpeed;
    [HideInInspector]
    public float savedThrottleSpeedAccel;
    [HideInInspector]
    public float savedThrottleSpeedDecel;
    
    public float yawAccel = 720f;
    public float yawDecel = 720f;
    public float yawMaxSpeed = 720f;

    [HideInInspector]
    public float savedYawAccel;
    [HideInInspector]
    public float savedYawDecel;
    [HideInInspector]
    public float savedYawMaxSpeed;
    
    public float pitchAccel = 180f;
    public float pitchDecel = 180f;
    public float pitchMaxSpeed = 180.0f;

    [HideInInspector]
    public float savedPitchAccel;
    [HideInInspector]
    public float savedPitchDecel;
    [HideInInspector]
    public float savedPitchMaxSpeed;
    
    public float rollAccel = 220f;
    public float rollDecel = 220f;
    public float rollMaxSpeed = 270.0f;
    public float rollOnYawMult = 0f;
    public float rollStrafingMult = 0f;

    [HideInInspector]
    public float savedRollAccel;
    [HideInInspector]
    public float savedRollDecel;
    [HideInInspector]
    public float savedRollMaxSpeed;
    [HideInInspector]
    public float savedRollOnYawMult;
    [HideInInspector]
    public float savedRollStrafingMult;    

    [HideInInspector]
    public State state;
    [HideInInspector]
    public float totalInteractTime;
    [HideInInspector]
    public float totalVisibleTime;
    [HideInInspector]
    public float stateTime;
    [HideInInspector]
    public float stateVisibleTime;
    [HideInInspector]
    public float stateNotVisibleTime;
    [HideInInspector]
    public bool statePlayedLookAnim;
    [HideInInspector]
    public int moveInViewFlipCount;
    [HideInInspector]
    public bool moveInViewStartLeft;
    [HideInInspector]
    public Vector3 stateTargetPos;
    [HideInInspector]
    public Vector3 playerStartPos;
    [HideInInspector]
    public Vector3 playerLastPos;
    [HideInInspector]
    public float lookOffsetYaw;
    [HideInInspector]
    public float lookOffsetPitch;
    [HideInInspector]
    public float lookOffsetNextTime;
    [HideInInspector]
    public float lookSwitchEyeNextTime;
    [HideInInspector]
    public int scareCount;
    [HideInInspector]
    public Vector3 randomMoveOffset;
    [HideInInspector]
    public float behaviorExitTime = -1f;


}
