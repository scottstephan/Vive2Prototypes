using System.Collections.Generic;
using System.Linq;
using Ese;
using UnityEditor;
using UnityEngine;

public static class EditorWindowUtility
{
    private const string AboutButtonContent = "About";
    private const string AboutDialogTitle = "{0} About";
    private const string OKButtonContent = "Ok";
    public const float Inset = 5f;
    public const float FieldHeight = 20f;
    public const float FieldWidth = 200f;

    /// <summary>
    /// Displays a button that shows an about dialog.
    /// </summary>
    /// <param name="buttonRect"></param>
    /// <param name="windowName"></param>
    /// <param name="aboutContentMessage"></param>
    /// <returns></returns>
    public static bool AboutDialogButton(Rect buttonRect, string windowName, string aboutContentMessage)
    {
        if (GUI.Button(buttonRect, AboutButtonContent))
        {
            return EditorUtility.DisplayDialog(string.Format(AboutDialogTitle, windowName), aboutContentMessage,
                                               OKButtonContent);
        }
        return false;
    }

    

    

    #region GameObject List

    public static Vector2 CreateGameObjectList<T>(Rect listRect, Vector2 scrollPosition, List<T> gameObjects,
                                               string title) where T : UnityEngine.Object
    {
        GUI.Box(listRect, string.Empty);
        listRect.xMin += 2f;
        GUI.Label(listRect, title);
        listRect.xMin -= 2f;

        Rect viewRect = new Rect(listRect);
        viewRect.height = gameObjects.Count * FieldHeight;
        viewRect.width -= 10f;
        if (viewRect.height > (listRect.height - 27f))
        {
            viewRect.width -= 20f;
        }
        if (listRect.height > 30f)
        {
            scrollPosition = CreateListScrollView(listRect, scrollPosition, viewRect);

            Rect fieldRect = new Rect(viewRect);
            fieldRect.height = FieldHeight;
            CreateListItems(fieldRect, gameObjects);

            GUI.EndScrollView();
        }
        return scrollPosition;
    }

    private static Vector2 CreateListScrollView(Rect listRect, Vector2 scrollPosition, Rect viewRect)
    {
        listRect.yMin += 22f;
        listRect.height -= Inset;
        listRect.xMin += Inset;
        listRect.width -= Inset;
        GUI.Box(listRect, string.Empty);
        return GUI.BeginScrollView(listRect, scrollPosition, viewRect);
    }

    private static void CreateListItems<T>(Rect fieldRect, IEnumerable<T> gameObjects) where T : UnityEngine.Object
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

    public static Vector2 CreateGameObjectToggleList<T>(Rect listRect, Vector2 scrollPosition, Dictionary<T, bool> gameObjects, string title) where T : UnityEngine.Object
    {
        GUI.Box(listRect, string.Empty);

        CreateToggleAllButton(listRect, gameObjects);

        listRect.xMin += 2f;
        GUI.Label(listRect, title);
        listRect.xMin -= 2f;

        scrollPosition = CreateView(listRect, scrollPosition, gameObjects);
        return scrollPosition;
    }

    private static Vector2 CreateView<T>(Rect listRect, Vector2 scrollPosition, Dictionary<T, bool> gameObjects)
        where T : UnityEngine.Object
    {
        Rect viewRect = new Rect(listRect);
        viewRect.height = gameObjects.Count*FieldHeight;
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

    private static void CreateToggleAllButton<T>(Rect listRect, Dictionary<T, bool> gameObjects) where T : UnityEngine.Object
    {
        Rect toggleRect = new Rect(listRect);
        toggleRect.xMin += Inset;
        toggleRect.height = FieldHeight;

        Dictionary<T, bool>.ValueCollection values = gameObjects.Values;

        bool isAllSelected = values.All(lul => lul);

        bool shouldAllBeSelected = EditorGUI.Toggle(toggleRect, string.Empty, isAllSelected);

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
        GUI.Box(listRect, string.Empty);
        return GUI.BeginScrollView(listRect, scrollPosition, viewRect);
    }

    private static void CreateListToggleItems<T>(Rect fieldRect, Dictionary<T, bool> gameObjects) where T : UnityEngine.Object
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
            else if(wasThere && !shouldBeThere)
            {
                unselected.Add(gameObject.Key);
            }

            fieldRect.yMin += FieldHeight;
            fieldRect.height = FieldHeight;
        }

        foreach(T select in selected)
        {
            gameObjects[select] = true;
        }

        foreach (T select in unselected)
        {
            gameObjects[select] = false;
        }
    }

    #endregion


}
