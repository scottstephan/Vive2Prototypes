using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using System.IO;
#endif

public class SnapshotSetup : MonoBehaviour {

#if UNITY_EDITOR
    void TakeSnapshot() {
#if !UNITY_WEBPLAYER		
        string json_output = "[";
        bool first = true;
        foreach( CritterInfo critter in SimInstance.Instance.critters ) {
            if( critter != null 
                && critter.dbVariantID > 0 
                && critter.critterTransform != null ) {
                if( first ) {
                    json_output += "{";
                    first = false;
                }
                else {
                    json_output += ",{";
                }
                // variant id
                json_output += "\"variant_id\":" + critter.dbVariantID + ",";
                // position
                json_output += "\"position\":{";
                json_output += "\"x\":" + critter.critterTransform.position.x + ",";
                json_output += "\"y\":" + critter.critterTransform.position.y + ",";
                json_output += "\"z\":" + critter.critterTransform.position.z + "},";
                // rotation
                json_output += "\"rotation\":{";
                json_output += "\"x\":" + critter.critterTransform.rotation.x + ",";
                json_output += "\"y\":" + critter.critterTransform.rotation.y + ",";
                json_output += "\"z\":" + critter.critterTransform.rotation.z + ",";
                json_output += "\"w\":" + critter.critterTransform.rotation.w + "},";
                // current behavior.
                json_output += "\"behavoir\":" + critter.generalSpeciesData.myCurrentBehaviorType.ToString() + ",";
                // speed information.
                json_output += "\"current_speed\":" + critter.generalMotionData.currentSpeed + ",";
                json_output += "\"current_accel\":" + critter.generalMotionData.currentAcc + ",";

                // final bracket.
                json_output += "}"; 
            }
        }
        json_output += "]";
        
        string snapshot_data_dir = string.Concat(Application.dataPath, "/../Snapshots/");
        if(!Directory.Exists(snapshot_data_dir))
        {
            Directory.CreateDirectory(snapshot_data_dir);
        }
        string snapshot_file = snapshot_data_dir + Application.loadedLevelName + ".json";
        File.WriteAllText(snapshot_file,json_output);
#endif
    }
#endif
    
    void Update() {
#if UNITY_EDITOR
        if( InputManager.GetKeyDown("s") ) {
            TakeSnapshot();
        }
#endif        
    }
}
