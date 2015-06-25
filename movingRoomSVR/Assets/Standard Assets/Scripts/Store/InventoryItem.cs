using OculusIAPAndroid;
using OculusIAPAndroid.Model;

public class InventoryItem {

	public string 	Id;						//eventually from a datbase?
	public string 	itemName;				//the name as shown in the store
	public string 	assetName;				//the name of the asset (the icon or whatever?)
	public string 	storeDescription;		//the text for the store
	public float	price;					//item price in real dollars ... ?
	public string 	assetBundleName;		//file name of asset bundle...!

	public Offer	oculusOffer;			//this is the offer data from oculus
}
