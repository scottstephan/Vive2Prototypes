using UnityEngine;
using System.Collections;

public class FxPortal : MonoBehaviour {

	private Animator animator;
	private float timer;

    [HideInInspector]
	public bool isInside;
    [HideInInspector]
    public bool isTriggered;

	void Start () {
		animator = GetComponent<Animator>();
	}
	
	void OnTriggerEnter(Collider col){
		isInside = true;
		timer = 0f;
		animator.SetTrigger("activate");
	}
	void OnTriggerExit(Collider col){
		isInside = false;
		animator.SetTrigger("deactivate");
	}

	void Update () {
		if(isInside){
			timer += Time.deltaTime;
			if(timer > 3f)
            {
                isTriggered = true;
                timer = 0f;
			}
		}
	}
}
