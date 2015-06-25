using UnityEngine;
using System.Collections;

public class FxTrigger : MonoBehaviour {
	public ParticleSystem[] fx;

	void Start () {
	
	}
	
	public void PlayFx(int i){
		if(fx[i].gameObject != null)
			fx[i].Play(true);
	}
}
