using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SB_CritterBubbles : SBBase {

	public string designGroupName;
    List<CritterInfo> searchCritters = new List<CritterInfo>();

    public float spawnTotalDuration = 0.55f;
    public float spawnSpacingMin = 0.005f;
    public float spawnSpacingMax = 0.06f;
    public int bubblesPerSpawnMin = 1;
    public int bubblesPerSpawnMax = 4;

    public float betweenSpawnMin = 0.45f;
    public float betweenSpawnMax = 1.2f;

    [Tooltip("This must match the critters bubble particle system!")]
    public float critterBubbleMinSize = 3f;
    [Tooltip("This must match the critters bubble particle system range!")]
    public float critterBubbleRange = 0.5f;

    IEnumerator ParticleSpawner( CritterInfo c, ParticleSystem ps ) {
        while( true ) {
            float rnd = Random.Range( betweenSpawnMin, betweenSpawnMax );
            yield return new WaitForSeconds( rnd );

            float time_left = spawnTotalDuration;
            while( time_left > 0f ) {
                int num = RandomExt.IntRange( bubblesPerSpawnMin, bubblesPerSpawnMax );
                ps.Emit( num );
                ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ ps.particleCount ];
                int numParticles = ps.GetParticles( particles );
                Vector3 vel = (c.cachedPosition - c.generalMotionData.lastPosition) * Time.deltaTime;
                for (int iP=0; iP<numParticles; ++iP)
                {
                    if( particles[iP].size < critterBubbleMinSize ) {

                        particles[iP].size += critterBubbleRange;
                        particles[iP].velocity += vel;
                    }
                    else {
                        num = 1;
                    }
                }
                
                ps.SetParticles(particles, numParticles);

                float gap = Random.Range( spawnSpacingMin, spawnSpacingMax );
                yield return new WaitForSeconds( gap );
                time_left -= gap;
            }
        }
    }

    void OnDestroy() {
        StopAllCoroutines();
    }

    public override bool ContainsDesignGroup( string design_group ) { 
        if (string.IsNullOrEmpty( designGroupName ) ) {
            return false;
        }
        
        return ( designGroupName.ToUpper().Equals( design_group ) );
    }

    public override void Reset() {
        StopAllCoroutines();
        base.Reset();
    }

    public override void BeginBeat() 
    {
        GetCrittersByDesignGroup(designGroupName, searchCritters);
        #if UNITY_EDITOR
        Debug.Log("SB_CritterBubbles " + gameObject.name + " Group:  " + designGroupName + " Count: " + searchCritters.Count);
        #endif

        for (int i=0; i<searchCritters.Count; ++i)
        {
            CritterInfo c = searchCritters[i];
            if( c == null )            
            {
                continue;
            }

            Transform bt = Find(c.critterTransform, "fishbubble");
            if (bt == null)
            {
                continue;
            }

            GameObject go = bt.gameObject;
            if (go == null)
            {
                continue;
            }

            ParticleSystem ps = go.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                StartCoroutine( ParticleSpawner( c, ps ) );
            }
        }

        searchCritters.Clear ();

        base.BeginBeat();
	}	

    static Transform Find(Transform current, string name)   
    {
        if (current == null)
        {
            return null;
        }
           
        // check if the current bone is the bone we're looking for, if so return it
        if (current.name == name)
            return current;
        
        // search through child bones for the bone we're looking for
        for (int i = 0; i < current.childCount; ++i)
        {
            // the recursive step; repeat the search one step deeper in the hierarchy
            Transform found = Find(current.GetChild(i), name);
            
            // a transform was returned by the search above that is not null,
            // it must be the bone we're looking for
            if (found != null)
                return found;
        }
        
        // bone with name was not found
        return null;
    }


	public override bool IsComplete()
    {
        return true;
	}			
}
