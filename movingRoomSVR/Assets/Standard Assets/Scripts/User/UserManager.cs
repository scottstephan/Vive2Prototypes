using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;

public class UserManager : MonoBehaviour {

	/*******************************************
	* CONSTANTS
	********************************************/
	public const string USER_ITEM_SPHERE = "sphere";
	public const string USER_ITEM_FISH = "fishvariant";
	
	// Namespaces for data that's saved in player prefs
	public const string USER_DATA = "USERSERVERDATA";
	public const string USER_AUDIO_MUTE = "USER_AUDIO_MUTE_SETTING";
	public const string USER_DISPLAY_NAMES = "USER_DISPLAY_NAME_SETTING";
    public const string USER_EDUCATION_MODE = "USER_EDUCATION_MODE";
	[HideInInspector]public string CookieAuthtoken = null;
	
	/*******************************************
	* VARIABLES
	********************************************/
	
    public int educationalMode = 0; // default off! currently tour mode only!

    // Used to query user data (owned fish, spheres, etc)
	public UserDataQuery query = new UserDataQuery();
	
	// Currently logged in user (if any)
	private DataUser _me = null;
	[HideInInspector]public DataUser me {
		
		set{
			_me = value;
			if(_me != null )//&& _me.dateCreated != null)
				//if( System.DateTime.Parse(_me.dateCreated) < System.DateTime.Parse("2008-10-01T00:00:00-5:00") )
					// any current user is pro
					_me.prouser = true;
		}
		
		get{
			return _me;
		}
	}
	
	// ALL of the current user's owned items
	[HideInInspector]public DataUserItem[] ownedItems;
	
	[HideInInspector]public List<DataUserItem> ownedSpheres = new List<DataUserItem>();
	[HideInInspector]public List<DataUserItem> ownedFish = new List<DataUserItem>();

	// Simple list of all IDs of owned Items (for quick lookup)
	[HideInInspector]public List<string> ownedItemIds = new List<string>();
	[HideInInspector]public List<int> LEGACY_ownedSphereIds = new List<int>();
	[HideInInspector]public List<int> LEGACY_ownedVariantids = new List<int>();
	
	// State
//	[HideInInspector]private bool gotOwnedItems = false;
//	[HideInInspector]private bool gotUserData = false;
	[HideInInspector]public bool initialized = false;
	
	// Callbacks
	public event Action __loggedIn = null;
	public event Action __userDataReady = null;

	/*******************************************
	* UNITY METHODS
	********************************************/
	public void Awake () {
	
		// Set static singleton reference
		App.UserManager = this;
	}
	
	void Start() {
        educationalMode = PlayerPrefs.GetInt(UserManager.USER_EDUCATION_MODE, 0);
//        educationalMode = PlayerPrefs.GetInt(UserManager.USER_EDUCATION_MODE, 1);


        // TODO: Fix this shit. I don't know why but DataUser obj is never null off the bat.  Probably an editor meta issue
        me = null;

        // load default user.
        TextAsset txt = (TextAsset)Resources.Load("JSONData/defaultUserResponse", typeof(TextAsset));
        string _info = txt.text;
        if( _info != null ) {
            me = JsonReader.Deserialize<DataUser>( _info );            
            PostAuthenticationSetup();
//            Debug.Log("Loaded default user. Bailing further user work for now.");
        }
       		
		// Get the current user
/*		GetCurrentUser( false, delegate( string error ) {
			
			// If user isn't logged in, then show the login form
			if ( error != null ) {
                if( App.UIManager == null ) {
                    // login override for oculus rift
                    App.UserManager.Login( "max@fish.com", "koolio", delegate( string derror ) { 
                        
                        if ( derror != null ) {
                            Debug.Log("error logging in: " + derror);
                            return;
                        }
                    });
                }
                else {
//				Debug.Log ("Opening the Signup.");
				//Login("max@fish.com", "koolio", null);
    			    App.UIManager.signin.Open();
                }
				return;
			}
			
//			Debug.Log ("Getting the user again??");
			// Get user data again
			GetCurrentUser( true );		
		});*/
	}
	
	public void OnDestroy () {
	}
	
	/*******************************************
	* METHODS
	********************************************/
	private void CheckInitializationStatus() {
		// blu2.0 has no initialization steps.
		
		initialized = true;
		if ( initialized ) {
			if ( __userDataReady != null ) {
				__userDataReady();
			}
		}
	}
	
	public void BasicUserLogin() {
		
		// create empty user and log in		
		if (me == null)
			me = new DataUser();
		me.SetToFreeUser();
		
		
		PostAuthenticationSetup();
		
		if ( __loggedIn != null ) {
			__loggedIn();
		}
	}
	
	public void GetCurrentUser( bool force, Action<string> callback=null ) {
		
/*		// LESSON #1: If we already have user info, don't try to get it again
		if ( me != null && me._id != null && !force && initialized ) {
//			Debug.Log ("Bailing cause we already have a user!");
			if ( callback != null ) callback( null );
			return;
		}
		
		App.ApiManager.GET( App.Urls.user.current, delegate( string response, string error ) {
			
//            Debug.Log ("current user response " + response);

			// If user isn't logged in - no need to continue
			if ( error != null ) {
				Debug.Log ("Get User Errored " + error);
				if ( callback != null ) callback( error );
				return;
			}

			// Save user data to our "me" instance
			me = JsonReader.Deserialize<DataUser>( response );
			PostAuthenticationSetup();
			
//			Debug.Log ("Get User Succeeded");
			if ( callback != null ) callback( null );
		});*/
	}

	public void Login( string email, string password, Action<string> callback ) {

/*		WWWForm loginParams = new WWWForm();
		loginParams.AddField( "email", email );
		loginParams.AddField( "password", password );
		
		App.ApiManager.POST ( App.Urls.user.login, loginParams, delegate( string response, string error ) {
			if ( error != null ) {
				if ( callback != null ) callback( error );
				return;
			}
			
			if ( response == null ) {
				if ( callback != null ) callback( "No Response" );
				return;
			}
			
			me = JsonReader.Deserialize<DataUser>( response );

			PostAuthenticationSetup();
			
			if ( callback != null ) callback( null );
			
			if ( __loggedIn != null ) {
				__loggedIn();
			}
		});*/
	}
	
	public void PostAuthenticationSetup() {
		
		CheckInitializationStatus();
		
		// Get owned items 
/*		GetOwnedItems( delegate() {
			gotOwnedItems = true;
			CheckInitializationStatus();
		});

		// Get the user's avatar
		if ( me.avatar != null ) {
			App.ApiManager.GetImage( me.avatar, delegate( Texture2D avatar, string error ) {
				if ( error != null || avatar == null ) 
					return;
				
				gotUserData = true;
				CheckInitializationStatus();
				
				me.AvatarImage = avatar;
				
				// Set Menu params to be user-specific
				App.UIManager.menu.buttonUser.Set( me.nickname, (Texture)me.AvatarImage );
			});
		}*/
	}
		
	public void GetOwnedItems( Action callback ) {
/*		App.ApiManager.GET( App.Urls.user.ownedItems, delegate( string response, string error ) {
			if ( error != null ) {
				App.ApiManager.ThrowError( App.Urls.user.ownedItems, error );
				return;
			}
			
			// Save owned items to instance
			ownedItems = JsonReader.Deserialize<DataUserItem[]>( response );
			
			// Add owned data to lists
			AggregateOwnedItems( ownedItems );

			if ( callback != null )
				callback();
		});*/
	}
	
	private void AggregateOwnedItems( DataUserItem[] allOwnedItems ) {

		foreach( DataUserItem userItem in allOwnedItems ) {
			
			// Add guids to ownedItems list, then add to ownedSpheres and ownedVariant lists
			ownedItemIds.Add( userItem._id );

			// Only one of each sphere can be owned
			if ( userItem.type == USER_ITEM_SPHERE ) {
				
				if ( !ownedSpheres.Contains( userItem ) )
					ownedSpheres.Add( userItem );
	
				if ( !LEGACY_ownedSphereIds.Contains( userItem.legacyitemid ) )
					LEGACY_ownedSphereIds.Add ( userItem.legacyitemid ); // this is the sphereid
			}
			
			// Multiple variants can be owned
			else if ( userItem.type == USER_ITEM_FISH ) {
				ownedFish.Add( userItem );
				LEGACY_ownedVariantids.Add ( userItem.legacyid ); // this is the userownedvariantid
			}
		}
		
//		gotOwnedItems = true;
	}

	public void GetUserByID( string ID, Action<DataUser,string> callback ) {
/*		// Check to see if we already have this user
		DataUser cachedUser = App.DataManager.GetCachedUserByID( ID );
		if ( cachedUser != null ) {
			if ( callback != null ) callback( cachedUser, null );
			return;
		}
		
		App.ApiManager.GET( 
			App.ApiManager.BuildUrl( App.Urls.user.byID, new string[]{ ID } ),
			delegate( string rawUser, string error ) {
				if ( error != null || rawUser == null ) {
					if ( callback != null ) callback( null, error == null ? "No user found" : error );
					return;
				}
			
				DataUser responseUser = JsonReader.Deserialize<DataUser>( rawUser );

				if ( responseUser == null ) {
					if ( callback != null ) callback( null, "No user found" );
					return;
				}
			
				if ( callback != null ) callback( responseUser, null );
			
				App.DataManager.otherUsers.Add ( responseUser );
			}
		);*/
	}
	
	//TODO: get rid of this method completely
	public void GetUserByLegacyID( int ID, Action<DataUser> callback=null ) {
		// Check to see if we already have this user
/*		DataUser cachedUser = App.DataManager.GetCachedUserByLegacyID( ID );
		if ( cachedUser != null ) {
			if ( callback != null ) callback( cachedUser );
			return;
		}
		
		App.ApiManager.GET( 
			
			App.ApiManager.BuildUrl( App.Urls.user.byLegacyID, new string[]{ ID.ToString() } ),
			
			delegate( string rawUser, string error ) {
				if ( error != null || rawUser == null ) {
					if ( callback != null ) callback( null );
					return;
				}
			
				DataUser responseUser = JsonReader.Deserialize<DataUser>( rawUser );

				if ( responseUser == null ) {
					if ( callback != null ) callback( null );
					return;
				}
			
				if ( callback != null ) callback( responseUser );
			
				App.DataManager.otherUsers.Add ( responseUser );
			}
		);*/
	}

}
