using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CritterInfo {
    public CritterInfoData critterInfoData; // build process fills out below..removes lots of get components

	public GameObject critterObject;
	public Transform critterTransform;
	public Collider critterCollider;
	public LODModelData critterLODData;
	public Animation critterAnimation;
    public CritterAnimationBase animBase;
    public FishAudioData audioData;
	public FishBendControllerData critterBendData;
	public EyeControllerData critterEyeData;
	public SwimIdleData swimIdleData;
	public SwimIdleBiteData swimIdleBiteData;
	public SwimDisperseData swimDisperseData;
	public SwimChaseData swimChaseData;
	public SwimSchoolFollowData swimSchoolFollowData;
	public SwimTargetedData swimTargetedData;
	public SwimToPointData swimToPointData;
	public SwimFreefallData swimFreefallData;
	public SwimStrafingData swimStrafingData;
	public SwimParkingData swimParkingData;
	public SwimFreeData swimFreeData;
	public SwimPlayerInteractData swimPlayerInteractData;
    public SwimPlayerViewData swimPlayerViewData;
    public SwimFollowPathData swimFollowPathData;
    public SwimScriptGoToPointData swimScriptGoToPointData;
    public SwimStrafePlayerData swimStrafePlayerData;
    public HoldData holdData;
    public DeadData deadData;
	public GeneralSpeciesData generalSpeciesData;
	public GeneralMotionData generalMotionData;
	public InteractionData interactionData;
	public ViewportMotionData viewportMotionData;
	public CircleAroundObjectData circleAroundObjectData;
	public AwarenessCollision awarenessCollision;
	public SteeringBase critterSteering;
	public BoxCollider critterBoxCollider;
	public WemoItemData critterItemData;
	public DataItem itemData;

    // updated in late update.
    public Vector3 cachedPosition;
    public Vector3 cachedForward;
    public Vector3 cachedRight;
    public Vector3 cachedUp;
    public Vector3 cachedEulers;
    public Quaternion cachedRotation;

//    public RealSpace3D.RealSpace3D_AudioSource binauralSource;

	// links into the database
	public int dbItemID;
	public int dbVariantID;
	public int dbOwnedID;

	public int masterIndex;
	public bool preMarkedForRemove;
	public bool markedForRemove;
	public float markedForRemoveCameraDist;
	public bool clickedOn;

	public bool isPlayer;
	public bool followIgnoreFishBowl;
	public int followCount;

	public bool ignoreAiAutoTriggers;

	// for reference by level data that needs to adjust fishies spawning, ai, etc.
	public string designGroupName;
    public int designGroupHash;
    public string targetDesignGroupName;

	public float lifetime;
//	public float maxLifetime;

	public int roamDest = 0;

	public bool isVisible
	{
		get
		{
			return critterLODData.curLOD.LODrenderer.isVisible;
		}
	}

	public bool IsInSchool(CritterInfo c)
	{
		if (c == null ||
		    this.swimSchoolFollowData == null ||
		    this.swimSchoolFollowData.leaderCritterInfo == null ||
		    c.swimSchoolFollowData == null ||
		    c.swimSchoolFollowData.leaderCritterInfo == null)
		{
			return false;     
		}
		
		return c.swimSchoolFollowData.leaderCritterInfo.critterObject == swimSchoolFollowData.leaderCritterInfo.critterObject ||
			c.swimSchoolFollowData.leaderCritterInfo.critterObject == this.critterObject ||
				this.swimSchoolFollowData.leaderCritterInfo.critterObject == c.critterObject;
	}

    public bool CanEat(CritterInfo c)
    {
        if (string.IsNullOrEmpty(targetDesignGroupName) ||
            (!string.IsNullOrEmpty(c.designGroupName) && string.Compare(targetDesignGroupName, c.designGroupName, System.StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            if (generalMotionData.critterBoxColliderSize.z * generalSpeciesData.maxSizeToEatFactor > c.generalMotionData.critterBoxColliderSize.z
                && generalMotionData.critterBoxColliderSize.z * generalSpeciesData.minSizeToEatFactor < c.generalMotionData.critterBoxColliderSize.z)
            {
                return true;
            }
        }

        return false;
    }

    public void UpdateCachedTransformData()
    {
        cachedPosition = critterTransform.position;
        cachedRotation = critterTransform.rotation;
        cachedForward = critterTransform.forward;
        cachedUp = critterTransform.up;
        cachedRight = critterTransform.right;
        cachedEulers = cachedRotation.eulerAngles;
    }

    public delegate void OnRemoved(CritterInfo removedCritter);
    public OnRemoved removedCallback;    
}

[System.Serializable]
public class CritterToAdd {
	public Object critter;
	public int amount;
	public int variantID;
	public int ownedID;
	public string designGroupName;
    public int designGroupHash;
}


public class SimInstance : MonoBehaviour {
	// TODO>this singleton and Instance needs to go away
	private static SimInstance singleton;
	public static SimInstance Instance {
		get {
            if( singleton != null ) {
                return singleton;
            }

			if (singleton == null) {
				// This is where the magic happens.
				//  FindObjectOfType(...) returns the first AManager object in the scene.
				singleton =  FindObjectOfType(typeof (SimInstance)) as SimInstance;
				if( singleton != null ) {
//					log.Trace ("SimInstance object found and assigned.");
				}
			}

			// If it is still null, create a new instance
			if (singleton == null) {
				GameObject obj = new GameObject("SimInstance");
				singleton = obj.AddComponent(typeof (SimInstance)) as SimInstance;
//				log.Trace ("SimInstance was Generated Automaticly.");
			}

			return singleton;
		}
	}
	
	private bool paused = false;

	private bool runningCompare = false;
	private bool addingCritters = false;

    [HideInInspector]
    public bool slowdownActive = false;
    public static float slowdownMultiplier = 15f;
    public static float slowdownMultiplierInv = 1f/15f;

	[HideInInspector]
	public static bool isVerifyAssetMode = false;
	[HideInInspector]
	public static string verifySpeciesName = null;
	[HideInInspector]
	public static string verifyCritterName = null;
	[HideInInspector]
	public static string verifyBuildDate = null;
	[HideInInspector]
	public static string verifyItemData = null;	

	[HideInInspector]
	public int crittersInPopulation; // ee changed to public to get access from other classes
	[HideInInspector]
	public CritterInfo[] critters;  // ee changed to public to get access from other classes

	private int populationTotalCount = 500;
	
	private List<CritterToAdd> crittersToAdd;

	public float realTimeRunningSimInSphere = 0f;
	
	public float speedMult = 1f;

	public static Vector3 debugGUIOffset = new Vector3(0.0f, 0.1f, 0.0f);

	// class log
//	private static Log log = Log.GetLog(typeof(SimInstance).FullName);

	void Awake() {
		crittersToAdd = new List<CritterToAdd>();
		
		critters = new CritterInfo[populationTotalCount];
		for( int i=0; i<populationTotalCount; i++ ) {
			 critters[i] = new CritterInfo();
		}
		crittersInPopulation = 0;
		
		if( singleton == null ) {
			singleton = this;
		}
	}
		
	public void DidPause( bool new_value ) {
		paused = new_value;
	}
	
	public bool IsSimPaused() {
		return paused;
	}
	
    public void SlowdownCritters( bool active, bool not_from_sim ) {
        bool set_it = ( active != slowdownActive );
        if( !set_it ) {
            if( not_from_sim ) {
                set_it = true;
            }
            else {
                int i = GetNumCrittersInBehavior( SwimBehaviorType.SWIM_PLAYER_VIEW );
                if( i == 1 ) { // its only me.. turn on the effect
                    set_it = true;
                }
            }
        }

        if( set_it ) {
            slowdownActive = active;

            GlobalOceanShaderAdjust.Instance.SetClickDarkEffect( active );

            // if we're slowed down, don't ingore small critter collider box layer
            Physics.IgnoreLayerCollision(22, 28, !active);
        }
    }

    public int GetNumCrittersInBehavior( SwimBehaviorType btype ) {
        int count = 0;
        for( int i=0; i < crittersInPopulation; i++ ) {
            CritterInfo critter = critters[i];
            if( critter.generalSpeciesData != null
               && critter.generalSpeciesData.myCurrentBehaviorType == btype ) {
                count++;
            }
        }
        
        return count;
    }
    

    public int GetNumCrittersExitingSceneInPopulation() {
        int count = 0;
        for( int i=0; i < crittersInPopulation; i++ ) {
            CritterInfo critter = critters[i];
            if( critter.generalSpeciesData != null
               && critter.generalSpeciesData.isExitingScene ) {
                count++;
            }
        }
        
        return count;
    }
    
    public int GetNumUnownedCrittersInPopulation() {
		int count = 0;
		for( int i=0; i<crittersInPopulation; i++ ) {
			CritterInfo critter = critters[i];
			if( critter.dbOwnedID <= 0 ) {
				count++;
			}
		}

		return count;
	}

	public int GetNumCrittersOfVariantInPopulation( int variant_id ) {
		int count = 0;
		for( int i=0; i<crittersInPopulation; i++ ) {
			CritterInfo critter = critters[i];
			if( critter.dbVariantID == variant_id ) {
				count++;
			}
		}

		return count;
	}

	public int GetNumNonNativeInSphere() {
		int count = 0;
		for( int i=0; i<crittersInPopulation; i++ ) {
			int critter_sphere_id = App.FishManager.GetLegacySphereIdByFishId(critters[i].dbVariantID);
			if( critter_sphere_id != App.SphereManager.LEGACY_GetCurrentSphere() ) {
				count++;
			}
		}

		return count;
	}


	public int GetNumOwnedCrittersOfItemInPopulation( int item_id ) {
		int count = 0;
		for( int i=0; i<crittersInPopulation; i++ ) {
			CritterInfo critter = critters[i];
			if( critter.dbItemID == item_id
			   && critter.dbOwnedID > 0 ) {
				count++;
			}
		}

		return count;
	}

	public void GetVariantCountsInScene( Hashtable variants ) {
		variants.Clear();
		for( int i = 0; i < crittersInPopulation; ++i ) {
			CritterInfo critter = critters[ i ];
			if( !variants.ContainsKey( critter.dbVariantID ) )
			{
				variants.Add( critter.dbVariantID, 1 );
			}
			else
			{
				variants[ critter.dbVariantID ] = ( (int) variants[ critter.dbVariantID ] ) + 1;
			}
		}
	}

	public CritterInfo GetCritterInfoForObject( GameObject obj ) {
		for( int i = 0; i < crittersInPopulation; i++ ) {
			CritterInfo critter = critters[i];
			if( critter.critterObject == obj ) {
				return critter;
			}
		}

		return null;
	}

	public CritterInfo GetCritterInfoFromIndex( int idx ) {
		if( idx >= 0 && idx < crittersInPopulation ) {
			return critters[idx];
		}

		return null;
	}



	public CritterInfo GetRandomCritterInfoWithItemId( int itemID ) {
		int count = 0;
		for( int i = 0; i < crittersInPopulation; i++ ) {
			if( critters[i].dbItemID == itemID ) {
				count++;
			}
		}

		// grab a random one that we found.
		if( count > 0 ) {
			int rnd = Random.Range(0,count);
			int cnt = 0;
			for( int i = 0; i < crittersInPopulation; i++ ) {
				if( critters[i].dbItemID == itemID ) {
					if( rnd == cnt ) {
						return singleton.critters[i];
					}
					else {
						cnt++;
					}
				}
			}
		}

		return null;
	}

	public GameObject GetRandomCritterObjectWithItemId( int itemID ) {
		CritterInfo critter = GetRandomCritterInfoWithItemId( itemID );
		if( critter != null ) {
			return critter.critterObject;
		}

		return null;
	}

	public CritterInfo GetNonOwnedCritterWithVariantId( int variantID ) {
		for( int i = 0; i < crittersInPopulation; i++ ) {
			CritterInfo critter = critters[i];
			if( critter.dbVariantID == variantID
			   && critter.dbOwnedID <= 0 ) {
				return critter;
			}
		}

		return null;
	}

	public CritterInfo GetOwnedCritterWithOwnedID( int ownedID ) {
		for( int i = 0; i < crittersInPopulation; i++ ) {
			CritterInfo critter = critters[i];
			if( critter.dbOwnedID == ownedID ) {
				return critter;
			}
		}
		return null;
	}

	public CritterInfo GetCritterWithVariantId( int variantID ) {
		for( int i = 0; i < crittersInPopulation; i++ ) {
			CritterInfo critter = critters[i];
			if( critter.dbVariantID == variantID ) {
				return critter;
			}
		}

		return null;
	}

	public CritterInfo GetRandomCritterInfoWithVariantId( int variantID ) {
		int count = 0;
		CritterInfo critter;
		for( int i = 0; i < crittersInPopulation; i++ ) {
			critter = critters[i];
			if( critter.dbVariantID == variantID ) {
				count++;
			}
		}

		// grab a random one that we found.
		if( count > 0 ) {
			int rnd = Random.Range(0,count);
			int cnt = 0;
			for( int i = 0; i < crittersInPopulation; i++ ) {
				critter = critters[i];
				if( critter.dbVariantID == variantID ) {
					if( rnd == cnt ) {
						return singleton.critters[i];
					}
					else {
						cnt++;
					}
				}
			}
		}

		return null;
	}

	public void GetCrittersByName( string startsWith, List<CritterInfo> results ) {

		if (startsWith == null ||
			results == null)
		{
			return;
		}

		CritterInfo c;

        for( int i = 0; i < crittersInPopulation; i++ ) {
			c = critters[i];
			if (c != null &&
				c.critterObject != null &&
				c.critterObject.name.StartsWith(startsWith, System.StringComparison.InvariantCultureIgnoreCase))
			{
				results.Add(c);
			}
		}
	}

    public void GetCrittersByDesignGroup( string designGroupName, List<CritterInfo> results ) {
        
        if (designGroupName == null)
        {
            return;
        }
        
        int hash = designGroupName.ToUpper().GetHashCode();

        GetCrittersByDesignGroup(hash, results);
    }

	public void GetCrittersByDesignGroup( int designGroupHash, List<CritterInfo> results ) {
		
		if (results == null)
		{
			return;
		}

		CritterInfo c;
		
        for( int i = 0; i < crittersInPopulation; i++ ) 
		{
			c = critters[i];
			if (c != null &&
				c.critterObject != null &&
                c.designGroupHash == designGroupHash)
			{
				results.Add(c);
			}
		}
	}

    public CritterInfo GetCritterInDesignGroup( string designGroupName ) 
    {
        if (designGroupName == null)
        {
            return null;
        }
        
        int hash = designGroupName.ToUpper().GetHashCode();
        return GetCritterInDesignGroup(hash);
    }
        

	public CritterInfo GetCritterInDesignGroup( int designGroupHash ) {
		
        for( int i = 0; i < crittersInPopulation; i++ ) 
		{
			CritterInfo c = critters[i];
			if (c != null &&
				c.critterObject != null &&
                c.designGroupHash == designGroupHash)
			{
				return c;
			}
		}

		return null;
	}

	public GameObject GetRandomCritterObjectWithVariantId( int variantID ) {
		CritterInfo critter = GetRandomCritterInfoWithVariantId( variantID );
		if( critter != null ) {
			return critter.critterObject;
		}

		return null;
	}

	public int GetSchoolFollowerCount(CritterInfo critter_info)
	{
		int count = 0;

        for (int i=0; i<crittersInPopulation; ++i)
		{
			CritterInfo c = critters[i];

			if (c == null ||
				c.critterObject == null ||
				c == critter_info)
			{
				continue;
			}

			while (c != null && c.swimSchoolFollowData != null)
			{
				if (c.swimSchoolFollowData.leaderCritterInfo != null &&
					c.swimSchoolFollowData.leaderCritterInfo.critterObject == critter_info.critterObject)
				{
					++count;
					break;
				}

				c = c.swimSchoolFollowData.leaderCritterInfo;
			}
		}

		return count;
	}

    public void GetSchoolCritters(CritterInfo critter_info, List<CritterInfo> school)
    {
        for (int i=0; i<crittersInPopulation; ++i)
        {
            CritterInfo c = critters[i];
            
            if (c == null ||
                c.critterObject == null ||
                c == critter_info)
            {
                continue;
            }
            
            if (c.swimSchoolFollowData.leaderCritterInfo == null)
            {
                continue;
            }

            if (c.swimSchoolFollowData.leaderCritterInfo.critterObject == critter_info.critterObject)
            {
                school.Add (c);
                continue;
            }

            if (critter_info.swimSchoolFollowData.leaderCritterInfo != null &&
                critter_info.swimSchoolFollowData.leaderCritterInfo.critterObject == c.swimSchoolFollowData.leaderCritterInfo.critterObject)
            {
                school.Add (c);
                continue;
            }
        }
    }

	public bool AddCrittersIsActive() {
		return (runningCompare
		   || addingCritters
		   || crittersToAdd.Count > 0);
	}
	
	public int GetNumAddCritters() {
		int ret_val = 0;
		foreach( CritterToAdd ct in crittersToAdd ) {
			if( ct != null ) {
				ret_val += ct.amount;
			}
		}
		
		return ret_val;	
	}

	public bool IsOwnedItemInAddCritters( int owned_id ) {
		foreach( CritterToAdd ct in crittersToAdd ) {
			if( ct != null && ct.ownedID == owned_id ) {
				return true;
			}
		}
			
		return false;
	}
	
    void CritterInfoGetComponents( CritterInfo new_info, GameObject critter ) {
        new_info.critterTransform = critter.transform;
        new_info.critterCollider = critter.GetComponent<Collider>();
        new_info.critterAnimation = critter.GetComponent<Animation>();
        new_info.critterLODData = critter.GetComponent<LODModelData>();
        new_info.audioData = critter.GetComponent<FishAudioData>();

        // upgrade old FishAnimationData components to new CritterAnimationBase
        new_info.animBase = critter.GetComponent<CritterAnimationBase>();
        if( new_info.animBase == null ) {
            OGFishAnimation.CreateFromDeprecatedData( new_info );
        }
        new_info.swimSchoolFollowData = critter.GetComponent<SwimSchoolFollowData>();
        new_info.swimTargetedData = critter.GetComponent<SwimTargetedData>();
        new_info.swimToPointData = critter.GetComponent<SwimToPointData>();
        new_info.swimIdleData = critter.GetComponent<SwimIdleData>();
        new_info.swimIdleBiteData = critter.GetComponent<SwimIdleBiteData>();
        new_info.swimDisperseData = critter.GetComponent<SwimDisperseData>();
        new_info.swimChaseData = critter.GetComponent<SwimChaseData>();
        new_info.swimFreefallData = critter.GetComponent<SwimFreefallData>();
        new_info.swimStrafingData = critter.GetComponent<SwimStrafingData>();
        new_info.swimParkingData = critter.GetComponent<SwimParkingData>();
        new_info.swimFreeData = critter.GetComponent<SwimFreeData>();
        new_info.swimPlayerInteractData = critter.GetComponent<SwimPlayerInteractData>();
        new_info.swimPlayerViewData = critter.GetComponent<SwimPlayerViewData>();
        new_info.swimStrafePlayerData = critter.GetComponent<SwimStrafePlayerData>();
        new_info.swimFollowPathData = critter.AddComponent<SwimFollowPathData>(); // this component added in code for all fish
        new_info.swimScriptGoToPointData = critter.AddComponent<SwimScriptGoToPointData>(); // this component added in code for all fish
        new_info.holdData = critter.AddComponent<HoldData>();
        new_info.deadData = critter.GetComponent<DeadData>();
        new_info.interactionData = critter.GetComponent<InteractionData>();
        new_info.viewportMotionData = critter.GetComponent<ViewportMotionData>();
        new_info.circleAroundObjectData = critter.GetComponent<CircleAroundObjectData>();
        new_info.critterBendData = critter.GetComponent<FishBendControllerData>();
        new_info.critterEyeData = critter.GetComponent<EyeControllerData>();
        new_info.generalSpeciesData = critter.GetComponent<GeneralSpeciesData>();
        new_info.generalMotionData = critter.GetComponent<GeneralMotionData>();
        
        new_info.critterSteering = critter.GetComponent<SteeringBase>();
        if( new_info.critterSteering == null ) {
            Log.Main.Error("Critter prefab (" + critter.name + ") does not have a steering component. Creating default and copying deprecated data from General Motion.");
            OGFishSteering.CreateFromGeneralMotion( new_info );
        }

        new_info.critterBoxCollider = critter.GetComponentInChildren<BoxCollider>();
        new_info.critterItemData = critter.GetComponent<WemoItemData>();
    }

    void SetupCritter( CritterInfo new_info, GameObject critter, int item_id, int variant_id, int owned_id, int user_id ) {
		new_info.critterObject = critter;
        new_info.critterInfoData = critter.GetComponent<CritterInfoData>();
        if( new_info.critterInfoData == null ) {
            CritterInfoGetComponents( new_info, critter );
        }
        else {
            // pass data up into critter info.
            CritterInfoData cid = new_info.critterInfoData;
            new_info.critterTransform = cid.transform;
            new_info.critterCollider = cid.GetComponent<Collider>();
            new_info.critterAnimation = cid.GetComponent<Animation>();
            new_info.critterLODData = cid.critterLODData;
            new_info.audioData = cid.audioData;            
            new_info.animBase = cid.animBase;
            new_info.swimSchoolFollowData = cid.swimSchoolFollowData;
            new_info.swimTargetedData = cid.swimTargetedData;
            new_info.swimToPointData = cid.swimToPointData;
            new_info.swimIdleData = cid.swimIdleData;
            new_info.swimIdleBiteData = cid.swimIdleBiteData;
            new_info.swimDisperseData = cid.swimDisperseData;
            new_info.swimChaseData = cid.swimChaseData;
            new_info.swimFreefallData = cid.swimFreefallData;
            new_info.swimStrafingData = cid.swimStrafingData;
            new_info.swimParkingData = cid.swimParkingData;
            new_info.swimFreeData = cid.swimFreeData;
            new_info.swimPlayerInteractData = cid.swimPlayerInteractData;
            new_info.swimPlayerViewData = cid.swimPlayerViewData;
            new_info.swimStrafePlayerData = cid.swimStrafePlayerData;
            new_info.swimFollowPathData = cid.swimFollowPathData;
            new_info.swimScriptGoToPointData = cid.swimScriptGoToPointData;
            new_info.holdData = cid.holdData;
            new_info.deadData = cid.deadData;
            new_info.interactionData = cid.interactionData;
            new_info.viewportMotionData = cid.viewportMotionData;
            new_info.circleAroundObjectData = cid.circleAroundObjectData;
            new_info.critterBendData = cid.critterBendData;
            new_info.critterEyeData = cid.critterEyeData;
            new_info.generalSpeciesData = cid.generalSpeciesData;
            new_info.generalMotionData = cid.generalMotionData;
            new_info.critterSteering = cid.critterSteering;
            new_info.critterBoxCollider = cid.critterBoxCollider;
            new_info.critterItemData = cid.critterItemData;
        }
        		
        new_info.generalSpeciesData.masterIndex = crittersInPopulation;
        new_info.animBase.critterInfo = new_info;		
		new_info.critterSteering.critterInfo = new_info;

		if( new_info.critterItemData != null ) {
			new_info.critterItemData.itemID = item_id;
			new_info.critterItemData.variantID = variant_id;
            new_info.critterItemData.critterInfo = new_info;
		}
		
        new_info.ignoreAiAutoTriggers = false;

        new_info.dbItemID = App.FishManager.GetLegacyItemIdByFishId( variant_id );
		new_info.dbVariantID = variant_id;
		new_info.dbOwnedID = owned_id;
		
		//RICHARD HEY LOOK TODO FIXME BLAH!!!
		// THIS IS WHERE WE DETERMINE WHETHER OR NOT A FISH HAS BEEN CLICKED ON OR NOT		
		if( owned_id <= 0 ){ //|| App.UserManager.query.OwnsItem( null, owned_id, UserManager.USER_ITEM_FISH ) ) {
			new_info.clickedOn = true;
		}
		else {
			new_info.clickedOn = false;
		}
		new_info.itemData = App.FishManager.GetParentByFishId( null,  variant_id );
		new_info.critterItemData.itemID = new_info.dbItemID;
		new_info.critterItemData.variantID = variant_id;
		new_info.markedForRemove = false;
		new_info.preMarkedForRemove = false;
		new_info.markedForRemoveCameraDist = 0f;

		// group all fish under the master ocean parent
		new_info.critterTransform.parent = transform;
		
/*		bool isOtherUsersFish = false;
		// check if user IDs are different and if critters name is null
        if( App.UserManager != null &&
            App.UserManager.me != null &&
            App.UserManager.me.userid != user_id &&
            critter_name != null )
        {
		//                Debug.Log("OTHER USERS FISH!");
			isOtherUsersFish = true;
		}*/
		
        // turn on the lit pieces and the unlit ones off
        new_info.critterLODData.SetupLitPieces( true );

		// create our debug gui pieces.
/*		new_info.generalSpeciesData.debugGUIText = new GameObject();
		new_info.generalSpeciesData.debugGUIText.transform.parent = DebugDisplay.singleton.gameObject.transform;
		new_info.generalSpeciesData.debugGUIText.AddComponent<GUIText>();
		new_info.generalSpeciesData.debugGUIText.guiText.alignment = TextAlignment.Center;
		new_info.generalSpeciesData.debugGUIText.guiText.anchor = TextAnchor.LowerCenter;
		new_info.generalSpeciesData.debugGUIText.guiText.enabled = true;*/
		
		if(isVerifyAssetMode){
/*			UILabel critterName = GameObject.Find("CritterName").GetComponent<UILabel>();
			UILabel speciesName = GameObject.Find("SpeciesName").GetComponent<UILabel>();
			UILabel buildDate = GameObject.Find("BuildDate").GetComponent<UILabel>();
			
			int index = new_info.critterObject.name.IndexOf('(');
			string name = new_info.critterObject.name.Remove(index);
			critterName.text = "Critter Name: " + name;
			speciesName.text = "Species Name: " + new_info.itemData.itemName;
			buildDate.text = "Build Date: " + new_info.critterItemData.buildInfoString;*/
			
			int index = new_info.critterObject.name.IndexOf('(');
			string name = new_info.critterObject.name.Remove(index);
			verifyItemData = "ItemID: " + new_info.dbItemID + "        VariantID: " + new_info.dbVariantID;
			verifyCritterName = "Critter Name: " + name;
			verifySpeciesName = "Species Name: " + new_info.itemData.name;
			verifyBuildDate = "Build Date: " + new_info.critterItemData.buildInfoString;			
		}

        // set our size before custom scale is applied
        float critterBoxColliderZ = new_info.critterBoxCollider.size.z * new_info.critterTransform.localScale.z;
        new_info.generalSpeciesData.mySize = GetSpeciesSize(critterBoxColliderZ);

        GeneralMotion.SetAvoidanceDelays(new_info);
	}

    static public SpeciesSize GetSpeciesSize(float z)
    {
        // set our size before custom scale is applied
        SpeciesSize retVal = SpeciesSize.TINY;
        
        if (z > 22)
        {
            retVal = SpeciesSize.SMALL;
        }
        
        if (z > 100) 
        {
            retVal = SpeciesSize.MEDIUM;
        }
        
        if (z > 200) 
        {
            retVal = SpeciesSize.LARGE;
        }
        
        if (z > 350)
        {
            retVal = SpeciesSize.HUGE;  
        }

        return retVal;
    }
	
	public CritterInfo AddCritterAtLocation( Object critter_to_add, Vector3 pos, Quaternion rot, float customScale, int variant_id ) {
		GameObject critter = Instantiate(critter_to_add, Vector3.zero, Quaternion.identity) as GameObject;

		critter.name += crittersInPopulation;
		CritterInfo new_info = critters[crittersInPopulation];
		new_info.lifetime = 0f;
		new_info.masterIndex = crittersInPopulation;

		int item_id = App.FishManager.GetLegacyItemIdByFishId( variant_id );

		SetupCritter( new_info, critter, item_id, variant_id, -1, -1 );

		//print(new_info.critterTransform.localScale.z);
		//new_info.critterTransform.localScale *= 0.8f+0.4f*Random.value;
        FishController.Start( new_info, customScale );

//		WemoLog.Eyal( new_info.critterObject.name + " is z " + new_info.generalMotionData.critterBoxColliderSize.z +  "  is size of " + new_info.generalSpeciesData.mySize);

		// TODO>make this function take an itemdata reference and optimize this set function out.
		App.FishManager.SetItemSize( null, variant_id, new_info.generalSpeciesData.mySize );

		SphereInstance.Instance.AssignCritterFishBowl( new_info );

		AI.Start( new_info, false );

        new_info.critterTransform.position = pos;
        new_info.critterTransform.rotation = rot;
        new_info.UpdateCachedTransformData();

		crittersInPopulation++;

		return new_info;
	}

    public void PlayBinauralAudio( CritterInfo critter ) {
/*        if( critter == null || critter.binauralSource == null ) {
            return;
        }

        Debug.Log("PLAYING BINAURAL AUDIO SOURCE ! ");
        critter.binauralSource.Play();
//        critter.binauralSource.rs3d_StartSoundSource();*/
    }
    
    public void StopBinauralAudio( CritterInfo critter ) {
/*        if( critter == null || critter.binauralSource == null  ) {
            return;
        }

        Debug.Log("STOPPING BINAURAL AUDIO SOURCE ! ");
//        critter.binauralSource.Stop();
//        critter.binauralSource.rs3d_StopSound();*/
    }
    
    public bool IsAddingCritters(string designGroupName)
    {
        int hash = designGroupName.ToUpper().GetHashCode();
        return IsAddingCritters(hash);
    }

	public bool IsAddingCritters(int designGroupHash)
	{
        for (int i=0; i<crittersToAdd.Count; ++i)
		{
            if (crittersToAdd[i].designGroupHash == designGroupHash)
			{
				return true;
			}
		}
		
		return false;
	}

	public void AddCritterFromSnapshot( Object critter_to_add, int variant_id, Vector3 pos, Quaternion rot ) {
		
		// adjust our critter in sphere data to include these new critters
		int item_id = App.FishManager.GetLegacyItemIdByFishId( variant_id );
		
		GameObject critter = Instantiate(critter_to_add, Vector3.zero, Quaternion.identity) as GameObject;
		
		critter.name += crittersInPopulation;
		CritterInfo new_info = critters[crittersInPopulation];
		new_info.lifetime = 0f;
		new_info.masterIndex = crittersInPopulation;
		SetupCritter( new_info, critter, item_id, variant_id, -1, -1 );
		
		//new_info.critterTransform.localScale *= 0.8f+0.4f*Random.value;
		FishController.Start( new_info, -1f );        
		
		new_info.critterTransform.position = pos;
		new_info.critterTransform.rotation = rot;
        		
		// TODO>make this function take an itemdata reference and optimize this set function out.
		App.FishManager.SetItemSize( null, variant_id, new_info.generalSpeciesData.mySize );
		
		App.FishManager.EnteredScene( new_info );
		
		SphereInstance.Instance.AssignCritterFishBowl( new_info );
		
		AI.Start( new_info, false );
		
		crittersInPopulation++;        
	}
	
	IEnumerator ReallyAddCrittersToPopulation( Object critter_to_add, int num_to_add,
											   int variant_id, int owned_id,
											   string designGroupName ) {
		int new_total = num_to_add + crittersInPopulation;
		if( addingCritters 
			|| new_total > populationTotalCount)
        {
			CritterToAdd add_me = new CritterToAdd();
			add_me.critter = critter_to_add;
			add_me.amount = num_to_add;
			add_me.variantID = variant_id;
			add_me.ownedID = owned_id;
			add_me.designGroupName = designGroupName;
            add_me.designGroupHash = designGroupName != null ? designGroupName.ToUpper().GetHashCode() : 0;
            crittersToAdd.Add(add_me);
//            Debug.Log("cant add, will do in a bit");
			yield break;
		}

		// adjust our critter in sphere data to include these new critters
//		int item_id = App.FishManager.GetLegacyItemIdByFishId( variant_id );

		addingCritters = true;

//		Debug.Log("adding " + num_to_add + " " + critter_to_add.name);

		//Called when game loads & when player purchases a fish
		//return something here that says whether it was collected or not
		for( int i = 0; i < num_to_add; i++ ) {
			
			// bail if we are loading a sphere.
			if( App.SphereManager.LEGACY_IsLoadingSphere()
			   && !AppBase.Instance.RunningAsPreview() ) {
				i = num_to_add;
				continue;
			}
			
            CritterInfo new_info = critters[crittersInPopulation];
            InitCritterInfo(new_info, critter_to_add, variant_id, owned_id, -1f, designGroupName, Vector3.zero);
            
            FishController.FindSpawnPosition( new_info, false );

			SphereInstance.Instance.AssignCritterFishBowl( new_info );

			FishController.FindSpawnPosition( new_info, false );

			AI.Start( new_info, false );
		
			crittersInPopulation++;

			//ee hack to make all fish glow
			//LODModelData.critterGlow(new_info,true);
   
			yield return null;
		}

//		DebugDisplay.AddDebugText("pop increased to " + crittersInPopulation);

		addingCritters = false;
	}

    public void InitCritterInfo(CritterInfo new_info, Object critter_to_add,
                                int variant_id, int owned_id, float customScale,
                                string designGroupName,
                                Vector3 pos) 
    {
        // adjust our critter in sphere data to include these new critters
        int item_id = App.FishManager.GetLegacyItemIdByFishId( variant_id );
        
        //      Debug.Log("adding " + num_to_add + " " + critter_to_add.name);
        
        //Called when game loads & when player purchases a fish
        //return something here that says whether it was collected or not
            
        GameObject critter = Instantiate(critter_to_add, pos, Quaternion.identity) as GameObject;

        if (!string.IsNullOrEmpty(designGroupName))
        {
            critter.name = designGroupName + " " + critter.name + crittersInPopulation;
        }
        else
        {
            critter.name += crittersInPopulation;
        }
        new_info.lifetime = 0f;
        new_info.masterIndex = crittersInPopulation;
        new_info.designGroupName = designGroupName;
        new_info.designGroupHash = designGroupName != null ? designGroupName.ToUpper().GetHashCode() : 0;
        new_info.isPlayer = false;
        new_info.followIgnoreFishBowl = false;
        new_info.followCount = 0;

        SetupCritter( new_info, critter, item_id, variant_id, owned_id, -1 );
        
        //print(new_info.critterTransform.localScale.z);
        //new_info.critterTransform.localScale *= 0.8f+0.4f*Random.value;
        FishController.Start( new_info, customScale );

        App.FishManager.SetItemSize( null, variant_id, new_info.generalSpeciesData.mySize );        
        App.FishManager.EnteredScene( new_info );
    }

	public void AddCrittersToPopulation( Object critter_to_add, int num_to_add, int variant_id, int owned_id, string designGroupName = null ) {
		StartCoroutine(ReallyAddCrittersToPopulation(critter_to_add,num_to_add,variant_id,owned_id,designGroupName));
	}

    public void AddCritterToPopulation(CritterInfo c)
    {
        if (crittersInPopulation >= critters.Length)
        {
            return;
        }

        addingCritters = true;
        critters[crittersInPopulation] = c;
        c.masterIndex = crittersInPopulation;
        GeneralMotion.SetAvoidanceDelays(c);
        ++crittersInPopulation;
        addingCritters = false;
    }

    private void RemoveCritterReferences( CritterInfo critter ) 
    {
        
        // go through the population and fixup critters that were referencing us for some reason/
        for( int i = 0; i < crittersInPopulation; i++ ) {
            CritterInfo clean_critter = critters[i];
            
            // skip ourselves first.
            if( clean_critter == critter ) 
            {
                continue;
            }
            
            if( clean_critter.markedForRemove ||
                clean_critter.dbVariantID == -1 ||
                clean_critter.critterObject == null)
            { 
                // dont check against any removed critters
                continue;
            }
            
            // get rid of any references to us as a predator
            if( clean_critter.generalMotionData.myDisperseXform == critter.critterTransform ) {
                clean_critter.generalMotionData.isDispersed = false;
                clean_critter.generalMotionData.isBeingChased = false;
                clean_critter.generalMotionData.myDisperseCritter = null;
                clean_critter.generalMotionData.myDisperseXform = null;
                clean_critter.generalSpeciesData.switchBehavior = true;
            }
            
            if( clean_critter.swimChaseData.victim == critter ) {
                clean_critter.swimChaseData.victim = null;
                clean_critter.generalSpeciesData.becameAgrressive = false;
                clean_critter.generalSpeciesData.becameNotHungry = true;
                clean_critter.generalSpeciesData.switchBehavior = true;
            }
            
            // get rid of any reference to us as a leader.. force us to go into random target.
            if( clean_critter.swimSchoolFollowData.leaderCritterInfo == critter ) {
                clean_critter.swimSchoolFollowData.leaderCritterInfo = null;
                clean_critter.generalSpeciesData.switchBehavior = true;
            }
            
            // get rid of any reference to us as a leader.. force us to go into random target.
            if( clean_critter.swimSchoolFollowData.leaderTransform == critter.critterTransform ) {
                clean_critter.swimSchoolFollowData.leaderTransform = null;
                clean_critter.generalSpeciesData.switchBehavior = true;
            }
            
            // get rid of any reference to us as a moving target
            if( clean_critter.swimTargetedData.movingTarget == critter.critterTransform ) {
                clean_critter.swimTargetedData.movingTarget = null;
                clean_critter.swimTargetedData.targetGSD = null;
                clean_critter.swimTargetedData.targetDeadData = null;
                clean_critter.generalSpeciesData.switchBehavior = true;
            }
        }
    }

	private void RemoveCritter( CritterInfo critter ) 
    {
        RemoveCritterReferences(critter);
		AI.End( critter );

        StopBinauralAudio( critter );
/* Disable random camera follow logic, breaking oculus follow cam logic, needs to be refactored completely, assumes random critter follow too much
		if( SimManager.cameraTargetIndex == critter.masterIndex ) {
			DebugDisplay.AddDebugText("OSC - cameraTargetIndex == masterIndex!");
			CameraManager.currentTarget = SimManager.GetRandomCameraTarget(false, false);

			if( CameraManager.CurrentCameraFollowsTargets() ) {
				DebugDisplay.AddDebugText("OSC - CURRENT CAMERA FOLLOWING A TARGET");
                OculusCameraFadeManager.StartCameraFadeFromBlack(1.5f,null,null);
				CameraManager.JumpToCameraOrder(1);
			}
		}
*/  

		LODManager.CleanupLOD( critter.critterLODData );
        critter.critterItemData.CleanUp();
        critter.animBase.CleanUp();

//		DestroyImmediate(critter.generalSpeciesData.debugGUIText);

		// get rid of reference to critterInfo
        critter.generalSpeciesData.myCritterInfo = null;
        critter.swimChaseData.victim = null;

		critter.critterObject = null;
        critter.critterInfoData = null;
		critter.critterTransform.parent = null;
		critter.critterTransform = null;
		critter.critterCollider = null;
		critter.critterAnimation = null;
		critter.critterLODData = null;
        critter.audioData = null;
        critter.animBase.critterInfo = null;
		critter.animBase = null;
		critter.swimSchoolFollowData = null;
		critter.swimTargetedData = null;
		critter.swimToPointData = null;
		critter.swimIdleData = null;
		critter.swimIdleBiteData = null;
		critter.swimDisperseData = null;
		critter.swimChaseData = null;
		critter.swimFreefallData = null;
		critter.swimStrafingData = null;
		critter.swimParkingData = null;
		critter.swimFreeData = null;
		critter.swimPlayerInteractData = null;
        critter.swimPlayerViewData = null;
        critter.swimStrafePlayerData = null;
        critter.swimFollowPathData = null;
        critter.swimScriptGoToPointData = null;
        critter.circleAroundObjectData = null;
        critter.holdData = null;
        critter.deadData = null;
		critter.viewportMotionData = null;
		critter.interactionData = null;
		critter.critterBendData = null;
		critter.critterEyeData = null;
		critter.generalSpeciesData.debugGUIText = null;
		critter.generalSpeciesData = null;
		critter.generalMotionData = null;
		critter.critterSteering.critterInfo = null;
		critter.critterSteering = null;
		critter.critterBoxCollider = null;
		critter.critterItemData = null;
		critter.itemData = null;
		critter.designGroupName = null;
        critter.designGroupHash = 0;

		if( critter.awarenessCollision != null ) {
			critter.awarenessCollision.myCritterInfo = null;
			critter.awarenessCollision = null;
		}

		critter.dbItemID = -1;
		critter.dbVariantID = -1;
		critter.dbOwnedID = -1;
		critter.markedForRemoveCameraDist = 0.0f;
		critter.markedForRemove = false;
	}

    static public void ForceRemoveCrittersIfInstance() 
    {
        if (singleton == null)
        {
            return;
        }

        singleton.ForceRemoveCritters();
    }
    

	public void ForceRemoveCritters() {
		for( int i = 0; i < crittersInPopulation; i++ ) {
			CritterInfo critter = critters[i];
			critter.markedForRemove = true;
		}
		// make sure we are not adding any critters.
		crittersToAdd.Clear();
		crittersToAdd.TrimExcess();
	}
	
	public void AllCrittersLeaveScene() {
		for( int i = 0; i < crittersInPopulation; i++ ) {
			CritterInfo critter = critters[i];
			LeaveScene(critter,false,-1);
		}
	}
 
	public void ForceExitingCrittersOut() {
		for( int i = 0; i < crittersInPopulation; i++ ) {
			CritterInfo critter = critters[i];
			if( critter != null 
			   && critter.generalSpeciesData != null
			   && critter.generalSpeciesData.isExitingScene ) {
				critter.markedForRemove = true;
			}
		}
	}

	public void ForceAllLocallyOwnedCrittersToLeave() {
		for( int i=0; i<crittersInPopulation; i++ ) {
			CritterInfo critter = critters[i];

			// skip unowned and critters currently marked for removal
			if( critter.dbOwnedID <= 0
			   || critter.markedForRemove
			   || ( critter.generalSpeciesData != null
				   && critter.generalSpeciesData.isExitingScene ) ) {
				continue;
			}

			LeaveScene(critter,true,-1);
		}

	}
	
	// too many critters..
	public void ForceTheOldestCritterToLeave() {
//        Debug.Log("trying to force an old to leave." + owned + " " + collected);
		CritterInfo oldest = null;
		float oldest_lifetime = 0f;

		for( int i=0; i<crittersInPopulation; i++ ) {
			CritterInfo critter = critters[i];
			
			if( !critter.markedForRemove && critter.lifetime > oldest_lifetime ) {
				oldest_lifetime = critter.lifetime;
				oldest = critter;
			}
		}

		if( oldest != null ) {
			oldest.markedForRemove = true;
		}
	}
//        Debug.Log("trying to force an old to leave." + owned + " " + collected);
	// TODO (Dynamic Critter)> hook up the oldest percentage so that we randomly choose a critter
	// thats age is in the oldest percentage.
	public CritterInfo ForceAnOldCritterToLeave( bool owned, bool collected, float oldest_percentage, float min_lifetime ) {
//        Debug.Log("trying to force an old to leave." + owned + " " + collected);
		CritterInfo oldest = null;
		float oldest_lifetime = 0f;
		bool follows_targets = CameraManager.CurrentCameraFollowsTargets();

		for( int i=0; i<crittersInPopulation; i++ ) {
			CritterInfo critter = critters[i];

			// we cannot force the critter we are following to leave.
			if( follows_targets
			   && SimManager.GetCritterForCameraTarget() == critter ) {
				continue;
			}

			if( critter.lifetime < min_lifetime ) {
				continue;
			}

			// if set to force an owned critter to leave.. make sure the critter is owned.
			if( owned && critter.dbOwnedID <= 0 ) {
				continue;
			}
			// if set to force a collected critter to leave.. make sure it is a collected critter.
			if( collected ) {
				// skip owned critters.
				if( critter.dbOwnedID > 0 ) {
					continue;
				}
				// skip limited editions
				if( App.FishManager.IsLimitedEdition( null, critter.dbVariantID) ) {
					continue;
				}
			}
			// always skip and critters currently marked for removal
			if( critter.markedForRemove
			   || ( critter.generalSpeciesData != null
				   && critter.generalSpeciesData.isExitingScene ) ) {
				continue;
			}
			if( critter.lifetime > oldest_lifetime ) {
				oldest_lifetime = critter.lifetime;
				oldest = critter;
			}
		}

		// TODO> Add a random chance that more in the school of fish
		// following this fish will also leave

		if( oldest != null ) {
			// randomly go into the blue vs behind the camera.
			float rnd = Random.value;
//			Debug.Log("FORCE OLD FISH TO LEAVE: " + oldest.dbVariantID + " " + oldest.dbOwnedID + " " + owned + " " + collected);
			LeaveScene( oldest, rnd < 0.7f, -1 ); // randomly go into the blue
			return oldest;
		}

		return null;
	}

	public void LeaveScene( CritterInfo critter, bool into_the_blu, int sphere_id, bool go_off_camera=false ) {
		if( CameraManager.IsInIntroCamMode() || critter == null ) {
			return;
		}

		if( critter.critterLODData.curLOD.LODrenderer.isVisible || sphere_id >= 0 ) {
			GeneralSpeciesData gsd = critter.generalSpeciesData;
			gsd.isExitingScene = true;
			if( critter.swimToPointData.pointReachedType != PointReachedType.ExitScene ) {
				gsd.swimToPoint = true;

                if (AI.IsSingletonBehavior(gsd.myCurrentBehaviorType))
                {
                    AI.ForceSwitchToBehavior(critter, SwimBehaviorType.SWIM_TO_POINT);
                }
                else
                {
                    gsd.switchBehavior = true;
                }

				critter.swimToPointData.pointReachedType = PointReachedType.ExitScene;
				gsd.searchNewLeaderCounter = 0;
				gsd.hungerLevel = 0;
				if( into_the_blu || go_off_camera ) {
					Vector3 dir = critter.critterTransform.forward;
					if( go_off_camera ) {
						dir = CameraManager.GetCurrentCameraForward();
						float swap = dir.x;
						dir.x = dir.z;
						dir.z = swap;
					}
					dir.y = 0f;
					dir.Normalize();
					Vector3 target_pos = critter.critterTransform.position;
					target_pos += dir * GlobalOceanShaderAdjust.CurrentDistance() * 2f;
//					target_pos.y = -150f;
					critter.swimToPointData.targetPosition = target_pos;
					critter.swimToPointData.savedTargetDirection = target_pos - critter.critterTransform.position;

					//Set time till leaving scene based on roaming (sphere_id == -1) or traveling
					if (sphere_id == -1)
						critter.swimToPointData.blockCameraSwitchTimer = 4f;
					else
						critter.swimToPointData.blockCameraSwitchTimer = 4f;
				}
				else {
					Transform trans = CameraManager.GetCurrentCameraTransform();
					float on_right = 1.0f;
					Vector3 tmp = critter.critterTransform.position - trans.position;
					float dot = Vector3.Dot(tmp,trans.right);
					if( dot < 0.0f ) {
						on_right = -1;
					}
					float rad = critter.generalMotionData.critterBoxColliderRadius;
					Vector3 target_pos = trans.position;
					target_pos += (trans.up * 5f * rad);
					target_pos += (trans.right * 8f * on_right * rad);
					target_pos += (trans.forward * -5f * rad);
					critter.swimToPointData.targetPosition = target_pos;
					critter.swimToPointData.savedTargetDirection = target_pos - critter.critterTransform.position;
				}
			}
		}
		else {
			// we are not drawn. delete.
			critter.markedForRemove = true;
		}
	}

	public void LeaveSceneMinDist( CritterInfo critter, float offCameraMinDist, Vector3 direction ) {
		if( CameraManager.IsInIntroCamMode() || critter == null ) {
			return;
		}

		bool bIsVisible = critter.critterLODData.curLOD.LODrenderer.isVisible;

		if (!bIsVisible &&
			(critter.critterTransform.position-CameraManager.GetCurrentCameraPosition()).magnitude > offCameraMinDist)
		{
			critter.markedForRemove = true;
			return;
		}

		GeneralSpeciesData gsd = critter.generalSpeciesData;
		gsd.isExitingScene = true;

		if( critter.swimToPointData.pointReachedType != PointReachedType.ExitScene ) 
		{
			gsd.swimToPoint = true;
			critter.swimToPointData.pointReachedType = PointReachedType.ExitScene;
			gsd.searchNewLeaderCounter = 0;
			gsd.hungerLevel = 0;
			Vector3 dir;
			if( !MathfExt.Approx(direction, Vector3.zero, 0.1f) ) { 
				dir = direction;
			}
			else {
				dir = CameraManager.GetCurrentCameraForward();
				float swap = dir.x;
				dir.x = dir.z;
				dir.z = swap;
				dir.y = 0f;
				dir.Normalize();
			}
			Vector3 target_pos = critter.critterTransform.position;
			target_pos += dir * GlobalOceanShaderAdjust.CurrentDistance() * 1.1f;
			critter.swimToPointData.targetPosition = target_pos;
			critter.swimToPointData.savedTargetDirection = target_pos - critter.critterTransform.position;           
			critter.swimToPointData.blockCameraSwitchTimer = 4f;
            gsd.switchBehavior = true;
            AI.ForceSwitchToBehavior(critter, SwimBehaviorType.SWIM_TO_POINT);
		}
	}

	public bool IsCritterLeavingScene( int index ) {
		if( index < 0 || index >= singleton.crittersInPopulation ) {
			return true;
		}

		CritterInfo critter = singleton.critters[index];
		if( critter == null
		   || critter.dbVariantID == -1	// already removed
		   || critter.markedForRemove   // marked for remove
		   || ( critter.generalSpeciesData.myCurrentBehaviorType == SwimBehaviorType.SWIM_TO_POINT
			   && critter.swimToPointData.pointReachedType == PointReachedType.ExitScene ) ) {
			return true;
		}
		return false;
	}

/*	void UpdateDebugGUI( CritterInfo critter ) {
		bool draw_me = DebugDisplay.DebugHUDActive();
		
		GUIText gt = critter.generalSpeciesData.debugGUIText.guiText;
		gt.enabled = draw_me;
		
		if( !draw_me ) {
			return;
		}
		critter.generalSpeciesData.debugGUIText.transform.position = CameraManager.GetScreenLocationFromWorldPosition(critter.critterTransform.position) + debugGUIOffset;
		if( critter.generalSpeciesData.debugGUIText.transform.position.z < 0f ) {
			gt.enabled = false;
			return;
		}
		
		float parkingLevel = 0f;
		if(critter.swimParkingData) parkingLevel = critter.swimParkingData.parkingLevel;
		gt.text = critter.generalSpeciesData.myCurrentBehaviorType.ToString() +
																"\n isHungry " + critter.generalSpeciesData.isHungry.ToString() +
																"\n hungerLevel " + critter.generalSpeciesData.hungerLevel +
																"\n airLevel " + critter.generalSpeciesData.airLevel +
																"\n parkingLevel " + parkingLevel + 
																"\n" + critter.animBase.GetAnimationsPlayingString();
		if( critter.generalSpeciesData.isExitingScene ) {
			gt.text += "\n EXITING ON";
		}
		if( critter.generalSpeciesData.myCurrentBehaviorType == SwimBehaviorType.SWIM_TO_POINT ) {
			gt.text += "\n " + critter.swimToPointData.pointReachedType;
		}
		if(gt.text == null
			|| gt.Equals("") ) {
			gt.text = "unknown";
		}
	}*/

    	
	public void UpdateSingular( float dt ) {		
		if( crittersToAdd.Count > 0
		   && !runningCompare
		   && !addingCritters ) 
        {
			CritterToAdd ca = crittersToAdd[0];
			int new_total = ca.amount + crittersInPopulation;
			if( new_total > populationTotalCount ) 
            {
				int num_to_force = new_total - populationTotalCount;
				for( int i = 0; i < num_to_force; i++ ) 
                {
					ForceTheOldestCritterToLeave();
				}
			}
			else if( ( new_total - GetNumCrittersExitingSceneInPopulation() ) <= populationTotalCount )
            {
				AddCrittersToPopulation(ca.critter,ca.amount,ca.variantID,ca.ownedID,ca.designGroupName);
				crittersToAdd.RemoveAt(0);
			}
			else 
            {
				foreach( CritterToAdd ct in crittersToAdd ) 
                {
					if( ct != null && ct.amount >= 1 ) 
                    {
						for( int i = 0; i < ct.amount; i++ ) 
                        {
							// we have critters to spawn and cant.. get rid of one							
							// TODO::we can do better!  try to find a regular non-owned critter first..
							CritterInfo cd = ForceAnOldCritterToLeave(false,true,0.1f,0f);
							if( cd == null ) 
                            {
								ForceAnOldCritterToLeave(false,false,0.1f,0f);
							}
						}
					}
				}
			}
		}

		Vector3 curCamPos = CameraManager.GetCurrentCameraPosition();
//		if( !CameraManager.IsInIntroCamMode() ) 
        {
			if( !paused ) {
				realTimeRunningSimInSphere += dt;
			}
			
			for( int i = 0; i < crittersInPopulation; i++ ) {
				CritterInfo critter = critters[i];

				// dont check against any removed critters.. this should happen.. but who knows.
				if( critter.markedForRemove || critter.dbVariantID == -1 ) {
					continue;
				}

/*#if UNITY_EDITOR
				UpdateDebugGUI( critter );
#endif*/

				// update the fish
				FishController.Update( critter );

				// update the lod
				if( critter.critterLODData ) 
                {
					float use_dist = ( curCamPos - critter.cachedPosition ).sqrMagnitude;
					LODManager.UpdateLOD( critter.critterLODData, use_dist );
				}
			}
		}

	}
	
	void LateUpdate() {
/*		if( paused ) {
			return;
		}*/

		// late update all fish.
		bool can_remove_fish = isVerifyAssetMode || ( !runningCompare && !addingCritters );
		bool critters_removed = false;
		for( int i = 0; i < crittersInPopulation; i++ ) {
			CritterInfo critter = critters[i];

			// if we are markedForRemove, do it. but only if we can
			if( critter.markedForRemove ) 
            {
				if( can_remove_fish ) 
                {
                    if (critter.removedCallback != null)
                    {
                        RemoveCritterReferences(critter);
                        critter.removedCallback(critter);
                        critters[i] = new CritterInfo(); // set this to a new one because critter info is pointed to elsewhere
                        critters[i].dbItemID = -1;
                    }
                    else
                    {
    					Object destroy_me = critter.critterObject;
    					int owned_id = critter.dbOwnedID;
    					int variant_id = critter.dbVariantID;
    					RemoveCritter( critter );
						App.FishManager.ExitedScene( owned_id, variant_id );
    					DestroyImmediate( destroy_me );
                    }

                    critters_removed = true;
					continue;
				}
				else {
					critter.markedForRemove = false;
				}
			}


			FishController.LateUpdate( critter );
		}

		if( critters_removed ) {
			int new_population = crittersInPopulation;
			for( int i = 0; i < new_population; i++ ) {
				CritterInfo critter = critters[i];
				if( critter.dbItemID == -1 && new_population > 0 ) {
					critters[i] = critters[new_population - 1];
					if( critters[i].dbItemID != -1 ) { // don't update us if we are removed.
						if( SimManager.cameraTargetIndex == ( new_population - 1 ) ) {
							SimManager.cameraTargetIndex = i;
						}
						critters[i].critterObject.name += " " + i;
						critters[i].masterIndex = i;
						critters[i].generalSpeciesData.masterIndex = i;
                        if( critters[i].critterItemData != null ) {
                            critters[i].critterItemData.critterInfo = critters[i];
                        }
                        if( critters[i].critterSteering != null ) {
                            critters[i].critterSteering.critterInfo = critters[i];
                        }
                        if( critters[i].generalSpeciesData != null ) {
							critters[i].generalSpeciesData.myCritterInfo = critters[i];
						}
						if( critters[i].awarenessCollision != null ) {
							critters[i].awarenessCollision.myCritterInfo = critters[i];
						}
						
					}
					critters[new_population - 1] = critter;
					new_population--;
					i--; // we might be swapping with something that is also removed.
				}
			}
			crittersInPopulation = new_population;
		}
	}
}
