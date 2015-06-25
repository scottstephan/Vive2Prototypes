using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(WemoObject))]
public class WemoObjectInspector : Editor
{
    private WemoObject _targetObject;
    private SerializedObject _serializedObj;
    string _tagToAdd = "Untagged";
    bool _update;
    Color _buttonColor = GUI.color;

    void OnEnable()
    {
        _targetObject = target as WemoObject;
        _serializedObj = new SerializedObject(target);
//        _buttonColor = AntaresEdWindow.colorSelected;
//        if (AntaresGameObjectCustom.addonController == null) AntaresGameObjectCustom.InitController();

        List<string> tagsList;
        tagsList = _targetObject.gameObject.GetTags();
        if(tagsList == null) return;
        int count = tagsList.Count;
        for (int i = 0; i < count; i++)
        {
            if(!WemoObjectExtension.DoesTagExist(tagsList[i]))
            {
                if (EditorUtility.DisplayDialog("Tags Error", string.Format("Tag {0} is missed in the Unity Tag Manager. Would you like to remove it from object?", tagsList[i]), "Ok", "Cancel"))
                {
                    tagsList.RemoveAt(i);
                    i--;
                    count--;
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        _update = false;
        _targetObject = target as WemoObject;

        EditorGUILayout.Separator();

//        GUI.color = Color.gray;
        GUILayout.Label("Tags : " + _targetObject.gameObject.TagsCount());
//       GUI.color = Color.black;
        _targetObject.tagsFoldout = EditorGUILayout.Foldout(_targetObject.tagsFoldout, "");
//       GUI.color = AntaresEdWindow.colorGUI;

        if (_targetObject.tagsFoldout) DrawTagsFoldout();


        if (_update)
        {
            _serializedObj.Update();
            _serializedObj.ApplyModifiedProperties();
        }

//        Routines.DrawFooter();
//        GUI.color = AntaresEdWindow.colorGUI;
    }

    private void DrawTagsFoldout()
    {
        bool iMInScene = Selection.activeTransform == _targetObject.transform, tagAdded = false;
        GUI.enabled = iMInScene;
        EditorGUILayout.BeginHorizontal();

        string tagBefore = _tagToAdd;

        _tagToAdd = EditorGUILayout.TagField(_tagToAdd, GUILayout.Width(128));
        if (_tagToAdd != tagBefore && _tagToAdd != "Untagged" && _tagToAdd != "") tagAdded = true;
        GUI.enabled = !(_tagToAdd == "Untagged");
        GUI.color = _buttonColor;
        GUILayout.Label("   Select tag to add");

        if (tagAdded)
        {
            Undo.RecordObject(this, "Add Tag");
            if (_targetObject.transform.childCount > 0)
            {
                if (EditorUtility.DisplayDialog("Add tag to children ?", "If 'YES', the Tag (" + _tagToAdd + ") will be added to all childrens of this object (" + _targetObject.name + ")", "YES", "NO"))
                {
                    foreach (Transform t in _targetObject.gameObject.GetComponentsInChildren<Transform>())
                        t.gameObject.TagsAdd(_tagToAdd);
                }
            }
            _targetObject.gameObject.TagsAdd(_tagToAdd);
            _tagToAdd = "Untagged";
            _update = true;
        }

        EditorGUILayout.EndHorizontal();

        List<string> tagsList;
        tagsList = _targetObject.gameObject.GetTags();

        int count = tagsList.Count;
        EditorGUILayout.Separator();
        GUI.enabled = true;
        GUI.enabled = iMInScene;
        GUI.color = Color.white;
        for (int i = 0; i < count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Button(tagsList[i], EditorStyles.miniButtonLeft, GUILayout.Width(128));
            if (GUILayout.Button("Delete", EditorStyles.miniButtonRight, GUILayout.Width(64)))
            {
                Undo.RecordObject(this, "Remove Tag");
                if (_targetObject.transform.childCount > 0)
                {
                    if (EditorUtility.DisplayDialog("Remove Tag from children ?",
                                                    "If 'YES', the Tag (" + _tagToAdd +
                                                    ") will be removed from all childrens of this object (" +
                                                    _targetObject.name + ")", "YES", "NO"))
                    {
                        foreach (Transform t in _targetObject.gameObject.GetComponentsInChildren<Transform>())
                            t.gameObject.TagsRemove(tagsList[i]);
                    }
                }
                _targetObject.gameObject.TagsRemove(tagsList[i]);
                _update = true;
				i--;
				count--;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Separator();
        GUI.enabled = !(count == 0);
        GUI.color = _buttonColor;
        if (GUILayout.Button("Remove All Tags", EditorStyles.miniButton, GUILayout.Width(128)))
        {
            Undo.RecordObject(this, "Remove all Tags");
            if (_targetObject.transform.childCount > 0)
            {
                if (EditorUtility.DisplayDialog("Remove All Tags from children ?",
                                                string.Format("If 'YES', All Tags will be removed from all childrens of this object ({0})", _targetObject.name), "YES", "NO"))
                {
                    foreach (Transform t in _targetObject.gameObject.GetComponentsInChildren<Transform>())
                    {
                        WemoObject a = t.gameObject.GetComponent<WemoObject>();
                        if (a != null) a.gameObject.TagsRemoveAll();
                    }
                }
            }
            _targetObject.gameObject.TagsRemoveAll();
            _update = true;
        }
        GUI.color = Color.white;
    }
}
