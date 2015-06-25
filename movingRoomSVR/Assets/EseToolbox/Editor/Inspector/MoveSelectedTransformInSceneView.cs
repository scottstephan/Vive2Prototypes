using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ese
{
	public class MoveSelectedTransformInSceneView
	{
        private const string UndoContent = "Move {0}";

	    private static bool createdCopy = false;

        public static void OnSceneGUI()
        {
            Event current = Event.current;
            if (current.control && current.shift)
            {
                int controlID = GUIUtility.GetControlID(FocusType.Passive);
                HandleUtility.AddDefaultControl(controlID);
                MoveTransformToMousePosition(current, Selection.transforms);
            }
        }

        private static void MoveTransformToMousePosition(Event currentEvent, params Transform[] toMove)
        {
            if (IsMouseClick(currentEvent, 0, UnityEngine.EventType.MouseDown, UnityEngine.EventType.MouseDrag) && !toMove.IsNullOrEmpty())
            {
                MoveSelectedTransforms(currentEvent, toMove);
            }
            else if (IsMouseClick(currentEvent, 1, UnityEngine.EventType.MouseDown) && !toMove.IsNullOrEmpty() && !createdCopy)
            {
                List<Transform> copiedTransforms = new List<Transform>(toMove.Length);
                List<GameObject> copiedGameObjects = new List<GameObject>(toMove.Length);
                CloneObjectsToMove(toMove, copiedGameObjects, copiedTransforms);

                Selection.objects = copiedGameObjects.ToArray();

                if (MoveSelectedTransforms(currentEvent, copiedTransforms.ToArray()))
                {
                    createdCopy = true;
                }
            }
            else if(IsMouseClick(currentEvent, 1, UnityEngine.EventType.MouseDrag) && !toMove.IsNullOrEmpty() && createdCopy)
            {
                MoveSelectedTransforms(currentEvent, toMove);
            }
            else if (IsMouseClick(currentEvent, 1, UnityEngine.EventType.MouseUp))
            {
                createdCopy = false;
            }
        }

	    private static void CloneObjectsToMove(IEnumerable<Transform> toMove, List<GameObject> copiedGameObjects, List<Transform> copiedTransforms)
	    {
	        foreach(Transform transform in toMove)
	        {
	            if (transform != null)
	            {
                    GameObject prefabRoot = UnityEditor.PrefabUtility.GetPrefabParent(transform.gameObject) as GameObject;
                    if(prefabRoot != null)
                    {
                        GameObject go = UnityEditor.PrefabUtility.InstantiatePrefab(prefabRoot) as GameObject;
                        if (go != null)
                        {
                            go.name = transform.name;
                            go.transform.position = transform.position;
                            go.transform.rotation = transform.rotation;
                            copiedGameObjects.Add(go);
                            copiedTransforms.Add(go.transform);
                        }
                    }
	            }
	        }
	    }

	    private static bool MoveSelectedTransforms(Event currentEvent, Transform[] toMove)
        {
            if(!toMove.IsNullOrEmpty())
            {
                Transform firstTransform = Selection.activeTransform;
                if (firstTransform != null)
                {
                    Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    List<RaycastHit> hits = GetTransforms(worldRay, toMove);
                    if (!hits.IsNullOrEmpty())
                    {
                        Vector3 closestPoint = FindClosestHitPoint(hits);
                        Vector3 moveDistance = firstTransform.position - closestPoint;
						Undo.RecordObject(firstTransform, string.Format(UndoContent, firstTransform.name));
                        foreach (Transform transform in toMove)
                        {
                            transform.position -= moveDistance;
                        }
                        currentEvent.Use();
                        return true;
                    }
                }
                else
                {
                    Logger.LogWarning("First transform to move is null.");
                }
            }
            else
            {
                Logger.LogWarning("No transforms to move.");
            }
	        return false;
        }

	    private static List<RaycastHit> GetTransforms(Ray worldRay, IEnumerable<Transform> toMove)
	    {
	        List<RaycastHit> hits = Physics.RaycastAll(worldRay).ToList();
            hits.RemoveAll(hit => toMove.Any(move => !IsValid(move, hit.transform)));
            
	        return hits;
	    }

        private static bool IsValid(Transform move, Transform hit)
        {
            if(move == hit)
            {
                return false;
            }
			int childCount = move.childCount;
            for(int i = 0; i < childCount; i++)
            {
                if(!IsValid(move.GetChild(i), hit))
                {
                    return false;
                }
            }
            return true;
        }

	    private static Vector3 FindClosestHitPoint(IEnumerable<RaycastHit> hits)
        {
            Vector3 currentCameraPosition = Camera.current.transform.position;
            Vector3 closestPoint = Vector3.zero;
            foreach (RaycastHit hit in hits)
            {
                float currentDistance = Vector3.Distance(closestPoint, currentCameraPosition);
                float hitDistance = Vector3.Distance(hit.point, currentCameraPosition);
                if (hitDistance < currentDistance || closestPoint == Vector3.zero)
                {
                    closestPoint = hit.point;
                }
            }
            return closestPoint;
        }

        private static bool IsMouseClick(Event currentEvent, int mouseButton = 0, params UnityEngine.EventType[] eventTypes)
        {
            return currentEvent.isMouse && currentEvent.button == mouseButton && eventTypes.Any(type => type == currentEvent.type);
        }
	}
}
