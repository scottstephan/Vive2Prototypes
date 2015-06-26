using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class FlareData{
	public Texture2D  texture;
	public float dist;
	public float size;
	public float opacMult;
	public bool useOpac;
	public bool UseRotSpeed;
	public float rotSpeed;
	[HideInInspector]
	public float rot;
}

public class LensFlareManager : MonoBehaviour {
	public Transform sun;
	public Camera camera;
	public float distFromCam = 10f;
	public GameObject flarePrefab;
	public FlareData[] set;

	private List<Transform> flares;
	private List<Material> flaresMats;

	private Transform camXform;
	private Transform myXform;
	private Vector3 sunDir;
	private Vector3 lastVp;
	private float globalAlpha;
	private bool isVisible;


	void Start () {
		flares = new List<Transform>();
		flaresMats = new List<Material>();
		myXform = transform;
		sunDir = sun.forward;
		camXform = camera.transform;

		for(int i = 0; i < set.Length; i++){
			GameObject flare = Instantiate(flarePrefab, Vector3.zero, Quaternion.identity) as GameObject;
			flares.Add(flare.transform);
			flaresMats.Add(flare.GetComponent<Renderer>().material);
			flaresMats[i].SetTexture("_MainTex", set[i].texture);
			float ratio = (float)set[i].texture.width / (float)set[i].texture.height;
			flare.transform.localScale = new Vector3( set[i].size * ratio, set[i].size * ratio, 1f);
		}
	}

	void LateUpdate () {
		myXform.position = camXform.position - sunDir * distFromCam;
		Vector3 vp =  camera.WorldToViewportPoint(myXform.position);
		//Debug.Log(vp  );
		if(vp.x > 0f && vp.x < 1f && vp.y > 0 && vp.y < 1f){
			globalAlpha = Mathf.Min(globalAlpha + Time.deltaTime * 2f, 1f);
			if(!isVisible)
				ShowFlares(true);
		}
		else{
			globalAlpha = Mathf.Max(globalAlpha - Time.deltaTime * 2f, 0f);
			if(isVisible && globalAlpha == 0f)
				ShowFlares(false);
		}


		float vpx =  Mathf.Clamp( Mathf.Abs( 2f * (vp.x - 0.5f)), 0f, 1f);
		float vpy =  Mathf.Clamp( Mathf.Abs( 2f * (vp.y - 0.5f)), 0f, 1f);
		float alpha = Mathf.Pow ( vpx * vpy, 0.3f);
		Vector3 dir = (camXform.position - myXform.position).normalized;
		Vector3 up = Vector3.Cross(dir, camXform.forward);
		Vector3 right = Vector3.Cross(dir, up);
		myXform.rotation = Quaternion.LookRotation(-dir, up);
		for(int i = 0; i < flares.Count; i++){
			Transform flare = flares[i];
			FlareData fd = set[i];
			flare.position = myXform.position - right * fd.dist;
			if(fd.UseRotSpeed){
				float dvpx = vp.x - lastVp.x;
				fd.rot += dvpx * fd.rotSpeed * Time.deltaTime;
				flare.rotation = Quaternion.AngleAxis( fd.rot, -dir);
			}
			else{
				flare.rotation = Quaternion.LookRotation(-dir, up);
			}
			if(fd.useOpac)
				flaresMats[i].SetColor("_TintColor", Color.white * alpha * fd.opacMult * globalAlpha);
			else
				flaresMats[i].SetColor("_TintColor", Color.white  * fd.opacMult * globalAlpha);

			
		}

		lastVp = vp;
	}

	public void ShowFlares(bool show){
		isVisible = show;
		for(int i = 0; i < flares.Count; i++){
			Transform flare = flares[i];
			flares[i].gameObject.SetActive(show);
		}
	}

}




/*
 * 
 * 	void LateUpdate () {
		myXform.position = camXform.position - sunDir * distFromCam;
		Vector3 vp =  camera.WorldToViewportPoint(myXform.position);
		//Debug.Log(vp  );
		if(vp.x > 0f && vp.x < 1f && vp.y > 0 && vp.y < 1f){
			globalAlpha = Mathf.Min(globalAlpha + Time.deltaTime * 2f, 1f);
			float vpx =  Mathf.Clamp( Mathf.Abs( 2f * (vp.x - 0.5f)), 0f, 1f);
			float vpy =  Mathf.Clamp( Mathf.Abs( 2f * (vp.y - 0.5f)), 0f, 1f);
			float alpha = Mathf.Pow ( vpx * vpy, 0.3f);
			Vector3 dir = (camXform.position - myXform.position).normalized;
			Vector3 up = Vector3.Cross(dir, camXform.forward);
			Vector3 right = Vector3.Cross(dir, up);
			myXform.rotation = Quaternion.LookRotation(-dir, up);
			for(int i = 0; i < flares.Count; i++){
				Transform flare = flares[i];
				FlareData fd = set[i];
				if(!flare.gameObject.activeSelf){
					flare.gameObject.SetActive(true);
				}
				flare.position = myXform.position - right * fd.dist;
				if(fd.UseRotSpeed){
					float dvpx = vp.x - lastVp.x;
					fd.rot += dvpx * fd.rotSpeed * Time.deltaTime;
					flare.rotation = Quaternion.AngleAxis( fd.rot, -dir);
				}
				else{
					flare.rotation = Quaternion.LookRotation(-dir, up);
				}
				if(fd.useOpac)
					flaresMats[i].SetColor("_TintColor", Color.white * alpha * fd.opacMult * globalAlpha);
				else
					flaresMats[i].SetColor("_TintColor", Color.white  * fd.opacMult * globalAlpha);

			}
		}
		else{
			for(int i = 0; i < flares.Count; i++){
				FlareData fd = set[i];
				globalAlpha = Mathf.Max(globalAlpha - Time.deltaTime * 0.35f, 0f);
				if(flares[i].gameObject.activeSelf){
					//if(fd.useOpac)
					//	flaresMats[i].SetColor("_TintColor", Color.white * alpha * fd.opacMult * globalAlpha);
					//else
						flaresMats[i].SetColor("_TintColor", Color.white  * fd.opacMult * globalAlpha);

					//if(globalAlpha == 0f)
						//flares[i].gameObject.SetActive(false);
				}
			}
		}
		lastVp = vp;
	}

*/
