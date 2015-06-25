using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.IO;
//using System.Text;
using JsonFx.Json;



[System.Serializable]
public class AtlasSprite{
	//public AtlasRect frame;
	public string name;
	public Dictionary<string, int> frame;
	public bool rotated;
	public bool trimmed;
	
}

[System.Serializable]
public class AtlasData{
	public Dictionary<string, AtlasSprite> frames;
}

[System.Serializable]
public class AtlasReader : MonoBehaviour {
	[SerializeField]
	public string jsonPath;
	public List<AtlasSprite> sprites;
	public AtlasData atlasData;

	// Use this for initialization
	void Awake () {
		sprites = new List<AtlasSprite>();
		Reload();
	}

	public void Reload(){
//		Debug.Log(gameObject.name + " Reload");
		sprites.Clear();
		TextAsset txt = (TextAsset)Resources.Load(jsonPath, typeof(TextAsset));
		string _info = txt.text;
		atlasData = JsonReader.Deserialize<AtlasData>(_info);
		foreach( KeyValuePair<string, AtlasSprite> kvp in atlasData.frames){
			//Debug.Log(kvp.Key );
			AtlasSprite sprite = kvp.Value as AtlasSprite;
			sprite.name = kvp.Key;
			sprites.Add(sprite);
			//Debug.Log(sprite.frame["x"] + " " + sprite.frame["y"]);
		}
	}



}
