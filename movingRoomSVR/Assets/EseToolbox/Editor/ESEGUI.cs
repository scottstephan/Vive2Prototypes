using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Utility functions for creating gui objects.
/// </summary>
public static class ESEGUI
{
    private const string AboutDialogTitle = "{0} About";
    private const string AboutButtonContent = "About";
    private const string AboutButtonTooltipContent = "Read more about {0}";
    private const string OKButtonContent = "Ok";

    private const int FieldHeight = 20;
    private const int FieldWidth = 50;
    private const float Inset = 5f;
    


    public static bool Button(Rect buttonRectangle, string title, string tooltip = "", bool valid = true)
    {
        if (valid)
        {
            return GUI.Button(buttonRectangle, new GUIContent(title, tooltip));
        }
        GUI.Box(buttonRectangle, new GUIContent(title, tooltip));
        return false;
    }

    public static Rect TitleBox(Rect titleRect, string title)
    {
        GUI.Box(new Rect(titleRect.xMin + Inset, titleRect.yMin + Inset, titleRect.width - (Inset * 2), titleRect.height - Inset * 2), String.Empty);
        GUI.Label(new Rect(titleRect.xMin + Inset + 2f, titleRect.yMin + Inset, titleRect.width - (Inset * 2) - 4f, (FieldHeight * 2) + 5), title);


        Rect nextStartPosition = new Rect(titleRect);
        nextStartPosition.yMin += FieldHeight + Inset;
        nextStartPosition.xMin += Inset * 2;
        nextStartPosition.width -= Inset * 2;
        nextStartPosition.height -= Inset * 2;
        return nextStartPosition;
    }

    /// <summary>
    /// Displays a button that shows an about dialog.
    /// </summary>
    /// <param name="buttonRect"></param>
    /// <param name="windowName"></param>
    /// <param name="aboutContentMessage"></param>
    /// <returns></returns>
    public static bool AboutDialogButton(Rect buttonRect, string windowName, string aboutContentMessage)
    {
        if (ESEGUI.Button(buttonRect, AboutButtonContent, string.Format(AboutButtonTooltipContent, windowName)))
        {
            return EditorUtility.DisplayDialog(string.Format(AboutDialogTitle, windowName), aboutContentMessage,
                                               OKButtonContent);
        }
        return false;
    }


    #region GameObject List

    public static Vector2 CreateObjectList<T>(Rect listRect, Vector2 scrollPosition, List<T> objects,
                                               string title) where T : Object
    {
        Rect upperLeft = TitleBox(listRect, title);

        Rect viewRect = new Rect(upperLeft);
        viewRect.height = objects.Count * FieldHeight;
        //viewRect.width -= 10f;
        if (viewRect.height > (listRect.height - 27f))
        {
            viewRect.width -= 20f;
        }
        if (listRect.height > 30f)
        {
            scrollPosition = CreateListScrollView(upperLeft, scrollPosition, viewRect);

            Rect fieldRect = new Rect(viewRect);
            fieldRect.height = FieldHeight;
            CreateListItems(fieldRect, objects);

            GUI.EndScrollView();
        }
        return scrollPosition;
    }

    private static Vector2 CreateListScrollView(Rect listRect, Vector2 scrollPosition, Rect viewRect)
    {
        GUI.Box(listRect, String.Empty);
        return GUI.BeginScrollView(listRect, scrollPosition, viewRect);
    }

    private static void CreateListItems<T>(Rect fieldRect, IEnumerable<T> gameObjects) where T : Object
    {
        foreach (T gameObject in gameObjects)
        {
            EditorGUI.ObjectField(fieldRect, gameObject, typeof(T), true);
            fieldRect.yMin += FieldHeight;
            fieldRect.height = FieldHeight;
        }
    }

    #endregion

    #region GameObject Toggle List

    public static Vector2 CreateGameObjectToggleList<T>(Rect listRect, Vector2 scrollPosition, Dictionary<T, bool> gameObjects, string title) where T : Object
    {
        GUI.Box(listRect, String.Empty);

        CreateToggleAllButton(listRect, gameObjects);

        listRect.xMin += 2f;
        GUI.Label(listRect, title);
        listRect.xMin -= 2f;

        scrollPosition = CreateView(listRect, scrollPosition, gameObjects);
        return scrollPosition;
    }

    private static Vector2 CreateView<T>(Rect listRect, Vector2 scrollPosition, Dictionary<T, bool> gameObjects)
        where T : Object
    {
        Rect viewRect = new Rect(listRect);
        viewRect.height = gameObjects.Count * FieldHeight;
        viewRect.width -= 10f;
        if (viewRect.height > (listRect.height - 27f))
        {
            viewRect.width -= 20f;
        }
        if (listRect.height > 30f)
        {
            scrollPosition = CreateListToggleScrollView(listRect, scrollPosition, viewRect);

            Rect fieldRect = new Rect(viewRect);
            fieldRect.height = FieldHeight;
            CreateListToggleItems(fieldRect, gameObjects);

            GUI.EndScrollView();
        }
        return scrollPosition;
    }

    private static void CreateToggleAllButton<T>(Rect listRect, Dictionary<T, bool> gameObjects) where T : Object
    {
        Rect toggleRect = new Rect(listRect);
        toggleRect.xMin += Inset;
        toggleRect.height = FieldHeight;

        Dictionary<T, bool>.ValueCollection values = gameObjects.Values;

        bool isAllSelected = values.All(lul => lul);

        bool shouldAllBeSelected = EditorGUI.Toggle(toggleRect, String.Empty, isAllSelected);

        if (!isAllSelected && shouldAllBeSelected)
        {
            T[] gos = gameObjects.Keys.ToArray();
            foreach (T go in gos)
            {
                gameObjects[go] = true;
            }
        }
        else if (isAllSelected && !shouldAllBeSelected)
        {
            T[] gos = gameObjects.Keys.ToArray();
            foreach (T go in gos)
            {
                gameObjects[go] = false;
            }
        }
    }

    private static Vector2 CreateListToggleScrollView(Rect listRect, Vector2 scrollPosition, Rect viewRect)
    {
        listRect.yMin += 22f;
        listRect.height -= Inset;
        listRect.xMin += Inset;
        listRect.width -= Inset;
        GUI.Box(listRect, String.Empty);
        return GUI.BeginScrollView(listRect, scrollPosition, viewRect);
    }

    private static void CreateListToggleItems<T>(Rect fieldRect, Dictionary<T, bool> gameObjects) where T : Object
    {
        List<T> selected = new List<T>();
        List<T> unselected = new List<T>();
        foreach (KeyValuePair<T, bool> gameObject in gameObjects)
        {
            bool wasThere = gameObject.Value;
            bool shouldBeThere = EditorGUI.Toggle(fieldRect, gameObject.Key.name, wasThere);

            if (shouldBeThere && !wasThere)
            {
                selected.Add(gameObject.Key);
            }
            else if (wasThere && !shouldBeThere)
            {
                unselected.Add(gameObject.Key);
            }

            fieldRect.yMin += FieldHeight;
            fieldRect.height = FieldHeight;
        }

        foreach (T select in selected)
        {
            gameObjects[@select] = true;
        }

        foreach (T select in unselected)
        {
            gameObjects[@select] = false;
        }
    }

    #endregion
}
