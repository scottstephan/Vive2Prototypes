using UnityEngine;
using System.Collections;

public class AtlasMaterial : MonoBehaviour {
	public Rect frame;
	public AtlasReader atlasReader;
	public string atlasName;
	public string spriteName;
	public bool isAtlas;


	// Use this for initialization

	void Start(){
        if( !AppBase.Instance.usingBuiltData ) {
            Setup();
        }
	}

    public void GetData( out float xMin, out float xMax, out float yMin, out float yMax, out float xDiff, out float yDiff ) {
        xMin = 0f; xMax = 0f;
        yMin = 0f; yMax = 0f;
        xDiff = 0f; yDiff = 0f;

        GameObject atlasReaderGO = GameObject.Find(atlasName) as GameObject;
        if( atlasReaderGO == null ) {
            return;
        }

        AtlasReader atlasReader = atlasReaderGO.GetComponent<AtlasReader>();
//        Debug.Log(atlasReaderGO + " " + atlasReader);
        if(atlasReader != null){
            AtlasData data = atlasReader.atlasData;
//            Debug.Log("atlas data " + data );
            if(data.frames == null)
                atlasReader.Reload();
            data = atlasReader.atlasData;
//            Debug.Log("data.frames " + data.frames);
            
            
            if(data != null && data.frames.Count > 0){
                if(data.frames.ContainsKey(spriteName)){
                    AtlasSprite atSp = data.frames[spriteName];
  //                  Debug.Log("atlas sprite " + atSp);
                    if(atSp != null){
                        frame.x = atSp.frame["x"];
                        frame.y = atSp.frame["y"];
                        frame.width = atSp.frame["w"];
                        frame.height = atSp.frame["h"];
                    }
                }
            }
        }

        xMin = 0f;
        yMin = 0f;
        xMax = 0f;
        yMax = 0f;
        xDiff = 0f;
        yDiff = 0f;

        if(GetComponent<Renderer>() != null && atlasReader != null){
            float atlasWidth = GetComponent<Renderer>().sharedMaterial.mainTexture.width;
            float atlasHeight = GetComponent<Renderer>().sharedMaterial.mainTexture.height;
            //Debug.Log(atlasWidth + " " + atlasHeight);
            xMin = frame.xMin / (float)atlasWidth;
            yMin = frame.yMin / (float)atlasHeight;
            xMax = (frame.xMin + frame.width) / (float)atlasWidth;
            yMax = (frame.yMin + frame.height) / (float)atlasHeight;
            xDiff = xMax - xMin;
            yDiff = yMax - yMin;
//            Debug.Log(xMin + " " + xMax + " " + yMin + " " + yMax);
//            Debug.Log( xDiff + " " + yDiff);
        }
    }

	public void Setup () {
		if(isAtlas){
			Debug.LogError("You must undo first, ignoring...");
			return;
		}

		GameObject atlasReaderGO = GameObject.Find(atlasName) as GameObject;
		AtlasReader atlasReader = atlasReaderGO.GetComponent<AtlasReader>();
//		Debug.Log(atlasReaderGO + " " + atlasReader);
		//if(AtlasManager.Instance != null && AtlasManager.Instance.atlasesDict.Count > 0){
			//atlasReader = AtlasManager.Instance.atlasesDict[atlasName];
		if(atlasReader != null){
			AtlasData data = atlasReader.atlasData;
//		    Debug.Log("atlas data " + data );
			if(data.frames == null)
				atlasReader.Reload();
			data = atlasReader.atlasData;
//			Debug.Log("data.frames " + data.frames);


		    if(data != null && data.frames.Count > 0){
				if(data.frames.ContainsKey(spriteName)){
					AtlasSprite atSp = data.frames[spriteName];
//					Debug.Log("atlas sprite " + atSp);
					if(atSp != null){
						frame.x = atSp.frame["x"];
						frame.y = atSp.frame["y"];
						frame.width = atSp.frame["w"];
						frame.height = atSp.frame["h"];
					}
				}
			}
		}

		if(GetComponent<Renderer>() != null && atlasReader != null){
			float atlasWidth = GetComponent<Renderer>().sharedMaterial.mainTexture.width;
			float atlasHeight = GetComponent<Renderer>().sharedMaterial.mainTexture.height;
			//Debug.Log(atlasWidth + " " + atlasHeight);
			float xMin = frame.xMin / (float)atlasWidth;
			float yMin = frame.yMin / (float)atlasHeight;
			float xMax = (frame.xMin + frame.width) / (float)atlasWidth;
			float yMax = (frame.yMin + frame.height) / (float)atlasHeight;
			float xDiff = xMax - xMin;
			float yDiff = yMax - yMin;
//			Debug.Log(xMin + " " + xMax + " " + yMin + " " + yMax);
//			Debug.Log( xDiff + " " + yDiff);

			GetComponent<Renderer>().material.mainTextureScale = new Vector2(xDiff, yDiff);
			GetComponent<Renderer>().material.mainTextureOffset = new Vector2(xMin, 1 - yMax);
		}
	}
}
