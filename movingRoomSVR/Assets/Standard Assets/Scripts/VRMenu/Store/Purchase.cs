using UnityEngine;
using System.Collections;
using OculusIAPAndroid;
using OculusIAPAndroid.Model;

public class Purchase : MonoBehaviour {
#if UNITY_ANDROID
//	static Purchase _singleton;

	void Awake() {

//		_singleton = this;
	}

	public static bool PurchaseItem(InventoryItem item) {

		if (!OculusIAPController.Initialized)
			return false;

		if (item.oculusOffer == null)
			return false;

		OculusIAPController.StartPurchase(item.oculusOffer);

		return true;
	}
#endif
}
