using UnityEngine;
using System;
using System.Collections;

public class UIBaseButton : UIButton {
	
	/*******************************************
	* CONSTANTS
	*******************************************/
	public const float DOWN_SCALE = 1.2f;
	public const float DISABLED_SCALE = 1f;
	public const float COOLDOWN_TIME = 0.3f;
	
	/*******************************************
	* PROPERTIES
	*******************************************/
	public bool overrideScale = false;
	public Vector3 scaleOveride;
	
	/*******************************************
	* VARIABLES
	*******************************************/
	private Vector3 scale;
	[HideInInspector] public float scaleDown = DOWN_SCALE;
	[HideInInspector] public float scaleDisabled = DISABLED_SCALE;
	[HideInInspector] public bool baseButtonEnabled = true;

	[HideInInspector] public Action __down;
	[HideInInspector] public Action __up;
	
	// Properties
	protected float releaseTime = 0;
	protected bool isDown = false;

	/*******************************************
	* UNITY METHODS
	*******************************************/
	public virtual void Awake(){
		
		// Set all NGUI Bull
		duration = -1;
		
		// If overide scale
		if(overrideScale){
			
			// Set base scale
			scale = scaleOveride;
		}
		
		// If Not overriding base scale
		else{
			// Save Scale
			scale = transform.localScale;
		}
	}
	
	/*******************************************
	* NGUI METHODS
	*******************************************/
	public override void OnHover (bool isOver){
		//base.OnHover (isOver);
	}
	public override void OnPress (bool isPressed){
		
		// We Are purposefully ignoring standard NGUI button
		//base.OnPress

		if(isPressed){
			
			// Trigger Down
			if(
				(baseButtonEnabled)
				&&(Time.time - releaseTime > COOLDOWN_TIME)
			){
				// Set Flag
				isDown = true;
				
				// action event
				OnDown();
			}
		}
		
		// On Up
		else{
			
			// Trigger On Up
			if(
				(baseButtonEnabled)
				&&(isDown)
			){
				// Set Flag
				isDown = true;
				
				// action event
				OnUp();
			}
		}
		
		// Save Cooldown Time
		releaseTime = Time.time;		
	}
	protected virtual void OnDown(){
		
		// TODO: Play Sound
		//AudioManager.PlayAudio((int)SoundFXID.Click);
		
		// Scale Up
		transform.localScale = new Vector3(
			scale.x * scaleDown,
			scale.y * scaleDown,
			scale.z
		);

		// If has down event
		if(__down != null){
				
			// Execute
			__down();
		}	
	}
	protected virtual void OnUp() {
		
		// Reset Scale
		transform.localScale = scale;
		
		// Launch Pause
		if(__up != null){
				
			// Execute
			__up();
		}	
	}
	
	public virtual void Enable(){
		
		// Set Flag
		baseButtonEnabled = true;
		
		// Set 
		transform.localScale = scale;
	}
	public virtual void Disable(){
		
		// Set Flag
		baseButtonEnabled = false;
		
		// Scale Up
		transform.localScale = new Vector3(
			scale.x * scaleDisabled,
			scale.y * scaleDisabled,
			scale.z
		);
		
		// Get NGUI sprites
		
		// Set Tint
	}
	
	public virtual void HideButton() {
		Disable();
		gameObject.SetActive(false);
		//renderer.enabled = false;
	}
	
	public virtual void ShowButton() {
		Enable();
		gameObject.SetActive(true);
		//renderer.enabled = true;
	}
}
