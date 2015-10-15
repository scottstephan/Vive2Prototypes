using UnityEngine;
using UnityEditor;
using System.Collections;

public class generateRotCubes : EditorWindow {

	[MenuItem ("Window/My Window")]
	
	public static void  ShowWindow () {
		EditorWindow.GetWindow(typeof(generateRotCubes));
	}
	
	void OnGUI () {
		// The actual window code goes here
	}
}
