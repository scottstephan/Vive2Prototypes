using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;

using System.IO;

public enum SchoolingType {
	PARENT = 0,
	ITEM,
	VARIANT,
}

public class FishManager : MonoBehaviour {

	/*******************************************
	* CONSTS
	********************************************/
	public const string FISH_TYPE_SEABED = "seabed";
	public const string FISH_TYPE_CRITTER = "critter";
	
	/*******************************************
	* VARIABLES
	********************************************/
	public event System.Action<int,int> __clientCritterArrived = null;
	public event System.Action<int,int> __clientCritterLeft = null;
	
	// State
	public bool fishVariantsReady = false;
	public bool fishParentsReady = false;
	public bool fishVariantsProcessed = false;
	public bool initialized = false;

	/*******************************************
	* UNITY METHODS
	********************************************/
	public void Awake () {
	
		// Set static singleton reference
		App.FishManager = this;
	}
	
	public void Start() {
	}
	
	public void OnDestroy () {
	}
	
	/*******************************************
	* DATA RETRIEVAL & INIT METHODS
	********************************************/
	public void CheckInitialization() {
		// The order of operations here is as follows...
		// 1. Get fish parent data
		// 2. Get fish variant data
		// 3. Process fish variant data into useable structure for client
		// 4. state bool "initialized" = true
		
		if ( fishParentsReady && fishVariantsReady && fishVariantsProcessed ) {
			initialized = true;
			return;
		}
		
		if ( fishParentsReady && fishVariantsReady )
			ProcessFishData();
	}
	
    void FinishVariantInit( string response ) {
//        Debug.Log("fish variant data response  : "  + response );
        
        App.DataManager.fishvariants = JsonReader.Deserialize<DataItem[]>( response );          
        
        if ( App.DataManager.fishvariants != null ) {
            // Cache the data
            PlayerPrefs.SetString( DataManager.FISHVARIANT_DATA, response );
            fishVariantsReady = true;
            CheckInitialization();
        }
    }

	public void GetFishVariantData( bool force=false ) {
		// Check to see if data was cached in player prefs
		string cachedData = App.DataManager.GetCachedData( DataManager.FISHVARIANT_DATA );

//        Debug.Log("get fish data force " + force );
        if ( cachedData != null && !force ) {
			App.DataManager.fishvariants = JsonReader.Deserialize<DataItem[]>( cachedData );
			if ( App.DataManager.fishvariants != null ) {
//                Debug.Log("fish used cached data.");
                // Got our data from cache; bail
				fishVariantsReady = true;
				CheckInitialization();
				return;
			}
		}
		
        if( !fishVariantsReady ) {
            // Preload default settings and urls.
            TextAsset txt = (TextAsset)Resources.Load("JSONData/allFishVariantResponse", typeof(TextAsset));
            string _info = txt.text;
            if( _info != null ) {
//                Debug.Log ("Default all fish variants response loaded.");
                FinishVariantInit( _info );
            }            
        }

/*
        // Get data from api call
		App.ApiManager.GET ( App.Urls.allFishVariants, delegate( string response, string error ) {
			if ( error != null || response == null ) {
				App.ApiManager.ThrowError( App.Urls.allFishVariants, error );
				return;
			}
            FinishVariantInit( response );
		});
        */
	}

    void FinishParentInit( string response ) {
        App.DataManager.fishparents = JsonReader.Deserialize<DataItem[]>( response );
        
        if ( App.DataManager.fishparents != null ) {            
            // Cache the data
            PlayerPrefs.SetString( DataManager.FISHPARENT_DATA, response );
            fishParentsReady = true;
            CheckInitialization();
        }
    }

	public void GetFishParentData( bool force=false ) {
		
		// Check to see if data was cached in player prefs
		string cachedData = App.DataManager.GetCachedData( DataManager.FISHPARENT_DATA );

//        Debug.Log("get fish parent data force " + force );
        if ( cachedData != null ) {
			App.DataManager.fishparents = JsonReader.Deserialize<DataItem[]>( cachedData );
			if ( App.DataManager.fishparents != null ) {

//                Debug.Log("fish parent used cached data.");
                // Got our data from cache; bail
				fishParentsReady = true;
				CheckInitialization();
                if( !force ) {
    				return;
                }
			}
		}

        if( !fishParentsReady ) {
            // Preload default settings and urls.
            TextAsset txt = (TextAsset)Resources.Load("JSONData/allFishParentResponse", typeof(TextAsset));
            string _info = txt.text;
            if( _info != null ) {
                Debug.Log ("Default all fish parent response loaded.");
                FinishParentInit( _info );
            }            
        }

/*
		// Get data from api call
		App.ApiManager.GET ( App.Urls.allFishParents, delegate( string response, string error ) {
			if ( error != null || response == null ) {
				App.ApiManager.ThrowError( App.Urls.allFishParents, error );
				return;
			}
			
            FinishParentInit( response );
		});
  */      
	}
	
    public static DataItem GetFishParent(DataItem fish)
    {
        return App.Query.GetItemById( DataQuery.ItemTypes.fishparent, fish.parentid );
    }

	public void ProcessFishData() {
		
		if ( App.DataManager.fishvariants == null || App.DataManager.fishvariants.Length <= 0 )
			return;

		int i;
		for ( i=0; i<App.DataManager.fishvariants.Length; i++ ) {
			DataItem fish = App.DataManager.fishvariants[ i ];
			
			DataItem sphere = App.Query.GetSphereById( fish.parentsphereid );
			
			if ( sphere != null && sphere.TotalVariantWeight > 0 ) 
				fish.Percentage = ( (float)fish.weight / (float)sphere.TotalVariantWeight );
			else
				fish.Percentage = 0f;
			
			fish.WeightSatisfiedCount = 0;
			fish.CurrentWeightCount = 0;
			fish.WeightWasSatisfied = false;
			fish.ForSale = fish.dateForsale != null;
			App.DataManager.fishvariants[ i ] = fish;
		}
		
		/*StreamWriter sw = new StreamWriter(new FileStream("FishVariants.csv", FileMode.OpenOrCreate, FileAccess.Write));
		for ( i=0; i<App.DataManager.fishvariants.Length; i++ )
		{
			Debug.Log("						debug Adam: [" + App.DataManager.fishvariants[i].legacyid + "] " + 
				App.DataManager.fishvariants[i].name + ": " + 
				App.DataManager.fishvariants[i].description);
			sw.WriteLine(App.DataManager.fishvariants[i].legacyid + ",\"" +
				App.DataManager.fishvariants[i].name + "\",\"" +
				App.DataManager.fishvariants[i].description + "\"");
		}
		sw.Close();*/
		
		fishVariantsProcessed = true;
		CheckInitialization();
	}

	/*******************************************
	* BUNDLE LOADING METHODS
	********************************************/
	public void LoadFishBundle( DataItem item ) {
		
		// API call to get sphere bundle
		App.ApiManager.GetBundle( 
			App.ApiManager.GetItemBundleUrl( item ), 
			item.version, 
			delegate( AssetBundle assetBundle, string error ) {
				if ( error != null || assetBundle == null ) {
					Debug.LogError( "ASSET BUNDLE REQUEST FAILED FOR ITEM: " + item.name + " :: ID: " + item.legacyid );
					return;
				}
			
				// Got the asset bundle; create ref to mainAsset
				Object mainAsset = assetBundle.mainAsset;
			
				// Convert to GO
				GameObject assetGameObject = mainAsset as GameObject;
			
				int i;
				for ( i=0; i<App.DataManager.fishvariants.Length; i++ ) {
					if ( App.DataManager.fishvariants[ i ] == null ) {
						Debug.LogError ( "FISH ID IS NULL :WTF: " + App.DataManager.fishvariants[ i ].legacyid );						
					}
				
					if ( App.DataManager.fishvariants[ i ] != null && App.DataManager.fishvariants[ i ]._id == item._id ) {
						//Debug.Log( "CREATING REFERENCE TO FISH ID: " + App.DataManager.fishvariants[ i ].legacyid );
						App.DataManager.fishvariants[ i ].MainAsset = assetGameObject;
						App.DataManager.fishvariants[ i ].MainAssetBundle = assetBundle;
						break;
					}
				}
			}
		);	
	}	
	
	public Object GetOrLoadObject( int variant_id ) {
		DataItem item = App.Query.GetItemByLegacyId( DataQuery.ItemTypes.fishvariant, variant_id );

		Object item_object = item.MainAsset;

		if( item_object == null ) {
			if( !VariantHasParentData( variant_id,true,true ) )
				return null;

			// Return if we have already requested our object and has not been recieved yet
			DataItem item_data = App.Query.GetItemByLegacyId( DataQuery.ItemTypes.fishvariant, variant_id );
			if ( item_data.ObjectRequested )
				return null;
			
			item_data.ObjectRequested = true;

			LoadFishBundle( item );
		}

		return item_object;
	}	

	/*******************************************
	* EVENT-BASED / STATE MANIPULATION METHODS
	********************************************/
	public void EnteredScene( CritterInfo critter ) {
/*		int owned_id = critter.dbOwnedID;
		if( owned_id != -1 ) {
			DataUserItem owned_item = App.OwnedFishManager.GetOwnedItemByLegacyId( owned_id );
			if( owned_item == null ) {
				SimInstance.Instance.LeaveScene(critter,false,-1);
			}
			else {
				owned_item.InScene = true;
                
                if( owned_item.legacyuserid == App.UserManager.me.userid 
                    && ( SimInstance.Instance.realTimeRunningSimInSphere > 10f ) 
                    && !CameraManager.IsInTravelCamera() 
                    && __clientCritterArrived != null ) {
                    __clientCritterArrived(owned_item.legacyitemid, owned_item.legacyid);
                }
			}
		}*/
	}

	public void ExitedScene( int owned_id, int variant_id ) {
/*		if( owned_id != -1 ) {
			DataUserItem owned_item = App.OwnedFishManager.GetOwnedItemByLegacyId( owned_id );
			if( owned_item != null && owned_item.legacyuserid == App.UserManager.me.userid ) {	// we may have switched accounts, and this owned item no longer exists
				owned_item.InScene = false;
                owned_item.Shared = true;
                owned_item.LastRoamedDateTime = StartupObject.ServerTime;
				owned_item.LastInSphereTime = Time.realtimeSinceStartup;
                if( ( SimInstance.Instance.realTimeRunningSimInSphere > 10f ) 
                    && !CameraManager.IsInTravelCamera() 
                    && __clientCritterLeft != null ) {
                    __clientCritterLeft( owned_item.legacyitemid, owned_item.legacyid );
                }
			}
		}*/
	}

	// used to be IsItemLocalForVariant
	public bool VariantHasParentData( int variant_id, bool load_object_too, bool from_get_or_load=false ) {
		foreach( DataItem item in App.DataManager.fishvariants ) {
			if( item != null && item.legacyid == variant_id ) {
                DataItem parent = GetFishParent(item);
                if( parent != null && parent.name != null ) {
					if( item.MainAsset == null && load_object_too && !from_get_or_load ) {
						GetOrLoadObject( variant_id );
					}
					return true;
				}
			}
		}
		return false;
	}

	public void SetItemSize( string ID, int legacyVariantID, SpeciesSize ssize ) {
		foreach( DataItem item in App.DataManager.fishvariants ) {
			if ( item == null )
				continue;
			
			if ( ( ID != null && item._id == ID ) || legacyVariantID == item.legacyid ) {
				item.SpeciesSize = ssize;
				return;
			}
		}
	}

	public SpeciesSize GetSpeciesSize( string ID, int legacyVariantID ) {
		foreach( DataItem item in App.DataManager.fishvariants ) {
			if ( item == null )
				continue;
			
			if ( ( ID != null && item._id == ID ) || legacyVariantID == item.legacyid )
				return item.SpeciesSize;
		}
		
		return SpeciesSize.UNKNOWN;
	}	
	
	public void ClearWeightsSatisfiedForSphere( string sphere_id ) {
		foreach( DataItem item_data in App.DataManager.fishvariants ) {
			if( item_data.parentsphereid == sphere_id ) {
				item_data.WeightWasSatisfied = false;
				item_data.CurrentWeightCount = 0;
			}
		}
	}

	// only called when the dynamic manager cannot find any critters.
	public void UpdateWeightsSatisfiedForSphere( string sphere_id ) {
		bool all_satisfied = true;
//        DataItem sphere_data = App.Query.GetSphereById( sphere_id );
		foreach( DataItem item_data in App.DataManager.fishvariants ) {
            bool can_be_checked = true;
/*            if( item_data.mustpurchasetospawn ) {
                can_be_checked = false;
                if( sphere_data.trueblu ) {
                    int owned = App.OwnedFishManager.GetLegacyOwnedIdFromFishId( item_data.legacyid );
                    if( owned > 0 ) {
                        can_be_checked = true;
                    }
                }
            }*/
            if( can_be_checked 
				&& item_data.subtype == FISH_TYPE_CRITTER
                && item_data.parentsphereid == sphere_id 
                && !item_data.WeightWasSatisfied ) {
				all_satisfied = false;
			}
		}

		if( all_satisfied ) {
			ClearWeightsSatisfiedForSphere( sphere_id );
		}
	}
	
	/*******************************************
	* QUERY METHODS
	********************************************/
	public int GetRandomNonNativeFishId() {
		int count = 0;
		string cur_sphere_id = App.SphereManager.currentSphere._id;
//		Debug.Log("Looking through " + App.DataManager.fishvariants + " : " + cur_sphere_id );
		foreach( DataItem item in App.DataManager.fishvariants ) {
			if( item == null ) {
//				Debug.Log("item_null");
				continue;
			}
			if( item.subtype != FISH_TYPE_CRITTER ) {
//				Debug.Log("item not a fish critter");
				continue;
			}
//			Debug.Log(item.dateForsale + " : " + item.ForSale );
			if( !item.ForSale ) {
//				Debug.Log("item_not for sale");
				continue;
			}
/*			if( item.mustpurchasetospawn && App.OwnedFishManager.GetLegacyOwnedIdFromFishId(item.legacyid) <= 0 ) {
//				Debug.Log("item must be purchased first");
				continue;
			}*/
			if( item.parentsphereid == cur_sphere_id ) {
//				Debug.Log("item is native");
				continue;
			}
			if( item.legacysphereid == (int)SphereName.MyGallery ) {
//				Debug.Log("item is from the gallery");
				continue;
			} 
			if( item.legacysphereid == (int)SphereName.Prochloro ) {
//				Debug.Log("item is from the Prochloro");
				continue;
			} 
			if( item.legacysphereid == (int)SphereName.TheDeep ) {
//				Debug.Log("item is from the TheDeep");
				continue;
			} 
			if( item.legacysphereid == (int)SphereName.GlobalWhale ) {
//				Debug.Log("item is from the GlobalWhale");
				continue;
			} 
			count++;
//			Debug.Log("FOUND A NON NATIVE FISH!");
		}

		if( count > 0 ) {
			int rnd = Random.Range(0,count);
			int cnt = 0;
			foreach( DataItem item in App.DataManager.fishvariants ) {
				if( item != null
				   && item.subtype == FISH_TYPE_CRITTER
                   && item.ForSale
//                   && ( !item.mustpurchasetospawn || App.OwnedFishManager.GetLegacyOwnedIdFromFishId(item.legacyid) > 0 )
				   && item.parentsphereid != cur_sphere_id
                   && item.legacysphereid != (int)SphereName.MyGallery 
				   && item.legacysphereid != (int)SphereName.Prochloro 
                   && item.legacysphereid != (int)SphereName.TheDeep 
                   && item.legacysphereid != (int)SphereName.GlobalWhale ) {
					if( rnd == cnt )
						return item.legacyid; // this is the itemvariantid
					else
						cnt++;
				}
			}
		}

		return -1;
	}

	public DataItem GetParentByFishId( string ID, int legacyVariantID ) {
		foreach( DataItem item in App.DataManager.fishvariants ) {
			if ( item == null )
				continue;
			
			if ( ID != null && item._id == ID )
				return item;
			
			if ( legacyVariantID == item.legacyid )
				return item;
		}
		return null;
	}


	public int GetLegacyItemIdByFishId( int legacyVariantID ) {
		DataItem parentItem = GetParentByFishId( null, legacyVariantID );

		if ( parentItem == null )
			return -1;
		
		return parentItem.legacyitemid;
	}
	
	public int GetLegacySphereIdByFishId( int legacyVariantID ) {
		DataItem item = App.Query.GetItemByLegacyId( DataQuery.ItemTypes.fishvariant, legacyVariantID );
		return item == null ? -1 : item.legacysphereid;
	}
	
	public string GetFishNameById( string ID, int legacyID ) {
		DataItem item = ID == null ? App.Query.GetFishVariantByLegacyId( legacyID ) : App.Query.GetFishVariantById( ID );
		if ( item == null )
			return null;
		
		if ( item.name == null )
        {
            DataItem parent = GetFishParent(item);
            return parent != null && parent.name != null ? parent.name : null;
        }
		
		return item.name;
	}
	
	public string GetFishImageUrlById( string ID, int legacyID ) {
		DataItem item = ID == null ? App.Query.GetFishVariantByLegacyId( legacyID ) : App.Query.GetFishVariantById( ID );
		if ( item == null )
			return null;
		
		return App.ApiManager.GetItemImageUrl( item );
	}
	
	public bool IsLimitedEdition( string ID, int legacyID ) {
		DataItem item = ID == null ? App.Query.GetFishVariantByLegacyId( legacyID ) : App.Query.GetFishVariantById( ID );
		if ( item == null )
			return false;
		
		return item.limitededition;
	}

	public int GetRandomLEForCurrentSphere( bool owned ) {
		
		
		
		List<int> potential_variants = new List<int>();
		potential_variants.Clear();
		int cur_sphere_id = App.SphereManager.currentSphere.legacyid;
        DataItem sphere_data = App.Query.GetSphereByLegacyId( cur_sphere_id );
		foreach( DataItem item in App.DataManager.fishvariants ) {
			if( item.limitededition
				// using the gallery sphere to designate anywhere.
		   		&& (item.legacysphereid == cur_sphere_id || (item.legacysphereid == 5 && !sphere_data.trueblu && sphere_data.legacyid != (int)SphereName.TheDeep)) 
				&& App.UserManager.query.OwnsItem( item._id ) == owned ) {
				potential_variants.Add(item.legacyid);
			}
		}
		if( potential_variants.Count > 0 ) {
			return potential_variants[Random.Range(0,potential_variants.Count)];
		}
		return -1;
	}
}
