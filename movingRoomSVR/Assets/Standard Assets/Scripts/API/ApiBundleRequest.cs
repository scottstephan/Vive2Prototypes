using UnityEngine;
using System;
using System.Collections;

public class ApiBundleRequest
{

	/*************************************
	 * VARIABLES
 	 *************************************/
	public Action<AssetBundle,string> __completed;
	public WWW www = null;
	public bool processing = false;
	
	/*************************************
	 * CONSTRUCTOR
 	 *************************************/
	public ApiBundleRequest (string url, int version, Action<AssetBundle,string> callback, bool pApiRequest=false)
	{
		
		Debug.Log ("Bundle Request [ " + version + " ] : " + url );
		
		www = WWW.LoadFromCacheOrDownload (url, version);
		
		__completed = callback;

		processing = true;		
		
	}

	/*************************************
	 * METHODS
 	 *************************************/
	public void Complete (string wwwError)
	{

		// Some debug logging
		if (wwwError != null)
		{
			Debug.LogError ("API Bundle Request ERROR: " + www.url + " - " + wwwError);
		}
		else
		{
			Debug.Log ("Bundle Loaded : " + www.url );
		}

		
		if (__completed != null) {
			__completed (www.assetBundle, wwwError);
		}
	}

	public void Dispose ()
	{
		__completed = null;

		/*
		if (www != null &&
		    www.assetBundle != null)
		{
			www.assetBundle.Unload (false);
		}
		*/
	}
}
