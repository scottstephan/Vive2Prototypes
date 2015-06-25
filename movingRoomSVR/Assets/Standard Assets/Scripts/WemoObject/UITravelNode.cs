using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public class UITravelNode : MonoBehaviour {
	public Transform text;
	public Transform icon;
	public Material glassMat;
	public Color defaultColor = Color.blue;
	public Color hoverColor = Color.red;


	private float iconSpeed = 70f;
//	private bool isHovering ;
//	private float accel = 45f;
	private Vector3 origScale;

	void Start () {
		origScale = Vector3.one * 0.582f;// transform.localScale;
	}

    const float NORMAL_FRESNEL = 0.9f; //0.5f
    const float NORMAL_OPACITY = 2.75f; //0.5f
    const float NORMAL_SPECPOW = 40f;

    const float HOVER_SCALE = 1.6f; 
    const float HOVER_FRESNEL = 1.7f; //1.7f 
    const float HOVER_OPACITY = 1.8f; //0.5f
    const float HOVER_SPECPOW = 32f;

    void OnEnable(){
		//SetHovering(false);
        glassMat.SetFloat("_fresnelPow", NORMAL_FRESNEL);
		text.localScale = Vector3.one * 0.001f;
	}

	public void SetHovering(bool isHover){
		//Debug.Log("SetHovering " + isHover);
//		isHovering = isHover;
		if(isHover){
			HOTween.To(transform, 0.4f, new TweenParms()
                       .Prop("localScale", origScale * HOVER_SCALE)
			           .Ease(EaseType.EaseOutExpo)
			           );
			//HOTween.To(compassMat, 0.4f, new TweenParms().Prop("color", new PlugSetColor(hoverColor).Property("_Color")).Ease(EaseType.Linear));
            HOTween.To(glassMat, 0.4f, new TweenParms().Prop("color", new PlugSetFloat(HOVER_FRESNEL).Property("_fresnelPow")).Ease(EaseType.Linear));
            HOTween.To(glassMat, 0.4f, new TweenParms().Prop("color", new PlugSetFloat(HOVER_OPACITY).Property("_OpacMult")).Ease(EaseType.Linear));
            HOTween.To(glassMat, 0.4f, new TweenParms().Prop("color", new PlugSetFloat(HOVER_SPECPOW).Property("_specPow")).Ease(EaseType.Linear));
            HOTween.To(text, 0.4f, new TweenParms()
			           .Prop("localScale", Vector3.one * 0.13f)
			           .Ease(EaseType.EaseOutExpo)
			           );

		}
		else{
			HOTween.To(transform, 0.4f, new TweenParms()
			           .Prop("localScale", origScale )
			           //.Ease(EaseType.EaseInExpo)
			           );
			//HOTween.To(compassMat, 0.4f, new TweenParms().Prop("color", new PlugSetColor(defaultColor).Property("_Color")).Ease(EaseType.Linear));
            HOTween.To(glassMat, 0.4f, new TweenParms().Prop("color", new PlugSetFloat(NORMAL_FRESNEL).Property("_fresnelPow")).Ease(EaseType.Linear));
            HOTween.To(glassMat, 0.4f, new TweenParms().Prop("color", new PlugSetFloat(NORMAL_OPACITY).Property("_OpacMult")).Ease(EaseType.Linear));
            HOTween.To(glassMat, 0.4f, new TweenParms().Prop("color", new PlugSetFloat(NORMAL_SPECPOW).Property("_specPow")).Ease(EaseType.Linear));           
			HOTween.To(text, 0.4f, new TweenParms()
			           .Prop("localScale", Vector3.one * 0.001f)
			           //.Ease(EaseType.EaseOutExpo)
			           );

		}
			
			
	}


	void Update () {
		//this.transform.LookAt(CameraManager.GetEyePosition());
		icon.localEulerAngles += Vector3.up * Time.deltaTime * iconSpeed;
		/*
		compass.localEulerAngles += Vector3.forward * Time.deltaTime * compassSpeed;
		if(isHovering){
			compassSpeed += Time.deltaTime * accel;
			//iconSpeed += Time.deltaTime * accel;
		}
		else{
			compassSpeed -= Time.deltaTime * accel;
			//iconSpeed -= Time.deltaTime * accel;
		}
		compassSpeed = Mathf.Clamp(compassSpeed, 0f, 50f);
		//iconSpeed = Mathf.Clamp(iconSpeed, 0f, 70f);
		*/
	}
}
