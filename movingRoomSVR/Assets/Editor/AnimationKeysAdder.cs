using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
public class AnimationKeysAdder : ScriptableWizard {

	public GameObject[] toBeAddedKeysPrefabs;
	[MenuItem("WEMOTools/Animation Key Adding Helper")]
	static AnimationKeysAdder CreateWizard()
	{
		return ScriptableWizard.DisplayWizard("Animation Key Adding Helper", typeof(AnimationKeysAdder), "Add") as AnimationKeysAdder;
	}

	void OnWizardUpdate()
	{
		helpString = "Step:\n" +
			"1. Drag prefab(s) with animation that is not keyed into the Hierarchy pane.\n" +
			"2. Drag the prefab(s) you just instantiated in the seem to To Be Added Keys Prefab\n" +
			"3. Click Add\n" +
			"If this tool messes up our animations, please let Yifu know ASAP.";
	}
	void OnWizardCreate()
	{
		foreach(GameObject toBeAddedKeysPrefab in toBeAddedKeysPrefabs)
		{
		//GameObject body = (toBeAddedKeysPrefab.transform.FindChild("root").FindChild("body")).gameObject;
			GameObject root = toBeAddedKeysPrefab.transform.FindChild("root").gameObject;
			string[] clipsInPrefab = GetAnimationClipsNames(toBeAddedKeysPrefab.GetComponent<Animation>());
			Animation animationComponent = toBeAddedKeysPrefab.GetComponent<Animation>();
			foreach(string clipInPrefab in clipsInPrefab)
			{
				Keyframe key = new Keyframe(0,0);
				float endTime = animationComponent.GetClip(clipInPrefab).length;
				Keyframe endKey = new Keyframe(endTime,0);
				List<Keyframe> keys = new List<Keyframe>();
				keys.Add(key);
				keys.Add(endKey);
				AnimationCurve curve = new AnimationCurve(keys.ToArray());
				Keyframe scaleStartKey = new Keyframe(0,1);
				Keyframe scaleEndKey = new Keyframe(endTime,1);
				keys.Clear();
				keys.Add(scaleStartKey);
				keys.Add(scaleEndKey);
				AnimationCurve scaleCurve = new AnimationCurve(keys.ToArray());
				
				//animationComponent.GetClip(clipInPrefab).ClearCurves();
				Transform[] childTransforms = root.GetComponentsInChildren<Transform>();
				List<string> childRelativePaths = new List<string>();
				foreach(Transform childTransform in childTransforms)
				{
					if(childTransform.name != "root")
					{
						List<string> myNameAndParentNames = new List<string>();
						Transform parentTransform = childTransform;
						string toBeAddedName;
						while(parentTransform.gameObject != root)
						{
							toBeAddedName = parentTransform.gameObject.name;
							myNameAndParentNames.Add(toBeAddedName);
							parentTransform = parentTransform.parent;
						}
						myNameAndParentNames.Reverse();
						string childRelativePath = string.Empty;
						childRelativePath = "root";
						foreach(string name in myNameAndParentNames)
						{
							childRelativePath+= "/" + name; 
						}
						childRelativePaths.Add(childRelativePath);
					}
				}
				AnimationClip clip = animationComponent.GetClip(clipInPrefab);
				EditorCurveBinding[] animationClipCurves = AnimationUtility.GetCurveBindings(clip);
				clip.ClearCurves();
				foreach(string childPath in childRelativePaths)
				{
					clip.SetCurve(childPath, typeof(Transform), "localPosition.x", curve);
					clip.SetCurve(childPath, typeof(Transform), "localPosition.y", curve);
					clip.SetCurve(childPath, typeof(Transform), "localPosition.z", curve);
					clip.SetCurve(childPath, typeof(Transform), "localRotation.x", curve);
					clip.SetCurve(childPath, typeof(Transform), "localRotation.y", curve);
					clip.SetCurve(childPath, typeof(Transform), "localRotation.z", curve);
					
					clip.SetCurve(childPath, typeof(Transform), "localScale.x", scaleCurve);
					clip.SetCurve(childPath, typeof(Transform), "localScale.y", scaleCurve);
					clip.SetCurve(childPath, typeof(Transform), "localScale.z", scaleCurve);
				}
				foreach(EditorCurveBinding data in animationClipCurves)
				{
					clip.SetCurve(data.path,data.type,data.propertyName,AnimationUtility.GetEditorCurve(clip, data));
				}
			}
		}
	}
	string[] GetAnimationClipsNames(Animation anim)
	{
		List<string> tmpList = new List<string>();
		foreach(AnimationState state in anim)
		{
			tmpList.Add(state.name);
		}
		string[] result = tmpList.ToArray();
		return result;
	}
}
