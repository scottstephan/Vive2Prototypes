using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Query session-user specific data (i.e owned spheres, owned variants, owned packages)
public class UserDataQuery {

	/*******************************************
	* METHODS
	********************************************/
	public bool OwnsItem( string ID, int legacyID=-1, string type=null ) {
		
		// Lookup by GUID
		if ( ID != null )
			return App.UserManager.ownedItemIds.Contains( ID );
		
		// Lookup by Legacy Int ID
		if ( legacyID == -1 || type == null )
			return false;
		
		return (
			type == UserManager.USER_ITEM_SPHERE ?
				App.UserManager.LEGACY_ownedSphereIds.Contains( legacyID )
					: App.UserManager.LEGACY_ownedVariantids.Contains( legacyID )
			);
	}
	
	public DataItem GetRandomSphere( bool allow_gallery=true ) {
		
		int count = 0;
        foreach( DataItem sphere_data in App.DataManager.spheres ) {
            if( sphere_data.dateForsale != null ) { // no micro!
				if( App.UserManager.me.prouser 
					|| ( !App.UserManager.me.prouser 
						&& ( sphere_data.legacysphereid == (int)SphereName.Cliff 
							|| sphere_data.legacysphereid == (int)SphereName.CoralGarden 
							|| sphere_data.legacysphereid == (int)SphereName.OuterSeychelles ) ) ) {					
	                count++;
				}
            }
        }
		
        int rnd = UnityEngine.Random.Range(0,count);
        count = 0;
        foreach( DataItem sphere_data in App.DataManager.spheres ) {
            if( sphere_data.dateForsale != null ) { // no micro!
				if( App.UserManager.me.prouser 
					|| ( !App.UserManager.me.prouser 
						&& ( sphere_data.legacysphereid == (int)SphereName.Cliff 
							|| sphere_data.legacysphereid == (int)SphereName.CoralGarden 
							|| sphere_data.legacysphereid == (int)SphereName.OuterSeychelles ) ) ) {
					if( rnd == count ) {
						return sphere_data;
					}
	                count++;
				}
            }
        }
        
		return App.SphereManager.currentSphere;
		// Create reference to owned spheres
/*		List<DataUserItem> ownedSpheres = App.UserManager.ownedSpheres;
		
		// Remove current sphere from list of possible owned spheres to return		
		bool done = false;
		while( !done ) {
			DataUserItem remove_me = null;
			foreach( DataUserItem ownedItem in ownedSpheres ) {
				if ( remove_me == null 
					&& ( ownedItem._id == App.SphereManager.currentSphere._id 
					|| ( !allow_gallery && ownedItem.legacyitemid == SphereManager.GALLERY_LEGACY_ID ) ) ) {
					remove_me = ownedItem;
	//				break;
				}
			}
			if( remove_me != null ) {
				ownedSpheres.Remove( remove_me );
			}
			else {
				done = true;
			}
		}
		
		if( ownedSpheres.Count <= 0 ) {
			return App.SphereManager.currentSphere;
		}
		
		DataUserItem randomOwnedSphere = ownedSpheres[ Random.Range( 0, ownedSpheres.Count ) ];		
		return App.Query.GetSphereById( randomOwnedSphere.itemid );*/
	}
	
    public int LEGACY_GetFirstOwnedSphere( bool allowGallery ) {
        if ( App.UserManager.LEGACY_ownedSphereIds == null )
            return -1;

		foreach ( int sphereId in App.UserManager.LEGACY_ownedSphereIds ) {
            if ( allowGallery || sphereId != SphereManager.GALLERY_LEGACY_ID )
                return sphereId;
        }
        return -1;
    }	
}
