using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(StoryBeatAnimObject))]
public class StoryBeatAnimEditor : Editor 
{
    List<Vector3> points = new List<Vector3>();
    int frameCount = 0;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if ((frameCount % 10) == 0)
        {
            Sample();
        }

        ++frameCount;
    }

    public void OnSceneGUI () 
    {
        DrawPoints ();
    }

    void Sample()
    {
        StoryBeatAnimObject obj = (StoryBeatAnimObject)target;
        if (obj == null)
        {
            return;
        }
        
        Animation anim = obj.gameObject.GetComponent<Animation>();

        Transform t = obj.gameObject.transform;
        Vector3 oldPos = t.position;
        Quaternion oldRot = t.rotation;
        
        points.Clear();
        float lastTime = anim[anim.clip.name].time;
        Vector3 curPos;
        float length = anim.clip.length;
        float curSample = 0f;
        anim.Play();
        while (curSample < length)
        {
            anim[anim.clip.name].time = curSample;
            anim.Sample();
            curPos = t.position;
            points.Add(curPos);
            curSample += 1/30f;
        }
        
        anim.Stop ();
        anim[anim.clip.name].time = lastTime;
        t.position = oldPos;
        t.rotation = oldRot;
    }

    void DrawPoints()
    {
        for (int i=0; i<points.Count-1; ++i)
        {
            Handles.DrawLine(points[i], points[i+1]);
        }
    }
}
