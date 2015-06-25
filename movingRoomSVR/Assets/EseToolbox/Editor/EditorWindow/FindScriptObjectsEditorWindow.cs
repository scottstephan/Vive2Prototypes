using System;
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
///     This tool searches for all the gameobjects that use a selected script.
/// 
/// Why:
///     When refactoring or creating new scripts that have the same functionality 
///     as old ones we wanted a tool that quickly searched the scene and prefabs 
///     for gameobjects that used a certain script.
/// </summary>
class FindScriptObjectsEditorWindow : ScriptableWizard
{
    // Make this window appear in the Window tab.
    private const string MenuName = "Window/ESE Toolbox/" + WindowName + ShortCutKey;
    // The window name, used in both the menu and the titlebar.
    private const string WindowName = "Find Script Objects";
    // The shortcut key command: ctrl+T
    private const string ShortCutKey = " %T";
    // The priority of the menu command
    private const int Priority = 1;
    // The about message
    private const string AboutContentMessage = "This tool helps you find objects in your scene that are using a script.";

    private const string StepOneContent = "1. Select MonoBehaviour and press Find.";
    private const string StepOneCheckbox = "Use inheritance";
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
    private const string FilteringSceneObjects = "Filtering scene objects";
    private const string Find = "Find";

    private static MonoScript script;
    private static readonly List<GameObject> HierarchyGameObjects = new List<GameObject>();
    private static readonly List<GameObject> ProjectGameObjects = new List<GameObject>();

    private static Vector2 scrollPositionTwo;
    private static Vector2 scrollPositionThree;
    private static bool useInheritance;


    [MenuItem(MenuName, false, Priority)]
    public static void Init()
    {
        DisplayWizard<FindScriptObjectsEditorWindow>(WindowName);
        ResetAllVariables();
    }

    public void OnDisable()
    {
        script = null;
        ResetAllVariables();
    }

    public void OnGUI()
    {
		EditorGUIUtility.labelWidth = (position.width / 2) - 55;
		EditorGUIUtility.fieldWidth = 25f;

        float startY = CreateStepOne(position.width);
        
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

    private static float CreateStepOne(float width)
    {
        Rect upperLeft = ESEGUI.TitleBox(new Rect(0, 0, width, FieldHeight * 2 + Inset * 3), StepOneContent);

        float findButtonX = CreateScriptField(upperLeft.x, upperLeft.y, width - 20 - (225) - Inset);
        findButtonX += CreateUseInheritanceCheckbox(findButtonX, upperLeft.y, 125);
        upperLeft.y += CreateFindButton(findButtonX, upperLeft.y, FieldWidth / 2.0f);
        upperLeft.y += Inset * 2;
        return upperLeft.y;
    }

    private static float CreateScriptField(float startX, float startY, float width)
    {
        MonoScript monoScript = EditorGUI.ObjectField(new Rect(startX, startY, width, FieldHeight), script, typeof(MonoScript), true) as MonoScript;
        if (IsMonoBehaviour(monoScript))
        {
            script = monoScript;
        }

        return startX + width + Inset;
    }

    private static bool IsMonoBehaviour(MonoScript monoScript)
    {
        return monoScript != null && monoScript.GetClass().IsSubclassOf(typeof(MonoBehaviour));
    }

    private static float CreateUseInheritanceCheckbox(float startX, float startY, float width)
    {
        Rect buttonRect = new Rect(startX, startY, width, FieldHeight);


        useInheritance = GUI.Toggle(buttonRect, useInheritance, StepOneCheckbox);

        return width;
    }

    private static float CreateFindButton(float startX, float startY, float width)
    {
        Rect buttonRect = new Rect(startX, startY, width, FieldHeight);
        

        if (script != null)
        {
            Type scriptType = script.GetClass();

            if (GUI.Button(buttonRect, Find))
            {
                
                ResetAllVariables();
                FindGameObjectsInHierarchy(scriptType);
                FindGameObjectsInProject(scriptType);
                EditorUtility.ClearProgressBar();
            }
        }
        else
        {
            GUI.Box(buttonRect, Find);
        }
        
        return FieldHeight;
    }

    private static void FindGameObjectsInProject(Type scriptType)
    {
        List<GameObject> prefabs = Ese.PrefabUtility.GetPrefabs();
        foreach (GameObject gameObject in prefabs)
        {
            AddGameObjectsToList(gameObject, ProjectGameObjects, scriptType);
        }
    }

    private static void FindGameObjectsInHierarchy(Type scriptType)
    {
        EditorUtility.DisplayProgressBar(LocatingSceneObjectsTitle, LoadingAllSceneObjects, 0);

        UnityEngine.Object[] objects = FindObjectsOfType(typeof(GameObject));

        float progress = 0.5f / objects.Length;
        float overallProgress = 0f;
        

        foreach (UnityEngine.Object obj in objects)
        {
            EditorUtility.DisplayProgressBar(LocatingSceneObjectsTitle, FilteringSceneObjects, overallProgress);
            GameObject gameObject = obj as GameObject;
            AddGameObjectsToList(gameObject, HierarchyGameObjects, scriptType);
            overallProgress += progress;
        }
    }

    private static void AddGameObjectsToList(GameObject gameObject, List<GameObject> listToAddTo, Type scriptType)
    {
        if(gameObject != null)
        {
            if (!listToAddTo.Contains(gameObject))
            {
                Component comp = gameObject.GetComponent(scriptType);
                if(comp != null)
                {
                    if(!useInheritance)
                    {
                        Type componentType = comp.GetType();
                        if (componentType == scriptType)
                        {
                            listToAddTo.Add(gameObject);
                        }
                    }
                    else
                    {
                        listToAddTo.Add(gameObject);
                    }
                    
                }
            }

            Transform thisTransform = gameObject.transform;
            int childCount = thisTransform.childCount;
            for(int child = 0; child < childCount; child++)
            {
                GameObject childGameObject = thisTransform.GetChild(child).gameObject;
                AddGameObjectsToList(childGameObject, listToAddTo, scriptType);
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

        Rect listRect = new Rect(0, startY, (windowPos.width / 2), height);
        scrollPositionTwo = ESEGUI.CreateObjectList(listRect, scrollPositionTwo, HierarchyGameObjects,
                                                 string.Format(StepTwoContent, count));
    }

    private static void CreateStepThree(Rect windowPos, float startY, float height)
    {
        int count = ProjectGameObjects.Count;

        float startX = (windowPos.width / 2);
        Rect listRect = new Rect(startX, startY, (windowPos.width / 2), height);
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
