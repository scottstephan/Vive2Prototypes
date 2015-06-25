using UnityEngine;
using System.Collections;

public enum CritterMusicType {
	PredatorTypeMusic,
	OpenTypeMusic,
	ReefTypeMusic
}

public enum SpeciesCategory
{
	PREDATOR,
	OPENWATER,
	REEF,
	SEABED
}

public enum SpeciesSize
{
	TINY,
	SMALL,
	MEDIUM,
	LARGE,
	HUGE,
	UNKNOWN
}

// TODO> need to get rid of this.
public enum SwimBehaviorType
{
	SWIM_IDLE,
	SWIM_SCHOOL_FOLLOW,
	SWIM_TARGETED,
	SWIM_TO_POINT,
	SWIM_DISPERSE,
	SWIM_CHASE,
	SWIM_FREEFALL,
	SWIM_STRAFING,
	SWIM_PARKING,
	SWIM_FREE,
	SWIM_IDLE_BITE,
	SWIM_PLAYER_INTERACT,
	VIEWPORT_MOTION,
	INTERACTION,
	CIRCLE_AROUND_OBJECT,
    SWIM_FOLLOWPATH,
    DEAD,
    HOLD,
    SWIM_STRAFE_PLAYER,
    SWIM_PLAYER_VIEW,
    SWIM_SCRIPT_GOTO
}

public class GeneralSpeciesData : MonoBehaviour 
{
	
	public SpeciesCategory myCategory;
	[HideInInspector]
	public SpeciesSize mySize;
	public SpeciesSize myBowlSizeOverride = SpeciesSize.UNKNOWN;
	public CritterMusicType audioType;
	public float hungerLevel = 1.0f;
	public float hungerIncrement = 0.01f;
	public bool eatsReef = true;
	public float minSizeToEatFactor = 0.2f;
	public float maxSizeToEatFactor = 1.0f;
	public bool doesSchool = false;
	public float aggressionLevel = 0f;
	public bool isDead;
	[HideInInspector]
	public float airLevel = 1.0f;
	public float airIncrement = 0f;
	[HideInInspector]
	public bool airNeeded = false;
	
	

	
	public float maxGlowFactor = 1.0f;
	public float minDistGlowScale = 30f;
	public float maxDistGlowScale = 300f;
	
	[HideInInspector]
	public SwimBehaviorType myCurrentBehaviorType;
	[HideInInspector]
	public BehaviorBase myCurrentBehavior;
	[HideInInspector]
	public bool switchBehavior;
	[HideInInspector]
	public bool swimToPoint;
	[HideInInspector]
	public bool isExitingScene = false;
	
   	[HideInInspector]
	public bool isHungry = false;
	[HideInInspector]
	public bool becameHungry = false;
	[HideInInspector]
	public bool becameNotHungry = false;
	[HideInInspector]
	public bool canGetHungry = true;
	[HideInInspector]
	public bool becameAgrressive = false;
	[HideInInspector]
	public bool canChase = false;
	[HideInInspector]
	public bool becameAirborn = false;
	
	public bool isStrafing = false;

	[HideInInspector]
	public bool isReactToPlayer = false;
	[HideInInspector]
	public float reactToPlayerTime;
	[HideInInspector]
	public bool isPlayerInteract = false;

	//[HideInInspector]
	//public bool isSchooling = false;
	[HideInInspector]
	public bool startSchooling = false;
	
	public float disperseIfSmallerThanFactor = 0.3f;
	//[HideInInspector]
	//public bool stopSchooling = false;
	
	[HideInInspector]
	public CritterInfo myCritterInfo;
	
	[HideInInspector]
	public bool gotoActiveCamera = true; // upon creation always go to the active camera
	[HideInInspector]
	public int searchNewLeaderCounter;
	
	public string speciesTag; // TODO>> BAD BAD BAD!!!!

	[HideInInspector]
	public FishBowlData fishBowlData;
	
	[HideInInspector]
	public GameObject debugGUIText;
	
	[HideInInspector]
	public int masterIndex;
	
	[HideInInspector]
	public Renderer leGlowRenderer;
	
	
}

