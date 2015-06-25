using UnityEngine;
using System.Collections;

[System.Serializable]
public class GibSpawnData
{
    public GameObject gibObject;
    public string designGroupName;
    public int count = 1;
    public Vector3 offset = new Vector3(1f, 0f, 0f);
}

public class DeadData : BehaviorDataBase {

    public GibSpawnData[] gibs;
    public GameObject eatenFX;

    public float DyingSwimSpeed = 50f;

    [HideInInspector]
    public bool SpawnGibs;

    [HideInInspector]
    public float curLerpToPredator;

    public bool HasGibs()
    {
        return (gibs != null && gibs.Length > 0);
    }
}
