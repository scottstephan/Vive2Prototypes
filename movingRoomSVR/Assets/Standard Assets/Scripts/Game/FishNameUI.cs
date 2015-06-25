using UnityEngine;
using System;
using System.Collections.Generic;

public class FishNameUI : MonoBehaviour
{
	public GameObject nameObject;

    TextMesh nameText;
    TextOutline nameOutline;

//	private float distance = 75f;
	
    void Awake() {
        if( nameObject != null ) 
        {
            nameText = nameObject.GetComponent<TextMesh>();
            nameOutline = nameObject.GetComponent<TextOutline>();
        }
    }

    public void SetName( string new_name ) {
        if( nameText != null ) {
            nameText.text = new_name;

            if (nameOutline != null)
            {
                nameOutline.UpdateText();
            }
        }

        // TODO> adjust size of background.
    }
}

