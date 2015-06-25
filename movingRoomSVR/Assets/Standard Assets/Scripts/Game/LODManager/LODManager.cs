using UnityEngine;
using System.Collections;

public class LODManager {
	
	public static void InitLOD( LODModelData data ) {
		if( data == null || data.LODs == null || data.LODs.Length <= 0 ) {            
			return;
		}
		
		foreach( LODLevel l in data.LODs ) {
			if( l == null || l.LOD == null ) {
				continue;			
			}
            l.distanceUpgradeSqrd = l.distance-data.distanceThreshold;
            l.distanceUpgradeSqrd *= l.distanceUpgradeSqrd;
            l.distanceDowngradeSqrd = l.distance * l.distance;
			l.LODrenderer = l.LOD.GetComponentInChildren<Renderer>();
			l.LOD.SetActive( false );
			if( l.LODrenderer == null ) {				
				DebugDisplay.AddDebugText("NO RENDER! " + data.gameObject.name + " " + l.LOD.name);
			}
            else {
                l.LODmaterial = l.LODrenderer.sharedMaterial;
            }
		}
        int use_idx = ( data.ForceLOD >= 0 ) ? data.ForceLOD : 0;
        if( data.LODs[use_idx].LOD != null ) {
            data.LODs[use_idx].LOD.SetActive( true );
		}
        data.curLODidx = use_idx;
        data.curLOD = data.LODs[use_idx];		

        if( use_idx > 0 ) {
            for( int i = 0; i < data.LODs.Length; i++ ) {
                if( i != 1 ) {
                    GameObject.Destroy( data.LODs[i].LOD );
                    data.LODs[i].LOD = null;
                }
            }
        }

        // if no LODs have been setup, renderer will be null, so we'll fake a base 0 LOD if possible.
        if (data.curLOD != null && data.curLOD.LODrenderer == null)
        {
            Renderer[] renderers = data.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; ++i)
            {
                if (renderers[i].enabled)
                {
                    data.curLOD.LODrenderer = renderers[i];
                    break;
                }
            }
        }
	}

    public static void ForceLOD( LODModelData data, int force_lod ) {
        data.ForceLOD = force_lod;
        if( data.ForceLOD >= 0 ) {
            if( data.ForceLOD != data.curLODidx ) 
            {
                //            Debug.Log("LOD Switch " +data.curLODidx + " to " + idx);
                data.curLODidx = data.ForceLOD;
                if( data.curLOD.LOD != null ) 
                {
                    data.curLOD.LOD.SetActive( false );
                }
                
                data.curLOD = data.LODs[data.ForceLOD];
                if( data.curLOD.LOD != null ) 
                {
                    data.curLOD.LOD.SetActive( true );
                }
            }       
        }
    }

	public static void CleanupLOD( LODModelData data ) {
		foreach( LODLevel l in data.LODs ) {			
			if( l == null || l.LOD == null ) {
				continue;			
			}
			
/*			GameObject lod = l.LOD;
			Transform lod_trans = lod.transform;
			int lodChildCount = lod_trans.childCount; 
			for(int j = 0; j < lodChildCount; j++)
			{
			    Transform srf = lod_trans.GetChild(j);
				if( srf.renderer.material != null ) {
					GameObject.Destroy( srf.renderer.material );
				}
			}*/

			l.LODrenderer = null;
			l.LODmaterial = null;			
		}
		
		data.curLOD = null;
	}
	
	public static void UpdateLOD( LODModelData data, float new_distance ) {
		if( data == null 
           || data.LODs == null 
           || data.LODs.Length <= 1 
           || data.ForceLOD >= 0
           || !data.objectActive ) 
        {
			return;
		}
		
		if( data.curLOD == null )
        {
			InitLOD( data );
			if( data.curLOD == null ) 
            {
				return;
			}
		}

        int idx = data.LODs.Length-1;

		for( int i = idx; i >= 0; i-- ) 
        {
            if (data.LODs[i].distance < 0f)
            {
                break;
            }

            if (i == data.curLODidx)
            {
                if (new_distance >= data.LODs[i].distanceUpgradeSqrd)
                {
                    break;
                }
            }
            else if( new_distance >= data.LODs[i].distanceDowngradeSqrd ) 
            {
                break;
            }

			--idx;
        }

        idx = Mathf.Clamp(idx, 0, data.LODs.Length);

		if( DebugInputHandler.FORCE_LOD0_OFF && idx == 0 ) 
        {
			idx = 1;
		}

		if( idx != data.curLODidx ) 
        {
//            Debug.Log("LOD Switch " +data.curLODidx + " to " + idx);
			data.curLODidx = idx;
			if( data.curLOD.LOD != null ) 
            {
				data.curLOD.LOD.SetActive( false );
			}

			data.curLOD = data.LODs[idx];
			if( data.curLOD.LOD != null ) 
            {
				data.curLOD.LOD.SetActive( true );
			}
		}       
	}
}
