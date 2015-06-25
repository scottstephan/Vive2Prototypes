using UnityEngine;
using System.Collections;
//using UnityEditor;
using System.Collections.Generic;
using System.IO;


public class PreviewTurntableApp : PreviewApp {
	
	public override bool Initialized() {
		return false;
	}
	// Use this for initialization
	public override void Start () {
		//base.Start();
	}
	
	
	// Update is called once per frame
	void Update () {
		if( !firstUpdate ) {	
			
			//PreviewTurntableApp preview_app = Object.FindObjectOfType(typeof(PreviewTurntableApp)) as PreviewTurntableApp;
		
			//TestMesh testMesh = Object.FindObjectOfType(typeof(TestMesh)) as TestMesh;
			//testMesh = new TestMesh(testMesh.mesh);
		
			//testMesh.Mesh.transform.position = GameObject.Find("sbpoint").transform.position;


			//testMesh.Mesh.AddComponent<MeshViewerAnimationController>();
		
			//GameObject.DestroyImmediate(mesh);
			//CSAdjustGlobalShderParams.AdjustGlobalShaderParams();
			firstUpdate = true;

			//float dt = Time.deltaTime;

			//InputManager.UpdateInput( dt );		
			
			//SphereManagerOLD.UpdateSpheres();
	
			//SimManager.UpdateSim( dt );
		}
	}
}