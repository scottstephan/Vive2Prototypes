using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParentData {
	public int parentID;
	public string name;
}

public class ParentDataManager {

	public static List<ParentData> parentData;
	
	public static void Init() {
		parentData = new List<ParentData>();
		
		MemoryUsageLogging();
		MemoryManager.MemoryTrackingDelegates += MemoryUsageLogging;
    }

	static void MemoryUsageLogging() 
	{
		MemoryManager.TrackArray("ParentDataManager.parentData", parentData.Capacity, parentData.Count);
	}
	
	public static int GetParentIDForName( string name ) {
		if( name == null || name.Length <= 0 ) {
			return -1;
		}
		
		foreach( ParentData pd in parentData ) {
			if( pd != null && pd.name.Equals( name ) ) {
				return pd.parentID;
			}
		}
		
		ParentData new_pd = new ParentData();
		new_pd.name = name;
		parentData.Add(new_pd);
		new_pd.parentID = parentData.Count;
		return new_pd.parentID;
	}
}
