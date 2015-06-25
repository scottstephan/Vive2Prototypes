using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AtlasManager : MonoBehaviour {
	private static AtlasManager singleton;
	public static AtlasManager Instance{
		get{ return singleton;}
	}

	public AtlasReader[] atlases;
	public Dictionary<string, AtlasReader> atlasesDict;

	void Awake(){
		singleton = this;
		atlasesDict = new Dictionary<string, AtlasReader>();
        for (int i=0; i<atlases.Length; ++i)
        {
            AtlasReader at = atlases[i];
            if (at == null)
            {
                continue;
            }

			atlasesDict.Add(at.name, at);
		}
	}



}
