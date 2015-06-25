using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GibData
{
	public GameObject go;
	public Transform myTransform;
	public Vector3 velocity;
	public float motionTimer;

	public float rotationSpeed;
}

public class GibManager : MonoBehaviour
{
	public int testPopulation;

	public FishBowl fishBowl;

	public GameObject[] meshObjects;

	public float motionTimeMin = 0.25f;
	public float motionTimeMax = 0.5f;

	public float speedMin = 0.1f;
	public float speedMax = 10.5f;

	public float rotateMin = -30f;
	public float rotateMax = 30f;

	private float oceanCurrentFactor = 0.2f;

	public float gravity = 0.1f;

	static GibManager instance;

	private List<GibData> gibList;
	// Use this for initialization
	void Start()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Debug.LogError("GibManager: more than one GibManager exists in the scene.");
		}

		// gather the variant data..
		if (!AppBase.Instance.RunningAsPreview())
		{
			foreach (GameObject go in meshObjects)
			{
				WemoItemData item_data = go.GetComponent<WemoItemData>();
				if (item_data != null)
				{
					App.FishManager.VariantHasParentData(item_data.variantID, false);
				}
			}
		}

		gibList = new List<GibData>();
		for (int i = 0; i < testPopulation; i++)
		{
			GibData new_data = new GibData();

			int idx = Random.Range(0, meshObjects.Length);
			FishBowlData bd = fishBowl.fishBowlData;
			//               DebugDisplay.AddDebugText(" intro fish bowl " +  bd.name + " :: p " + bd.position + " :: r "  + bd.rotation + " :: s " + bd.size);
			Vector3 rand_pos = RandomExt.VectorRange(bd.size);
			Vector3 new_pos = bd.position + (bd.rotation * rand_pos);
			new_data.go = GameObject.Instantiate(meshObjects[idx], new_pos, Random.rotation) as GameObject;
			new_data.myTransform = new_data.go.transform;

			float speed = Random.Range(speedMin, speedMax);
			new_data.velocity = Random.insideUnitSphere * speed;
			new_data.motionTimer = Random.Range(motionTimeMin, motionTimeMax);
			new_data.rotationSpeed = Random.Range(rotateMin, rotateMax);

			gibList.Add(new_data);
		}
	}

	public static void SpawnGibs(CritterInfo dead_critter)
	{
		if (instance == null)
		{
			Debug.LogWarning("GibManager: could not find a GibManager so could not spawn gibs");
			return;
		}

		if (dead_critter.deadData.HasGibs())
		{
			instance.Spawn(dead_critter);
		}
	}

	void Spawn(CritterInfo dead_critter)
	{
		if (dead_critter.deadData.eatenFX != null)
		{
			// TODO: pooling
			GameObject.Instantiate(dead_critter.deadData.eatenFX, dead_critter.critterTransform.position, dead_critter.critterTransform.rotation);            
		}

        for (int i=0; i<dead_critter.deadData.gibs.Length; ++i)
		{
            GibSpawnData gib = dead_critter.deadData.gibs[i];

			if (gib.gibObject == null || gib.count <= 0)
			{
				continue;
			}

			//Debug.Log("SpawnGib " + gib.gibObject.name + " " + gib.designGroupName);

			for (int g = 0; g < gib.count; ++g)
			{
//				GameObject critter = gib.gibObject;
				Vector3 pos = dead_critter.critterTransform.position;
				Quaternion rot = Random.rotation;

				pos += (rot * gib.offset);

				GibData new_data = new GibData();

				//            FishBowlData bd = dead_critter.generalSpeciesData.fishBowlData;
				//               DebugDisplay.AddDebugText(" intro fish bowl " +  bd.name + " :: p " + bd.position + " :: r "  + bd.rotation + " :: s " + bd.size);
				//            Vector3 new_pos = bd.position + (bd.rotation * rand_pos);
				new_data.go = GameObject.Instantiate(gib.gibObject, pos, rot) as GameObject;
				new_data.go.transform.localScale = dead_critter.critterTransform.localScale;
				new_data.myTransform = new_data.go.transform;

				float speed = Random.Range(speedMin, speedMax);
				new_data.velocity = Random.insideUnitSphere * speed;
				new_data.motionTimer = Random.Range(motionTimeMin, motionTimeMax);
				new_data.rotationSpeed = Random.Range(rotateMin, rotateMax);

				gibList.Add(new_data);
			}
		}	
	}

	// Update is called once per frame
	void Update()
	{
		//        Transform cam = CameraManager.GetCurrentCameraTransform();
		float dt = Time.deltaTime;
		Vector3 cur_dir = Vector3.zero;
		if (OceanCurrents.Singleton != null)
		{
			cur_dir = OceanCurrents.Singleton.currentDirection;
		}

        for (int i=0; i<gibList.Count; ++i)
		{
            GibData bd = gibList[i];

			bd.motionTimer -= dt;
			if (bd.motionTimer < 0f)
			{
				float speed = Random.Range(speedMin, speedMax);
				bd.velocity = Random.insideUnitSphere * speed;
				bd.motionTimer = Random.Range(motionTimeMin, motionTimeMax);
			}

			bd.myTransform.position += bd.velocity * dt;

			bd.myTransform.position += (cur_dir * oceanCurrentFactor);

			bd.myTransform.position += new Vector3(0f, -gravity, 0f);

			bd.myTransform.Rotate(bd.myTransform.up, bd.rotationSpeed * dt);
		}
	}

	void OnDestroy()
	{
        for (int i=0; i<gibList.Count; ++i)
		{
            GibData bd = gibList[i];
			bd.myTransform = null;
			Destroy(bd.go);
			bd.go = null;
		}

		gibList.Clear();
		gibList.TrimExcess();
		gibList = null;
	}
}
