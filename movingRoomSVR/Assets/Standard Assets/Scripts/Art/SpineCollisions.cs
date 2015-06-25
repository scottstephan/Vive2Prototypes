using UnityEngine;
using System.Collections;

public class SpineCollisions : MonoBehaviour {
	
	public Transform[] bones;
	public Vector4[] bonesOrigin;
	public float amplitude = 140;
	public float maxHeight = 0;
	public float originY = 0;
	public int boneCount = 0;

	// Use this for initialization
	void Start () {
		/*
		Transform thisXform = transform;
		boneCount = 0;
		while(thisXform.childCount > 0){
			Transform child = thisXform.GetChild(0);
			bonesOrigin[boneCount] = child.TransformPoint(child.position);
			bones[boneCount] = child;
			thisXform = child;
			boneCount++;
			maxHeight+= child.position.y;
		}
		*/
		//Vector4 pos = transform.localToWorldMatrix * transform.position;
		//originY = pos.y;
		for(int i=0; i < bones.Length; i++){
			bonesOrigin[i] = bones[i].position;	
			boneCount++;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		for(int i = 1; i < boneCount; i++){
			Transform bone = bones[i];
			Transform prevBone = bones[i-1];
			//Vector4 pos = bone.localToWorldMatrix * bone.position;
			//float wy = pos.y;
			//float ly = wy - originY;
			//float ny = ly / maxHeight;
			//float yRamp = Mathf.Pow(ny,3);
			float yRamp = Mathf.Pow((float)i / ((float)boneCount - 1f),3f);
			Vector3 boneWp =  bonesOrigin[i];
            Vector4 cur_dir = Vector3.forward;
            if( OceanCurrents.Singleton != null ) {
                cur_dir = OceanCurrents.Singleton.currentDirection;
            }
			float x = boneWp.x + yRamp * amplitude  * cur_dir.x;
			float y = boneWp.y;
			float z = boneWp.z + yRamp * amplitude  * cur_dir.z;
			Vector3 wp = new Vector3(x,y,z);
			bone.position = wp;
			prevBone.LookAt(wp); 
			if(i == boneCount - 1) bone.LookAt(prevBone);
			//bone.position.Set(bonesOrigin[i].x + x, bonesOrigin[i].y, bonesOrigin[i].z + z);
			//bone.Rotate(z,0,0);
//			Debug.Log(i + " : " + (yRamp * amplitude  * cur_dir.z) + " " + Time.time);
			//Debug.Log(oceanCurrents.currentDirection + " " + Time.time);
		}
		
	}
}
