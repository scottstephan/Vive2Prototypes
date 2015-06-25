using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class FlurryAndroidEventListener : MonoBehaviour
{
#if UNITY_ANDROID
	void OnEnable()
	{
		// Listen to all events for illustration purposes
		FlurryAndroidManager.adAvailableForSpaceEvent += adAvailableForSpaceEvent;
		FlurryAndroidManager.adNotAvailableForSpaceEvent += adNotAvailableForSpaceEvent;
		FlurryAndroidManager.onAdClosedEvent += onAdClosedEvent;
		FlurryAndroidManager.onApplicationExitEvent += onApplicationExitEvent;
		FlurryAndroidManager.onRenderFailedEvent += onRenderFailedEvent;
		FlurryAndroidManager.spaceDidFailToReceiveAdEvent += spaceDidFailToReceiveAdEvent;
		FlurryAndroidManager.spaceDidReceiveAdEvent += spaceDidReceiveAdEvent;
		FlurryAndroidManager.onAdClickedEvent += onAdClickedEvent;
		FlurryAndroidManager.onAdOpenedEvent += onAdOpenedEvent;
		FlurryAndroidManager.onVideoCompletedEvent += onVideoCompletedEvent;
	}


	void OnDisable()
	{
		// Remove all event handlers
		FlurryAndroidManager.adAvailableForSpaceEvent -= adAvailableForSpaceEvent;
		FlurryAndroidManager.adNotAvailableForSpaceEvent -= adNotAvailableForSpaceEvent;
		FlurryAndroidManager.onAdClosedEvent -= onAdClosedEvent;
		FlurryAndroidManager.onApplicationExitEvent -= onApplicationExitEvent;
		FlurryAndroidManager.onRenderFailedEvent -= onRenderFailedEvent;
		FlurryAndroidManager.spaceDidFailToReceiveAdEvent -= spaceDidFailToReceiveAdEvent;
		FlurryAndroidManager.spaceDidReceiveAdEvent -= spaceDidReceiveAdEvent;
		FlurryAndroidManager.onAdClickedEvent -= onAdClickedEvent;
		FlurryAndroidManager.onAdOpenedEvent -= onAdOpenedEvent;
		FlurryAndroidManager.onVideoCompletedEvent -= onVideoCompletedEvent;
	}



	void adAvailableForSpaceEvent( string adSpace )
	{
#if UNITY_EDITOR
		Debug.Log( "adAvailableForSpaceEvent: " + adSpace );
#endif
	}


	void adNotAvailableForSpaceEvent( string adSpace )
	{
#if UNITY_EDITOR
        Debug.Log( "adNotAvailableForSpaceEvent: " + adSpace );
#endif
    }


	void onAdClosedEvent( string adSpace )
	{
#if UNITY_EDITOR
        Debug.Log( "onAdClosedEvent: " + adSpace );
#endif
	}


	void onApplicationExitEvent( string adSpace )
	{
#if UNITY_EDITOR
		Debug.Log( "onApplicationExitEvent: " + adSpace );
#endif
	}


	void onRenderFailedEvent( string adSpace )
	{
#if UNITY_EDITOR
        Debug.Log( "onRenderFailedEvent: " + adSpace );
#endif
	}


	void spaceDidFailToReceiveAdEvent( string adSpace )
	{
#if UNITY_EDITOR
		Debug.Log( "spaceDidFailToReceiveAdEvent: " + adSpace );
#endif
	}


	void spaceDidReceiveAdEvent( string adSpace )
	{
#if UNITY_EDITOR
        Debug.Log( "spaceDidReceiveAdEvent: " + adSpace );
#endif
	}


	void onAdClickedEvent( string adSpace )
	{
#if UNITY_EDITOR
        Debug.Log( "onAdClickedEvent: " + adSpace );
#endif
	}


	void onAdOpenedEvent( string adSpace )
	{
#if UNITY_EDITOR
        Debug.Log( "onAdOpenedEvent: " + adSpace );
#endif
	}


	void onVideoCompletedEvent( string adSpace )
	{
#if UNITY_EDITOR
		Debug.Log( "onVideoCompletedEvent: " + adSpace );
#endif
	}

#endif	
}


