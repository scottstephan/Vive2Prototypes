using UnityEngine;
using System.Collections;

[System.Serializable]
public enum PlayerDataType {
    INT,
    FLOAT,
    STRING
}

public class BadgeBase : MonoBehaviour {
    
    [HideInInspector]
    public bool completed = false;

    [HideInInspector]
    public int savedInt = 0;

    [HideInInspector]
    public float savedFloat = 0f;

    [HideInInspector]
    public string savedString = "";

    public PlayerDataType playerDataType = PlayerDataType.INT;

    public virtual string PlayerPrefsString() {
        Debug.LogError("Badge Class has not overrided the PlayerPrefsString function!");
        return "ERROR";
    }
    
    public virtual bool IsCompleted() {
        Debug.LogError("Badge Class has not overrided the PlayerPrefsString function!");
        return false;
    }
    
    // Use this for initialization
	public virtual void Start () {
        if( playerDataType == PlayerDataType.INT ) {
            savedInt = PlayerPrefs.GetInt( PlayerPrefsString() );
        }
	}	
}
