using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SECTR_TerrainWindow : SECTR_Window
{
	#region Private Details
	private Vector2 scrollPosition;
	private string sectorSearch = "";
	private Terrain selectedTerrain = null;
	private int sectorsWidth = 4;
	private int sectorsHeight = 1;
	private int sectorsLength = 4;
	private bool sectorizeConnected = false;
	private bool splitTerrain = false;
	private bool groupStaticObjects = false;
	private bool groupDynamicObjects = false;
	#endregion
	
	#region Public Interface
	public static void SectorizeTerrain(Terrain terrain, int sectorsWidth, int sectorsLength, int sectorsHeight, bool splitTerrain, bool includeStatic, bool includeDynamic)
	{
		if(!terrain)
		{
			Debug.LogWarning("Cannot sectorize null terrain.");
			return;
		}
		
		if(terrain.transform.root.GetComponentsInChildren<SECTR_Sector>().Length > 0)
		{
			Debug.LogWarning("Cannot sectorize terrain that is already part of a Sector."); 
		}
		
		string undoString = "Sectorized " + terrain.name;
		
		if(sectorsWidth == 1 && sectorsLength == 1)
		{
			SECTR_Sector newSector = terrain.gameObject.AddComponent<SECTR_Sector>();
			SECTR_Undo.Created(newSector, undoString);
			newSector.ForceUpdate();
			return;
		}
		
		if(splitTerrain && (!Mathf.IsPowerOfTwo(sectorsWidth) || !Mathf.IsPowerOfTwo(sectorsLength)))
		{
			Debug.LogWarning("Splitting terrain requires power of two sectors in width and length.");
			splitTerrain = false;
		}
		else if(splitTerrain && sectorsWidth != sectorsLength)
		{
			Debug.LogWarning("Splitting terrain requires same number of sectors in width and length.");
			splitTerrain = false;
		}
		
		Vector3 terrainSize = terrain.terrainData.size;
		float sectorWidth = terrainSize.x / sectorsWidth;
		float sectorHeight = terrainSize.y / sectorsHeight;
		float sectorLength = terrainSize.z / sectorsLength;
		
		int heightmapWidth = (terrain.terrainData.heightmapWidth / sectorsWidth);
		int heightmapLength = (terrain.terrainData.heightmapHeight / sectorsLength);
		int alphaWidth = terrain.terrainData.alphamapWidth / sectorsWidth;
		int alphaLength = terrain.terrainData.alphamapHeight / sectorsLength;
		int detailWidth = terrain.terrainData.detailWidth / sectorsWidth;
		int detailLength = terrain.terrainData.detailHeight / sectorsLength;
		
		string sceneDir = "";
		string sceneName = "";
		string exportFolder = splitTerrain ? SECTR_Asset.MakeExportFolder("TerrainSplits", false, out sceneDir, out sceneName) : "";
		
		Transform baseTransform = null;
		if(splitTerrain)
		{
			GameObject baseObject = new GameObject(terrain.name);
			baseTransform = baseObject.transform;
			SECTR_Undo.Created(baseObject, undoString);
		}
		
		List<Transform> rootTransforms = new List<Transform>();
		List<Bounds> rootBounds = new List<Bounds>();
		_GetRoots(includeStatic, includeDynamic, rootTransforms, rootBounds);
		
		// Create Sectors
		SECTR_Sector[,,] newSectors = new SECTR_Sector[sectorsWidth,sectorsLength,sectorsHeight];
		Terrain[,] newTerrains = splitTerrain ? new Terrain[sectorsWidth,sectorsLength] : null;
		for(int widthIndex = 0; widthIndex < sectorsWidth; ++widthIndex)
		{
			for(int lengthIndex = 0; lengthIndex < sectorsLength; ++lengthIndex)
			{
				for(int heightIndex = 0; heightIndex < sectorsHeight; ++heightIndex)
				{
					string newName = terrain.name + " " + widthIndex + "-" + lengthIndex + "-" + heightIndex;
					GameObject newSectorObject = new GameObject("SECTR " + newName + " Sector");
					newSectorObject.transform.parent = baseTransform;
					newSectorObject.isStatic = terrain.gameObject.isStatic;
					Vector3 sectorCorner = new Vector3(widthIndex * sectorWidth,
					                                   heightIndex * sectorHeight,
					                                   lengthIndex * sectorLength) + terrain.transform.position;
					newSectorObject.transform.position = sectorCorner;
					newSectorObject.isStatic = true;
					SECTR_Sector newSector = newSectorObject.AddComponent<SECTR_Sector>();
					newSector.OverrideBounds = !splitTerrain && (sectorsWidth > 1 || sectorsLength > 1);
					newSector.BoundsOverride = new Bounds(sectorCorner + new Vector3(sectorWidth * 0.5f, sectorHeight * 0.5f, sectorLength * 0.5f),
					                                      new Vector3(sectorWidth, sectorHeight, sectorLength));
					newSectors[widthIndex,lengthIndex,heightIndex] = newSector;
					
					if(splitTerrain && heightIndex == 0)
					{
						Terrain newTerrain = newSectorObject.AddComponent<Terrain>();
						newTerrain.terrainData = SECTR_Asset.Create<TerrainData>(exportFolder, newName, new TerrainData());
						EditorUtility.SetDirty(newTerrain.terrainData);
						SECTR_VC.WaitForVC();
						
						// Copy properties
						// Basic terrain properties
						newTerrain.editorRenderFlags = terrain.editorRenderFlags;
						newTerrain.castShadows = terrain.castShadows;
						newTerrain.heightmapMaximumLOD = terrain.heightmapMaximumLOD;
						newTerrain.heightmapPixelError = terrain.heightmapPixelError;
						newTerrain.lightmapIndex = -1; // Can't set lightmap UVs on terrain.
						newTerrain.materialTemplate = terrain.materialTemplate;
						
						// Copy geometric data
						int heightmapBaseX = widthIndex * heightmapWidth;
						int heightmapBaseY = lengthIndex * heightmapLength;
						int heightmapWidthX = heightmapWidth + (sectorsWidth > 1 ? 1 : 0);
						int heightmapWidthY = heightmapLength + (sectorsLength > 1 ? 1 : 0);	
						newTerrain.terrainData.heightmapResolution = terrain.terrainData.heightmapResolution / sectorsWidth;
						newTerrain.terrainData.size = new Vector3(sectorWidth, terrainSize.y, sectorLength);
						newTerrain.terrainData.SetHeights(0, 0, terrain.terrainData.GetHeights(heightmapBaseX, heightmapBaseY, heightmapWidthX, heightmapWidthY));
						
						// Copy alpha maps
						int alphaBaseX = alphaWidth * widthIndex;
						int alphaBaseY = alphaLength * lengthIndex;
						newTerrain.terrainData.splatPrototypes = terrain.terrainData.splatPrototypes;
						newTerrain.basemapDistance = terrain.basemapDistance;
						newTerrain.terrainData.baseMapResolution = terrain.terrainData.baseMapResolution / sectorsWidth;
						newTerrain.terrainData.alphamapResolution = terrain.terrainData.alphamapResolution / sectorsWidth;
						newTerrain.terrainData.SetAlphamaps(0, 0, terrain.terrainData.GetAlphamaps(alphaBaseX, alphaBaseY, alphaWidth, alphaLength));
						
						// Copy detail info
						newTerrain.detailObjectDensity = terrain.detailObjectDensity;
						newTerrain.detailObjectDistance = terrain.detailObjectDistance;
						newTerrain.terrainData.detailPrototypes = terrain.terrainData.detailPrototypes;
						newTerrain.terrainData.SetDetailResolution(terrain.terrainData.detailResolution / sectorsWidth, 8); // TODO: extract detailResolutionPerPatch
						
						int detailBaseX = detailWidth * widthIndex;
						int detailBaseY = detailLength * lengthIndex;
						int numLayers = terrain.terrainData.detailPrototypes.Length;
						for(int layer = 0; layer < numLayers; ++layer)
						{
							newTerrain.terrainData.SetDetailLayer(0, 0, layer, terrain.terrainData.GetDetailLayer(detailBaseX, detailBaseY, detailWidth, detailLength, layer)); 
						}
						
						// Copy grass and trees
						newTerrain.terrainData.wavingGrassAmount = terrain.terrainData.wavingGrassAmount;
						newTerrain.terrainData.wavingGrassSpeed = terrain.terrainData.wavingGrassSpeed;
						newTerrain.terrainData.wavingGrassStrength = terrain.terrainData.wavingGrassStrength;
						newTerrain.terrainData.wavingGrassTint = terrain.terrainData.wavingGrassTint;
						newTerrain.treeBillboardDistance = terrain.treeBillboardDistance;
						newTerrain.treeCrossFadeLength = terrain.treeCrossFadeLength;
						newTerrain.treeDistance = terrain.treeDistance;
						newTerrain.treeMaximumFullLODCount = terrain.treeMaximumFullLODCount;
						newTerrain.terrainData.treePrototypes = terrain.terrainData.treePrototypes;
						newTerrain.terrainData.RefreshPrototypes();

						foreach(TreeInstance treeInstace in terrain.terrainData.treeInstances)
						{
							if(treeInstace.prototypeIndex >= 0 && treeInstace.prototypeIndex < newTerrain.terrainData.treePrototypes.Length &&
							   newTerrain.terrainData.treePrototypes[treeInstace.prototypeIndex].prefab)
							{
								Vector3 worldSpaceTreePos = Vector3.Scale(treeInstace.position, terrainSize) + terrain.transform.position;
								if(newSector.BoundsOverride.Contains(worldSpaceTreePos))
								{
									Vector3 localSpaceTreePos = new Vector3((worldSpaceTreePos.x - newTerrain.transform.position.x) / sectorWidth,
									                                        treeInstace.position.y,
									                                        (worldSpaceTreePos.z - newTerrain.transform.position.z) / sectorLength);
									TreeInstance newInstance = treeInstace;
									newInstance.position = localSpaceTreePos;
									newTerrain.AddTreeInstance(newInstance);
								}
							}
						}

#if UNITY_5_TODO
						// Copy physics
						#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2				
						newTerrain.terrainData.physicMaterial = terrain.terrainData.physicMaterial;
						#endif
#endif						
						// Force terrain to rebuild
						newTerrain.Flush();
						UnityEditor.EditorUtility.SetDirty(newTerrain.terrainData);
						SECTR_VC.WaitForVC();
						newTerrain.enabled = false;
						newTerrain.enabled = true;
						
						TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
						if(terrainCollider)
						{
							TerrainCollider newCollider = newSectorObject.AddComponent<TerrainCollider>();	
							#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
							newCollider.sharedMaterial = terrainCollider.sharedMaterial;
							#endif
							newCollider.terrainData = newTerrain.terrainData;
						}
						
						newTerrains[widthIndex,lengthIndex] = newTerrain;
					}
					newSector.ForceUpdate();
					SECTR_Undo.Created(newSectorObject, undoString);
					
					_Encapsulate(newSector, rootTransforms, rootBounds, undoString);
				}
			}
		}
		
		// Create portals
		for(int widthIndex = 0; widthIndex < sectorsWidth; ++widthIndex)
		{
			for(int lengthIndex = 0; lengthIndex < sectorsLength; ++lengthIndex)
			{
				for(int heightIndex = 0; heightIndex < sectorsHeight; ++heightIndex)
				{
					if(widthIndex < sectorsWidth - 1)
					{
						_CreatePortal(newSectors[widthIndex + 1, lengthIndex, heightIndex], newSectors[widthIndex, lengthIndex, heightIndex], baseTransform, undoString);
					}
					
					if(lengthIndex < sectorsLength - 1)
					{
						_CreatePortal(newSectors[widthIndex, lengthIndex + 1, heightIndex], newSectors[widthIndex, lengthIndex, heightIndex], baseTransform, undoString);
					}
					
					if(heightIndex > 0)						
					{
						_CreatePortal(newSectors[widthIndex, lengthIndex, heightIndex], newSectors[widthIndex, lengthIndex, heightIndex - 1], baseTransform, undoString);
					}
					else if(splitTerrain)
					{
						Terrain leftNeighbor = widthIndex > 0 ? newTerrains[widthIndex - 1, lengthIndex] : null;
						Terrain rightNeighbor = widthIndex < sectorsWidth - 1 ? newTerrains[widthIndex + 1, lengthIndex] : null;
						Terrain bottomNeighbor = lengthIndex > 0 ? newTerrains[widthIndex, lengthIndex - 1] : null;
						Terrain topNeighbor = lengthIndex < sectorsLength - 1 ? newTerrains[widthIndex, lengthIndex + 1] : null;
						newTerrains[widthIndex, lengthIndex].SetNeighbors(leftNeighbor, topNeighbor, rightNeighbor, bottomNeighbor);
						SECTR_Sector terrainSector = newSectors[widthIndex, lengthIndex, 0];
						terrainSector.TopTerrain = topNeighbor ? topNeighbor.GetComponent<SECTR_Sector>() : null;
						terrainSector.BottomTerrain = bottomNeighbor ? bottomNeighbor.GetComponent<SECTR_Sector>() : null;
						terrainSector.LeftTerrain = leftNeighbor ? leftNeighbor.GetComponent<SECTR_Sector>() : null;
						terrainSector.RightTerrain = rightNeighbor ? rightNeighbor.GetComponent<SECTR_Sector>() : null;
					}
				}
			}
		}
		
		// destroy original terrain
		if(splitTerrain)
		{
			SECTR_Undo.Destroy(terrain.gameObject, undoString);
		}
	}
	
	public static void SectorizeConnected(Terrain terrain, bool includeStatic, bool includeDynamic)
	{
		Dictionary<Terrain, Terrain> processedTerrains = new Dictionary<Terrain, Terrain>();
		List<Transform> rootTransforms = new List<Transform>();
		List<Bounds> rootBounds = new List<Bounds>();
		_GetRoots(includeStatic, includeDynamic, rootTransforms, rootBounds);
		_SectorizeConnected(terrain, includeStatic, includeDynamic, processedTerrains, rootTransforms, rootBounds);
	}
	#endregion
	
	#region Unity Interface
	protected override void OnGUI()
	{
		base.OnGUI();
		
		Terrain[] terrains = (Terrain[])GameObject.FindObjectsOfType(typeof(Terrain));
		int numTerrains = terrains.Length;
		bool sceneHasTerrains = numTerrains > 0;
		bool selectedInSector = false;
		bool hasTerrainComposer = false;
		
		EditorGUILayout.BeginVertical();
		DrawHeader("TERRAINS", ref sectorSearch, 100, true);
		Rect r = EditorGUILayout.BeginVertical();
		r.y -= lineHeight;
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		bool wasEnabled = GUI.enabled;
		GUI.enabled = false;
		GUI.Button(r, sceneHasTerrains ? "" : "Current Scene Has No Terrains");
		GUI.enabled = wasEnabled;
		Terrain newSelectedTerrain = Selection.activeGameObject ?  Selection.activeGameObject.GetComponent<Terrain>() : null;
		if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
		{
			newSelectedTerrain = null;
		}
		
		for(int terrainIndex = 0; terrainIndex < numTerrains; ++terrainIndex)
		{
			Terrain terrain = terrains[terrainIndex];
			if(terrain.name.ToLower().Contains(sectorSearch.ToLower()))
			{
				bool selected = terrain == selectedTerrain;
				bool inSector = false;
				Transform parent = terrain.transform;
				while(parent != null)
				{
					if(parent.GetComponent<SECTR_Sector>())
					{
						inSector = true;
						if(selected)
						{
							selectedInSector = true;
						}
						break;
					}
					parent = parent.parent;
				}
				hasTerrainComposer |= terrain.GetComponent("TerrainNeighbors") != null;
				
				Rect clipRect = EditorGUILayout.BeginHorizontal();
				if(selected)
				{
					Rect selectionRect = clipRect;
					selectionRect.y += 1;
					selectionRect.height += 1;
					GUI.Box(selectionRect, "", selectionBoxStyle);
				}
				
				GUILayout.FlexibleSpace();
				elementStyle.normal.textColor = selected ? Color.white : UnselectedItemColor;
				elementStyle.alignment = TextAnchor.MiddleCenter;
				EditorGUILayout.LabelField(terrain.name, elementStyle);
				GUILayout.FlexibleSpace();
				
				EditorGUILayout.EndHorizontal();
				
				if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && 
				   clipRect.Contains(Event.current.mousePosition) )
				{
					newSelectedTerrain = terrain;
					selectedInSector = inSector;
				}
			}
		}
		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();
		
		bool doRepaint = false;
		if(newSelectedTerrain != selectedTerrain && SceneView.lastActiveSceneView)
		{
			selectedTerrain = newSelectedTerrain;
			Selection.activeGameObject = selectedTerrain ? selectedTerrain.gameObject : null;
			SceneView.lastActiveSceneView.FrameSelected();
			doRepaint = true;
		}
		
		bool sectorizableSelection = sceneHasTerrains && selectedTerrain != null && !selectedInSector;
		string nullSearch = null;
		
		DrawHeader("SETTINGS", ref nullSearch, 0, true);
		
		EditorGUILayout.BeginVertical();
		sectorsWidth = EditorGUILayout.IntField(new GUIContent("Sectors Width", "Number of Sectors to create across terrain width."), sectorsWidth);
		sectorsWidth = Mathf.Max(sectorsWidth, 1);
		sectorsLength = EditorGUILayout.IntField(new GUIContent("Sectors Length", "Number of Sectors to create across terrain length."), sectorsLength);
		sectorsLength = Mathf.Max(sectorsLength, 1);
		sectorsHeight = EditorGUILayout.IntField(new GUIContent("Sectors Height", "Number of Sectors to create across terrain height."), sectorsHeight);
		sectorsHeight = Mathf.Max(sectorsHeight, 1);
		
		sectorizeConnected = EditorGUILayout.Toggle(new GUIContent("Include Connected", "Sectorizes all terrains directly or indirectly connected to selected terrain."), sectorizeConnected);
		
		bool canSplitTerrain = selectedTerrain != null &&
			sectorsWidth > 1 && sectorsLength > 1 && sectorsWidth == sectorsLength &&
				Mathf.IsPowerOfTwo(sectorsWidth) && Mathf.IsPowerOfTwo(sectorsLength) &&
				(selectedTerrain.terrainData.heightmapResolution - 1) / sectorsWidth >= 32;
		splitTerrain = EditorGUILayout.Toggle(new GUIContent("Split Terrain", "Splits terrain into multiple objects (for streaming or culling)."), splitTerrain);
		groupStaticObjects = EditorGUILayout.Toggle(new GUIContent("Group Static Objects", "Make all static game objects on the terrain children of the Sector."), groupStaticObjects);
		groupDynamicObjects = EditorGUILayout.Toggle(new GUIContent("Group Dynamic Objects", "Make all dynamic game objects on the terrain children of the Sector."), groupDynamicObjects);
		EditorGUILayout.EndVertical();
		
		if(!selectedTerrain)
		{
			GUI.enabled = false;
			GUILayout.Button("Select Terrain To Sectorize");
			GUI.enabled = true;
		}
		else if(!sectorizableSelection && selectedInSector)
		{
			GUI.enabled = false;
			GUILayout.Button("Cannot Sectorize Terrain That Is Already In a Sector");
			GUI.enabled = false;
		}
		else if(sectorizeConnected && splitTerrain)
		{
			GUI.enabled = false;
			GUILayout.Button("Cannot both Split and Sectorize Connected Terrains");
			GUI.enabled = false;
		}
		else if(sectorizeConnected && (sectorsWidth != 1 || sectorsLength != 1 || sectorsHeight != 1))
		{
			GUI.enabled = false;
			GUILayout.Button("Width/Length/Height Must be 1 to Sectorize Connected Terrains");
			GUI.enabled = false;
		}
		else if(splitTerrain && sectorsWidth != sectorsLength)
		{
			GUI.enabled = false;
			GUILayout.Button("Cannot split terrain unless Sectors Width and Length match.");
			GUI.enabled = true;
		}
		else if(splitTerrain && !Mathf.IsPowerOfTwo(sectorsWidth))
		{
			GUI.enabled = false;
			GUILayout.Button("Cannot split terrain unless Sectors Width and Length are powers of 2.");
			GUI.enabled = true;
		}
		else if(splitTerrain && (selectedTerrain.terrainData.heightmapResolution - 1) / sectorsWidth < 32)
		{
			GUI.enabled = false;
			GUILayout.Button("Cannot split terrain into chunks less than 32 x 32.");
			GUI.enabled = true;
		}
		else if(GUILayout.Button("Sectorize Terrain"))
		{
			if(sectorizeConnected)
			{
				SectorizeConnected(selectedTerrain, groupStaticObjects, groupDynamicObjects);
				doRepaint = true;
			}
			else if(!splitTerrain || selectedTerrain.lightmapIndex < 0 || EditorUtility.DisplayDialog("Lightmap Warning", "Splitting terrain will not preserve lightmaps. They will need to be rebaked. Continue sectorization?", "Yes", "No"))
			{
				SectorizeTerrain(selectedTerrain, sectorsWidth, sectorsLength, sectorsHeight, canSplitTerrain && splitTerrain, groupStaticObjects, groupDynamicObjects);
				doRepaint = true;
			}
		}
		GUI.enabled = wasEnabled;
		
		EditorGUILayout.EndVertical();
		
		if(doRepaint)
		{
			Repaint();
		}
	}
	#endregion
	
	#region Private Interface
	private static void _CreatePortal(SECTR_Sector front, SECTR_Sector back, Transform parent, string undoString)
	{
		if(front && back)
		{
			GameObject newPortalObject = new GameObject("SECTR Terrain Portal");
			SECTR_Portal newPortal = newPortalObject.AddComponent<SECTR_Portal>();
			newPortal.SetFlag(SECTR_Portal.PortalFlags.PassThrough, true);
			newPortal.FrontSector = front;
			newPortal.BackSector = back;
			newPortalObject.transform.parent = parent;
			newPortalObject.transform.position = (front.TotalBounds.center + back.TotalBounds.center) * 0.5f;
			newPortalObject.transform.LookAt(front.TotalBounds.center);
			SECTR_Undo.Created(newPortalObject, undoString);
		}
	}
	
	private static void _GetRoots(bool includeStatic, bool includeDynamic, List<Transform> rootTransforms, List<Bounds> rootBounds)
	{
		if(includeStatic || includeDynamic)
		{
			Transform[] allTransforms = (Transform[])GameObject.FindObjectsOfType(typeof(Transform));
			foreach(Transform transform in allTransforms)
			{
				if(transform.parent == null &&
				   ((transform.gameObject.isStatic && includeStatic) || !transform.gameObject.isStatic && includeDynamic))
				{
					rootTransforms.Add(transform);
					Bounds aggregateBounds = new Bounds();
					bool initBounds = false;
					Renderer[] childRenderers = transform.GetComponentsInChildren<Renderer>();
					foreach(Renderer renderer in childRenderers)
					{
						if(!initBounds)
						{
							aggregateBounds = renderer.bounds;
							initBounds = true;
						}
						else
						{
							aggregateBounds.Encapsulate(renderer.bounds);
						}
					}
					Light[] childLights = transform.GetComponentsInChildren<Light>();
					foreach(Light light in childLights)
					{
						if(!initBounds)
						{
							aggregateBounds = SECTR_Geometry.ComputeBounds(light);
							initBounds = true;
						}
						else
						{
							aggregateBounds.Encapsulate(SECTR_Geometry.ComputeBounds(light));
						}
					}
					rootBounds.Add(aggregateBounds);
				}
			}
		}
	}
	
	private static void _Encapsulate(SECTR_Sector newSector, List<Transform> rootTransforms, List<Bounds> rootBounds, string undoString)
	{
		int numRoots = rootTransforms.Count;
		for(int rootIndex = numRoots - 1; rootIndex >= 0; --rootIndex)
		{
			Transform rootTransform = rootTransforms[rootIndex];
			if(rootTransform != newSector.transform && SECTR_Geometry.BoundsContainsBounds(newSector.TotalBounds, rootBounds[rootIndex]))
			{
				SECTR_Undo.Parent(newSector.gameObject, rootTransform.gameObject, undoString);
				rootTransforms.RemoveAt(rootIndex);
				rootBounds.RemoveAt(rootIndex);
			}
		}
	}
	
	private static void _SectorizeConnected(Terrain terrain, bool includeStatic, bool includeDynamic, Dictionary<Terrain, Terrain> processedTerrains, List<Transform> rootTransforms, List<Bounds> rootBounds)
	{
		if(terrain && !processedTerrains.ContainsKey(terrain))
		{
			string undoString = "Sectorize Connected";
			processedTerrains[terrain] = terrain;
			terrain.gameObject.isStatic = true;
			SECTR_Sector newSector = terrain.gameObject.AddComponent<SECTR_Sector>();
			newSector.ForceUpdate();
			SECTR_Undo.Created(newSector, undoString);
			_Encapsulate(newSector, rootTransforms, rootBounds, undoString);
			
			Component terrainNeighbors = terrain.GetComponent("TerrainNeighbors");
			if(terrainNeighbors)
			{
				System.Type neighborsType = terrainNeighbors.GetType();
				Terrain topTerrain = neighborsType.GetField("top").GetValue(terrainNeighbors) as Terrain;
				if(topTerrain)
				{
					SECTR_Sector neighborSector = topTerrain.GetComponent<SECTR_Sector>();
					if(neighborSector)
					{
						newSector.TopTerrain = neighborSector;
						neighborSector.BottomTerrain = newSector;
						_CreatePortal(newSector, neighborSector, terrain.transform.parent, undoString);
					}
					_SectorizeConnected(topTerrain, includeStatic, includeDynamic, processedTerrains, rootTransforms, rootBounds);
				}
				Terrain bottomTerrain = neighborsType.GetField("bottom").GetValue(terrainNeighbors) as Terrain;
				if(bottomTerrain)
				{
					SECTR_Sector neighborSector = bottomTerrain.GetComponent<SECTR_Sector>();
					if(neighborSector)
					{
						newSector.BottomTerrain = neighborSector;
						neighborSector.TopTerrain = newSector;
						_CreatePortal(newSector, neighborSector, terrain.transform.parent, undoString);
					}
					_SectorizeConnected(bottomTerrain, includeStatic, includeDynamic, processedTerrains, rootTransforms, rootBounds);
				}
				Terrain leftTerrain = neighborsType.GetField("left").GetValue(terrainNeighbors) as Terrain;
				if(leftTerrain)
				{
					SECTR_Sector neighborSector = leftTerrain.GetComponent<SECTR_Sector>();
					if(neighborSector)
					{
						newSector.LeftTerrain = neighborSector;
						neighborSector.RightTerrain = newSector;
						_CreatePortal(newSector, neighborSector, terrain.transform.parent, undoString);
					}
					_SectorizeConnected(leftTerrain, includeStatic, includeDynamic, processedTerrains, rootTransforms, rootBounds);
				}
				Terrain rightTerrain = neighborsType.GetField("right").GetValue(terrainNeighbors) as Terrain;
				if(rightTerrain)
				{
					SECTR_Sector neighborSector = rightTerrain.GetComponent<SECTR_Sector>();
					if(neighborSector)
					{
						newSector.RightTerrain = neighborSector;
						neighborSector.LeftTerrain = newSector;
						_CreatePortal(newSector, neighborSector, terrain.transform.parent, undoString);
					}
					_SectorizeConnected(rightTerrain, includeStatic, includeDynamic, processedTerrains, rootTransforms, rootBounds);
				}
			}
		}
	}
	#endregion
}
