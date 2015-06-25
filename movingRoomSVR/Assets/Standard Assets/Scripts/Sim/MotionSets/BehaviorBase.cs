using UnityEngine;
using System.Collections;

public class BehaviorBase {
	public static BehaviorBase singleton; 
	
    public void Construct() {
		singleton = this;
	}

	// behavior can never change
	public virtual bool IsSingletonBehavior() { return false; }
	public static bool StaticIsSingletonBehavior() 
	{
		return singleton.IsSingletonBehavior();
	}

	// behavior can never change
	public virtual bool SingletonAllowsSwitchTo() { return false; }
	public static bool StaticSingletonAllowsSwitchTo() 
	{
		return singleton.SingletonAllowsSwitchTo();
	}

	// called once at initialization time for hte fish
	public virtual void OneTimeStart( CritterInfo critter_info ) {}
	public static void StaticOneTimeStart( CritterInfo critter_info ) 
	{
		singleton.OneTimeStart(critter_info);
	}

    // called once at delete time for the fish
    public virtual void OneTimeEnd( CritterInfo critter_info ) {}
    public static void StaticOneTimeEnd( CritterInfo critter_info ) 
    {
        singleton.OneTimeEnd(critter_info);
    }
    
	// called every time this behavior becomes the active behavior
	public virtual void Start( CritterInfo critter_info ) {}
	public static void StaticStart( CritterInfo critter_info ) 
	{
		singleton.Start(critter_info);
	}
	
	public virtual void Update( CritterInfo critter_info ) {}
	public static void StaticUpdate( CritterInfo critter_info ) 
	{
		singleton.Update(critter_info);
	}
	
	// called every time a different behavior becomes the active behavior and this one ends
	public virtual void End( CritterInfo critter_info ) {}
	public static void StaticEnd( CritterInfo critter_info ) 
	{
		singleton.End(critter_info);
	}		
	
	public virtual float EvaluatePriority( CritterInfo critter_info ) {return 0f;}
	public static float StaticEvaluatePriority( CritterInfo critter_info ) 
	{
		return singleton.EvaluatePriority(critter_info);
	}

	public virtual void PreCollision( CritterInfo critter_info ) {}
	public static void StaticPreCollision( CritterInfo critter_info ) 
	{
		singleton.PreCollision(critter_info);
	}

	public virtual void PostCollision( CritterInfo critter_info ) {}
	public static void StaticPostCollision( CritterInfo critter_info ) 
	{
		singleton.PostCollision(critter_info);
	}
	
	public virtual void LateUpdate( CritterInfo critter_info ) {}
	public static void StaticLateUpdate( CritterInfo critter_info ) 
	{
		singleton.LateUpdate(critter_info);
	}
	
	public virtual void UpdateMotion( CritterInfo critter_info ) {
		GeneralMotion.UpdateMotion( critter_info, true );
	}
}
