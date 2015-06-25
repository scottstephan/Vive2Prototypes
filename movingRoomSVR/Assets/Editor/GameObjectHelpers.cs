using UnityEditor;
using UnityEngine;
using System.Collections;

public class GameObjectHelpers : Editor {
	
    [MenuItem("WEMOTools/GameObject/Add Child &c")]
    static void MenuAddChild()
    {
        Transform selected = Selection.activeTransform;
		Undo.RecordObject(selected, "Add Child");
        GameObject newChild = new GameObject("_Child");
        newChild.transform.parent = selected;
        Selection.activeTransform = newChild.transform;
        Selection.activeObject = newChild;
    }

    [MenuItem("WEMOTools/GameObject/Add Child &c", true)]
    static bool ValidateMenuAddChild()
    {
        return Selection.activeTransform != null;
    }

    [MenuItem("WEMOTools/GameObject/Add Parent &p")]
    public static void MenuAddParent()
    {
        Transform[] selected = Selection.transforms;
		GameObject newParent = new GameObject("_Parent");
        newParent.transform.position = selected[0].position;
        newParent.transform.parent = selected[0].parent;
		Undo.RecordObject(newParent, "New Object created");
        foreach (Transform selectedTransform in selected)
            selectedTransform.parent = newParent.transform;
        Selection.activeTransform = newParent.transform;
        Selection.activeObject = newParent;
    }

    [MenuItem("WEMOTools/GameObject/Add Parent &p", true)]
    static bool ValidateMenuAddParent()
    {
        return Selection.activeTransform != null;
    }
}
