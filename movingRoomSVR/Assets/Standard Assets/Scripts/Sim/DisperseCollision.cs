using UnityEngine;
using System.Collections;

public class DisperseCollision : MonoBehaviour 
{
	private Transform myTransform;
	private Collider myCollider;
	private float myDisperseRadius;
//	private CritterInfo myCritterInfo;
	private GeneralSpeciesData myGSD;
	private GeneralMotionData myGMD;
	// Use this for initialization
	public static bool dispersalBlocked = false;

    public bool interactive = false;
	
	void Awake () 
	{
//		Debug.Log("Awake");
        if (interactive)
        {
            myTransform = transform;
        }
        else
        {
            myTransform = transform.parent;
        }

        myCollider = GetComponent<Collider>();
        myDisperseRadius = GetColliderRadius(myCollider, interactive);

//		Debug.Log("myTransform " + myTransform.gameObject.name);
		myGSD = myTransform.GetComponent<GeneralSpeciesData>();
//		Debug.Log("myGSD ");
		myGMD = myTransform.GetComponent<GeneralMotionData>();
//		Debug.Log("myGMD ");
//		myCritterInfo = myGSD.myCritterInfo;
	}
	
	float GetColliderRadius(Collider c, bool bInteractive)
    {
        CapsuleCollider cc = c as CapsuleCollider;

        if (cc != null) 
        {
            return cc.radius;
        }
        else if (bInteractive)
        {
            SphereCollider sc = c as SphereCollider;

            if (sc != null)
            {  
                return sc.radius;
            }
        }

        return 0f;
    }

	void OnTriggerEnter (Collider other) 
	{
		if( dispersalBlocked )
        {
			return;
		}
		
		//WemoLog.Eyal("myname " + myTransform.gameObject.name );
		//WemoLog.Eyal("other name  " + other.gameObject.name );
		GeneralSpeciesData gsd = other.GetComponent<GeneralSpeciesData>();
		if (gsd == null && other.transform.parent != null)
		{
			gsd = other.transform.parent.GetComponent<GeneralSpeciesData>();
		}

		GeneralMotionData gmd = other.GetComponent<GeneralMotionData>();
		if( gmd == null && other.transform.parent != null ) 
        {
			gmd = other.transform.parent.GetComponent<GeneralMotionData>();
		}

        if (gsd == null ||
            gmd == null ||
            gsd.myCritterInfo == null ||
            gsd == myGSD)
        {
            return;
        }

        if (interactive)
        {
            if (gsd.myCritterInfo.swimDisperseData != null &&
                gsd.myCritterInfo.swimDisperseData.playerDisperseDisableCount <= 0)
            {
//                Debug.Log("Player caused disperse with " + other.name);
                Disperse(gsd, null, myTransform, myDisperseRadius, true);
            }

            return;
        }
        else
        {
            if (gsd.myCritterInfo.swimDisperseData != null &&
                gsd.myCritterInfo.swimDisperseData.critterDisperseDisableCount > 0)
            {
                return;
            }
        }
                    // certain anim states may want to disable disperse.  such as shark bite killing player
        if (gsd.myCritterInfo.animBase == null || !gsd.myCritterInfo.animBase.AllowDisperse)
        {
            return;
        }

		//WemoLog.Eyal(" TEST   "  + myGSD.myCritterInfo.critterObject.name + " disperse " + gsd.myCritterInfo.critterObject.name);
		if(gmd.critterBoxColliderSize.z > myGMD.critterBoxColliderSize.z * myGSD.disperseIfSmallerThanFactor) 
        {
            return;
        }

		// if me or other is pursuing a scripted target, don't disperse
		if (myGSD.myCritterInfo.swimTargetedData.HasScriptTarget() || gsd.myCritterInfo.swimTargetedData.HasScriptTarget()) 
        {
            return;
        }

        // if we can ionteract in vr, disperse the other fish
        if (gsd.myCritterInfo.critterItemData.vrInteractable &&
            gsd.myCritterInfo.critterItemData.Hovering())
        {
            Disperse(myGSD, gsd.myCritterInfo, gsd.myCritterInfo.critterTransform, GetColliderRadius(gsd.myCritterInfo.critterCollider, false), false);
        }
        else
        {
            Disperse(gsd, gsd.myCritterInfo, myTransform, myDisperseRadius, false);
    		
    		if(myGSD.isHungry &&
               (myGSD.myCritterInfo.swimSchoolFollowData == null || myGSD.myCritterInfo.swimSchoolFollowData.state != SwimSchoolFollowData.SchoolState.Manual) &&
               myGSD.myCritterInfo.CanEat(myGSD.myCritterInfo))
    		{
    //			Debug.Log(myGSD.myCritterInfo.critterTransform + " Disperse changed movingTarget to " + gsd.myCritterInfo.critterTransform);
    			myGSD.myCritterInfo.swimTargetedData.movingTarget = gsd.myCritterInfo.critterTransform;
    			myGSD.myCritterInfo.swimTargetedData.savedTargetDirection = gsd.myCritterInfo.critterTransform.position - myTransform.position;
    		}
        }		
//		WemoLog.Eyal(myGSD.myCritterInfo.critterObject.name + " disperse " + gsd.myCritterInfo.critterObject.name);
//		EventManager.dispersalEventIncrease(myTransform);
	}
	
	void OnTriggerExit (Collider other) 
	{
		GeneralSpeciesData gsd = other.GetComponent<GeneralSpeciesData>();
		if( gsd == null && other.transform.parent != null ) 
        {
			gsd = other.transform.parent.GetComponent<GeneralSpeciesData>();
		}

		GeneralMotionData gmd = other.GetComponent<GeneralMotionData>();
		if (gmd == null && other.transform.parent != null)
		{
			gmd = other.transform.parent.GetComponent<GeneralMotionData>();
		}

        if (gsd == null ||
            gmd == null ||
            gsd.myCritterInfo == null ||
            gsd == myGSD ||
            interactive)
        {
            return;
        }

		// don't switch behavior here if scripted
        if ((myGSD != null && myGSD.myCritterInfo != null && myGSD.myCritterInfo.swimTargetedData != null && myGSD.myCritterInfo.swimTargetedData.HasScriptTarget()) ||
            (gsd.myCritterInfo.swimTargetedData != null && gsd.myCritterInfo.swimTargetedData.HasScriptTarget()))
        {
            return;
        }

        //      WemoLog.Eyal(myTransform.gameObject.name + " trigger exit " + gsd.myCritterInfo.critterObject.name);
//        if (gsd.myCritterInfo.swimDisperseData.extraColliderDistMult < 1.00001f)
        {
    		gmd.isDispersed = false;
        	gmd.isBeingChased = false;
		    gsd.switchBehavior = true;
        }
	}

    static public void Disperse(GeneralSpeciesData gsd, CritterInfo disperseCritter, Transform disperseXForm, float disperseRadius, bool bMoving)
    {
        SwimDisperseData sd = gsd.myCritterInfo.swimDisperseData;
        if (sd != null && sd.useBounds)
        {
            // means we're in a scripted disperse
             return;
        }

        sd.checkMovingTarget = bMoving || (disperseCritter != null && disperseCritter.swimDisperseData != null && disperseCritter.swimDisperseData.disperseFromMovingTarget);
        sd.useBounds = false;

        GeneralMotionData gmd = gsd.myCritterInfo.generalMotionData;
        gmd.myDisperseCritter = disperseCritter;
        gmd.myDisperseXform = disperseXForm;
        gmd.isDispersed = true;
        gmd.isBeingChased = false;
        gmd.disperseRadius = disperseRadius;
        gsd.switchBehavior = true;
    }
}
