using UnityEngine;
using System.Collections;

// camera follows a path of gameobject positions and orientations.
// the code does not handle the case of 2 or more positions being so close that they
// are moved across in a single frame. this is would be a bad camera placement anyways.
public class RevealCameraMode : BaseCameraMode {
	
	enum RevealMotionState {
		AtStartPoint,
		MovingTowardsReveal,
		AtReveal,
		OkToEnd,
	};
	
	public GameObject[] pathPoints; // pathpoints must be children
	private Vector3[] pathPointPositions;
	private Quaternion[] pathPointRotations;
	private float[] pathPointDistances;
	
	private RevealMotionState revealMotionState;
	public BaseCameraMode startCamera;
	public BaseCameraMode endCamera;

	private float curMotionStateTimer;
	private float revealedTimer;
	private bool useMotionStateTimer;
	
	public float atStartTime = 1.5f;

	public float revealDistance = 1400.0f;
	public float postRevealDistance = -1350.0f;
	public float revealTime = 0.5f;
	public float revealedTime = 12.0f;
	public float closeRevealTime = 2.5f;
	
	public float atRevealSpeedTime = 10.0f;
	public float camAccel = 150.0f;
	public float preRevealSpeed = 70.0f;
	public float revealSpeed = 30.0f;
	public float postRevealSpeed = 130.0f;
	private float curSpeed = 0.0f;
	private float desiredSpeed = 0.0f;
	
	public int revealIndex = -1;
	
	private int onPathIdx = 0;
	private int nextPathIdx = 0;
	private int maxPathIdx;
	
	private Color startColor = new Color(0F, 1F, 0F, 0.6F);
	private Color endColor = new Color(0F, 0F, 1F, 0.6F);
	private Color pointColor = new Color(0.8F, 0.8F, 0.8F, 0.6F);
	private Color slowColor = new Color(1F, 0F, 0F, 0.6F);


	void SetPathIdx( int idx ) {
		onPathIdx = idx;
		nextPathIdx = onPathIdx + 1;
		if( nextPathIdx > maxPathIdx ) {
			nextPathIdx = maxPathIdx;
		}
	}
	
	// Use this for initialization
	public override void InitCameraMode() {
        if( inited ) {
            return;
        }
        
        base.InitCameraMode();        
        
		runCollision = CameraCollisionType.None;
		
		blockSwitch = true;
		
		cameraType = CameraType.RevealCamera;
		
		myTransform = transform;
		
		// setup the reveal to trigger when this camera unlocks
		endCamera.cameraReveal = this;
		revealMotionState = RevealMotionState.AtStartPoint;
		curMotionStateTimer = atStartTime;
		useMotionStateTimer = true;
		revealedTimer = -1.0f;
		
		pathPointPositions = new Vector3[pathPoints.Length + 2];
		pathPointRotations = new Quaternion[pathPoints.Length + 2];
		pathPointDistances = new float[pathPoints.Length + 2];
		pathPointPositions[0] = startCamera.transform.position;
		pathPointRotations[0] = startCamera.transform.rotation;
		for( int i = 0; i < pathPoints.Length; i++ ) {
			pathPointPositions[i+1] = pathPoints[i].transform.position;
			pathPointRotations[i+1] = pathPoints[i].transform.rotation;
		}
		pathPointPositions[pathPoints.Length + 1] = endCamera.transform.position;
		pathPointRotations[pathPoints.Length + 1] = endCamera.transform.rotation;
		maxPathIdx = pathPointPositions.Length - 1;

		for( int i = 0; i < pathPointPositions.Length; i++ ) {
			int next = i + 1;
			if( next > maxPathIdx ) {
				pathPointDistances[i] = 0f;
			}
			else {
				Vector3 tmp = pathPointPositions[i] - pathPointPositions[next];
				pathPointDistances[i] = tmp.magnitude;
			}
		}
		
		SetPathIdx( 0 );
		myTransform.position = pathPointPositions[0];		
		myTransform.rotation = pathPointRotations[0];		
		
		cameraName = "Reveal Camera";
	}
	
    public override void StartCameraMode() {
		blockSwitch = true;
	}
	
	// Update is called once per frame
    public override void UpdateCameraMode() {
		float dt = Time.deltaTime;
		
		curMotionStateTimer -= dt;
		if( useMotionStateTimer && curMotionStateTimer <= 0f ) {
			switch( revealMotionState ) {
				case RevealMotionState.AtStartPoint: {
//					WemoLog.Scott("movetowards");
					desiredSpeed = preRevealSpeed;
					useMotionStateTimer = false;
					revealMotionState = RevealMotionState.MovingTowardsReveal;
					break;
				}
				case RevealMotionState.MovingTowardsReveal: {
					break;
				}
				case RevealMotionState.AtReveal: {
//					WemoLog.Scott("DONE WITH REVEAL SPEED");
					desiredSpeed = postRevealSpeed;
					useMotionStateTimer = false;
					revealMotionState = RevealMotionState.OkToEnd;
					break;
				}
				case RevealMotionState.OkToEnd: {
					break;
				}
			}
		}
		
		if( revealedTimer > 0.0 ) {
			revealedTimer -= dt;
			if( revealedTimer <= 0f ) {
				GlobalOceanShaderAdjust.ExpandSphere(postRevealDistance,closeRevealTime);
			}
		}

		// adjust our speed.
		if( curSpeed != desiredSpeed ) {
			float acc = ( camAccel * dt );
			if( curSpeed > desiredSpeed ) {
				curSpeed -=  acc;
				if( curSpeed < desiredSpeed ) {
					curSpeed = desiredSpeed;
				}
			}
			else { // if( curSpeed < desiredSpeed ) {
				curSpeed +=  acc;
				if( curSpeed > desiredSpeed ) {
					curSpeed = desiredSpeed;
				}
			}
		}
		
		if( revealedTimer < 0f 
			&& onPathIdx == maxPathIdx 
			&& revealMotionState == RevealMotionState.OkToEnd ) {
//			WemoLog.Scott("END");
			blockSwitch = false;
			CameraManager.JumpToCameraMasterIndex( endCamera.masterIndex );
		}
		
		if( curSpeed > 0f ) {
			float move_delta = curSpeed * dt;
	
			// whats our distance to our next point..				
			Vector3 cur_pos = myTransform.position;
			Vector3 cur = cur_pos - pathPointPositions[nextPathIdx];
			float dist = cur.magnitude;
//			WemoLog.Scott("disttonext " + dist + " : move " + move_delta);
			if( dist <= move_delta ) {
				
				if( revealMotionState == RevealMotionState.MovingTowardsReveal 
					&& nextPathIdx  == revealIndex ) {
//					WemoLog.Scott("AT REVEAL");
					desiredSpeed = revealSpeed;
					GlobalOceanShaderAdjust.ExpandSphere(revealDistance,revealTime);
					curMotionStateTimer = atRevealSpeedTime;
					revealedTimer = revealedTime;
					useMotionStateTimer = true;
					revealMotionState = RevealMotionState.AtReveal;
				}
								
				SetPathIdx( nextPathIdx );
				
				// we've reached teh end.. now we wait..
				if( onPathIdx == maxPathIdx ) {
//					WemoLog.Scott("READY TO END");
					curSpeed = 0.0f;
					desiredSpeed = 0.0f;
					myTransform.position = pathPointPositions[onPathIdx];
					myTransform.rotation = pathPointRotations[onPathIdx];
					return;
				}
				
				cur_pos = pathPointPositions[onPathIdx];
				move_delta -= dist;
			}
			myTransform.position = Vector3.MoveTowards(cur_pos, pathPointPositions[nextPathIdx], move_delta);
	
			Vector3 tmp = myTransform.position - pathPointPositions[onPathIdx];
			
			float ratio = tmp.magnitude / pathPointDistances[onPathIdx];
	//		WemoLog.Scott("i " + onPathIdx + " : ratio " + ratio);
			
			myTransform.rotation = Quaternion.Slerp(pathPointRotations[onPathIdx],pathPointRotations[nextPathIdx],ratio);
		}
	}
	
	
	void OnDrawGizmos() {
		if( !drawGizmos ) { 
			return;
		}
		
		if( startCamera != null ) {
			Vector3 pos = startCamera.transform.position;
			Vector3 forward = startCamera.transform.forward;
			Gizmos.color = startColor;
			Gizmos.DrawSphere(pos, 6.8f);	
			Gizmos.DrawLine(pos, pos + (forward * 100.0f));
		}
		
		if( endCamera != null ) {
			Vector3 pos = endCamera.transform.position;
			Vector3 forward = endCamera.transform.forward;
			Gizmos.color = endColor;
			Gizmos.DrawSphere(pos, 6.8f);	
			Gizmos.DrawLine(pos, pos + (forward * 100.0f));
		}
		
		if( pathPoints != null 
			&& pathPoints.Length > 0 ) {
	        for( int i = 0; i < pathPoints.Length; i++ ) {
				GameObject pathpoint = pathPoints[i];
				if( pathpoint != null ) {
					Vector3 pos = pathpoint.transform.position;
					Color use_color = pointColor;
					float use_radius = 3.4f;
					if( i == revealIndex - 1 ) { // take the start into account
						use_color = slowColor;
						use_radius = 6.8f;
					}
					
					Gizmos.color = use_color;
					Gizmos.DrawSphere(pos, use_radius);	
					Gizmos.DrawLine(pos, pos + (pathpoint.transform.forward * 100.0f));
				}
			}
		}
	}

}
