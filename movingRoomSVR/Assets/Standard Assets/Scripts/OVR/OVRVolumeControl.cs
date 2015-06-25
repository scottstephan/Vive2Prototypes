/************************************************************************************

Filename    :   OVRVolumeControl.cs
Content     :   Volume controller interface. 
				This script is used to display a popup UI dialog when the volume is changed.
				Override with a subclass and replace the OVRVolumeController prefab.
Created     :   September 24, 2014
Authors     :   Jim Dose

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.

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
using System.Collections;

// WEMOLAB BEGIN
#if UNITY_ANDROID
// WEMOLAB END
public class OVRVolumeControl : MonoBehaviour {
	private const float 		ShowPopupTime = 3;
	private const float			PopupDepth = 1.5f;
	private const int 			MaxVolume = 15;
	private const int 			NumVolumeImages = MaxVolume + 1;
	
	private Transform			MyTransform = null;
	
    private Vector3 ogLocalScale = Vector3.one;

	// Use this for initialization
    void Awake() {
// WEMOLAB BEGIN :: Moved the below from Start.
        DontDestroyOnLoad( gameObject );
        MyTransform = transform;
        ogLocalScale = MyTransform.localScale;
        GetComponent<Renderer>().enabled = false;
// WEMOLAB END :: Moved the below from Start.
    }
	void Start () {
// WEMOLAB BEGIN :: Moved this all into Awake..
// WEMOLAB END
    }

	// Updates the position of the volume popup
	// Should be called by the current camera controller in LateUpdate
// WEMOLAB BEGIN :: Need player scale from the camera.
    public virtual void UpdatePosition ( Quaternion cameraRot, Vector3 cameraPosition, float p_scale )
//  public virtual void UpdatePosition ( Quaternion cameraRot, Vector3 cameraPosition )
// WEMOLAB END :: Need player scale from the camera.
    {
// WEMOLAB BEGIN :: Check for nulls
        if( GetComponent<Renderer>() == null || MyTransform == null ) {
            return;
        }
// WEMOLAB END
		// OVRDevice.GetTimeSinceLastVolumeChange() will return -1 if the volume listener hasn't initialized yet,
		// which sometimes takes place after a frame has run in Unity.
		double timeSinceLastVolumeChange = OVRDevice.GetTimeSinceLastVolumeChange();
		if ( ( timeSinceLastVolumeChange != -1 ) && ( timeSinceLastVolumeChange < ShowPopupTime ) )
		{
			GetComponent<Renderer>().enabled = true;
			GetComponent<Renderer>().material.mainTextureOffset = new Vector2( 0, ( float )( MaxVolume - OVRDevice.GetVolume() ) / ( float )NumVolumeImages );
			if ( MyTransform != null )
			{
				// place in front of camera on the horizon with no pitch or roll
				Vector3	ang = cameraRot.eulerAngles;
				Quaternion rot = Quaternion.Euler( 0, ang.y, 0 );
				MyTransform.forward = rot * Vector3.forward;
// WEMOLAB BEGIN .. positioning needs to take our player scale into account.
				MyTransform.position = cameraPosition + ( MyTransform.forward * PopupDepth * p_scale );
                MyTransform.localScale = ogLocalScale * p_scale;
// WEMOLAB END
			}
		}
		else
		{
			GetComponent<Renderer>().enabled = false;
		}
	}
}
// WEMOLAB BEGIN
#endif
// WEMOLAB END

