
using UnityEngine;
using System.Collections;

public class CustomTerrainScript : MonoBehaviour {
	
	public Texture2D Bump0;
	public Texture2D Bump1;
	public Texture2D Bump2;
	public Texture2D Bump3;
	
	public Texture2D WemoSplat0;
	public Texture2D WemoSplat1;
	public Texture2D WemoSplat2;
	public Texture2D WemoSplat3;
	
	public Texture2D Caustic;
	public Texture2D WemoControl;
	
	public float Tile0;
	public float Tile1;
	public float Tile2;
	public float Tile3;
	public float CusticTile;
	
	public float terrainSizeX;
	public float terrainSizeZ;
	
	void Start () {
		
		Terrain terrainComp = (Terrain)GetComponent(typeof(Terrain));
		
		if(WemoControl)
			Shader.SetGlobalTexture("_Control", WemoControl);
		
		if(WemoSplat0)
			Shader.SetGlobalTexture("_Splat0", WemoSplat0);
		
		if(WemoSplat1)
			Shader.SetGlobalTexture("_Splat1", WemoSplat1);
		
		if(WemoSplat2)
			Shader.SetGlobalTexture("_Splat2", WemoSplat2);
		
		if(WemoSplat3)
			Shader.SetGlobalTexture("_Splat3", WemoSplat3);
		
		if(Bump0)
			Shader.SetGlobalTexture("_BumpMap0", Bump0);
		
		if(Bump1)
			Shader.SetGlobalTexture("_BumpMap1", Bump1);
		
		if(Bump2)
			Shader.SetGlobalTexture("_BumpMap2", Bump2);
		
		if(Bump3)
			Shader.SetGlobalTexture("_BumpMap3", Bump3);
		
		if(Caustic)
			Shader.SetGlobalTexture("_CausticTex", Caustic);
		

		Shader.SetGlobalFloat("_Tile0", Tile0);
		Shader.SetGlobalFloat("_Tile1", Tile1);
		Shader.SetGlobalFloat("_Tile2", Tile2);
		Shader.SetGlobalFloat("_Tile3", Tile3);
		Shader.SetGlobalFloat("_Tile4", CusticTile);
		
		terrainSizeX = terrainComp.terrainData.size.x;
		terrainSizeZ = terrainComp.terrainData.size.z;
		
		Shader.SetGlobalFloat("_TerrainX", terrainSizeX);
		Shader.SetGlobalFloat("_TerrainZ", terrainSizeZ);
	}
	
	/* Don't need this update unless you're testing during play */
	void Update () {
		
		if(WemoSplat0)
			Shader.SetGlobalTexture("_Splat0", WemoSplat0);
		
		if(WemoSplat1)
			Shader.SetGlobalTexture("_Splat1", WemoSplat1);
		
		if(WemoSplat2)
			Shader.SetGlobalTexture("_Splat2", WemoSplat2);
		
		if(WemoSplat3)
			Shader.SetGlobalTexture("_Splat3", WemoSplat3);
		
		if(Bump0)
			Shader.SetGlobalTexture("_BumpMap0", Bump0);
		
		if(Bump1)
			Shader.SetGlobalTexture("_BumpMap1", Bump1);
		
		if(Bump2)
			Shader.SetGlobalTexture("_BumpMap2", Bump2);
		
		if(Bump3)
			Shader.SetGlobalTexture("_BumpMap3", Bump3);
		
		if(Caustic)
			Shader.SetGlobalTexture("_CausticTex", Caustic);
		

		Shader.SetGlobalFloat("_Tile0", Tile0);
		Shader.SetGlobalFloat("_Tile1", Tile1);
		Shader.SetGlobalFloat("_Tile2", Tile2);
		Shader.SetGlobalFloat("_Tile3", Tile3);
		Shader.SetGlobalFloat("_Tile4", CusticTile);
		
		Shader.SetGlobalFloat("_TerrainX", terrainSizeX);
		Shader.SetGlobalFloat("_TerrainZ", terrainSizeZ);
	}
}
