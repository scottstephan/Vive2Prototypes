using System.Collections.Generic;
using System.IO;
using Ese;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

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
///     28th of June, 2011
/// 
/// Purpose:
///     This tool searches for all the gameobjects that use a selected layer.
/// 
/// Why:
///     After spending a long time on the same project we wanted to clean up the layers, 
///     because we knew that some layers were used while others were just there. 
///     So I made this small tool to help us search for objects that use a selected layer 
///     and now we use it almost constantly in order to navigate in our project.
/// </summary>
class FindLayerObjectsEditorWindow : ScriptableWizard
{
    // Make this window appear in the Window tab.
    private const string MenuName = "Window/ESE Toolbox/" + WindowName + ShortCutKey;
    // The window name, used in both the menu and the titlebar.
    private const string WindowName = "Find Layer Objects";
    // The shortcut key command: ctrl+shift+Y
    private const string ShortCutKey = " %#T";
    // The priority of the menu command
    private const int Priority = 2;
    // The about message
    private const string AboutContentMessage = "This tool helps you find objects in your scene that are using layers.";

    private const string StepOneContent = "1. Select Layermask and press Find.";
    private const string StepTwoContent = "2. Hierarchy ({0}).";
    private const string StepThreeContent = "3. Project ({0}).";

    private const string LocatingPrefabsTitle = "Locating prefabs...";
    private const string SearchingForPrefabs = "Searching for prefabs...";
    private const string LoadingPrefabs = "Loading prefabs...";

    private const int FieldHeight = 20;
    private const int FieldWidth = 200;
    private const float Inset = 5f;
    private const string LocatingSceneObjectsTitle = "Locating scene objects";
    private const string LoadingAllSceneObjects = "Loading all scene objects";
    private const string FfilteringSceneObjects = "Filtering scene objects";
    private const string Find = "Find";

    private static LayerMask mask;
    private static readonly List<GameObject> HierarchyGameObjects = new List<GameObject>();
    private static readonly List<GameObject> ProjectGameObjects = new List<GameObject>();

    private static Vector2 scrollPositionTwo;
    private static Vector2 scrollPositionThree;


    [MenuItem(MenuName, false, Priority)]
    public static void Init()
    {
        DisplayWizard<FindLayerObjectsEditorWindow>(WindowName);
        ResetAllVariables();
    }

    public void OnDisable()
    {
        mask = 0;
        ResetAllVariables();
    }

    public void OnGUI()
    {
		EditorGUIUtility.labelWidth = (position.width / 2) - 55;
		EditorGUIUtility.fieldWidth = 25f;

        float startY = CreateStepOne(Inset, position.width);
        
        float height = PrepareForNextSteps(position, startY);

        if(!IsListsEmpty())
        {
            CreateFinalSteps(position, startY, height);
        }

        CreateAbout(position, startY, height);
    }

    private static bool IsListsEmpty()
    {
        return HierarchyGameObjects.Count == 0 && ProjectGameObjects.Count == 0;
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
        mask = EditorGUI.LayerField(new Rect(startX, startY, width, FieldHeight), mask);

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
        List<GameObject> prefabs = Ese.PrefabUtility.GetPrefabs();
        foreach (GameObject gameObject in prefabs)
        {
            AddGameObjectsToList(gameObject, ProjectGameObjects);
        }
    }

    private static void FindGameObjectsInHierarchy()
    {
        EditorUtility.DisplayProgressBar(LocatingSceneObjectsTitle, LoadingAllSceneObjects, 0);

        UnityEngine.Object[] objects = FindObjectsOfType(typeof(GameObject));

        float progress = 0.5f / objects.Length;
        float overallProgress = 0f;

        foreach (UnityEngine.Object obj in objects)
        {
            EditorUtility.DisplayProgressBar(LocatingSceneObjectsTitle, FfilteringSceneObjects, overallProgress);
            GameObject gameObject = obj as GameObject;
            AddGameObjectsToList(gameObject, HierarchyGameObjects);
            overallProgress += progress;
        }
    }

    private static void AddGameObjectsToList(GameObject gameObject, List<GameObject> listToAddTo)
    {
        if(gameObject != null)
        {
            if (!listToAddTo.Contains(gameObject) && gameObject.layer == mask)
            {
                listToAddTo.Add(gameObject);
            }

            Transform thisTransform = gameObject.transform;
            int childCount = thisTransform.childCount;
            for (int child = 0; child < childCount; child++)
            {
                GameObject childGameObject = thisTransform.GetChild(child).gameObject;
                AddGameObjectsToList(childGameObject, listToAddTo);
            }
        }
    }

    private static float PrepareForNextSteps(Rect windowPos, float startY)
    {
        float height = windowPos.height - (Inset * 2);
        height -= startY;
        height -= FieldHeight;
        return height;
    }

    private static void CreateFinalSteps(Rect windowPos, float startY, float height)
    {
        CreateStepTwo(windowPos, startY, height);

        CreateStepThree(windowPos, startY, height);
    }

    private static void CreateStepTwo(Rect windowPos, float startY, float height)
    {
        int count = HierarchyGameObjects.Count;

        Rect listRect = new Rect(0, startY, (windowPos.width/2), height);
        scrollPositionTwo = ESEGUI.CreateObjectList(listRect, scrollPositionTwo, HierarchyGameObjects,
                                                 string.Format(StepTwoContent, count));
    }

    private static void CreateStepThree(Rect windowPos, float startY, float height)
    {
        int count = ProjectGameObjects.Count;

        float startX = (windowPos.width / 2f);
        Rect listRect = new Rect(startX, startY, startX, height);
        scrollPositionThree = ESEGUI.CreateObjectList(listRect, scrollPositionThree, ProjectGameObjects,
                                                 string.Format(StepThreeContent, count));
    }

    private static void CreateAbout(Rect windowPos, float startY, float height)
    {
        Rect buttonRect = new Rect(windowPos.width - (FieldWidth / 2) - Inset, (startY + height) + 5f, FieldWidth / 2.0f, FieldHeight);
        ESEGUI.AboutDialogButton(buttonRect, WindowName, AboutContentMessage);
    }

    private static void ResetAllVariables()
    {
        HierarchyGameObjects.Clear();
        ProjectGameObjects.Clear();
    }
}
