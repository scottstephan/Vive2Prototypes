using UnityEngine;
using System.Collections;

public class CubeFieldGenerator : MonoBehaviour {

	public GameObject cubeObject;
	public Transform matrixParent;

	public int matrixSize = 32;
	public float cubeSpacing = 65f;

	// Use this for initialization
	void Start () {

		GenerateCubes();
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	void GenerateCubes() {

		/*
		//destroy all children (in case we re-caculate for some reason)
		for (int i = 0; i < matrixParent.childCount; i++) {
			GameObject.Destroy(matrixParent.GetChild(i).gameObject);
		}
		*/

		float offset = (matrixSize * cubeSpacing) / 2f;

		for (int i = 0; i < matrixSize; i++) {
			for (int j = 0; j < matrixSize; j++) {
				for (int k = 0; k < matrixSize; k++) {

					Vector3 newPos = new Vector3(offset - (i * cubeSpacing), offset - (j * cubeSpacing), offset - (k * cubeSpacing));

					GameObject newCube = (GameObject)GameObject.Instantiate(cubeObject);

					newCube.transform.parent = matrixParent;
					newCube.transform.position = newPos;

					newCube.SetActive(true);

					if (newCube.GetComponent<Collider>().bounds.Contains(CameraManager.GetCurrentCameraPosition()) || newCube.GetComponent<Collider>().bounds.Contains(CameraManager.GetEyePosition()))
					    newCube.SetActive(false);
				}
			}
		}


	}
}
