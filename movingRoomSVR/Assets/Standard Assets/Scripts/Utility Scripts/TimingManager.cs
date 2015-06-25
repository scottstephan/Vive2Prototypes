using UnityEngine;
using System;
using System.Collections;

public class TimingManager : SingletonBehaviour< TimingManager >
{
    static TimingManager()
    {
        IsCreateInstance = true;
    }
    
    public static void setTimeout( float seconds, Action callback )
    {
        Instance.StartCoroutine( Instance._setTimeout( seconds, callback ) );
    }
    
	private IEnumerator _setTimeout( float seconds, Action callback )
	{
		yield return new WaitForSeconds( seconds );
        if ( callback != null )
        {
            callback();
        }
	}

}
