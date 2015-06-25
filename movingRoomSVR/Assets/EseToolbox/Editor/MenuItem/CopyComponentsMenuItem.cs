using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CopyComponentsMenuItem
{
    private const string CopyComponentsContent = "Copy Components";
    private const int Priority = 31;

    public static List<Component> CopiedComponents;

    [MenuItem("GameObject/" + CopyComponentsContent, true, Priority)]
    [MenuItem("CONTEXT/Transform/" + CopyComponentsContent, true, Priority)]
    public static bool ValidateCopy(MenuCommand command)
    {
        Transform commandContext = command.context as Transform;
        if (commandContext != null)
        {
            return true;
        }
        Transform activeTransform = Selection.activeTransform;
        if (activeTransform != null)
        {
            return true;
        }
        return false;
    }

    [MenuItem("GameObject/" + CopyComponentsContent, false, Priority)]
    [MenuItem("CONTEXT/Transform/" + CopyComponentsContent, false, Priority)]
    public static void Copy(MenuCommand command)
    {
        Transform commandContext = command.context as Transform;
        if (commandContext != null)
        {
            CopyComponent(commandContext);
        }
        else
        {
            Transform activeTransform = Selection.activeTransform;
            if (activeTransform != null)
            {
                CopyComponent(activeTransform);
            }
        }
    }

    private static void CopyComponent(Transform activeGameObject)
    {
        CopiedComponents = activeGameObject.GetComponents<Component>().ToList();

        //dont copy transform
        CopiedComponents.Remove(activeGameObject.transform);
    }
}
