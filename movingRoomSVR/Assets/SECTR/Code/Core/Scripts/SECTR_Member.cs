// Copyright (c) 2014 Nathan Martz

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

/// \ingroup Core
/// Member represents anything that can be part of a SECTR_Sector, including Sectors themselves. 
/// 
/// Member's primary job is to figure out which Sectors a given GameObject belongs to. In
/// order to accomplish this goal, it needs to compute a bounding box (actually several) and to
/// periodically check to see which Sectors overlap that box. SectorMember also caches information
/// that other clients are interested in, such as the aggregate render bounds of its children,
/// or a list of child Light components.
/// 
/// Members may be dynamic or static, and will save some per-frame CPU work if marked as
/// static. Care should be taken that static Members by accurately marked, as moving children
/// of a static Member may produce unexpected results.
/// 
/// Note that Members may have children that are themselves Members. The code will
/// automatically detect this case and not include the child Member's children in its list.
/// This can be a very convienient way to control the granuality of scene partitioning, especially
/// for culling and streaming.
[ExecuteInEditMode]
[AddComponentMenu("SECTR/Core/SECTR Member")]
public class SECTR_Member : MonoBehaviour
{	
	#region Private Details
	private List<Child> children = new List<Child>(16);
	private List<Child> renderers = new List<Child>(16);
	private List<Child> lights = new List<Child>(16);
	private List<Child> terrains = new List<Child>(2);
	private List<Child> shadowLights = SECTR_Modules.VIS ? new List<Child>(16) : null;
	private List<Child> shadowCasters = SECTR_Modules.VIS ? new List<Child>(16) : null;
	private List<SECTR_Sector> sectors = new List<SECTR_Sector>(4);
	private List<SECTR_Sector> newSectors = new List<SECTR_Sector>(4);
	private List<SECTR_Sector> leftSectors = new List<SECTR_Sector>(4);
	private Dictionary<Light, Light> bakedOnlyTable = null;
	private Vector3 lastPosition = Vector3.zero;
	private bool started = false;
	private bool usedStartSector = false;
	
	[SerializeField] [HideInInspector] private Bounds totalBounds;
	[SerializeField] [HideInInspector] private Bounds renderBounds;
	[SerializeField] [HideInInspector] private Bounds lightBounds;
	[SerializeField] [HideInInspector] private bool hasRenderBounds = false;
	[SerializeField] [HideInInspector] private bool hasLightBounds = false;
	[SerializeField] [HideInInspector] private bool shadowCaster = false;
	[SerializeField] [HideInInspector] private bool shadowLight = false;
	[SerializeField] [HideInInspector] private bool frozen = false;
	[SerializeField] [HideInInspector] protected bool isSector = false;
	[SerializeField] [HideInInspector] protected List<Light> bakedOnlyLights = SECTR_Modules.VIS ? new List<Light>(8) : null; 

	private static List<SECTR_Member> allMembers = new List<SECTR_Member>(256);
	#endregion

	#region Public Interface
	/// Simple data structure to represent the important information about one of the
	/// children of a SECTR_Member.
	public struct Child
	{
		public void Init(GameObject gameObject, Renderer renderer, Light light, Terrain terrain, SECTR_Member member, bool dirShadowCaster, Vector3 shadowVec)
		{
			this.gameObject = gameObject;
			this.member = member;
			this.renderer = renderer && renderer.enabled ? renderer : null;
			this.light = (light && light.enabled && (light.type == LightType.Point || light.type == LightType.Spot)) ? light : null;
			this.terrain = (terrain && terrain.enabled) ? terrain : null;
			rendererBounds = this.renderer ? this.renderer.bounds : new Bounds();
			lightBounds = this.light ? SECTR_Geometry.ComputeBounds(this.light) : new Bounds();
			terrainBounds = this.terrain ? SECTR_Geometry.ComputeBounds(this.terrain) : new Bounds();
			#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1
			shadowLight = this.light && light.shadows != LightShadows.None;
			#else
			shadowLight = this.light && light.shadows != LightShadows.None && (!light.alreadyLightmapped || (LightmapSettings.lightmapsMode == LightmapsMode.CombinedDirectional));
			#endif
			rendererCastsShadows = this.renderer && renderer.castShadows && (renderer.lightmapIndex == -1 || LightmapSettings.lightmapsMode == LightmapsMode.CombinedDirectional);
			terrainCastsShadows = this.terrain && terrain.castShadows && (terrain.lightmapIndex == -1 || LightmapSettings.lightmapsMode == LightmapsMode.CombinedDirectional);

			if(dirShadowCaster)
			{
				if(rendererCastsShadows)
				{
					rendererBounds = SECTR_Geometry.ProjectBounds(rendererBounds, shadowVec);
				}
				if(terrainCastsShadows)
				{
					terrainBounds = SECTR_Geometry.ProjectBounds(terrainBounds, shadowVec);
				}
			}
		}
		
		/// The game object this member was created from.
		public GameObject gameObject;
		/// The Member this child belongs to.
		public SECTR_Member member;
		/// Renderer component of Member child. Can be null. 
		public Renderer renderer;
		/// Light component of Member child. Can be null.
		public Light light;
		/// Terrain component of Member child. Can be null.
		public Terrain terrain;
		/// Cached world space bounds of the renderer component.
		public Bounds rendererBounds;
		/// Cached world space bounds of the light component.
		public Bounds lightBounds;
		/// Cached world space bounds of the terrain component.
		public Bounds terrainBounds;
		/// Cached value of ability to create dynamic shadows.
		public bool shadowLight;
		/// Cached value renderer dynamic shadow casting.
		public bool rendererCastsShadows;
		/// Cached value of terrain ability to cast dynamic shadows.
		public bool terrainCastsShadows;
	};

	/// Rules for how often to update the children and bounds.
	public enum BoundsUpdateModes
	{
		/// Compute children on start. Update bounds on movement.
		Start,			
		/// Compute children and bounds on movement.
		Movement,		
		/// Compute children and bounds on every update.
		Always,			
		/// Compute children and bounds on only on start.
		Static,			
	};

	[SECTR_ToolTip("Set to true if Sector membership should only change when crossing a portal.")]
	public bool PortalDetermined = false;
	[SECTR_ToolTip("If set, forces the initial Sector to be the specified Sector.", "PortalDetermined")]
	public SECTR_Sector ForceStartSector = null;
	[SECTR_ToolTip("Determines how often the bounds are recomputed. More frequent updates requires more CPU.")]
	public BoundsUpdateModes BoundsUpdateMode = BoundsUpdateModes.Always;
	[SECTR_ToolTip("Adds a buffer on bounding box to compensate for minor imprecisions.")]
	public float ExtraBounds = SECTR_Geometry.kBOUNDS_CHEAT;
	[SECTR_ToolTip("Override computed bounds with the user specified bounds. Advanced users only.")]
	public bool OverrideBounds = false;
	[SECTR_ToolTip("User specified override bounds. Auto-populated with the current bounds when override is inactive.", "OverrideBounds")]
	public Bounds BoundsOverride;
	[SECTR_ToolTip("Optional shadow casting directional light to use in membership calculations. Bounds will be extruded away from light, if set.")]
	public Light DirShadowCaster;
	[SECTR_ToolTip("Distance by which to extend the bounds away from the shadow casting light.", "DirShadowCaster")]
	public float DirShadowDistance = 100;

	/// Returns a list of all enabled Members.
	public static List<SECTR_Member> All
	{
		get { return allMembers; }
	}

	/// Returns a flattened list of all relevant children.
	public List<Child> Children
	{
		get { return children; }
	}

	/// Returns the subset of the Children list that contains Renderers.
	public List<Child> Renderers
	{
		get { return renderers; }
	}

	/// Returns true if any child renderer components casts shadows.
	public bool ShadowCaster
	{
		get { return shadowCaster; }
	}

	/// Returns the subset of the Children list that cast shadows.
	public List<Child> ShadowCasters
	{
		get { return shadowCasters; }
	}

	/// Returns the subset of the Children list that contains Lights.
	public List<Child> Lights
	{
		get { return lights; }
	}

	/// Returns true if any child light components create shadows.
	public bool ShadowLight
	{
		get { return shadowLight; }
	}

	/// Returns the subset of the Children list that create shadows.
	public List<Child> ShadowLights
	{
		get { return shadowLights; }
	}

	/// Returns the subset of the Children list that contains Lights.
	public List<Child> Terrains
	{
		get { return terrains; }
	}

	/// Returns all the Sectors that this object belongs to.
	public List<SECTR_Sector> Sectors
	{
		get { return sectors; }
	}

	/// Returns the union of the RenderBounds and LightBounds
	public Bounds TotalBounds
	{
		get { return totalBounds; }
	}

	/// Returns the union of the Bounds of all child Renderers.
	public Bounds RenderBounds
	{
		get { return renderBounds; }
	}

	/// Returns true if the RenderBounds contains valid data.
	public bool HasRenderBounds
	{
		get { return hasRenderBounds; }
	}

	/// Returns the union of the Bounds of all child Lights.
	public Bounds LightBounds
	{
		get { return lightBounds; }
	}

	/// Returns true if the LightBounds contains valid data.
	public bool HasLightBounds
	{
		get { return hasLightBounds; }
	}

	/// (Un)Freezes (i.e. disables updates and preserves bounds).
	public bool Frozen
	{
		set
		{
			// Currently intended for sectors only.
			if(isSector && frozen != value)
			{
				frozen = value;
				if(!frozen)
				{
					_UpdateChildren();
				}
			}
		}
		get { return frozen; }
	}

	public bool IsSector
	{
		get { return isSector; }
	}

	/// Forces an update. Used only in special cases.
	public void ForceUpdate()
	{
		_UpdateChildren();
		lastPosition = transform.position;
		if(!isSector)
		{
			_UpdateSectorMembership();
		}
	}

	/// Called when a Sector is being destroyed 
	public void SectorDisabled(SECTR_Sector sector)
	{
		if(sector)
		{
			sectors.Remove(sector);
			if(Changed != null)
			{
				leftSectors.Clear();
				leftSectors.Add(sector);
				Changed(leftSectors, null);
			}
		}
	}

	/// Delegate delcaration for anyone who wants to be notified when membership changes.
	public delegate void MembershipChanged(List<SECTR_Sector> left, List<SECTR_Sector> joined);

	/// Event handler for membership changed callbacks.
	public event MembershipChanged Changed;

#if UNITY_EDITOR
	// Debug Rendering stuff
	public static Color MemberColor = Color.white;
#endif
	#endregion

	#region Unity Interface
	void Start()
	{
		started = true;
		ForceUpdate();
	}

	protected virtual void OnEnable()
	{
		allMembers.Add(this);
		if(bakedOnlyLights != null)
		{
			int numBakedOnlyLights = bakedOnlyLights.Count;
			bakedOnlyTable = new Dictionary<Light, Light>(numBakedOnlyLights);
			for(int lightIndex = 0; lightIndex < numBakedOnlyLights; ++lightIndex)
			{
				Light bakedOnlyLight = bakedOnlyLights[lightIndex];
				bakedOnlyTable[bakedOnlyLight] = bakedOnlyLight;
			}
		}
		if(started)
		{
			ForceUpdate();
		}
	}
	
	protected virtual void OnDisable()
	{
		if(Changed != null && sectors.Count > 0)
		{
			Changed(sectors, null);
		}

		if(!isSector)
		{
			int numSectors = sectors.Count;
			for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
			{
				SECTR_Sector sector = sectors[sectorIndex];
				if(sector)
				{
					sector.Deregister(this);
				}
			}
			sectors.Clear();
		}
		bakedOnlyTable = null;
		allMembers.Remove(this);
	}

	#if UNITY_EDITOR
	protected virtual void OnDrawGizmosSelected() 
	{
		// Render a cube of the aggregate bounds.
		if(enabled && !isSector)
		{
			// Draw the bounds
			Gizmos.color = MemberColor;
			Gizmos.DrawWireCube( totalBounds.center, totalBounds.size ); 

			// Render links to neighbor Sectors.
			Gizmos.color = SECTR_Sector.SectorColor;
			int numSectors = sectors.Count;
			for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
			{
				SECTR_Sector sector = sectors[sectorIndex];
				Gizmos.DrawLine(totalBounds.center, sector.TotalBounds.center);
			}
		}
	}
	#endif
	
	void LateUpdate()
	{
#if UNITY_EDITOR
		if(gameObject.isStatic && Application.isEditor && !Application.isPlaying)
		{
			BoundsUpdateMode = BoundsUpdateModes.Static;
		}
#endif

		// In the Editor, we're always dynamic, all the time, because
		// users modify static objects constantly.
		if(
#if UNITY_3_5 || UNITY_4_0
		BoundsUpdateMode != BoundsUpdateModes.Static
#else
		BoundsUpdateMode != BoundsUpdateModes.Static && (BoundsUpdateMode == BoundsUpdateModes.Always || transform.hasChanged)
#endif
#if UNITY_EDITOR
		|| (Application.isEditor && !Application.isPlaying)
#endif
		)
		{
			_UpdateChildren();
			if(!isSector)
			{			
				_UpdateSectorMembership();
			}
			lastPosition = transform.position;
		}
	}
	#endregion
	
	#region Private Utility Methods
	// Recomputes all Child related data.
	private void _UpdateChildren()
	{
		if(frozen)
		{
			return;
		}

		bool dirShadowCaster = SECTR_Modules.VIS && DirShadowCaster && DirShadowCaster.type == LightType.Directional && DirShadowCaster.shadows != LightShadows.None;
		Vector3 shadowVec = dirShadowCaster ? DirShadowCaster.transform.forward * DirShadowDistance : Vector3.zero;

		// Start mode is a fast path that assumes children do not change, so we don't need to find them
		// every frame. Instead, use the children we have to update the bounds accordingly.
		if(BoundsUpdateMode == BoundsUpdateModes.Start && children.Count > 0
#if UNITY_EDITOR
		   && (!Application.isEditor || Application.isPlaying)
#endif
		) 
		{
			hasLightBounds = false;
			hasRenderBounds = false;
			shadowCaster = false;
			shadowLight = false;

			int numChildren = children.Count;
			for(int childIndex = 0; childIndex < numChildren; ++ childIndex)
			{
				Child child = children[childIndex];
				child.Init(child.gameObject, child.renderer, child.light, child.terrain, child.member, dirShadowCaster, shadowVec);
				if(child.renderer)
				{
					if(!hasRenderBounds)
					{
						renderBounds = child.rendererBounds;
						hasRenderBounds = true;
					}
					else
					{
						renderBounds.Encapsulate(child.rendererBounds);
					}
					shadowCaster |= child.rendererCastsShadows;
				}

				if(child.terrain)
				{
					if(!hasRenderBounds)
					{
						renderBounds = child.terrainBounds;
						hasRenderBounds = true;
					}
					else
					{
						renderBounds.Encapsulate(child.terrainBounds);
					}
					shadowCaster |= child.terrainCastsShadows;
				}

				if(child.light)
				{
					if(!hasLightBounds)
					{
						lightBounds = child.lightBounds;
						hasLightBounds = true;
					}
					else
					{
						lightBounds.Encapsulate(child.lightBounds);
					}
					shadowLight |= child.shadowLight;
				}
			}
		}
		else
		{
			children.Clear();
			renderers.Clear();
			lights.Clear();
			terrains.Clear();
			if(SECTR_Modules.VIS)
			{
				shadowCasters.Clear();
				shadowLights.Clear();
			}
			hasLightBounds = false;
			hasRenderBounds = false;
			shadowCaster = false;
			shadowLight = false;

			#if UNITY_EDITOR
			if(bakedOnlyLights != null)
			{
				bakedOnlyLights.Clear();
			}
			#endif

			_AddChildren(transform, dirShadowCaster, shadowVec);
		}

		Bounds zeroBounds = new Bounds(transform.position, Vector3.zero);
		if(hasRenderBounds && isSector)
		{
			totalBounds = renderBounds;
		}
		else if(hasRenderBounds && hasLightBounds)
		{
			totalBounds = renderBounds;
			totalBounds.Encapsulate(lightBounds);
		}
		else if(hasRenderBounds)
		{
			totalBounds = renderBounds;
			lightBounds = zeroBounds;
		}
		else if(hasLightBounds)
		{
			totalBounds = lightBounds;
			renderBounds = zeroBounds;
		}
		else
		{
			totalBounds = zeroBounds;
			lightBounds = zeroBounds;
			renderBounds = zeroBounds;
		}
		totalBounds.Expand(ExtraBounds);

		// Bounds override only affects the total member bounds.
		// We'll keep light and render bounds honest (for now).
		if(OverrideBounds)
		{
			totalBounds = BoundsOverride;
		}
#if UNITY_EDITOR
		else
		{
			BoundsOverride = totalBounds;
		}
#endif
	}

	// Recursive utility function to walk the tree of objects.
	private void _AddChildren(Transform childTransform, bool dirShadowCaster, Vector3 shadowVec)
	{
		// We always stop the descent when we encounter another SectorMember 
		if(childTransform.gameObject.activeSelf && (childTransform == transform || childTransform.GetComponent<SECTR_Member>() == null))
		{
			Light childLight = childTransform.GetComponent<Light>();
			Renderer childRenderer = childTransform.GetComponent<Renderer>();
			Terrain childTerrain = null;

			// Perform some more expensive Sector specific logic.
			if(isSector)
			{
				// Avoid an expensive GetComponent check as it's very unlikely to have Terrain's outside of Sectors.
				childTerrain = childTransform.GetComponent<Terrain>();
				// Filter particle systems out of Sectors as their bounds are simply too unreliable.
				if(childRenderer)
				{
					System.Type renderType = childRenderer.GetType();
					if(renderType == typeof(ParticleSystemRenderer) || renderType == typeof(ParticleRenderer))
					{
						childRenderer = null;
					}
				}
			}
			//

			// Baked only lights are a waste of CPU time for visibility culling.
			// So we want to ignore them as far as Member's are concerned, but
			// doing som requires a bit of a hack.
			if(bakedOnlyLights != null && childLight)
			{
				// Unfortunately, the only way to find out if a light is baked-only is in the Editor,
				// so we create and store a list of all baked lights.
				#if UNITY_EDITOR
				SerializedObject serialObj = new SerializedObject(childLight); 
				SerializedProperty lightmapProp = serialObj.FindProperty("m_Lightmapping");
				if(lightmapProp != null && lightmapProp.enumNames[lightmapProp.enumValueIndex] == "BakedOnly")
				{
					bakedOnlyLights.Add(childLight);
					if(bakedOnlyTable == null)
					{
						bakedOnlyTable = new Dictionary<Light, Light>();
					}
					bakedOnlyTable[childLight] = childLight;
				}
				else if(bakedOnlyTable != null)
				{
					bakedOnlyTable.Remove(childLight);
				}
				#endif

				// In editor and in game, we skip over any lights contained in the list
				// of baked only lights. We use a dictionary to compute things extra quickly.
				if(bakedOnlyTable != null && bakedOnlyTable.ContainsKey(childLight))
				{
					childLight = null;
				}
			}

			if(childRenderer || childLight || childTerrain)
			{
				Child newChild = new Child();
				newChild.Init(childTransform.gameObject, childRenderer, childLight, childTerrain, this, dirShadowCaster, shadowVec);
				if(newChild.renderer)
				{					
					if(!hasRenderBounds)
					{
						renderBounds = newChild.rendererBounds;
						hasRenderBounds = true;
					}
					else
					{
						renderBounds.Encapsulate(newChild.rendererBounds);
					}
					renderers.Add(newChild);
				}
				if(newChild.light)
				{
					if(SECTR_Modules.VIS)
					{
						if(newChild.shadowLight)
						{
							shadowLights.Add(newChild);
							shadowLight = true;
						}
					}

					if(!hasLightBounds)
					{
						lightBounds = newChild.lightBounds;
						hasLightBounds = true;
					}
					else
					{
						lightBounds.Encapsulate(newChild.lightBounds);
					}
					lights.Add(newChild);
				}
				if(newChild.terrain)
				{					
					if(!hasRenderBounds)
					{
						renderBounds = newChild.terrainBounds;
						hasRenderBounds = true;
					}
					else
					{
						renderBounds.Encapsulate(newChild.terrainBounds);
					}
					terrains.Add(newChild);
				}

				if(SECTR_Modules.VIS && (newChild.terrainCastsShadows || newChild.rendererCastsShadows))
				{
					shadowCasters.Add(newChild);
					shadowCaster = true;
				}

				children.Add(newChild);
			}

			int numChildren = childTransform.transform.childCount;
			for(int childIndex = 0; childIndex < numChildren; ++childIndex)
			{
				_AddChildren(childTransform.GetChild(childIndex), dirShadowCaster, shadowVec);
			}
		}
	}

	// Recomputes the list of Sectors that this Member is a part of. Also
	// ensures that Sectors are appropriately notified of the changes.
	private void _UpdateSectorMembership()
	{
		if(frozen || isSector)
		{
			return;
		}

		newSectors.Clear();
		leftSectors.Clear();

		if(PortalDetermined && sectors.Count > 0)
		{
			int numSectors = sectors.Count;
			for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
			{
				SECTR_Sector sector = sectors[sectorIndex];
				SECTR_Portal crossedPortal = _CrossedPortal(sector);
				if(crossedPortal)
				{
					SECTR_Sector newSector = crossedPortal.FrontSector == sector ? crossedPortal.BackSector : crossedPortal.FrontSector;
					newSectors.Add(newSector);
					leftSectors.Add(sector);
				}
			}

			numSectors = newSectors.Count;
			for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
			{
				SECTR_Sector newSector = newSectors[sectorIndex];
				newSector.Register(this);
				sectors.Add(newSector);
			}

			numSectors = leftSectors.Count;
			for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
			{
				SECTR_Sector leftSector = leftSectors[sectorIndex];
				leftSector.Deregister(this);
				sectors.Remove(leftSector);
			}
		}
		else if(PortalDetermined && ForceStartSector && !usedStartSector)
		{
			ForceStartSector.Register(this);
			sectors.Add(ForceStartSector);
			newSectors.Add(ForceStartSector);
			usedStartSector = true;
		}
		else
		{
			SECTR_Sector.GetContaining(ref newSectors, TotalBounds);

			int sectorIndex = 0;
			int numSectors = sectors.Count;
			while(sectorIndex < numSectors)
			{
				SECTR_Sector oldSector = sectors[sectorIndex];
				if(newSectors.Contains(oldSector))
				{
					newSectors.Remove(oldSector);
					++sectorIndex;
				}
				else
				{
					oldSector.Deregister(this);
					leftSectors.Add(oldSector);
					sectors.RemoveAt(sectorIndex);
					--numSectors;
				}
			}
			
			numSectors = newSectors.Count;
			if(numSectors > 0)
			{
				for(sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
				{
					SECTR_Sector newSector = newSectors[sectorIndex];
					newSector.Register(this);
					sectors.Add(newSector);
				}
			}
		}

		if(Changed != null && (leftSectors.Count > 0 || newSectors.Count > 0))
		{
			Changed(leftSectors, newSectors);
		}
	}

	private SECTR_Portal _CrossedPortal(SECTR_Sector sector)
	{
		if(sector)
		{
			Vector3 rayDirection = transform.position - lastPosition;
			int numPortals = sector.Portals.Count;
			for(int portalIndex = 0; portalIndex < numPortals; ++portalIndex)
			{
				SECTR_Portal portal = sector.Portals[portalIndex];
				if(portal)
				{
					bool forwardTraversal = (portal.FrontSector == sector);
					Plane portalPlane = forwardTraversal ? portal.HullPlane : portal.ReverseHullPlane;
					SECTR_Sector oppositeSector = forwardTraversal ? portal.BackSector : portal.FrontSector;
					if(oppositeSector &&
					   Vector3.Dot(rayDirection, portalPlane.normal) < 0f && 
					   portalPlane.GetSide(transform.position) != portalPlane.GetSide(lastPosition) &&
					   portal.IsPointInHull(transform.position, rayDirection.magnitude))
					{
						return portal;
					}
				}
			}
		}
		return null;
	}
	#endregion
}
