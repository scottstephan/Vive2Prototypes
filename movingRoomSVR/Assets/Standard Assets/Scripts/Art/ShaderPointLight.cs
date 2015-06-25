using UnityEngine;
using System.Collections;

public class ShaderPointLight : MonoBehaviour {
	
	Transform myXform;
	//Renderer myRenderer;
	public Material[] myMaterials;
	
	// Use this for initialization
	void Start () {
		myXform = transform;
		//myRenderer = renderer;
		//myMaterials = new Material[9];
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 wp = myXform.TransformPoint(Vector3.zero);
		for(int i=0; i < myMaterials.Length; i++)
		{
			if(myMaterials[i])
			{
				myMaterials[i].SetFloat("_pLightPosX",wp.x);
				myMaterials[i].SetFloat("_pLightPosY",wp.y);
				myMaterials[i].SetFloat("_pLightPosZ",wp.z);
			}
		}
		
	}
}
