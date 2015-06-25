using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
public class SetSceneName : MonoBehaviour 
{
	void Start () 
	{
		TextMesh tm = GetComponent<TextMesh> ();
		if (tm != null)
		{
			tm.text = Application.loadedLevelName;
		}
	}

}
