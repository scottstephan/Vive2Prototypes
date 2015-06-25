using UnityEngine;
using System.Collections;

[System.Serializable]
public class MeshToDisplay {
	public Object prefab;
	
	[HideInInspector]
	public GameObject mesh;
}

public class TestMesh: MonoBehaviour {
	
	public MeshToDisplay meshToDisplay;
	private bool firstUpdate = false;
	
	public TestMesh(){
		meshToDisplay = new MeshToDisplay();
	}
	
	public void add(GameObject mesh, Object prefab){
		meshToDisplay = new MeshToDisplay();
		meshToDisplay.mesh = mesh;
		meshToDisplay.prefab = prefab;
	}

	
	void Update () {
		if( !firstUpdate ) {	
			if(meshToDisplay.prefab != null){
				meshToDisplay.mesh = GameObject.Instantiate(meshToDisplay.prefab) as GameObject;
				meshToDisplay.mesh.transform.position = GameObject.Find("sbpoint").transform.position;
				if(meshToDisplay.mesh.GetComponent<MeshViewerAnimationController>() == null)
					meshToDisplay.mesh.AddComponent<MeshViewerAnimationController>();
			}
			firstUpdate = true;
		}
	}
}