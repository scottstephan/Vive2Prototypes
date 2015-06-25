
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
public static class Console
{
    private const int HistoryCapacity = 128;
    public class DebugObject
    {
        public DateTime lastTime;

        public Color color = Color.white;

        public List<DebugObject> history = new List<DebugObject>();
        public Vector2 scroll;

        public object Value
        {
            get { return _value; }
            set
            {
                lastTime = DateTime.Now; _value = value;
                history.Insert(0, new DebugObject(value.ToString()));
                if (history.Count > HistoryCapacity) history.RemoveAt(HistoryCapacity);
            }
        }
        private object _value;
        public bool open = false;

        public DebugObject(object value, Color color)
        {
            lastTime = DateTime.Now;
            history.Add(new DebugObject(value.ToString()));
            _value = value;
            this.color = color;
        }

        public DebugObject(object value)
        {
            lastTime = DateTime.Now;
            _value = value;
        }

        public Dictionary<string, DebugObject> GetMessageHolder()
        {
            return Value as Dictionary<string, DebugObject>;
        }

        public void RemoveEntry(string entryName)
        {
            Dictionary<string, DebugObject> messages = _value as Dictionary<string, DebugObject>;
            if (messages == null || !messages.ContainsKey(entryName)) return;
            messages.Remove(entryName);
            _value = messages;
        }

        public GameObject owner;
        public bool noMonobeh = false;
    }

    private const uint Capacity = 128;
    public static Dictionary<object, DebugObject> entries = new Dictionary<object, DebugObject>();

    public static void Log(object sender, string description, object value, Color color)
    {
        if (value == null) return;
        DebugObject obj;
        if (entries.TryGetValue(sender, out obj))
        {
            Dictionary<string, DebugObject> member = obj.GetMessageHolder();
            switch (member.ContainsKey(description))
            {
                case true:
                    member[description].Value = value;
                    break;
                case false:
                    member.Add(description, new DebugObject(value, color));
                    break;
            }
            obj.color = color;
            obj.lastTime = DateTime.Now;
            return;
        }
        Dictionary<string, DebugObject> d = new Dictionary<string, DebugObject>();
        d.Add(description, new DebugObject(value, color));
        entries.Add(sender, new DebugObject(d, color));
        MonoBehaviour mb = sender as MonoBehaviour;
        if (mb == null)
        {
            switch (sender.GetType().ToString())
            {
                case "UnityEngine.GameObject":
                    entries[sender].owner = (GameObject)sender;
                    return;
            }
            entries[sender].noMonobeh = true;
            return;
        }
        entries[sender].owner = mb.gameObject;
        if (entries.Count > Capacity)
        {
            object[] keys = new object[entries.Count];
            entries.Keys.CopyTo(keys, 0);
            entries.Remove(keys[keys.Length - 1]);
        }
    }

    public static void Log(object sender, string description, object value)
    {
        Log(sender, description, value, Color.white);
    }

    public static void ClearAll()
    {
        entries.Clear();
    }
}
*/
	
/*
public class Layout
{
    public class Horizontal : IDisposable
    {
        bool _disposed = false;
        public Horizontal()
        {
            GUILayout.BeginHorizontal();
        }

        public void Dispose()
        {
            if (_disposed) return;
            GUILayout.EndHorizontal();
            _disposed = true;
        }
    }

    public class AlignToCenter : IDisposable
    {
        bool _disposed = false;
        public AlignToCenter()
        {
            GUILayout.Label("", GUILayout.ExpandWidth(true));
        }

        public void Dispose()
        {
            if (_disposed) return;
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            _disposed = true;
        }
    }

    public class Vertical : IDisposable
    {
        bool _disposed = false;
        public Vertical()
        {
            GUILayout.BeginVertical();
        }

        public void Dispose()
        {
            if (_disposed) return;
            GUILayout.EndVertical();
            _disposed = true;
        }
    }

    public class Area : IDisposable
    {
        bool _disposed = false;
        public Area(Rect rect)
        {
            GUILayout.BeginArea(rect);
        }

        public void Dispose()
        {
            if (_disposed) return;
            GUILayout.EndArea();
            _disposed = true;
        }
    }
}
*/

public static class WemoObjectExtension
{
    public static bool DoesTagExist(string tag)
    {
        try
        {
            GameObject.FindGameObjectWithTag(tag);
        }
        catch
        {
            return false;
        }
        return true;
    }

    public static List<GameObject> FindObjectsWithTags(this GameObject gameObject, params string[] tags)
    {
        WemoObject[] wemo_objs = GameObject.FindObjectsOfType(typeof(WemoObject)) as WemoObject[];
        List<GameObject> ret = new List<GameObject>();
        int count = wemo_objs.Length, mcount = tags.Length;
        for (int i = 0; i < count; i++)
            for (int t = 0; t < mcount; t++)
                if (wemo_objs[i].tags.Contains(tags[t]) && !ret.Contains(wemo_objs[i].gameObject))
                    ret.Add(wemo_objs[i].gameObject);
        return ret;
    }

    public static List<GameObject> FindObjectsWithTags(params string[] tags)
    {
        WemoObject[] wemo_objs = GameObject.FindObjectsOfType(typeof(WemoObject)) as WemoObject[];
        List<GameObject> ret = new List<GameObject>();
        int count = wemo_objs.Length, mcount = tags.Length;
        for (int i = 0; i < count; i++)
            for (int t = 0; t < mcount; t++)
                if (wemo_objs[i].tags.Contains(tags[t]) && !ret.Contains(wemo_objs[i].gameObject))
                    ret.Add(wemo_objs[i].gameObject);
        return ret;
    }

    public static bool HasTag(this GameObject gameObject, string tag)
    {
        WemoObject wemo_obj = gameObject.GetComponent<WemoObject>();
        return wemo_obj != null ? wemo_obj.tags.Contains(tag) : false;
    }

    public static bool HasAnyTag(this GameObject gameObject, params string[] tags)
    {
        if (tags == null)
        {
            return false;
        }

        WemoObject wemo_obj = gameObject.GetComponent<WemoObject>();
        if (wemo_obj == null)
        {
            return false;
        }

        int count = tags.Length;
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < wemo_obj.tags.Count; j++)
            {
                if (string.Compare(tags[i], wemo_obj.tags[j], StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool HasAllTags(this GameObject gameObject, params string[] tags)
    {
        if (tags == null)
        {
            return false;
        }

        WemoObject wemo_obj = gameObject.GetComponent<WemoObject>();
        if (wemo_obj == null) 
        {
            return false;
        }

        int count = tags.Length;
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < wemo_obj.tags.Count; j++)
            {
                if (string.Compare(tags[i], wemo_obj.tags[j], StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static int TagsCount(this GameObject gameObject)
    {
        WemoObject wemo_obj = gameObject.GetComponent<WemoObject>();
        return wemo_obj != null ? wemo_obj.tags.Count : 0;
    }

    public static void TagsAdd(this GameObject gameObject, params string[] tags)
    {
        WemoObject wemo_obj = gameObject.GetComponent<WemoObject>();
        if (wemo_obj == null) wemo_obj = gameObject.AddComponent<WemoObject>();
        int count = tags.Length;
        for (int i = 0; i < count; i++)
            if (!wemo_obj.tags.Contains(tags[i]))
                wemo_obj.tags.Add(tags[i]);
    }

    public static void TagsRemove(this GameObject gameObject, params string[] tags)
    {
        WemoObject wemo_obj = gameObject.GetComponent<WemoObject>();
        if (wemo_obj == null) return;
        int count = tags.Length;
        for (int i = 0; i < count; i++)
            if (wemo_obj.tags.Contains(tags[i]))
                wemo_obj.tags.Remove(tags[i]);
    }

    public static void TagsCopyTo(this GameObject copyFrom, GameObject copyTo)
    {
        WemoObject wemo_from = copyFrom.GetComponent<WemoObject>();
        if (wemo_from == null) return;
        WemoObject wemo_to = copyTo.GetComponent<WemoObject>();
        if (wemo_to == null) wemo_to = copyTo.AddComponent<WemoObject>();
        wemo_to.tags = new List<string>(wemo_from.tags);
    }

    public static List<string> GetTags(this GameObject gameObject)
    {
        WemoObject wemo_obj = gameObject.GetComponent<WemoObject>();
        return wemo_obj != null ? wemo_obj.tags : new List<string>();
    }

    public static void TagsRemoveAll(this GameObject gameObject)
    {
        WemoObject wemo_obj = gameObject.GetComponent<WemoObject>();
        if (wemo_obj == null) return;
        wemo_obj.tags.Clear();
        wemo_obj.tags.TrimExcess();
    }
}