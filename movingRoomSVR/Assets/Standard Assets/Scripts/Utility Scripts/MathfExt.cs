using UnityEngine;
using System.Collections;

public class MathfExt {
	public const float TWO_PI = 6.28318531f;
	public const float PI_2 = 1.57079633f;
	public const float PI_3 = 1.04719755f;
	public const float PI_4 = 0.785398163f;
    // class log
//    private static Log log = Log.GetLog(typeof(MathfExt).FullName);

    public static float WrapAngle( float angle )
    {
		while(angle > 180F) {
			angle -= 360F;
		}
		while(angle < -180F) {
			angle += 360F;
		}

        return angle;
    }

    public static float ClampAngle( float angle, float min, float max )
    {
		while(angle > 180F) {
			angle -= 360F;
		}
		while(angle < -180F) {
			angle += 360F;
		}

        return Mathf.Clamp (angle, min, max);
    }

    // Unity stores euler angles 0-360 degrees and not -180-180
    // this converts a unity euler into a regular euler
    public static float RegularEuler( float angle )
    {
        if( angle > 180f )
        {
            angle = angle - 360f;

            if (angle > 180f)
            {
                angle = 180f;
            }
        }

        return angle;
    }
    
    // Unity stores euler angles 0-360 degrees and not -180-180
    // this converts a unity euler into a regular euler
    public static Vector3 RegularEuler( Vector3 eulers )
    {
        eulers.x = RegularEuler( eulers.x );
        eulers.y = RegularEuler( eulers.y );
        eulers.z = RegularEuler( eulers.z );
        return eulers;
    }
    
    // Unity stores euler angles 0-360 degrees and not -180-180
	// this converts a regular euler to a unity euler
    public static float UnityEuler( float angle )
    {
		if( angle < 0f )
        {
            angle = angle + 360f;
            
            if (angle < 0f)
            {
                angle = 0f;
            }
		}

		return angle;
	}
	
    // Unity stores euler angles 0-360 degrees and not -180-180
    // this converts a unity euler into a regular euler
    public static Vector3 UnityEuler( Vector3 eulers )
    {
        eulers.x = UnityEuler( eulers.x );
        eulers.y = UnityEuler( eulers.y );
        eulers.z = UnityEuler( eulers.z );
        return eulers;
    }
    
    public static bool Approx(float val, float about, float range) {
        return ( ( Mathf.Abs(val - about) < range) );
    }

    public static bool Approx(Vector3 val, Vector3 about, float range) {
        return ( (val - about).sqrMagnitude < range*range);
    }
	
	public static float Fit(float x ,float x0 ,float x1, float y0 ,float y1)
	{
		float v = y0+(x-x0)*(y1-y0)/(x1-x0); // 5 2-6 10-20     10 + (5-2) * (20-10) / (6-2) = 17.5
		if(y0<y1)
		{
			v = Mathf.Clamp(v, y0, y1);
		}
		else
		{
			v = Mathf.Clamp(v, y1, y0);   //  5 2-6 20-10     20 + (5-2) * (10-20) / (6-2) = 12.5
		}
		return v;
	}
	
	public static Vector3 YawVector(Vector3 vec, float yaw) {
		float cos = Mathf.Cos(yaw);
		float sin = Mathf.Sin(yaw);
		float new_x = cos * vec[0] - sin * vec[2];
		float new_z = sin * vec[0] + cos * vec[2];
		return new Vector3(new_x,vec[1],new_z);
	}

    public static Vector3 PitchVector(Vector3 vec, float pitch) {
        float cos = Mathf.Cos(pitch);
        float sin = Mathf.Sin(pitch);
        float new_y = cos * vec[1] - sin * vec[2];
        float new_z = sin * vec[1] + cos * vec[2];
        return new Vector3(vec[0],new_y,new_z);
    }

    // utility for AI when you need to know if something is on the right or left, 1 is right, -1 is left
    // Just for code simplicity, if reusing these cross products and sqrts best to unroll manually
    public static float DotRight(Vector3 forward, Vector2 test)
    {
        Vector3 right = Vector3.Cross( forward, Vector3.up );

        if (right.magnitude == 0f)
        {
            right = Vector3.Cross( forward, Vector3.right );

            if (right.magnitude == 0f)
            {
                right = Vector3.Cross( forward, Vector3.forward );
            }
        }

        return Vector3.Dot(right.normalized, test.normalized);
    }

	// z forward
	public static Vector3 BuildYawPitchUnitVec(float yaw, float pitch) {
		float ycos = Mathf.Cos(yaw);
		float ysin = Mathf.Sin(yaw);
		float pcos = Mathf.Cos(pitch);
		float psin = Mathf.Sin(pitch);
		float new_x = -pcos * ysin;
		float new_y = psin;
		float new_z = pcos * ycos;
		return new Vector3(new_x,new_y,new_z);
	}
	
	public static void AccelDampDelt( Vector3 desired, float accel, float decel, float dt, float max_speed, ref Vector3 velocity, ref Vector3 val ) {
		if ( Approx(val,desired,0.001f) ) {
			val = desired;
			velocity = Vector3.zero;
			return;
		}

		// do we start decelerating.
		// time to stop. t = v0 / a
		float speed = velocity.magnitude;
		float time_to_stop = speed / decel;
		// d = vt * 1/2at^2
		float distance_required_to_stop = speed * time_to_stop - 0.5f * decel * time_to_stop * time_to_stop;		

		Vector3 to_desired = desired - val;
		float distance_left = to_desired.magnitude;
		
		if( distance_required_to_stop >= distance_left ) {
			speed -= decel * dt;
		}
		else {
			speed += accel * dt;
		}
		speed = Mathf.Clamp(speed,-max_speed,max_speed);
		velocity = to_desired * ( speed / distance_left );
		
		Vector3 inc = velocity * dt;
		val += inc;
/*		float new_distance_left = desired - val;
		if( ( new_distance_left >= 0 && raw_distance_left < 0f )
			||( new_distance_left <= 0 && raw_distance_left > 0f ) ) { // dont overshoot.
			val = desired;
			speed = 0f;
		}*/
	}

	public static void AccelDampDelt( float desired, float accel, float decel, float dt, float max_speed, ref float speed, ref float val, ref bool decel_active, bool debug=false) {
//		if( debug ) {	
//			log.Debug("TOP: desired " + desired + " : speed " + speed + " : val " + val);
//		}
        float approx = val - desired;
        if( approx >= -0.001f && approx <= 0.001f ) {
            val = desired;
			speed = 0f;
			decel_active = false;
//			if( debug ) {	
//				log.Debug("DONE");
//			}
			return;
		}

		// do we start decelerating.
		// time to stop. t = v0 / a
		float abs_speed = speed < 0f ? -speed : speed;
		float time_to_stop = abs_speed / decel;
		// d = vt * 1/2at^2
		float distance_required_to_stop = abs_speed * time_to_stop - 0.5f * decel * time_to_stop * time_to_stop;
		
		float raw_distance_left = desired - val;
        float distance_left = raw_distance_left < 0f ? -raw_distance_left : raw_distance_left; 
		float sign = (raw_distance_left > 0f) ? 1f : -1f;
		
		if( ( raw_distance_left > 0 && speed < 0 ) 
		   || ( raw_distance_left < 0 && speed > 0 ) ) {
//			if( debug ) {	
//				log.Debug("OTHERDECEL:abs_speed " + abs_speed + " : time_to_stop " + time_to_stop + " : dist_for_stop " + distance_required_to_stop + " : raw_left " + raw_distance_left + " : dist_left " + distance_left + " : sign " + sign);
//			}
			speed -= decel * dt * sign * -1f;
		}		
		else if( decel_active || ( time_to_stop > 0f && distance_required_to_stop >= distance_left ) ) {
//			if( debug ) {	
//				log.Debug("DECEL:abs_speed " + abs_speed + " : time_to_stop " + time_to_stop + " : dist_for_stop " + distance_required_to_stop + " : raw_left " + raw_distance_left + " : dist_left " + distance_left + " : sign " + sign);
//			}
			float spd_dec = decel * dt * sign;
            float abs_spd_dec = spd_dec < 0f ? -spd_dec : spd_dec;
            if( decel_active && ( abs_spd_dec > abs_speed ) ) {
				speed *= 0.5f;
			}
			else {
				speed -= spd_dec;
			}
			decel_active = true;
		}
		else {
			decel_active = false;
			speed += accel * dt * sign;
//			if( debug ) {	
//				log.Debug("ACCEL:abs_speed " + abs_speed + " : time_to_stop " + time_to_stop + " : dist_for_stop " + distance_required_to_stop + " : raw_left " + raw_distance_left + " : dist_left " + distance_left + " : sign " + sign);
//			}
		}
        if (speed < -max_speed)
        {
            speed = -max_speed;
        }
        else if (speed > max_speed)
        {
            speed = max_speed;
        }
		
		float inc = speed * dt;
		val += inc;
		float new_distance_left = desired - val;
		if( ( new_distance_left >= 0 && raw_distance_left < 0f )
			||( new_distance_left <= 0 && raw_distance_left > 0f ) ) { // dont overshoot.
			val = desired;
			decel_active = false;
			speed = 0f;
//			if( debug ) {	
//				log.Debug("DONE");
//			}
		}
//		if( debug ) {	
//			log.Debug("BOT:desired + " + desired + " : speed " + speed + " : val " + val + " : new_dist_left " + new_distance_left);
//		}
	}

	public static void AccelDampDelt_Angle( float desired, float accel, float decel, float dt, float max_speed, ref float speed, ref float val, ref float prev_desired, ref bool decel_active, bool debug=false ) {
//		if( debug )
//		{
//			log.Debug("TOP: desired " + desired + " : speed " + speed + " : val " + val);
//		}

        float approx = val - desired;
        if( approx >= -0.001f && approx <= 0.001f ) {
			val = desired;
			prev_desired = desired;
			decel_active = false;
			speed = 0f;
//			log.Debug("sameexit d " + desired + " : v " + val + " : s " + speed);
//			if( debug ) 
//            {
//				log.Debug("DONE");
//			}
			return;
		}

        approx = prev_desired - desired;

        if( approx < -0.001f || approx > 0.001f ) 
        {
			decel_active = false;
			prev_desired = desired;
		}
		
		// do we start decelerating.
		// time to stop. t = v0 / a
        float abs_speed = speed < 0f ? -speed : speed;
		float time_to_stop = abs_speed / decel;
		// d = vt * 1/2at^2
		float distance_required_to_stop = abs_speed * time_to_stop - 0.5f * decel * time_to_stop * time_to_stop;
		
		float raw_distance_left = WrapAngle(desired - val);
        float distance_left = raw_distance_left < 0f ? -raw_distance_left : raw_distance_left;
		float sign = (raw_distance_left > 0f) ? 1f : -1f;
		
		if( ( raw_distance_left > 0f && speed < 0f )
		   || ( raw_distance_left < 0f && speed > 0f ) ) 
        {
//			if(debug)
//			{
//				log.Debug("OTHERDECEL:abs_speed " + abs_speed + " : time_to_stop " + time_to_stop + " : dist_for_stop " + distance_required_to_stop + " : raw_left " + raw_distance_left + " : dist_left " + distance_left + " : sign " + sign);
//			}
			float spd_dec = decel * dt * sign * -1f;
			speed -= spd_dec;
		}
		else if( decel_active || ( time_to_stop > 0f && distance_required_to_stop >= distance_left ) ) {
//			if(debug)
//			{
//				log.Debug("DECEL:abs_speed " + abs_speed + " : time_to_stop " + time_to_stop + " : dist_for_stop " + distance_required_to_stop + " : raw_left " + raw_distance_left + " : dist_left " + distance_left + " : sign " + sign);
//			}
			float spd_dec = decel * dt * sign;
            float abs_spd_dec = spd_dec < 0f ? -spd_dec : spd_dec;
            if( decel_active && ( abs_spd_dec > abs_speed ) ) {
				speed *= 0.5f;
			}
			else {
				speed -= spd_dec;
			}
			decel_active = true;
		}
		else {
//			if(debug)
//			{
//				log.Debug("ACCEL:abs_speed " + abs_speed + " : time_to_stop " + time_to_stop + " : dist_for_stop " + distance_required_to_stop + " : raw_left " + raw_distance_left + " : dist_left " + distance_left + " : sign " + sign);
//			}
			speed += accel * dt * sign;
		}
        if (speed < -max_speed)
        {
            speed = -max_speed;
        }
        else if (speed > max_speed)
        {
            speed = max_speed;
        }

		float inc = speed * dt;
		val += inc;
		val = MathfExt.WrapAngle(val);
		float new_distance_left = WrapAngle(desired - val);			
        float abs_inc = inc < 0f ? -inc : inc;
        float abs_nd = new_distance_left < 0f ? -new_distance_left : new_distance_left;
		if( ( abs_nd <= abs_inc || distance_left <= abs_inc ) // make sure one of our distances left is less than our increment. we may be swaping signs across the singularity.
		     && ( ( new_distance_left >= 0 && raw_distance_left < 0f )
				||( new_distance_left <= 0 && raw_distance_left > 0f ) ) ) { // dont overshoot.
			val = desired;
			speed = 0f;
			prev_desired = desired;
			decel_active = false;
//			if( debug ) 
//            {
//				log.Debug("DONE");
//			}
		}

//		if( debug ) 
//        {
//			log.Debug("new_dist_left " + new_distance_left + " : abs_inc " + abs_inc + " : abd_nd " + abs_nd);
//			log.Debug("BOT:desired + " + desired + " : speed " + speed + " : val " + val);
//		}
	}
}
