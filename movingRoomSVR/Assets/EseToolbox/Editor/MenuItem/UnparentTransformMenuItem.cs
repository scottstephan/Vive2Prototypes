using UnityEditor;
using UnityEngine;

public class UnparentTransformMenuItem
{
    private const string UnparentContent = "Unparent";
    private const string UnparentShortcut = " %q";
    private const int Priority = 12;

    [MenuItem("GameObject/" + UnparentContent + UnparentShortcut, false, Priority)]
    [MenuItem("CONTEXT/Transform/" + UnparentContent, false, Priority)]
    public static void Unparent(MenuCommand command)
    {
        Transform commandContext = command.context as Transform;
        if (commandContext != null)
        {
            Unparent(commandContext);
        }
        else
        {
            Transform activeTransform = Selection.activeTransform;
            if (activeTransform != null)
            {
                Unparent(activeTransform);
            }
        }
    }

    [MenuItem("GameObject/" + UnparentContent + UnparentShortcut, true, Priority)]
    [MenuItem("CONTEXT/Transform/" + UnparentContent, true, Priority)]
    public static bool ValidateUnparent(MenuCommand command = null)
    {
        if(command != null)
        {
            Transform commandContext = command.context as Transform;
            if (command.context != null && commandContext != null && commandContext.parent != null)
            {
                return true;
            }
        }

        Transform activeTransform = Selection.activeTransform;
        if (activeTransform != null && activeTransform.parent != null)
        {
            return true;
        }
        return false;
    }

    public static void Unparent(Transform targetTransform)
    {
		Undo.RecordObject(targetTransform, UnparentContent);
        targetTransform.parent = null;
    }
}
