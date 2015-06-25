using UnityEngine;
using System.Collections;

public class SwimIdleBite : BehaviorBase {
    
    // called once at initialization time for hte fish
    public override void OneTimeStart( CritterInfo critter_info ) {
        if( critter_info.swimIdleBiteData == null ) {
            return;
        }

        critter_info.swimIdleData.PreStart += IdleIsStarting;
        critter_info.swimIdleData.PreEnd += IdleIsEnding;
    }
    
    // called once at initialization time for hte fish
    public override void OneTimeEnd( CritterInfo critter_info ) {
        if( critter_info.swimIdleBiteData == null ) {
            return;
        }
        
        critter_info.swimIdleData.PreStart -= IdleIsStarting;
        critter_info.swimIdleData.PreEnd -= IdleIsEnding;
    }

    public void IdleIsStarting( CritterInfo critter_info ) {
        critter_info.awarenessCollision.CollisionEntered += CheckTriggerBite;
    }

    public void IdleIsEnding( CritterInfo critter_info ) {
        critter_info.awarenessCollision.CollisionEntered -= CheckTriggerBite;
    }
 
    void CheckTriggerBite( CritterInfo me, Collider other ) {
        GeneralSpeciesData other_gsd = other.GetComponent<GeneralSpeciesData>();
        if( other_gsd == null ) {
            other_gsd = other.transform.parent.GetComponent<GeneralSpeciesData>();
        }
        
        if( other_gsd.myCritterInfo == me ) { // ?? can this even happen
            return;
        }
        
        GeneralMotionData other_gmd = other.GetComponent<GeneralMotionData>();
        if( other_gmd == null ) {
            other_gmd = other.transform.parent.GetComponent<GeneralMotionData>();
        }
  
        if( other_gsd == null || other_gmd == null ) {
            return;
        }
            
        if(other_gmd.critterBoxColliderSize.z * me.generalSpeciesData.maxSizeToEatFactor > me.generalMotionData.critterBoxColliderSize.z
            && other_gmd.critterBoxColliderSize.z * me.generalSpeciesData.minSizeToEatFactor < me.generalMotionData.critterBoxColliderSize.z) {
            me.animBase.isFeeding = true;
            me.generalSpeciesData.isHungry = true;
        }
    }    
}
