using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StoryBeatDesignGroupAddOn : MonoBehaviour {
	
	public string[] designGroupNames;
    [HideInInspector]
    public int[] designGroupHashes;

	void Start () 
    {
        if (designGroupNames != null &&
            designGroupNames.Length > 0)
        {
            designGroupHashes = new int[designGroupNames.Length];

            for (int i=0; i<designGroupNames.Length; ++i)
            {
                designGroupHashes[i] = designGroupNames[i].ToUpper().GetHashCode();
            }
        }
	}
}
