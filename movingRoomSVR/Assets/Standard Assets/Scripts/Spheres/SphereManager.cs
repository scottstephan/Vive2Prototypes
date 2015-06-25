using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;

// This needs to go...
public enum SphereName {
	None = 0,
    CoralReef = 1,
    SandyBottom = 2,
    OpenWater_Depracated = 3,
    Cliff = 4,
    MyGallery = 5,
    CoralGarden = 6,
    TheDeep = 7,
    OpenWater = 8,
    OuterSeychelles = 9,
    ScrippsKelp = 10,
    GlobalWhale = 15,
    Prochloro = 16,
	Iceberg = 17,
}

public class SphereManager : MonoBehaviour {

	/*******************************************
	* CONSTANTS
	********************************************/
	public const string LAST_SPHERE = "SPHERE_MANAGER_LAST_SPHERE";
	
	public const float TRAVEL_FADE_TIME = 0.7f; //2.25f
	public const int GALLERY_LEGACY_ID = 5;

	/*******************************************
	* VARIABLES
	********************************************/

	// Current, previous, and upcoming sphere
	[HideInInspector]public DataItem loadingSphere = null;
	[HideInInspector]public DataItem currentSphere = null;
	[HideInInspector]public DataItem previousSphere = null;

	[HideInInspector] public string currentSphereName = null;
	
	// State
	[HideInInspector]public bool sphereIsLoading = false;
	[HideInInspector]public bool firstTimeLoadFinished = false;
	[HideInInspector]public bool initialized = false;
	
	[HideInInspector] public int postLoadSpawnVariantID = -1;
    [HideInInspector] public List<int> spawnOwnedIDs = new List<int>();
	[HideInInspector] public bool postLoadSpawnFade = false;
    [HideInInspector] public bool initialLoadComplete = false;
    [HideInInspector] public bool startAsyncLoading = false;
//    [HideInInspector] public bool loadedBaseAudio = false;

	[HideInInspector]public static string destinationName = "sphere";


	[HideInInspector]public static float habitatPlayStart;

	static AssetBundle _loadedBundle;

	// Listeners / Callbacks
	public Action __sphereLoaded = null;
	public Action<int,int> __ownedMomentTriggered = null;
	
	/*******************************************
	* UNITY METHODS
	********************************************/
	public void Awake () {
	
		// Set static singleton reference
		App.SphereManager = this;
	}
	
	public void Start() {
	}
	
	public void OnDestroy () {
	}

	
	/*******************************************
	* SPHERE METHODS
	********************************************/
	public void InitForNewUser() {
        // TODO> make asset bundle handling its own thing seperate from these tables.
        // clear out all owned spheres..
        // cannot do a clear(), otherwise we lose the reference to the asset bundle.
        if( App.DataManager.spheres.Length > 0 ) {
            foreach(DataItem sphere_data in App.DataManager.spheres ) {
                if( sphere_data != null ) {
                    sphere_data.CurrentMaxPop = 10;//sphere_data.SpecMinPop;
                    UpdatePops(sphere_data);
                }
            }
        }
	}
	
    void FinishInit( string response ) {
        App.DataManager.spheres = JsonReader.Deserialize<DataItem[]>( response );
        
        if ( App.DataManager.spheres != null ) {
            
            // PROCESS SPHERES DATA!!!
            App.DataManager.spheres = ProcessSphereData( App.DataManager.spheres );
            
            // Cache the data
            PlayerPrefs.SetString( DataManager.SPHERE_DATA, response );
            initialized = true;
        }
    }

	public void GetSphereData( bool force=false ) {
		
		// Check to see if data was cached in player prefs
		string cachedData = App.DataManager.GetCachedData( DataManager.SPHERE_DATA );

		if ( cachedData != null ) {
			App.DataManager.spheres = JsonReader.Deserialize<DataItem[]>( cachedData );
			if ( App.DataManager.spheres != null ) {

				// Got our data from cache; bail
				initialized = true;
                if( !force ) {
    				return;
                }
			}
		}
		
        if( !initialized ) {
            // Preload default settings and urls.
            TextAsset txt = (TextAsset)Resources.Load("JSONData/allSpheresResponse", typeof(TextAsset));
            string _info = txt.text;
            if( _info != null ) {
                FinishInit( _info );
            }

        }
/*
		// Get data from api call
		App.ApiManager.GET ( App.Urls.allSpheres, delegate( string response, string error ) {
//            Debug.Log ("sphere response " + response);
            if ( error != null || response == null ) {
				App.ApiManager.ThrowError( App.Urls.allSpheres, error );
				return;
			}
			
            FinishInit( response );
		});
*/        
	}	
	
	public DataItem[] ProcessSphereData( DataItem[] spheres ) {		
		int i;
		for ( i=0; i<spheres.Length; i++ ) {	
			spheres[ i ] = App.SphereManager.Process( spheres[ i ] );			
		}
		return spheres;
	}
	
	public void DiveIntoStartingSphere() {		
//        LoadSphere( "Borut-VR-beats" );
        //LoadSphere( "open-water-multi" );


		//LoadSphere ("coral-garden-lite_ralph", null, FinishStartingSphereLoad);
	
	}

	public void FinishStartingSphereLoad() {
	}
	
 /*  IEnumerator DelayShowLoad()
    {
        yield return new WaitForSeconds(TRAVEL_FADE_TIME - 0.1f);
    }*/

	public void TravelToSphere( string sphereId, DataItem sphere=null ) {

		AudioManager.Instance.FadeOutAllMusic();
		AudioManager.Instance.PlayIntroAudio( false );
		
//        StartCoroutine(DelayShowLoad());

		OculusCameraFadeManager.StartCameraFadeToBlack( 
//		OculusCameraFadeManager.StartCameraFadeToBlue(
            TRAVEL_FADE_TIME, 
			delegate( object arg ) {

			CameraManager.IntroCameraExit();
            			
				CameraManager.SwitchToCamera(CameraType.TravelCamera);
			
				SimInstance.Instance.ForceRemoveCritters();
				
				FloatingMenuManager.ShowLoading();

				//HACKOLA! RAB -- to hide the intro menu scene from the first load
				GameObject introFloor = (GameObject)GameObject.Find("IntroFloor");

				if (introFloor)
					introFloor.SetActive(false);
				
				LoadSphere( sphereId, sphere, delegate() {
					currentSphereName = sphereId;
				});
			}, 
			true 
		);
	}
	
	public void LoadSphere( string sphereId, DataItem sphere=null, Action callback=null ) {

		// Either get sphere by ID, or by an already-passed sphere data object
		sphere = sphereId != null ? App.Query.GetItemById( DataQuery.ItemTypes.sphere, sphereId ) : sphere;

		int version = 1001;
/*		if ( sphere != null ) {
			version = sphere.version;
		}*/

		// Set state
		sphereIsLoading = true;
		loadingSphere = sphere;
		
		__sphereLoaded = callback;
		
		// bail if reloading the same sphere.
/*		if(  currentSphere != null && sphere != null && currentSphere._id == sphere._id ) {
			SphereInstance.sphereLoadedFinished += SphereInstanceFinishedLoading;
			SphereInstance.Instance.Loaded( null, null );
			return;
		}*/

		string bundleItemUrl = App.ApiManager.GetItemBundleUrl( sphere );

		//what we need is a way to fall back to the URL if this bundle call fails
		//string localURL = App.ApiManager.GetLocalURL(sphere);
		string localURL = App.ApiManager.GetLocalURLFromString(sphereId);

		if (localURL != null)
			bundleItemUrl = localURL;
		else if( bundleItemUrl == null ) {
			bundleItemUrl = "/client/unity4_5_b7/1/" + App.ApiManager.GetPlatformUrlString() + "/items/" + sphereId + ".unity3d";
		}

		// API call to get sphere bundle
		App.ApiManager.GetBundle( 
			bundleItemUrl, 
			version, 
			delegate( AssetBundle sphereBundle, string error ) {
				if ( error != null ) {
					App.requiredNetworkFailure = true;
					return;
				}
			
				// Set up sphere loading callbacks
				SphereInstance.sphereLoadedFinished += SphereInstanceFinishedLoading;
				_loadedBundle = sphereBundle;
				//SphereInstance.Instance.Loaded( sphereBundle.mainAsset );
				sphereIsLoading = false;
			}
		);

        App.MetricsManager.Stage("habitat_load_name", sphereId, true);
		App.MetricsManager.TrackStaged("habitat_load_start");
	}

	public static void InstantiateLoadedAssets(Action postInstantiate) {

		SphereInstance.Instance.Loaded(_loadedBundle.mainAsset, postInstantiate);

		_loadedBundle.Unload(false);
		_loadedBundle = null;
	}

	public void SphereInstanceFinishedLoading() {		
	
        // Only run this when we are loading into our first sphere.
        if( CameraManager.IsInIntroCamMode() ) {

            CameraManager.TriggerIntroCameraOkToEnd();
        }
		
		// Update states
		firstTimeLoadFinished = true;
		sphereIsLoading = false;
		previousSphere = currentSphere;		
		currentSphere = loadingSphere;
		loadingSphere = null;
				
		// Kill sphere load callbacks
		SphereInstance.sphereLoadedFinished -= SphereInstanceFinishedLoading;
		
		if ( __sphereLoaded != null ) {
			__sphereLoaded();
			
			__sphereLoaded = null;
		}

		FloatingMenuManager.SetMenuMode(FloatingMenuManager.MenuType.Travel);
		FloatingMenuManager.MakeMenuAvailable();
	}
	
	public void UnloadSphere( string ID ) {
		foreach( DataItem sphere in App.DataManager.spheres ) {
			if ( sphere._id == ID ) {
				sphere.MainAsset = null;
				if ( sphere.MainAssetBundle != null ) {
					sphere.MainAssetBundle.Unload( true );
					sphere.MainAssetBundle = null;
				}
				break;
			}
		}
	}
	
	/*******************************************
	* STATE MANIPULATION / SPHERE QUERY METHODS
	********************************************/

	// Calculate populations, etc
	public DataItem Process( DataItem sphere ) {
		sphere.NonNativePercentage = (float)sphere.roamerpercentage / 100f;
		sphere.OwnedNativePercentage = (float)sphere.ownednativepercent / 100f;
		sphere.OwnedNonNativePercentage = (float)sphere.ownednonnativepercent / 100f;
		
		sphere.SpecMinPop = GetMinPop( sphere );
        sphere.SpecMaxPop = GetMaxPop( sphere );
		
        sphere.CurrentMaxPop = 10;//( sphere.legacyid == GALLERY_LEGACY_ID ) ? sphere.SpecMaxPop : sphere.SpecMinPop;
		
		UpdatePops( sphere );
		
		return sphere;
	}
	
	public void SetNoVisitors( string ID, int legacyID=-1 ) {
		int i;
		for( i=0; i<App.DataManager.spheres.Length; i++ ) {
			if ( ( ID != null && App.DataManager.spheres[ i ]._id == ID ) || ( App.DataManager.spheres[ i ].legacyid == legacyID ) ) {
				App.DataManager.spheres[ i ].NoVisitors = true;
				break;
			}				
		}
	}
    public void ResetNoVisitors() {
		int i;
		for( i=0; i<App.DataManager.spheres.Length; i++ ) {
			App.DataManager.spheres[ i ].NoVisitors = false;
		}
    }


    public int GetMinPop( DataItem sphere_data ) {
        if( SystemSpec.sysLevel == SystemLevel.Low ) {
            return sphere_data.lowspecminpop;
        }
        if( SystemSpec.sysLevel == SystemLevel.Medium ) {
            return sphere_data.medspecminpop;
        }

		return sphere_data.highspecminpop;
    }

    public int GetMaxPop( DataItem sphere_data ) {
        if( SystemSpec.sysLevel == SystemLevel.Low ) {
            return sphere_data.lowspecmaxpop;
        }
        if( SystemSpec.sysLevel == SystemLevel.Medium ) {
            return sphere_data.medspecmaxpop;
        }
        return sphere_data.highspecmaxpop;
    }

    public void IncMaxPop( DataItem sphere_data ) {
        sphere_data.CurrentMaxPop++;
        UpdatePops( sphere_data );
    }

    public void DecMaxPop( DataItem sphere_data ) {
        sphere_data.CurrentMaxPop--;
        if( sphere_data.CurrentMaxPop < 1 ) {
            sphere_data.CurrentMaxPop = 1;
        }
        UpdatePops( sphere_data );
    }
	
    public void UpdatePops( DataItem sphere_data ) {	
        sphere_data.CurrentMaxNonNativePop = (int)(sphere_data.NonNativePercentage * ((float)sphere_data.CurrentMaxPop));
        sphere_data.CurrentMaxNativePop = sphere_data.CurrentMaxPop - sphere_data.CurrentMaxNonNativePop;
        
        sphere_data.CurrentMaxNativeVisitorsPop = (int)(sphere_data.OwnedNativePercentage * ((float)sphere_data.CurrentMaxNativePop));
        sphere_data.CurrentMaxNonNativeVisitorsPop = (int)(sphere_data.OwnedNonNativePercentage * ((float)sphere_data.CurrentMaxNonNativePop));
        
        // If our population has changed we need to redistribute our native variant weights.
		LEGACY_UpdateNativeVariantWeightCnts( sphere_data );
    }
		
    public int LEGACY_GetCurrentSphere() {
        if( AppBase.Instance.RunningAsPreview() ) {
			PreviewApp pa = AppBase.Instance as PreviewApp;
            return pa.previewSphereID;
        }
		if( currentSphere != null ) {
	        return currentSphere.legacyid;
		}
		return 0;
    }	

	public int LEGACY_GetLoadingSphere() {
		return loadingSphere == null ? 0 : loadingSphere.legacyid;
	}

	public int LEGACY_GetPreviousSphere() {
		return previousSphere == null ? 0 : previousSphere.legacyid;
	}

	public bool LEGACY_IsLoadingSphere() {
		return ( sphereIsLoading );//loadingSphere != null && loadingSphere._id != currentSphere._id );
	}

	public void LEGACY_PostLoadCritterFollow( CritterInfo critter, bool can_remove ) {		
		CameraManager.SwitchToTarget( critter.critterObject, CameraType.FollowBehindCamera );
		if( postLoadSpawnFade ) {
            OculusCameraFadeManager.StartCameraFadeFromBlack( TRAVEL_FADE_TIME, null, null);
		}
		postLoadSpawnVariantID = -1;
        if( can_remove ) {
    		spawnOwnedIDs.Remove(critter.dbOwnedID);
        }
		postLoadSpawnFade = false;
	}

    public int LEGACY_GetRandomForSaleSphereIDForVisitors() {
        int visitors_count = 0;
        int for_sale_count = 0;
        foreach( DataItem sphere_data in App.DataManager.spheres ) {
            if( sphere_data.dateForsale != null
                && sphere_data.legacyid != GALLERY_LEGACY_ID
                && sphere_data.legacyid != 7  // dont try to find a visitor from the deep.
                && sphere_data.legacyid != 16 ) { // or from the micro sphere.
                for_sale_count++;
                if( !sphere_data.NoVisitors ) {
                    visitors_count++;
                }
            }
        }
        if( for_sale_count > 0 && visitors_count <= 0 ) {
            ResetNoVisitors();
            return LEGACY_GetRandomForSaleSphereID();
        }
        
        int rnd = UnityEngine.Random.Range(0,visitors_count);
        visitors_count = 0;
        foreach( DataItem sphere_data in App.DataManager.spheres ) {
            if( sphere_data.dateForsale != null && !sphere_data.NoVisitors
                && sphere_data.legacyid != GALLERY_LEGACY_ID
                && sphere_data.legacyid != 7  // dont try to find a visitor from the deep.
                && sphere_data.legacyid != 16 ) { // or from the micro sphere.
                if( rnd == visitors_count ) {
                    return sphere_data.legacyid;
                }
                visitors_count++;
            }
        }
        
        return LEGACY_GetCurrentSphere();
    }
    
	public int LEGACY_GetRandomForSaleSphereID() {
        int count = 0;
        foreach( DataItem sphere_data in App.DataManager.spheres ) {
            if( sphere_data.dateForsale != null 
				&& sphere_data.legacyid != 16 ) { // no micro!
                count++;
            }
        }
        int rnd = UnityEngine.Random.Range(0,count);
        count = 0;
        foreach( DataItem sphere_data in App.DataManager.spheres ) {
            if( sphere_data.dateForsale != null 
				&& sphere_data.legacyid != 16 ) { // no micro!
                if( rnd == count ) {
                    return sphere_data.legacyid;
                }
                count++;
            }
        }
        
        return App.SphereManager.LEGACY_GetCurrentSphere();
    }	
 
    public void LEGACY_UpdateNativeVariantWeightCnts( DataItem sphere_data ) {
        bool init_percent = false;
        if( sphere_data.TotalVariantWeight <= 0 ) {
            LEGACY_UpdateTotalVariantWeight(sphere_data);
            init_percent = true;
        }
        
        //       DebugDisplay.AddDebugText("UpdateSphereNativeVariantWeightCnts");
        foreach( DataItem item_data in App.DataManager.fishvariants ) {
            if( item_data.parentsphereid == sphere_data._id ) {
                bool can_be_added = true;
                if( item_data.mustpurchasetospawn && !CameraManager.singleton.isOculusMode ) {
                    can_be_added = false;
/*					if( sphere_data.trueblu && App.OwnedFishManager != null ) {
                        int owned = App.OwnedFishManager.GetLegacyOwnedIdFromFishId( item_data.legacyid );
                        if( owned > 0 ) {
                            can_be_added = true;
                        }
                    }*/
                }
                if( can_be_added ) {
                    int old_cnt = item_data.WeightSatisfiedCount;
                    if( init_percent || item_data.Percentage <= 0f ) {
                        item_data.Percentage = ( (float)item_data.weight / (float)sphere_data.TotalVariantWeight );
                    }
                    item_data.WeightSatisfiedCount = Mathf.FloorToInt(item_data.Percentage * ((float)sphere_data.CurrentMaxNativePop));
                    item_data.WeightSatisfiedCount += 1;
                    
                    //               DebugDisplay.AddDebugText("Updating Item Weight Cnt :: " + item_data.percentage + " " + sphere_data.currentMaxNativePop + " " + item_data.weight + " " + sphere_data.totalVariantWeight);
                    if( old_cnt < item_data.WeightSatisfiedCount && item_data.WeightWasSatisfied ) {
                        item_data.WeightWasSatisfied = false;
                    }
                }
            }
        }
    }

 
    public void LEGACY_UpdateTotalVariantWeight( DataItem sphere_data )
    {
/*        if (App.OwnedFishManager == null)
        {
            return;
        }

        int count = 0;
        int total_weight = 0;        
        foreach( DataItem item_data in App.DataManager.fishvariants ) {
            if( item_data.parentsphereid == sphere_data._id 
                && item_data.subtype == FishManager.FISH_TYPE_CRITTER ) {
                bool can_be_added = true;
                if( item_data.mustpurchasetospawn) {
                    can_be_added = false;
                    if( sphere_data.trueblu ) {
                        int owned = App.OwnedFishManager.GetLegacyOwnedIdFromFishId( item_data.legacyid );
                        if( owned > 0 ) {
                            can_be_added = true;
                        }
                    }
                }
                if( can_be_added ) {
                    count++;
                    total_weight += (int)item_data.weight;
                }
            }
        }
        
        sphere_data.TotalVariantWeight = total_weight;*/
    }
	
    public int LEGACY_GetIDForName( string name ) {
        foreach( DataItem sphere_data in App.DataManager.spheres ) {
            if ( App.ApiManager.GetItemBundleUrl( sphere_data ).Contains( name ) ) {
                return sphere_data.legacyid;
            }
        }
        return -1;    
    }

	public DataItem GetDataForName( string name ) {
        foreach( DataItem sphere_data in App.DataManager.spheres ) {
            if ( App.ApiManager.GetItemBundleUrl( sphere_data ).Contains( name ) ) {
                return sphere_data;
            }
        }
        return null;    
    }

	public static bool IsInBoot() {

		//there has to be a better way to do this
		if (destinationName == "sphere")
			return true;

		return false;
	}
	
}