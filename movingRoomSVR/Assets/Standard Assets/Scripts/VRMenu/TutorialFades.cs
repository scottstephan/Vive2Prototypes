using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public class TutorialFades : MonoBehaviour {

	public bool hideOut = false;

	public bool noFade = false;

	void OnEnable() {

		if (noFade)
			return;

		Color col = this.GetComponent<Renderer>().material.GetColor("_TintColor");
		col.a = 0f;
		this.GetComponent<Renderer>().material.SetColor("_TintColor", col);

		col.a = 1f;
		HOTween.To(this.GetComponent<Renderer>().material, .5f, new TweenParms().Prop("color", new PlugSetColor(col).Property("_TintColor")));
	}

	public void FadeDisable() {

		if (noFade) {
			FadeDone ();
			return;
		}

		Color col = this.GetComponent<Renderer>().material.GetColor("_TintColor");
		col.a = 0f;

		HOTween.To(this.GetComponent<Renderer>().material, .5f, new TweenParms().Prop("color", new PlugSetColor(col).Property("_TintColor")).OnComplete(FadeDone));
	}
	
	void FadeDone() {

		FloatingMenuManager.HideMenu(false);

		this.gameObject.SetActive(false);
	}
}
