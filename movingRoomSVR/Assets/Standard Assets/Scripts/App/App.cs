using UnityEngine;
using System.Collections;

public static class App {

	// Properties	
	public static bool initialized = false;
	
	public static bool requiredNetworkFailure = false;
	
	// Statics
	public static AppManager Manager;
	public static DataManager DataManager;
	public static ApiManager ApiManager;
	public static UserManager UserManager;
	public static SphereManager SphereManager;
	public static FishManager FishManager;
	public static PurchaseManager PurchaseManager;
	public static MetricsManager MetricsManager;
	
	public static DataQuery Query = new DataQuery();
	
	public static DataSettings Settings = new DataSettings();
	
	/*******************************************
	* MAIN METHODS
	********************************************/
	public static void Init( AppManager appM ){
		
		// Create a static reference to the AppManager
		Manager = appM;
		
		// We are initialized!!!
		initialized = true;
	}
	
	/*******************************************
	* VARIABLE METHODS
	********************************************/
	public static bool AppReady() {
		// All requirements before initializing dive-in
		return (
			initialized
			&&
			DataManager.initialized 
			&&
			UserManager.initialized
		);
	}	
}
