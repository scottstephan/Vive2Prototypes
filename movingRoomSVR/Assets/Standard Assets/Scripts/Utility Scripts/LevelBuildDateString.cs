using UnityEngine;
using System.Collections;

public class LevelBuildDateString : MonoBehaviour {
	[HideInInspector]
	public string buildDate;
	
	void Start () {
		/*
		if( buildDate == null || buildDate.Length <= 0 ) {
			return;
		}
		
		int sphere_id = 0;
		if( App.SphereManager.LEGACY_IsLoadingSphere() ) {
			sphere_id = App.SphereManager.LEGACY_GetLoadingSphere();
		}
		else {
			sphere_id = App.SphereManager.LEGACY_GetCurrentSphere();
		}
		SphereData sphere_data = SphereDataManager.GetData( sphere_id );
		Log.Main.Trace( "Instantiated :: " + sphere_data.name + " :: Build Date :: " + buildDate );
		*/
	}	
}
