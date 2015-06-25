//**********************************************************************
//*	ONE DIMENTIONAL RANGE CLASS
//**********************************************************************
using UnityEngine;

//*************************
//* CLASS
//*************************
[System.Serializable]
public class Range {
		
	//*************************
	//* STATICS
	//*************************
		
	//*********************************
	//* CONSTANTS
	//*********************************

	//*************************
	//* VARIABLES
	//*************************
	public float from;
	public float to;
	
	//*************************
	//* CONSTRUCTOR
	//*************************
	public Range() {
			
		//*** Set Variables
		from = 0;
		to = 0;
    }
	public Range(float xFrom, float xTo) {
			
		//*** Set Variables
		from = xFrom;
		to = xTo;
	}

	//*************************
	//* GETTER / SETTER
	//*************************
	public float min{
        get { return from > to ? to : from; }
	}
	public float max{
        get { return from > to ? from : to; }
	}
	public float length{
			
        get {
			//*** Return Length (delta)
			return to - from;
        }
	}
	public float distance{
		get{
			//*** Return distance
			return Mathf.Abs(length);
        }
	}
	public float dir {

		get {	
		    //*** Return 
		    return Mathf.Sign(to - from);
        }
	}

	//*************************
	//* FUNCTIONS
	//*************************
	public float Random() {
		return UnityEngine.Random.Range(from, to);
	}
	public float Lerp(float xRatio) {
			
		//*** Return
		return from + (xRatio * length);
	}
	public override string ToString() {
			
		//*** Return
		return "Range: " + from.ToString() + " to " + to.ToString();
	}
	public Range Clone() {
		return new Range(from, to);			
	}
	public float Clamp(float xValue) {
		return Mathf.Max(Mathf.Min(xValue, max), min);
	}
	public bool In(float xValue) {
		return ((xValue >= min) && (xValue <= max));
	}
	public bool Between(float xValue) {
		return ((xValue > min) && (xValue < max));
	}
	public float Ratio(float xValue) {
		return (Clamp(xValue) - from) / (length);
	}
	public bool Intersect(float xFrom, float xTo) {
			
		//*** Check for overlap
		return !((xFrom > max) || (xTo < min));
	}
		
	public void ShiftBack() {
			
		//*** Populate from from to;
		from = to;
	}
	public void ShiftForward() {
			
		//*** Populate to from from;
		to = from;
	}
	public void Zero() {
			
		//*** Zero all values;
		from = 0;
		to = 0;
	}
	public void Set(float xValue) {
			
		//*** Set both values;
		from = xValue;
		to = xValue;
	}
		
	//*************************
	//* STATIC FUNCTIONS
	//*************************
	public static Range FromString(string xString) {
			
		//*** Variables
		Range oReturn;
			
		//*** Parse & clean string
		string[] aString = string.Join("", xString.Split(new char[]{' '}) ).Split(new char[]{','});
			
		//*** Make Point
		oReturn = new Range(float.Parse(aString[0]), float.Parse(aString[1]));
			
		//*** Return
		return oReturn;
	}
}
