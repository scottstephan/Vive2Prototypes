using UnityEngine;
using System.Collections;

public class passive_Crab : svrInteractableBase {
	private passiveCreatureAI aiScript;

		void Start()
		{
			base.Start ();
			aiScript = gameObject.GetComponent<passiveCreatureAI> ();
		}
		
		public override void controllerInputTriggerDown()
		{
			base.controllerInputTriggerDown ();
			aiScript.stopAI ();
			objectIsPickedUp(); 
		}
		
		public override void controllerInputTriggerUp()
		{
			base.controllerInputTriggerUp ();
			aiScript.startAI (); //may need a delay or wait for it to land etc etc.
			objectIsDropped();
		}

		private void crabCustomPhysics(){
			//Need to turn on/off holds in order to let crab fly, land and recover
		}
		
}


