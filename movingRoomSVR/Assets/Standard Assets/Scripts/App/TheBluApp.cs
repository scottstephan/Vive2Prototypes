using UnityEngine;
using System.Collections;

public delegate void FirstDiveInStartDelegate();
	
public class TheBluApp : AppBase {

	public static FirstDiveInStartDelegate FirstDiveInStart;
	
	public static System.DateTime startupDate;
    public static long selfDestructThresholdSeconds = 180 * 60;	

	public override void Start() {
		base.Start();

        usingBuiltData = true;

  		startupDate = System.DateTime.Now;
		
		// add components..
        // TODO> BluVR has no use of Waldos. at least not right now.
//		gameObject.AddComponent<WaldoManager>();
//		WaldoManager.InitWaldos();

//		EventStreamManager.InitOnAwake();
	}
	
	public override void OnFirstDiveIn(){
		
		Debug.Log("FIRST DIVE IN - GO!");
	
		base.OnFirstDiveIn();
		
		if (FirstDiveInStart != null)
			FirstDiveInStart();
	}

	void Update() {
		// do anything that we need to stage after 'start' but before our first true update.
		if( !firstUpdate ) {			
			//////////////////////
			// TODO> this can probably be re-factored out this.
			DebugDisplay.OutputBuildNumber();
			//////////////////////
			
			SystemSpec.OutputSetup(); // requires the network manager to be setup for metrics related calls

			firstUpdate = true;
		}

		//Graffiti Specific exiting
//		if( !Application.isWebPlayer && InputManager.GetKeyDown("escape") ) {
//            StartupObject.singleton.DelayedFrameQuit(5);
//		}

		float dt = Time.deltaTime;

        // TODO> BluVR has no use of Waldos. at least not right now.
        //WaldoManager.UpdateWaldos( dt );

		InputManager.UpdateInput( dt );
		SimManager.UpdateSim( dt );		

		SphereInstance.Instance.UpdateSphere();
		
		//EventStream Update
//		EventStreamManager.UpdateStreamState();

		// see if we need to auto destruct.
/*		System.TimeSpan diff = System.DateTime.Now.Subtract(startupDate);
		if( diff.TotalSeconds > selfDestructThresholdSeconds
			&& InputManager.timeSinceMouseMovement > 20f ) {
            StartupObject.singleton.DelayedFrameQuit(5);
		}*/
	}
}
