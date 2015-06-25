using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class HashtableData {
	public string name;
	public int maxCount;
}

public class ArrayData {
	public string name;
	public int maxCap;
	public int maxCount;
}

public delegate void MemoryTrackingDelegate();

public class MemoryManager : MonoBehaviour
{
    private static Log log = Log.GetLog(typeof(MemoryManager).FullName);

	private static long memoryHighMark = 0;
	private static System.DateTime lastGarbageCollection;
    public static float garbageCollectionSeconds = 60;
	private static int memoryIncreaseCnt = 0;
	
	private static List<HashtableData> hashTables = new List<HashtableData>();
	private static List<ArrayData> arrays = new List<ArrayData>();
	
	public static MemoryTrackingDelegate MemoryTrackingDelegates = null;
	
	void Start() {
		lastGarbageCollection = System.DateTime.Now;
		StartCoroutine(UpdateMemoryAnalysis());
	}
	
	public static HashtableData FindHashtable( string name ) {
		foreach( HashtableData hd in hashTables ) {
			if( hd.name == name ) {
				return hd;
			}
		}		
		return null;
	}
		
	public static void TrackHashtable( string name, int count ) {	
		HashtableData data = FindHashtable( name );
		if( data != null ) {
			if( count > data.maxCount ) {
				log.Debug("Hashtable [" + data.name + "] has increased in size! [" + data.maxCount + " -> " + count + "]" );			
				data.maxCount = count;
			}
			else {
				log.Debug("Hashtable [" + data.name + "] maxCount remains steady [" + data.maxCount + "]");			
			}
			return;
		}
		
		data = new HashtableData();
		data.name = name;
		data.maxCount = count;
		hashTables.Add(data);
		log.Debug("Hashtable [" + data.name + "][" + data.maxCount + "] has been registered with the Memory Manager" );			
	}
	
	public static void RemoveHashtable( string name ) {
		HashtableData remove_me = FindHashtable( name );
		if( remove_me != null ) {
			remove_me.name = null;
			hashTables.Remove(remove_me);
			hashTables.TrimExcess();
			log.Debug("Hashtable [" + name + "] has been removed from the Memory Manager tracking" );			
		}
	}

	public static ArrayData FindArray( string name ) {
		foreach( ArrayData ad in arrays ) {
			if( ad.name == name ) {
				return ad;
			}
		}
		return null;
	}

	public static void TrackArray( string name, int capacity, int count ) {
		ArrayData data = FindArray( name );
		if( data != null ) {
			// do some array analysis
			if( count > data.maxCount ) {
				log.Debug("Array [" + data.name + "] has increased in size! [" + data.maxCount + " -> " + count + "]" );
				data.maxCount = count;
			}
			else {
				log.Debug("Array [" + data.name + "] maxCount remains steady [" + data.maxCount + "]");			
			}
			return;
		}
		
		data = new ArrayData();
		data.name = name;
		data.maxCap = capacity;
		data.maxCount = count;
		arrays.Add(data);
	}

	public static void RemoveArray( string name ) {
		ArrayData remove_me = FindArray( name );		
		if( remove_me != null ) {
			remove_me.name = null;
			arrays.Remove(remove_me);
			arrays.TrimExcess();
		}		
	}

	private static void LogMemoryStats() {
		log.Debug("=======================");
		log.Debug("!!MEMORY USAGE RISING!!");
		log.Debug("Memory High Mark :: " + memoryHighMark);
		
		// gather all objects
		UnityEngine.Object[] objects = Resources.FindObjectsOfTypeAll( typeof( UnityEngine.Object ) );
		
		log.Debug("Objects : " + objects.Length);
		
		int texture_cnt = 0;
		int material_cnt = 0;
		int go_cnt = 0;
		for( int i = 0; i < objects.Length; i++ ) {
			UnityEngine.Object ob = objects[i];
			if( ob is Texture ) {
				texture_cnt++;
			}
			if( ob is Material ) {
				material_cnt++;
			}
			if( ob is GameObject ) {
				go_cnt++;
			}					
		}
		
		log.Debug("GameObjects : " + go_cnt);
		log.Debug("Materials : " + material_cnt);
		log.Debug("Textures : " + texture_cnt);
		
		if ( MemoryTrackingDelegates != null ) {
			MemoryTrackingDelegates();
		}
		
		log.Debug("Total Tracking hashtables count[" + hashTables.Count + "] capacity[" + hashTables.Capacity + "]");
		log.Debug("Total Tracking arrays count[" + arrays.Count + "] capacity[" + arrays.Capacity + "]");

		log.Debug("END MEMORY TRACKING OUTPUT");
		log.Debug("==========================");
		log.Debug("==========================");
	}

	public static void RunGarbageCollection() {
//        log.Trace("Garbage Collection Activating");
		
		lastGarbageCollection = System.DateTime.Now;
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
		
		// increase in memory usage?
/*		long cur_usage = System.GC.GetTotalMemory(false);
        log.Trace("Garbage Collection :: CurMem[" + cur_usage + "] Max[" + memoryHighMark + "]");
		if( cur_usage > memoryHighMark ) {
			memoryHighMark = cur_usage;
			
			memoryIncreaseCnt++;
			if( memoryIncreaseCnt > 4 ) {
		        log.Warning("Memory usage between garbage collections has increased " + memoryIncreaseCnt + "times. ( "+ memoryHighMark + " )");
			}
			
			// expensive logging.. only run if we are able to output
			if( log.WillLog( Log.LogLevel.DEBUG ) ) {
				LogMemoryStats();
			}
		}*/
		
/*        if( AppBase.Instance != null && !AppBase.Instance.RunningAsPreview() ) {			
			// send some metrics!
			System.TimeSpan diff = lastGarbageCollection.Subtract(TheBluApp.startupDate);
		}*/
	}
	
	IEnumerator UpdateMemoryAnalysis() {
		while( true ) {
			yield return new WaitForSeconds(garbageCollectionSeconds);
			
			// make sure we are running the garbage collection regularly.
			// we still want to give priority to running this during the autotravel when the screen is black
			System.TimeSpan diff = System.DateTime.Now.Subtract(lastGarbageCollection);
			log.Debug("Memory Analysis check :: diff[" + diff.TotalSeconds + "] freq[" + garbageCollectionSeconds + "]");
			if( diff.TotalSeconds >= garbageCollectionSeconds ) {
				RunGarbageCollection();
			}		
		}
	}
}