using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ese
{
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
    ///     23rd of August, 2011
    /// 
    /// Purpose:
    ///     A utility that searches for references.
    /// </summary>
	public class ReferenceUtility
	{
        /// <summary>
        /// Finds objects that references this gameobject in the scene.
        /// </summary>
        /// <param name="gameObject">the gameobject to check</param>
        /// <returns></returns>
        public static List<Object> GetProjectReferences(GameObject gameObject)
        {
            //1. Retreive all the gameobjects in the project.
            List<GameObject> prefabs = PrefabUtility.GetPrefabs();

            return FilterReferences(gameObject, prefabs);
        }

        /// <summary>
        /// Finds objects that references this gameobject in the project.
        /// </summary>
        /// <param name="gameObject">the gameobject to check</param>
        /// <returns></returns>
        public static List<Object> GetHierarchyReferences(GameObject gameObject)
        {
            //1. Retreive all the gameobjects in the hierarchy.
            Object[] currentPotentialReferences = Object.FindObjectsOfType(typeof (GameObject));

            return FilterReferences(gameObject, currentPotentialReferences.OfType<GameObject>());
        }

        private static List<Object> FilterReferences(GameObject targetGameObject, IEnumerable<GameObject> gameObjectsToCheck)
        {
            List<Object> filtered = new List<Object>();
            //DebugReferenceUtility("retreiving references for: " + targetGameObject + " from " + gameObjectsToCheck.Count() + " gameObjects", targetGameObject);

            if (targetGameObject != null && gameObjectsToCheck != null && gameObjectsToCheck.Count() > 0)
            {
                // 2. Retreive all the gameobjects on the target gameobject.
                MonoBehaviour[] behaviours = targetGameObject.GetComponents<MonoBehaviour>();

                //DebugReferenceUtility("found " + behaviours.Length + " behaviours");
                // 3. Go through each gameobject.
                foreach (GameObject gameObjectToCheck in gameObjectsToCheck)
                {
                    CheckGameObject(targetGameObject, behaviours, gameObjectToCheck, filtered);
                }
            }
            return filtered;
        }

        private static void CheckGameObject(GameObject targetGameObject, MonoBehaviour[] behaviours, GameObject gameObjectToCheck, List<Object> filtered)
        {
            if (gameObjectToCheck != null)
            {
                // 4. Retreive all the scripts on the gameobject.
                MonoBehaviour[] behavioursToCheck = gameObjectToCheck.GetComponents<MonoBehaviour>();
                if (behavioursToCheck != null && behavioursToCheck.Length > 0)
                {
                    List<object> hasBeenChecked = new List<object>();
                    //DebugReferenceUtility("actually checking : " + behavioursToCheck.Length + " behaviours on this one", gameObjectToCheck);
                    CheckComponents(targetGameObject, behaviours, behavioursToCheck, filtered, hasBeenChecked);
                }
            }
        }

        private static void CheckComponents(GameObject targetGameObject, MonoBehaviour[] behaviours, MonoBehaviour[] triggerComponents, List<Object> filtered, List<object> hasBeenChecked)
	    {
            // 5. Loop the scripts and retreive all the variables.
            foreach (MonoBehaviour component in triggerComponents)
            {
                if (component != null && !hasBeenChecked.Contains(component))
                {
                    hasBeenChecked.Add(component);
                    if (IsComponentValid(targetGameObject, behaviours, component, hasBeenChecked))
                    {
                        //DebugReferenceUtility(component + " on " + component.gameObject + " is valid, so adding to filtered");

                        filtered.Add(component);
                    }
                }
	        }
	    }

        private static bool IsComponentValid(GameObject targetGameObject, MonoBehaviour[] behaviours, MonoBehaviour componentToCheck, List<object> hasBeenChecked)
        {
            Type componentType = componentToCheck.GetType();
            FieldInfo[] fields = GetFields(componentType);

            if (!fields.IsNullOrEmpty())
            {
                //DebugReferenceUtility(componentToCheck + ", has " + fields.Length + " members");
                //6. Send the variables in to a method that checks the type
                if (AreAnyFieldsValid(targetGameObject, behaviours, componentToCheck, fields, hasBeenChecked))
                {
                    return true;
                }
            }
            return false;
        }

        private static FieldInfo[] GetFields(Type objectType)
        {
            return objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        }

        private static bool AreAnyFieldsValid(GameObject targetGameObject, MonoBehaviour[] behaviours, object componentToCheck, FieldInfo[] members, List<object> hasBeenChecked)
        {
            foreach (FieldInfo fieldInfo in members)
            {
                if (IsFieldValid(targetGameObject, behaviours, componentToCheck, fieldInfo, hasBeenChecked))
                {
                    //DebugReferenceUtility(componentToCheck + ", field:  " + fieldInfo + " and it was valid");
                    return true;
                }
                //DebugReferenceUtility(componentToCheck + ", field:  " + fieldInfo + " and it was NOT valid");
            }
            return false;
        }

        private static bool IsFieldValid(GameObject targetGameObject, MonoBehaviour[] behaviours, object componentToCheck, FieldInfo fieldInfo, List<object> hasBeenChecked)
        {
            if (fieldInfo != null && componentToCheck != null)
            {
                Type fieldType = fieldInfo.FieldType;

                

                Type fieldToCheck = fieldType;
                if(fieldType.IsArray)
                {
                    
                    fieldToCheck = fieldType.GetElementType();
                    //DebugReferenceUtility(componentToCheck + ",IsFieldValid, "+ fieldType + " is an array of " + fieldToCheck);
                }

                //DebugReferenceUtility(componentToCheck + ",IsFieldValid, " + fieldToCheck + " a valid type: " + IsFieldTypeValid(fieldInfo, fieldToCheck));

                if (IsFieldTypeValid(fieldInfo, fieldToCheck))
                {
                    if (fieldType.IsArray)
                    {
                        //DebugReferenceUtility(componentToCheck + ",IsFieldValid, " + fieldType + " is an array");
                        return IsValidArray(targetGameObject, behaviours, componentToCheck, fieldInfo, hasBeenChecked);
                    }
                    object value = fieldInfo.GetValue(componentToCheck);
                    //DebugReferenceUtility(componentToCheck + ",IsFieldValid, value: " + value + " from " + fieldInfo);
                    return CheckFieldRecursively(targetGameObject, behaviours, componentToCheck, value, hasBeenChecked);  
                }
            }

            //DebugReferenceUtility(componentToCheck + ",IsFieldValid, " + fieldInfo + " was not valid");
            return false;
        }

        private static bool IsFieldTypeValid(FieldInfo fieldInfo, Type fieldType)
        {
            return fieldType.IsClass &&
                !fieldInfo.IsStatic && 
                !fieldType.IsPrimitive && 
                !fieldType.IsAbstract &&
                !fieldType.IsEnum &&
                !fieldType.IsGenericType &&
                !fieldType.IsValueType &&
                !fieldType.Equals(typeof(String));
        }

        private static bool IsValidArray(GameObject targetGameObject, MonoBehaviour[] behaviours, object componentToCheck, FieldInfo fieldInfo, List<object> hasBeenChecked)
        {
            Array array = fieldInfo.GetValue(componentToCheck) as Array;

            if (!array.IsNullOrEmpty() && CheckArrayObjectsRecursively(targetGameObject, behaviours, componentToCheck, array, hasBeenChecked))
            {
                //DebugReferenceUtility(componentToCheck + ", " + array + " array was valid");
                return true;
            }
            //DebugReferenceUtility(componentToCheck + ", " + array + " array was NOT valid");
            return false;
        }

        private static bool CheckArrayObjectsRecursively(GameObject targetGameObject, MonoBehaviour[] behaviours, object componentToCheck, Array array, List<object> hasBeenChecked)
        {
            foreach (object obj in array)
            {
                //DebugReferenceUtility(componentToCheck + ", checking array " + array + " element: " + obj + " : " + (obj != null && !hasBeenChecked.Contains(obj)));
                if (CheckFieldRecursively(targetGameObject, behaviours, componentToCheck, obj, hasBeenChecked))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckFieldRecursively(GameObject targetGameObject, MonoBehaviour[] behaviours, object componentToCheck, object obj, List<object> hasBeenChecked)
        {
            if (obj != null && componentToCheck != null && !hasBeenChecked.Contains(obj))
            {
                hasBeenChecked.Add(obj);
                if (IsValidFieldType(targetGameObject, behaviours, componentToCheck, obj))
                {
                    //DebugReferenceUtility(componentToCheck + ", CheckFieldRecursively " + obj + " is valid");
                    return true;
                }
                else
                {
                    //DebugReferenceUtility(componentToCheck + ", CheckFieldRecursively " + obj + " is NOT valid");
                }

                FieldInfo[] members = GetFields(obj.GetType());

                return AreAnyFieldsValid(targetGameObject, behaviours, obj, members, hasBeenChecked);
            }
            return false;
        }

        private static bool IsValidFieldType(GameObject targetGameObject, MonoBehaviour[] behaviours, object componentToCheck, object objToCheck)
        {
            Type fieldType = objToCheck.GetType();
            //DebugReferenceUtility(componentToCheck + ", checking " + objToCheck + " of type: " + fieldType);
            if (fieldType == typeof(GameObject) || fieldType == typeof(Object))
            {
                GameObject outgoing = objToCheck as GameObject;
                if (outgoing != null)
                {
                    //DebugReferenceUtility(componentToCheck + ", fieldtype was GameObject or Object: " + outgoing);
                    return IsGameObjectTheSame(targetGameObject, outgoing);
                }
            }

            if(IsFieldTypeMonoBehaviour(objToCheck, fieldType, targetGameObject, behaviours, componentToCheck))
            {
                return true;
            }

            if (IsFieldTypeComponent(objToCheck, fieldType, targetGameObject))
            {
                return true;
            }
            //DebugReferenceUtility(componentToCheck + ", " + objToCheck + " was not valid");
            return false;
        }

        private static bool IsFieldTypeComponent(object objToCheck, Type fieldType, GameObject targetGameObject)
        {
            if (fieldType.IsSubclassOf(typeof(Component)))
            {
                Component outgoing = objToCheck as Component;
                if (outgoing != null && targetGameObject != null)
                {
                    //DebugReferenceUtility("fieldtype is component: " + outgoing);
                    if(IsGameObjectTheSame(targetGameObject, outgoing.gameObject))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsGameObjectTheSame(GameObject targetGameObject, GameObject outgoing)
        {
            if (outgoing != null && targetGameObject == outgoing)
            {
                return true;
            }
            return false;
        }

        private static bool IsFieldTypeMonoBehaviour(object objToCheck, Type fieldType, GameObject targetGameObject, MonoBehaviour[] behaviours, object componentToCheck)
        {
            if (fieldType.IsSubclassOf(typeof(MonoBehaviour)))
            {
                MonoBehaviour monoBehaviour = behaviours.FirstOrDefault(behaviour => behaviour != null && behaviour.GetType() == fieldType);
                MonoBehaviour behaviourToCheck = componentToCheck as MonoBehaviour;
                if (behaviourToCheck != null && monoBehaviour != null)
                {
                    //DebugReferenceUtility(componentToCheck + ",fieldtype is subclass of monobehaviour and could be found in our list: " + monoBehaviour);
                    return IsCorrectMonoBehaviour(objToCheck, targetGameObject, monoBehaviour);
                }
            }
            return false;
        }

        private static bool IsCorrectMonoBehaviour(object objToCheck, GameObject targetGameObject, MonoBehaviour monoBehaviour)
        {
            if (objToCheck != null && monoBehaviour != null) // The object we are checking on is a monobehaviour that is on the selected gameobject
            {
                MonoBehaviour monoBehaviourToCheck = objToCheck as MonoBehaviour;
                
                if (monoBehaviourToCheck != null)
                {
                    //DebugReferenceUtility(targetGameObject + " is possibly " + monoBehaviourToCheck + " (objToCheck: " + objToCheck + ")", monoBehaviour);
                    if (IsGameObjectTheSame(targetGameObject, monoBehaviourToCheck.gameObject))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /*private static void DebugReferenceUtility(string debug, Object context = null)
        {
            string keyWord = "";
            if(debug.Contains(keyWord))
            {
                Debug.Log(debug, context);
            }
        }*/
	}
}
