using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//TODO: re-enable warnings
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private variable assigned but not used.

public class GesturePathPoint {
	public Vector3 point;
	public Vector3 directionToNext;
}

public class PathGesture : GestureBase {
	
	public AnimationClip thePathGesture;
	
	[HideInInspector] 
	public List<GesturePathPoint> pathControlPoints; // build at Unity Start()
	
	public int numSamples = 2;
	
	// when moving from key to key, how much angular error (off path) is allowed before the trace is a failure.
	public float angularErrorAllowed = 45f;
	private float angularErrorAllowedCos;
	// when finger lifts what percentage distance must be covered, based on pattern length?
	public float minRealEstateCoverage = 40f;

	private int leadingSatisfiedIdx = 0;
	private Vector3 previousTouchPosition = Vector3.zero;
	private bool traceEvalActive = false;
	private bool fullySatisfied = false;
	
	private bool initialized = false;
	
	private GameObject traceObject = null;
	private Mesh pathTraceMesh = null;

	private static float scrollV = 0f;
	private static float scrollVSpeed = 0.15f;
	
	public override void StartTrace ( Action<float> finishedFunc )
	{
//		Debug.Log ("attempting to start trace.");
		base.StartTrace( finishedFunc );
		
		if( !initialized ) {
			return;
		}
		
		fullySatisfied = false;
		traceEvalActive = false;		
		leadingSatisfiedIdx = 0;
		scrollV = 0;
//		Debug.Log ("trace started");
	}
	
	void TraceFailed() {
		// failure case. we satisfied another segment before our first.
//		log.Error ("trace failed");
		base.tpFinishedFunc(0);
		base.traceActive = false;
	}
		
	void EvaluateNewTouchPosition() {
		Vector3 new_pos = Input.mousePosition;
		
		Vector3 new_dir = new_pos - previousTouchPosition;
//		Debug.Log ("[" + new_pos + "][" + previousTouchPosition + "]");
		if( MathfExt.Approx( new_dir, Vector3.zero, minRealEstateCoverage ) ) {
			// touch not moving.
//			Debug.Log ("trace from previous too short");
			return;
		}
		
		new_dir.Normalize();
		
		GesturePathPoint lead = pathControlPoints[leadingSatisfiedIdx];
		// our lead must be valid
		if( Vector3.Dot( new_dir, lead.directionToNext ) < angularErrorAllowedCos ) {
			TraceFailed();
//			Debug.Log("[" + leadingSatisfiedIdx + "] lead failed [" + new_dir + "][" + lead.directionToNext + "]");
			return;
		}
		
		// see how far forward we can move along our sample points.		
		bool done = false;
		while( !done && leadingSatisfiedIdx < numSamples - 2 ) {
			int next_idx = leadingSatisfiedIdx+1;
//			Debug.Log("trying new lead : " + next_idx);
			lead = pathControlPoints[next_idx];
			// move our lead up if our next is valid.
			if( Vector3.Dot( new_dir, lead.directionToNext ) > angularErrorAllowedCos ) {
				leadingSatisfiedIdx = next_idx;
//				Debug.Log("passed");
			}
			else {
				done = true;
			}
		}
		
		if( leadingSatisfiedIdx == numSamples - 2 ) {
//			Debug.Log("fully satisfied");
			fullySatisfied = true;
			return;
		}
		
		previousTouchPosition = new_pos;
	}
	
	public override void Evaluate() 
	{
		if( base.traceActive ) {
			if( Input.touchCount > 0 
#if UNITY_EDITOR
				|| Input.GetMouseButton(0)
#endif				
				) {
				if( !traceEvalActive ) {
					traceEvalActive = true;
//					Debug.Log("starting a new trace attempt");
					previousTouchPosition = Input.mousePosition;
				}
				else {
					EvaluateNewTouchPosition();
				}
			}
			else {
				if( traceEvalActive ) {
//					log.Error("ending a trace attempt");
					base.tpFinishedFunc(0);
					traceEvalActive = false;
					base.traceActive = false;
				}
			}
		}
	}
	
	// Use this for initialization
	void Start () {
		// make sure we are at the origin.
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		
//		Debug.Log("creating a path trace " + gameObject.name);
		if( numSamples < 2 ) {		
			Debug.LogError("["+gameObject.name+"] TRACE PATTERN CANNOT HAVE LESS THAN 2 SAMPLE POINTS.");
			return;
		}
		
		Animation anim = gameObject.GetComponent<Animation>();
		anim.Play( thePathGesture.name );
		anim.Sample(); // BUG: need to call Sample() once up front or AnimationStates in Animation component may reorder during iteration
		anim.Stop(); // call Stop() to ensure that all weights go to 0
		anim.enabled = true; // reenable the animation component to ensure that values will be correct when sampling

		float time_step = thePathGesture.length / ( numSamples - 1 );
		angularErrorAllowedCos = Mathf.Cos( angularErrorAllowed );
		pathControlPoints = new List<GesturePathPoint>();
		float cur_time = 0f;
//		Debug.Log("["+gameObject.name+"] " + numSamples + " : " + thePattern.length + " : " + time_step);
		while( cur_time <= thePathGesture.length ) {
			float nt = cur_time / thePathGesture.length;
			anim[thePathGesture.name].normalizedTime = nt;
			anim.Play(thePathGesture.name);
			anim.Sample();
			anim.Stop(); // call Stop() to ensure that all weights go to 0
			anim.enabled = true; // reenable the animation component to ensure that values will be correct when sampling
			Vector3 new_control_point = transform.position;
//			Debug.Log("adding a sample at " + cur_time + " : " + nt + " :: " + new_control_point);
			// convert into viewport space.
			// TODO>>Screen size into account.
			new_control_point.x *= (1f/1024f);
			new_control_point.y *= (1f/768f);
//			new_control_point *= 2;
//			new_control_point.x -= 1f;
//			new_control_point.y -= 1f;
			
//			Debug.Log("viewpoint " + new_control_point);
			
			GesturePathPoint new_point = new GesturePathPoint();
			new_point.point = new_control_point;			
			pathControlPoints.Add(new_point);
			
			cur_time += time_step;
		}
				
		pathControlPoints[numSamples-1].directionToNext = Vector3.zero;
		
		for( int i = 0; i < (numSamples - 1); i++ ) {
			GesturePathPoint cp = pathControlPoints[i];
			GesturePathPoint np = pathControlPoints[i+1];
			
			cp.directionToNext = (np.point - cp.point).normalized;
//			Debug.Log("["+i+"]["+cp.directionToNext+"]");
		}
	
		initialized = true;
	}
		
	void Update() {
		if( base.traceActive ) {
			traceObject.SetActive( true );
			BuildMesh( 0.4f, 0f );
			// Tom > turn on cirle at the below screen location
//			Vector3 pt = GestureFlowManager.Instance.convertViewpointToScreen(pathControlPoints[0].point);
		}
		else {
			traceObject.SetActive( false );
			// Tom > turn off
		}
	}
	
	void BuildMesh( float width, float z )
	{
		int i = 0;

		if(pathControlPoints.Count > 1)
		{
			Vector3[] newVertices = new Vector3[pathControlPoints.Count * 2];
			Vector3[] newNormals = new Vector3[pathControlPoints.Count * 2];
			Vector2[] newUV = new Vector2[pathControlPoints.Count * 2];
			Color[] newColors = new Color[pathControlPoints.Count * 2];
			int[] newTriangles = new int[(pathControlPoints.Count - 1) * 6];
			float curDistance = 0.00f;
			
			foreach (GesturePathPoint p in pathControlPoints)
			{				
				Vector3 lineDirection = Vector3.zero;
				Vector3 p0 = p.point;
				Vector3 p1;
				if(i == 0) {
					p1 = ((GesturePathPoint)pathControlPoints[i + 1]).point;
					lineDirection = p0 - p1;
				}
				else {
					p1 = ((GesturePathPoint)pathControlPoints[i - 1]).point;
					lineDirection = p1 - p0;
				}
				
				Vector3 perpendicular = Vector3.Cross(lineDirection, Vector3.back).normalized;
					
				newVertices[i * 2] = p0 + (perpendicular * width);
				newVertices[i * 2].z = z;
				newVertices[(i * 2) + 1] = p0 + (-perpendicular * width);
				newVertices[(i * 2) + 1].z = z;
				
				newNormals[i * 2] = newNormals[(i * 2) + 1] = Vector3.up;
				
				newColors[i * 2] = newColors[(i * 2) + 1] = Color.white;
				
				scrollV += scrollVSpeed * Time.deltaTime;
				newUV[i * 2] = new Vector2(0, scrollV + curDistance * 100f);
				newUV[(i * 2) + 1] = new Vector2(1, scrollV + curDistance * 100f);
				
				if( i > 0 )
				{
					curDistance += (p0 - p1).magnitude;
					
					newTriangles[(i - 1) * 6] = (i * 2) - 2;
					newTriangles[((i - 1) * 6) + 1] = (i * 2) - 1;
					newTriangles[((i - 1) * 6) + 2] = i * 2;
					
					newTriangles[((i - 1) * 6) + 3] = (i * 2) + 1;
					newTriangles[((i - 1) * 6) + 4] = i * 2;
					newTriangles[((i - 1) * 6) + 5] = (i * 2) - 1;
				}
				
				i++;
			}
		
			pathTraceMesh.Clear();
            /* No mesh for now
			pathTraceMesh.vertices = newVertices;
			pathTraceMesh.colors = newColors;
			pathTraceMesh.normals = newNormals;
			pathTraceMesh.uv = newUV;
			pathTraceMesh.triangles = newTriangles;
            */
		}
	}
	
}
