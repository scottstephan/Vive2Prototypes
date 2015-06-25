using UnityEngine;
using System.Collections;

public class AnimEventSpawnObject : MonoBehaviour 
{
    public GameObject ToSpawn;
    Transform SpawnTransform;

    void Start()
    {
        foreach (Transform t in transform)
        {
            if (t.name == "SpawnFXPos")
            {
                SpawnTransform = t;
            }
        }
    }

    void SpawnObject()
    {
        if (ToSpawn != null)
        {
            if (SpawnTransform == null)
            {
                GameObject.Instantiate(ToSpawn);
            }
            else
            {
                GameObject.Instantiate(ToSpawn, SpawnTransform.position, SpawnTransform.rotation);
            }
        }
    }
}
