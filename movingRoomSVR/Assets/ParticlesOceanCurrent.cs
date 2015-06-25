using UnityEngine;
using System.Collections;

public class ParticlesOceanCurrent : MonoBehaviour 
{
    public float amplifier = 10f;
    
    OceanCurrents oceanCurrents;
    ParticlePlayground.PlaygroundParticlesC pp;

	void Start () 
    {
        oceanCurrents = OceanCurrents.Singleton;
   	    if (oceanCurrents == null)
        {
            Debug.Log(Application.loadedLevelName + " contains ParticlesOceanCurrent but there is no OceanCurrent object.");
            enabled = false;
        }

        pp = GetComponent<ParticlePlayground.PlaygroundParticlesC>();
	}
	
	void Update () 
    {
        if (pp != null)
        {
            pp.gravity = amplifier * oceanCurrents.currentDirection;
        }
	}
}
