using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DataItem {
	
	public string _id = null;
	public string parentid = null;
	public string parentsphereid = null;
	public string basetype = null; //sphere, fishparent, fishvariant, audio
	public string subtype = null; // critter, seabed
	
	public int legacyid = 0; // this can be sphereid, itemid, or itemvariantid
	public int legacyitemid = 0; // for itemvariants
	public int legacysphereid = 0; // for itemvariants and items
	public int legacystartingsetid = 0; // for itemvariants
	public int legacycauseid = 0; // for itemvariants
	public string legacytype = null; // for itemvariants
	public string legacydescription = null; // for itemvariants and items
	public string legacymetadata = null; 
	
	public string urlkey = null;
	public string name = null;
	public string description = null;
	public string image = null;
	public string bundle = null;
	public string audiobundle = null;
	public int version = 1;
	public int price = 0;
	public int limitedcount = 0;
	public bool limitededition = false;
	public bool trueblu = false;
	
	public float weight = 1f;
	public int minspawncnt = 1;
	public int maxspawncnt = 1;
	public int purchasespawncnt = 3;
	public bool mustpurchasetospawn = false;
	
	public string geolocation = null;
    
	public int lowspecminpop = 15;
    public int lowspecmaxpop = 18;
    public int medspecminpop = 25;
    public int medspecmaxpop = 28;
    public int highspecminpop = 35;
    public int highspecmaxpop = 38;
    public int geolocpop = 1;
    public int roamerpercentage = 25;
    public int ownednativepercent = 1;
    public int ownednonnativepercent = 0;
    public int latitude = 0;
    public int longitude = 0;
    public bool startingsphere = false;
    public int companionsphereid = 1;
    public bool geograntable = false;
    public bool granttoeveryone = false;

	public string dateForsale = null;
	public string datePublished = null;
	public string dateCreated = null;
	
	
	// System params. These params don't come in from the web
	// Some are configured after the item data is retrieved from API
	// Some are constantly changing based on state management
	
	// SPHERE SYSTEM PARAMS
	public bool NoVisitors = false;
	public float NonNativePercentage;
	public float OwnedNativePercentage;
	public float OwnedNonNativePercentage;
	public int CurrentMaxPop;
    public int CurrentMaxNativePop;
    public int CurrentMaxNonNativePop;
    public int CurrentMaxNativeVisitorsPop;
    public int CurrentMaxNonNativeVisitorsPop;
    public int SpecMinPop;
    public int SpecMaxPop;
    public int TotalVariantWeight = 0;
	
	// FISH SYSTEM PARAMS
	public float Percentage;
	public int WeightSatisfiedCount;
	public int CurrentWeightCount;
	public bool WeightWasSatisfied;	
	public bool ForSale = false;
	public bool ObjectRequested;
	public SpeciesSize SpeciesSize;
	public int FakeClick;
	public int ClickGoal;
	public int NumClicks;
	
	// ITEM-AGNOSTIC SYSTEM PARAMS
    public Object MainAsset;
    public AssetBundle MainAssetBundle;
}
