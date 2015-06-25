using UnityEngine;
using System.Collections;
using OculusIAPAndroid;
using OculusIAPAndroid.Model;
using System.Collections.Generic;
using System;

public class OculusIAPController : MonoBehaviour {
#if UNITY_ANDROID

	string _PRIVATEKEY = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAm6tl3H+Jbrs4LzlmFHpJocX1GXyc3YbfsonYx9lxsP2oH4qEoSUgSAjaZx30/57SBwDnBisiGJAn5JdQgqQCez4YLNAag2sLaMv9DZT6lU7gir6yNKslqJIntyTIWStvUDUh5aUnCP+QBlKl2LND3PZjdhIxiPPgQ4+J5Ovm/ml5hAyDt55OXBmegEpqvgF74F+NJrNwDMMDI1WqEK50Yi7pLH0MA8xgGm224dMru3yixj/zYHB+csranqJ1NlTDdcUW/SLIHPeH237r75qc9QAS29xZ/9WrozfqQfnue0qCvz+x5Lz6FMoSajOSJr3CmVcqdm89ulChksBIIKKFQwIDAQAB";

	public static bool Initialized { get; set; }
	
	/// <summary>
	/// Gets or sets the current user.
	/// </summary>
	/// <value>The current user.</value>
	public static User CurrentUser { get; set; }
	
	/// <summary>
	/// Gets or sets the current inventory.
	/// </summary>
	/// <value>The current inventory.</value>
	public static Inventory CurrentInventory { get; set; }
	
	/// <summary>
	/// Gets or sets the purchased entitlements.
	/// </summary>
	/// <value>The purchased entitlements.</value>
	public static IList<Entitlement> PurchasedEntitlements { get; set; }
	
	/// <summary>
	/// Gets or sets the consumption.
	/// </summary>
	/// <value>The consumption.</value>
	public static Consumption Consumption { get; set; }

	static bool _buying;

	public bool disableIAP = false;

	void Awake() {

		_buying = false;
	}
	
	// Use this for initialization
	void Start () {

		if (disableIAP)
			return;

		OculusIAP.Init(_PRIVATEKEY);
	}
	
	public static void FetchUser() {
		OculusIAP.GetUser();
	}

	public static void SetupUserInventory() {

		var moreSkus = new string[]
		{
			"giants01",
			"uh01"
		};
		
		// after the current user is available, call QueryInventory to get the inventory of the user.
		OculusIAP.QueryInventory(true, moreSkus);
	}

	public static void StartLogin() {

		OculusIAP.Login();
	}

	public static void StartPurchase(Offer o) {

		Debug.Log("ABOUT TO PURCHASE ITEM!");

		OculusIAP.PurchaseOffer(o.OfferId);
		_buying = true;
	}

	public static void PurchaseComplete() {
		StoreMenuManager.CompletePurchase();
		_buying = false;
	}

	public static bool buying {get{return(_buying);}}
#endif
}
