using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Ese;
using UnityEngine;
using UnityEditor;
using EventType = UnityEngine.EventType;
using Input = Ese.Input;
using Object = UnityEngine.Object;

/// <summary>
/// Author: 
///     Skjalg S. Mæhre
/// 
/// Company: 
///     Exit Strategy Entertainment
/// 
/// Website: 
///     http://www.exitstrategyentertainment.com/
/// 
/// Date:
///     4th of August, 2011
/// 
/// Purpose:
///     This inspector provides some often used buttons and shows you all the other 
///     gameobjects in the scene that references this gameobject, transform or any MonoBehaviour on the gameobject with a script.
/// 
/// Why:
///     Games often use intricate logical features where this gameobject triggers that 
///     gameobject that triggers a third gameobject, and this tool provides an 
///     easy way to follow a chain of referencing. It also provides you with some 
///     handy buttons that make your life a little easier.
/// </summary>
[CustomEditor(typeof(Transform))]
public class ReferencesTransformInspector : Editor
{
    private const string UndoTransformChangeContent = "Transform Change";
    private const string ShowReferencesContent = "Show References ({0})";
    private const string RefreshReferencesContent = "Refresh References";
    private const string ReferenceContent = "Reference {0}";
    private const string ShowButtonsContent = "Show Options";
    private const string ResetPositionContent = "Reset Position";
    private const string ResetRotationContent = "Reset Rotation";
    private const string ResetScaleContent = "Reset Scale";
    private const string CreateChildContent = "Create Child";
    private const string CreateChildShortcut = " %#m";
    private const string ChildName = "Child";
    private const string UnparentContent = "Unparent";
    private const string UnparentShortcut = " %q";
    private const string ResetAllContent = "Reset All";

    private const string ResetAllTooltipContent = "Reset position, rotation and scale";
    private const string CreateChildTooltipContent = "Create a child to this transform";
    private const string UnparentTooltipContent = "Remove the reference to its parent";
    private const string RefreshReferencesTooltipContent = "Refresh the list of references";
    private const string ResetPositionTooltipContent = "Reset the transform position to [0,0,0]";
    private const string ResetRotationTooltipContent = "Reset the transform rotation to [0,0,0]";
    private const string ResetScaleTooltipContent = "Reset the transform scale to [1,1,1]";

    private const int ResetButtonWidth = 100;
    private const int VectorFieldMinimumWidth = 30;
    private const int ButtonBelowWidth = 75;


    private static List<Object> filteredReferences = new List<Object>();
    private static bool showReferences = true;
    private static int referenceCount = 0;
    private static Transform currentTransform;
    

    private static void DrawReference(string name, Object reference)
    {
        //EditorGUIUtility.ObjectContent()
        //EditorGUIUtility.HasObjectThumbnail(objType)
        //EditorGUIUtility.
        //(Texture) EditorGUIUtility.GetMiniThumbnail()
        EditorGUILayout.ObjectField(name, reference, reference.GetType(), true);
    }

    public override void OnInspectorGUI()
    {
        Transform targetTransform = target as Transform;

        //Refresh the references when you switch objects
        if (targetTransform != null && targetTransform != currentTransform)
        {
            currentTransform = targetTransform;

            if(showReferences)
            {
                RefreshReferences(currentTransform);
            }
        }

        //Only draw stuff when we have a transform
        if (currentTransform != null)
        {
            DrawDefaultTransformInspector(currentTransform);

            DrawButtons(currentTransform);

            if (!filteredReferences.IsNullOrEmpty())
            {
                DrawReferences(targetTransform);
            }
        }
    }

    private void DrawButtons(Transform targetTransform)
    {
        GUILayout.BeginHorizontal();
        DrawResetAllButton(targetTransform);
        DrawCreateChildButton(targetTransform);
        DrawUnparentButton(targetTransform);
        DrawRefreshReferencesButton(targetTransform);
        GUILayout.EndHorizontal();
    }

    private static void DrawResetAllButton(Transform targetTransform)
    {
        if (ESEGUILayout.Button(ResetAllContent, ResetAllTooltipContent, true, GUILayout.Width(ResetButtonWidth)))
        {
            ResetAll(targetTransform);
        }
    }

    private static bool IsResetAllValid(Transform targetTransform)
    {
        return IsResetPositionValid(targetTransform) ||
               IsResetRotationValid(targetTransform) ||
               IsResetScaleValid(targetTransform);
    }

    private static bool IsResetPositionValid(Transform targetTransform)
    {
        return (targetTransform.localPosition != Vector3.zero);
    }

    private static bool IsResetRotationValid(Transform targetTransform)
    {
        return (targetTransform.localEulerAngles != Vector3.zero);
    }

    private static bool IsResetScaleValid(Transform targetTransform)
    {
        return (targetTransform.localScale != new Vector3(1, 1, 1));
    }

    private static void ResetAll(Transform targetTransform)
    {
        SetLocalParameters(targetTransform, Vector3.zero, Vector3.zero, new Vector3(1f, 1f, 1f));
    }

    private static void DrawCreateChildButton(Transform targetTransform)
    {
        if (ESEGUILayout.Button(CreateChildContent, CreateChildTooltipContent, true, GUILayout.MinWidth(ButtonBelowWidth)))
        {
            CreateChild(targetTransform);
        }
    }

    public static void CreateChild(Transform targetTransform)
    {
        Undo.RecordObject(targetTransform, CreateChildContent);
        GameObject child = new GameObject(ChildName);
        child.transform.parent = targetTransform;

        child.transform.localPosition = Vector3.zero;
        child.transform.localEulerAngles = Vector3.zero;
        Selection.activeGameObject = child;
    }

    private static void DrawUnparentButton(Transform targetTransform)
    {
        if (ESEGUILayout.Button(UnparentContent, UnparentTooltipContent, UnparentTransformMenuItem.ValidateUnparent(null), GUILayout.MinWidth(ButtonBelowWidth)))
        {
            UnparentTransformMenuItem.Unparent(targetTransform);
        }
    }

    private void DrawRefreshReferencesButton(Transform targetTransform)
    {
        if (ESEGUILayout.Button(RefreshReferencesContent, RefreshReferencesTooltipContent, true, GUILayout.MinWidth(ButtonBelowWidth)))
        {
            RefreshReferences(targetTransform);
        }
    }

    private void DrawReferences(Transform targetTransform)
    {
        bool oldShowReferences = showReferences;
        showReferences = EditorGUILayout.Foldout(showReferences, string.Format(ShowReferencesContent, filteredReferences.Count));

        if(!oldShowReferences && showReferences)
        {
            //Just got toggled to show, so I should refresh.
            RefreshReferences(targetTransform);
        }

		EditorGUIUtility.fieldWidth = 0f;
		EditorGUIUtility.labelWidth = 0f;

		if (showReferences)
        {
            EditorGUI.indentLevel = 2;
            referenceCount = 0;
            foreach (Object reference in filteredReferences)
            {
                DrawReference(string.Format(ReferenceContent, referenceCount), reference);
                referenceCount++;
            } 
        }
    }

    private void RefreshReferences(Transform target)
    {
        //DateTime time = DateTime.Now;
        filteredReferences = ReferenceUtility.GetHierarchyReferences(target.gameObject);
        //Debug.Log("it took " + (DateTime.Now - time).Milliseconds + "ms to filter");
    }

    private static Vector3 Vector3Field(string id, Vector3 value)
    {
        GUI.SetNextControlName(id + "X");
        float x = EditorGUILayout.FloatField("X", value.x, GUILayout.MinWidth(VectorFieldMinimumWidth));
        GUI.SetNextControlName(id + "Y");
        float y = EditorGUILayout.FloatField("Y", value.y, GUILayout.MinWidth(VectorFieldMinimumWidth));
        GUI.SetNextControlName(id + "Y");
        float z = EditorGUILayout.FloatField("Z", value.z, GUILayout.MinWidth(VectorFieldMinimumWidth));

        EditorGUILayout.EndHorizontal();
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// This is the one we really want. But it is kinda broken in Unity 2.6.x (3.3.x as well)
    /// Below is a reverse engineering of DrawDefaultInspector for Transform
    /// </summary>
    /// <param name="targetTransform">The transform to alter</param>
    private static void DrawDefaultTransformInspector(Transform targetTransform)
    {
		EditorGUIUtility.labelWidth = 15f;
		EditorGUIUtility.fieldWidth = 0f;

        DrawResetPositionButton(targetTransform);
        Vector3 position = Vector3Field("position", targetTransform.localPosition);

        DrawResetRotationButton(targetTransform);
        Vector3 rotation = Vector3Field("rotation", targetTransform.localEulerAngles);

        DrawResetScaleButton(targetTransform);
        Vector3 scale = Vector3Field("scale", targetTransform.localScale);

        if (GUI.changed)
        {
            SetLocalParameters(targetTransform, position, rotation, scale);
        }

        //EditorGUIUtility.LookLikeControls();
    }

    private static void SetLocalParameters(Transform targetTransform, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Undo.RecordObject(targetTransform, UndoTransformChangeContent);
        targetTransform.localPosition = FixIfNaN(position);
        targetTransform.localEulerAngles = FixIfNaN(rotation);
        targetTransform.localScale = FixIfNaN(scale);
    }

    private static void DrawResetPositionButton(Transform targetTransform)
    {
        EditorGUILayout.BeginHorizontal();
        if (ESEGUILayout.Button(ResetPositionContent, ResetPositionTooltipContent, true, GUILayout.MinWidth(ResetButtonWidth)))
        {
            Undo.RecordObject(targetTransform, ResetPositionContent);
            targetTransform.localPosition = Vector3.zero;
        }
        // End horizontal outside of this method
    }

    private static void DrawResetRotationButton(Transform targetTransform)
    {
        EditorGUILayout.BeginHorizontal();
        if (ESEGUILayout.Button(ResetRotationContent, ResetRotationTooltipContent, true, GUILayout.MinWidth(ResetButtonWidth)))
        {
			Undo.RecordObject(targetTransform, ResetRotationContent);
            targetTransform.localEulerAngles = Vector3.zero;
        }
        // End horizontal outside of this method
    }

    private static void DrawResetScaleButton(Transform targetTransform)
    {
        EditorGUILayout.BeginHorizontal();
        if (ESEGUILayout.Button(ResetScaleContent, ResetScaleTooltipContent, true, GUILayout.MinWidth(ResetButtonWidth)))
        {
			Undo.RecordObject(targetTransform, ResetScaleContent);
            targetTransform.localScale = new Vector3(1f, 1f, 1f);
        }
        // End horizontal outside of this method
    }

    private static Vector3 FixIfNaN(Vector3 vector)
    {
        vector.x = FixNanAxis(vector.x);
        vector.y = FixNanAxis(vector.y);
        vector.z = FixNanAxis(vector.z);
        return vector;
    }

    private static float FixNanAxis(float axis)
    {
        if (float.IsNaN(axis))
        {
            axis = 0;
        }
        return axis;
    }

    public void OnSceneGUI()
    {
        MoveSelectedTransformInSceneView.OnSceneGUI();
    }
}