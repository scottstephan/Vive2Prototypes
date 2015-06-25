using UnityEngine;
using System.Collections;

public class OculusInfo  {
	
	
	public static int sensorCount=1;
	
	public static bool isUsingOculus {
		get{
			return sensorCount > 0;
		}	
	}
}
