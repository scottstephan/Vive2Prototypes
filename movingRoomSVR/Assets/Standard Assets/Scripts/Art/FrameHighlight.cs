using UnityEngine;
using System.Collections;

public class FrameHighlight : MonoBehaviour {
	public Transform target;
	public Transform[] corners;
	public Material material;
	public bool isFading;

    public Transform icon;
    public float iconAlphaMult = 1.25f;
    Vector3 iconLocalScale;
    Material iconMaterial;

	public float tinySize = 0.6f;
	public float smallSize = 1f;
	public float mediumSize = 1.5f;
	public float largeSize = 2f;
	public float HugeSize = 3f;

    public float iconTinySize = 1f;
    public float iconSmallSize = 1.5f;
    public float iconMediumSize = 2f;
    public float iconLargeSize = 3f;
    public float iconHugeSize = 5f;

    public float fadeOutTime = 1f;
    public float fadeInTime = 1f;

    [HideInInspector]
    public float maxAlpha = 0.25f;

    [HideInInspector]
    public WemoItemData nextTarget;
    [HideInInspector]
    public WemoItemData currentTarget;

	private bool isFlashing;
	private bool isClearing;
	private float fadeDir;
	private float alpha;  
	private Transform myXform;
	private Vector3 size;

	void Start () {
		myXform = transform;

        if (icon != null)
        {
            iconLocalScale = icon.localScale;

            if (icon.GetComponent<Renderer>() != null)
            {
                iconMaterial = icon.GetComponent<Renderer>().material;
            }
        }
	}

	public void Clear(){
		nextTarget = null;
		isClearing = true;
		isFlashing = false;
        Fade(-fadeOutTime);
	}

	private void ClearNow(){
		isClearing = false;
        currentTarget = null;
        if( corners == null ) {
            return;
        }

        for(int i = 0; i < corners.Length; i++ ) {
            Transform c = corners[i];
            if( c == null ) {
                continue;
            }
			c.transform.parent = myXform;
			c.transform.position = Vector3.up * 1000f;
		}
	}

    float GetSizeMult(SpeciesSize s)
    {
        switch(s)
        {
            case SpeciesSize.TINY: return tinySize;
            case SpeciesSize.SMALL: return smallSize;
            case SpeciesSize.MEDIUM: return mediumSize;
            case SpeciesSize.LARGE: return largeSize;
            case SpeciesSize.HUGE: return HugeSize;
        }

        return 1f;
    }

    float GetIconSizeMult(SpeciesSize s)
    {
        switch(s)
        {
        case SpeciesSize.TINY: return iconTinySize;
        case SpeciesSize.SMALL: return iconSmallSize;
        case SpeciesSize.MEDIUM: return iconMediumSize;
        case SpeciesSize.LARGE: return iconLargeSize;
        case SpeciesSize.HUGE: return iconHugeSize;
        }
        
        return 1f;
    }

    private void Refresh()
    {
        float sizeMult = 1f;
        float iconSizeMult = 1f;

        if (nextTarget.critterInfo != null &&
            nextTarget.critterInfo.generalSpeciesData != null)
        {
            sizeMult = GetSizeMult(nextTarget.critterInfo.generalSpeciesData.mySize);
            iconSizeMult = GetIconSizeMult(nextTarget.critterInfo.generalSpeciesData.mySize);
        }

        for(int i = 0; i < corners.Length; i++ ) {
            Transform c = corners[i];
            if( c == null ) {
                continue;
            }
            c.transform.parent = target;
			c.transform.localRotation = Quaternion.identity;
            c.transform.localScale = Vector3.one * sizeMult;
		}
        Vector3 targetLocalScale = target.localScale;
        Vector3 newLocalScale = iconLocalScale * iconSizeMult;
        newLocalScale.x *= targetLocalScale.x;
        newLocalScale.y *= targetLocalScale.y;
        newLocalScale.z *= targetLocalScale.z;
        icon.localScale = newLocalScale;

		corners[0].localPosition = new Vector3(size.x, size.y, size.z) * 0.5f;
		corners[1].localPosition = new Vector3(-size.x, size.y, size.z) * 0.5f;
		corners[2].localPosition = new Vector3(size.x, -size.y, size.z) * 0.5f;
		corners[3].localPosition = new Vector3(-size.x, -size.y, size.z) * 0.5f;
		corners[4].localPosition = new Vector3(size.x, size.y, -size.z) * 0.5f;
		corners[5].localPosition = new Vector3(-size.x, size.y, -size.z) * 0.5f;
		corners[6].localPosition = new Vector3(size.x, -size.y, -size.z) * 0.5f;
		corners[7].localPosition = new Vector3(-size.x, -size.y, -size.z) * 0.5f;

	}

	public void SwitchTarget(WemoItemData pickedItem){
		nextTarget = pickedItem;
		isFlashing = true;
		isClearing = false;
        Fade (-fadeOutTime);
	}

	private void SwitchTargetNow(){
		target = nextTarget.transform;
        currentTarget = nextTarget;

        // hover target can be the train station, which for som ereason has a critter info? But no general motion data
        if (nextTarget.critterInfo != null && 
            nextTarget.critterInfo.generalMotionData != null)
        {
		    size = nextTarget.critterInfo.generalMotionData.critterBoxColliderSize;
        }
        else if (nextTarget.gameObject.GetComponent<Collider>() != null)
        {
            size = nextTarget.gameObject.GetComponent<Collider>().bounds.size;
        }

		Refresh();
		isFlashing = false;
        Fade(fadeInTime);
	}

	
	private void Fade(float dir){
        alpha = Mathf.Clamp(alpha,0f, maxAlpha);
		isFading = true;
		fadeDir = dir;
	}

	void Update () {
		if(isFading){
			alpha += Time.deltaTime * fadeDir;

            material.color = new Color(1f, 1f, 1f, alpha);

            if (iconMaterial != null)
            {
                iconMaterial.color = new Color(1f, 1f, 1f, alpha*iconAlphaMult);
            }

			if(alpha < 0f){
				isFading = false;
				if(isFlashing)
					SwitchTargetNow();
				if(isClearing)
					ClearNow();
			}
            if( alpha > maxAlpha ){
				isFading = false;
			}
		}
	}

    void LateUpdate() {
        // update our rotation .. doing this late so that we have the latest camera orientations applied.
        if( icon != null && currentTarget != null) 
        {
            Transform critterTransform = currentTarget.critterInfo.critterTransform;
            if (critterTransform != null)
            {
                Transform cam_trans = CameraManager.GetCurrentCameraTransform();
                // since we are not using billboards anymore . the rotation should be the fish rotation for the particle system to work
                Vector3 critterScale = critterTransform.localScale;
                icon.rotation = cam_trans.rotation ;//cam_trans.rotation;//Quaternion.LookRotation( cam_trans.forward, cam_trans.up );
                icon.position = critterTransform.position +
                                critterTransform.forward * (currentTarget.vrIconOffset.z*critterScale.z) +
                                critterTransform.right * (currentTarget.vrIconOffset.x*critterScale.x) +
                                Vector3.up * (currentTarget.vrIconOffset.y * critterScale.y);                
            }
        }
    }
}


/*
		if(target != null){
			myXform.position = target.position;
			//myXform.rotation = Quaternion.LookRotation(CameraManager.GetCurrentCameraPosition() - myXform.position);
		}
		if(target != null){
			//Bounds b = targetCollider.bounds;
			//Debug.Log(target.position + " " + b.min);
			//Debug.Log(CameraManager.singleton.transform.position);
			// this doesn't work, since we need to test all 8 corners of the bounding box
			//Vector3 min = target.TransformPoint(size * -0.5f);
			//Vector3 max = target.TransformPoint(size * 0.5f);
			//Vector3 wsMin = new Vector3(Mathf.Min(min.x,max.x),Mathf.Min(min.y,max.y),Mathf.Min(min.z,max.z));
			//Vector3 wsMax = new Vector3(Mathf.Max(min.x,max.x),Mathf.Max(min.y,max.y),Mathf.Max(min.z,max.z));
			//min = CameraManager.singleton.leftCamera.WorldToScreenPoint(wsMin);
			//max = CameraManager.singleton.leftCamera.WorldToScreenPoint(wsMax);	
			//Vector3 ssMin = new Vector3(Mathf.Min(min.x,max.x),Mathf.Min(min.y,max.y),Mathf.Min(min.z,max.z));
			//Vector3 ssMax = new Vector3(Mathf.Max(min.x,max.x),Mathf.Max(min.y,max.y),Mathf.Max(min.z,max.z));
			//tl.position = CameraManager.singleton.leftCamera.ScreenToWorldPoint(new Vector3(ssMin.x, ssMax.y, 20f));
			//tr.position = CameraManager.singleton.leftCamera.ScreenToWorldPoint(new Vector3(ssMax.x, ssMax.y, 20f));
			//bl.position = CameraManager.singleton.leftCamera.ScreenToWorldPoint(new Vector3(ssMin.x, ssMin.y, 20f));
			//br.position = CameraManager.singleton.leftCamera.ScreenToWorldPoint(new Vector3(ssMax.x, ssMin.y, 20f));
			/*  UI solution
			Vector3[] points = new Vector3[8];
			points[0] = target.TransformPoint(new Vector3(size.x, size.y, size.z) * 0.5f);
			points[1] = target.TransformPoint(new Vector3(-size.x, size.y, size.z) * 0.5f);
			points[2] = target.TransformPoint(new Vector3(size.x, -size.y, size.z) * 0.5f);
			points[3] = target.TransformPoint(new Vector3(-size.x, -size.y, size.z) * 0.5f);
			points[4] = target.TransformPoint(new Vector3(size.x, size.y, -size.z) * 0.5f);
			points[5] = target.TransformPoint(new Vector3(-size.x, size.y, -size.z) * 0.5f);
			points[6] = target.TransformPoint(new Vector3(size.x, -size.y, -size.z) * 0.5f);
			points[7] = target.TransformPoint(new Vector3(-size.x, -size.y, -size.z) * 0.5f);
			Vector3[] ssPoints = new Vector3[8];
			ssPoints[0] = CameraManager.singleton.leftCamera.WorldToScreenPoint(points[0]);
			ssPoints[1] = CameraManager.singleton.leftCamera.WorldToScreenPoint(points[1]);
			ssPoints[2] = CameraManager.singleton.leftCamera.WorldToScreenPoint(points[2]);
			ssPoints[3] = CameraManager.singleton.leftCamera.WorldToScreenPoint(points[3]);
			ssPoints[4] = CameraManager.singleton.leftCamera.WorldToScreenPoint(points[4]);
			ssPoints[5] = CameraManager.singleton.leftCamera.WorldToScreenPoint(points[5]);
			ssPoints[6] = CameraManager.singleton.leftCamera.WorldToScreenPoint(points[6]);
			ssPoints[7] = CameraManager.singleton.leftCamera.WorldToScreenPoint(points[7]);

			float minX, minY, minZ, maxX, maxY, maxZ ;
			minX = minY = minZ = 999999f;
			maxX = maxY = maxZ = -999999f;

			int i = 0;
			foreach(Vector3 ssp in ssPoints){
				if(debug)
					corners[i].position = points[i];
				minX = Mathf.Min(minX, ssp.x);
				minY = Mathf.Min(minY, ssp.y);
				minZ = Mathf.Min(minZ, ssp.z);
				maxX = Mathf.Max(maxX, ssp.x);
				maxY = Mathf.Max(maxY, ssp.y);
				maxZ = Mathf.Max(maxZ, ssp.z);
				i++;
			}

			tl.position = CameraManager.singleton.leftCamera.ScreenToWorldPoint(new Vector3(minX, maxY, 20f));
			tr.position = CameraManager.singleton.leftCamera.ScreenToWorldPoint(new Vector3(maxX, maxY, 20f));
			bl.position = CameraManager.singleton.leftCamera.ScreenToWorldPoint(new Vector3(minX, minY, 20f));
			br.position = CameraManager.singleton.leftCamera.ScreenToWorldPoint(new Vector3(maxX, minY, 20f));

			tl.rotation = Quaternion.LookRotation(CameraManager.GetCurrentCameraPosition() - tl.position);
			tr.rotation = Quaternion.LookRotation(CameraManager.GetCurrentCameraPosition() - tr.position);
			bl.rotation = Quaternion.LookRotation(CameraManager.GetCurrentCameraPosition() - bl.position);
			br.rotation = Quaternion.LookRotation(CameraManager.GetCurrentCameraPosition() - br.position);

		}
			*/
