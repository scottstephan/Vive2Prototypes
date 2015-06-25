using UnityEngine;
using System.Collections;

[System.Serializable]
public class StaticBatch {
    public string materialName;
    public GameObject[] batchObjects;
}

public class AssetBundleStaticBatcher : MonoBehaviour {

    public StaticBatch[] staticBatches = null;

	// Use this for initialization
	void Start () {
        if( staticBatches == null || staticBatches.Length <= 0 ) {
            return;
        }

        for( int i = 0; i < staticBatches.Length; i++ ) {
            StaticBatch sb = staticBatches[i];

            if( sb == null || sb.batchObjects == null || sb.batchObjects.Length <= 0 ) {
                continue;
            }

            GameObject new_parent = new GameObject();
            new_parent.name = sb.materialName + "BATCH";
            new_parent.transform.parent = this.transform;
            StaticBatchingUtility.Combine(sb.batchObjects,new_parent);
        }
	}	
}
