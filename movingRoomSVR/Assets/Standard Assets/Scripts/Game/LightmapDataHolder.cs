using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WemoLightmapData {
	public Texture2D far;
	public Texture2D near;
}

public class LightmapDataHolder : MonoBehaviour {
	public WemoLightmapData[] lightmapData;
	public LightmapsMode lightmapsMode;
}
