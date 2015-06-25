using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using System;
using System.IO;
using System.Text;
using System.Security;
using OculusIAPAndroid;
using OculusIAPAndroid.Model;

public class PlayerInventory : MonoBehaviour {

	public bool noLoad = false;
	public bool allByDefault = true;

	public static string HASH_SALT = "L0YMbSOqQuU0Gj6W3ony";

	static PlayerInventory _singleton;
	static List<InventoryItem> _inventory;

	Dictionary<string, object>_data;

	public string[] defaultInventoryIds;

	void Awake() {

		_singleton = this;
		_inventory = new List<InventoryItem>();
	}

	// Use this for initialization
	void Start () {

		if (noLoad) {
			DefaultInventory();
			SaveUser();
			return;
		}

		if (!LoadUser()) {
			DefaultInventory();
			SaveUser();
		}
	}
	
	void DefaultInventory() {

		_inventory = new List<InventoryItem>();

		//add all 3 oceans
		if (allByDefault)
			AddAllOceans();
		else {

			for (int i = 0; i < defaultInventoryIds.Length; i++) {

				InventoryItem item = StoreManager.GetItemById(defaultInventoryIds[i]);

				if (item != null)
					AddItem(item);
			}
		}
	}

	void AddAllOceans() {

		foreach (InventoryItem i in StoreManager.storeInventory) {

			if (!HasItem(i.Id))
				_inventory.Add(i);
		}
	}
	
	public static void AddItem(InventoryItem item) {

		_inventory.Add(item);
	}

	public static bool DestroyItem(InventoryItem item) {

		foreach (InventoryItem i in _inventory) {
			if (i.Id == item.Id) {
				_inventory.Remove(i);
				return true;
			}
		}

		return false;
	}

	public static bool HasItem(InventoryItem item) {

		if (_inventory == null)
			return false;

		foreach (InventoryItem i in _inventory)
			if (i.Id == item.Id)
				return true;

		return false;
	}

	public static bool HasItem(string id) {

		if (_inventory == null)
			return false;

		foreach (InventoryItem i in _inventory)
			if (i.Id == id)
				return true;
		
		return false;
	}

	public static void SaveUser() {
			
		Hashtable saveDict = new Hashtable();
		Hashtable inventoryDict = _singleton.MakeInventoryDictionary();

		if (inventoryDict != null)	{
			saveDict.Add("inventory", inventoryDict);	
			saveDict.Add("hash", Md5Sum(inventoryDict.ToString()));
		}
		else
			saveDict.Add("hash", "none");
		
		WriteToFile(DataFilename(), Json.Serialize(saveDict));
	}

	public static bool LoadUser() {

		_singleton.DefaultInventory();

		string txtData = ReadFromFile(DataFilename());
		
		if (txtData == null)
			return false; //no inventory
		
		_singleton._data = (Dictionary<string, object>)Json.Deserialize(txtData);

		if (_singleton.MakeInventoryFromDictionary())
			return(_singleton.CheckHash());

		return true; //no inventory, don't check hash
	}

	public bool CheckHash() {

		try {
			string hash = (string)_data["hash"];
			Hashtable inventoryDict = _singleton.MakeInventoryDictionary();

			if (Md5Sum(inventoryDict.ToString()) == hash) {
				return true;
			}
		}
		catch (Exception e)
		{
			Debug.LogError("Save hash check failed." + e);
			return true;
		}

		return false;
	}

	public bool MakeInventoryFromDictionary() {

		try {
			Dictionary<string,object>inventoryItems = (Dictionary<string,object>)_data["inventory"];

			_inventory = new List<InventoryItem>();

			foreach (KeyValuePair<string,object>kvp in inventoryItems) {
				InventoryItem item = StoreManager.GetItemById(kvp.Key);

				if (item != null)
					_inventory.Add(item);
			}
		}
		catch (Exception e) {
			Debug.LogError("No inventory in save file." + e.ToString());
			return false;
		}

		return true;
	}
	
	Hashtable MakeInventoryDictionary() {

		if ((_inventory == null) || (_inventory.Count == 0))
			return null;

		Hashtable inventoryDict = new Hashtable();

		foreach (InventoryItem i in _inventory)
			inventoryDict.Add(i.Id, i.itemName); //this is all we need for now

		return(inventoryDict);
	}

	//old util functions I use in all FLARB stuff
	public static string DataFilename() {
		
		string fileName = SystemInfo.deviceUniqueIdentifier + "_data.txt";
		return(fileName);
	}

	public static string ReadFromFile(string filename)
	{		
		try {
			StreamReader sr = new StreamReader(Application.persistentDataPath + "/" + filename);
			string temp = sr.ReadToEnd();
			sr.Close();
			return(temp);
		}
		catch (Exception e) {
			Debug.LogError("Exception! " + e);
			return null;
		}
	}

	public static void WriteToFile(string filename , string textData)
	{
		try {
			StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/" + filename);
			
			sw.AutoFlush = true;
			sw.Write(textData);
			sw.Close();
		}
		catch (Exception e) {
			Debug.LogError("Exception!" + e);
		}
	}

	public static string Md5Sum(string strToEncrypt)
	{
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		byte[] bytes = ue.GetBytes(strToEncrypt);
		
		// encrypt bytes
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytes);
		
		// Convert the encrypted bytes back to a string (base 16)
		string hashString = "";
		
		for (int i = 0; i < hashBytes.Length; i++)
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		
		return hashString.PadLeft(32, '0');
	}

	public static void AddOculusIAPInventory(Inventory i) {

		//find the inventory items that match the inventory id and add it to our inventory
		foreach (Entitlement entitlement in i.Entitlements) {
			InventoryItem item = StoreManager.GetItemById(entitlement.EntitlementId);

			if (item == null)
				continue;

			_inventory.Add(item);
		}
	}

	public List<InventoryItem>inventory {get{return(_inventory);}}
	
}
