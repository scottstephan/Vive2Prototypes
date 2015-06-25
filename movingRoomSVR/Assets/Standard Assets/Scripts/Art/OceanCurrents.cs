using UnityEngine;
using System.Collections;

public class OceanCurrents : MonoBehaviour {
    private static OceanCurrents singleton = null;
    public static OceanCurrents Singleton {
        get {
            return singleton;
        }
    }
    public static void Reset() {
        singleton = null;
    }
    
    public float frequencySpeed = 0.0025f;
    public float frequencyMin = 0.3f;
    public float frequencyMax = 0.5f;
    public float startingFrequency = 0.3f;
    public float startingFrequencyDir = 1; // 1 or -1
    public float currentFrequency;
    private float currentFrequencyDir;
 
    private float myTime = 0f;
    
    public float speedAccel = 0.001f;
    public float speedMin = 0.7f;
    public float speedMax = 0.9f;
    public float startingSpeed = 0.7f;
    public float startingSpeedDir = 1; // 1 or -1
    public float currentSpeed;
    private float currentSpeedDir;
    
	private Vector3 direction;    
    
    public float minForwardMotion = 0.1f;

    [HideInInspector]
    public Vector3 currentDirectionForward;    

    [HideInInspector]
    public Vector3 currentDirection;
    [HideInInspector]
    public Vector3 simulatedCurrentDirection; // always reflect underlying sim so scripted currents can blend effectively

    [HideInInspector]
    public bool UseOverride;
    [HideInInspector]
    public Vector3 OverrideCurrentDirection;

    void Awake()
    {
        singleton = this;
    }

    // Use this for initialization
	void Start () {
        myTime = 0f;
        currentFrequency = Mathf.Clamp(startingFrequency, frequencyMin, frequencyMax);
        currentFrequencyDir = startingFrequencyDir > 0f ? 1f : -1f;
        currentSpeed = Mathf.Clamp(startingSpeed, speedMin, speedMax);
        currentSpeedDir = startingSpeedDir > 0f ? 1f : -1f;
        
		direction = gameObject.transform.forward;
        direction.y = 0;
        direction.Normalize();
	}
	
	// Update is called once per frame
	void Update () {
        float dt = Time.deltaTime;
        currentFrequency += ( frequencySpeed * dt * currentFrequencyDir );
        if( currentFrequency < frequencyMin ) {
            currentFrequency = frequencyMin;
            currentFrequencyDir = 1f;
        }
        else if( currentFrequency > frequencyMax ) {
            currentFrequency = frequencyMax;
            currentFrequencyDir = -1;
        }
            
        currentSpeed += ( speedAccel * dt * currentSpeedDir );
        if( currentSpeed < speedMin ) {
            currentSpeed = speedMin;
            currentSpeedDir = 1f;
        }
        else if( currentSpeed > speedMax ) {
            currentSpeed = speedMax;
            currentSpeedDir = -1;
        }
            
        
        // whats our current waveform
        myTime += ( dt * currentFrequency );
        float cur_wave = Mathf.Sin( myTime );
//        Debug.Log("cur_wave " + cur_wave + " :: frequency " + currentFrequency);
        // apply our primary wave direction (used by critters and kelp)

        // always reflect underlying sim so scripted currents can blend effectively
        simulatedCurrentDirection = currentDirection = direction * (cur_wave * currentSpeed);

        if (UseOverride)
            currentDirection = OverrideCurrentDirection;
        else
            currentDirection = simulatedCurrentDirection;

        // BV: vertex shaders respond immediately and don't have "weight" like other objects do - scripted force looks artificial so just continue using simulated direction.
		// Vector4 oceanCurrents = new Vector4(currentDirection.x, 0f, currentDirection.z, 1.0f);
        Vector4 oceanCurrents = new Vector4(simulatedCurrentDirection.x, 0f, simulatedCurrentDirection.z, 1.0f);
        Shader.SetGlobalVector("_OceanCurrents", oceanCurrents);
		//Debug.Log(oceanCurrents);
        
        // apply our secondary wave direction (used by ocean surface)
        float forward_motion_range = 1.0f - minForwardMotion;        
        cur_wave += 1f;
        cur_wave *= 0.5f;
        cur_wave = minForwardMotion + ( forward_motion_range * cur_wave );
        
        currentDirectionForward = direction * ( cur_wave * currentSpeed );   
		
        Vector4 oceanCurrentsForward = new Vector4(currentDirectionForward.x+=(Time.time/10+currentDirectionForward.x), 0f, currentDirectionForward.z+=(Time.time/10+currentDirectionForward.x), 1.0f);
		//Debug.Log(oceanCurrentsForward);
		
        Shader.SetGlobalVector("_OceanCurrentsForward",oceanCurrentsForward);
    }
    
    public void UpdateCritter( CritterInfo critter_info, ref Vector3 new_position ) {
        GeneralMotionData gmd = critter_info.generalMotionData;
        
        float max_depth = -500f;
        float y = new_position.y;
        if( y < max_depth ) y = max_depth;  
        float depth_factor = 1.0f - ( y / max_depth );
        float force = depth_factor * (1f/(gmd.critterBoxColliderVolume * 0.01f)) * gmd.oceanCurrentMassFactor;
                
        // limit the force to something reasonable based on swim speed.
        float max = critter_info.generalMotionData.swimSpeed * 1.5f;
        if( force > max ) {
            force = max;
        }
        
        force *= Time.deltaTime;
//        Debug.Log(force + " " + depth_factor);
        new_position += currentDirection * force;
    }
}
