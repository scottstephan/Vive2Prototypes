using UnityEngine;
using System;
using System.Collections;

/*******************************************
* USED TO QUERY DATA OBJECTS
********************************************/
public class DataQuery {

	/*******************************************
	* ENUMS
	********************************************/
	public enum ItemTypes {
		fishparent,
		fishvariant,
		sphere,
		audio
	}
	
	public enum ItemSubTypes {
		critter,
		seabed
	}
	
	/*******************************************
	* METHODS
	********************************************/
	public DataItem GetItemById( ItemTypes pType, string pId ) {
		
		// Vars
		int i;
		DataItem[] items = null;
		DataItem returnItem = null;

		if ( pType == ItemTypes.sphere ) {
			items = App.DataManager.spheres;
		}
		else if ( pType == ItemTypes.fishparent ) {
			items = App.DataManager.fishparents;
		}
		else if ( pType == ItemTypes.fishvariant ) {
			items = App.DataManager.fishvariants;
		}
		

		if ( items == null )
			return null;
		
		for( i=0; i<items.Length; i++ ) {
			if ( items[ i ]._id == pId ) {
				returnItem = items[ i ];
				break;
			}
		}
		
		return returnItem;
	}

	public DataItem GetItemByUrlkey( ItemTypes pType, string urlkey ) {
		
		// Vars
		int i;
		DataItem[] items = null;
		DataItem returnItem = null;
		
		if ( pType == ItemTypes.sphere ) {
			items = App.DataManager.spheres;
		}
		else if ( pType == ItemTypes.fishparent ) {
			items = App.DataManager.fishparents;
		}
		else if ( pType == ItemTypes.fishvariant ) {
			items = App.DataManager.fishvariants;
		}
		
		if ( items == null )
			return null;

		for( i=0; i<items.Length; i++ ) {
//			Debug.Log("					debug Adam: urlkey = " + items[i].urlkey);
			if ( items[ i ].urlkey != null && items[ i ].urlkey == urlkey ) {
				returnItem = items[ i ];
				break;
			}
		}
		
		return returnItem;
	}
	
	public DataItem GetItemByLegacyId( ItemTypes pType, int pId ) {
		
		// Vars
		int i;
		DataItem[] items = null;
		DataItem returnItem = null;

		if ( pType == ItemTypes.sphere ) {
			items = App.DataManager.spheres;
		}
		else if ( pType == ItemTypes.fishparent ) {
			items = App.DataManager.fishparents;
		}
		else if ( pType == ItemTypes.fishvariant ) {
			items = App.DataManager.fishvariants;
		}
		

		if ( items == null )
			return null;
		
		for( i=0; i<items.Length; i++ ) {
			if ( items[ i ].legacyid == pId ) {
				returnItem = items[ i ];
				break;
			}
		}
		
		return returnItem;
	}

	public DataItem GetSphereById( string pId ) {
		return GetItemById( ItemTypes.sphere, pId );
	}

	public DataItem GetFishVariantById( string pId ) {
		return GetItemById( ItemTypes.fishvariant, pId );
	}
	
	public DataItem GetSphereByLegacyId( int pId ) {
		return GetItemByLegacyId( ItemTypes.sphere, pId );
	}

	public DataItem GetFishVariantByLegacyId( int pId ) {
		return GetItemByLegacyId( ItemTypes.fishvariant, pId );
	}
}
