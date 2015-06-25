using UnityEngine;
using System.Collections;

// SRNOTE:: Delegate support..Maybe time to move away from static classes and data containers?
public delegate void PreStartDelegate( CritterInfo critter_info );
public delegate void PostStartDelegate( CritterInfo critter_info );
public delegate void PreEndDelegate( CritterInfo critter_info );
public delegate void PostEndDelegate( CritterInfo critter_info );

public class BehaviorDataBase : MonoBehaviour {     
    [HideInInspector]
    public bool ogUseAvoidance;

    public PreStartDelegate PreStart;
    public PostStartDelegate PostStart;
    public PreEndDelegate PreEnd;
    public PostEndDelegate PostEnd;
}
