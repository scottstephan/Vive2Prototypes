using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class ViewComponentMappingWindow : EditorWindow
{
    bool excludeUnityEngineClasses = false;
    Vector2 scrollPos = new Vector2();
    SortedDictionary<string, List<GameObject>> mappingSearchResult = new SortedDictionary<string, List<GameObject>>();
    Dictionary<string, bool> foldoutStatus = new Dictionary<string, bool>();

    [MenuItem("WEMOTools/View component mapping")]
    static void ShowWindow()
    {
//        ViewComponentMappingWindow window = (ViewComponentMappingWindow)EditorWindow.GetWindow(typeof(ViewComponentMappingWindow));
        EditorWindow.GetWindow(typeof(ViewComponentMappingWindow));
    }

    void OnGUI()
    {
        RenderHeader();

        AddVerticalSpace(30);

        RenderSearchForm();

        if (mappingSearchResult.Count > 0)
        {
            RenderSearchResult();
        }
    }

    void OnHierarchyChange()
    {
        ResetState();
    }

    void RenderHeader()
    {
        GUILayout.Label("VIEW COMPONENT MAPPING TOOL", EditorStyles.toolbarButton);
        GUILayout.Label("This tool allows you to monitor which components are being attached to which scene objects. Usage:\n1. Click \"Scan scene\".\n2. Select your interested components from returned list.\n3. Click on a button will select associated game object in scene hierarchy.", EditorStyles.wordWrappedMiniLabel);
    }

    void RenderSearchForm()
    {
        GUILayout.Label("Search options", EditorStyles.boldLabel);
		EditorGUIUtility.labelWidth = 200f;
		EditorGUIUtility.fieldWidth = 0f;
        excludeUnityEngineClasses = EditorGUILayout.Toggle("Exclude UnityEngine types", excludeUnityEngineClasses);
		EditorGUIUtility.labelWidth = 0f;
		EditorGUIUtility.fieldWidth = 0f;

        AddVerticalSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Scan scene", GUILayout.Width(100)))
        {
            ScanScene();
        }
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
    }

    void ScanScene()
    {
        ResetState();
        Object[] gos = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject go in gos)
        {
            Component[] components = go.GetComponents(typeof(Component));
            foreach (Component com in components)
            {
                System.Type type = com.GetType();
                string strType = type.ToString();
                if (!excludeUnityEngineClasses || (excludeUnityEngineClasses && type.Namespace != "UnityEngine"))
                {
                    if (!mappingSearchResult.ContainsKey(strType))
                    {
                        mappingSearchResult.Add(strType, new List<GameObject>());
                        foldoutStatus.Add(strType, false);
                    }

                    mappingSearchResult[strType].Add(go);
                }
            }
        }
    }

    void ResetState()
    {
        mappingSearchResult = new SortedDictionary<string, List<GameObject>>();
        foldoutStatus = new Dictionary<string, bool>();
    }

    void RenderSearchResult()
    {
        GUILayout.Label("Search result:", EditorStyles.boldLabel);
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.BeginVertical();
        foreach (KeyValuePair<string, List<GameObject>> pair in mappingSearchResult)
        {
            foldoutStatus[pair.Key] = EditorGUILayout.Foldout(foldoutStatus[pair.Key], pair.Key.ToString());

            if (foldoutStatus[pair.Key])
            {
                GUILayout.Label("is being attached to the following object(s):");
                GUILayout.BeginHorizontal();
                float rowTotalWidth = 0;
                float buttonWidth = 200;
                float buttonGap = 10;
                foreach (GameObject go in pair.Value)
                {
                    if (GUILayout.Button(go.name, GUILayout.Width(buttonWidth)))
                    {
                        Selection.activeGameObject = go;
                    }
                    GUILayout.Space(buttonGap);

                    rowTotalWidth += buttonWidth + buttonGap;
                    if (rowTotalWidth + buttonWidth + buttonGap > position.width)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        rowTotalWidth = 0;
                    }
                }
                GUILayout.EndHorizontal();

            }
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    void AddVerticalSpace()
    {
        AddVerticalSpace(20);
    }

    void AddVerticalSpace(float space)
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Space(space);
        EditorGUILayout.EndVertical();
    }
}