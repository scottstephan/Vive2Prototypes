using UnityEngine;
using System.Collections;

[System.Serializable]
public class FishBowlData {
	public Vector3 position;
	public Quaternion rotation;
	public Vector3 size;
	public Vector3 halfSize;
	public string name;
	public float maxPitch;
	public float maxPitchY;
	//public CritterInfo critter_info;
}

public class FishBowl : MonoBehaviour {

	public SpeciesSize[] critterSizeInBowl;
	
	[HideInInspector]
	public FishBowlData fishBowlData;
		
	public bool defaultBowl;
	public bool spawnBowl;

	public virtual void Awake() 
    {
        Transform myTransform = transform;

		fishBowlData = new FishBowlData();
        fishBowlData.position = myTransform.position;		
        fishBowlData.rotation = myTransform.rotation;
        fishBowlData.size = myTransform.localScale;
        fishBowlData.halfSize = ( myTransform.localScale * 0.5f );
		fishBowlData.name = gameObject.name;

        //calculate max pitch
		if(fishBowlData.size.x > fishBowlData.size.z) 
		{
			fishBowlData.maxPitch = Mathf.Acos(fishBowlData.size.y / fishBowlData.size.z);
			Vector2 vec = new Vector2(fishBowlData.size.z, fishBowlData.size.y).normalized;
			fishBowlData.maxPitchY = vec.y;
		}
		else
		{
			fishBowlData.maxPitch = Mathf.Acos(fishBowlData.size.y / fishBowlData.size.x);
			Vector2 vec = new Vector2(fishBowlData.size.x, fishBowlData.size.y).normalized;
			fishBowlData.maxPitchY = vec.y;
		}

		//WemoLog.Eyal(fishBowlData.name + " maxPitch " + fishBowlData.maxPitch + " angle " + Mathf.Rad2Deg * fishBowlData.maxPitch);
		//WemoLog.Eyal(fishBowlData.name + " maxPitch " + fishBowlData.maxPitch + " y " + fishBowlData.maxPitchY);
	}	

    public void UpdateTransform(Transform t)
    {
        fishBowlData.position = t.position;     
        fishBowlData.rotation = t.rotation;
        fishBowlData.size = t.localScale;
        fishBowlData.halfSize = ( t.localScale * 0.5f );
    }

	public bool drawGizmos = true;

	public Color gizmoColor = new Color(1,1,1,0.5f);
	
	public void SetDrawGizmos( bool draw ) {
		drawGizmos = draw;
	}

	void OnDrawGizmos() {
		if (drawGizmos)
		{
			Gizmos.color = gizmoColor;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube(Vector3.zero,Vector3.one);
			//Gizmos.DrawLine(Vector3.zero,Vector3.forward);
		}
	}
}
