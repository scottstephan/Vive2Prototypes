using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BacteriaData {
    public GameObject go;
    public Transform myTransform;
    public Vector3 velocity;
    public float motionTimer;
    
    public float rotationSpeed;
}

public class BacteriaManager : MonoBehaviour {
 
    public int population;
    
    public FishBowl fishBowl;
    
    public Object[] meshObjects;
    
    public float motionTimeMin = 0.25f;
    public float motionTimeMax = 0.5f;
    
    public float speedMin = 0.1f;
    public float speedMax = 10.5f;
        
    public float rotateMin = -30f;
    public float rotateMax = 30f;

    private float oceanCurrentFactor = 0.2f;
    
    private List<BacteriaData> bacteriaList;
	// Use this for initialization
	void Start () {
        
        // gather the variant data..
        if( !AppBase.Instance.RunningAsPreview() ) {
            foreach( Object ob in meshObjects ) {
                GameObject go = (GameObject)ob;
                WemoItemData item_data = go.GetComponent<WemoItemData>();
                if( item_data != null ) {
                    App.FishManager.VariantHasParentData( item_data.variantID, false );
                }
            }
        }
        
        bacteriaList = new List<BacteriaData>();
        for( int i = 0; i < population; i++ ) {
            BacteriaData new_data = new BacteriaData();
            
            int idx = Random.Range(0, meshObjects.Length);
            FishBowlData bd = fishBowl.fishBowlData;
//               DebugDisplay.AddDebugText(" intro fish bowl " +  bd.name + " :: p " + bd.position + " :: r "  + bd.rotation + " :: s " + bd.size);
            Vector3 rand_pos = RandomExt.VectorRange(bd.size);            
            Vector3 new_pos = bd.position + ( bd.rotation * rand_pos );
            new_data.go = GameObject.Instantiate(meshObjects[idx], new_pos, Random.rotation) as GameObject;
            new_data.myTransform = new_data.go.transform;
            
            float speed = Random.Range(speedMin, speedMax);
            new_data.velocity = Random.insideUnitSphere * speed;
            new_data.motionTimer = Random.Range(motionTimeMin, motionTimeMax);
            new_data.rotationSpeed = Random.Range(rotateMin, rotateMax);
            
            bacteriaList.Add(new_data);
        }	
	}
	
	// Update is called once per frame
	void Update () {
//        Transform cam = CameraManager.GetCurrentCameraTransform();
        float dt = Time.deltaTime;
        Vector3 cur_dir = Vector3.zero;
        if( OceanCurrents.Singleton != null ) {
            cur_dir = OceanCurrents.Singleton.currentDirection;
        }
        foreach( BacteriaData bd in bacteriaList ) {
            bd.motionTimer -= dt;
            if( bd.motionTimer < 0f ) {
                float speed = Random.Range(speedMin, speedMax);
                bd.velocity = Random.insideUnitSphere * speed;
                bd.motionTimer = Random.Range(motionTimeMin, motionTimeMax);
            }

/*            if( !CameraManager.IsInTravelCamera() && !bd.go.renderer.isVisible ) {
                // warp us to the opposite side of the camera
                Vector3 rel = cam.InverseTransformPoint(bd.myTransform.position);
                rel.x *= -1f;
                bd.myTransform.position = cam.TransformPoint(rel);
            }*/
           
            bd.myTransform.position += bd.velocity * dt;
            
            bd.myTransform.position += ( cur_dir * oceanCurrentFactor );
            
            bd.myTransform.Rotate(bd.myTransform.up,bd.rotationSpeed * dt);
        }
	}
    
    void OnDestroy() {
        foreach( BacteriaData bd in bacteriaList ) {
            bd.myTransform = null;
            Destroy(bd.go);
            bd.go = null;
        }
        
        bacteriaList.Clear();
        bacteriaList.TrimExcess();
        bacteriaList = null;
    }
}
