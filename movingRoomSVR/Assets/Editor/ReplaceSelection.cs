/*
 * This wizard will replace a selection with an object or prefab.
 * Scene objects will be cloned (destroying their prefab links).
 */
using UnityEngine;
using UnityEditor;
using System.Collections;

public class ReplaceSelection : ScriptableWizard
{
    static GameObject replacement = null;
    static bool rotation = true;
    static bool keep = false;
    static Vector3 roffset = Vector3.zero;

    public GameObject ReplacementObject = null;
    public bool ApplyRotation = true;
    public bool KeepOriginals = false;
    public Vector3 OffsetRotation = Vector3.zero;

    [MenuItem("WEMOTools/Replace Selection...")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard(
            "Replace Selection", typeof(ReplaceSelection), "Replace");
    }

    public ReplaceSelection()
    {
        ReplacementObject = replacement;
        ApplyRotation = rotation;
        KeepOriginals = keep;
        OffsetRotation = roffset;
    }

    void OnWizardUpdate()
    {
        replacement = ReplacementObject;
        rotation = ApplyRotation;
        keep = KeepOriginals;
        roffset = OffsetRotation;
    }

    void OnWizardCreate()
    {
        if (replacement == null)
            return;

		Undo.RecordObject(this, "Replace Selection");
        
        Transform[] transforms = Selection.GetTransforms(
            SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);

        foreach (Transform t in transforms)
        {
            GameObject g;
            PrefabType pref = PrefabUtility.GetPrefabType(replacement);

            if (pref == PrefabType.Prefab || pref == PrefabType.ModelPrefab)
            {
                g = (GameObject)PrefabUtility.InstantiatePrefab(replacement);
            }
            else
            {
                g = (GameObject)Editor.Instantiate(replacement);
            }

            g.name = replacement.name;
            g.transform.position = t.position;
            g.transform.localScale = t.localScale;
			g.transform.parent = t.parent;
            if (rotation)
            {
                g.transform.rotation = t.rotation * Quaternion.Euler(roffset);
            }
        }

        if (!keep)
        {
            foreach (GameObject g in Selection.gameObjects)
            {
                GameObject.DestroyImmediate(g);
            }
        }
    }
}