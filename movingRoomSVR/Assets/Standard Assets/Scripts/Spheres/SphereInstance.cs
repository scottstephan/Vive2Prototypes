using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void SphereLoadedFinishedDelegate();

[System.Serializable]
public class StaticObjectLODInfo {
	public GameObject obj;
	public Transform objTransform;
	public LODModelData objLodData;
}

public class SphereInstance : MonoBehaviour {
	// TODO> this singleton and Instance needs to go away.
	private static SphereInstance singleton = null;
    public static SphereInstance Instance {
        get {
            if (singleton != null ) {
                return singleton;
            }

            if (singleton == null) {
                // This is where the magic happens.
                //  FindObjectOfType(...) returns the first AManager object in the scene.
                singleton =  FindObjectOfType(typeof (SphereInstance)) as SphereInstance;
				if (singleton != null)
				{
//                	Debug.Log ("SphereInstance object found and assigned.");
				}
            }

            // If it is still null, create a new instance
            if (singleton == null) {
                GameObject obj = new GameObject("SphereInstance");
                singleton = obj.AddComponent(typeof(SphereInstance)) as SphereInstance;
//                Debug.Log("Could not locate an SphereInstance object. \n SphereInstance was Generated Automaticly.");
            }
            
            return singleton;
        }
    }
 
    public static SphereLoadedFinishedDelegate sphereLoadedFinished = null;
	
	public const string levelLightMapDataObjName = "LevelLightMapData";
	public const string levelStreamingDataObjName = "LevelStreamingData";

	private List<WemoObject> wemoObjects;
	public List<StaticObjectLODInfo> staticLODs;
	public List<FishBowl> fishBowls;

    public bool setupPostLoad;
    
    private GameObject[] oceanSurfaceGameObjects;

    void Awake() {
        if (singleton == null) {
            singleton = this;
        }
        oceanSurfaceGameObjects = null;
    }
    
	public void Construct() {
		staticLODs = new List<StaticObjectLODInfo>();
		wemoObjects = new List<WemoObject>();
		fishBowls = new List<FishBowl>();
	}

	public void DeleteEnvironmentObject( int variant_id ) {
		if( !DebugInputHandler.environmentDeleteObjectActive ) {
			return;
		}

		if( staticLODs.Count > 0 ) {
			// update our static lod objects.
			foreach( StaticObjectLODInfo st in staticLODs ) {
				if( st != null ) {
					WemoItemData item = st.obj.GetComponentInChildren<WemoItemData>();
					if( item != null && item.variantID == variant_id ) {
						st.obj.SetActive( false );
						st.objLodData.objectActive = false;
					}
				}
			}
		}
	}

	public void ResetEnvironmentDeletedObjects() {
		if( staticLODs.Count > 0 ) {
			// update our static lod objects.
			foreach( StaticObjectLODInfo st in staticLODs ) {
				if( st != null ){
					st.obj.SetActive(true);
					st.objLodData.objectActive = true;
				}
			}
		}
	}

	void BuildStaticLODsArray() {
		if( staticLODs.Count > 0 ) {
			foreach( StaticObjectLODInfo st in staticLODs ) {
				if( st != null ){
					GameObject.DestroyImmediate(st.obj);
					GameObject.DestroyImmediate(st.objLodData);
					GameObject.DestroyImmediate(st.objTransform);
					st.obj = null;
					st.objLodData = null;
					st.objTransform = null;
				}
			}
			
			staticLODs.Clear();
			staticLODs.TrimExcess();
		}
		
		GameObject[] lods = GameObject.FindGameObjectsWithTag("LODObject");

		int cnt = 0;
		foreach( GameObject lod in lods ) {
			bool in_list = false;
			for( int i = 0; i < SimInstance.Instance.crittersInPopulation; i++ ) {
				if( SimInstance.Instance.critters[i].critterObject == lod ) {
					in_list = true;
					i = SimInstance.Instance.crittersInPopulation;
				}
			}
			if( !in_list ) {
				cnt++;
			}
		}
		if( cnt > 0 ) {
			// TODO>mark them in another list in the loop above? instead of
			foreach( GameObject lod in lods ) {
				bool in_list = false;
				for( int i = 0; i < SimInstance.Instance.crittersInPopulation; i++ ) {
					if( SimInstance.Instance.critters[i].critterObject == lod ) {
						in_list = true;
						i = SimInstance.Instance.crittersInPopulation;
					}
				}
				if( !in_list ) {
					StaticObjectLODInfo new_info = new StaticObjectLODInfo();
					new_info.obj = lod;
					new_info.objTransform = lod.transform;
					new_info.objLodData = lod.GetComponent<LODModelData>();
					LODManager.InitLOD( new_info.objLodData );
					// if not default style, lets set it for lowest lod and not update it
					if(GlobalOceanShaderAdjust.Instance.lodStyle != LODStyle.Default){
						SetLowestLod(new_info.objLodData);
					}
					else{ // don't add it to the list so lod doesn't get evaluated
						staticLODs.Add(new_info);
					}
				}
			}
		}
	}

	private void SetLowestLod(LODModelData lmd){
		// turn on lowest lod and turn off everything else
		if(lmd == null || lmd.LODs == null || lmd.LODs.Length == 0)
			return;
		for(int i = 0;  i < lmd.LODs.Length - 1; i++){
			if(lmd.LODs[i].LOD != null)
				lmd.LODs[i].LOD.SetActive(false);
		}
		lmd.LODs[lmd.LODs.Length - 1].LOD.SetActive(true);

	}

	// UCK. but itll work for now.. revisit when we have a lot of objects to search through
	public static GameObject FindRandomWemoObjectWithAllTags( string[] tags ) {
		if( singleton.wemoObjects == null ) {
			return null;
		}

		int count = 0;
		foreach( WemoObject wemo_obj in singleton.wemoObjects ) {
			if( wemo_obj.gameObject.HasAllTags( tags ) ) {
				wemo_obj.marked = true;
				count++;
			}
			else {
				wemo_obj.marked = false;
			}
		}

		// grab a random one that we found.
		if( count > 0 ) {
			int rnd = Random.Range(0,count);
			int cnt = 0;
			foreach( WemoObject wemo_obj in singleton.wemoObjects ) {
				if( wemo_obj.marked ) {
					if( rnd == cnt ) {
						return wemo_obj.gameObject;
					}
					else {
						cnt++;
					}
				}
			}
		}

		return null;
	}


	public static List<WemoObject> GetWemoObjectsWithAllTags( string[] tags ) {
		if( singleton.wemoObjects == null ) {
			return null;
		}

		List<WemoObject> ret = new List<WemoObject>();
		foreach( WemoObject wemo_obj in singleton.wemoObjects ) {
			if( wemo_obj.gameObject.HasAllTags( tags ) ) {
				ret.Add(wemo_obj);
			}
		}

		return ret;
	}
		// UCK. but itll work for now.. revisit when we have a lot of objects to search through
	public static GameObject FindRandomWemoObjectWithAllTagsAndAnyTags( string[] tags, string[] any ) {
		if( singleton.wemoObjects == null ) {
			return null;
		}

		int count = 0;
		foreach( WemoObject wemo_obj in singleton.wemoObjects ) {
			if( wemo_obj.gameObject.HasAllTags( tags )
               && (any == null || wemo_obj.gameObject.HasAnyTag( any ) ) ) {
				wemo_obj.marked = true;
				count++;
			}
			else {
				wemo_obj.marked = false;
			}
		}

		// grab a random one that we found.
		if( count > 0 ) {
			int rnd = Random.Range(0,count);
			int cnt = 0;
			foreach( WemoObject wemo_obj in singleton.wemoObjects ) {
				if( wemo_obj.marked ) {
					if( rnd == cnt ) {
						return wemo_obj.gameObject;
					}
					else {
						cnt++;
					}
				}
			}
		}

		return null;
	}

	public static void PostSphereLoadSetup( bool not_first_load ) {
		////////////////////////////////////////////
		// YAHOO! We are in a black screen, lets free up some memory!
        if( SimManager.realTimeRunningSim > 1260 ) // ts build gets cleaned up at 21 min in.
        {
			MemoryManager.RunGarbageCollection();
        }

		////////////////////////////////////////////

        if( singleton.oceanSurfaceGameObjects != null && singleton.oceanSurfaceGameObjects.Length > 0 ) {
            foreach( GameObject oc in singleton.oceanSurfaceGameObjects ) {
                oc.SetActive(true);
            }
        }
        
		SimInstance.Instance.realTimeRunningSimInSphere = 0f;

		if( !AppBase.Instance.RunningAsPreview() ) {
			if( !not_first_load ) {
				AppBase.Instance.OnFirstDiveIn();
			}
		}

		// own sphere user is diving into (if they don't already)
		if( AppBase.Instance.RunningAsPreview() &&
            !App.UserManager.query.OwnsItem( null, App.SphereManager.currentSphere.legacyid, UserManager.USER_ITEM_SPHERE ) ) {
            Debug.LogWarning( "User does not own sphere: " + App.SphereManager.LEGACY_GetCurrentSphere() );
		}

        singleton.setupPostLoad = true;
	}

	IEnumerator ApplyLevelStreamingDataFromBundle(Object main_asset)
	{        
		DestroyLevelObject();
        
        yield return null;
        
//		Debug.Log("Creating streaming data obj.");
		GameObject go = GameObject.Instantiate(main_asset) as GameObject;
		go.name = SphereInstance.levelStreamingDataObjName;
//        Debug.Log("Complete " + go.name + ". Looking for: " + SphereInstance.levelLightMapDataObjName);

        // we need a single frame after instantiation so that the awakes and starts can all fire.
        yield return null;

        string failed = string.Empty;
  

        if( CameraManager.IsInTravelCamera() ) {
            oceanSurfaceGameObjects = GameObject.FindGameObjectsWithTag("sphere_ocean_surface") as GameObject[];
            if( oceanSurfaceGameObjects != null && oceanSurfaceGameObjects.Length > 0 ) {
                foreach( GameObject oc in oceanSurfaceGameObjects ) {
                    oc.SetActive(false);
                }
            }
        }
        else {
            oceanSurfaceGameObjects = null;
        }
        
        //yield return null;

/*		Renderer[] renderers = GameObject.FindObjectsOfType(typeof(Renderer)) as Renderer[];
		foreach(Renderer render in renderers ) {
			Debug.Log("lightmap idx " + render.gameObject.name + " :: " + render.lightmapIndex);
		}*/

		foreach (Transform tr in go.GetComponentsInChildren(typeof(Transform)))
		{
			if (tr.parent == go.transform)
			{
				if (tr.name == SphereInstance.levelLightMapDataObjName)
				{
//					Debug.Log("Found lightmap object");

					LightmapDataHolder data = (LightmapDataHolder)tr.gameObject.GetComponent(typeof(LightmapDataHolder));
					if (data != null)
					{
						if(data.lightmapData != null ) {
							// SOMETHING NEEDS TO BE ADJUSTED HERE... LightmapDataHolder does not seem to hold data as expected...
//							Debug.Log("APPLYING LIGHTMAP DATA. Current " + LightmapSettings.lightmaps.Length + " vs " + data.lightmapData.Length + " " + " " + LightmapSettings.lightmapsMode + " vs " + data.lightmapsMode);
							int cnt = data.lightmapData.Length;
							LightmapData[] new_data = new LightmapData[cnt];
							for( int i = 0; i < cnt; i ++ ) {
								LightmapData new_light = new LightmapData();
								new_light.lightmapFar = data.lightmapData[i].far;
								new_light.lightmapNear =  data.lightmapData[i].near;
								new_data[i] = new_light;
							}
							LightmapSettings.lightmaps = new_data;
//							Debug.Log("lightmap ok");
						}
/*						else {
							Debug.Log("lightmap data null");
						}*/

						LightmapSettings.lightmapsMode = data.lightmapsMode;

//						Debug.Log("After " + LightmapSettings.lightmaps.Length.ToString());
					}

					break;
				}
				else
				{
					failed += tr.name + "  --  ";
					if (failed.Length > 100)
					{
//						Debug.Log(failed);
						failed = string.Empty;
					}
				}
			}
		}

		if( failed != string.Empty || ( failed != null && failed.Length > 0 ) ) {
//			Debug.Log("Failed Last:");
//			Debug.Log(failed);
		}
	}

    IEnumerator _Loaded(System.Object main_asset, System.Action postInstantiate) {
        if( main_asset != null ) { // this is possible when loading from a preview scene built webplayer
            if (main_asset is UnityEngine.Object) {
                yield return StartCoroutine(ApplyLevelStreamingDataFromBundle((UnityEngine.Object)main_asset));
            }
        }
        
        // when on the webplayer, this will trigger the html to initialize our network.
        // once the network is setup we will be able to load and instance our fish.
//        Debug.Log("Done loading ground, loading Fish and Audio");
        
        // because our instancer runs during the 'start' state and cannot run during the 'awake' state
        // we must do this here.
        BuildStaticLODsArray();
        
        wemoObjects.Clear();
        wemoObjects.TrimExcess();
        WemoObject[] wemo_objects = GameObject.FindObjectsOfType(typeof(WemoObject)) as WemoObject[];
        wemoObjects.AddRange(wemo_objects);

        fishBowls.Clear();
        fishBowls.TrimExcess();
        FishBowl[] fish_bowls = GameObject.FindObjectsOfType(typeof(FishBowl)) as FishBowl[];
        fishBowls.AddRange(fish_bowls);
        
        ParkingLot.InitNewLevel();
        
        CameraManager.StaticPostLoadSetup(false);        

        // TODO> I feel wrong about this check being here. 
        // items below require components not in the preview scene.
        if( AppBase.Instance.RunningAsPreview() ) {
            if( sphereLoadedFinished != null ) {
                sphereLoadedFinished();
			}
            yield break;
        }
        
        if( sphereLoadedFinished != null ) {
            sphereLoadedFinished();

			if (postInstantiate != null)
				postInstantiate();
        }
    }
    
	public void Loaded(System.Object main_asset, System.Action postInstantiate)
	{
        StartCoroutine(_Loaded(main_asset, postInstantiate));        
	}
	
	public void AssignCritterFishBowl( CritterInfo critter ) {
//		WemoLog.Eyal("----------------------------AssignCritterFishBowl----------- " + critter.critterObject.name);
		critter.generalSpeciesData.fishBowlData = null;
		FishBowlData default_bowl_data = null;
        SpeciesSize use_size = critter.generalSpeciesData.mySize;
        if( critter.generalSpeciesData.myBowlSizeOverride != SpeciesSize.UNKNOWN ) {
            use_size = critter.generalSpeciesData.myBowlSizeOverride;
        }
		foreach( FishBowl bowl in fishBowls ) {
			// SUPER HACK FOR ANGLERS
			if( bowl.defaultBowl ) {
				default_bowl_data = bowl.fishBowlData;
			}
			foreach( SpeciesSize ss in bowl.critterSizeInBowl ) {
				if( ss == use_size ) {
					critter.generalSpeciesData.fishBowlData = bowl.fishBowlData;
					return;
				}
			}
		}
		critter.generalSpeciesData.fishBowlData = default_bowl_data;
	}
	
	public void DestroyLevelObject() {
		//delete if exist
        oceanSurfaceGameObjects = null;
		GameObject levelObj = GameObject.Find(levelStreamingDataObjName);
		if (levelObj != null)
		{
			CameraManager.LevelCamerasRemoved();
            OceanCurrents.Reset();
			GameObject.DestroyImmediate(levelObj);
			if(App.SphereManager.LEGACY_GetCurrentSphere() != 0 ) {
				App.SphereManager.UnloadSphere(App.SphereManager.currentSphere._id);
			}
		}
	}

	public void UpdateSphere() {
		if( staticLODs != null && staticLODs.Count > 0 ) {
			Vector3 curCamPos = CameraManager.GetCurrentCameraPosition();
			// update our static lod objects.
			foreach( StaticObjectLODInfo st in staticLODs ) {
				if( st != null && st.objTransform != null ){
					Vector3 tmp = curCamPos - st.objTransform.position;
					LODManager.UpdateLOD( st.objLodData, tmp.sqrMagnitude );
				}
			}
		}
	}
}
