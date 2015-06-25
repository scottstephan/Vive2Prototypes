using UnityEngine;
using System.Collections;

public class PlayerTriggeredDisperse : MonoBehaviour 
{
    public float extraColliderDistMult = 3f;
    public float extraSpeedMult = 2f;

    float colliderSize;

    void Start()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider c in colliders)
        { 
            if (!c.isTrigger)
            {
                continue;
            }

            if (c is SphereCollider)
            {
                colliderSize = ((SphereCollider)c).radius;
            }
            else if (c is BoxCollider)
            {
                colliderSize = ((BoxCollider)c).bounds.size.magnitude / 2f;
            }
            else if (c is CapsuleCollider)
            {
                colliderSize = ((CapsuleCollider)c).radius;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collided");

        GeneralSpeciesData gsd = other.GetComponent<GeneralSpeciesData>();
        if (gsd == null && other.transform.parent != null)
        {
            gsd = other.transform.parent.GetComponent<GeneralSpeciesData>();
        }

        if (gsd != null)
        {
            DisperseCollision.Disperse(gsd, null, transform, colliderSize, true);

            gsd.myCritterInfo.swimDisperseData.extraColliderDistMult = extraColliderDistMult;
            gsd.myCritterInfo.swimDisperseData.extraSpeedMult = extraSpeedMult;
        }
    }
}
