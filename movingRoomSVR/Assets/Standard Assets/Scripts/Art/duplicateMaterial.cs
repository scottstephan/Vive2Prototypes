using UnityEngine;
using System.Collections;

public class duplicateMaterial : MonoBehaviour 
{
	public GameObject[] objects;
	public Color baseColor = new Color(0.8f,0.8f,0.8f,1f);
	public Vector3 randRGB = new Vector3(0.2f,0.2f,0.2f);
	
	// Use this for initialization
	void Start () 
	{
        /*
		Vector3 rand = new Vector3(randRGB.x * Random.value, randRGB.y * Random.value, randRGB.z * Random.value);
		for(int i=0; i<objects.Length; i++)
		{
			GameObject obj = objects[i];
			if(obj != null){
				Material mat = obj.renderer.material;
				Material newMat = new Material(mat.shader);
				newMat.CopyPropertiesFromMaterial(mat);
				DestroyImmediate( obj.renderer.material );
				obj.renderer.material = newMat;
				Color randColor = new Color(baseColor.r  + rand.x, baseColor.g + rand.y, baseColor.b + rand.z, 1f);
				newMat.SetColor("_MainTexColor",randColor);
			}
		}
        */
	}
	

}
