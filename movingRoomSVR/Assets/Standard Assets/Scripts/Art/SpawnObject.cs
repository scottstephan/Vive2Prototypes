using UnityEngine;
using System.Collections;

public class SpawnObject : MonoBehaviour {
	public GameObject prefab;
	public Vector3 spawnPosition;
	public Vector3 spawnRotation;

	void Start () {
		Instantiate(prefab, spawnPosition, Quaternion.Euler(spawnRotation));
	}

}
