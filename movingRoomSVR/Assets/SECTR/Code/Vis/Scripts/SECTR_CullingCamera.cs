// Copyright (c) 2014 Nathan Martz

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;

/// \ingroup Vis
/// CullingCamera is the workhorse of SECTR Vis, culling objects by propagating Camera
/// data down through the Sector/Portal graph and into individual SECTR_Culler objects.
///
/// Culling in SECTR is a fairly straightforward process. Each CullingCamera is expected to
/// have a sibling Unity Camera. This allows PreCull() to be called, which is where our Camera
/// does its work. CullingCamera cleans up after itself in PostRender, which allows multiple SECTR Cameras
/// to be active in a single scene at once (if so desired).
/// 
/// Culling starts with the Sector(s) that contain the current Camera. From there, the
/// CCullingamera walks the Sector graph. At each Portal, the Camera tests
/// to see if its view frustum intersects the Portal's geometry. If it does, the frustum
/// is clipped down by the Portal geometry, and the traversal continues to the next Sector
/// (in a depth-first manner). Eventually, the frustum is winowed down to the point where no
/// additional Portals are visible and the traversal completes.
/// 
/// SECTR Vis also allows the use of culling via instances of SECTR_Occluder. As the CullingCamera walks
/// the Sector/Portal graph, it accumulates any Occluders that are present in that Sector. All future objects
/// are then tested against the accumulated Occluders.
/// 
/// Lastly, shadow casting lights are accumulated during the traversal. Because of the complexities
/// of shadow casting lights effectively extending the bounds of shadow casting meshes into Sectors
/// that they would not otherwise occupy, the CullingCamera accumulates shadow casting point lights during
/// the main traversal and then performs a post-pass for on any relevant meshes to ensure shadows are
/// never prematurely culled.
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("SECTR/Vis/SECTR Culling Camera")]
public class SECTR_CullingCamera : MonoBehaviour 
{
	#region Private Details
	/// A node of the Sector/Portal graph as relevant to the culling process.
	private struct VisibilityNode
	{	
		public VisibilityNode(SECTR_Sector sector, SECTR_Portal portal)
		{
			this.sector = sector;
			this.portal = portal;
			this.frustumPlanes = null;
		}

		public VisibilityNode(SECTR_Sector sector, SECTR_Portal portal, Plane[] frustumPlanes)
		{
			this.sector = sector;
			this.portal = portal;
			if(frustumPlanes == null)
			{
				this.frustumPlanes = null;
			}
			else if(frustumPool.Count > 0)
			{
				this.frustumPlanes = frustumPool.Pop();
				this.frustumPlanes.Clear();
				this.frustumPlanes.AddRange(frustumPlanes);
			}
			else
			{
				this.frustumPlanes = new List<Plane>(frustumPlanes);
			}
		}

		public VisibilityNode(SECTR_Sector sector, SECTR_Portal portal, List<Plane> frustumPlanes)
		{
			this.sector = sector;
			this.portal = portal;
			if(frustumPlanes == null)
			{
				this.frustumPlanes = null;
			}
			else if(frustumPool.Count > 0)
			{
				this.frustumPlanes = frustumPool.Pop();
				this.frustumPlanes.Clear();
				this.frustumPlanes.AddRange(frustumPlanes);
			}
			else
			{
				this.frustumPlanes = new List<Plane>(frustumPlanes);
			}
		}
		
		public SECTR_Sector sector;
		public SECTR_Portal portal;
		public List<Plane> frustumPlanes;
	};
	
	/// An expanded vertex structure used for clipping SectorPortal geometry.
	private struct ClipVertex
	{
		public ClipVertex(Vector4 vertex, float side)
		{
			this.vertex = vertex;
			this.side = side;
		}
		
		public Vector4 vertex;
		public float side;
	}

	// Records if culling was actually performed this frame.
	private bool didCull = false;
	// Bookkeeping data structures
	private List<SECTR_Sector> initialSectors = new List<SECTR_Sector>(4);
	private Stack<VisibilityNode> nodeStack = new Stack<VisibilityNode>(10);
	private Dictionary<SECTR_Portal, SECTR_Portal> visitedPortals = new Dictionary<SECTR_Portal, SECTR_Portal>(10);
	private List<ClipVertex> portalVertices = new List<ClipVertex>(16);
	private List<Plane> newFrustum = new List<Plane>(16);
	private List<Plane> cullingPlanes = new List<Plane>(16);
	private List<List<Plane>> occluderFrustums = new List<List<Plane>>(10);
	private Dictionary<SECTR_Occluder, SECTR_Occluder> activeOccluders = new Dictionary<SECTR_Occluder, SECTR_Occluder>(10);
	private List<ClipVertex> occluderVerts = new List<ClipVertex>(10);
	private List<SECTR_Member.Child> shadowLights = new List<SECTR_Member.Child>(10);
	private List<SECTR_Sector> shadowSectors = new List<SECTR_Sector>(4);
	private Dictionary<SECTR_Sector, List<SECTR_Member.Child>> shadowSectorTable = new Dictionary<SECTR_Sector, List<SECTR_Member.Child>>(4);

	private static Stack<List<Plane>> frustumPool = new Stack<List<Plane>>(32);

	#if UNITY_EDITOR
	private class ClipRenderData
	{
		public ClipRenderData(List<ClipVertex> clippedPortalVerts, bool forward)
		{
			this.clippedPortalVerts = new List<ClipVertex>(clippedPortalVerts);
			this.forward = forward;
		}

		public List<ClipVertex> clippedPortalVerts = null;
		public bool forward = true;
	}

	// For debug visualization
	private List<ClipRenderData> clipPortalData = new List<ClipRenderData>(16);
	private List<ClipRenderData> clipOccluderData = new List<ClipRenderData>(16);
	private Mesh debugFrustumMesh;
	private Mesh debugOccluderMesh;
	private static Color ClippedPortalColor = Color.blue;
	private static Color ClippedOccluderColor = Color.red;
	#endif
	#endregion

	#region Public Interface
	[SECTR_ToolTip("The layer that culled objects should be assigned to.", false, true)]
	public int InvisibleLayer = 0;
	[SECTR_ToolTip("Distance to draw clipped frustums.", 0f, 100f)]
	public float GizmoDistance = 10f;
	[SECTR_ToolTip("Material to use to render the debug frustum mesh.")]
	public Material GizmoMaterial = null;
	[SECTR_ToolTip("Makes the Editor camera display the Game view's culling while playing in editor.")]
	public bool CullInEditor = false;
	[SECTR_ToolTip("Set to false to disable shadow culling post pass.", true)]
	public bool CullShadows = true;
	[SECTR_ToolTip("Use another camera for culling properties.", true)]
	public Camera cullingProxy = null;
	
	public Camera CullingCamera
	{
		set { cullingProxy = value; }
	}

	public void AddCullersToAllMembers()
	{
		int numMemebers = SECTR_Member.All.Count;
		for(int memberIndex = 0; memberIndex < numMemebers; ++memberIndex)
		{
			SECTR_Member member = SECTR_Member.All[memberIndex];
			SECTR_Culler culler = member.GetComponent<SECTR_Culler>();
			if(!culler)
			{
				culler = member.gameObject.AddComponent<SECTR_Culler>();
				culler.CullEachChild = member.IsSector;
			}
		}
	}
	#endregion

	#region Unity Interface
	void OnEnable()
	{
#if UNITY_EDITOR
		debugFrustumMesh = new Mesh();
		if(!GizmoMaterial)
		{
			GizmoMaterial = (Material)(AssetDatabase.LoadAssetAtPath("Assets/SECTR/Code/Vis/Assets/FrustumDebug.mat", typeof(Material)));
		}
#endif
	}

	void OnDisable()
	{
#if UNITY_EDITOR
		Mesh.DestroyImmediate(debugFrustumMesh);
		debugFrustumMesh = null;
		GizmoMaterial = null;
#endif
	}

	void OnDestroy()
	{
#if UNITY_EDITOR
		Camera sceneCamera = SceneView.lastActiveSceneView ? SceneView.lastActiveSceneView.camera : null;
		if(sceneCamera)
		{
			SECTR_CullingCamera sceneCamCuller = sceneCamera.GetComponent<SECTR_CullingCamera>();
			if(sceneCamCuller != null && sceneCamCuller.cullingProxy == GetComponent<Camera>())
			{
				sceneCamCuller.cullingProxy = null;
				sceneCamCuller.enabled = false;
				
				int numPortals = SECTR_Portal.All.Count;
				for(int portalIndex = 0; portalIndex < numPortals; ++portalIndex)
				{
					SECTR_Portal portal = SECTR_Portal.All[portalIndex];
					portal.Hidden = false;
				}
			}
		}
#endif
	}

	void OnPreCull()
	{	
#if UNITY_EDITOR
		clipPortalData.Clear();
		clipOccluderData.Clear();
#endif
		// Compute the culling camear and some constants that we'll use throughout the walk.
		Camera cullingCamera = cullingProxy != null ? cullingProxy : GetComponent<Camera>();
		Vector3 cameraPos = cullingCamera.transform.position;
		float maxCameraFOVAngle = Mathf.Max(cullingCamera.fieldOfView, cullingCamera.fieldOfView * cullingCamera.aspect) * 0.5f;
		float maxCameraFOV = Mathf.Cos(maxCameraFOVAngle * Mathf.Deg2Rad);
		const float kNEAR_CLIP_SCALE = 1.001f;
		float maxNearClipDistance = (cullingCamera.nearClipPlane / maxCameraFOV) * kNEAR_CLIP_SCALE;

		// Get all of the Sectors that contain the camera, which can be more than one.
		// If the camera is not inside any Sector at all, no culling is possible, so just skip it.
		SECTR_Sector.GetContaining(ref initialSectors, new Bounds(cameraPos, new Vector3(maxNearClipDistance, maxNearClipDistance, maxNearClipDistance)));
		int numInitialSectors = initialSectors.Count;
		if(enabled && cullingCamera.enabled && numInitialSectors > 0)
		{
			Profiler.BeginSample("Setup");
			didCull = true;

#if UNITY_EDITOR
			bool fullCulling = (!Application.isEditor || Application.isPlaying);
			didCull = fullCulling;
#endif

			if(InvisibleLayer != 0
#if UNITY_EDITOR
			&& fullCulling
#endif
			   )
			{
				cullingCamera.cullingMask &= ~(1 << InvisibleLayer);
			}

			// We only want to visit each portal once, so we'll mark them in order to avoid cycles.
			// Note that we do not flag Sectors because a single Sector may have several active portals into it.
			#if UNITY_EDITOR
			int numPortals = SECTR_Portal.All.Count;
			for(int portalIndex = 0; portalIndex < numPortals; ++portalIndex)
			{
				SECTR_Portal portal = SECTR_Portal.All[portalIndex];
				if(Selection.activeObject && Selection.activeObject == cullingCamera.gameObject && fullCulling)
				{
					portal.Hidden = true;
				}
			}
			#endif

			if(SECTR_Culler.All.Count < SECTR_Member.All.Count 
#if UNITY_EDITOR
			&& fullCulling
#endif
			   )
			{
				AddCullersToAllMembers();
			}

			// We'll walk the graph in a DFS, so use a stack.
			// We accumulate shadow lights and occluders as we go along.
			// In all cases, we clear rather than allocating a new List
			// so that we don't create any more garbage than necessary. 
			nodeStack.Clear();
			shadowLights.Clear();
			visitedPortals.Clear();
			
			// Populate the stack with all of the Sectors that contain the camera to the stack.
			// The initial Sectors are use main camera frustum.
			Plane[] initialFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(cullingCamera);
			for(int sectorIndex = 0; sectorIndex < numInitialSectors; ++sectorIndex)
			{
				SECTR_Sector sector = initialSectors[sectorIndex];
				nodeStack.Push(new VisibilityNode(sector, null, initialFrustumPlanes));
			}
			Profiler.EndSample();
			
			// Walk the portal graph, culling as we go.
			// Start counter at -1 because first Sector has an implicit portal into it.
			Profiler.BeginSample("Graph Walk");
			while(nodeStack.Count > 0)
			{
				VisibilityNode currentNode = nodeStack.Pop();

				Profiler.BeginSample("Culling");
				if(currentNode.frustumPlanes != null)
				{
					cullingPlanes.Clear();
					cullingPlanes.AddRange(currentNode.frustumPlanes);
					int numCullingPlanes = cullingPlanes.Count;
					// If two frustum planes intersect at a very acute angle,
					// they may cull more pooly than necessary. To combat that,
					// we create a synthetic border plane at the intersection.
					for(int planeIndex = 0; planeIndex < numCullingPlanes; ++planeIndex)
					{
						Plane plane0 = cullingPlanes[planeIndex];
						Plane plane1 = cullingPlanes[(planeIndex+1) % cullingPlanes.Count];
						float normalDot = Vector3.Dot(plane0.normal, plane1.normal);
						// If the two planes are acute we want to add a plane, 
						// but not if they are directly oppositional (i.e. near and far)
						if(normalDot < -0.9f && normalDot > -0.99f)
						{
							// Compute the normal of the new plane
							Vector3 vecA = plane0.normal + plane1.normal;
							Vector3 vecB = Vector3.Cross(plane0.normal, plane1.normal);
							Vector3 newNormal = vecA - (Vector3.Dot(vecA, vecB) * vecB);
							newNormal.Normalize();
							
							// Compute a point on the line of intersection
							Matrix4x4 m = new Matrix4x4();
							m.SetRow(0, new Vector4(plane0.normal.x, plane0.normal.y, plane0.normal.z, 0f));
							m.SetRow(1, new Vector4(plane1.normal.x, plane1.normal.y, plane1.normal.z, 0f));
							m.SetRow(2, new Vector4(vecB.x, vecB.y, vecB.z, 0f));
							m.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
							Vector3 intersectionPos = m.inverse.MultiplyPoint3x4(new Vector3(-plane0.distance, -plane1.distance, 0f));								
							
							// Add the new plane, but make sure to skip it in the loop.
							// We don't want to do any checks against our newly added plane.
							cullingPlanes.Insert(++planeIndex, new Plane(newNormal, intersectionPos));
						}
					}

					// Now test the frustum against the contents of the sector.
					numCullingPlanes = cullingPlanes.Count;
					int baseMask = 0;
					for(int planeIndex = 0; planeIndex < numCullingPlanes; ++planeIndex)
					{
						baseMask |= 1 << planeIndex;
					}

					SECTR_Sector sector = currentNode.sector;

					// Add the occluders in this sector into the traversal list
					if(SECTR_Occluder.All.Count > 0)
					{
						List<SECTR_Occluder> sectorOccluders = SECTR_Occluder.GetOccludersInSector(sector);
						if(sectorOccluders != null)
						{
							int numOccluders = sectorOccluders.Count;
							for(int occluderIndex = 0; occluderIndex < numOccluders; ++occluderIndex)
							{
								SECTR_Occluder occluder = sectorOccluders[occluderIndex];
								int outMask;
								if(occluder.HullMesh && !activeOccluders.ContainsKey(occluder))
								{
									Matrix4x4 localToWorld = occluder.GetCullingMatrix(cameraPos);
									// Using the reverse normal here to save time in the plane construction below.
									Vector3 occluderReverseNormal = localToWorld.MultiplyVector(-occluder.MeshNormal).normalized;
									if(!SECTR_Geometry.IsPointInFrontOfPlane(cameraPos, occluder.Center, occluderReverseNormal))
									{
										Vector3[] srcVerts = occluder.VertsCW;
										int numOccluderVerts = srcVerts.Length;
										occluderVerts.Clear();
										Bounds occluderBounds = new Bounds(occluder.transform.position, Vector3.zero);
										for(int vertIndex = 0; vertIndex < numOccluderVerts; ++vertIndex)
										{
											Vector3 vertexWS = localToWorld.MultiplyPoint3x4(srcVerts[vertIndex]);
											occluderBounds.Encapsulate(vertexWS);
											occluderVerts.Add(new ClipVertex(new Vector4(vertexWS.x, vertexWS.y, vertexWS.z, 1), 0f));
										}
										if(SECTR_Geometry.FrustumIntersectsBounds(occluder.BoundingBox, cullingPlanes, baseMask, out outMask))
										{
											List<Plane> occluderFrustum;
											if(frustumPool.Count > 0)
											{
												occluderFrustum = frustumPool.Pop();
												occluderFrustum.Clear();
											}
											else
											{
												occluderFrustum = new List<Plane>(numOccluderVerts + 1);
											}
											_BuildFrustumFromHull(cullingCamera, true, occluderVerts, ref occluderFrustum);
											occluderFrustum.Add(new Plane(occluderReverseNormal, occluder.Center));
											occluderFrustums.Add(occluderFrustum);
											activeOccluders[occluder] = occluder;
											#if UNITY_EDITOR
											clipOccluderData.Add(new ClipRenderData(occluderVerts, true));
											#endif
										}
									}
								}
								#if UNITY_EDITOR
								occluder.CullingCamera = cullingCamera;
								#endif
							}
						}
					}

	#if UNITY_EDITOR
					if(fullCulling)
	#endif
					{
						// Sectors always have a culler.
						SECTR_Culler sectorCuller = sector.GetComponent<SECTR_Culler>();
						sectorCuller.FrustumCull(cameraPos, cullingPlanes, occluderFrustums, ref shadowLights, baseMask);

						// Cull individual members of the sector
						int numDynamicChildren = sector.Members.Count;
						for(int dynamicChildIndex = 0; dynamicChildIndex < numDynamicChildren; ++dynamicChildIndex)
						{
							SECTR_Member member = sector.Members[dynamicChildIndex];
							if(member && (member.HasRenderBounds || member.HasLightBounds))
							{
								SECTR_Culler memberCuller = member.GetComponent<SECTR_Culler>();
								memberCuller.FrustumCull(cameraPos, cullingPlanes, occluderFrustums, ref shadowLights, baseMask);
							}
						}
					}
					Profiler.EndSample();
		
					// Now we'll check our frustum against the portals in the Sector.
					// If the Sector frustum intersects any portals, we'll create a new clipped frustum
					// and pass that along for a future iteration of the search.
					Profiler.BeginSample("Clipping");
					int numNodePortals = currentNode.sector.Portals.Count;
					for(int portalIndex = 0; portalIndex < numNodePortals; ++portalIndex)
					{
						SECTR_Portal nextPortal = currentNode.sector.Portals[portalIndex];
						bool passThrough = (nextPortal.Flags & SECTR_Portal.PortalFlags.PassThrough) != 0;
						if(!visitedPortals.ContainsKey(nextPortal) && (nextPortal.HullMesh || passThrough) && (nextPortal.Flags & SECTR_Portal.PortalFlags.Closed) == 0)
						{
							// We need to know which direction we are passing through the portal,
							// so compute that up front based on the side we're coming through.
							bool forwardTraversal = currentNode.sector == nextPortal.FrontSector;
							// Get the next sector based on traversal direction
							SECTR_Sector nextSector = forwardTraversal ? nextPortal.BackSector : nextPortal.FrontSector;

							// Some early out checks
							bool earlyOut = !nextSector;

							// bail if we're on the wrong side of the plane
							if(!earlyOut)
							{
								earlyOut = SECTR_Geometry.IsPointInFrontOfPlane(cameraPos, nextPortal.Center, nextPortal.Normal) != forwardTraversal;
							}

							// bail if we're completely occluded
							if(!earlyOut && !passThrough)
							{
								int numOccluders = occluderFrustums.Count;
								for(int occluderIndex = 0; occluderIndex < numOccluders; ++occluderIndex)
								{
									if(SECTR_Geometry.FrustumContainsBounds(nextPortal.BoundingBox, occluderFrustums[occluderIndex]))
									{
										earlyOut = true;
										break;
									}
								}
							}

							if(earlyOut)
							{
								nodeStack.Push(new VisibilityNode(nextSector, nextPortal));
								visitedPortals[nextPortal] = nextPortal;
								continue;
							}

							// Time to build a new frustrum from the clipped Portal.
							// Transform portal vertices into world space to match our frustum planes.
							if(!passThrough)
							{
								portalVertices.Clear();
								Matrix4x4 localToWorld = nextPortal.transform.localToWorldMatrix;
								Vector3[] srcVerts = forwardTraversal ? nextPortal.VertsCCW : nextPortal.VertsCW;
								int numSrcVerts = srcVerts.Length;
								for(int vertIndex = 0; vertIndex < numSrcVerts; ++vertIndex)
								{
									Vector3 vertexWS = localToWorld.MultiplyPoint3x4(srcVerts[vertIndex]);
									portalVertices.Add(new ClipVertex(new Vector4(vertexWS.x, vertexWS.y, vertexWS.z, 1), 0f));
								}
							}

							// Build up the planes of the new culling frustum
							newFrustum.Clear();
							if((!passThrough && !nextPortal.IsPointInHull(cameraPos, maxNearClipDistance))
	#if UNITY_EDITOR
							|| (Application.isEditor && !Application.isPlaying && !passThrough)					
	#endif
							   )
							{
								// Now clip the portal by each plane in the frustum
								int numPlanes = currentNode.frustumPlanes.Count;
								for(int planeIndex = 0; planeIndex < numPlanes; ++planeIndex)
								{
									Plane frustrumPlane = currentNode.frustumPlanes[planeIndex];
									// Determine which side of the plane each vert is on 
									Vector4 planeVec = new Vector4(frustrumPlane.normal.x, frustrumPlane.normal.y, frustrumPlane.normal.z, frustrumPlane.distance);

									// We can do some extra optiizations if all verts are on one side of the plane.
									bool allPositive = true;
									bool allNegative = true;
									for(int vertIndex = 0; vertIndex < portalVertices.Count; ++vertIndex)
									{
										Vector4 vertex = portalVertices[vertIndex].vertex;
										float side = Vector4.Dot(planeVec, vertex);
										portalVertices[vertIndex] = new ClipVertex(vertex, side);
										allPositive = allPositive && side > 0;
										allNegative = allNegative && side <= -SECTR_Geometry.kVERTEX_EPSILON;
									}
									
									// If all points are on the neg side, then we can reject the entire portal
									if(allNegative)
									{
										portalVertices.Clear();
										break;
									}
									// If some points are on the positive side and some on the neg side of the plane
									// Then we need to clip the portal geometry by the plane.
									else if(!allPositive) 
									{	
										// First, we'll loop through the shape, inserting verts anywhere
										// that a pair of verts straddles the plane.
										int numVerts = portalVertices.Count;
										for(int vertIndex = 0; vertIndex < numVerts; ++vertIndex)
										{
											int nextVert = (vertIndex + 1) % portalVertices.Count;
											float lDotV0 = portalVertices[vertIndex].side;
											float lDotV1 = portalVertices[nextVert].side;
											if( (lDotV0 > 0f && lDotV1 <= -SECTR_Geometry.kVERTEX_EPSILON) ||
												(lDotV1 > 0f && lDotV0 <= -SECTR_Geometry.kVERTEX_EPSILON) )
											{
												Vector4 vPos0 = portalVertices[vertIndex].vertex;
												Vector4 vPos1 = portalVertices[nextVert].vertex;
												// T is the parametric position of the new vert
												// between the two verts that straddle the plane.
												float t = lDotV0 / Vector4.Dot(planeVec, vPos0 - vPos1);
												Vector4 newVertPos = vPos0 + (t * (vPos1 - vPos0));
												newVertPos.w = 1;
												portalVertices.Insert(vertIndex+1, new ClipVertex(newVertPos, 0f));
												++numVerts;
											}
										}
										
										// Now that all of the new verts are added, we remove any verts
										// that are on the wrong side of the plane.
										// It's this simple because the portal verts are pre-sorted CW or CCW.
										int portalVertIndex = 0;
										while(portalVertIndex < numVerts)
										{
											if(portalVertices[portalVertIndex].side < -SECTR_Geometry.kVERTEX_EPSILON)
											{
												portalVertices.RemoveAt(portalVertIndex);
												--numVerts;
											}
											else
											{
												++portalVertIndex;
											}
										}
									}
								}

								// With the final clipped portal plane we need to generate a new frustum.
								_BuildFrustumFromHull(cullingCamera, forwardTraversal, portalVertices, ref newFrustum);
							}
							else
							{
								// If the camera is very, very close to the portal, then
								// just pass the frustum on without modification
								newFrustum.AddRange(initialFrustumPlanes);
							}
								
							// At long last, push the next Sector/frustum onto the stack.
							if(newFrustum.Count > 2)
							{	
								nodeStack.Push(new VisibilityNode(nextSector, nextPortal, newFrustum));
								#if UNITY_EDITOR							
								clipPortalData.Add(new ClipRenderData(portalVertices, forwardTraversal));
								if(Selection.activeObject && Selection.activeObject == cullingCamera.gameObject && fullCulling)
								{
									nextPortal.Hidden = false;
								}
								#endif
							}
							else
							{
								nodeStack.Push(new VisibilityNode(nextSector, nextPortal));
							}
							// The next portal is visited regardless.
							visitedPortals[nextPortal] = nextPortal;
						}
					}
				}
				if(currentNode.portal)
				{
					visitedPortals.Remove(currentNode.portal);
				}
				if(currentNode.frustumPlanes != null)
				{
					frustumPool.Push(currentNode.frustumPlanes);
				}
				Profiler.EndSample();
			}
			Profiler.EndSample();

			// After we've culled the primary scene, we do a post pass checking for any objects
			// that overlap any of the shadow casting point or spot lights in the scene.
			Profiler.BeginSample("Shadows");
			int numShadowLights = shadowLights.Count;
			if(numShadowLights > 0 && CullShadows
			   #if UNITY_EDITOR
			   && fullCulling
			   #endif
			   )
			{
				Profiler.BeginSample("Shadow Setup");
				shadowSectorTable.Clear();
				for(int shadowIndex = 0; shadowIndex < numShadowLights; ++shadowIndex)
				{
					SECTR_Member.Child shadowLight = shadowLights[shadowIndex];
					List<SECTR_Sector> affectedSectors;
					if(shadowLight.member && shadowLight.member.IsSector)
					{
						shadowSectors.Clear();
						shadowSectors.Add((SECTR_Sector)shadowLight.member);
						affectedSectors = shadowSectors;
					}
					else if(shadowLight.member && shadowLight.member.Sectors.Count > 0)
					{
						affectedSectors = shadowLight.member.Sectors;
					}
					else
					{
						SECTR_Sector.GetContaining(ref shadowSectors, shadowLight.lightBounds);
						affectedSectors = shadowSectors;
					}
					int numSectors = affectedSectors.Count;
					for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
					{
						SECTR_Sector sector = affectedSectors[sectorIndex];
						List<SECTR_Member.Child> sectorShadowLights;
						if(!shadowSectorTable.TryGetValue(sector, out sectorShadowLights))
						{
							sectorShadowLights = new List<SECTR_Member.Child>(16);
							shadowSectorTable[sector] = sectorShadowLights;
						}

						sectorShadowLights.Add(shadowLight);
					}
				}
				Profiler.EndSample();

				Dictionary<SECTR_Sector, List<SECTR_Member.Child>>.Enumerator shadowEnum = shadowSectorTable.GetEnumerator();
				while(shadowEnum.MoveNext())
				{
					SECTR_Sector sector = shadowEnum.Current.Key;
					List<SECTR_Member.Child> sectorShadowLights = shadowEnum.Current.Value;
					Profiler.BeginSample("Shadow Sector Cull");
					if(sector.ShadowCaster)
					{
						SECTR_Culler culler = sector.GetComponent<SECTR_Culler>();
						culler.ShadowCull(sectorShadowLights);
					}
					Profiler.EndSample();

					Profiler.BeginSample("Shadow Member Cull");
					int numDynamicChildren = sector.Members.Count;
					for(int dynamicChildIndex = 0; dynamicChildIndex < numDynamicChildren; ++dynamicChildIndex)
					{
						SECTR_Member member = sector.Members[dynamicChildIndex];
						if(member.ShadowCaster)
						{
							SECTR_Culler memberCuller = member.GetComponent<SECTR_Culler>();
							memberCuller.ShadowCull(sectorShadowLights);
						}
					}
					Profiler.EndSample();
				}
			}
			Profiler.EndSample();

			int numOccluderFrustums = occluderFrustums.Count;
			for(int occluderIndex = 0; occluderIndex < numOccluderFrustums; ++occluderIndex)
			{
				frustumPool.Push(occluderFrustums[occluderIndex]);
			}
			occluderFrustums.Clear();
			activeOccluders.Clear();

			// Actually hide everything that needs to be hidden
			Profiler.BeginSample("Apply");
#if UNITY_EDITOR
			if(fullCulling)
#endif
			{
				int numCullers = SECTR_Culler.All.Count;
				for(int cullerIndex = 0; cullerIndex < numCullers; ++cullerIndex)
				{
					SECTR_Culler culler = SECTR_Culler.All[cullerIndex];
					culler.ApplyCulling(InvisibleLayer);
				}
			}
			Profiler.EndSample();
		}
	}
	
	void OnPostRender()
	{
		if(didCull)
		{
			int numCullers = SECTR_Culler.All.Count;
			for(int cullerIndex = 0; cullerIndex < numCullers; ++cullerIndex)
			{
				SECTR_Culler.All[cullerIndex].UndoCulling();
			}
			didCull = false;
		}
	}

	#if UNITY_EDITOR
	void OnDrawGizmos() 
	{
		Camera sceneCamera = SceneView.lastActiveSceneView.camera;
		if(sceneCamera)
		{
			SECTR_CullingCamera sceneCamCuller = sceneCamera.GetComponent<SECTR_CullingCamera>();
			if(Selection.activeObject == gameObject && enabled && CullInEditor)
			{
				if(sceneCamCuller == null)
				{
					sceneCamCuller = sceneCamera.gameObject.AddComponent<SECTR_CullingCamera>();
				}
				sceneCamCuller.cullingProxy = GetComponent<Camera>();
				sceneCamCuller.CullShadows = CullShadows;
				sceneCamCuller.enabled = true;
			}
			else if(sceneCamCuller != null && sceneCamCuller.cullingProxy == GetComponent<Camera>())
			{
				sceneCamCuller.cullingProxy = null;
				sceneCamCuller.enabled = false;
				
				int numPortals = SECTR_Portal.All.Count;
				for(int portalIndex = 0; portalIndex < numPortals; ++portalIndex)
				{
					SECTR_Portal portal = SECTR_Portal.All[portalIndex];
					portal.Hidden = false;
				}
			}
		}
	}

	void OnDrawGizmosSelected() 
	{
		if(enabled)
		{
			Gizmos.matrix = Matrix4x4.identity;
			Vector3 origin = cullingProxy ? cullingProxy.transform.position : transform.position;

			Gizmos.color = ClippedPortalColor;
			int numClippedPortals = clipPortalData.Count;
			for(int portalIndex = 0; portalIndex < numClippedPortals; ++portalIndex)
			{
				ClipRenderData renderData = clipPortalData[portalIndex];
				List<ClipVertex> _clippedVerts = renderData.clippedPortalVerts;
				int numClippedVerts = _clippedVerts.Count;
				for(int vertIndex = 0; vertIndex < numClippedVerts; ++vertIndex)
				{
					Vector3 vert = _clippedVerts[vertIndex].vertex;
					int nextIndex = (vertIndex + 1) % numClippedVerts;
					Vector3 nextVert = _clippedVerts[nextIndex].vertex;
					Gizmos.DrawLine(vert, nextVert);
					
					Vector3 projectedVert = vert + ((vert - origin).normalized * GizmoDistance);
					Vector3 projectedNextVert = nextVert + ((nextVert - origin).normalized * GizmoDistance);
					
					Gizmos.DrawLine(vert, projectedVert);
					Gizmos.DrawLine(nextVert, projectedNextVert);
					Gizmos.DrawLine(projectedVert, projectedNextVert);
				}
			}

			Gizmos.color = ClippedOccluderColor;
			int numClippedOccluders = clipOccluderData.Count;
			for(int occluderIndex = 0; occluderIndex < numClippedOccluders; ++occluderIndex)
			{
				ClipRenderData renderData = clipOccluderData[occluderIndex];
				List<ClipVertex> _clippedVerts = renderData.clippedPortalVerts;
				int numClippedVerts = _clippedVerts.Count;
				for(int vertIndex = 0; vertIndex < numClippedVerts; ++vertIndex)
				{
					Vector3 vert = _clippedVerts[vertIndex].vertex;
					int nextIndex = (vertIndex + 1) % numClippedVerts;
					Vector3 nextVert = _clippedVerts[nextIndex].vertex;
					Gizmos.DrawLine(vert, nextVert);
					
					Vector3 projectedVert = vert + ((vert - origin).normalized * GizmoDistance);
					Vector3 projectedNextVert = nextVert + ((nextVert - origin).normalized * GizmoDistance);
					
					Gizmos.DrawLine(vert, projectedVert);
					Gizmos.DrawLine(nextVert, projectedNextVert);
					Gizmos.DrawLine(projectedVert, projectedNextVert);
				}
			}
			
			if(GizmoMaterial && debugFrustumMesh)
			{
				_BuildFrustumMesh(ref debugFrustumMesh, origin, GizmoDistance);
				Gizmos.color = Color.white;
				GizmoMaterial.SetPass(0);
				Graphics.DrawMeshNow(debugFrustumMesh, Matrix4x4.identity);
			}
		}
	}
	#endif
	#endregion

	#region Private Methods
	private void _BuildFrustumFromHull(Camera cullingCamera, bool forwardTraversal, List<ClipVertex> portalVertices, ref List<Plane> newFrustum)
	{
		// If there are not at least three verts, we can skip the portal
		// (and skip the Sector beyond) entirely.
		int numPortalVerts = portalVertices.Count;
		if(numPortalVerts > 2)
		{
			// Each edge of the portal polygon will become a new plane.
			for(int portalVertIndex = 0; portalVertIndex < numPortalVerts; ++portalVertIndex)
			{
				Vector3 vert0 = portalVertices[portalVertIndex].vertex;
				Vector3 vert1 = portalVertices[(portalVertIndex + 1) % numPortalVerts].vertex;
				Vector3 edgeVec = vert1 - vert0;
				if(Vector3.SqrMagnitude(edgeVec) > SECTR_Geometry.kVERTEX_EPSILON)
				{
					Vector3 cameraVec = vert0 - cullingCamera.transform.position;
					Vector3 planeVec = forwardTraversal ? Vector3.Cross(edgeVec, cameraVec) : Vector3.Cross(cameraVec, edgeVec);
					planeVec.Normalize();
					newFrustum.Add(new Plane(planeVec, vert0));
				}
			}
		}
	}

    #if UNITY_EDITOR
	private void _BuildFrustumMesh(ref Mesh mesh, Vector3 origin, float projDist)
	{
		mesh.Clear();
		List<Vector3> verts = new List<Vector3>(32);
		List<Vector3> normals = new List<Vector3>(32);
		List<Vector2> uvs = new List<Vector2>(32);
		List<Color> vertColors = new List<Color>(32);
		List<int> tris = new List<int>(32);

		int numClippedPortals = clipPortalData.Count;
		for(int portalIndex = 0; portalIndex < numClippedPortals; ++portalIndex)
		{
			ClipRenderData renderData = clipPortalData[portalIndex];
			List<ClipVertex> _clippedVerts = renderData.clippedPortalVerts;
			int numClippedVerts = _clippedVerts.Count;
			for(int vertIndex = 0; vertIndex < numClippedVerts; ++vertIndex)
			{
				int vertIndex0 = renderData.forward ? vertIndex : numClippedVerts - 1 - vertIndex;
				Vector3 vert0 = _clippedVerts[vertIndex0].vertex;
				int vertIndex1 = renderData.forward ? (vertIndex0 + 1) % numClippedVerts : vertIndex0 - 1;
				if(vertIndex1 < 0 )
				{
					vertIndex1 = numClippedVerts - 1;
				}
				Vector3 vert1 = _clippedVerts[vertIndex1].vertex;

				Vector3 projectedVert0 = vert0 + ((vert0 - origin).normalized * projDist);
				Vector3 projectedVert1 = vert1 + ((vert1 - origin).normalized * projDist);
				
				int baseIndex = verts.Count;
				verts.Add(vert0);
				verts.Add(projectedVert0);
				verts.Add(vert1);
				verts.Add(projectedVert1);

				Vector3 edgeNormal = Vector3.Cross((vert1 - vert0).normalized, (projectedVert0 - vert0).normalized).normalized;
				
				normals.Add(edgeNormal);
				normals.Add(edgeNormal);
				normals.Add(edgeNormal);
				normals.Add(edgeNormal);
				
				uvs.Add(new Vector2(0,0));
				uvs.Add(new Vector2(1,0));
				uvs.Add(new Vector2(0,1));
				uvs.Add(new Vector2(1,1));
				
				vertColors.Add(new Color(1,1,1,1));
				vertColors.Add(new Color(1,1,1,0));
				vertColors.Add(new Color(1,1,1,1));
				vertColors.Add(new Color(1,1,1,0));
				
				tris.Add(baseIndex);
				tris.Add(baseIndex + 1);
				tris.Add(baseIndex + 3);

				tris.Add(baseIndex);
				tris.Add(baseIndex + 3);
				tris.Add(baseIndex + 2);
			}
		}

		mesh.vertices = verts.ToArray();
		mesh.normals = normals.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.colors = vertColors.ToArray();
		mesh.triangles = tris.ToArray();
		mesh.RecalculateBounds();
	}
	#endif
	#endregion
}
