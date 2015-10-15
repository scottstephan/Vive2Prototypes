using UnityEngine;
using System.Collections;

public class rotateableObjectsCreator : MonoBehaviour {
	public GameObject testSphere;
	public float numObjects;
	public static float circleRadius = 10f;
	public static Vector3 circleOrigin = Vector3.zero;
	public bool useRandomDistro = false;
	private Vector2 circPoint;
	private Vector3 curCircPoint;
	private GameObject circleCenterObject;
	private float angleIncrement;
	public Vector3 viewFwdLength;
	public static Vector3 fwdViewPt;
	public float rotSpeed = .1f;

	void Start () {
		circleCenterObject = GameObject.Find ("centerRot");
		circleOrigin = circleCenterObject.transform.position;
		angleIncrement = 360 / numObjects;
		fwdViewPt = circleOrigin + viewFwdLength; //Gets us the z loc of the fwd spawn
	}
	
	// Update is called once per frame
	void Update () {
		Debug.DrawLine (circleOrigin, fwdViewPt);

		if (Input.GetKeyDown (KeyCode.Space)) {
			for(int i = 0; i < numObjects; i++){
				circPoint = pickRandomCircPoint(angleIncrement * i);

				curCircPoint.x = circPoint.x;
				curCircPoint.z = circPoint.y;
				curCircPoint.y = circleOrigin.y;
				GameObject thisTestSphere;

				thisTestSphere = Instantiate(testSphere,curCircPoint,testSphere.transform.localRotation) as GameObject;
				thisTestSphere.transform.parent = circleCenterObject.transform;
			}
		}

		rotateAround();

	}

	private Vector2 pickRandomCircPoint(float angle){
		Vector2 randCircPoint = Vector2.zero;
		if(useRandomDistro) angle = Random.Range (0, 360);

		randCircPoint.x = circleOrigin.x + circleRadius * Mathf.Cos (angle);
		randCircPoint.y = circleOrigin.y + circleRadius * Mathf.Sin (angle);

		Debug.Log ("Created random circle vector: " + randCircPoint + " from angle:" + angle);

		return randCircPoint;
	}

	private void rotateAround(){
		Vector3 newRot = circleCenterObject.transform.eulerAngles;
		newRot.y += rotSpeed;
		circleCenterObject.transform.rotation = Quaternion.Euler (newRot);
	}
}
