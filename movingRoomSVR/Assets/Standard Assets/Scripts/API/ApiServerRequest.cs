using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ApiServerRequest {

	/*************************************
	 * VARIABLES
 	 *************************************/
	public Action<string,string> __completed;

	public WWW www = null;
	
	public bool processing = false;
	
	/*************************************
	 * CONSTRUCTOR
 	 *************************************/
	public ApiServerRequest(string url, Action<string,string> callback, WWWForm form=null, bool pApiRequest=false ) {
		
		Debug.Log ("ApiServerRequest :: " + url);
		if ( form != null ) {
			www = new WWW( url, form.data, App.ApiManager.GetCookieForRequest() );
		}
		else {
			www = new WWW( url, null, App.ApiManager.GetCookieForRequest() );
		}
		
		
		__completed = callback;

		processing = true;		
		
	}
	
	/*************************************
	 * METHODS
 	 *************************************/
	public void Complete(string pWWW, string wwwError) {
		
		// Get authtoken cookie, if it exists
		string authtokenCookie = App.ApiManager.GetCookieFromResponse( www );
		string playerprefsCookie = App.ApiManager.GetCookie();

		// Update authtoken cookie if it's out of date or not in player prefs
		if ( authtokenCookie != null && ( playerprefsCookie == null || authtokenCookie != playerprefsCookie ) ) {
			App.ApiManager.SetCookie( authtokenCookie );
		}
		
		if (__completed != null) {
			__completed(pWWW, wwwError);
		}
	}
	public void Dispose() {

		__completed = null;
		
	}

}
