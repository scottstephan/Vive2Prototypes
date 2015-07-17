using UnityEngine;
using System.Collections;

public class svr_birdFeeder : MonoBehaviour {
    private controllerListener.svrController activatingController;
    public delegate void foodThrown();
    public static event foodThrown foodOut;
    public ParticleSystem foodEmitter;
    
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void objectIsBeingHeld()
    {
        Debug.Log(gameObject.name + "is being held");
        
    }

    public void objectIsReleased()
    {
        Debug.Log(gameObject.name + "is being released");
        StartCoroutine("foodEventDelay");
    //    foodEmitter.Play();
        activatingController = null;
    }

    IEnumerator foodEventDelay()
    { // lets the food settle before broadcasting its location. probs better to just update the position in real-time, but, uh. lazy.
        yield return new WaitForSeconds(2.5f);
        birdSimManager.lastTossedFood = gameObject;
        if (foodOut != null) foodOut(); //sends event
    }
}
