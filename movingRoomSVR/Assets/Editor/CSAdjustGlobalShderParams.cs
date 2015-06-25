using UnityEngine;
using UnityEditor;
using System.Collections;

public class CSAdjustGlobalShderParams 
{
	public  static float dist = 1000;
	public  static float deepestPoint = 2600;
	public  static float deepestPointBrightness = 0;
	public  static  Color deepColor = new Color(0.003f,0.01f,0.125f,0f); // 24M is deep
	public  static Color middleColor = new Color(0.015f,0.2f,0.4f,0f); 
	public  static Color shallowColor = new Color(0.03f,0.386f,0.8f,0f); 
	public  static float depthPower = 1.0f;
	public  static float depthStartVal = 0.1f;
	public  static Vector4 keyLightDir = new Vector4(0.6f,-1f,0f,1f);
	public  static Color keyLightColor = new Color(1f,1f,1f,1f);
	public  static float keyLightIntensity = 1.5f;
	public  static Color ambLightColor = new Color(0.1f,0.1f,0.1f,1f);
	public  static float causticsTileMult = 1.0f;
	public  static float causticsMult = 1.0f;
	public  static Transform lightTransform = null;			
	
	public  static float glow_glowIntensity = 1.6f;
	public  static int glow_blurIterations = 5;
	public  static float glow_blurSpread = 1f;
	public  static Color glow_glowTint = new Color(1f,1f,1f,0f);
	
	
	
	[MenuItem("WEMOTools/Adjust Global Shader Params")]
	public static void AdjustGlobalShaderParams ()
	{
		GameObject master = GameObject.Find("MasterOceanObject") as GameObject;
		
		if( master != null ) 
		{
			GlobalOceanShaderAdjust globalAdjust = master.GetComponent("GlobalOceanShaderAdjust") as GlobalOceanShaderAdjust;
			dist = globalAdjust.dist;
			deepestPoint = globalAdjust.deepestPoint;
			deepestPointBrightness = globalAdjust.deepestPointBrightness;
			deepColor = globalAdjust.deepColor;
			middleColor =  globalAdjust.middleColor;
			shallowColor = globalAdjust.shallowColor;
			depthPower = globalAdjust.depthPower;
			depthStartVal = globalAdjust.depthStartVal;
			keyLightDir = globalAdjust.keyLightDir;
			keyLightColor = globalAdjust.keyLightColor;
			keyLightIntensity = globalAdjust.keyLightIntensity;
			ambLightColor = globalAdjust.ambLightColor;
			causticsTileMult = globalAdjust.causticsTileMult;
			causticsMult = globalAdjust.causticsMult;
			lightTransform = globalAdjust.lightTransform;
			
			glow_glowIntensity = globalAdjust.glow_glowIntensity;
			glow_blurIterations = globalAdjust.glow_blurIterations;
			glow_blurSpread = globalAdjust.glow_blurSpread;
			glow_glowTint = globalAdjust.glow_glowTint;
			
			if( globalAdjust != null ) 
			{
				AdjustToParams( false );
			}
		}
		
	}
	public static void AdjustToParams(bool do_planes) {		
		Shader.SetGlobalFloat("_DepthMax",dist);	
		Shader.SetGlobalFloat("_DeepestPoint",deepestPoint);
		Shader.SetGlobalFloat("_DeepestPointBrightness",deepestPointBrightness);
		Shader.SetGlobalColor("_DeepColor",deepColor);
		Shader.SetGlobalColor("_MiddleColor",middleColor);
		Shader.SetGlobalColor("_ShallowColor",shallowColor);
		Shader.SetGlobalFloat("_DepthPower",depthPower);
		Shader.SetGlobalFloat("_DepthStartVal",depthStartVal);	
		Vector3 keyLightDirNorm = new Vector3(keyLightDir.x,keyLightDir.y,keyLightDir.z).normalized;
		if(lightTransform != null) 
		{
			keyLightDirNorm = lightTransform.forward;
			Shader.SetGlobalMatrix("_lightMatrix", lightTransform.localToWorldMatrix);
       	}
		Shader.SetGlobalVector("_lightDir",keyLightDirNorm);
		Shader.SetGlobalColor("_keyLightColor",keyLightColor);
		Shader.SetGlobalFloat("_keyLightIntensity",keyLightIntensity);
		Shader.SetGlobalColor("_ambLightColor",ambLightColor);
		Shader.SetGlobalFloat("_causticsTileMult",causticsTileMult);
		Shader.SetGlobalFloat("_causticsMult",causticsMult);
 		
		if( do_planes ) {
			float mult = dist/1500;
			GameObject[] camPlanes = GameObject.FindGameObjectsWithTag("CamPlane");
			
			foreach( GameObject camPlane in camPlanes )
			{
				camPlane.transform.localPosition = new Vector3(0,0,dist);
				camPlane.transform.localScale = new Vector3(720*mult,180*mult,180*mult);
				//sp.transform.localScale = Vector3(mult,mult,mult);
				//var spMesh : Mesh = sp.GetComponent(MeshFilter).mesh;
			}
		}

		CameraManager.singleton.farClipPlane = dist + 2;
		
		//Glow adjust
//		WemoLog.Eyal("#############################     glow adjust        ###############################");
//		WemoLog.Eyal(Camera.main.gameObject.name + " glowIntensity " + glow_glowIntensity + " blurIterations " + glow_blurIterations + " blurSpread " + glow_blurSpread + " glowTint " + glow_glowTint);
		/*GlowEffect ge = Camera.main.gameObject.GetComponent<GlowEffect>();
		ge.glowIntensity = glow_glowIntensity;
		ge.blurIterations = glow_blurIterations;
		ge.blurSpread = glow_blurSpread;
		ge.glowTint = glow_glowTint;
		XCamera.SetGlowEffect (glow_glowIntensity, glow_blurIterations, glow_blurSpread, glow_glowTint);
		*/
	}

}

