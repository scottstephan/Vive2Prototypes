using UnityEngine;
using System.Collections;

public class DynamicPlanarDriftCameraMode : BaseCameraMode {
	private Vector3[] planarDriftLocations;

	private GameObject internalTarget;

	public float useDriftLocationRatio = 1.0f;

	public float xRandomBounds = 200.0f;
	public float yRandomBounds = 200.0f;
	
	public float switchLocationTimeMin = 2.0f;
	public float switchLocationTimeMax = 10.0f;
	public float switchLocationTimeRawBias = 4.0f;
	public float switchLocationTimePower = 1.5f;

	private float currentSmoothTime;
	public float smoothTimeMin = 2.0f;
	public float smoothTimeMax = 15.0f;
	public float smoothTimeRawBias = 5.0f;
	public float smoothTimePower = 1.5f;
	
	private float distanceFromTarget = 175f;
	
	public float speedMax = 60.0f;	
	
	private float timeToSwitchLocation = 10.0f;

	private Vector3 currentVelocity;	
	
//	private Vector3 camForward;
	private Vector3 camRight;
	private Vector3 camUp;
	private Vector3 desiredLocation;
	
	private Vector3 ogPosition;
	
	private Color mainColor = new Color(1F, 0F, 0F, 0.6F);
	private Color otherColor = new Color(0F, 1F, 0F, 0.6F);

	public float maxCameraHeight = -10.0f;

	private void CalculateDriftPositions(bool init_me) {
		Transform[] drift_transforms = gameObject.GetComponentsInChildren<Transform>();
		if( init_me ) {
			planarDriftLocations = new Vector3[drift_transforms.Length];
		}
		int i = 0;
        foreach (Transform drift_transform in drift_transforms) {
			planarDriftLocations[i++] = drift_transform.position;
		}		
		
		timeToSwitchLocation = 0f;
	}

	public override void InitCameraMode() {
        if( inited ) {
            return;
        }
        
        base.InitCameraMode();        

        internalTarget = null;
		
		cameraType = CameraType.DynamicDriftCamera;
		myTransform = gameObject.transform;
		
		CalculateDriftPositions(true);
//		camForward = transform.forward;
		camRight = myTransform.right;
		camUp = myTransform.up;
		ogPosition = myTransform.position;	
		desiredLocation = myTransform.position;
		
		timeToSwitchLocation = 0.4f;
		
		currentVelocity = Vector3.zero;	
		
		cameraName = "Dynamic Drift Camera";
	}
	
	private void NewTarget() {
		internalTarget = CameraManager.currentTarget;
				
		Transform target_transform = internalTarget.transform;
		Vector3 target_position = target_transform.position;
		
		Vector3 dir = CameraManager.GetCurrentCameraPosition() - target_position;
		dir.y = 0f;
		
		float dist = dir.magnitude;
		if( dist <= 0.001f ) {
			dir = Vector3.forward;
			dist = 1f;
		}
		dir *= ( distanceFromTarget / dist );
		RaycastHit hit_info;
		float height = 0f;
		float total_angle = 0f;
		bool done = false;
		Vector3 best_pos = Vector3.zero;
		bool best_set = false;
		while( !done ) {
			Vector3 check_pos = target_position + dir;
			Vector3 top_pos = check_pos;
			top_pos.y = -10.1f;
			height = -1f * check_pos.y;
			Vector3 new_pos = check_pos;
			bool no_hit = true;
			if( Physics.SphereCast(top_pos,10.0f,Vector3.down,out hit_info, height, 1<<14) ) {
				new_pos = top_pos + Vector3.down * hit_info.distance;
				no_hit = false;
			}
			
			Vector3 new_dir = target_position - new_pos;
			dist = new_dir.magnitude;
			bool ok = true;
			if( Physics.SphereCast(new_pos,5f,new_dir,out hit_info, dist,1<<15 ) ) { 	// cast against items
				if( hit_info.collider.gameObject != internalTarget ) {
					ok = false;
				}
				no_hit = false;
			}
			if( ok || no_hit || !best_set ) {
				best_pos = new_pos;
				best_set = true;
			}

			if( no_hit ) {
				done = true;
			}
			
			MathfExt.YawVector(dir,15f * Mathf.Deg2Rad);
			total_angle += 15f;
			if( total_angle >= 360f ) {
				done = true;
			}
			
		}
		
		myTransform.position = best_pos;
		myTransform.LookAt(target_position);
			
		CalculateDriftPositions(true);
	}

	public override void StartCameraMode ()
	{
		if( internalTarget != CameraManager.currentTarget ) {
			NewTarget();
		}
	}
	
    public override void UpdateCameraMode() {
		runCollision = CameraCollisionType.None;

		if( internalTarget != CameraManager.currentTarget ) {
			NewTarget();
		}
		
		float dt = Time.deltaTime;
		timeToSwitchLocation -= dt;
		
		if( timeToSwitchLocation <= 0.0f ) {
			float tmp = Random.value;
			if( planarDriftLocations != null && planarDriftLocations.Length > 0 && tmp < useDriftLocationRatio ) {
				int new_idx = Random.Range(0, planarDriftLocations.Length);
			    desiredLocation = planarDriftLocations[new_idx];
			}
			else {
				float x_offset = RandomExt.FloatRange(-xRandomBounds,xRandomBounds);
				float y_offset = RandomExt.FloatRange(-yRandomBounds,yRandomBounds);
				desiredLocation = ogPosition + ( camUp * y_offset ) + ( camRight * x_offset );			
			}

			currentSmoothTime = RandomExt.FloatWithBiasPower(smoothTimeMin, smoothTimeMax, smoothTimeRawBias, smoothTimePower);
			timeToSwitchLocation = RandomExt.FloatWithBiasPower(switchLocationTimeMin, switchLocationTimeMax, switchLocationTimeRawBias, switchLocationTimePower);
		}
		
		myTransform.position = Vector3.SmoothDamp( myTransform.position, desiredLocation, ref currentVelocity, currentSmoothTime, speedMax );
		
		if (myTransform.position.y > maxCameraHeight ) {
			myTransform.position = new Vector3(myTransform.position.x,maxCameraHeight,myTransform.position.z);
		}

	}

	void OnDrawGizmos() {
		if( !drawGizmos ) { 
			return;
		}
		
		Transform[] drift_transforms = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform drift_transform in drift_transforms) {
			Color use_color = otherColor;
			float use_radius = 3.4f;
			if( drift_transform.position == transform.position ) {
				use_color = mainColor;
				use_radius = 6.8f;
			}
			
			Gizmos.color = use_color;
			Gizmos.DrawSphere(drift_transform.position, use_radius);	
			Gizmos.DrawLine(drift_transform.position, drift_transform.position + (transform.forward * 100.0f));// TODO>pivot
		}
	}
}