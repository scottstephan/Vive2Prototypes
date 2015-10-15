using UnityEngine;
using System.Collections;

public class lineNav : MonoBehaviour {
	public static Vector3 fwdViewPt;

	public GameObject testNavObject;
	public Vector3 centerPoint;

	public Vector3 viewFwdLength; 
	public static float navRadius = 15; //1/2 the len
	public int numNavObjects;

	// Use this for initialization
	void Start () {
		fwdViewPt = centerPoint + viewFwdLength; //Gets us the z loc of the fwd spawn

	}
	
	// Update is called once per frame
	void Update () {
		Debug.DrawLine (centerPoint, fwdViewPt);
		Debug.DrawLine (fwdViewPt - new Vector3 (navRadius, 0, 0), fwdViewPt + new Vector3 (navRadius, 0, 0));

		if (Input.GetKeyDown (KeyCode.Space)) {
			Instantiate(testNavObject,getObjectLinePos(), testNavObject.transform.localRotation);
		}
	}

	private Vector3 getObjectLinePos(){
		Vector3 objPos = fwdViewPt;
		objPos.x = Random.Range (fwdViewPt.x - navRadius, fwdViewPt.x + navRadius);
		return objPos;
	}


}
