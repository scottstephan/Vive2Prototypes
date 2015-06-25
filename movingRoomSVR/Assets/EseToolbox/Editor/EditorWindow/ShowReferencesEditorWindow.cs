using System.Collections.Generic;
using Ese;
using UnityEditor;
using UnityEngine;

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
///     10th of August, 2011
/// 
/// Purpose:
///     Shows the references of a GameObject
/// </summary>
public class ShowReferencesEditorWindow : ScriptableWizard
{
    // Make this window appear in the Window tab.
    private const string MenuName = "Window/ESE Toolbox/" + WindowName + ShortCutKey;
    // The window name, used in both the menu and the titlebar.
    private const string WindowName = "Show References";
    // The shortcut key command: ctrl-shirt-L
    private const string ShortCutKey = " %#R";
    // The priority of the menu command
    private const int Priority = 40;
    // The about message
    private const string AboutContentMessage = "This tool helps you check who is referencing the selected gameobject.";

    private const string StepOneContent = "1. Select GameObject and press Find.";
    private const string StepTwoContent = "2. Hierarchy objects ({0}).";
    private const string StepThreeContent = "3. Project objects ({0}).";

    private const int FieldHeight = 20;
    private const int FieldWidth = 200;
    private const float Inset = 5f;
    private const string Find = "Find";

    private static GameObject gameObjectToSearchFor;
    private static List<Object> hierarchyReferences = new List<Object>();
    private static List<Object> projectReferences = new List<Object>();

    private static Vector2 scrollPositionTwo;
    private static Vector2 scrollPositionThree;

    /// <summary>
    /// This method will be called when the window is opened.
    /// </summary>
    [MenuItem(MenuName, false, Priority)]
    public static void Init()
    {
        DisplayWizard<ShowReferencesEditorWindow>(WindowName);
        ResetAllVariables();
    }

    /// <summary>
    /// This method will be called when the window is closed.
    /// </summary>
    public void OnDisable()
    {
        ResetAllVariables();
    }

    /// <summary>
    /// Reset all variables, so that the window will clear when shut down.
    /// </summary>
    private static void ResetAllVariables()
    {
        hierarchyReferences.Clear();
        projectReferences.Clear();
    }

    /// <summary>
    /// Draws the editor window.
    /// </summary>
    public void OnGUI()
    {
		EditorGUIUtility.labelWidth = (position.width / 2) - 55;
		EditorGUIUtility.fieldWidth = 25f;

        float startY = CreateStepOne(Inset, position.width);

        float height = PrepareForNextSteps(position, startY);

        if (!IsListsEmpty())
        {
            CreateFinalSteps(position, startY, height);
        }

        CreateAbout(position, startY, height);
    }

    private static bool IsListsEmpty()
    {
        return hierarchyReferences.Count == 0 && projectReferences.Count == 0;
    }

    private static float CreateStepOne(float startY, float width)
    {
        GUI.Box(new Rect(Inset, startY, width - (Inset * 2), (FieldHeight * 2) + 5), string.Empty);
        GUI.Label(new Rect(Inset + 2f, startY, (width / 2.0f) - (Inset * 2), (FieldHeight * 2) + 5), StepOneContent);


        startY += FieldHeight;

        float findButtonX = CreateLayerField(Inset * 2, startY, width - 22 - (FieldWidth / 2.0f) - Inset);
        startY += CreateFindButton(findButtonX, startY, FieldWidth / 2.0f);
        startY += Inset * 2;
        return startY;
    }

    private static float CreateLayerField(float startX, float startY, float width)
    {
        gameObjectToSearchFor = EditorGUI.ObjectField(new Rect(startX, startY, width, FieldHeight), gameObjectToSearchFor, typeof(GameObject), true) as GameObject;

        return startX + width + Inset;
    }

    private static float CreateFindButton(float startX, float startY, float width)
    {
        if (GUI.Button(new Rect(startX, startY, width, FieldHeight), Find))
        {
            ResetAllVariables();
            FindGameObjectsInHierarchy();
            FindGameObjectsInProject();
            EditorUtility.ClearProgressBar();
        }
        return FieldHeight;
    }

    private static void FindGameObjectsInProject()
    {
        projectReferences = ReferenceUtility.GetProjectReferences(gameObjectToSearchFor);
    }

    private static void FindGameObjectsInHierarchy()
    {
        hierarchyReferences = ReferenceUtility.GetHierarchyReferences(gameObjectToSearchFor);
    }

    private static float PrepareForNextSteps(Rect windowPos, float startY)
    {
        float height = windowPos.height;
        height -= (Inset * 3);
        height -= startY;
        height -= FieldHeight;
        return height;
    }

    private static void CreateFinalSteps(Rect windowPos, float startY, float height)
    {
        //height -= 40 + Inset * 2f;

        CreateStepTwo(windowPos, startY, height);

        CreateStepThree(windowPos, startY, height);
    }

    private static void CreateStepTwo(Rect windowPos, float startY, float height)
    {
        Rect listRect = new Rect(0, startY, (windowPos.width / 2), height);
        scrollPositionTwo = ESEGUI.CreateObjectList(listRect, scrollPositionTwo, hierarchyReferences,
                                                           string.Format(StepTwoContent, hierarchyReferences.Count));
    }


    private static float CreateStepThree(Rect windowPos, float startY, float height)
    {
        Rect listRect = new Rect((windowPos.width / 2), startY, (windowPos.width / 2), height);
        scrollPositionThree = ESEGUI.CreateObjectList(listRect, scrollPositionThree, projectReferences,
                                                 string.Format(StepThreeContent, projectReferences.Count));

        return startY + height;
    }

    private static void CreateAbout(Rect windowPos, float startY, float height)
    {
        Rect buttonRect = new Rect(windowPos.width - (Inset * 2) - FieldWidth / 2.0f, (startY + height) + 5f, FieldWidth / 2, FieldHeight);
        ESEGUI.AboutDialogButton(buttonRect, WindowName, AboutContentMessage);
    }
}