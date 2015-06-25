using UnityEngine;
using System.Collections;

public class AgentGlowBehavior : MonoBehaviour {
	
	Component[] particles; 
	private bool isSelected;
	
	// Use this for initialization
	//Disable the particle emitters at initilization
	void Start () {
		
		isSelected = false;
		
		// Init the particles to not emit and switch off the spotlights:
		particles = GetComponentsInChildren<ParticleEmitter>();
		HideParticles();

	}
	
	public void AgentSelected() {		
			ShowParticles();
		
	}
	
	public void AgentDeselected() {	
			HideParticles();	
		
	}
	
	private void ShowParticles() {
		
		isSelected = true;
		
		//TEMPORARILY DISABLE PARTICLE EFFECTS
//		foreach(ParticleEmitter p in particles)
//		{
//			
//			p.emit = true;
//		}
		
	}
	
	private void HideParticles() {
	
		isSelected = false;
		
		foreach(ParticleEmitter p in particles)
		{
			p.emit = false;
		}
	}
	
	public bool IsObjectSelected() {
		return isSelected;
	}
	
}
