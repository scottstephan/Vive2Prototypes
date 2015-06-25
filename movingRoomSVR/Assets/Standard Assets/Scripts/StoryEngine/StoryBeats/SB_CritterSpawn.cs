using UnityEngine;
using System.Collections;

[System.Serializable]
public class CritterToSpawn {
	public Object critter;
	public int count;
	public string designGroupName;
	public bool relativeToCamera;
	public Transform spawnLocation;
    public float customScale = -1f;
    public float customScaleAddRandomness = 0f;
    public bool putInHold = false;
    public bool randomizeDirection;
    [HideInInspector]
    public CritterInfo[] infos;
}

public class SB_CritterSpawn : SBBase {
	
    public CritterToSpawn[] spawnCritters;

    bool bCreated;
    bool bSpawned;

    public override bool ContainsDesignGroup( string design_group ) {
        if( spawnCritters == null || spawnCritters.Length <= 0 ) {
            return false;
        }

        for (int i=0; i<spawnCritters.Length; ++i) {
            CritterToSpawn spawn = spawnCritters[i];
            if( spawn == null ||
                spawn.critter == null) {
                continue;
            }

            if( string.IsNullOrEmpty( spawn.designGroupName ) ) {
                continue;
            }
            if( spawn.designGroupName.ToUpper().Equals( design_group ) ) {
                return true;
            }
        }

        return false;
    }

    public void Awake()
    {
        StartCoroutine( SpawnPool( ) );
    }

    IEnumerator SpawnPool( ) 
    {
        if (SphereInstance.Instance == null ||
            !SphereInstance.Instance.setupPostLoad)
        {
            yield return new WaitForSeconds( 0.1f );
        }

        
        for (int i=0; i<spawnCritters.Length; ++i)
        {
            CritterToSpawn spawn = spawnCritters[i];
            if (spawn == null ||
                spawn.critter == null)
            {
                continue;
            }

            spawn.infos = new CritterInfo[spawn.count];
            DataItem variant = App.Query.GetItemByUrlkey(DataQuery.ItemTypes.fishvariant, spawn.critter.name );
            int variant_id = variant == null ? -1 : variant.legacyid;
            
            Vector3 pos = spawn.spawnLocation != null ? spawn.spawnLocation.position : Vector3.zero;

            for (int j=0; j<spawn.count; ++j)
            {
                CritterInfo c = spawn.infos[j] = new CritterInfo();
                float scale = spawn.customScale;
                if (spawn.customScaleAddRandomness > 0f)
                {
                    scale += Random.value * spawn.customScaleAddRandomness;
                }

                SimInstance.Instance.InitCritterInfo(c, spawn.critter, variant_id, -1, scale, spawn.designGroupName, pos);

                if (j == 0 && c.critterItemData != null)
                {
                    TourCompleteMenuManager.SpeciesAdd(c.critterItemData.speciesName);
                }

				AI.Start(c, false);
                c.removedCallback = OnRemoved;               
            }
        }
        
		Shader.WarmupAllShaders();
		yield return null; // one frame to get collision volumes worked out.

		for (int i=0; i<spawnCritters.Length; ++i)
		{
			CritterToSpawn spawn = spawnCritters[i];
			if (spawn == null ||
			    spawn.critter == null)
			{
				continue;
			}
			
			for (int j=0; j<spawn.count; ++j)
			{
				CritterInfo c = spawn.infos[j];

				c.critterObject.SetActive(false);
				c.critterAnimation.enabled = false;
				
				if (c.dbVariantID == -1)
				{
					c.dbVariantID = 1;
					c.dbItemID = 1;
				}
			}
		}

		bCreated = true;
    }

    public void OnRemoved(CritterInfo critter)
    {
        SimInstance.Instance.StopBinauralAudio( critter );
        critter.critterObject.SetActive (false);
        critter.critterAnimation.enabled = false;
    }
        
    public override void Reset() 
    {
		bSpawned = false;
		base.Reset();
	}
	
	public override void UpdateBeat() 
    {	
        if (!bCreated) 
        {
            return;
        }

        if( spawnCritters == null 
           || spawnCritters.Length <= 0 )
        {
            return;
        }
        
        for( int i = 0; i < spawnCritters.Length; i++ )
        {
            CritterToSpawn spawn = spawnCritters[i];

            if(spawn.critter == null || spawn.count <= 0) 
            {
                continue;
            }

#if UNITY_EDITOR
//            Debug.Log("SB_CritterSpawn "  + gameObject.name + " Prefab: " + spawn.critter.name + " Group: " + spawn.designGroupName);
#endif
			if( spawn.spawnLocation != null ) 
            {
//				Collider c = spawn.spawnLocation.collider;
				Vector3 pos = spawn.spawnLocation.position;

				Quaternion rot = spawn.spawnLocation.rotation;

				if( spawn.relativeToCamera ) 
                {
					Vector3 cam_pos = CameraManager.GetCurrentCameraPosition();
					Quaternion cam_rot = CameraManager.GetCurrentCameraFlattenedRotation();

					pos = cam_pos + ( cam_rot * pos );
					rot = cam_rot * rot;
				}

				Vector3 ls = spawn.spawnLocation.localScale;

                CritterInfo first_critter = spawn.infos[0];
                first_critter.critterTransform.position = pos;
                first_critter.critterTransform.rotation = rot;
                InitCritter(first_critter);

                // for testing when new prefab has not been added to db
                if( spawn.putInHold ) 
                {
                    AI.ForceSwitchToBehavior( first_critter, SwimBehaviorType.HOLD );
                }

				if( spawn.count > 1 ) 
                {
					int x, y;
					Vector3 cs = first_critter.generalMotionData.critterBoxColliderSize;
					int steps_x = (int)(ls.x / cs.x);
					int steps_y = (int)(ls.y / cs.y);
					int steps_z = (int)(ls.z / cs.z);

					int[] idxs = new int[spawn.count];
					int t_s = steps_x * steps_y * steps_z;
					if( t_s < spawn.count )
                    {
                        Debug.LogError("SPAWN VOLUME IS TOO SMALL FOR FISH COUNT. ONLY FIRST FISH WAS SPAWNED:" + gameObject.name + " Group: " + spawn.designGroupName + " " + spawn.critter.name);
					}
					else 
                    {
						for( x = 0; x < spawn.count; x++ )
                        {
							idxs[x] = Random.Range(0,t_s);
							bool idx_ok = false;
							while( !idx_ok ) 
                            {
								idx_ok = true;
								for( y = 0; y < x; y++ ) 
                                {
									if( idxs[y] == idxs[x] ) 
                                    {
										idx_ok = false;
									}
								}
								if( !idx_ok ) 
                                {
									idxs[x]++;
									if( idxs[x] >= t_s ) 
                                    {
										idxs[x] = 0;
									}
								}
							}

							// get indexs from idx.
							int floor_cnt = steps_x * steps_y;
							int floors = idxs[x] / floor_cnt;
							int apt = idxs[x] % floor_cnt;
							int j = apt % steps_x;
							int k = apt / steps_x;

							// get grid space.
							Vector3 np = Vector3.right * j * cs.x;
							np += Vector3.up * k * cs.y;
							np += Vector3.forward * floors * cs.z;
							np -= ( ls * 0.5f );	// recenter.
                            Vector3 rotNP = rot * np;
							Vector3 fp = pos + rotNP;
							if( x == 0 )
                            {
								first_critter.critterTransform.position = fp;
							}
							else
                            {
                                CritterInfo addedCritter = spawn.infos[x];
                                addedCritter.critterTransform.position = fp;
                                addedCritter.critterTransform.rotation = rot;
                                if (spawn.randomizeDirection)
                                {
                                    addedCritter.critterTransform.rotation = Random.rotation;
                                }
                                InitCritter(addedCritter);

                                if( spawn.putInHold ) 
                                {
                                    AI.ForceSwitchToBehavior( addedCritter, SwimBehaviorType.HOLD );
                                }
                            }
						}
					}
				}
				else if( !MathfExt.Approx( ls, Vector3.one, 0.1f ) ) 
                {
					// hard place the critter in the spawn zone.
					Vector3 rand_spot = RandomExt.VectorRange( spawn.spawnLocation.localScale );
					first_critter.critterTransform.position = pos + ( rot * rand_spot );
				}
			}
			else 
            {
                for (int x=0; x<spawn.count; ++x)
                {
                    CritterInfo c = spawn.infos[x];
                    InitCritter(c);
                    FishController.FindSpawnPosition( c, false );

                    if( spawn.putInHold ) 
                    {
                        Debug.LogError( "SB_CritterSpawn "  + gameObject.name + ": Group : " + spawn.designGroupName + " :: SpawnLocation required for putInHold to work.");
                    }
                }
			}
        }		

        base.UpdateBeat();

        bSpawned = true;
	}
	
    void InitCritter(CritterInfo c)
    {
        c.markedForRemove = false;
        c.critterObject.SetActive(true);
        c.critterAnimation.enabled = true;
        SphereInstance.Instance.AssignCritterFishBowl( c );
        SimInstance.Instance.AddCritterToPopulation(c);
        c.generalSpeciesData.swimToPoint = false;
        c.generalSpeciesData.switchBehavior = true;
        c.swimToPointData.pointReachedType = PointReachedType.EnterTargetedMotion;

        SimInstance.Instance.PlayBinauralAudio( c );
    }
	
	public override bool IsComplete()
    {
        return bSpawned;
	}			

    void OnDestroy() {
        for (int i=0; i<spawnCritters.Length; ++i)
        {
            CritterToSpawn spawn = spawnCritters[i];
            if (spawn == null ||
                spawn.critter == null)
            {
                continue;
            }
            
            for( int j = 0; j < spawn.count; j++ ) {
				if( spawn.infos[j] != null && spawn.infos[j].critterObject != null) {
                    if( !spawn.infos[j].critterObject.activeSelf ) {
                        spawn.infos[j].critterObject.SetActive( true );
                        DestroyImmediate(spawn.infos[j].critterObject);
                    }
                    else {
                        spawn.infos[j].removedCallback = null;
                    }
                }
                spawn.infos[j] = null;
            }
            spawn.infos = null;
        }
    }
}
