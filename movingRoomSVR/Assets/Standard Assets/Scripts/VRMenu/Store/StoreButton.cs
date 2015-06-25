using UnityEngine;
using System.Collections;
using System;
using Holoville.HOTween;

public class StoreButton : VRButton {

    public GameObject TextNormal;
    public GameObject TextHighlight;

	bool _selected;

	public override void Awake() {
        base.Awake ();
		_localScale = this.transform.localScale;
	}


	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
		if (!_isEnabled)
			return;

		ButtonInput();

		if (ProcessInput())
			OnClick();
	}

    public override void Highlight(bool set, bool on_enable = false) 
    {
        base.Highlight(set, on_enable);

        if (TextNormal != null && TextHighlight != null)
        {
            TextHighlight.SetActive(_highlighted);
            TextNormal.SetActive(!_highlighted);
        }
    }
}
