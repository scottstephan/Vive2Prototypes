using UnityEngine;
using System.Collections;
using ParticlePlayground;

public class VortexStrengthByVelocity : VelocityTracker 
{
    public float maxStrength = 25f;
    public float minStrength = 0;
    public float maxSpeed = 1f;
    public int manipulatorIndex = 0;

    ManipulatorObjectC manipulator;

    // Use this for initialization
    void Start()
    {
        manipulator = PlaygroundC.GetManipulator(manipulatorIndex, GetComponent<PlaygroundParticlesC>());
        if (manipulator == null)
            enabled = false;

        if (trackedObject == null)
            trackedObject = manipulator.transform.transform;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        manipulator.strength = minStrength + maxStrength * GetVelocity().magnitude/maxSpeed;
    }
}
