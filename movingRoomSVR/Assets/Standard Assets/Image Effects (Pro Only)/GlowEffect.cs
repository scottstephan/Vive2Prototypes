using UnityEngine;

// Glow uses the alpha channel as a source of "extra brightness".
// All builtin Unity shaders output baseTexture.alpha * color.alpha, plus
// specularHighlight * specColor.alpha into that.
// Usually you'd want either to make base textures to have zero alpha; or
// set the color to have zero alpha (by default alpha is 0.5).
 
[ExecuteInEditMode]
[RequireComponent (typeof(Camera))]
[AddComponentMenu("Image Effects/Glow")]
public class GlowEffect : MonoBehaviour
{
	/// The brightness of the glow. Values larger than one give extra "boost".
	public float glowIntensity = 1.5f;
	
	/// Blur iterations - larger number means more blur.
	public int blurIterations = 3;
	
	/// Blur spread for each iteration. Lower values
	/// give better looking blur, but require more iterations to
	/// get large blurs. Value is usually between 0.5 and 1.0.
	public float blurSpread = 0.7f;
	
	/// Tint glow with this color. Alpha adds additional glow everywhere.
	public Color glowTint = new Color(1,1,1,0);
	
	public string glowTag ;
	public Shader addAlphaHackShader;
	public Material mat;
	public Material mat1;
	
	
	[HideInInspector]
	public Transform myXform;
	
	
	// --------------------------------------------------------
	// The final composition shader:
	//   adds (glow color * glow alpha * amount) to the original image.
	// In the combiner glow amount can be only in 0..1 range; we apply extra
	// amount during the blurring phase.
	
    public Shader compositeShader;
    Material m_CompositeMaterial = null;
	protected Material compositeMaterial {
		get {
			if (m_CompositeMaterial == null) {
                m_CompositeMaterial = new Material(compositeShader);
				m_CompositeMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_CompositeMaterial;
		} 
	}
	
	
	// --------------------------------------------------------
	// The blur iteration shader.
	// Basically it just takes 4 texture samples and averages them.
	// By applying it repeatedly and spreading out sample locations
	// we get a Gaussian blur approximation.
	// The alpha value in _Color would normally be 0.25 (to average 4 samples),
	// however if we have glow amount larger than 1 then we increase this.

    public Shader blurShader;
    Material m_BlurMaterial = null;
	protected Material blurMaterial {
		get {
			if (m_BlurMaterial == null) {
                m_BlurMaterial = new Material(blurShader);
                m_BlurMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_BlurMaterial;
		} 
	}
	
	
	// --------------------------------------------------------
	// The image downsample shaders for each brightness mode.
	// It is in external assets as it's quite complex and uses Cg.
	public Shader downsampleShader;
	Material m_DownsampleMaterial = null;
	protected Material downsampleMaterial {
		get {
			if (m_DownsampleMaterial == null) {
				m_DownsampleMaterial = new Material( downsampleShader );
				m_DownsampleMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_DownsampleMaterial;
		} 
	}
	

	// --------------------------------------------------------
	//  finally, the actual code
	
	protected void OnDisable()
	{
		if( m_CompositeMaterial ) {
			DestroyImmediate( m_CompositeMaterial );
		}
		if( m_BlurMaterial ) {
			DestroyImmediate( m_BlurMaterial );
		}
		if( m_DownsampleMaterial )
			DestroyImmediate( m_DownsampleMaterial );
	}
	
	protected void Start()
	{
		// Disable if we don't support image effects
		if (!SystemInfo.supportsImageEffects)
		{
			enabled = false;
			return;
		}
		
		// Disable the effect if no downsample shader is setup
		if( downsampleShader == null )
		{
			//WemoLog.Log("No downsample shader assigned! Disabling glow.");
			enabled = false;
		}
		// Disable if any of the shaders can't run on the users graphics card
		else
		{		
			if( !blurMaterial.shader.isSupported )
				enabled = false;
			if( !compositeMaterial.shader.isSupported )
				enabled = false;
			if( !downsampleMaterial.shader.isSupported )
				enabled = false;
		}
		
		
		if(!mat) {
	        mat = new Material( "Shader \"Hidden/SetAlpha\" {" +
	            "SubShader {" +
				"Tags"+
				"{"+
				"\"RenderType\"=\"Opaque\""+
				"\"Queue\" = \"Background\""+
				"}"+
	            "    Pass {" +
	            "        ZTest Always Cull Off ZWrite Off" +
	            "        ColorMask A" +
	            "        Color (1,1,1,0)" +
	            "    }" +
	            "}" +
	            "}"
	        );
	    }
		
		
		if(!mat1) 
		{
			
	        mat1 = new Material( "Shader \"Hidden/SetAlpha1\" {" +
	            "SubShader {" +
				"Tags"+
				"{"+
				"\"RenderType\"=\"Transparent\""+
				"\"Queue\" = \"Overlay+1000\""+
				"}"+
	            "    Pass {" +
	            "        ZTest Always Cull Off ZWrite Off" +
	            "        ColorMask A" +
	            "        Color (1,1,1,0.1)" +
	            "    }" +
	            "}" +
	            "}"
	        );
	        
			//mat1 = new Material(addAlphaWaldo);
	    }
		
		myXform = transform;
		
	}
	
	// Performs one blur iteration.
	public void FourTapCone (RenderTexture source, RenderTexture dest, int iteration)
	{
		float off = 0.5f + iteration*blurSpread;

		// discard dest first to avoid perf warning in Unity for Android
		dest.DiscardContents(true, true);

		Graphics.BlitMultiTap (source, dest, blurMaterial,
            new Vector2( off, off),
			new Vector2(-off, off),
            new Vector2( off,-off),
            new Vector2(-off,-off)
		);
	}
	
	// Downsamples the texture to a quarter resolution.
	private void DownSample4x (RenderTexture source, RenderTexture dest)
	{
		downsampleMaterial.color = new Color( glowTint.r, glowTint.g, glowTint.b, glowTint.a/4.0f );
		Graphics.Blit (source, dest, downsampleMaterial);
	}
	

	
	void OnPostRender()
	{
	    // Draw a quad over the whole screen with the above shader
		
	    GL.PushMatrix ();
	    GL.LoadOrtho ();
	    //for (int j = 0; j < mat.passCount; ++j) {
	        mat.SetPass (0);
	        GL.Begin( GL.QUADS );
	        GL.Vertex3( 0f, 0f, 0.1f );
	        GL.Vertex3( 1f, 0f, 0.1f );
	        GL.Vertex3( 1f, 1f, 0.1f );
	        GL.Vertex3( 0f, 1f, 0.1f );
	        GL.End();
	    //}
	    GL.PopMatrix ();    
		

		mat1.SetPass(0);
		// render alpha
		if(glowTag != "") 
		{	
			GameObject[] gos = GameObject.FindGameObjectsWithTag(glowTag);
			for (int i=0; i < gos.Length ; i++)
			{
				GameObject go = gos[i];
				go.GetComponent<Renderer>().enabled = true;
				
				Transform critterObject = go.transform.parent;
				GeneralSpeciesData gsd = critterObject.GetComponent<GeneralSpeciesData>();
				//WemoLog.Eyal("parent " + critterObject.name + " size " );
				
				Vector3 diff = myXform.position - critterObject.transform.position;
				float dist = diff.magnitude;
				float sc = MathfExt.Fit(dist,gsd.minDistGlowScale,gsd.maxDistGlowScale,1f,gsd.maxGlowFactor);
				//WemoLog.Eyal("dist "  + dist + " sc " + sc + " maxScale " + gsd.maxGlowFactor); 
				//Debug.Log("gameObject " + go.name);
					
				
				go.transform.localScale = new Vector3(sc, sc, sc);
				MeshFilter mf = go.GetComponent<MeshFilter>();
				if(mf) 
				{
					Mesh mesh = mf.sharedMesh;
					if(mesh)
					{
						Graphics.DrawMeshNow(mesh,go.transform.localToWorldMatrix);
					}
					//Debug.Log("MeshFilter " + go.name);
				}
				SkinnedMeshRenderer smf = go.GetComponent<SkinnedMeshRenderer>();
				if(smf) 
				{
					Mesh mesh = smf.sharedMesh;
					if(mesh) Graphics.DrawMeshNow(mesh,go.transform.localToWorldMatrix);
					//Debug.Log("SkinnedMeshRenderer " + go.name);
				}
				//go.transform.localScale = new Vector3(1f,1f,1f);
				go.GetComponent<Renderer>().enabled = false;
			}		
		}	
	}
	
	
 // Called by the camera to apply the image effect
  void OnRenderImage (RenderTexture source, RenderTexture destination)
  {
    
    //camera.cullingMask = 0;
    // Clamp parameters to sane values
    glowIntensity = Mathf.Clamp( glowIntensity, 0.0f, 10.0f );
    blurIterations = Mathf.Clamp( blurIterations, 0, 30 );
    blurSpread = Mathf.Clamp( blurSpread, 0.5f, 1.0f );
    
    RenderTexture buffer = RenderTexture.GetTemporary(source.width/4, source.height/4, 0);
    RenderTexture buffer2 = RenderTexture.GetTemporary(source.width/4, source.height/4, 0);
    
    //ee make alpha 0
    //alpha0Material.color = new Color(1f,1f,1f,1f);
    //RenderTexture bufferNoAlpha = RenderTexture.GetTemporary(source.width, source.height, 0);    
    //Graphics.Blit(source,bufferNoAlpha,alpha0Material);
    //Graphics.Blit(source,bufferNoAlpha);
    
  
    
    
    // Copy source to the 4x4 smaller texture.
    DownSample4x (source, buffer);
    
    
    
    // Blur the small texture
    float extraBlurBoost = Mathf.Clamp01( (glowIntensity - 1.0f) / 4.0f );
    blurMaterial.color = new Color( 1F, 1F, 1F, 0.25f + extraBlurBoost );
    
    bool oddEven = true;
    for(int i = 0; i < blurIterations; i++)
    {
      if( oddEven )
        FourTapCone (buffer, buffer2, i);
      else
        FourTapCone (buffer2, buffer, i);
      oddEven = !oddEven;
    }

    Graphics.Blit(source,destination);
        
    if( oddEven )
      BlitGlow(buffer, destination);
    else
      BlitGlow(buffer2, destination);
    

    //Graphics.Blit(source,destination,alpha0Material);
    
    
	RenderTexture.ReleaseTemporary(buffer);
    RenderTexture.ReleaseTemporary(buffer2);
    //RenderTexture.ReleaseTemporary(bufferNoAlpha);
  }
	//void OnPreRender()
	//{
	//	camera.cullingMask = 1<<0|1<<1|1<<2|1<<3|1<<4|1<<5|1<<6|1<<7|1<<8|1<<9|1<<10|1<<11|1<<12|1<<13|1<<14|1<<15;
	//}

	
	public void BlitGlow( RenderTexture source, RenderTexture dest )
	{
		compositeMaterial.color = new Color(1F, 1F, 1F, Mathf.Clamp01(glowIntensity));
		Graphics.Blit (source, dest, compositeMaterial);
	}	
}
