using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ese
{
	public abstract class AbstractListMemoryCommand<T> : ConsoleCommand where T : Object
	{
        public override void OnCommand(ConsoleContext context)
        {
#if UNITY_EDITOR
            ListMemory();
#else
            Debug.LogError("Profiler is not enabled");
#endif
        }

	    private void ListMemory()
	    {
	        Type objectType = typeof (T);
	        T[] objects = FindObjectsOfType(objectType) as T[];
	        Debug.Log("Listing memory for "+ objects.Length +" " + objectType.Name + "s");
	        int totalSize = 0;
	        if(objects.IsNullOrEmpty())
	        {
	            totalSize = ListArrayObjects(objects, totalSize);
	        }
	        Debug.Log("Total " + objectType.Name + " size: " + totalSize + "bytes");
	    }

	    private static int ListArrayObjects(T[] objects, int totalSize)
        {
/* 3.5
#if UNITY_EDITOR
            int size = 0;
	        foreach (T obj in objects)
	        {
	            size = Profiler.GetRuntimeMemorySize(obj);
	            totalSize += size;
	            Debug.Log(obj.name + " is using: " + size + " bytes");
            }
#else
*/
            Debug.LogError("Profiler is not enabled");
//#endif
            return totalSize;
	    }
	}
}
