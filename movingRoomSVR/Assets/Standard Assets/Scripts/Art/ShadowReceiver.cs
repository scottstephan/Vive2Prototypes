using UnityEngine;
using System.Collections;

public class ShadowReceiver : MonoBehaviour {
	public bool exitCollider;
	public bool enterCollider; 
	Color lightColor;
	public Color darkColor = new Color(0.3f,0.3f,0.3f,1.0f);
	float t = 0.0f;
	float r = 0.0f;
	void Start(){
		LODModelData critter = this.GetComponent<LODModelData>();
		GameObject lod = critter.LODs[0].LOD;
		Material mat = lod.transform.GetChild(0).GetComponent<Renderer>().material;
		lightColor = mat.GetColor("_MainTexColor");
		t = 0.0f;
		r = 0.0f;
		exitCollider = false;
		enterCollider = false;
	}
	void Update(){
		LODModelData critter = this.GetComponent<LODModelData>();
	
		if (exitCollider){
			Color lerpedColor =  Color.Lerp(darkColor, lightColor, t); 
			for(int x = 0; x <3; x++){
				GameObject lod = critter.LODs[x].LOD;
				int lodChildCount = lod.transform.childCount; 
				for(int j = 0; j < lodChildCount; j++){ 
					Material mat = lod.transform.GetChild(j).GetComponent<Renderer>().material;
					mat.SetColor("_MainTexColor",lerpedColor);
				}
			}
			t+=0.05f;
			if(lerpedColor == lightColor){
				t=0.0f;
				exitCollider = false;
			}
		}
		if (enterCollider){
			Color lerpedColor =  Color.Lerp(lightColor, darkColor, r); 
			for(int x = 0; x <3; x++){
				GameObject lod = critter.LODs[x].LOD;
				int lodChildCount = lod.transform.childCount; 
				for(int j = 0; j < lodChildCount; j++){ 
					Material mat = lod.transform.GetChild(j).GetComponent<Renderer>().material;
					mat.SetColor("_MainTexColor",lerpedColor);
				}
			}
			r+=0.05f;
			if(lerpedColor == darkColor){
				r=0.0f;
				enterCollider = false;
			}
		}
	}
}
