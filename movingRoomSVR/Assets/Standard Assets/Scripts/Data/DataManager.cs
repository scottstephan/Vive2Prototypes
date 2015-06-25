using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;

public class DataManager : MonoBehaviour {
	
	/*******************************************
	* CONSTANTS
	********************************************/
	// Namespaces for data that's saved in player prefs
	public const string SPHERE_DATA = "SPHEREDATA";
	public const string FISHPARENT_DATA = "FISHPARENTDATA";
	public const string FISHVARIANT_DATA = "FISHVARIANTDATA";
	
	/*******************************************
	* VARIABLES
	********************************************/
	// Data Properties
	public DataApi api;
	public DataSettings settings;
	public DataItem[] spheres;
	public DataItem[] fishvariants;
	public DataItem[] fishparents;

	// Owned Items / Users that client collects data on that gets cached on a per-session 
	// basis so that we don't need to collect it from a network call again
	public List<DataUserItem> ownedItems;
	public List<DataUser> otherUsers;
	
	// Initialization listener
	public event Action __dataManagerInited = null;
	
	// State
	[HideInInspector]public bool initialized = false;
	[HideInInspector]private bool requiredApiCallFailed = false;
	
	/*******************************************
	* UNITY METHODS
	********************************************/
	public void Awake () {
	
		// Set static singleton reference
		App.DataManager = this;
	}
	
	public void Start() {
	}
	
	public void Update() {
		if ( App.ApiManager.initialized 
			&& !initialized
			&& !requiredApiCallFailed 
			&& App.SphereManager.initialized 
			&& App.FishManager.initialized
		) {
			
			initialized = true;

			// Raise data manager initiation event
			if ( __dataManagerInited != null ) {
				__dataManagerInited();
				
				__dataManagerInited = null;
			}
		}
	}
	
	public void OnDestroy () {
		
	}

	
	/*******************************************
	* DATA RETRIEVAL METHODS
	********************************************/
	public void Init() {				
		// Get the rest of data needed before dive-in
		StartCoroutine( GetRequiredData( true ) );
	}
	
	public IEnumerator GetRequiredData( bool force=false ) {
		
		// Wait till API information is ready before we attempt to retrieve other data
		while ( !App.ApiManager.initialized )
			yield return null;
		
		App.SphereManager.GetSphereData( force );
		App.FishManager.GetFishParentData( force );
		App.FishManager.GetFishVariantData( force );
	}

	public string GetCachedData( string dataNamespace ) {
		string xDataString = PlayerPrefs.GetString( dataNamespace );
		
		if ( xDataString == null || xDataString.Trim() == "" )
			return null;
		
		return xDataString;
	}

	/*******************************************
	* SESSION CACHE METHODS
	********************************************/
	public DataUser GetCachedUserByID( string ID ) {
		if ( otherUsers.Count <= 0 )
			return null;
		
		foreach( DataUser otherUser in otherUsers ) {
			if ( otherUser._id == ID ) 
				return otherUser;
		}		
		return null;
	}

	public DataUser GetCachedUserByLegacyID( int ID ) {
		if ( otherUsers.Count <= 0 )
			return null;
		
		foreach( DataUser otherUser in otherUsers ) {
			if ( otherUser.userid == ID ) 
				return otherUser;
		}		
		return null;
	}

}
