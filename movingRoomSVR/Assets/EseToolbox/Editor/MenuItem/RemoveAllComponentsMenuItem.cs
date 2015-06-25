using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


class RemoveAllComponentsMenuItem
{
    private const string CreateChildContent = "Remove All Components";
    //private const string CreateChildShortcut = " %#m";
    private const int Priority = 0;

    [MenuItem("GameObject/" + CreateChildContent, true, Priority)]
    [MenuItem("CONTEXT/Transform/" + CreateChildContent, true, Priority)]
    public static bool ValidateRemoveAllComponents(MenuCommand command)
    {
        Transform commandContext = command.context as Transform;
        if (commandContext != null)
        {
            if (GetAllComponents(commandContext).Length > 0)
            {
                return true;
            }
        }
        Transform activeTransform = Selection.activeTransform;
        if (activeTransform != null)
        {
            if (GetAllComponents(activeTransform).Length > 0)
            {
                return true;
            }
        }

        return false;
    }

    private static Component[] GetAllComponents(Transform transform)
    {
        return transform.GetComponents<Component>();
    }

    [MenuItem("GameObject/" + CreateChildContent, false, Priority)]
    [MenuItem("CONTEXT/Transform/" + CreateChildContent, false, Priority)]
    public static void RemoveAllComponents(MenuCommand command)
    {
        Transform commandContext = command.context as Transform;
        if (commandContext != null)
        {
            DestroyAllComponents(commandContext);
        }
        else
        {
            Transform activeTransform = Selection.activeTransform;
            if (activeTransform != null)
            {
                DestroyAllComponents(activeTransform);
            }
        }
    }

    private static void DestroyAllComponents(Transform transform)
    {
        Component[] allComponents = GetAllComponents(transform);
        foreach (Component component in allComponents)
        {
            // Dont remove transform.
            if(component.GetType() != typeof(Transform))
            {
                Object.DestroyImmediate(component);
            }
        }
    }
}
