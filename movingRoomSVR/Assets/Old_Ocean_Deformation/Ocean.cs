using UnityEngine;
using System.Collections;

public class Ocean : MonoBehaviour {
	
	public bool useCaptureScreenshotTime = false;
	public float timeScale = 1;
	public int width = 32;
	public int height = 32;
	private int wh_size;
	public int s = 2;
	public float scale = 0.4f;
	public Vector3 size = new Vector3 (150.0f, 1.0f, 150.0f);
	public float tiles_x = 20f;
	public float tiles_y = 20f;
	public float windx = 10.0f;
	public float normal_scale = 16f;
	public float normalStrength = 1.0f;
	public float choppy_scale = 4.0f;
	public float uv_speed = 0.01f;
	public Material material_ocean;
	public Material material_plane;
	public Material material_underwater;
	public float delayx = 10;
	public float delayxtemp = 0;

	private int max_LOD = 4;
	private int[] max_LOD_POW;
	private ComplexF[] h0;
	public Texture2D[] caustics;
	private int framesPerSecond = 10;

	private ComplexF[] t_x;
//	private ComplexF[] t_y;

	private ComplexF[] n0;
	private ComplexF[] n_x;
	private ComplexF[] n_y;

	private ComplexF[] data;
//	private ComplexF[] data_x;

	private Color[] pixelData;
	private Texture2D textureA;
	private Texture2D textureB;

	private Vector3[] baseHeight;
	private Vector2[] baseUV;

	private Mesh baseMesh = null;

//	private GameObject child;

	private ArrayList tiles_LOD;
	private ArrayList tangentsLOD;
	private ArrayList verticesLOD;
	private ArrayList normalsLOD;
	private ArrayList uvLOD;
	
	private int g_height;
	private int g_width;
	private int g_wh_size;

	private int n_width;
	private int n_height;
	private int n_wh_size;
	
//	private bool drawFrame = true;
	
	private bool normalDone = false;
	
	private bool reflectionRefractionEnabled = false;
	private Camera offscreenCam;
	private RenderTexture reflectionTexture = null;
	private RenderTexture refractionTexture = null;
	
	// public variables
	public float foamTime = 1.3f;
	public Texture2D foamTexture;
	public Texture2D fresnelTexture;
	public Texture2D bumpTexture;
	public Texture2D maintexTexture;
	public Texture2D causticsTexture;
	public Color surfaceColor;
	public Color waterColor;
	public Color mainColor;
	public Color specularColor;
	public Color transparency;
	public Color sunColor;
	public Light sunposition;
	public float refractionlevel = 0.05f;
	public float sunpower = 0.05f;
//	private bool forceOriginalShader = false;
	private Camera mycam;
	
	private Shader shader = null;
//	private Shader shader_plane = null;
//	private Shader shader_underwater = null;
	
	private float waterDirtyness = 0.5f;
	
	
	
	
	private Vector2[] uvs;
	private Vector3[] vertices;
	private Vector3[] normals;
	private Vector4[] tangents;
	
//	private float oceansize;	
	
	//################################
	//#########          pause     ##############
	//################################
	private bool paused;
	private float pausedTime;
	private float pausedTimeOffset;
	void DidPause( bool new_value )
	{
		paused = new_value;
		
		if( paused )
		{
			pausedTime = Time.time;
		}
		else
		{
			pausedTimeOffset += Time.time - pausedTime;
		}
	}
	//################################
	//#########   end pause      ##############
	//################################
	
	
	
	
	
	
	
	float GetWaterHeightAtLocation ( float x, float y ) {
		x = x / size.x;
		x = (x-Mathf.Floor (x)) * width;
		y = y / size.z;
		y = (y-Mathf.Floor (y)) * height;
	
		// TODO: Interpolate
		return data[width * Mathf.FloorToInt(y) + Mathf.FloorToInt(x)].GetModulus() * scale / wh_size;
	}
	
	float GaussianRnd (int x, int y) {
		float x1 = Random.value;
		float x2 = Random.value;
		
		if (x1 == 0.0f)
			x1 = 0.01f;
		
		return Mathf.Sqrt (-2.0f * Mathf.Log (x1)) * Mathf.Cos (MathfExt.TWO_PI * x2);
	
		
		//return Mathf.Cos(x/73+y*81);
	}
	
	// Phillips spectrum
	float P_spectrum ( Vector2 vec_k, Vector2 wind ) {
		float A = vec_k.x > 0.0f ? 1.0f : 0.05f; // Set wind to blow only in one direction - otherwise we get turmoiling water
		
		float L = wind.sqrMagnitude / 9.81f;
		float k2 = vec_k.sqrMagnitude;
		// Avoid division by zero
		if (vec_k.magnitude == 0.0f) {
			return 0.0f;
		}
		return A * Mathf.Exp (-1.0f / (k2*L*L) - Mathf.Pow (vec_k.magnitude * 0.1f, 2.0f)) / (k2*k2) * Mathf.Pow (Vector2.Dot (vec_k/vec_k.magnitude, wind/wind.magnitude), 2.0f);// * wind_x * wind_y;
	}
	
	void Start () 
	{
		mycam =  CameraManager.GetCurrentCamera();
		
		max_LOD_POW = new int[max_LOD];
		for(int i = 0; i < max_LOD; i++ ) {
			max_LOD_POW[i] = Mathf.RoundToInt(Mathf.Pow(2.0f,i));
		}
		wh_size = width * height;
//		oceansize = width*s*scale*(max_LOD*2+1);
		
		// normal map size
		n_width = 256;
		n_height = 256;
		n_wh_size = n_width * n_height;
		
		textureA = new Texture2D(n_width, n_height);
		textureB = new Texture2D(n_width, n_height);
		textureA.filterMode = FilterMode.Bilinear;
		textureB.filterMode = FilterMode.Bilinear;
	
		// ee edit to avoid error comment out
		/*
		if (!SetupOffscreenRendering())
		{
			material_ocean.SetTexture ("_BumpMap", textureA);
			material_ocean.SetTextureScale ("_BumpMap", Vector2(normal_scale, normal_scale));
	
			material_ocean.SetTexture ("_BumpMap2", textureB);
			material_ocean.SetTextureScale ("_BumpMap2", Vector2(normal_scale, normal_scale));
		}
		*/
		
		pixelData = new Color[n_wh_size];
	
		// Init the water height matrix
		data = new ComplexF[wh_size];
		// lateral offset matrix to get the choppy waves
//		data_x = new ComplexF[wh_size];
	
		// tangent
		t_x = new ComplexF[wh_size];
//		t_y = new ComplexF[wh_size];
	
		n_x = new ComplexF[n_wh_size];
		n_y = new ComplexF[n_wh_size];
	
		// Geometry size
		g_height = height + 1;	
		g_width = width + 1;
		g_wh_size = g_width * g_height;
		
		tiles_LOD = new ArrayList();
		
		for (int LOD=0;LOD<max_LOD*max_LOD;LOD++) {
			tiles_LOD.Add (new ArrayList());
	 	}
	
		tangentsLOD = new ArrayList();
		verticesLOD = new ArrayList();
		normalsLOD = new ArrayList();
		uvLOD = new ArrayList();
		
		GameObject tile;
		int chDist; // Chebychev distance	
		for (int y=0;y<tiles_y;y++) {
			for (int x=0;x<tiles_x;x++) {
				
				chDist = Mathf.RoundToInt(Mathf.Max (Mathf.Abs (tiles_y/2 - y), Mathf.Abs (tiles_x/2 - x)));
				chDist = chDist > 0 ? chDist-1 : 0;
				//WemoLog.Log(chDist);
				float cy = y-tiles_y/2;
				float cx = x-tiles_x/2;
				tile = new GameObject ("Tile"+chDist);
				
				tile.transform.position = new Vector3( Mathf.RoundToInt(cx * size.x * s),  Mathf.RoundToInt(-2.0f * chDist), Mathf.RoundToInt(cy * size.z*s));
				tile.transform.localScale = new Vector3(s,s,s);
				tile.AddComponent<MeshFilter>();
				tile.AddComponent<MeshRenderer>();
				tile.GetComponent<Renderer>().material = material_ocean;
	
				//Make child of this object, so we don't clutter up the
				//scene hierarchy more than necessary.
				tile.transform.parent = transform;
				
				//Also we don't want these to be drawn while doing refraction/reflection passes,
				//so we'll add the to the water layer for easy filtering.
				tile.layer = LayerMask.NameToLayer("Water");
				
				// Determine which LOD the tile belongs
				((ArrayList)tiles_LOD[chDist]).Add (tile.GetComponent<MeshFilter>().mesh);
			}
		}
	
		
		// Init wave spectra. One for vertex offset and another for normal map
		h0 = new ComplexF[wh_size];
		n0 = new ComplexF[n_wh_size];
	
		// Wind restricted to one direction, reduces calculations
		Vector2 wind = new Vector2 (windx, 0.0f);
	
		// Initialize wave generator	
		for (int y=0;y<height;y++) {
			for (int x=0;x<width;x++) {
				int yc = y < height / 2 ? y : -height + y;
				int xc = x < width / 2 ? x : -width + x;
				Vector2 vec_k = new Vector2 (MathfExt.TWO_PI * xc / size.x, MathfExt.TWO_PI * yc / size.z);
				h0[width * y + x] = new ComplexF ( GaussianRnd(x,y), GaussianRnd(x,y)) * 0.707f * Mathf.Sqrt (P_spectrum (vec_k, wind));
			}
		}
	
		for (int y=0;y<n_height;y++) {
			for (int x=0;x<n_width;x++) {	
				int yc = y < n_height / 2 ? y : -n_height + y;
				int xc = x < n_width / 2 ? x : -n_width + x;
				Vector2 vec_k = new Vector2 (MathfExt.TWO_PI * xc / (size.x / normal_scale), MathfExt.TWO_PI * yc / (size.z / normal_scale));
				n0[n_width * y + x] = new ComplexF ( GaussianRnd(x,y), GaussianRnd(x,y)) * 0.707f * Mathf.Sqrt (P_spectrum (vec_k, wind));
			}
		}
		GenerateHeightmap ();
		GenerateBumpmaps ();
	}

	void GenerateBumpmaps () {
		if (!normalDone) { 
			for (int idx=0;idx<2;idx++) {
				for (int y = 0;y<n_height;y++) {
					for (int x = 0;x<n_width;x++) {	
						int yc = y < n_height / 2 ? y : -n_height + y;
						int xc = x < n_width / 2 ? x : -n_width + x;
						Vector2 vec_k = new Vector2 (MathfExt.TWO_PI * xc / (size.x / normal_scale), 2.0f * Mathf.PI * yc / (size.z / normal_scale));
	
						float iwkt = idx == 0 ? 0.0f : MathfExt.PI_2;
						ComplexF coeffA = new ComplexF (Mathf.Cos (iwkt), Mathf.Sin (iwkt));
						ComplexF coeffB = coeffA.GetConjugate();
	
						int ny = y > 0 ? n_height - y : 0;
						int nx = x > 0 ? n_width - x : 0;
	
						n_x[n_width * y + x] = (n0[n_width * y + x] * coeffA + n0[n_width * ny + nx].GetConjugate() * coeffB) * new ComplexF (0.0f, -vec_k.x);				
						n_y[n_width * y + x] = (n0[n_width * y + x] * coeffA + n0[n_width * ny + nx].GetConjugate() * coeffB) * new ComplexF (0.0f, -vec_k.y);				
					}
				}
				Fourier.FFT2 (n_x, n_width, n_height, FourierDirection.Backward);
				Fourier.FFT2 (n_y, n_width, n_height, FourierDirection.Backward);
	
				for (int i=0; i<n_wh_size; i++){
					Vector3 bump = new Vector3 (n_x[i].Re*Mathf.Abs(n_x[i].Re), n_y[i].Re*Mathf.Abs(n_y[i].Re), n_wh_size / scale / normal_scale * normalStrength).normalized * 0.5f;
					pixelData[i] = new Color (bump.x + 0.5f, bump.y + 0.5f, bump.z + 0.8f);
					//			pixelData[i] = Color (0.5, 0.5, 1.0);			
				}
				if (idx==0) {
					textureA.SetPixels (pixelData, 0);
					textureA.Apply();
				}
				else {
					textureB.SetPixels (pixelData, 0);
					textureB.Apply();
				}
			}
			normalDone = true;
		}
		
	}
	
	void GenerateHeightmap () {
	
		Mesh mesh = new Mesh();
	
		int y = 0;
		int x = 0;
	
		// Build vertices and UVs
		Vector3[] vertices = new Vector3[g_wh_size];
		Vector4[] tangents = new Vector4[g_wh_size];
		Vector2[] uv = new Vector2[g_wh_size];
	
		Vector2 uvScale = new Vector2 (1.0f / (g_width - 1), 1.0f / (g_height - 1));
		Vector3 sizeScale = new Vector3 (size.x / (g_width - 1), size.y, size.z / (g_height - 1));
	
		for (y=0;y<g_height;y++) {
			for (x=0;x<g_width;x++) {
				Vector3 vertex = new Vector3 (x, 0.0f, y);
				vertices[y * g_width + x] = Vector3.Scale (sizeScale, vertex);
				uv[y*g_width + x] = Vector2.Scale (new Vector2 (x, y), uvScale);
			}
		}
		
		mesh.vertices = vertices;
		mesh.uv = uv;
	
		for (y=0;y<g_height;y++) {
			for (x=0;x<g_width;x++) {
				tangents[y*g_width + x] = new Vector4 (1.0f, 0.0f, 0.0f, -1.0f);
			}
		}
		mesh.tangents = tangents;	
		
		for (int LOD=0;LOD<max_LOD;LOD++) {
			int cnt = Mathf.RoundToInt((height/max_LOD_POW[LOD]+1) * (width/max_LOD_POW[LOD]+1));
			Vector3[] verticesLOD = new Vector3[cnt];
			Vector2[] uvLOD = new Vector2[cnt];
			int idx = 0;
	 
			for (y=0;y<g_height;y+=max_LOD_POW[LOD]) {
				for (x=0;x<g_width;x+=max_LOD_POW[LOD]) {
					verticesLOD[idx] = vertices[g_width * y + x];
					uvLOD[idx++] = uv[g_width * y + x];
				}			
			}
			for (int k=0;k<((ArrayList)tiles_LOD[LOD]).Count;k++) {
				Mesh meshLOD = ((Mesh)((ArrayList)tiles_LOD[LOD])[k]);
				meshLOD.vertices = verticesLOD;
				meshLOD.uv = uvLOD;
			}		
		}
	
		// Build triangle indices: 3 indices into vertex array for each triangle
		for (int LOD=0;LOD<max_LOD;LOD++) 
		{
			int index = 0;
			int width_LOD = Mathf.RoundToInt(width / max_LOD_POW[LOD] +1);
			int[] triangles = new int[Mathf.RoundToInt((height/max_LOD_POW[LOD] * width/max_LOD_POW[LOD]) * 6)];
			for (y=0;y<height/max_LOD_POW[LOD];y++) {
				for (x=0;x<width/max_LOD_POW[LOD];x++) {
					// For each grid cell output two triangles
					triangles[index++] = (y * width_LOD) + x;
					triangles[index++] = ((y+1) * width_LOD) + x ;
					triangles[index++] = (y     * width_LOD) + x + 1;
	
					triangles[index++] = ((y+1) * width_LOD) + x;
					triangles[index++] = ((y+1) * width_LOD) + x + 1;
					triangles[index++] = (y* width_LOD) + x + 1;
				}
			}
			for (int k=0;k<((ArrayList)tiles_LOD[LOD]).Count;k++) {
				Mesh meshLOD = ((Mesh)((ArrayList)tiles_LOD[LOD])[k]);
				meshLOD.triangles = triangles;
			}
		}
		
		baseMesh = mesh;
	}
	
	/*
	Prepares the scene for offscreen rendering; spawns a camera we'll use for for
	temporary renderbuffers as well as the offscreen renderbuffers (one for
	reflection and one for refraction).
	*/
	bool SetupOffscreenRendering()
	{
		//Check for rendertexture support and return false if not supported
		if( !SystemInfo.supportsRenderTextures)
			return false;
			
		
		shader = Shader.Find("Ocean/OceanMain_v1");
		
		//Bail out if the shader could not be compiled
		if (shader == null)
			return false;
		
		if (!shader.isSupported)
			return false;
			
		//TODO: More fail-tests?
		
			
		refractionTexture = new RenderTexture(128, 128, 16);
		refractionTexture.wrapMode = TextureWrapMode.Clamp;
		refractionTexture.isPowerOfTwo = true;
		
		reflectionTexture = new RenderTexture(128,128, 16);
		reflectionTexture.wrapMode = TextureWrapMode.Clamp;
		reflectionTexture.isPowerOfTwo = true;
			
			
		//Spawn the camera we'll use for offscreen rendering (refraction/reflection)
		GameObject cam = new GameObject();
		cam.name = "DeepWaterOffscreenCam";
		cam.transform.parent = transform;
		offscreenCam = cam.AddComponent(typeof(Camera)) as Camera;
		offscreenCam.enabled = false;
		offscreenCam.fieldOfView = 60;
		CameraManager.singleton.farClipPlane = 10000;
		
		
		
		//Hack to make this object considered by the renderer - first make a plane
		//covering the watertiles so we get a decent bounding box, then
		//scale all the vertices to 0 to make it invisible.
		gameObject.AddComponent<MeshRenderer>();
			
		GetComponent<Renderer>().material.renderQueue = 1;
		GetComponent<Renderer>().receiveShadows = true;
		GetComponent<Renderer>().castShadows = true;
		
		Mesh m = new Mesh();
			
		Vector3[] verts = new Vector3[4];
		Vector2[] uv = new Vector2[4];
		Vector3[] n = new Vector3[4];
		int[] tris =  new int[6];
			
		float minSizeX = -1024;
		float maxSizeX = 1024;
		
		float minSizeY = -1024;
		float maxSizeY = 1024;
			
		verts[0] = new Vector3(minSizeX, 0.0f, maxSizeY);
		verts[1] = new Vector3(maxSizeX, 0.0f, maxSizeY);
		verts[2] = new Vector3(maxSizeX, 0.0f, minSizeY);
		verts[3] = new Vector3(minSizeX, 0.0f, minSizeY);
		
		tris[0] = 0;
		tris[1] = 1;
		tris[2] = 2;
			
		tris[3] = 2;
		tris[4] = 3;
		tris[5] = 0;
			
		m.vertices = verts;
		m.uv = uv;
		m.normals = n;
		m.triangles = tris;
			
			
		MeshFilter mfilter = gameObject.GetComponent<MeshFilter>();
			
		if (mfilter == null)
			mfilter = gameObject.AddComponent<MeshFilter>();
			
		mfilter.mesh = m;
			
		m.RecalculateBounds();
			
		//Hopefully the bounds will not be recalculated automatically
		verts[0] = Vector3.zero;
		verts[1] = Vector3.zero;
		verts[2] = Vector3.zero;
		verts[3] = Vector3.zero;
			
		m.vertices = verts;
		
		//Create the material and set up the texture references.
		material_ocean = new Material(shader);
		
		material_ocean.SetTexture("_Reflection", reflectionTexture);
		material_ocean.SetTexture("_Refraction", refractionTexture);
		material_ocean.SetTexture ("_Bump", bumpTexture);
		material_ocean.SetTexture("_Fresnel", fresnelTexture);
		material_ocean.SetTexture("_Foam", foamTexture);
		material_ocean.SetTexture("_MainTex", maintexTexture);
		//material.SetTexture ("_Caustics", caustics);
		material_ocean.SetTexture("_Caustics", causticsTexture);
		material_ocean.SetVector("_Size", new Vector4(size.x, size.y, size.z, 0.0f));
		material_ocean.SetColor("_SurfaceColor", surfaceColor);
		material_ocean.SetColor("_WaterColor", waterColor);
		material_ocean.SetColor("_Color", mainColor);
		material_ocean.SetColor("_SpecColor", specularColor);
		material_ocean.SetColor("_SunColor", sunColor);
		material_ocean.SetVector("_SunPosition", sunposition.transform.position);
	//	material_ocean.SetVector("_SunPosition", sunposition.transform.position);
	//	material_ocean.SetFloat("_SunPower", sunpower);
		material_ocean.SetColor("_Transparency", transparency);
		material_ocean.SetFloat("_RefractionLevel", refractionlevel);
	
		reflectionRefractionEnabled = true;
	
		return true;
	}
	
	/*
	Delete the offscreen rendertextures on script shutdown.
	*/
	bool OnDisable()
	{
		if (reflectionTexture != null)
			DestroyImmediate(reflectionTexture);
				
		if (refractionTexture != null)
			DestroyImmediate(refractionTexture);
				
		reflectionTexture = null;
		refractionTexture = null;
		
		return false;
	}
	
	// Wave dispersion
	float disp ( float vec_k_mag  ) {
		return Mathf.Sqrt (9.81f * vec_k_mag);
	}
	
	/*function OnGUI()
	{
	
		if (reflectionTexture != null)
			GUI.DrawTexture (new Rect(0, 0, 128, 128), reflectionTexture, ScaleMode.StretchToFill, false);
		
		if (refractionTexture != null)
			GUI.DrawTexture (new Rect(0, 128, 128, 128), refractionTexture, ScaleMode.StretchToFill, false);
		
		GUI.DrawTexture (new Rect(128, 0, 128, 128), textureA, ScaleMode.StretchToFill, false);
		GUI.DrawTexture (new Rect(128, 128, 128, 128), textureB, ScaleMode.StretchToFill, false);
	
	
		GUI.Label(new Rect(10, 10, 100, 20), "Waveheight");
		scale = GUI.HorizontalSlider(new Rect(120, 10, 200, 20), scale, 0.05, 10.0);
		// scale=3;
		
		GUI.Label(new Rect(10, 30, 100, 20), "Wave sharpness");
		choppy_scale = GUI.HorizontalSlider(new Rect(120, 30, 200, 20), choppy_scale, 0.00,100.0);
		//choppy_scale=5;
		GUI.Label(new Rect(10, 50, 100, 20), "Water dirtyness");
		waterDirtyness = GUI.HorizontalSlider(new Rect(120, 50, 200, 20), waterDirtyness, 0.0, 1);
	
			
	}*/
	
	void Update () 
	{
		if( paused )
		{
			return;
		}
		float time;
		float dt;
		
		/*
		if(useCaptureScreenshotTime)
		{
			time = captureScreenshots.captureTime;
			dt = 1f / captureScreenshots.fps;
			//Debug.Log("water capturetime " + captureScreenshots.captureTime + " " + dt);
		}
		else
		{
		*/
			time = Time.time * timeScale;
			dt = Time.deltaTime * timeScale;
		//}
		
		float height_div_2 = ( height / 2 );
		float width_div_2 = ( width / 2 );
		Vector2 vec_k = new Vector2(0.0f,0.0f);
		ComplexF coeffA = new ComplexF (0.0f,0.0f);
		ComplexF co_tmp = new ComplexF (0.0f,0.0f);
		for (int y = 0;y<height;y++) {
			int yc = y < height_div_2 ? y : -height + y;
			int ny = y > 0 ? height - y : 0;
			float vec_k_y = MathfExt.TWO_PI * yc / size.z;
			vec_k[1] = vec_k_y;
			int wy = width * y;
			int nwy = width * ny;
			for (int x = 0;x<width;x++) {
				int idx = wy + x;
				ComplexF d_idx = data[idx];
				int xc = x < width_div_2 ? x : -width + x;
				vec_k[0] = MathfExt.TWO_PI * xc / size.x;
	
				float vec_k_mag = vec_k.magnitude;
				float iwkt = disp(vec_k_mag) * time;
				coeffA.Re = Mathf.Cos(iwkt);
				coeffA.Im = Mathf.Sin(iwkt);
				ComplexF coeffB = coeffA.GetConjugate();
	
				int nx = x > 0 ? width - x : 0;
	
				co_tmp.Im = vec_k.x;
				d_idx = h0[idx] * coeffA + h0[nwy + nx].GetConjugate() * coeffB;				
				t_x[idx] = d_idx * co_tmp - d_idx * vec_k.y;				
	
				// Choppy wave calcuations
				if (x + y > 0)
					d_idx += d_idx * vec_k.x / vec_k_mag;
			}
		}
		material_ocean.SetFloat( "_BlendA",Mathf.Cos(time)*0.1f); 
		material_ocean.SetFloat( "_BlendB",Mathf.Sin(time)*0.5f); 
	
		Fourier.FFT2 (data, width, height, FourierDirection.Backward);
		Fourier.FFT2 (t_x, width, height, FourierDirection.Backward);
	
		// Get base values for vertices and uv coordinates.
		if (baseHeight == null) {
			Mesh mesh = baseMesh;
			baseHeight = mesh.vertices;
			baseUV = mesh.uv;
		
			int itemCount = baseHeight.Length;
			uvs = new Vector2[itemCount];
			vertices = new Vector3[itemCount];
			normals = new Vector3[itemCount];
			tangents = new Vector4[itemCount];
		}
	
//		Vector3 vertex;
		Vector2 uv;
//		Vector3 normal;
//		float n_scale = size.x / width / scale;	
	
		float scaleA = choppy_scale / wh_size;
		float scaleB = scale / wh_size;
		float scaleBinv = 1.0f / scaleB;
		
		float uv_speed_delta = time * uv_speed;
		for (int i=0;i<wh_size;i++) {
			int iw = Mathf.RoundToInt(i+i/width);
			vertices[iw] = baseHeight[iw];
			vertices[iw].x += data [i].Im * scaleA;
			vertices[iw].y = data [i].Re * scaleB;
	
			normals[iw] = new Vector3 (t_x[i].Re, scaleBinv, t_x[i].Im).normalized;
	
			uv = baseUV[iw];
			uv.x = uv.x + uv_speed_delta;
			uvs[iw] = uv;
	
			if (((i+1) % width) == 0) {
				int data_idx = i+1-width;
				int iw_inc = iw+1;
				vertices[iw_inc] = baseHeight[iw_inc];
				vertices[iw_inc].x += data [data_idx].Im * scaleA;
				vertices[iw_inc].y = data [data_idx].Re * scaleB;
	
				normals[iw_inc] = new Vector3 (t_x[data_idx].Re, scaleBinv, t_x[data_idx].Im).normalized;
	
				uv = baseUV[iw_inc];
				uv.x = uv.x + uv_speed_delta;
				uvs[iw_inc] = uv;				
			}
		}
	
	
		int offset = g_width*(g_height-1);
	
		for (int i=0;i<g_width;i++) {
			int io = i+offset;
			int iw = i%width;
			vertices[io] = baseHeight[io];
			vertices[io].x += data [iw].Im * scaleA;
			vertices[io].y = data [iw].Re * scaleB;
	
			normals[io] = new Vector3 (t_x[iw].Re, scaleBinv, t_x[iw].Im).normalized;
		
			uv = baseUV[io];
			uv.x = uv.x + uv_speed_delta;
			uvs[io] = uv;
		}
	
		for (int i=0;i<g_wh_size-1;i++) {
			
			//Need to preserve w in refraction/reflection mode
			if (!reflectionRefractionEnabled)
			{
				if (((i+1) % g_width) == 0) {
					tangents[i] = (vertices[i-width+1] + new Vector3 (size.x, 0.0f, 0.0f) - vertices[i]).normalized;
				}
				else {
					tangents[i] = (vertices[i+1]-vertices[i]).normalized;
				}
			
				tangents[i][3] = 1.0f;
			}
			else
			{
				Vector3 tmp = Vector3.zero;
			
				if (((i+1) % g_width) == 0) 
				{
					tmp = (vertices[i-width+1] + new Vector3 (size.x, 0.0f, 0.0f) - vertices[i]).normalized;
				}
				else 
				{
					tmp = (vertices[i+1]-vertices[i]).normalized;
				}
				
				tangents[i] = new Vector4(tmp.x, tmp.y, tmp.z, tangents[i].w);
			}
		}
		
		//In reflection mode, use tangent w for foam strength
		if (reflectionRefractionEnabled)
		{
			for (int y = 0; y < g_height; y++)
			{
				for (int x = 0; x < g_width; x++)
				{
					if (x+1 >= g_width)
					{
						tangents[x + g_width*y].w = tangents[g_width*y].w;
					
						continue;
					}
					
					if (y+1 >= g_height)
					{
						tangents[x + g_width*y].w = tangents[x].w;
						
						continue;
					}
				
					Vector3 right = vertices[(x+1) + g_width*y] - vertices[x + g_width*y];
//					Vector3 back = vertices[x + g_width*y] - vertices[x + g_width*(y+1)];
					
					float foam = right.x/(size.x/g_width);
					
					
					if (foam < 0.0)
						tangents[x + g_width*y].w = 1;
					else if (foam < 0.5)
						tangents[x + g_width*y].w += 2 * dt;
					else
						tangents[x + g_width*y][3] -= 0.5f * dt;
						
					tangents[x + g_width*y][3] = Mathf.Clamp(tangents[x + g_width*y].w/foamTime, 0.0f, 2.0f);
				}
			}
		}
	
		tangents[g_wh_size-1] = (vertices[g_wh_size-1]+ new Vector3(size.x, 0.0f, 0.0f)-vertices[1]).normalized;
	
		int LOD=0;
		for (LOD=0;LOD<max_LOD;LOD++) 
		{
			float den = max_LOD_POW[LOD];
			int den_int = Mathf.RoundToInt(den);
			int itemcount = Mathf.RoundToInt((height/den+1) * (width/den+1));
			
			if( tangentsLOD.Count < ( LOD + 1 ) ) {
				tangentsLOD.Add(new Vector4[itemcount]);
				verticesLOD.Add(new Vector3[itemcount]);
				normalsLOD.Add(new Vector3[itemcount]);
				int uvCount = Mathf.RoundToInt((height/max_LOD_POW[LOD]+1) * (width/max_LOD_POW[LOD]+1));
				uvLOD.Add(new Vector2[uvCount]);
			}
			
			int idx = 0;
	
			for (int y=0;y<g_height;y+=den_int) {
				int gy = g_width * y;
				for (int x=0;x<g_width;x+=den_int) {
					int idx2 = gy + x;
					((Vector3[])(verticesLOD[LOD]))[idx] = vertices[idx2];
					((Vector2[])(uvLOD[LOD]))[idx] = uvs[idx2];
					((Vector4[])(tangentsLOD[LOD]))[idx] = tangents[idx2];
					((Vector3[])(normalsLOD[LOD]))[idx++] = normals[idx2];
				}			
			}
			for (int k=0;k<((ArrayList)tiles_LOD[LOD]).Count;k++) {
				Mesh meshLOD = ((Mesh)((ArrayList)tiles_LOD[LOD])[k]);
				meshLOD.vertices = (Vector3[])verticesLOD[LOD];
				meshLOD.normals = (Vector3[])normalsLOD[LOD];
				meshLOD.uv = (Vector2[])uvLOD[LOD];
				meshLOD.tangents = (Vector4[])tangentsLOD[LOD];
			}		
		}
//		oceansize=width*(max_LOD*2+1);
		int width_LOD2 = 175*s; 
		int tmpx=Mathf.RoundToInt(mycam.transform.position.x/width_LOD2);
		int tmpz=Mathf.RoundToInt(mycam.transform.position.z/width_LOD2);
		//tmpx= tmpx*width_LOD2-(width_LOD2/2);
		//tmpz= tmpz*width_LOD2-(width_LOD2/2);
		tmpx=-3 *width_LOD2-(width_LOD2/2);
		tmpz=-3 *width_LOD2-(width_LOD2/2);
		transform.position = new Vector3( tmpx, 0.0f, tmpz );
		
		
		//mesh.RecalculateNormals();
		//ee edit added for camera aim
		//material_ocean.SetFloat( "_BlendB",Mathf.Sin(Time.time)*0.5); 
	}
	
	
	
	
	/*
	Called when the object is about to be rendered. We render the refraction/reflection
	passes from here, since we only need to do it once per frame, not once per tile.
	*/
	void OnWillRenderObject()
	//function OnRenderImage()
	{
//		print("offScreenRender");
		//Recursion guard, don't let the offscreen cam go into a never-ending loop.
		if (Camera.current == offscreenCam)		return;
				
		if (reflectionTexture == null
		|| refractionTexture == null)
			return;
			
	
		material_ocean.SetTexture("_Reflection", reflectionTexture);
		material_ocean.SetTexture("_Refraction", refractionTexture);
		material_ocean.SetTexture ("_Bump", bumpTexture);
		material_ocean.SetTexture("_Fresnel", fresnelTexture);
		material_ocean.SetTexture("_Foam", foamTexture);
		material_ocean.SetTexture("_MainTex", maintexTexture);
		material_ocean.SetVector("_Size", new Vector4(size.x, size.y, size.z, 0.0f));
		material_ocean.SetColor("_SurfaceColor", surfaceColor);
		material_ocean.SetColor("_WaterColor", waterColor);
		material_ocean.SetColor("_Color", mainColor);
		material_ocean.SetColor("_SpecColor", specularColor);
		material_ocean.SetColor("_Transparency", transparency);
		int index  = Mathf.RoundToInt((Time.time * framesPerSecond) % caustics.Length);
		causticsTexture= caustics[index]; 
		material_ocean.SetTexture("_Caustics", causticsTexture);
		material_ocean.SetFloat("_RefractionLevel", refractionlevel);
		material_ocean.SetColor("_SunColor", sunColor);
		material_ocean.SetVector("_SunPosition", sunposition.transform.position);
		material_ocean.SetFloat("_SunPower", sunpower);
	
		RenderReflectionAndRefraction();
	}
	
	
	/*
	Renders the reflection and refraction buffers using a second camera copying the current
	camera settings.
	*/
	void RenderReflectionAndRefraction()
	{
	//return;
		//camera.ResetWorldToCameraMatrix();
	
		offscreenCam.enabled = true;
		int oldPixelLightCount = QualitySettings.pixelLightCount;
		QualitySettings.pixelLightCount = 0;
		
		
		Camera renderCamera = mycam;
	
		Matrix4x4 originalWorldToCam = renderCamera.worldToCameraMatrix;
			
		int cullingMask = renderCamera.cullingMask & ~(1 << LayerMask.NameToLayer("Water"));
			
		//Reflection pass
		Matrix4x4 reflection = Matrix4x4.zero;
		
		//TODO: Use local plane here, not global!
		CameraHelper.CalculateReflectionMatrix (ref reflection, new Vector4(0.0f, 1.0f, 0.0f, 0.0f));
			
		offscreenCam.transform.position = reflection.MultiplyPoint(renderCamera.transform.position);
		offscreenCam.transform.rotation = renderCamera.transform.rotation;
		offscreenCam.transform.Rotate(0,180,0); 
		offscreenCam.worldToCameraMatrix = originalWorldToCam * reflection;
		
		offscreenCam.cullingMask = cullingMask;
		offscreenCam.targetTexture = reflectionTexture;
		offscreenCam.clearFlags = renderCamera.clearFlags;
		
		//Need to reverse face culling for reflection pass, since the camera
		//is now flipped upside/down.
		GL.SetRevertBackfacing (false);
			
		Vector4 cameraSpaceClipPlane = CameraHelper.CameraSpacePlane(offscreenCam, Vector3.zero, Vector3.up, 1.0f);
			
		Matrix4x4 projection = renderCamera.projectionMatrix;
		Matrix4x4 obliqueProjection = projection;
			
		CameraHelper.CalculateObliqueMatrix (ref obliqueProjection, cameraSpaceClipPlane);
		
		//Do the actual render, with the near plane set as the clipping plane. See the
		//pro water source for details.
		offscreenCam.projectionMatrix = obliqueProjection;
		offscreenCam.Render();
	
		
		GL.SetRevertBackfacing (false);
		
		//Refractionpass
		bool fog = RenderSettings.fog;
		Color fogColor = RenderSettings.fogColor;
		float fogDensity = RenderSettings.fogDensity;
			
		RenderSettings.fog = true;
		RenderSettings.fogColor = Color.grey;
		RenderSettings.fogDensity = waterDirtyness/10;
			
		//TODO: If we want to use this as a refraction seen from under the seaplane,
		//      the cameraclear should be skybox.
		offscreenCam.clearFlags = CameraClearFlags.Color;
		offscreenCam.backgroundColor = Color.grey;
			
		offscreenCam.targetTexture = refractionTexture;
		obliqueProjection = projection;
		
		offscreenCam.transform.position = renderCamera.transform.position;
		offscreenCam.transform.rotation = renderCamera.transform.rotation;
		offscreenCam.worldToCameraMatrix = originalWorldToCam;
		
		cameraSpaceClipPlane = CameraHelper.CameraSpacePlane(offscreenCam, Vector3.zero, Vector3.up, -1.0f);
		CameraHelper.CalculateObliqueMatrix (ref obliqueProjection, cameraSpaceClipPlane);
		offscreenCam.projectionMatrix = obliqueProjection;
		
		offscreenCam.Render();
			
		RenderSettings.fog = fog;
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogDensity = fogDensity;
			
		offscreenCam.projectionMatrix = projection;
			
			
		offscreenCam.targetTexture = null;
			
		QualitySettings.pixelLightCount = oldPixelLightCount;
	
	//	camera.ResetWorldToCameraMatrix();
	
	//GL.SetRevertBackfacing (true);
	}
}	
