using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WemoObject : MonoBehaviour {   
	public bool tagsFoldout = false;
	public List<string> tags = new List<string>();
	
	[HideInInspector]
	public bool marked = false; // this used by the searching algorithms in OceanSphereController	

	private bool drawGizmos = false;
	private Color gizmo_color = new Color(0.8F, 0.8F, 0.8F, 0.6F);
	private Color no_collider_gizmo_color = new Color(1.0F, 0F, 0F, 0.6F);
	public void SetDrawGizmos( bool draw ) {
		drawGizmos = draw;
	}
	
	void OnDrawGizmos() {
		if( !drawGizmos ) { 
			return;
		}
		if( GetComponent<Collider>() != null ) {
			if( GetComponent<Collider>().GetType() == typeof(BoxCollider) ) {
				Gizmos.color = gizmo_color;
				Gizmos.DrawCube(transform.position + ((BoxCollider)GetComponent<Collider>()).center,((BoxCollider)GetComponent<Collider>()).size);
			}
			else if( GetComponent<Collider>().GetType() == typeof(SphereCollider) ) {
				Gizmos.color = gizmo_color;
				float radius = ((SphereCollider)GetComponent<Collider>()).radius * transform.localScale.x;
				Gizmos.DrawSphere(transform.position + ((SphereCollider)GetComponent<Collider>()).center,radius);
			}
			else {
				Gizmos.color = no_collider_gizmo_color;
				Gizmos.DrawSphere(transform.position,50.0f);			
			}
		}
		else {
			Gizmos.color = no_collider_gizmo_color;
			Gizmos.DrawSphere(transform.position,50.0f);			
		}
	}
}
