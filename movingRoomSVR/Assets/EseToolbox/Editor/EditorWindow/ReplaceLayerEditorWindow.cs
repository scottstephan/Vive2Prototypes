using System.Collections.Generic;
using System.Reflection;
using Ese;
using UnityEditor;
using UnityEngine;

public class ReplaceLayerEditorWindow : ScriptableWizard
{
    // Make this window appear in the Window tab.
    private const string MenuName = "Window/ESE Toolbox/" + WindowName + ShortCutKey;
    // The window name, used in both the menu and the titlebar.
    private const string WindowName = "Replace Layer";
    // The shortcut key command: ctrl+shift+Y
    private const string ShortCutKey = " %#Y";
    // The priority of the menu command
    private const int Priority = 3;
    // The about message
    private const string AboutContentMessage = "This tool helps you find objects by layers and replace the layers.";

    private const string StepOneContent = "1. Select Layermask and press Find.";
    private const string StepTwoContent = "2. Choose Hierarchy objects ({0}).";
    private const string StepThreeContent = "3. Choose Project objects ({0}).";
    private const string StepFourContent = "4. Select Layermask and press Replace.";

    private const string StepTwoError1Content = "There are no gameobjects with this layer.";

    private const int FieldHeight = 20;
    private const int FieldWidth = 200;
    private const float Inset = 5f;
    private const string LocatingSceneObjectsTitle = "Locating scene objects";
    private const string LoadingAllSceneObjects = "Loading all scene objects";
    private const string FfilteringSceneObjects = "Filtering scene objects";
    private const string Find = "Find";

    private const string ReplaceButtonContent = "Replace";

    private static LayerMask maskToSearchFor;
    private static readonly Dictionary<GameObject, bool> HierarchyGameObjects = new Dictionary<GameObject, bool>();
    private static readonly Dictionary<GameObject, bool> ProjectGameObjects = new Dictionary<GameObject, bool>();
    private static LayerMask maskToReplaceWith;

    private static Vector2 scrollPositionTwo;
    private static Vector2 scrollPositionThree;

    /// <summary>
    /// This method will be called when the window is opened.
    /// </summary>
    [MenuItem(MenuName, false, Priority)]
    public static void Init()
    {
        DisplayWizard<ReplaceLayerEditorWindow>(WindowName);
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
        HierarchyGameObjects.Clear();
        ProjectGameObjects.Clear();
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
        ESEGUI.TitleBox(new Rect(0, 0, width, (FieldHeight * 2 + Inset * 3)), StepOneContent);

        startY += FieldHeight;

        float findButtonX = CreateLayerField(Inset * 2, startY, width - 22 - (FieldWidth / 2.0f) - Inset);
        startY += CreateFindButton(findButtonX, startY, FieldWidth / 2.0f);
        startY += Inset * 2;
        return startY;
    }

    private static float CreateLayerField(float startX, float startY, float width)
    {
        maskToSearchFor = EditorGUI.LayerField(new Rect(startX, startY, width, FieldHeight), maskToSearchFor);

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

    private static void AddGameObjectsToList(GameObject gameObject, Dictionary<GameObject, bool> listToAddTo)
    {
        if(gameObject != null)
        {
            if (!listToAddTo.ContainsKey(gameObject) && gameObject.layer == maskToSearchFor)
            {
                listToAddTo.Add(gameObject, true);
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
        float height = windowPos.height;
        height -= (Inset * 3);
        height -= startY;
        height -= FieldHeight;
        return height;
    }

    private void CreateFinalSteps(Rect windowPos, float startY, float height)
    {
        height -= 40 + Inset * 2f;

        CreateStepTwo(windowPos, startY, height);

        startY = CreateStepThree(windowPos, startY, height);

        startY += Inset;
        CreateStepFour(startY, windowPos.width);
    }

    private static void CreateStepTwo(Rect windowPos, float startY, float height)
    {
        Rect listRect = new Rect(Inset, startY, (windowPos.width/2) - Inset, height);
        scrollPositionTwo = ESEGUI.CreateGameObjectToggleList<GameObject>(listRect, scrollPositionTwo, HierarchyGameObjects,
                                                           string.Format(StepTwoContent, HierarchyGameObjects.Count));
    }


    private static float CreateStepThree(Rect windowPos, float startY, float height)
    {
        Rect listRect = new Rect((windowPos.width / 2) + Inset, startY, (windowPos.width / 2) - Inset * 2, height);
        scrollPositionThree = ESEGUI.CreateGameObjectToggleList(listRect, scrollPositionThree, ProjectGameObjects, 
                                                 string.Format(StepThreeContent, ProjectGameObjects.Count));

        return startY + height;
    }

    private void CreateStepFour(float startY, float width)
    {
        GUI.Box(new Rect(Inset, startY, width - (Inset * 2), (FieldHeight * 2) + 5), string.Empty);
        GUI.Label(new Rect(Inset + 2f, startY, (width / 2.0f) - (Inset * 2), (FieldHeight * 2) + 5), StepFourContent);


        startY += FieldHeight;

        float findButtonX = CreateLayerToCopyToField(Inset * 2, startY, width - 22 - (FieldWidth / 2.0f) - Inset);
        CreateCopyButton(findButtonX, startY);
    }

    private static float CreateLayerToCopyToField(float startX, float startY, float width)
    {
        maskToReplaceWith = EditorGUI.LayerField(new Rect(startX, startY, width, FieldHeight), maskToReplaceWith);

        return startX + width + Inset;
    }

    private void CreateCopyButton(float startX, float startY)
    {
        Rect buttonRect = new Rect(startX, startY, FieldWidth / 2.0f, FieldHeight);
        if ((!HierarchyGameObjects.IsNullOrEmpty() || !ProjectGameObjects.IsNullOrEmpty()) && maskToReplaceWith != maskToSearchFor)
        {
            PerformCopy(buttonRect);
        }
        else
        {
            GUI.Box(buttonRect, ReplaceButtonContent);
        }
    }

    private void PerformCopy(Rect buttonRect)
    {
        bool performCopy = GUI.Button(buttonRect, ReplaceButtonContent);

        if (performCopy)
        {
            List<GameObject> gameObjectsToChangeLayerOn = new List<GameObject>();
            foreach (KeyValuePair<GameObject, bool> hierarchy in HierarchyGameObjects)
            {
                if (hierarchy.Value)
                {
                    gameObjectsToChangeLayerOn.Add(hierarchy.Key);
                }
            }

            foreach (KeyValuePair<GameObject, bool> project in ProjectGameObjects)
            {
                if(project.Value)
                {
                    gameObjectsToChangeLayerOn.Add(project.Key);
                }
            }
            
            foreach(GameObject gos in gameObjectsToChangeLayerOn)
            {
                gos.layer = maskToReplaceWith;
                RemoveFromList(HierarchyGameObjects, gos);
                RemoveFromList(ProjectGameObjects, gos);
            }
        }

        EditorUtility.ClearProgressBar();
    }

    

    private void RemoveFromList(Dictionary<GameObject, bool> list, GameObject go)
    {
        if(list.ContainsKey(go))
        {
            list.Remove(go);
        }
    }

    private static void CreateAbout(Rect windowPos, float startY, float height)
    {
        Rect buttonRect = new Rect(windowPos.width - (Inset * 2) - FieldWidth / 2.0f, (startY + height) + 5f, FieldWidth / 2, FieldHeight);
        ESEGUI.AboutDialogButton(buttonRect, WindowName, AboutContentMessage);
    }
}