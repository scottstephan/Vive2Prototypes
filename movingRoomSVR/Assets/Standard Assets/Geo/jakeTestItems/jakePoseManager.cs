using UnityEngine;
using System.Collections;
//1/20- I commented out a whole mess of stuff relating to auto-cycling since the time cueing replaces it. Feel free to uncomment if needed
public class jakePoseManager : MonoBehaviour {
	//public bool autoCyclePoses;
	public bool useTimeToCycle;
	public bool useControllerOrKeysToCycle;
    public bool loopCycle;
	//public bool playOverTime;
    private int poseIndex = -1;
    public float introDelay = 7f; //delay on 1st frame to allow for putting on headset etc.

	public GameObject[] poses;
	public float[] timeToTriggerPoses;
	public float timeThisLoop;

	
	//public float cycleDelay;
    public float totalTimeElapsed;
	private float playTime;
	private float playOverTimeInterval;
	public TextMesh frameIndicator;
	public float timeElapsedInAnimCycle;

    bool useController;
    bool buttonDown;
    public  bool canLoop = false;
	// Use this for initialization
	void Start () {       
		foreach (GameObject go in poses) {
			go.SetActive(false);
		}

	    //	poses [poseIndex].SetActive (true);
		//if(playOverTime) playOverTimeInterval = playTime / poses.Length;
        if (frameIndicator != null)	frameIndicator.text =  "Frame: " + poseIndex;

		if(useTimeToCycle)Invoke("showFirstPose",introDelay);
		//timeToTriggerPoses [0] = introDelay; //To keep the delay intact

		if (useControllerOrKeysToCycle)
		{
			showFirstPose();
		}
	}
	
	// Update is called once per frame
	void Update () {
        totalTimeElapsed = Time.time;
        timeElapsedInAnimCycle += Time.deltaTime;

		if (frameIndicator != null)
			frameIndicator.text = "Frame: " + poseIndex + "\n" + "Time:" + timeElapsedInAnimCycle.ToString("F2"); //maybe create a sep. text mesh for time?

        if (canLoop && !useControllerOrKeysToCycle)
        {
            if (poseIndex >= 0)
            {
                if (timeElapsedInAnimCycle >= timeToTriggerPoses[poseIndex]) showNextPose();
            }
		}

	    if (useControllerOrKeysToCycle)
	    {
	        if (Input.GetKeyDown(KeyCode.RightArrow))
	        {
	            showNextPose(true);
	        }
	        else if (Input.GetKeyDown(KeyCode.LeftArrow))
	        {
	            showNextPose(false);
	        }

	        if (!useController)
	        {
	            useController = SteamVR.instance != null && SteamControllerManager.Initialized(0);
	        }

	        if (useController)
	        {
	            if (!buttonDown)
	            {
	                if (SteamControllerManager.AnyButton(0))
	                {
	                    buttonDown = true;
	                    showNextPose();
	                }
	            }
	            else
	            {
	                if (!SteamControllerManager.AnyButton(0))
	                {
	                    buttonDown = false;
	                }
	            }
	        }
	    }
	}

	private void showNextPose(bool isMovingForward){
		Debug.Log("Cycling poses");
		poses[poseIndex].SetActive(false); //wont ever get ooi error

		if(isMovingForward){
			poseIndex++;
			if(poseIndex == poses.Length) {
				if (loopCycle)
				{
					poseIndex = 0; 
					timeElapsedInAnimCycle = 0;
				}
				else
				{
					canLoop = false;
				}
			}
		}
		else{
			poseIndex--;
			if(poseIndex == -1) poseIndex = poses.GetUpperBound(0);
		}

		
		if (frameIndicator != null)	frameIndicator.text = "Frame: " + poseIndex + "\n" + "Time:" + timeElapsedInAnimCycle;
		poses [poseIndex].SetActive (true);
			//this should probably get its own function, but whatever. 
		//float delay = determineDelay();
		//if(autoCyclePoses)Invoke("showNextPose",determineDelay());
	}

    private void showFirstPose()
    {
        poseIndex = 0;
        canLoop = true;
        poses[poseIndex].SetActive(true);
        
    }

	private void showNextPose(){
		showNextPose (true);
	}

	private float determineDelay(){
		//float delay = cycleDelay; //by default
		//if(playOverTime) delay = playOverTimeInterval;
		//return delay;
        return 0;
	}
}
