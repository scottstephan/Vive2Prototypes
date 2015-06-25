using UnityEngine;
using System.Collections;

public delegate void CollisionEnteredDelegate( CritterInfo me, Collider other );
public delegate void CollisionExitedDelegate( CritterInfo me, Collider other );

public class AwarenessCollision : MonoBehaviour {
    [HideInInspector]
    public CritterInfo myCritterInfo;
 
    public CollisionEnteredDelegate CollisionEntered = null;
    public CollisionExitedDelegate CollisionExited = null; 
 
    void OnTriggerEnter ( Collider other ) {
        if( CollisionEntered != null && myCritterInfo != null ) {
            CollisionEntered( myCritterInfo, other );
        }
    }
 
    void OnTriggerExit (Collider other) {
        if( CollisionExited != null && myCritterInfo != null ) {
            CollisionExited( myCritterInfo, other );
        }
    }
}
