using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
///     28th of June, 2011
/// 
/// Purpose:
///     This tool copies the contents of one variable into another in the chosen script 
///     in all the prefabs it can locate in your Project view.
/// 
/// Why:
///     The need for this tool arose when we had come along way in our production. 
///     The programmers would sometimes make scripts that did not adhear to the coding 
///     standard or did not make sense naming-wise, 
///     but the designers had already started using it. 
/// 
///     So when a programmer started cleaning up his code and renamed a variable 
///     the game broke because Unity lost all its references to the old variable name, 
///     and thus ruining the designers work.
/// </summary>
public class CopyFieldEditorWindow : ScriptableWizard
{
    // Make this window appear in the Window tab.
    private const string MenuName = "Window/ESE Toolbox/" + WindowName + ShortCutKey;
    // The window name, used in both the menu and the titlebar.
    private const string WindowName = "Copy Field";
    // The shortcut key command: Ctrl + Shift + O
    private const string ShortCutKey = " %#O";
    // The priority of the menu command
    private const int Priority = 20;
    // The about message
    private const string AboutContentMessage = "This tool helps you rename variables without losing information set in your prefabs.\n"+
                                   "\nWhen renaming a variable;"+
                                   "\n1. Introduce the new named variable in your script."+
                                   "\n2. Use this tool to copy the old variables contents into the new variable."+
                                   "\n3. Then remove the old variable from your script.";

    private const string StepOneContent = "1. Drag & Drop in the script to copy from.";
    private const string StepTwoContent = "2. Drag & Drop in the script to copy to.";
    private const string StepThreeContent = "3. Choose one field to copy from ({0}).";
    private const string StepThreeError1Content = "There are no public variables to copy to.";
    private const string StepThreeError2Content = "There are no public variables to copy.";

    private const string SelectedContent = "Selected: {0} ({1})";
    private const string StepFourContent = "4. Choose one field to copy to ({0}).";

    private const string StepFiveContent = "5. Select where to find gameobjects to affect.";

    private const string CopyButtonContent = "Copy";
    private const string CopyingFieldsTitle = "Copying fields...";
    private const string CopyingValues = "Copying values in {0} ({1})";
    private const string UndoContent = "Copy field in {0}";
    private const string OKButtonContent = "Ok";
    
    private const string DebugFoundPrefabs = "{0}: Found {1} prefabs.";
    private const string DebugChangedScripts = "{0}: Changed {1} {2} scripts.";

    private const int FieldHeight = 20;
    private const int FieldWidth = 200;
    private const float Inset = 5f;
    private const string CopyButtonTooltipContent = "Copy contents";

    private static MonoScript stepOneScript;
    private static Type scriptOneType;
    private static MonoScript stepTwoScript;
    private static Type scriptTwoType;
    private static List<FieldInfo> infosToSelectFrom = new List<FieldInfo>();
    private static List<FieldInfo> infosToSelectTo = new List<FieldInfo>();
    private static Dictionary<MemberInfo, bool> fromSelected = new Dictionary<MemberInfo, bool>();
    private static Dictionary<MemberInfo, bool> toSelected = new Dictionary<MemberInfo, bool>();
    private static FieldInfo selectedFromInfo;
    private static FieldInfo selectedToInfo;
    private static Vector2 scrollPositionTwo;
    private static Vector2 scrollPositionThree;

    private static CopyOption option = CopyOption.Hierarchy;

    [MenuItem(MenuName, false, Priority)]
    public static void Init()
    {
        DisplayWizard<CopyFieldEditorWindow>(WindowName);
        stepOneScript = null;
        stepTwoScript = null;
        ResetAllVariables();
    }

    public void OnDisable()
    {
        stepOneScript = null;
        stepTwoScript = null;
        ResetAllVariables();
    }

    private static void ResetAllVariables()
    {
        if (stepOneScript != null)
        {
            scriptOneType = stepOneScript.GetClass();
            infosToSelectFrom = new List<FieldInfo>(scriptOneType.GetFields().Where(info => info.IsPublic));
        }
        else
        {
            infosToSelectFrom.Clear();
        }

        if (stepTwoScript != null)
        {
            scriptTwoType = stepTwoScript.GetClass();
            infosToSelectTo = new List<FieldInfo>(scriptTwoType.GetFields().Where(info => info.IsPublic));
        }
        else
        {
            infosToSelectTo.Clear();
        }

        fromSelected = new Dictionary<MemberInfo, bool>();
        toSelected = new Dictionary<MemberInfo, bool>();
        selectedFromInfo = null;
        selectedToInfo = null;
    }

    public void OnGUI()
    {
		EditorGUIUtility.labelWidth = (position.width / 2) - 55;
		EditorGUIUtility.fieldWidth = 25f;

        float startY = CreateStepOne(Inset, position.width);

        startY = CreateStepTwo(startY, position.width);
        
        float height = PrepareForNextSteps(position, startY);

        CreateFinalSteps(position, startY, height);

        height += Inset;
        CreateAbout(position, startY, height);

        CreateCopyButton(position, startY, height);
    }

    private static void CreateAbout(Rect windowPos, float startY, float height)
    {
        Rect buttonRect = new Rect(windowPos.width - (FieldWidth / 2) - (Inset * 2) - FieldWidth / 2.0f, (startY + height) + 5f, FieldWidth / 2, FieldHeight);
        ESEGUI.AboutDialogButton(buttonRect, WindowName, AboutContentMessage);
    }

    private static float CreateStepOne(float startY, float width)
    {
        GUI.Box(new Rect(Inset, startY, width - (Inset * 2), (FieldHeight * 2) + 5), string.Empty);
        GUI.Label(new Rect(Inset + 2f, startY, width - (Inset * 2), (FieldHeight * 2) + 5), StepOneContent);
        startY += FieldHeight;

        startY += CreateStepOneScriptName(Inset * 2, startY, width - 22);
        startY += Inset * 2;
        return startY;
    }

    private static float CreateStepOneScriptName(float startX, float startY, float width)
    {
        MonoScript oldScript = stepOneScript;
        MonoScript newScript = EditorGUI.ObjectField(new Rect(startX, startY, width, FieldHeight), stepOneScript, typeof(MonoScript), true) as MonoScript;

        stepOneScript = newScript;
        if (oldScript != newScript)
        {
            ResetAllVariables();
        }

        return FieldHeight;
    }

    private static float CreateStepTwo(float startY, float width)
    {
        GUI.Box(new Rect(Inset, startY, width - (Inset * 2), (FieldHeight * 2) + 5), string.Empty);
        GUI.Label(new Rect(Inset + 2f, startY, width - (Inset * 2), (FieldHeight * 2) + 5), StepTwoContent);
        startY += FieldHeight;

        startY += CreateStepTwoScriptName(Inset * 2, startY, width - 22);
        startY += Inset * 2;
        return startY;
    }

    private static float CreateStepTwoScriptName(float startX, float startY, float width)
    {
        MonoScript oldScript = stepTwoScript;
        MonoScript newScript = EditorGUI.ObjectField(new Rect(startX, startY, width, FieldHeight), stepTwoScript, typeof(MonoScript), true) as MonoScript;

        stepTwoScript = newScript;
        if (oldScript != newScript)
        {
            ResetAllVariables();
        }

        return FieldHeight;
    }

    private static float PrepareForNextSteps(Rect windowPos, float startY)
    {
        float height = windowPos.height;
        height -= (Inset*3);
        height -= startY;
        height -= FieldHeight;
        return height;
    }

    private static void CreateFinalSteps(Rect windowPos, float startY, float height)
    {
        EnsureDictionaryHasValues();

        CreateStepThree(windowPos, startY, height);

        startY = CreateStepFour(windowPos, startY, height);

        CreateStepFive(windowPos, startY, height);

        if (selectedFromInfo != null)
        {
            DeselectEverythingExcept(selectedFromInfo, fromSelected);
            CreateSelectedLabel(windowPos);
        }

        if (selectedToInfo != null)
        {
            DeselectEverythingExcept(selectedToInfo, toSelected);
        }
    }

    private static void CreateStepFive(Rect windowPos, float startY, float height)
    {
        startY += ((height + 3) - FieldHeight * 2);
        GUI.Box(new Rect(Inset, startY, windowPos.width - Inset * 2, FieldHeight), string.Empty);
        GUI.Label(new Rect(Inset + 2f, startY, (windowPos.width / 2) - Inset * 2, FieldHeight), StepFiveContent);

        Rect buttonRect = new Rect(windowPos.width - (FieldWidth / 2) - Inset, startY + 2, FieldWidth / 2, FieldHeight);
        option = (CopyOption)EditorGUI.EnumPopup(buttonRect, option);
    }

    private static void EnsureDictionaryHasValues()
    {
        foreach (FieldInfo info in infosToSelectFrom)
        {
            if (!fromSelected.ContainsKey(info))
            {
                fromSelected[info] = false;
            }
        }

        foreach (FieldInfo info in infosToSelectTo)
        {
            if (!toSelected.ContainsKey(info))
            {
                toSelected[info] = false;
            }
        }
    }


    #region StepThree
    private static void CreateStepThree(Rect windowPos, float startY, float height)
    {
        int count = infosToSelectFrom.Count;

        height -= FieldHeight;
        GUI.Box(new Rect(Inset, startY, (windowPos.width / 2) - Inset * 2, height), string.Empty);
        GUI.Label(new Rect(Inset + 2f, startY, (windowPos.width / 2) - Inset * 2, height), string.Format(StepThreeContent, count));
        startY += 22f;
        float fieldStartY = startY;

        float scrollWidth = CreateStepThreeScrollView(windowPos, height, count, fieldStartY);

        CreateStepThreeContent(fieldStartY, scrollWidth, count);

        GUI.EndScrollView();
    }

    private static float CreateStepThreeScrollView(Rect windowPos, float height, int count, float fieldStartY)
    {
        float scrollWidth = (windowPos.width / 2) - Inset * 4;
        Rect scrollView = new Rect(Inset * 2, fieldStartY, scrollWidth, height - 28f);
        Rect viewRect = new Rect(Inset * 2, fieldStartY, scrollWidth, FieldHeight * count);
        GUI.Box(scrollView, string.Empty);
        scrollPositionTwo = GUI.BeginScrollView(scrollView, scrollPositionTwo, viewRect);
        return scrollWidth;
    }

    private static void CreateStepThreeContent(float fieldStartY, float scrollWidth, int count)
    {
        if (count > 1)
        {
            CreateStepThreeList(fieldStartY, scrollWidth);
        }
        else if (count == 1)
        {
            DisplayStepThreeError(fieldStartY, scrollWidth, StepThreeError1Content);
        }
        else
        {
            DisplayStepThreeError(fieldStartY, scrollWidth, StepThreeError2Content);
        }
    }

    private static void CreateStepThreeList(float fieldStartY, float scrollWidth)
    {
        foreach (FieldInfo info in infosToSelectFrom)
        {
            fieldStartY += CreateStepThreeListItem(info, fieldStartY, scrollWidth);
        }
    }

    private static int CreateStepThreeListItem(FieldInfo info, float fieldStartY, float scrollWidth)
    {
        bool oldValue = fromSelected[info];

        Rect toggleRect = new Rect(Inset * 2, fieldStartY, scrollWidth, FieldHeight);
        bool newValue = EditorGUI.Toggle(toggleRect, info.Name, oldValue);
        if (newValue && oldValue == false)
        {
            selectedFromInfo = info;
            fromSelected[info] = true;
            DeselectEverything(toSelected);
            selectedToInfo = null;
        }
        return FieldHeight;
    }

    private static void DisplayStepThreeError(float fieldStartY, float scrollWidth, string error)
    {
        Rect errorRect = new Rect(Inset * 2, fieldStartY, scrollWidth, FieldHeight);
        GUI.Label(errorRect, error);
    }
    #endregion

    #region StepFour
    private static float CreateStepFour(Rect windowPos, float startY, float height)
    {
        List<FieldInfo> realInfos = infosToSelectTo;

        if (selectedFromInfo != null)
        {
            realInfos = infosToSelectTo.Where(info => info != selectedFromInfo && info.FieldType == selectedFromInfo.FieldType).ToList();
        }

        int count = realInfos.Count;

        height -= FieldHeight;
        float startX = (windowPos.width / 2) + Inset;
        GUI.Box(new Rect(startX, startY, (windowPos.width / 2) - Inset * 2, height), string.Empty);
        GUI.Label(new Rect(startX + 2f, startY, (windowPos.width / 2) - Inset * 2, height), string.Format(StepFourContent, count));
        startY += 22f;
        float fieldStartY = startY;



        float scrollWidth = CreateStepFourScrollView(windowPos, startX, height, count, fieldStartY);

        CreateStepFourContent(realInfos, startX, count, fieldStartY, scrollWidth);
        GUI.EndScrollView();

        return startY;
    }

    private static void CreateStepFourContent(IEnumerable<FieldInfo> realInfos, float startX, int count, float fieldStartY, float scrollWidth)
    {
        if (count > 0)
        {
            CreateStepFourList(realInfos, startX, fieldStartY, scrollWidth);
        }
        else
        {
            DisplayStepFourError(startX, fieldStartY, scrollWidth);
        }
    }

    private static void CreateStepFourList(IEnumerable<FieldInfo> realInfos, float startX, float fieldStartY, float scrollWidth)
    {
        foreach (FieldInfo info in realInfos)
        {
            fieldStartY += CreateStepFourListItem(info, startX, fieldStartY, scrollWidth);
        }
    }

    private static float CreateStepFourListItem(FieldInfo info, float startX, float fieldStartY, float scrollWidth)
    {
        bool oldValue = toSelected[info];

        Rect toggleRect = new Rect(startX + Inset, fieldStartY, scrollWidth, FieldHeight);
        bool newValue = EditorGUI.Toggle(toggleRect, info.Name, oldValue);
        if (newValue && oldValue == false)
        {
            selectedToInfo = info;
            toSelected[info] = true;
        }

        return FieldHeight;
    }

    private static float CreateStepFourScrollView(Rect windowPos, float startX, float height, int count, float fieldStartY)
    {
        float scrollWidth = (windowPos.width / 2) - Inset * 4;
        Rect scrollView = new Rect(startX + Inset, fieldStartY, scrollWidth, height - 28f);
        Rect viewRect = new Rect(startX + Inset, fieldStartY, scrollWidth, FieldHeight * count);
        GUI.Box(scrollView, string.Empty);
        scrollPositionThree = GUI.BeginScrollView(scrollView, scrollPositionThree, viewRect);
        return scrollWidth;
    }

    private static void DisplayStepFourError(float startX, float fieldStartY, float scrollWidth)
    {
        Rect errorRect = new Rect(startX + Inset, fieldStartY, scrollWidth, FieldHeight);
        GUI.Label(errorRect, StepThreeError1Content);
    }
    #endregion


    private static void CreateCopyButton(Rect windowPos, float startY, float height)
    {
        Rect buttonRect = new Rect(windowPos.width - (FieldWidth / 2) - Inset, (startY + height) + 5f, FieldWidth / 2, FieldHeight);
        if (ESEGUI.Button(buttonRect, CopyButtonContent, CopyButtonTooltipContent, selectedToInfo != null && selectedFromInfo != null))
        {
            PerformCopy();
        }

        EditorUtility.ClearProgressBar();
    }

    private static void PerformCopy()
    {
        List<GameObject> prefabs = new List<GameObject>();

        switch (option)
        {
            case CopyOption.Both:
                prefabs.AddRange(Ese.PrefabUtility.GetPrefabs());
                prefabs.AddRange(GetSceneGameObjects());
                break;
            case CopyOption.Hierarchy:
                prefabs.AddRange(GetSceneGameObjects());
                break;
            case CopyOption.Project:
                prefabs.AddRange(Ese.PrefabUtility.GetPrefabs());
                break;
        }

        CopyPrefabFields(prefabs, selectedFromInfo, selectedToInfo);
    }

    private static IEnumerable<GameObject> GetSceneGameObjects()
    {
        GameObject[] sceneObjects = FindObjectsOfType(typeof (GameObject)) as GameObject[];

        return sceneObjects;
    }

    
    #region CopyField
    private static void CopyPrefabFields(List<GameObject> prefabs, FieldInfo from, FieldInfo to)
    {
        float progress = 0.5f + (0.5f / prefabs.Count);
        Debug.Log(string.Format(DebugFoundPrefabs, WindowName, prefabs.Count));
        float overallProgress = 0f;

        int countChanged = 0;
        foreach (GameObject prefab in prefabs)
        {
            countChanged = CopyPrefabField(prefab, overallProgress, countChanged, from, to);
            overallProgress += progress;
        }

        Debug.Log(string.Format(DebugChangedScripts, WindowName, countChanged, scriptOneType.Name));
    }

    private static int CopyPrefabField(GameObject prefab, float overallProgress, int countChanged, FieldInfo from, FieldInfo to)
    {
        List<Component> components = GetScripts(prefab, scriptOneType);
        EditorUtility.DisplayProgressBar(CopyingFieldsTitle, string.Format(CopyingValues, prefab.name, components.Count), overallProgress);

        countChanged += CopyFieldInComponents(components, from, to);
        return countChanged;
    }

    private static List<Component> GetScripts(GameObject go, Type type)
    {
        List<Component> components = new List<Component>(go.GetComponents(type));
        components.AddRange(go.GetComponentsInChildren(type));
        return components;
    }

    private static int CopyFieldInComponents(IEnumerable<Component> components, FieldInfo from, FieldInfo to)
    {
        return components.Count(component => CopyFieldInComponent(component, from, to));
    }

    private static bool CopyFieldInComponent(Component component, FieldInfo from, FieldInfo to)
    {
        FieldInfo fieldInfoFrom = null;
        FieldInfo fieldInfoTo = null;

        FieldInfo[] fieldInfos = component.GetType().GetFields();
        
        foreach (FieldInfo fieldInfo in fieldInfos)
        {
            string name = fieldInfo.Name;
            fieldInfoFrom = FindFromField(fieldInfo, name, from, fieldInfoFrom);
            
            fieldInfoTo = FindToField(fieldInfo, name, to, fieldInfoTo);
        }

        return CopyField(component, fieldInfoTo, fieldInfoFrom);
    }

    private static FieldInfo FindToField(FieldInfo fieldInfo, string name, FieldInfo to, FieldInfo fieldInfoTo)
    {
        if (name.Equals(to.Name))
        {
            fieldInfoTo = fieldInfo;
        }
        return fieldInfoTo;
    }

    private static FieldInfo FindFromField(FieldInfo fieldInfo, string name, FieldInfo from, FieldInfo fieldInfoFrom)
    {
        if (name.Equals(from.Name))
        {
            fieldInfoFrom = fieldInfo;
        }
        return fieldInfoFrom;
    }

    private static bool CopyField(Component component, FieldInfo fieldInfoTo, FieldInfo fieldInfoFrom)
    {
        if (fieldInfoTo != null && fieldInfoFrom != null)
        {
            Undo.RecordObject(component, string.Format(UndoContent, component.name));
            object fromValue = fieldInfoFrom.GetValue(component);

            fieldInfoTo.SetValue(component, fromValue);
            return true;
        }
        return false;
    }
    #endregion

    private static void DeselectEverythingExcept(MemberInfo selectedInfo, Dictionary<MemberInfo, bool> dictionary)
    {
        DeselectEverything(dictionary);

        dictionary[selectedInfo] = true;
    }

    private static void DeselectEverything(Dictionary<MemberInfo, bool> dictionary)
    {
        List<MemberInfo> infos = new List<MemberInfo>(dictionary.Count);
        infos.AddRange(dictionary.Select(item => item.Key));

        foreach (MemberInfo info in infos)
        {
            dictionary[info] = false;
        }
    }

    private static void CreateSelectedLabel(Rect windowPos)
    {
        GUI.Label(new Rect(Inset, windowPos.height - 22.5f, windowPos.width / 2, 20f), string.Format(SelectedContent, selectedFromInfo.Name, selectedFromInfo.FieldType.Name));
    }

    public enum CopyOption
    {
        Hierarchy,
        Project,
        Both
    }
}
