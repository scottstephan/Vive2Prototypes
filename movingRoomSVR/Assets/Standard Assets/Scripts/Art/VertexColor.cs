using UnityEngine;
using System.Collections;

public class VertexColor : MonoBehaviour {
	public Color color = Color.white;


	void Start () {
		MeshFilter mf = GetComponent<MeshFilter>();
		if(mf != null){
			Mesh mesh = mf.mesh;
			for(int i = 0; i < mesh.vertexCount; i++){
				//mesh.colors[i] = color;
			}
		}
	}

}
