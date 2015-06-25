/*
 * This wizard will replace a selection with an object or prefab.
 * Scene objects will be cloned (destroying their prefab links).
 */
using UnityEngine;
using UnityEditor;
using System.Collections;

public class ReplaceMaterial : ScriptableWizard
{
	static Material replacementMat = null;


	public Material ReplacementMaterial = null;

    [MenuItem("WEMOTools/Replace Material...")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard(
            "Replace Material", typeof(ReplaceMaterial), "Replace");
    }

    public ReplaceMaterial()
    {
		ReplacementMaterial = replacementMat;
    }

    void OnWizardUpdate()
    {
		replacementMat = ReplacementMaterial;
    }

    void OnWizardCreate()
    {
        if (replacementMat == null)
            return;

		Undo.RecordObject(this, "Replace Material");
        
        Transform[] transforms = Selection.GetTransforms(
            SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);

        foreach (Transform t in transforms)
        {
			Renderer[] renders = t.GetComponentsInChildren<Renderer>();
			foreach(Renderer r in renders){
				r.material = replacementMat;
			}

        }

    }
}