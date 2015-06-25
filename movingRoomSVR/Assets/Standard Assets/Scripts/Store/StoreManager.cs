using UnityEngine;
using System.Collections;
using OculusIAPAndroid;
using OculusIAPAndroid.Model;
using System.Collections.Generic;

public class StoreManager : MonoBehaviour {

//	static StoreManager _singleton;
	static InventoryItem[]	_storeItems;

	void Awake() {

//		_singleton = this;
		MakeDefaultStoreStock();
	}

	void MakeDefaultStoreStock() {

		_storeItems = new InventoryItem[4];

		//we should pull this out of a JSON file or something, but
		//since there's only 3 purchasable items, might as well hardcode it?

		_storeItems[0] = new InventoryItem();
		_storeItems[0].assetName = "huh";
		_storeItems[0].Id = "sandy01";
		_storeItems[0].itemName = "Sandy Bottom";
		_storeItems[0].price = .99f; //this probably comes from the store
		_storeItems[0].storeDescription = "Luxuriate in this beautiful ocean!";
		_storeItems[0].assetBundleName = "sandy";

		_storeItems[1] = new InventoryItem();
		_storeItems[1].assetName = "huh";
		_storeItems[1].Id = "arctic01";
		_storeItems[1].itemName = "Iceberg";
		_storeItems[1].price = .99f; //this probably comes from the store
		_storeItems[1].storeDescription = "Luxuriate in this freezing ocean!";
		_storeItems[1].assetBundleName = "arctic";

		_storeItems[2] = new InventoryItem();
		_storeItems[2].assetName = "huh";
		_storeItems[2].Id = "coral01";
		_storeItems[2].itemName = "Coral Garden";
		_storeItems[2].price = .99f; //this probably comes from the store
		_storeItems[2].storeDescription = "Luxuriate in this coral garden!";
		_storeItems[2].assetBundleName = "coral";

		_storeItems[3] = new InventoryItem();
		_storeItems[3].assetName = "huh";
		_storeItems[3].Id = "giants01";
		_storeItems[3].itemName = "Giants";
		_storeItems[3].price = .99f; //this probably comes from the store
		_storeItems[3].storeDescription = "Luxuriate with a WHALE!!";
		_storeItems[3].assetBundleName = "whale";
	}

	public static InventoryItem[] storeInventory { get{return(_storeItems);}}

	public static InventoryItem GetItemById(string id) {

		foreach (InventoryItem i in _storeItems) {

			if (i.Id == id)
				return (i);
		}

		return null;
	}

    public static InventoryItem GetItemByAssetBundleName(string assetBundleName) {
        
        foreach (InventoryItem i in _storeItems) {
            
            if (i.assetBundleName == assetBundleName)
                return (i);
        }
        
        return null;
    }

	public static void MakeStoreStockFromIAPInventory(Inventory i) {

		Debug.Log("Adding " + i.Offers.Count + " offers to store stock.");

		foreach (Offer offer in i.Offers)
		{
			Debug.Log("OFFER: " + offer.Sku + ": " + offer.Price);

			//we basically have to grab them from the IAP server and fill out our inventory from this....??
			//for now see if we have a matching item in our existing inventory and fill it out....????

			InventoryItem item = GetItemById(offer.Sku);

			if (item == null)
				continue;

			item.oculusOffer = offer;
		}
	}

}
