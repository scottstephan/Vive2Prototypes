using UnityEngine;
using System;
using System.Collections;

[System.Serializable]
public class DataUserItem {
	
	public string _id = null;
	public string itemid = null;
	public string userid = null;
	public string type = "fish";
	public int legacyid = 0;
	public int legacyitemid = 0;
	public int legacyuserid = 0;
	public int legacyfromuserid = 0;
	public int legacyinviterewardid = 0;
	public string name = "Blu Fish";
	public string birthplace = null;
	
	public string datePurchased = null;
	public string dateOwnedMoment = null;
	public string dateLastRoamed = null;
	public string dateCreated = null;
	public string dateUpdated = null;
	
	// System generated params
	public bool Shared = false;
	public bool InScene = false;
	public bool WaitingToName = false;
	public bool NewlyAquired = false;
	public bool AddedPurchaseAmtToSphereTotal = false;
	public float LastInSphereTime = 0f;
	public DateTime LastRoamedDateTime;
	public DataItem ItemData = null;
}