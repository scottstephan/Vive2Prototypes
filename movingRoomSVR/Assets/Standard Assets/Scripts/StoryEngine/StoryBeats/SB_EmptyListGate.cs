using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SB_EmptyListGate : SBBase {
	
	public List<GameObject> objects;
	
	private bool isComplete = false;
		
	public override void BeginBeat() {
		//Debug.Log("SB_EmptyListGate BeginBeat");
		base.BeginBeat();
		foreach(GameObject obj in objects){
			//Debug.Log ("SetEmptyListGate " + obj.name);
			obj.SendMessage("SetEmptyListGate", this, SendMessageOptions.DontRequireReceiver); // assigning this cmpt to whoever is listening
		}
	}
		
	public void RemoveObject(GameObject obj){
		objects.Remove(obj);
		if(objects.Count == 0){
			isComplete = true;
			markedForRemoval = true;
		}
	}
	
	public override bool IsComplete() { 
		return isComplete; 
	}
}
