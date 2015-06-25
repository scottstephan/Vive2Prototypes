using System.Collections.Generic;
using System.Linq;
using Ese;
using UnityEngine;

/// <summary>
/// Utility functions for creating gui objects.
/// </summary>
public static class ESEGUILayout
{
    public static bool Button(string title, string tooltip = "", bool valid = true, params GUILayoutOption[] layoutOptions)
    {
        GUI.SetNextControlName(title);
        if (valid)
        {
            if(GUILayout.Button(new GUIContent(title, tooltip), layoutOptions))
            {
                GUIUtility.keyboardControl = 0;
                return true;
            }
            return false;
        }

        List<GUILayoutOption> list = new List<GUILayoutOption>();
        if (!layoutOptions.IsNullOrEmpty())
        {
            list.AddRange(layoutOptions);
        }
        list.Add(GUILayout.Height(18f));
        list.Add(GUILayout.ExpandWidth(true));
        

        GUILayout.Box(new GUIContent(title, tooltip), list.ToArray());
        return false;
    }
}
