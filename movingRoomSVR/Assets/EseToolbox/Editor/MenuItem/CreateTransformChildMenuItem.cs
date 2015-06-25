using UnityEditor;
using UnityEngine;

public class CreateTransformChildMenuItem
{
    private const string CreateChildContent = "Create Child";
    private const string CreateChildShortcut = " %#m";
    private const int Priority = 11;

    [MenuItem("GameObject/" + CreateChildContent + CreateChildShortcut, true, Priority)]
    [MenuItem("CONTEXT/Transform/" + CreateChildContent, true, Priority)]
    public static bool ValidateCreateTransformChild(MenuCommand command)
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

    [MenuItem("GameObject/" + CreateChildContent + CreateChildShortcut, false, Priority)]
    [MenuItem("CONTEXT/Transform/" + CreateChildContent, false, Priority)]
    public static void CreateTransformChild(MenuCommand command)
    {
        Transform commandContext = command.context as Transform;
        if (commandContext != null)
        {
            ReferencesTransformInspector.CreateChild(commandContext);
        }
        else
        {
            Transform activeTransform = Selection.activeTransform;
            if (activeTransform != null)
            {
                ReferencesTransformInspector.CreateChild(activeTransform);
            }
        }
    }
}
