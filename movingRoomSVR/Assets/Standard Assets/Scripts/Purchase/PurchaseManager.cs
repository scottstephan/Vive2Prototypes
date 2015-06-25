using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using OculusIAPAndroid;
using OculusIAPAndroid.Model;
using JsonFx.Json;
//using WemoEngine;

public class PurchaseManager : MonoBehaviour {
	#if UNITY_ANDROID
	/*******************************************
	* VARIABLES
	********************************************/

	// State
	[HideInInspector] public bool initialized = false;

	// Gets or sets the current user
	[HideInInspector] public User CurrentUser { get; set; }
	
	// Gets or sets the current inventory.
	[HideInInspector] public Inventory CurrentInventory { get; set; }
	
	// Gets or sets the purchased entitlements.
	[HideInInspector] public IList<Entitlement> PurchasedEntitlements { get; set; }
	
	// Gets or sets the consumption.
	[HideInInspector] public Consumption Consumption { get; set; }
	
	// The scroll view position.
	[HideInInspector] private Vector2 scrollPos;

	// Callbacks

	/*******************************************
	* UNITY METHODS
	********************************************/
	public void Awake () {
		// Set static singleton reference
		App.PurchaseManager = this;
	}
	
	public void OnDestroy () {
	}

	public void OnEnable() {
		OculusIAPManager.BillingSupportedEvent += OnBillingSupported;
		OculusIAPManager.BillingNotSupportedEvent += OnBillingNotSupported;
		
		OculusIAPManager.GetUserSucceededEvent += OnGetUserSucceeded;
		OculusIAPManager.GetUserFailedEvent += OnGetUserFailed;
		
		OculusIAPManager.LoginSucceededEvent += OnLoginSucceeded;
		OculusIAPManager.LoginFailedEvent += OnLoginFailed;
		
		OculusIAPManager.QueryInventorySucceededEvent += OnQueryInventorySucceeded;
		OculusIAPManager.QueryInventoryFailedEvent += OnQueryInventoryFailed;
		
		OculusIAPManager.PurchaseOfferSucceededEvent += OnPurchaseOfferSucceeded;
		OculusIAPManager.PurchaseOfferFailedEvent += OnPurchaseOfferFailed;
		
		OculusIAPManager.ConsumeEntitlementSucceededEvent += OnConsumeEntitlementSucceeded;
		OculusIAPManager.ConsumeEntitlementFailedEvent += OnConsumeEntitlementFailed;
	}

	public void OnDisable() {
		OculusIAPManager.BillingSupportedEvent += OnBillingSupported;
		OculusIAPManager.BillingNotSupportedEvent += OnBillingNotSupported;
		
		OculusIAPManager.GetUserSucceededEvent += OnGetUserSucceeded;
		OculusIAPManager.GetUserFailedEvent += OnGetUserFailed;
		
		OculusIAPManager.LoginSucceededEvent += OnLoginSucceeded;
		OculusIAPManager.LoginFailedEvent += OnLoginFailed;
		
		OculusIAPManager.QueryInventorySucceededEvent += OnQueryInventorySucceeded;
		OculusIAPManager.QueryInventoryFailedEvent += OnQueryInventoryFailed;
		
		OculusIAPManager.PurchaseOfferSucceededEvent += OnPurchaseOfferSucceeded;
		OculusIAPManager.PurchaseOfferFailedEvent += OnPurchaseOfferFailed;
		
		OculusIAPManager.ConsumeEntitlementSucceededEvent += OnConsumeEntitlementSucceeded;
		OculusIAPManager.ConsumeEntitlementFailedEvent += OnConsumeEntitlementFailed;
	}
	
/*	TODO> RICH.RALPH. NEEDS TO BE CONVERTED TO NON _ UNITY GUI . COMMENTED OUT FOR PERFORMANCE REASONS ON DEVICE>
 *  public void OnGUI() {
		float horizRatio = Screen.width / 432.0f;
		
		float vertRatio = Screen.height / 768.0f;
		
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(horizRatio, vertRatio, 1.0f));
		
		scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(432.0f), GUILayout.Height(768.0f));
		
		if (GUILayout.Button("Initialize IAP", GUILayout.Width(200))) {
			// the first action is initialize the Oculus IAP.
			OculusIAP.Init("YourPublicKeyBase64");
		}
		
		if (App.PurchaseManager.initialized) {
			if (GUILayout.Button("GetUser", GUILayout.Width(200))) {
				// after that call GetUser() to get the current user. If null, call Login()
				OculusIAP.GetUser();
			}
			
			if (GUILayout.Button("Login", GUILayout.Width(200))) {
				OculusIAP.Login();
			}
			
			if (CurrentUser == null) {
				GUILayout.Label("No User Yet.", GUILayout.Width(200));
			} 
			else {
				GUILayout.Label("User: " + CurrentUser, GUILayout.Width(400));
				
				if (GUILayout.Button("Query Inventory", GUILayout.Width(200))) {
					var moreSkus = new string[] {
						"bird",
						"birdupgrade"
					};
					
					// after the current user is available, call QueryInventory to get the inventory of the user.
					OculusIAP.QueryInventory(true, moreSkus);
				}
				
				if (CurrentInventory == null) {
					GUILayout.Label("Inventory Not Queried Yet.", GUILayout.Width(200));
				} 
				else {
					// now you get the offer details and the entitlements the user has.
					GUILayout.Label("Entitlements you have: ");
					foreach (Entitlement entitlement in CurrentInventory.Entitlements) {
						GUILayout.Label(entitlement.ToString(), GUILayout.Width(400));
					}
					
					foreach (Offer offer in CurrentInventory.Offers) {
						if (GUILayout.Button("Purchase Offer: " + offer.Title, GUILayout.Width(200))) {
							// call PurchaseOffer to launch the purchase flow.
							OculusIAP.PurchaseOffer(offer.OfferId);
						}
					}
					
					if (PurchasedEntitlements != null) {
						// after PurchaseOffer succeeds, you will get a list of the purchased entitlements.
						GUILayout.Label("Purchased Entitlements: ");
						foreach (Entitlement entitlement in PurchasedEntitlements) {
							GUILayout.Label(entitlement.ToString(), GUILayout.Width(400));
						}
					}
					
					foreach (Entitlement entitlement in CurrentInventory.Entitlements) {
						// for a consumable entitlement, you can call ConsumeEntitlement to consume it.
						if (entitlement.IsConsumable) {
							if (GUILayout.Button("Consume Entitlement: " + entitlement.Sku, GUILayout.Width(200))) {
								OculusIAP.ConsumeEntitlement(entitlement.EntitlementId, 1, Guid.NewGuid().ToString());
							}
						}
					}
					
					if (Consumption != null) {
						// finally you will get the consumption record.
						GUILayout.Label("Consumed Entitlement: " + Consumption, GUILayout.Width(400));
					}
				}
			}
		}
		
		GUILayout.EndScrollView();
	}*/


	/*******************************************
	* METHODS
	********************************************/
	private void OnBillingSupported() {
		Debug.Log("OnBillingSupported");
		Debug.LogError ("billing supported");
		App.PurchaseManager.initialized = true;
	}
	
	private void OnBillingNotSupported(string error) {
		Debug.LogError ("billing not supported");
		Debug.Log("OnBillingNotSupported: " + error);
	}
	
	private void OnGetUserSucceeded(User user) {
		Debug.Log("OnGetUserSucceeded: " + user);
		
		App.PurchaseManager.CurrentUser = user;
	}
	
	private void OnGetUserFailed(string error) {
		Debug.Log("OnGetUserFailed: " + error);
	}
	
	private void OnLoginSucceeded(User user) {
		Debug.Log("OnLoginSucceeded: " + user);
		
		App.PurchaseManager.CurrentUser = user;
	}
	
	private void OnLoginFailed(string error) {
		Debug.Log("OnLoginFailed: " + error);
	}
	
	private void OnQueryInventorySucceeded(Inventory inventory) {
		Debug.Log("OnQueryInventorySucceeded: " + inventory);
		
		App.PurchaseManager.CurrentInventory = inventory;
	}
	
	private void OnQueryInventoryFailed(string error) {
		Debug.Log("OnQueryInventoryFailed: " + error);
	}
	
	private void OnPurchaseOfferSucceeded(IList<Entitlement> entitlements) {
		foreach (Entitlement entitlement in entitlements) {
			Debug.Log("OnPurchaseOfferSucceeded: " + entitlement);
		}
		
		App.PurchaseManager.PurchasedEntitlements = entitlements;
	}
	
	private void OnPurchaseOfferFailed(string error) {
		Debug.Log("OnPurchaseOfferFailed: " + error);
	}
	
	private void OnConsumeEntitlementSucceeded(Consumption consumption) {
		Debug.Log("OnConsumeEntitlementSucceeded: " + consumption);
		
		App.PurchaseManager.Consumption = consumption;
	}
	
	private void OnConsumeEntitlementFailed(string error) {
		Debug.Log ("OnConsumeEntitlementFailed: " + error);
	}

	private void Init() {
		OculusIAP.Init("YourPublicKeyBase64");
		if (App.PurchaseManager.initialized) {
			OculusIAP.GetUser ();
			OculusIAP.Login ();
			//OculusIAP.QueryInventory (true, moreSkus);
			//OculusIAP.PurchaseOffer (offer.OfferId);
			//OculusIAP.ConsumeEntitlement (entitlement.EntitlementId, 1, Guid.NewGuid ().ToString ());
		}
	}
#endif
}
