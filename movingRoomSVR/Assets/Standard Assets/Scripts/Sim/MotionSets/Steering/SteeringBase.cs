using UnityEngine;
using System.Collections;

public class SteeringBase : MonoBehaviour {
    // class log
    private static Log log = Log.GetLog(typeof(SteeringBase).FullName);
	
	[HideInInspector]
	public CritterInfo critterInfo;

	[HideInInspector]
	public Quaternion desiredRotation;
	
	[HideInInspector]
	public float desiredSteeringThrottle; // 0 to 1 value that defines the range of steering speeds a critter has available.
	
    public virtual void Reset()
    {
    }

	public virtual float GetYawSpeed() {
		log.Error("Derived SteeringBase class needs to implement GetYawSpeed.");
		return 0;
	}
	
	public virtual float GetPitchSpeed() {
		log.Error("Derived SteeringBase class needs to implement GetPitchSpeed.");
		return 0;
	}

	public virtual float GetMaxYawSpeed() {
		log.Error("Derived SteeringBase class needs to implement GetMaxYawSpeed.");
		return 0;
	}
	
	public virtual float GetMaxPitchSpeed() {
		log.Error("Derived SteeringBase class needs to implement GetMaxPitchSpeed.");
		return 0;
	}
	
	public virtual float GetMaxRollSpeed() {
		log.Error("Derived SteeringBase class needs to implement GetMaxRollSpeed.");
		return 0;
	}
	
	// not sure init is needed yet.
	public virtual void Init() {
	}
	
	public virtual void SteerUpdate(float dt) {
	}
}
