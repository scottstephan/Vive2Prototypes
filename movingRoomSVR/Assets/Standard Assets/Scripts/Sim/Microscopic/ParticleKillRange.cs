using UnityEngine;
using System.Collections;

public class ParticleKillRange : MonoBehaviour 
{
    public Transform xform;
	public float range;

    ParticleSystem myParticleSys;
	ParticleSystem.Particle [] particles;
	float rangeSq;

    public static bool paused = false;
    private bool myPause = false;

    int step = 0;
	// Use this for initialization
	void Start () 
	{
        myParticleSys = GetComponent<ParticleSystem>();
        rangeSq = range * range;

        if (myParticleSys == null)
		{
			enabled = false;
		}
        else
        {
            particles = new ParticleSystem.Particle[myParticleSys.maxParticles];
        }
	}

	void Update()
	{
		if (xform == null)
		{
			return;
		}

        if( myPause != paused ) {
            if( paused ) {
                myParticleSys.Pause();
            }
            else {
                myParticleSys.Play();
            }
            myPause = paused;
        }

        if( paused ) {
            return;
        }

        step = step == 0 ? 1 : 0;
        int num = myParticleSys.GetParticles(particles);
        int half_num = num / 2;
        Vector3 xpos = xform.position;
        int idx = 0;
		for (int i = 0; i < half_num; ++i)
		{
            idx = ( i * 2 ) + step;
            if( idx < num ) 
            {
                // could amortize cost here over multiple frames
                Vector3 ppos = particles[idx].position;
                float x = ppos.x - xpos.x;
                float y = ppos.y - xpos.y;
                float z = ppos.z - xpos.z;

                if ((x*x + y*y + z*z) < rangeSq)
                {
                    //              particles[i].lifetime = -1;
                    particles[idx].size *= 0.5f;
                }
            }
		}
        myParticleSys.SetParticles(particles, num);
	}
}
