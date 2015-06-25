using UnityEngine;
using System.Collections;

[System.Serializable]
public class TESTCrittersInSphere {
	public Object critter;
	public int count;
    public string designGroupName;
}

public class TestCritters : MonoBehaviour {
	
	public  TESTCrittersInSphere[] testCritters;
	
    void Start() {
        SphereInstance.sphereLoadedFinished += SphereFinishedLoading;
    }
    
	public void add(Object prefab){
		testCritters[testCritters.Length-1] = new TESTCrittersInSphere();
		testCritters[testCritters.Length-1].count = 1;
		testCritters[testCritters.Length-1].critter = prefab;
	}
	
	public void SphereFinishedLoading() {
        if( testCritters == null 
             || testCritters.Length <= 0 )
        {
            return;
        }
        
		int i = 0;
		foreach( TESTCrittersInSphere critter in testCritters ) {
			i++;
			if(critter.critter == null || critter.count <= 0) {
                continue;
            }

            SimInstance.Instance.AddCrittersToPopulation( critter.critter, critter.count, i, -1, critter.designGroupName );
		}
	}
}