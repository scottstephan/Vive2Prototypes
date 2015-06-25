using UnityEngine;
using System.Collections;

public class HMDScalerBase : MonoBehaviour
{
    public float Scale = 1f;
    public bool UseMouseTestScale;
    Vector3 m_startMousePos;

    GlobalOceanShaderAdjust shader;

    void Start()
    {
        OnStart();
        ApplyScale();

        // must wait to update the shader params, otherwise the planes don't get set correctly
        StartCoroutine(UpdateOcean());
    }

    IEnumerator UpdateOcean()
    {
        yield return new WaitForSeconds(1f);
        GlobalOceanShaderAdjust.StaticAdjustToParams(true);
    }

    protected virtual void OnStart() { } // cache your HMD vars
    protected virtual void ApplyScale() { }

    void Update()
    {
        // click and drag right to scale player up (make world feel smaller), or left to scale down (make world feel bigger)
        if (UseMouseTestScale)
        {
            if (Input.GetMouseButtonDown(0))
                m_startMousePos = Input.mousePosition;

            if (Input.GetMouseButton(0))
            {
                Scale *= 1 + (Input.mousePosition - m_startMousePos).x / 10000f;
                Scale = Mathf.Clamp(Scale, 0.001f, 1000f);
                ApplyScale();
            }
        }
    }
}

