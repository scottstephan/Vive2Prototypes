using UnityEngine;
using System.Collections;

public class ParkingLot {
	//private static ParkingLot singleton;
	public static ParkingSpot[] parkingSpots;
	
	public static void InitNewLevel(){
//		WemoLog.Log("***********************************************  initNewLevel ******************************************");
		parkingSpots = GameObject.FindObjectsOfType(typeof(ParkingSpot)) as ParkingSpot[];
	}

	
	public static void FindClosestSpot(CritterInfo critter_info){
		Vector3 pos =  critter_info.critterTransform.position;
//		WemoLog.Eyal("FindClosestSpot pos " + pos );
		SwimParkingData sd = critter_info.swimParkingData;
		float dist = 99999999f;
		ParkingSpot currentSpot = null;
		for(int i = 0; i < parkingSpots.Length; i++){
			ParkingSpot spot = parkingSpots[i];
			Transform spotXform = spot.transform;
			
//			WemoLog.Eyal("spot " + i + " name " + spot.gameObject.name +" available " + spot.available + " pos " + spotXform.position);
			if(spot.available)
			{
				Vector3 diff = spotXform.position - pos;
                float dot = Vector3.Dot( diff, critter_info.critterTransform.forward );
				float mag = diff.magnitude;
				if(mag < dist && dot > 0)
				{
					dist = mag;
					currentSpot = spot;
				}
			}

						
		}
		if(currentSpot) // we found the closest available spot
		{		
//			WemoLog.Eyal("The closest spot is " + currentSpot.gameObject.name);
			currentSpot.available = false;
			sd.spot = currentSpot;
			MeshFilter mf = currentSpot.GetComponent<MeshFilter>();
			if(mf != null)
			{
				Mesh mesh = mf.mesh;
				if(mesh)
				{
					int randVert = Random.Range(0,mesh.vertexCount -1);
					sd.targetPosition = currentSpot.transform.TransformPoint(mesh.vertices[randVert]);
//					WemoLog.Eyal("random vert " + sd.targetPosition);
				}
			}
			else sd.targetPosition = currentSpot.pos;
            
            sd.targetPosition += Vector3.up * critter_info.generalMotionData.critterBoxColliderRadius;
		}
		else
		{
//			WemoLog.Eyal("THERE ARE NO AVAILABLE SPOTS");
			GeneralSpeciesData gsd = critter_info.generalSpeciesData;
			gsd.switchBehavior = true;
			sd.parkingNeeded = false;
			sd.parkingLevel = 1f;
		}
	}
}
