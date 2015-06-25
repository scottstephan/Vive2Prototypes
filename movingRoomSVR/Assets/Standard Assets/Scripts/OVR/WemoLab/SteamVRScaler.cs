using UnityEngine;
using System.Collections;

public class SteamVRScaler : HMDScalerBase
{
    protected override void ApplyScale()
    {
        transform.localScale = Scale * Vector3.one; 
        FixChildPlane();
    }

    void FixChildPlane()
    {
        GameObject[] camPlanes = GameObject.FindGameObjectsWithTag("CamPlane");
        foreach (GameObject go in camPlanes)
        {
            Transform tp = go.transform;
            // check if this object is a parent of the plane
            while (tp != transform && tp.parent != null)
            {
                tp = tp.parent;
            }

            if (tp == transform)
            {
                go.transform.localScale /= Scale;
            }
        }
    }
}

