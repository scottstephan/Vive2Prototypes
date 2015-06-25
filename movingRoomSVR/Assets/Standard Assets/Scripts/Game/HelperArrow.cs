using UnityEngine;
using System.Collections;

public class HelperArrow : MonoBehaviour {

    public GameObject[] arrows;

    private Transform myTransform = null;
    private Vector3 myForward;

    private float minTimeInZone = 15f;
    private float timeInZone = 0f;

    private bool inZone = false;
    private bool arrowsOn = false;

    // activate the arrows
    private float maxTimeMovingWrongDirection = 1f;
    private float timeMovingWrongDirection = 0f;

    private float maxTimeLookingWrongDirection = 10f;
    private float timeLookingWrongDirection = 0f;

    private float wrongDirectionDot = -0.1f;

    // deactivate the arrows
    private float minTimeMovingRightDirectiion = 0.25f;
    private float timeMovingRightDirection = 0f;

    private float rightDirectionDot = 0.5f;

    void ActivateArrows( bool on ) {
        arrowsOn = on;

        if( arrows == null || arrows.Length <= 0 ) {
            return;
        }
        
        for( int i = 0; i < arrows.Length; i++ ) {
            GameObject arrow = arrows[i];
            if( arrow == null ) {
                continue;
            }
            
            arrow.SetActive( on );
        }
    }

    // Use this for initialization
	void Start () {
        myTransform = transform;
        myForward = myTransform.forward;
        ActivateArrows( false );
	}
	
	// Update is called once per frame
	void Update () {
        if( !inZone ) {
            return;
        }

        float dt = Time.deltaTime;
        timeInZone += dt;
        if( timeInZone <= minTimeInZone ) {
            return;
        }

        Vector3 movedir = OculusFPSCameraMode.singleton._curSpeed;
        float movemag = movedir.magnitude;

        // check directionality to player
        Vector3 dir = CameraManager.GetCurrentCameraFlattenedForward();
        float look_dot = Vector3.Dot( dir, myForward );

        if( arrowsOn ) {
            if( movemag > 0.05f && look_dot > rightDirectionDot ) {
                timeMovingRightDirection += dt;
                if( timeMovingRightDirection > minTimeMovingRightDirectiion ) {
                    ActivateArrows( false );
                    timeInZone = 0f; // reset our inzone timer.
                }
            }
            else {
                timeMovingRightDirection = 0f;
            }
        }
        else {
            if( movemag > 0.05f && look_dot < wrongDirectionDot ) {
                timeMovingWrongDirection += dt;
                if( timeMovingWrongDirection > maxTimeMovingWrongDirection ) {
                    ActivateArrows( true );
                }
            }
            else {
                timeMovingWrongDirection = 0f;
            }

            if( look_dot < wrongDirectionDot ) {
                timeLookingWrongDirection += dt;
                if( timeLookingWrongDirection > maxTimeLookingWrongDirection ) {
                    ActivateArrows( true );
                }
            }
            else {
                timeLookingWrongDirection = 0f;
            }
        }
	}

    void OnTriggerEnter (Collider other) 
    {
        // this really should only ever be the player.. but check just in case..
        if( other != CameraManager.singleton.ovrPlayerBody ) {
            return;
        }

        if( !inZone ) {
            timeInZone = 0f;
        }
        inZone = true;
    }

    void OnTriggerExit (Collider other) 
    {
        // this really should only ever be the player.. but check just in case..

        // this really should only ever be the player.. but check just in case..
        if( other != CameraManager.singleton.ovrPlayerBody ) {
            return;
        }
        
        inZone = false;
        ActivateArrows( false );
    }
}