using UnityEngine;
using System.Collections;

public class SimManager {

	public static int cameraTargetIndex = 0;
	public static int cameraTargetItemID;

	public static float avgDeltaTime = (1f/30f); // start us off at 30hz

    public static float realTimeRunningSim = 0f;

	private static bool blockCameraTargetSwitching = false;
	
	// class log
//    private static Log log = Log.GetLog(typeof(SimManager).FullName);


	public static void Construct () {
		AI.Construct();
		FishController.Construct();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	public static void SwitchToVariantCameraTarget( int variant_id ) {
		CritterInfo critter = GetCritterForCameraTarget();

		if( critter == null ) {
			return;
		}

		if( critter.dbVariantID == variant_id && !CameraManager.CurrentCameraFollowsTargets() ) {
			CameraManager.SwitchToTarget( critter.critterObject, CameraType.FollowBehindCamera );
			return;
		}

		foreach( CritterInfo cr in SimInstance.Instance.critters ) {
			if( cr.dbVariantID == variant_id ) {
				CameraManager.SwitchToTarget( cr.critterObject, CameraType.FollowBehindCamera );
				return;
			}
		}
	}

    public static GameObject GetClosestCameraTarget( bool use_sphere_center ) {
        if( SimInstance.Instance.crittersInPopulation <= 0 || blockCameraTargetSwitching ) {
            return null;
        }
        
        float dist_sqr = float.MaxValue;
        GameObject closest = null;
        Vector3 center_pos = Vector3.zero;
        if( use_sphere_center ) {
            center_pos = CameraManager.GetFollowCamSphereCenter();
        }
        else {
            center_pos = CameraManager.GetCurrentCameraPosition();
        }
        for( int i = 0; i < SimInstance.Instance.crittersInPopulation; i++ ) {
            CritterInfo critter = SimInstance.Instance.critters[i];
            if( !SimInstance.Instance.IsCritterLeavingScene( i ) ) {
                Vector3 diff = center_pos - critter.critterTransform.position;
                float my_dist = diff.sqrMagnitude;
                if( my_dist < dist_sqr ) {
                    dist_sqr = my_dist;
                    closest = critter.critterObject;
                }
            }
        }
        
        return closest;
    }
    
	public static GameObject GetRandomCameraTarget( bool new_species ) {
		if( SimInstance.Instance.crittersInPopulation <= 0 || blockCameraTargetSwitching ) {
			return null;
		}

		int new_idx = cameraTargetIndex;
		if( SimInstance.Instance.crittersInPopulation == 1 ) {
			new_idx = 0;
		}
		else if( SimInstance.Instance.crittersInPopulation <= 4 ) {
			bool done = false;
			int og = cameraTargetIndex;
			new_idx = cameraTargetIndex;
			while( !done ) {
				new_idx++;
				if( new_idx > 4 ) {
					new_idx = 0;
				}
				
				if( new_idx == og ) {
					done = true;
				}
				
				CritterInfo critter = SimInstance.Instance.GetCritterInfoFromIndex( new_idx );
				if( critter != null && !SimInstance.Instance.IsCritterLeavingScene( new_idx ) ) { 
					if( CameraManager.IsCritterWithinViewableDistance( critter ) ) {
						done = true;
					}
				}
			}
		}
		else {
			int og = Random.Range(0,SimInstance.Instance.crittersInPopulation-1);
			bool done = !new_species;
			new_idx = og;
			while( !done ) {
				CritterInfo critter = SimInstance.Instance.GetCritterInfoFromIndex( new_idx );
				if( critter == null
				   || SimInstance.Instance.IsCritterLeavingScene(new_idx)
				   || ( cameraTargetIndex == new_idx && SimInstance.Instance.crittersInPopulation > 1 )
				   || cameraTargetItemID == SimInstance.Instance.critters[new_idx].dbItemID 
				   || !CameraManager.IsCritterWithinViewableDistance( critter ) ) {
					new_idx++;
					if( new_idx >= SimInstance.Instance.crittersInPopulation ) {
						new_idx = 0;
					}
					if( new_idx == og ) { // incase we only have one type of viewable fish in our sphere.
						done = true;
						new_species = false; // force the next block of code to run to choose a random fish.
					}
				}
				else {
					done = true;
				}
			}
			if( !new_species ) {
				done = false;
				while( !done ) {
					CritterInfo critter = SimInstance.Instance.GetCritterInfoFromIndex( new_idx );
					if( critter == null 
					   || SimInstance.Instance.IsCritterLeavingScene(new_idx)
					   || new_idx == cameraTargetIndex 
					   || !CameraManager.IsCritterWithinViewableDistance( critter ) ) {
						new_idx++;
						if( new_idx >= SimInstance.Instance.crittersInPopulation ) {
							new_idx = 0;
						}
						if( og == new_idx ) {
							done = true;
							new_idx = -1;
						}
					}
					else {
						done = true;
					}
				}
			}
		}
		cameraTargetIndex = new_idx;
		if( new_idx == -1 ) {
			cameraTargetIndex = 0;
			return null;
		}
		cameraTargetItemID = SimInstance.Instance.critters[cameraTargetIndex].dbItemID;
		return SimInstance.Instance.critters[cameraTargetIndex].critterObject;
	}

	public static GameObject IncrementCameraTarget( bool new_species, int direction ) {
		if( SimInstance.Instance.crittersInPopulation <= 0 || blockCameraTargetSwitching ) {
			return null;
		}

		if( cameraTargetIndex < 0 
		   || cameraTargetIndex >= SimInstance.Instance.crittersInPopulation ) {
			cameraTargetIndex = 0;
		}

		int og = cameraTargetIndex;
		int item = SimInstance.Instance.critters[cameraTargetIndex].dbItemID;
		int bestitem = -1;
		int bestitemidx = -1;
		int curitemid = -1;
		bool done = false;
		bool find_opp = false;
		while( !done ) {
			cameraTargetIndex += direction;
			if(cameraTargetIndex >= SimInstance.Instance.crittersInPopulation ) {
				cameraTargetIndex = 0;
			}
			else if(cameraTargetIndex < 0 ) {
				cameraTargetIndex = SimInstance.Instance.crittersInPopulation - 1;
			}
			curitemid = SimInstance.Instance.critters[cameraTargetIndex].dbItemID;
			if( og == cameraTargetIndex ) {
				if( new_species ) {
					if( bestitemidx != -1 ) {
						cameraTargetIndex = bestitemidx;
						done = true;
					}
					else if( !find_opp ) {
						find_opp = true;
					}
					else {
						done = true;
					}
				}
				else {
					done = true;
				}
			}
			else if( SimInstance.Instance.IsCritterLeavingScene( cameraTargetIndex ) ) {
//				log.Debug("Critter is leaving");
				continue;
			}
			else if( !new_species ) {
				if( item == curitemid ) {
					done = true;
				}
			}
			else if( new_species ) {
				if( item != curitemid ) {
					if( find_opp ) {
						if( direction > 0 ) {
							if( curitemid < item ) {
								if( bestitem == -1 || curitemid < bestitem ) {
									bestitem = curitemid;
									bestitemidx = cameraTargetIndex;
								}
							}
						}
						else if( direction < 0 ) {
							if( curitemid > item ) {
								if( bestitem == -1 || curitemid > bestitem ) {
									bestitem = curitemid;
									bestitemidx = cameraTargetIndex;
								}
							}
						}
					}
					else {
						if( direction > 0 ) {
							if( curitemid > item ) {
								if( bestitem == -1 || curitemid < bestitem ) {
									bestitem = curitemid;
									bestitemidx = cameraTargetIndex;
								}
							}
						}
						else if( direction < 0 ) {
							if( curitemid < item ) {
								if( bestitem == -1 || curitemid > bestitem ) {
									bestitem = curitemid;
									bestitemidx = cameraTargetIndex;
								}
							}
						}
					}
				}
			}
		}
		
		if( SimInstance.Instance.IsCritterLeavingScene( cameraTargetIndex ) ) {
//			log.Debug("Choosing a leaving critter! Prepping to CRASH.");
			return null;
		}
		
		cameraTargetItemID = SimInstance.Instance.critters[cameraTargetIndex].dbItemID;
		return SimInstance.Instance.critters[cameraTargetIndex].critterObject;
	}

	public static CritterInfo GetCritterForCameraTarget() {
		if( cameraTargetIndex >= 0 && cameraTargetIndex < SimInstance.Instance.crittersInPopulation ) {
			return SimInstance.Instance.critters[cameraTargetIndex];
		}

		return null;
	}

	public static void SetBlockCameraTargetSwitching( bool block ) {
		blockCameraTargetSwitching = block;
	}

	public static bool IsCameraTargetSwitchingBlocked() {
		return blockCameraTargetSwitching;
	}

	public static GameObject GetCameraTarget() {
		if( SimInstance.Instance.crittersInPopulation <= 0 || blockCameraTargetSwitching ) {
			return null;
		}

		GameObject ret = SimInstance.Instance.critters[cameraTargetIndex].critterObject;
		if( InputManager.GetKeyDown("up") ) {
			ret = IncrementCameraTarget(false,1);
		}
		else if( InputManager.GetKeyDown("down") ) {
			ret = IncrementCameraTarget(false,-1);
		}
		else if( InputManager.GetKeyDown("right") ) {
			ret = IncrementCameraTarget(true,-1);
		}
		else if( InputManager.GetKeyDown("left") ) {
			ret = IncrementCameraTarget(true,1);
		}

		return ret;
	}

	public static void SetCameraCritterIndex( int index ) {
		if( blockCameraTargetSwitching ) {
			return;
		}

		cameraTargetIndex = index;
		if( index >= 0 && index < SimInstance.Instance.crittersInPopulation ) {
			cameraTargetItemID = SimInstance.Instance.critters[cameraTargetIndex].dbItemID;
		}
	}

	public static void UpdateSim( float dt ) {

		avgDeltaTime = avgDeltaTime*0.97f + 0.03f*dt;

        if( !CameraManager.IsInIntroCamMode() ) {
            realTimeRunningSim += dt;
        }
        
		SimInstance.Instance.UpdateSingular(dt);
	}
}
