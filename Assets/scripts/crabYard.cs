using UnityEngine;
using System.Collections;

public class crabYard : MonoBehaviour {
	public TextMesh scoreText;
	public TextMesh rankText;
	public int crabScore;
	void OnTriggerEnter(Collider col){
		if (col.gameObject.name == "crab") 
		{
			Debug.Log("Crab in collider");
			crabScore++;
			scoreText.text += crabScore;
			if(crabScore > 1) rankText.text = "DADDY SATED";
			Destroy(col.gameObject);
		}
	}
}
