using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;

public class ApiManager : MonoBehaviour {

	/*******************************************
	* CONSTS
	********************************************/
	public const string INITIAL_API_CALL = "/Init.json";	
	public const string COOKIE_AUTHTOKEN_PLAYERPREFS = "SESSIONCOOKIE_AUTHTOKEN";
	public const string COOKIE_AUTHTOKEN_WWWRESPONSE = "authtoken";
	
	/*******************************************
	* VARIABLES
	********************************************/
	public List<ApiServerRequest> serverRequests = new List<ApiServerRequest>();
	public List<ApiBundleRequest> bundleRequests = new List<ApiBundleRequest>();
	public List<ApiImageRequest> imageRequests = new List<ApiImageRequest>();
	
//	[HideInInspector] public string ipAddress = null;
	
	// State
	[HideInInspector] public bool initialized = false;

	// update available
	[HideInInspector] public bool updateAvailable = false;
	[HideInInspector] public bool updateRequired = false;

	/*******************************************
	* SERVER TIME VARIABLES
	********************************************/
	
//	private double lastKnownServerTime;
//	private double lastServerTimeUpdate;

/*    public double ServerTime {
        get { return lastKnownServerTime + ( Time.realtimeSinceStartup - lastServerTimeUpdate ); }
    }
	
    public DateTime ServerTimeAsDateTime {
        get { return new System.DateTime( (long)( ServerTime * TimeSpan.TicksPerSecond ), System.DateTimeKind.Utc ); }
    }
    
    public string GetServerTimeString( System.DateTime dateTime ) {
		return dateTime.ToString( @"yyyy-MM-dd\THH:mm:ss.fff\Z" );
	} */
	
	/*******************************************
	* UNITY METHODS
	********************************************/
	public void Awake () {
	
		// Set static singleton reference
		App.ApiManager = this;
	}
	
	public void Start() {

		Init();
	}
	
	public void OnDestroy () {
		serverRequests = null;
	}
	
	public void Update() {

		// API Server Request processing
		if ( serverRequests.Count > 0 ) {
			
			// Loop through and continue processing each request
			int i;
			for ( i=0; i<serverRequests.Count; i++ ) {
					
				if ( serverRequests[ i ].processing && ( serverRequests[ i ].www.isDone ) ) {
					
//					Debug.Log ("Server Request is Done.");
					// Make references
					int stackPosition = i;
					ApiServerRequest request = serverRequests[ i ];
					
					// Stop processing and finish the request
					request.processing = false;
					
					// Server request was successful 
					if ( request.www.error == null ) {
						
						request.Complete( request.www.text, null );
					}
					
					// Server request returned an error
					else {					
						request.Complete( null, request.www.error );
					}
					
					// Throw the request away
					serverRequests.RemoveAt( stackPosition );
					
					request.Dispose();
					
				}
			}
		}

		// Bundle Request processing
		if ( bundleRequests.Count > 0 ) {
			
			// Loop through and continue processing each request
			int bi;
			for ( bi=0; bi<bundleRequests.Count; bi++ ) {
					
				if ( bundleRequests[ bi ].processing && ( bundleRequests[ bi ].www.isDone ) ) {
					
					// Make references
					int bStackPosition = bi;
					ApiBundleRequest bRequest = bundleRequests[ bi ];
					
					// Stop processing and finish the request
					bRequest.processing = false;
					
					// Bundle request was successful 
					if ( bRequest.www.error == null ) {
						
						bRequest.Complete( null );
					}
					
					// All other cases handle errors
					else if ( bRequest.www.assetBundle == null || bRequest.www.assetBundle.mainAsset == null ) {
						bRequest.Complete( "Bundle was either missing or corrupted. " + bRequest.www.url );
					}
					else {					
						bRequest.Complete( bRequest.www.error );
					}
					
					// Throw the request away
					bundleRequests.RemoveAt( bStackPosition );
					
					bRequest.Dispose();
					
				}
			}
		}
		
		// API Image Request processing
		if ( imageRequests.Count > 0 ) {
			
			// Loop through and continue processing each request
			int ii;
			for ( ii=0; ii<imageRequests.Count; ii++ ) {
					
				if ( imageRequests[ ii].processing && ( imageRequests[ ii ].www.isDone ) ) {
					
					// Make references
					int stackPosition = ii;
					ApiImageRequest iRequest = imageRequests[ ii ];
					
					// Stop processing and finish the request
					iRequest.processing = false;
					
					iRequest.Complete();
					
					// Throw the request away
					imageRequests.RemoveAt( stackPosition );
					
					iRequest.Dispose();
					
				}
			}
		}
	}

	/*******************************************
	* HOUSE-KEEPING METHODS
	********************************************/
    void FinishInit( string response ) {
        App.DataManager.api = JsonReader.Deserialize<DataApi>( response );

		initialized = true;
    }

	public void Init() {

		// Log IP Address
/*		string url = "http://api.externalip.net/ip/";
		GET ( url, delegate( string response, string error ) {
			if ( error != null || response.IsNullOrEmpty() ) {
				Debug.LogWarning( "IP Retrieval failed. Probably no network connection." );
				return;
			}
			
			ipAddress = response.Trim();
		});*/
				
        // Preload default settings and urls.
        TextAsset txt = (TextAsset)Resources.Load("JSONData/apiInitResponse", typeof(TextAsset));
        string _info = txt.text;
        if( _info != null ) {
            FinishInit( _info );
        }
        
        // Check for updated settings and/or urls.
/*		GET ( INITIAL_API_CALL, delegate( string response, string error ) {
//            Debug.Log ("init response " + response);
			if ( error != null || response == null ) {
				ThrowError( INITIAL_API_CALL, error );
				return;
			}
			
            FinishInit( response );
		});		*/
	}
	
	/*******************************************
	* API CALL METHODS
	********************************************/
	
	// API Post Call
	public void POST( string url, WWWForm form, Action<string,string> callback ) {
		// If this isn't a full URL, add the FQDN to the beginning of the URL
		string fullURL = url.Substring( 0, 4 ).Contains( "http" ) ? url : BaseApiUrl() + url;
		
		// Add new server request to the stack
		serverRequests.Add(
			new ApiServerRequest( 
				fullURL,
				callback,
				form
			)
		);
	}
	
	// API Get Call
	public void GET( string url, Action<string,string> callback ) {
		string fullURL = url.Substring( 0, 4 ).Contains( "http" ) ? url : BaseApiUrl() + url;
		
		// Add new server request to the stack with no POST form
		serverRequests.Add(
			new ApiServerRequest( 
				fullURL,
				callback,
				null
			)
		);
	}
	
	// Get Bundle
	public void GetBundle( string url, int bundleVersion, Action<AssetBundle,string> callback ) {

		Debug.Log("GET BUNDLE: " + url);

		string bundleUrl;

		if (!url.StartsWith("jar") && !url.StartsWith("file")) {
			if( !url.StartsWith("/") ) {
				Debug.LogWarning("Data integrity problem found and fixed. URL is missing leading '/'. [ " + url + " ]");
				url = "/" + url;
			}

			bundleUrl = BaseAssetUrl() + url;
		}
		else
			bundleUrl = url; //getting local file

		bundleRequests.Add(
			new ApiBundleRequest(
				bundleUrl,
				bundleVersion,
				callback
			)
		);
	}
	
	public void GetLocalBundle( string url, int bundleVersion, Action<AssetBundle,string> callback ) {
		
		string localSphereDir = Application.dataPath;
		//localSphereDir = localSphereDir.Remove(localSphereDir.IndexOf("unity/Assets"));
		url = "file://" + localSphereDir + url;
				
		bundleRequests.Add(
			new ApiBundleRequest(
				/*BaseAssetUrl() +*/ url,
				bundleVersion,
				callback
			)
		);
	}

	// Get Texture Image
	public void GetImage( string url, Action<Texture2D,string> callback ) {
		
		Debug.Log ( "GET IMAGE: " + url );
		
		// Add new server request to the stack with no POST form
		imageRequests.Add(
			new ApiImageRequest( 
				url,
				callback
			)
		);
	}

	
	public void ThrowError( string url, string error ) {
		
		// TODO: User-facing error? probably not
		Debug.LogError( " API ERROR: URL: " + url + "\n\r ERROR: " + ( error != null ? error : "**No Error Provided. Maybe the response was empty?**" ) );
	}

	
	/*******************************************
	* API URL METHODS
	********************************************/
	
	public string BaseApiUrl() {
	
		string serverUrl = "http://sandbox.api.theblu.com";     
		
		switch( App.Manager.API_SERVER ) {
			
			case AppManager.Servers.Sandbox:
				serverUrl = "http://sandbox.api.thebluvr.com";
				break;
			case AppManager.Servers.Dev:
				serverUrl = "http://dev.api.thebluvr.com";
				break;
			case AppManager.Servers.Live:
				serverUrl = "http://api.thebluvr.com";
				break;
		}
		
		return serverUrl;
	}

	public string BaseAssetUrl() {
	
		string serverUrl = "http://assets.thebluvr.com.s3.amazonaws.com";
				
		return serverUrl;
	}
	
	public string BuildUrl( string url, string[] urlParams ) {
		if ( urlParams.Length == 0 )
			return url;
		
		int i=0;
		for ( i=0; i<urlParams.Length; i++ ) {
			int bracketPosition = i + 1;
			url = url.Replace( "{{" + bracketPosition + "}}", urlParams[ i ] );
		}
		
		return url;
	}

	public string GetPlatformUrlString()
	{
		string platform = "standalone";

#if UNITY_ANDROID && !UNITY_EDITOR
		platform = "android";
#endif
		return platform;
	}

	public string GetItemBundleUrl( DataItem item, bool fullUrl=false ) {
		if ( item == null || item.bundle == null )
			return null;

//		if (item.urlkey.Contains("micro"))
//		{
//			int i =0;
//			++i;
//		}

		//Borut
//		return ( fullUrl ? BaseAssetUrl() : "" ) + "/client/unity4_3_4/1/" + GetPlatformUrlString() + "/items/" + item.urlkey +".unity3d";		
		return ( fullUrl ? BaseAssetUrl() : "" ) + "/client/unity4_5_b7/1/" + GetPlatformUrlString() + "/items/" + item.urlkey +".unity3d";		
		//		return ( fullUrl ? BaseAssetUrl() : "" ) + item.bundle.Replace( "{{version}}", item.version.ToString() );	
	}

	public string GetLocalURLFromString(string item) {

#if UNITY_EDITOR
	    return("file://" + Application.streamingAssetsPath + "/" + item + ".unity3d");
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
		return(Application.streamingAssetsPath + "/" + item + ".unity3d");
#endif
		
#if !UNITY_EDITOR && !UNITY_ANDROID
		return null;
#endif
	}

	//we should 
	public string GetLocalURL(DataItem item) {

		if (item == null)
			return null;

#if (UNITY_ANDROID || UNITY_EDITOR)
		return(Application.streamingAssetsPath + "/" + item.urlkey + ".unity3d");
#else
		return null;
#endif
	}
	
	public string GetItemImageUrl( DataItem item, bool fullUrl=false ) {
		if ( item == null || item.image == null )
			return null;
		bool hasSlash = item.image.StartsWith("/");
		return ( fullUrl ? BaseAssetUrl() : "" ) + (hasSlash ? "" : "/") + item.image.Replace( "{{version}}", item.version.ToString() ).Replace( "{{size}}", "140x140" );
	}
	
	
	/*******************************************
	* COOKIE METHODS
	********************************************/
	public string GetCookie() {
		string xDataString = PlayerPrefs.GetString( COOKIE_AUTHTOKEN_PLAYERPREFS );
		
		if ( xDataString == null || xDataString.Trim() == "" )
			return null;
		
		return xDataString;
	}
	
	public void SetCookie( string cookie ) {
		PlayerPrefs.SetString( COOKIE_AUTHTOKEN_PLAYERPREFS, cookie );
		PlayerPrefs.Save();
	}
	
	public Dictionary<string,string> GetCookieForRequest() {
        Dictionary<string,string> headers = new Dictionary<string, string>();
		string cookie = GetCookie();
		
		if ( cookie == null )
			return headers;
		
		headers.Add( "Cookie", cookie );
		
		return headers;
	}
	
	public string GetCookieFromResponse( WWW www ) {
		if ( !www.responseHeaders.ContainsKey( "SET-COOKIE" ) )
			return null;
		
		char[] cookieValSplit = { ';' };
		string[] cookieVal = www.responseHeaders[ "SET-COOKIE" ].Split( cookieValSplit );
        if( cookieVal != null && cookieVal.Length > 0 ) {
            for( int i = 0; i < cookieVal.Length; i++ ) {
                string s = cookieVal[i];
                if ( string.IsNullOrEmpty( s ) || s.Length < 9 ) 
    				continue;
    			if ( s.Substring( 0, 9 ).ToLower().Equals( COOKIE_AUTHTOKEN_WWWRESPONSE ) )
    				return s;
    		}
        }
		
		return null;
	}
}
