using System;
using System.Collections.Generic;
using System.Linq;
using Ese;
using UnityEditor;
using UnityEngine;

public class RearrangeScriptEditorWindow : ScriptableWizard
{
    // Make this window appear in the Window tab.
    private const string MenuName = "Window/ESE Toolbox/" + WindowName + ShortCutKey;
    // The window name, used in both the menu and the titlebar.
    private const string WindowName = "Rearrange Scripts";
    // The shortcut key command: # = ctrl, % = shift
    private const string ShortCutKey = "";
    // The priority of the menu command
    private const int Priority = 3;
    // The about message
    private const string AboutContentMessage = "This tool helps you reorganize the position of the scripts on a selected gameobject.";
    // The title
    private const string Title = "1. Select a gameobject and press the buttons on the right to rearrange script order.";

    private const int NameLabelWidth = 300;
    private const int ButtonWidth = 60;

    private List<Type> illegalComponents = null;

    /// <summary>
    /// This method will be called when the window is opened.
    /// </summary>
    [MenuItem(MenuName, false, Priority)]
    public static void Init()
    {
        RearrangeScriptEditorWindow wizard = DisplayWizard<RearrangeScriptEditorWindow>(WindowName);

        const float minWidth = 20 + NameLabelWidth + ButtonWidth * 4;
        float minHeight = 200;
        List<Component> components = GetValidComponents(GetIllegalComponents());
        if (!components.IsNullOrEmpty() && components.Count < 10)
        {
            minHeight = 65 + 20 * components.Count;
        }
        
        wizard.minSize = new Vector2(minWidth, minHeight);

        ResetAllVariables();
    }

    [MenuItem(MenuName, true, Priority)]
    public static bool Validate()
    {
        List<Component> components = GetValidComponents(GetIllegalComponents());

        return !components.IsNullOrEmpty();
    }

    private static List<Component> GetValidComponents(List<Type> illegalComponents)
    {
        GameObject selectedGameObject = Selection.activeGameObject;
        if (selectedGameObject != null)
        {
            return RemoveIllegalComponents(GetCustomComponents(selectedGameObject), illegalComponents);
        }
        return new List<Component>();
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
    {}

    internal enum Choice
    {
        None,
        Top,
        Up,
        Down,
        Bottom
    }

    /// <summary>
    /// Draws the editor window.
    /// </summary>
    public void OnGUI()
    {
        if (illegalComponents.IsNullOrEmpty())
        {
            illegalComponents = GetIllegalComponents();
        }
        List<Component> components = GetValidComponents(illegalComponents);

        if (!components.IsNullOrEmpty())
        {
            ESEGUI.TitleBox(new Rect(0, 0, position.width, components.Count * 20 + 40), Title);
            int index = 0;
            Choice choice = Choice.None;
            Component componentToMove = null;
            foreach (Component component in components)
            {
                EditorGUI.InspectorTitlebar(new Rect(10, 30 + 20 * index, NameLabelWidth, 20), false, component);
                componentToMove = component;
                choice = DrawButtons(index++, (components.Count - 1), position);
                if(choice != Choice.None)
                {
                    break;
                }
            }

            if (componentToMove != null)
            {
                MoveComponent(componentToMove, components, choice, Selection.activeGameObject);
            }
        }

        CreateAbout(position);
    }

    private static List<Component> RemoveIllegalComponents(List<Component> components, List<Type> illegalComponents)
    {
        List<Component> cleanComponents = new List<Component>();
        foreach(Component component in components)
        {
            Type componentType = component.GetType();
            if (IsComponentValid(componentType, illegalComponents))
            {
                cleanComponents.Add(component);
            }
        }
        return cleanComponents;
    }

    private static bool IsComponentValid(Type componentType, List<Type> illegalComponents)
    {
        foreach(Type illegalComponentType in illegalComponents)
        {
            if (componentType == illegalComponentType || componentType.IsSubclassOf(illegalComponentType))
            {
                return false;
            }
        }
        return true;
    }

    private static Choice DrawButtons(int index, int componentCount, Rect position)
    {
        Choice choice = Choice.None;
        float yPosition = 30 + 20 * index;
        float xPosition = position.width - (ButtonWidth * 4) - 10;

        if (ESEGUI.Button(new Rect(xPosition, yPosition, ButtonWidth, 20), "Up", "Move component up", index > 0))
        {
            choice = Choice.Up;
        }
        xPosition += ButtonWidth;
        if (ESEGUI.Button(new Rect(xPosition, yPosition, ButtonWidth, 20), "Down", "Move component down", index < componentCount))
        {
            choice = Choice.Down;
        }
        xPosition += ButtonWidth;
        if (ESEGUI.Button(new Rect(xPosition, yPosition, ButtonWidth, 20), "Bottom", "Move component to the bottom", index < componentCount))
        {
            choice = Choice.Bottom;
        }
        xPosition += ButtonWidth;
        if (ESEGUI.Button(new Rect(xPosition, yPosition, ButtonWidth, 20), "Top", "Move component to the top", index > 0))
        {
            choice = Choice.Top;
        }
        return choice;
    }

    private void MoveComponent(Component componentToMove, List<Component> components, Choice choice, GameObject selectedGameObject)
    {
        switch (choice)
        {
            case Choice.None:
                break;
            case Choice.Top:
                MoveComponentToTop(selectedGameObject, componentToMove, components);
                break;
            case Choice.Up:
                MoveComponentUp(selectedGameObject, componentToMove, components);
                break;
            case Choice.Down:
                MoveComponentDown(selectedGameObject, componentToMove, components);
                break;
            case Choice.Bottom:
                MoveComponentToBottom(selectedGameObject, componentToMove, components);
                break;
        }
    }

    private void MoveComponentToBottom(GameObject selectedGameObject, Component componentToMove, List<Component> components)
    {
        components.Remove(componentToMove);
        components.Add(componentToMove);
        RearrangeComponents(selectedGameObject, components);
    }

    private void MoveComponentDown(GameObject selectedGameObject, Component componentToMove, List<Component> components)
    {
        int moveDownIndex = components.FindIndex(compo => compo == componentToMove);
        if (moveDownIndex < components.Count - 1)
        {
            components.Remove(componentToMove);
            components.Insert(moveDownIndex + 1, componentToMove);

            RearrangeComponents(selectedGameObject, components);
        }
    }

    private void MoveComponentUp(GameObject selectedGameObject, Component componentToMove, List<Component> components)
    {
        int moveUpIndex = components.FindIndex(compo => compo == componentToMove);
        if (moveUpIndex > 0)
        {
            components.Remove(componentToMove);
            components.Insert(moveUpIndex - 1, componentToMove);
            RearrangeComponents(selectedGameObject, components);
        }
    }

    private void MoveComponentToTop(GameObject selectedGameObject, Component componentToMove, List<Component> components)
    {
        components.Remove(componentToMove);
        components.Insert(0, componentToMove);
        RearrangeComponents(selectedGameObject, components);
    }

    private static void RearrangeComponents(GameObject selectedGameObject, List<Component> components)
    {
        foreach (Component component in components)
        {
            Component addedComponent = selectedGameObject.AddComponent(component.GetType());
            if (addedComponent != null)
            {
                PasteComponentsMenuItem.CopyComponent(component, addedComponent);
                DestroyImmediate(component);
            }
        }
    }

    private static List<Type> GetIllegalComponents()
    {
        List<Type> illegalTypes = new List<Type>
                                      {
                                          typeof (Animation),
                                          typeof (AudioListener),
                                          typeof (Camera),
                                          typeof (Collider),
                                          typeof (ConstantForce),
                                          typeof (GUIText),
                                          typeof (GUITexture),
                                          typeof (HingeJoint),
                                          typeof (Light),
                                          typeof (NetworkView),
                                          typeof (ParticleEmitter),
                                          typeof (Renderer),
                                          typeof (Rigidbody),
                                          typeof (Transform),
                                          typeof (GUILayer),
                                          typeof (MeshFilter)
                                      };
        return illegalTypes;
    }

    private static List<Component> GetCustomComponents(GameObject gameObject)
    {
        return gameObject.GetComponents<Component>().ToList();
    }

    private static void CreateAbout(Rect windowPos)
    {
        Rect buttonRect = new Rect(windowPos.width - 105, windowPos.height - 25, 100, 20);
        ESEGUI.AboutDialogButton(buttonRect, WindowName, AboutContentMessage);
    }
}