//#define DEBUG

using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using Ese;

public class PasteComponentsMenuItem
{
    private const string PasteComponentsContent = "Paste Components";
    private const int Priority = 32;

    [MenuItem("GameObject/" + PasteComponentsContent, true, Priority)]
    [MenuItem("CONTEXT/Transform/" + PasteComponentsContent, true, Priority)]
    public static bool ValidatePaste()
    {
        if (CopyComponentsMenuItem.CopiedComponents.IsNullOrEmpty())
        {
            return false;
        }
        return true;
    }

    [MenuItem("GameObject/" + PasteComponentsContent, false, Priority)]
    [MenuItem("CONTEXT/Transform/" + PasteComponentsContent, false, Priority)]
    public static void Paste(MenuCommand command)
    {
        Transform commandContext = command.context as Transform;
        if (commandContext != null)
        {
            PasteComponent(commandContext);
        }
        else
        {
            Transform activeTransform = Selection.activeTransform;
            if (activeTransform != null)
            {
                PasteComponent(activeTransform);
            }
        }
    }

    public static void PasteComponent(Transform activeGameObject)
    {
        if (!CopyComponentsMenuItem.CopiedComponents.IsNullOrEmpty())
        {
            foreach (Component sourceComponent in CopyComponentsMenuItem.CopiedComponents)
            {
                Component targetComponent = activeGameObject.gameObject.AddComponent(sourceComponent.GetType());
                if (targetComponent != null)
                {
                    CopyComponent(sourceComponent, targetComponent);
                }
            }
        }
        else
        {
            Logger.LogWarning("There was nothing to copy");
        }
    }

    public static void CopyComponent(Component sourceComponent, Component targetComponent)
    {
        Type targetType = targetComponent.GetType();
        List<PropertyInfo> targetProperties = new List<PropertyInfo>();
        List<FieldInfo> targetFields = new List<FieldInfo>();

        Type sourceType = sourceComponent.GetType();
        PropertyInfo[] properties = sourceType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo sourceProperty in properties)
        {
            if (!sourceProperty.CanRead)
            {
                continue;
            }
            PropertyInfo targetProperty = targetType.GetProperty(sourceProperty.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (targetProperty == null)
            {
                continue;
            }
#if DEBUG
            DebugLog(sourceType + ", checking sourceProperty: " + sourceProperty.Name);
#endif
            if (!targetProperty.CanWrite)
            {
                continue;
            }
#if DEBUG
            DebugLog(sourceType + ", checking sourceProperty: " + sourceProperty.Name);
#endif


#if DEBUG
            DebugLog(sourceType + ", targetProperty.GetSetMethod(): " + targetProperty.GetSetMethod());
#endif
            if (targetProperty.GetSetMethod() != null && (targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
            {
                continue;
            }
            if (!targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
            {
                continue;
            }
            if (targetProperty.Name.Equals("active"))
            {
                continue;
            }
            if (targetProperty.Name.Equals("tag"))
            {
                continue;
            }
            if (targetProperty.Name.Equals("name"))
            {
                continue;
            }
            if (targetProperty.Name.Equals("hideFlags"))
            {
                continue;
            }
#if DEBUG
            DebugLog(sourceType + ", adding property: " + sourceProperty.Name);
#endif
            targetProperties.Add(targetProperty);
        }

        FieldInfo[] fields = sourceType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo fieldInfo in fields)
        {
#if DEBUG
            DebugLog(sourceType + ", checking targetField: " + fieldInfo);
#endif
            FieldInfo targetField = targetType.GetField(fieldInfo.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (targetField == null)
            {
#if DEBUG
                DebugLog(sourceType + ", targetfield is null");
#endif
                continue;
            }

            if ((targetField.Attributes & FieldAttributes.Static) != 0)
            {
#if DEBUG
                DebugLog(sourceType + ", static");
#endif
                continue;
            }
            if (!targetField.FieldType.IsAssignableFrom(fieldInfo.FieldType))
            {
#if DEBUG
                DebugLog(sourceType + ", assignable from fieldtype");
#endif
                continue;
            }
            if (targetField.Name.Equals("active"))
            {
                continue;
            }
            if (targetField.Name.Equals("tag"))
            {
                continue;
            }
            if (targetField.Name.Equals("name"))
            {
                continue;
            }
            if (targetField.Name.Equals("hideFlags"))
            {
                continue;
            }
#if DEBUG
            DebugLog(sourceType + ", adding field: " + targetField.Name);
#endif

            targetFields.Add(targetField);
        }

        for (int propertyCount = 0; propertyCount < targetProperties.Count; propertyCount++)
        {
            targetProperties[propertyCount].SetValue(targetComponent, targetProperties[propertyCount].GetValue(sourceComponent, null),
                                                     null);
        }

        for (int fieldCount = 0; fieldCount < targetFields.Count; fieldCount++)
        {
            targetFields[fieldCount].SetValue(targetComponent, targetFields[fieldCount].GetValue(sourceComponent));
        }
    }

#if DEBUG

    private static void DebugLog(string debug)
    {
        if(debug.ToLower().Contains("enemybehaviour"))
        {
            Debug.Log(debug);
        }
    }
#endif
}
