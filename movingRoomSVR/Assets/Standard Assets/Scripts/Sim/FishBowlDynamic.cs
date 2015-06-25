using UnityEngine;
using System.Collections;

public class FishBowlDynamic : FishBowl {

    Transform myTransform;

	public override void Awake() 
    {
        myTransform = transform;
        base.Awake();
	}	

    public void Update()
    {
        UpdateTransform(myTransform);
    }
}
