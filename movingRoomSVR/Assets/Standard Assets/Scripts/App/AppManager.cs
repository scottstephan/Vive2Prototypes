using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//using WemoEngine;

public class AppManager : MonoBehaviour {
	
	/*******************************************
	* ENUMS
	********************************************/
	public enum Servers {
		Sandbox,
		Dev,
		Live
	}
	
	/*******************************************
	* VARIABLES
	********************************************/
	public GameObject[] modules;
	
	public Servers API_SERVER = Servers.Sandbox; 
	
	public event Action __appInitialized = null;
	
	/*******************************************
	* UNITY METHODS
	********************************************/
	public void Awake () {
	
		DontDestroyOnLoad( gameObject );
		
		Init();
	}
	public void OnDestroy () {
	}

	/*******************************************
	* MAIN METHODS
	********************************************/
	private void Init(){
		
		// Vars
		int i;
		
		// Init statics
		App.Init( this );
		
		for(i=0; i<modules.Length; i++){
			GameObject oObject = GameObject.Instantiate( modules[ i ] ) as GameObject;
			oObject.transform.parent = transform;
			oObject.name = "_" + oObject.name.ToLower().Replace("(clone)", "");
		}
		
		// Begin our initial load logic
		StartCoroutine( InitialLoad() );
	}
	
	private IEnumerator InitialLoad() {

		// This is where the magic happens; initial load for all user-facing features		
		while ( !App.AppReady() )
			yield return null;
		
        if(AppBase.Instance.RunningAsPreview())
        {
            SphereInstance.Instance.Loaded(null, null);
        }
        else
        {
            App.SphereManager.DiveIntoStartingSphere();
        }
		
		if ( __appInitialized != null ) {
			__appInitialized();
		}
	}
}