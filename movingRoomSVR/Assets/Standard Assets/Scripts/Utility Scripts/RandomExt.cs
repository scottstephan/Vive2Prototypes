using UnityEngine;
//using System.coll;

public class RandomExt {	
	// this is slightly different from the standard Random class in that the max is inclusive as well.
	public static Vector3 VectorRange(float x_range, float y_range, float z_range)
	{
		float x = (Random.value - 0.5f) * x_range;
		float y = (Random.value - 0.5f) * y_range;
		float z = (Random.value - 0.5f) * z_range;
		
		return new Vector3( x, y, z );
	}
	
	public static Vector3 VectorRange(Vector3 size)
	{
		float x = (Random.value - 0.5f) * size.x;
		float y = (Random.value - 0.5f) * size.y;
		float z = (Random.value - 0.5f) * size.z;
		
		return new Vector3( x, y, z );
	}
	
	// this is slightly different from the standard Random class in that the max is inclusive as well.
	public static float FloatRange(float min, float max)
	{
		return ( min + ( Random.value * ( max - min ) ) );
	}

    public static float FloatRangePosNeg(float min, float max)
    {
        min = Mathf.Abs(min);
        max = Mathf.Abs(max);
        if (CoinFlip())
        {
            return FloatRange(min, max);
        }
        else
        {
            return FloatRange(-max, -min);
        }
    }

    // return value in -max to -min range, or min to max ranage
    public static float FloatRangeAbs(float min, float max)
    {
        if (CoinFlip())
        {
            return ( min + ( Random.value * ( max - min ) ) );
        }
        else
        {
            return ( -max + ( Random.value * ( (-min) - (-max) ) ) );
        }
    }

    public static bool CoinFlip()
    {
        return Random.Range(0, 2) == 0;
    }

	// this is slightly different from the standard Random class in that the max is inclusive as well.
	public static int IntRange(int min, int max)
	{
		return ( Random.Range( min, max + 1 ) );
	}

	// return a random number that is baised to a specific value within a range.
	public static float FloatWithBiasPower(float min, float max, float bias, float power)
	{
		// choose a random between 0 and 1
		float new_float = Random.value;
	
		// make sure our bias is between 0 and 1
		bias = Mathf.Clamp01( bias );
	
		// we always want to bias towards our bias! .. 
		if( new_float > bias )
		{
			float rescaled_float = ( new_float - bias ) / ( 1.0f - bias );
		
			new_float = bias + ( Mathf.Pow( rescaled_float, power ) * ( 1.0f - bias ) );
		}
		else if( new_float < bias )
		{
			float rescaled_float = new_float / bias;
		
			new_float = ( 1.0f - Mathf.Pow( 1.0f - rescaled_float, power ) ) * bias;
		}
	
		new_float = Mathf.Clamp01( new_float );
	
		return ( min + ( new_float * ( max - min ) ) );
	}

	// return a random number that is baised to a specific value within a range.
	public static float FloatWithRawBiasPower(float min, float max, float bias, float power)
	{
		bias = Mathf.Clamp(bias, min, max);
		return FloatWithBiasPower(min, max, Mathf.InverseLerp( min, max, bias ), power);
	}
}
