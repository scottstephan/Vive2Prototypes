using UnityEngine;
using System.Collections;

public class MenuSelector : MonoBehaviour {

	public GameObject selectBall;
	public GameObject selectPlus;
	public GameObject hoverRing;
	Transform _defaultTransform;

//    private Quaternion prevRot = Quaternion.identity;
//    private Quaternion offset = Quaternion.identity;
    GameObject[] _frustumBoundingObjects;

//	bool _hot;
//	float _spinAngle;
    Transform cam;
    Vector3 _target;
    bool _bTargetSet;
    Vector3 curVelocity;

	void Awake() {
//		_hot = false;
		_defaultTransform = this.transform;
        curVelocity = Vector3.zero;
	}

	// Use this for initialization
	void Start () 
    {
        cam = CameraManager.singleton.rightCamera != null ? CameraManager.singleton.rightCamera.transform : CameraManager.singleton.GetComponent<Camera>().transform;
//        prevRot = cam.rotation;
	}
	
	// Update is called once per frame
	void LateUpdate () {
	

        // apply negative rotation offset.
/*        Quaternion cur_rot = cam.rotation;
        Quaternion diff = Quaternion.Inverse( Quaternion.Inverse( prevRot ) * cur_rot );

        Vector3 eulers = MathfExt.RegularEuler( diff.eulerAngles );
        float max = 0.75f;
        if( MathfExt.Approx( eulers, Vector3.zero, 0.15f ) ) {
            eulers = Vector3.zero;
            max = 1.75f;
        }
        eulers *= 20f;
        eulers = MathfExt.UnityEuler( eulers );

        offset = Quaternion.RotateTowards( offset, Quaternion.Euler( eulers ), max );

        _defaultTransform.rotation = _defaultTransform.rotation * offset;

        prevRot = cur_rot;
		/*
		if (_hot) {
			_spinAngle += 150f * Time.deltaTime;

			Vector3 eulers = this.transform.localRotation.eulerAngles;
			eulers += new Vector3(0f, 0f, _spinAngle);
			Quaternion q = Quaternion.identity;
			q.eulerAngles = eulers;

			this.transform.localRotation = q;
		}
		*/

		_defaultTransform.position = Vector3.SmoothDamp(_defaultTransform.position, _target, ref curVelocity, 0.01f );
		
		_defaultTransform.LookAt(cam);
    }


	void OnEnable() {

        _defaultTransform.rotation = Quaternion.identity;
//		_spinAngle = 0f;
        _bTargetSet = false;
		SetHot(false);
	}

	void OnDisable() {

		SetHot (false);
	}

	public void SetHot(bool hot) {

        if (selectBall != null)
        {
    		if (hot == false)
            {
			    selectBall.SetActive(false);
				selectPlus.SetActive(true);
                //this.renderer.material.color = Color.red;
            }
		    else
            {
				selectBall.SetActive(true);
				selectPlus.SetActive(false);
				//this.renderer.material.color = Color.green;
            }
        }

//		_hot = hot;
	}

    public void SetTarget(Vector3 targetPos)
    {
        if( _defaultTransform == null ) {
            return;
        }

        const float MAX_LERP_DIST_SQR = 200f * 200f;

        _target = targetPos;

        if (!_bTargetSet || (targetPos - _defaultTransform.position).sqrMagnitude > MAX_LERP_DIST_SQR)
        {
            _bTargetSet = true;
            _defaultTransform.position = _target;
            return;
        }
    }
}
