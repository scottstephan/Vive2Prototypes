using UnityEngine;
using System.Collections;

public class PlayerFeelFX : MonoBehaviour
{
    public float MaxSoundSeperation = 0.8f;
    // note RotationSpeedAtMax can be negative (swaps ear panning direction)
    public float RotationSpeedAtMax = -3f;
    public float FadeSpeedIn = 0.2f;
    public float FadeSpeedOut = 1f;
    [Tooltip("y axis should start at 1, x axis is in seconds and determines length of fade out")]
    public AnimationCurve FadeOutCurve;
    bool bUseFadeOutCurve = false; // auto sets to true if any key data in the curve
    float curFadeOutTime = -1f;
    float fadeOutStartVol;
    public float MinDeltaAngleTrigger = 0.1f;

    private float OffDeltaAngle;

    AudioSource audioSrc;

    private bool bAudioPlaying;
    private float lastAngleY; // y axis rotation, e.g. look L/R
    private float lastDeltaAbsY;
    private float lastAngleX; // x axis rotation, e.g. look U/D
    private float lastDeltaAbsX;
    private float curVolume;
    private float curPan;
    private float maxVolume;

    public float moveParticleMinVelocityScale = -0.5f;
    public float moveParticleMaxVelocityScale = -5f;
    public float moveParticleRandSpeedAdd = 5;
    //    ParticleEmitter planktonMovingEmitter;
    //    ParticleRenderer planktonMovingRenderer;
    ParticleSystem planktonMovingSystem;


    enum BoostState
    {
        Stopped,
        Starting,
        Running,
        Stopping
    }

    BoostState boostState = BoostState.Stopped;
    ParticleSystemRenderer planktonRenderer;
    //    float boostTimer = 0f;

    Vector3 lastPosition;
    Vector3 avgVelocity;

    bool ready = false;


    IEnumerator _DelayReady()
    {
        yield return new WaitForSeconds(1f);
        ready = true;
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(_DelayReady());
        audioSrc = GetComponent<AudioSource>();
        Vector3 rot = CameraManager.GetCurrentCameraRotation().eulerAngles;
        lastAngleY = rot.y;
        lastAngleX = rot.x;
        maxVolume = AudioManager.Instance != null ? AudioManager.Instance.maxPlayerFeelVolume : 0.53f;
        curVolume = 0f;
        OffDeltaAngle = MinDeltaAngleTrigger * 0.75f;

#if UNITY_ANDROID && !UNITY_EDITOR
        maxVolume *= AudioManager.AndroidVolumeBoost;
        maxVolume = Mathf.Clamp01(maxVolume);
#endif

        GameObject go;

        //        go = GameObject.Find ("PlanktonMoving");
        //        if (go != null)
        //        {
        //            planktonMovingEmitter = go.GetComponent<ParticleEmitter>();
        //            planktonMovingRenderer = go.GetComponent<ParticleRenderer>();
        //        }

        go = GameObject.Find("planktonOpaqueMoving");
        if (go != null)
        {
            planktonMovingSystem = go.GetComponent<ParticleSystem>();
            if (planktonMovingSystem != null)
                planktonMovingSystem.enableEmission = false;
        }

        go = GameObject.Find("planktonOpaqueLight");
        if (go != null)
        {
            planktonRenderer = go.GetComponent<ParticleSystemRenderer>();
        }

        lastPosition = CameraManager.GetCurrentCameraPosition();

        bUseFadeOutCurve = FadeOutCurve != null && FadeOutCurve.keys.Length > 0;
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        if (audioSrc != null && AudioManager.Instance != null)
        {
            if (SimInstance.Instance.slowdownActive || SimInstance.Instance.IsSimPaused())
            {
                if (audioSrc.isPlaying)
                {
                    audioSrc.Pause();
                }
                return;
            }
            else if (!SimInstance.Instance.IsSimPaused() && !SimInstance.Instance.slowdownActive && !audioSrc.isPlaying)
            {
                audioSrc.Play();
            }

            maxVolume = AudioManager.Instance.maxPlayerFeelVolume;

            Vector3 rot = CameraManager.GetCurrentCameraRotation().eulerAngles;
            float deltaY = Mathf.DeltaAngle(lastAngleY, rot.y);
            float absDeltaY = Mathf.Abs(deltaY);
            float deltaX = Mathf.DeltaAngle(lastAngleX, rot.x);
            float absDeltaX = Mathf.Abs(deltaX);
            // note RotationSpeedAtMax can be negative (swaps ear panning direction)
            float unitDeltaY = deltaY / RotationSpeedAtMax;
            float absUnitDeltaX = absDeltaX / Mathf.Abs(RotationSpeedAtMax);
            float volInc;
            if (ready && ((absDeltaY > MinDeltaAngleTrigger && absDeltaY > lastDeltaAbsY)
                || (absDeltaX > MinDeltaAngleTrigger && absDeltaX > lastDeltaAbsX)))
            {
                volInc = dt / FadeSpeedIn * Mathf.Clamp01(Mathf.Max(Mathf.Abs(unitDeltaY), absUnitDeltaX));
                if (!bAudioPlaying)
                {
                    bAudioPlaying = true;
                    audioSrc.Play();
                }

                if (curVolume < 1f)
                {
                    curVolume += volInc;
                    if (curVolume > 1f)
                        curVolume = 1f;
                }

                curFadeOutTime = -1f;
            }
            else if (absDeltaX < OffDeltaAngle && absDeltaY < OffDeltaAngle)
            {
                if (curFadeOutTime < 0)
                {
                    // just started fading out, so scale curve y axis to current volume.
                    fadeOutStartVol = curVolume;
                    curFadeOutTime = 0f;
                }
                else
                {
                    curFadeOutTime += dt;
                }

                volInc = -dt / FadeSpeedOut;

                // fade out
                if (bAudioPlaying)
                {
                    if (!bUseFadeOutCurve)
                    {
                        curVolume += volInc;
                    }
                    else
                    {
                        curVolume = fadeOutStartVol * FadeOutCurve.Evaluate(curFadeOutTime);
                    }

                    if (curVolume < 0f)
                    {
                        bAudioPlaying = false;
                        audioSrc.Pause();
                        lastDeltaAbsY = 0f;
                    }
                }
            }

            if (bAudioPlaying)
            {
                curPan = Mathf.Clamp(unitDeltaY, -MaxSoundSeperation, MaxSoundSeperation);
                audioSrc.volume = maxVolume * curVolume;
                audioSrc.panStereo = Mathf.SmoothStep(audioSrc.panStereo, curPan, 10f * dt);
            }

            lastAngleY = rot.y;
            lastDeltaAbsY = absDeltaY;
            lastAngleX = rot.x;
            lastDeltaAbsX = absDeltaX;
        }

        Vector3 curPos = CameraManager.GetCurrentCameraPosition();

        if (planktonMovingSystem != null &&
            dt > 0f)
        {
            Vector3 deltaDir = curPos - lastPosition;
            float deltaDist = deltaDir.magnitude;
            float playerSpeed = deltaDist / dt;
            if (playerSpeed > 10f)
            {
                deltaDir /= dt;

                avgVelocity += deltaDir;
                avgVelocity /= 30f;
                planktonMovingSystem.enableEmission = true;
            }
            else
            {
                avgVelocity = Vector3.zero;
                //planktonMovingSystem.enableEmission = false;
            }
        }

        if (boostState == BoostState.Starting)
        {
            planktonRenderer.velocityScale += (0.15f / 0.2f) * dt;
            if (planktonRenderer.velocityScale > 0.15f)
            {
                planktonRenderer.velocityScale = 0.15f;
                boostState = BoostState.Running;
            }
        }
        else if (boostState == BoostState.Stopping)
        {
            planktonRenderer.velocityScale -= (0.15f / 0.2f) * dt;
            if (planktonRenderer.velocityScale < 0f)
            {
                planktonRenderer.velocityScale = 0f;
                planktonRenderer.renderMode = ParticleSystemRenderMode.Billboard;
                boostState = BoostState.Stopped;
            }
        }

        lastPosition = curPos;
    }

    public void StartStarfield()
    {
        if (planktonMovingSystem != null)
            planktonMovingSystem.GetComponent<Animator>().SetTrigger("starfield");
    }

    public void StartBoost()
    {
        if (planktonRenderer == null)
        {
            return;
        }

        boostState = BoostState.Starting;
        planktonRenderer.renderMode = ParticleSystemRenderMode.Stretch;
    }

    public void StopBoost()
    {
        if (planktonRenderer == null)
        {
            return;
        }

        boostState = BoostState.Stopping;
    }
}
