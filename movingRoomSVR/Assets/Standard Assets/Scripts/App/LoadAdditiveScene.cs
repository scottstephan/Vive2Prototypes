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

public class LoadAdditiveScene : MonoBehaviour {

	public float				delayBeforeLoad = 0.0f;
	public string				sceneToLoad = "vr-boot";

    static bool bLoaded;

	/// <summary>
	/// Start a delayed scene load
	/// </summary>
	void Start () {
		// start the main scene load
		StartCoroutine( DelayedSceneLoad( sceneToLoad ) );
	}
	
	/// <summary>
	/// Asynchronously start the main scene load
	/// </summary>
	IEnumerator DelayedSceneLoad( string sceneName ) {
		// delay two frames to make sure everything has initialized
		yield return null;

		// this is *ONLY* here for example as our 'main scene' will load too fast
    	// remove this for production builds or set the time to 0.0f
//	    yield return new WaitForSeconds( delayBeforeLoad );

		//*************************
		// load the scene asynchronously.
		// this will allow the player to 
		// continue looking around in your loading screen
		//*************************
//		Debug.Log( "Loading: " + sceneName );
//		float startTime = Time.realtimeSinceStartup;

        bLoaded = false;

        StartCoroutine(DelayedFade());
//        yield return new WaitForSeconds(3f);

//        AsyncOperation async = Application.LoadLevelAdditiveAsync( 1 );
        AsyncOperation async = Application.LoadLevelAdditiveAsync( sceneName );
		yield return async;

        CameraManager.singleton.BootLoad();
        bLoaded = true;
	}

    IEnumerator DelayedFade()
    {
        // load time of vr boot min was 2.3 s
        yield return new WaitForSeconds(1.0f);

#if UNITY_EDITOR
        if (bLoaded)
        {
			CameraManager.singleton.FinishBootLoad();
            yield break;
        }
#endif
        OculusCameraFadeManager.StartCameraFadeToBlack(0.01f, null, null);

        while (!bLoaded)
        {
            yield return null;
        }

		yield return new WaitForSeconds(0.1f);

		while( !App.AppReady() ) {
            yield return null;
        }

		CameraManager.singleton.FinishBootLoad();
		Shader.WarmupAllShaders();
	
		App.MetricsManager.Track ("loading_sequence");

        yield return null;

		OculusCameraFadeManager.StartCameraFadeFromBlack(0.3f, null, null);
    }


}
