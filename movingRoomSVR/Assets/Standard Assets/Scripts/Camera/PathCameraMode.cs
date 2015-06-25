using UnityEngine;
using System.Collections;

// camera follows a path of gameobject positions and orientations.
// the code does not handle the case of 2 or more positions being so close that they
// are moved across in a single frame. this is would be a bad camera placement anyways.
public class PathCameraMode : BaseCameraMode {
	public GameObject[] pathPoints; // pathpoints must be children
	private Transform[] pathPointsTransforms;
	private float[] pathPointDistances;
	
	public int startingPathIdx = 0;
	public float speed = 100.0f;
	
	private bool movedForward = false;
	private bool movedBackward = false;

	private int onPathIdx = 0;
	private int nextPathIdx = 0;
	private int prevPathIdx = 0;
	private int maxPathIdx;
	
	void SetPathIdx( int idx ) {
		onPathIdx = idx;
		nextPathIdx = onPathIdx + 1;
		if( nextPathIdx > maxPathIdx ) {
			nextPathIdx = 0;
		}
		prevPathIdx = onPathIdx - 1;
		if( prevPathIdx < 0 ) {
			prevPathIdx = maxPathIdx;
		}		
	}
	
	// Use this for initialization
	public override void InitCameraMode() {
        if( inited ) {
            return;
        }
        
        base.InitCameraMode();        
        
		cameraType = CameraType.PathCamera;

		myTransform = gameObject.transform;

		maxPathIdx = pathPoints.Length - 1;

		pathPointsTransforms = new Transform[pathPoints.Length];
		pathPointDistances = new float[pathPoints.Length];
		for( int i = 0; i < pathPoints.Length; i++ ) {
			pathPointsTransforms[i] = pathPoints[i].transform;
		}
		
		for( int i = 0; i < pathPoints.Length; i++ ) {
			int next = i + 1;
			if( next > maxPathIdx ) {
				next = 0;
			}
			Vector3 tmp = pathPointsTransforms[i].position - pathPointsTransforms[next].position;
			pathPointDistances[i] = tmp.magnitude;
		}
		
		SetPathIdx( startingPathIdx );
		myTransform.position = pathPointsTransforms[startingPathIdx].position;		
		myTransform.rotation = pathPointsTransforms[startingPathIdx].rotation;		
		
		cameraName = "Path Camera";
	
	}
	
	// Update is called once per frame
    public override void UpdateCameraMode() {		
		bool update_rotation = false;
		
		float move_dir = Input.GetAxis("Forward_Back");		
		if( move_dir > 0.0f ) {
			movedForward = true;
		}
		else if( move_dir < 0.0f ) {
			movedBackward = true;
		}
		
		if( movedForward ) {
			float move_delta = speed * Time.deltaTime;

			// whats our distance to our next point..				
			Vector3 cur_pos = myTransform.position;
			Vector3 cur = cur_pos - pathPointsTransforms[nextPathIdx].position;
			float dist = cur.magnitude;
			if( dist <= move_delta ) {
				SetPathIdx( nextPathIdx );
				
				cur_pos = pathPointsTransforms[onPathIdx].position;
				move_delta -= dist;
			}
			myTransform.position = Vector3.MoveTowards(cur_pos, pathPointsTransforms[nextPathIdx].position, move_delta);
			update_rotation = true;
		}
		
		if( movedBackward ) {
			float move_delta = speed * Time.deltaTime;
			
			// whats our distance to our current point.. we are moving backwards on the path
			Vector3 cur_pos = myTransform.position;
			Vector3 cur = cur_pos - pathPointsTransforms[onPathIdx].position;
			float dist = cur.magnitude;
			if( dist <= move_delta ) {
				cur_pos = pathPointsTransforms[onPathIdx].position;

				SetPathIdx( prevPathIdx );
				move_delta -= dist;
			}
			myTransform.position = Vector3.MoveTowards(cur_pos, pathPointsTransforms[onPathIdx].position, move_delta);
			update_rotation = true;
		}
		
		if( update_rotation ) {
			Vector3 tmp = myTransform.position - pathPointsTransforms[onPathIdx].position;
			
			float ratio = tmp.magnitude / pathPointDistances[onPathIdx];
			
			myTransform.rotation = Quaternion.Slerp(pathPointsTransforms[onPathIdx].rotation,pathPointsTransforms[nextPathIdx].rotation,ratio);
		}
		
		movedForward = false;	
		movedBackward = false;	
	}
		
	public void MoveForwardOnPath() {
		movedForward = true;
	}

	public void MoveBackwardOnPath() {
		movedBackward = true;
	}
}
