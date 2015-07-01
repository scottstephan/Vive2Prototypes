using UnityEngine;
using System.Collections;

public class drawCircle : MonoBehaviour {
	public float theta_scale = 0.01f;        //Set lower to add more points
	public int size; //Total number of points in circle
	public float radius = 3f;
	public LineRenderer lineRenderer;
	public GameObject circleCenter;
	public bool isDrawingCircle = false;
	public Material lineMaterial;
	private bool isCircleSet = false;

	void Awake () {       
		Debug.Log ("Food is awake");
		float sizeValue = (2.0f * Mathf.PI) / theta_scale; 
		size = (int)sizeValue;
		size++;
//		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = lineMaterial;
		lineRenderer.SetWidth(0.02f, 0.02f); //thickness of line
		lineRenderer.SetVertexCount(size);      
	}

	public void setCircleParams(){
		if (!isCircleSet) {

		}
	}

	void Update(){
		if (isDrawingCircle) draw ();
	}

	private void draw () {      
		Vector3 pos;
		float theta = 0f;

		for (int i = 0; i < size; i++) {          
			theta += (2.0f * Mathf.PI * theta_scale);         
			float x = radius * Mathf.Cos (theta);
			float y = radius * Mathf.Sin (theta);          
			x += gameObject.transform.position.x;
			y += gameObject.transform.position.z;
			pos = new Vector3 (x, 2, y);
			lineRenderer.SetPosition (i, pos);
		}
	}
	
}