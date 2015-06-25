using UnityEngine;
using System.Collections;

public abstract class VelocityTracker : MonoBehaviour
{
    public Transform trackedObject;
    Vector3 velocity;
    Vector3 lastpos;

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (trackedObject == null)
        {
            velocity = Vector3.zero;
            return;
        }

        Vector3 curpos = trackedObject.position;
        velocity = (curpos - lastpos) / Time.deltaTime;
        lastpos = curpos;
    }
}
