using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void PulseEffectDoneDelegate(CritterInfo critter_info);

[System.Serializable]
public class LODLevel {
	public GameObject LOD;
	public float distance;
	
	[HideInInspector]
	public Renderer LODrenderer;

	[HideInInspector]
    public float distanceUpgradeSqrd;
    [HideInInspector]
    public float distanceDowngradeSqrd;

	[HideInInspector]
	public Material LODmaterial;
}

public class LODModelData : MonoBehaviour {
	public LODLevel[] LODs;
	
	public Transform pointLight;
//	public GlowEffect glowEffect;
	
    public int ForceLOD = -1;

    public float distanceThreshold = 5f;

    public GameObject[] litPieces;
    public GameObject[] unlitPieces;

    [HideInInspector]
	public int curLODidx;
	
	[HideInInspector]
	public LODLevel curLOD;
	
	[HideInInspector]
	public bool objectActive = true;
	
	[HideInInspector]
	public bool staticMesh = false; // this is used to disable bendController when we are using meshRenderer instead of skinnedMeshRenderer

    // class log
    private static Log log = Log.GetLog(typeof(LODModelData).FullName);

    IEnumerator _DelayLitPieces( bool lit_on, float delay ) {
        yield return new WaitForSeconds( delay );
        SetupLitPieces( lit_on, 0f );
    }

    public void SetupLitPieces( bool lit_on, float delay=0f ) {
        if( delay > 0f ) {
            StartCoroutine( _DelayLitPieces( lit_on, delay ) );
            return;
        }

        int i;
        if( litPieces != null && litPieces.Length > 0 ) {
            for( i = 0; i < litPieces.Length; i++ ) {
                litPieces[i].SetActive( lit_on );
            }
        }

        if( unlitPieces != null && unlitPieces.Length > 0 ) {
            lit_on = !lit_on;
            for( i = 0; i < unlitPieces.Length; i++ ) {
                unlitPieces[i].SetActive( lit_on );
            }
        }
    }

/*	private float pulseSpeed = 0.10f;
	private float pulseValue = 0f;
	
	private float lastRealTime = 0f;
	private float pulseDir = 1f;
	private float pulseMaxValue = 1.25f;*/
	
/*	private static bool _glowActive = false;
	public static bool GlowActive {
        get { return _glowActive; }
    }
	private static CritterInfo _glowCritter = null;*/
	
/*	public void waldoFoundMaterial()
	{
		//WemoLog.Eyal("waldoFoundMaterial");
		StartCoroutine(saturateWaldoLodMaterial());
	}*/
	
/*	IEnumerator saturateWaldoLodMaterial()
	{
		bool fadeToColor = true;
		while(fadeToColor)
		{
			int numLOD = LODs.Length;
			for(int x = 0; x < numLOD; x++)
			{
				GameObject lod = LODs[x].LOD;
				int lodChildCount = lod.transform.childCount; 
				for(int j = 0; j < lodChildCount; j++)
				{
				    Transform srf = lod.transform.GetChild(j);
					Material mat = srf.renderer.material;
					float val = mat.GetFloat("_saturation") + 0.004f ;
					//WemoLog.Eyal(val);
					if(val > 1.0)
					{
						val = 1.0f;
						fadeToColor = false;
					}
					mat.SetFloat("_saturation", val);
				}	
			}
		
			yield return true; 	
		}
	}*/
	
/*	public void EnterSceneMaterialEffect( PulseEffectDoneDelegate callback_func, CritterInfo critter_info )
	{
		if( _glowActive ) {
			return;
		}
		_glowActive = true;
		pulseValue = pulseMaxValue;
		pulseDir = -1f;
		lastRealTime = 0f;
		StartCoroutine((_PulseMaterialEffect(callback_func, critter_info)));
	}*/
	
/*	public void ExitSceneMaterialEffect( PulseEffectDoneDelegate callback_func, CritterInfo critter_info )
	{
		if( _glowActive ) {
			return;
		}
		_glowActive = true;
		pulseValue = 0f;
		pulseDir = 1f;
		lastRealTime = 0f;
		StartCoroutine((_PulseMaterialEffect(callback_func, critter_info)));
	}*/

/*	IEnumerator _PulseMaterialEffect( PulseEffectDoneDelegate callback_func, CritterInfo critter_info )
	{
		LODModelData.critterGlow(critter_info,true);

		bool done = false;
		while(!done)
		{			
			float ot = lastRealTime;
			float ct = Time.time;
			float dt = 0f;
			if( lastRealTime > 0f ) 
			{
				dt = ct - ot;
			}
			lastRealTime = ct;
					
//			Debug.Log(gameObject.name + " :: " + pulseValue );
			glowEffect.glowIntensity = pulseValue;
			
			float inc = pulseSpeed * dt * pulseDir;
			pulseValue += inc;
			if( pulseValue < 0f ) {
				done = true;
				pulseValue = 0f;
			}
			else if( pulseValue > pulseMaxValue ) {
				done = true;
				pulseValue = pulseMaxValue;
			}
			
			if( done ) // we are entering. end with val at trough. 	
			{
//					Debug.Log("DONE");
				
				if( callback_func != null ) 
				{
					callback_func(critter_info);							
				}
		
				LODModelData.critterGlow(critter_info,false);
			}
		
			yield return null; 	
		}
		
		yield break;
	}*/
	
/*	public static void leGlow(CritterInfo critter_info, bool isOn)
	{
		if(!critter_info.generalSpeciesData.leGlowRenderer) return;
		if(isOn)
		{
			critter_info.generalSpeciesData.leGlowRenderer.enabled = true;
		}
		else
		{	
			critter_info.generalSpeciesData.leGlowRenderer.enabled = false;
		}
	}*/
	
/*	public static void adjustWaldoLodMaterial(CritterInfo critter_info, float frequency)
	{
		int numLOD = critter_info.critterLODData.LODs.Length;
		for(int x = 0; x < numLOD; x++)
		{
			GameObject lod = critter_info.critterLODData.LODs[x].LOD;
			int lodChildCount = lod.transform.GetChildCount(); 
			for(int j = 0; j < lodChildCount; j++)
			{
			    Transform srf = lod.transform.GetChild(j);
				Material mat = srf.renderer.material;
				float val = 0.5f + 0.5f * Mathf.Sin(critter_info.lifetime * frequency);
				//WemoLog.Eyal(srf.name + " " + val + " " + waldoTime);
				mat.SetFloat("_saturation", val);
			}	
		}
	}*/

/*	public void waldoFoundMaterialBlue(CritterInfo critter)
	{
		//WemoLog.Eyal("waldoFoundMaterial");
		StartCoroutine(saturateWaldoLodMaterialBlue(critter));
	}

	IEnumerator saturateWaldoLodMaterialBlue(CritterInfo critter_info)
	{
		bool fadeToColor = true;
		while(fadeToColor)
		{
			int numLOD = critter_info.critterLODData.LODs.Length;
			for(int x = 0; x < numLOD; x++)
			{
				GameObject lod = critter_info.critterLODData.LODs[x].LOD;
				int lodChildCount = lod.transform.childCount; 
				for(int j = 0; j < lodChildCount; j++)
				{
				    Transform srf = lod.transform.GetChild(j);
					Material mat = srf.renderer.material;
					float val = mat.GetFloat("_saturation") + 0.004f ;
					//WemoLog.Eyal(val);
					if(val > 1.0)
					{
						val = 1.0f;
						fadeToColor = false;
					}
					//mat.SetFloat("_saturation", val);
					Color col = new Color(val,val,1f,1f);
					mat.SetColor("_MainTexColor", col);
				}
			}
		
			yield return true; 	
		}
	}*/
	
/*	public static void adjustWaldoLodMaterialBlue(CritterInfo critter_info, float frequency)
	{
		int numLOD = critter_info.critterLODData.LODs.Length;
		float val = 0.7f + 0.3f * Mathf.Sin(critter_info.lifetime * 3f);
		Color col = new Color(val,val,1f,1f);
		for(int x = 0; x < numLOD; x++)
		{
			GameObject lod = critter_info.critterLODData.LODs[x].LOD;
			int lodChildCount = lod.transform.childCount; 
			for(int j = 0; j < lodChildCount; j++)
			{
			    Transform srf = lod.transform.GetChild(j);
				Material mat = srf.renderer.material;
				//WemoLog.Eyal(srf.name + " " + val + " " + waldoTime);
				mat.SetColor("_MainTexColor", col);
			}	
		}
	}
	
	public static void adjustWaldoLodMaterialYellow(CritterInfo critter_info, float frequency)
	{
		int numLOD = critter_info.critterLODData.LODs.Length;
		float val = 0.7f + 0.3f * Mathf.Sin(critter_info.lifetime * frequency);
		Color col = new Color(val,val,val,1f);
		for(int x = 0; x < numLOD; x++)
		{
			GameObject lod = critter_info.critterLODData.LODs[x].LOD;
			int lodChildCount = lod.transform.childCount; 
			for(int j = 0; j < lodChildCount; j++)
			{
			    Transform srf = lod.transform.GetChild(j);
				Material mat = srf.renderer.material;
				//WemoLog.Eyal(srf.name + " " + val + " " + waldoTime);
				mat.SetColor("_MainTexColor", col);
			}	
		}
	}*/
		
	public static void duplicateCritterMaterials( CritterInfo critter_info)
	{
//		GameObject cam = CameraManager.GetCurrentCamera().gameObject;//GameObject.Find("MainCamera");
//		critter_info.critterLODData.glowEffect = cam.GetComponent<GlowEffect>();
		
		int numLOD = critter_info.critterLODData.LODs.Length;
		int i = 0;
		bool deep = false;

		ShaderPointLightNode spln = critter_info.critterObject.GetComponent<ShaderPointLightNode>();
		if(spln) deep = spln.imDeepSea;
		int sphereId = App.SphereManager.LEGACY_GetCurrentSphere();
		
		//turn all lights off when it's not a deep-sea sphere
		Light[] lights = FindObjectsOfType(typeof(Light)) as Light[];
		
		//Debug for deep-sea
		//sphereId = 7;
		
		if(sphereId == 7)
		{
			foreach(Light light in lights){
				if(light.type == LightType.Point){
					light.enabled = true;
//					log.Trace("Turn on the light: " + light);
				}
			}
		}else{
			foreach(Light light in lights){
				if(light.type == LightType.Point){
					light.enabled = false;
//					log.Trace("Turn off the light: " + light);
				}
			}
		}//end turn all lights off 
		
		for(int x = 0; x < numLOD; x++)
		{
			GameObject lod = critter_info.critterLODData.LODs[x].LOD;
			int lodChildCount = lod.transform.childCount; 
			List<GameObject> created_gos = new List<GameObject>();
			for(int j = 0; j < lodChildCount; j++)
			{
			    Transform srf = lod.transform.GetChild(j);
				Material mat = srf.GetComponent<Renderer>().material;
				
				//assign the lod1 and lod2 switch distance to LOD0 / LOD1 materials
				if(x==0 && numLOD > 2){
					mat.SetFloat("_lod0Dist",critter_info.critterLODData.LODs[1].distance);
					mat.SetFloat("_lod1Dist",critter_info.critterLODData.LODs[2].distance);
				}
				if(x==1 && numLOD > 2){
					mat.SetFloat("_lod1Dist",critter_info.critterLODData.LODs[2].distance);
				}
				
				Material newMat = new Material(mat.shader);
				newMat.CopyPropertiesFromMaterial(mat);
				if( srf.GetComponent<Renderer>().material != null ) {
					Destroy( srf.GetComponent<Renderer>().material );
				}				
				srf.GetComponent<Renderer>().material = newMat;
				if(deep)
				{
//					log.Trace("found pointLight   sphereId "  + sphereId);
					if(sphereId != 7 )
					{
						if(srf.GetComponent<Renderer>().material.shader.name == "underwater_lit/BumpSpecBlend")
						{
							srf.GetComponent<Renderer>().material = spln.unlitMaterialBlend;
						}
						if(srf.GetComponent<Renderer>().material.shader.name == "underwater_lit/BumpBlend_Glow")
						{
							srf.GetComponent<Renderer>().material = spln.unlitMaterialBlend01;
						}
						if(srf.GetComponent<Renderer>().material.shader.name == "underwater_lit/Bump_Glow")
						{
							srf.GetComponent<Renderer>().material = spln.unlitMaterial01;
						}
						if(srf.GetComponent<Renderer>().material.shader.name == "underwater_lit/BumpSpec")
						{
							srf.GetComponent<Renderer>().material = spln.unlitMaterial;
						}
						if(srf.GetComponent<Renderer>().material.shader.name == "underwater_lit/Diffuse_Alpha_Ambient")
						{
							srf.GetComponent<Renderer>().material = spln.unlitMaterialBlend02;
						}
						if(srf.GetComponent<Renderer>().material.shader.name == "underwater_lit/Diffuse_Ambient")
						{
							srf.GetComponent<Renderer>().material = spln.unlitMaterial02;
						}
						if(srf.GetComponent<Renderer>().material.shader.name == "Self-Illumin/Diffuse")
						{
							srf.GetComponent<Renderer>().material = spln.illumination;
							log.Trace("changing material for angler normal " + spln.illumination);
						}
					}
					i++;
				}
				
				// adding a meshRenderer per lod srf				
				SkinnedMeshRenderer smr = srf.GetComponent<SkinnedMeshRenderer>();
				
				// if no skinned mesh renderer, skip.
				if( smr == null ) {
					continue;
				}
				
				Mesh mesh = smr.sharedMesh;
				Material meshMat = smr.material;

				// make a new surface and parent it	
				GameObject go = new GameObject();
				created_gos.Add(go);
				
				MeshFilter mf = go.AddComponent<MeshFilter>();
				//WemoLog.Eyal("added mf " + mf + " to lod " + go.name);
				mf.sharedMesh = mesh;
				MeshRenderer mr = go.AddComponent<MeshRenderer>();
				//WemoLog.Eyal("added mr " + mr + " to lod " + go.name);
				Material[] ar = new Material[4] ;
				ar[0] = meshMat;
				ar[1] = meshMat;
				ar[2] = meshMat;
				ar[3] = meshMat;
				mr.materials = ar;
				go.name = srf.gameObject.name + "_unskinned";
				mr.enabled = false;
			}	
			
			// give each created object our lod as parent.
			// we cannot add the parent within the loop above because we are traversing the
			// lod's children and this operation may reorder them.
			foreach( GameObject created_obj in created_gos ) {
				created_obj.transform.parent = lod.transform;
			}
		}
	}
		
/*	public static void SetStaticMesh( CritterInfo critter_info, bool setStatic)
	{
		int numLOD = critter_info.critterLODData.LODs.Length;
		
		for(int x = 0; x < numLOD; x++)
		{
			GameObject lod = critter_info.critterLODData.LODs[x].LOD;
			int lodChildCount = lod.transform.childCount; 
			for(int j = 0; j < lodChildCount; j++)
			{
			    Transform srf = lod.transform.GetChild(j);
				SkinnedMeshRenderer smr = srf.GetComponent<SkinnedMeshRenderer>();
				MeshRenderer  mr = srf.GetComponent<MeshRenderer>();
				if(setStatic){
					if(smr) smr.enabled = false;
					if(mr) mr.enabled = true;
					critter_info.critterLODData.staticMesh = true;
				}
				else{
					if(smr) smr.enabled = true;
					if(mr) mr.enabled = false;
					critter_info.critterLODData.staticMesh = false;
				}
			}	
		}
	}*/
	
/*	public static void ClearCritterGlow( CritterInfo critter_info ) 
	{
		if( _glowCritter == critter_info ) {
			_glowActive = false;
			_glowCritter = null;
		}
	}*/
	
/*	public static void critterGlow( CritterInfo critter_info, bool isGlow)
	{
		_glowActive = isGlow;	
		if( isGlow ) {
			_glowCritter = critter_info;
		}
		else {
			_glowCritter = null;
		}
			
		bool glowShader = true; // if true glow else outline
		if(glowShader)
		{
			//WemoLog.Eyal("glow fish " + critter_info.critterObject.name);
			int lodChildCount = critter_info.critterTransform.childCount; 
			for(int j = 0; j < lodChildCount; j++)
			{
			    Transform srf = critter_info.critterTransform.GetChild(j);
				if(srf.name=="glow")
				{
					if(isGlow) srf.gameObject.tag = "glow";
					else srf.gameObject.tag = "";
				}	
			}
		}
		else // outline shader
		{
			int numLOD = critter_info.critterLODData.LODs.Length;
			for(int x = 0; x < numLOD; x++)
			{
				GameObject lod = critter_info.critterLODData.LODs[x].LOD;
				int lodChildCount = lod.transform.childCount); 
				for(int j = 0; j < lodChildCount; j++)
				{
				    Transform srf = lod.transform.GetChild(j);
					if(isGlow) srf.renderer.material = critter_info.critterLODData.waldoMaterial;
					else srf.renderer.material = critter_info.critterLODData.LODs[x].LODmaterial;
				}	
			}
		}
		
	}*/
	
/*	public void flashMaterial(CritterInfo critter)
	{
		//WemoLog.Eyal("flashMaterial");
		StartCoroutine(flashLodMaterial(critter));
	}
	
	IEnumerator flashLodMaterial(CritterInfo critter_info)
	{
		bool keepFlashing = true;
		glowEffect.glowIntensity = 0f;
		LODModelData.critterGlow(critter_info,true);
		while(keepFlashing)
		{
			glowEffect.glowIntensity += 0.1f;
			if(glowEffect.glowIntensity >= 1.5f){
				keepFlashing = false;
				critter_info.critterLODData.flashMaterialOff(critter_info);
			}
			yield return true; 	
		}
	}*/
	
/*	public void flashMaterialOff(CritterInfo critter)
	{
		//WemoLog.Eyal("flashMaterial");
		StartCoroutine(flashLodMaterialOff(critter));
	}
	
	IEnumerator flashLodMaterialOff(CritterInfo critter_info)
	{
		bool keepFlashing = true;
		while(keepFlashing)
		{
			glowEffect.glowIntensity -= 0.1f;
			if(glowEffect.glowIntensity <= 0f ) {
				keepFlashing = false;
				LODModelData.critterGlow(critter_info,false);
			}
			yield return true; 	
		}
	}*/
}
