using UnityEngine;
using System.Collections;

public class CollisionHelpers {
	
	// SphereSlide
	// returns true or false depending on whether or not we hit anything.
	// does not currently return any raycasthits. TODO pass a callback function for hit detection.
	// moves a sphere the movement amount and applies a slide collision algorithm to the movement if we hit 
	// anything.
	// slide_cnt is the number of allowed slides before we consider ourselves stopped. 
	// slide_cnt should really never be more than 2, otherwise we are looping back on ourselves and in a corner.
	public static bool SphereSlide( Transform trans, Vector3 move_amt, float radius, int layer_mask, int slide_cnt, ref Vector3 norm ) {
		bool ret_val = false;
		float move_mag = move_amt.magnitude;
        Ray move_ray = new Ray(trans.position, move_amt);
		RaycastHit hit;
		bool done = false;
		int cnt = 0;
//		Vector3 origPos = trans.position;
//		float origMoveMag = move_mag;
		
		if( Mathf.Approximately(move_mag, 0f ) ) {
			return false;
		}
		
		while( !done ) {
			if( Physics.SphereCast(move_ray,radius,out hit,move_mag,layer_mask) ) {
				ret_val = true;
				norm = hit.normal;
				Vector3 piece = move_ray.direction;
				// the 0.02 is necessary to keep us out of the surface a little
				if( hit.distance <= 0.02f ) {
					piece = Vector3.zero;
//					Debug.Log("slide " + cnt + " move_mag " +  move_mag + " hit.distance " + hit.distance + " " +  hit.collider.gameObject.name);
					trans.position = move_ray.origin;
					// we cannot return otherwise we jitter? but what are we colliding with?!?
					//					return true;
				}
				else {
					piece *= ( hit.distance - 0.02f );
				}
				// move to our collision point
				trans.position += piece;
				
				// build a slide vector for our motion remainder.
				float move_left = move_mag - hit.distance;
				move_amt *= ( move_left / move_mag );
				float dot = Vector3.Dot( move_amt, hit.normal );
				move_amt -= hit.normal * dot;

				// assign our slide vector to our ray for the next iteration.
				move_ray.origin = trans.position;
				move_mag = move_amt.magnitude;
				move_ray.direction = move_amt;
			}
			else {
				trans.position += move_amt;
				done = true;
			}
		
			if( Mathf.Approximately(move_mag, 0f ) ) {
				done = true;
			}		

			cnt++;
			if( cnt >= slide_cnt ) {
				done = true;
			}
		}

		return ret_val;
	}

	static public bool IsInside( Collider test, Vector3 point, Vector3 collider_center ) 
    {
        return test.bounds.Contains(point);
	}
	
}
