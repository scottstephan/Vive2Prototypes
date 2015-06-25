/************************************************************************************

Filename    :   StartupSample.cs
Content     :   An example of how to set up your game for fast loading with a
			:	black splash screen, and a small logo screen that triggers an
			:	async main scene load
Created     :   June 27, 2014
Authors     :   Andrew Welch

Copyright   :   Copyright 2014 Oculus VR, Inc. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.1 (the "License"); 
you may not use the Oculus VR Rift SDK except in compliance with the License, 
which is provided at the time of installation or download, or which 
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculusvr.com/licenses/LICENSE-3.1 

Unless required by applicable law or agreed to in writing, the Oculus VR SDK 
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using System.Collections;				// required for Coroutines

public class StartupScene : MonoBehaviour {

	public float				delayBeforeLoad = 0.0f;
	public string				sceneToLoad = "MainScene";

	/// <summary>
	/// Start a delayed scene load
	/// </summary>
	void Start () {
		// all applications should run at 60fps
		Application.targetFrameRate = 60;

		// start the main scene load
		StartCoroutine( DelayedSceneLoad( sceneToLoad ) );
	}
	
	/// <summary>
	/// Asynchronously start the main scene load
	/// </summary>
	IEnumerator DelayedSceneLoad( string sceneName ) {
		// delay one frame to make sure everything has initialized
		yield return 0;

		// this is *ONLY* here for example as our 'main scene' will load too fast
		// remove this for production builds or set the time to 0.0f
		yield return new WaitForSeconds( delayBeforeLoad );

		//*************************
		// load the scene asynchronously.
		// this will allow the player to 
		// continue looking around in your loading screen
		//*************************
		Debug.Log( "Loading: " + sceneName );
		float startTime = Time.realtimeSinceStartup;
		AsyncOperation async = Application.LoadLevelAsync( sceneName );
		yield return async;
		Debug.Log( "Finished loading: " + ( Time.realtimeSinceStartup - startTime ).ToString( "F2" ) + " sec(s)" );
	}

}
