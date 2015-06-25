//********************************************
//*	GENERAL EDTIOR UTILS
//********************************************
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//********************************************
//*	CLASS
//********************************************
public class CoreUtils {

	//********************************************
	//*	STRUCTS
	//********************************************

	//********************************************
	//*	VARIABLES
	//********************************************
	
	//********************************************
	//*	MAIN METHODS
	//********************************************
	[MenuItem("WEMOTools/Utils/Clear Preferences")]
    public static void PlayerPrefs_Clear() {
		
        //*** Clear Player prefs
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("WEMOTools/Utils/Preference Edu Mode ON")]
    public static void PlayerPrefs_EduModeOn() {
        PlayerPrefs.SetInt(UserManager.USER_EDUCATION_MODE, 1);
    }

    [MenuItem("WEMOTools/Utils/Preference Edu Mode OFF")]
    public static void PlayerPrefs_EduModeOff() {
        PlayerPrefs.SetInt(UserManager.USER_EDUCATION_MODE, 0);
    }
}
