using UnityEngine;
using System.Collections;

public class SB_OceanShader: SBBase {

    public Color deepColor;
    public Color middleColor;
    public Color shallowColor;
    public float duration = 10f;

    public bool adjustDistance;
    public float newDistance;

    Color deepColorStart;
    Color middleColorStart;
    Color shallowColorStart;
    float distanceStart;
    float timer;
    GlobalOceanShaderAdjust shader;

    Color resetDeepColorStart;
    Color resetMiddleColorStart;
    Color resetShallowColorStart;
    float resetDistanceStart;

    public override void Start ()
    {
        base.Start ();

        shader = (GlobalOceanShaderAdjust) GameObject.FindObjectOfType(typeof(GlobalOceanShaderAdjust));
        if (shader != null)
        {
            resetDeepColorStart = shader.deepColor;
            resetMiddleColorStart = shader.middleColor;
            resetShallowColorStart = shader.shallowColor;
            resetDistanceStart = shader.dist;
        }
    }

    public override void Reset ()
    {
        base.Reset ();

        shader = (GlobalOceanShaderAdjust) GameObject.FindObjectOfType(typeof(GlobalOceanShaderAdjust));
        if (shader != null)
        {
            shader.deepColor = resetDeepColorStart;
            shader.middleColor = resetMiddleColorStart;
            shader.shallowColor = resetShallowColorStart;      
            GlobalOceanShaderAdjust.SetDist(resetDistanceStart);
        }
    }

    public override void BeginBeat() 
    {  
        shader = (GlobalOceanShaderAdjust) GameObject.FindObjectOfType(typeof(GlobalOceanShaderAdjust));
        deepColorStart = shader.deepColor;
        middleColorStart = shader.middleColor;
        shallowColorStart = shader.shallowColor;
        distanceStart = shader.dist;

        timer  = 0f;
        base.BeginBeat();
		Debug.Log("SB_OceanShader " + gameObject.name);
    }

    public override void UpdateBeat()
    {
        timer += GetDeltaTime();
        shader.deepColor = Color.Lerp(deepColorStart, deepColor, timer/duration);
        shader.middleColor = Color.Lerp(middleColorStart, middleColor, timer/duration);
        shader.shallowColor = Color.Lerp(shallowColorStart, shallowColor, timer/duration);

        if (adjustDistance)
        {
            GlobalOceanShaderAdjust.SetDist(Mathf.Lerp(distanceStart, newDistance, timer/duration));
        }
        else
        {
            shader.AdjustToParams(false);
        }

        base.UpdateBeat ();
    }
    
    public override bool IsComplete()
    {
        return timer > duration;
    }           
}
