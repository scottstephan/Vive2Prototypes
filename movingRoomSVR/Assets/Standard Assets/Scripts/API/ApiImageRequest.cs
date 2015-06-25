using UnityEngine;
using System;
using System.Collections;

public class ApiImageRequest
{

	/*************************************
	 * VARIABLES
 	 *************************************/
	public Action<Texture2D,string> __completed;
	public WWW www = null;
	public bool processing = false;
	
	/*************************************
	 * CONSTRUCTOR
 	 *************************************/
	public ApiImageRequest (string url, Action<Texture2D,string> callback)
	{
		
		www = new WWW (url);
		
		__completed = callback;

		processing = true;				
	}

	/*************************************
	 * METHODS
 	 *************************************/
	public void Complete ()
	{
		
		Texture2D texture = www.error == null ? www.texture : null;
		
		if (__completed != null) {
			__completed (texture, www.error);
		}
	}

	public void Dispose ()
	{

		__completed = null;
		
	}
}
