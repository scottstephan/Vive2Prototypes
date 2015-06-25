//********************************************
//*	STATIC TWEEN HELPER CLASS
//********************************************

/********************************************
TERMS OF USE - EASING EQUATIONS
Open source under the BSD License.
Copyright (c)2001 Robert Penner
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
********************************************/

using UnityEngine;

//*****************************
//*	CLASS
//*****************************
public static class TweenHelper {

	//***************************************
	//* DELEGATES
	//***************************************
	public delegate float EaseMethod(float pFrom, float pTo, float pRatio);

	//***************************************
	//* ENUMS
	//***************************************
	public enum EaseType : int {
		Linear = 0,
		Spring = 1,
		
		QuadraticIn = 2,
		QuadraticOut = 3,
		QuadraticInOut = 4,
		
		CubicIn = 5,
		CubicOut = 6,
		CubicInOut = 7,
		
		QuarticIn = 8,
		QuarticOut = 9,
		QuarticInOut = 10,
		
		QuinticIn = 11,
		QuinticOut = 12,
		QuinticInOut = 13,
				
		TrigonometricIn = 14,
		TrigonometricOut = 15,
		TrigonometricInOut = 16,
		
		ExponentialIn = 17,
		ExponentialOut = 18,
		ExponentialInOut = 19,
		
		SqrtIn = 20,
		SqrtOut = 21,
		SqrtInOut = 22,
		
		BounceIn = 23,
		BounceOut = 24,
		BounceInOut = 25,
		
		BackIn = 26,
		BackOut = 27,
		BackInOut = 28,
		
		ElasticIn = 29,
		ElasticOut = 30,
		ElasticInOut = 31
	};

	//*****************************
	//*	STATICS
	//*****************************
	public static EaseMethod[] EaseMethods = new EaseMethod[]{
		Ease_Linear,
		Ease_Spring,
		
		Ease_Quadratic_In,
		Ease_Quadratic_Out,
		Ease_Quadratic_InOut,
		
		Ease_Cubic_In,
		Ease_Cubic_Out,
		Ease_Cubic_InOut,
		
		Ease_Quartic_In,
		Ease_Quartic_Out,
		Ease_Quartic_InOut,
		
		Ease_Quintic_In,
		Ease_Quintic_Out,
		Ease_Quintic_InOut,
		
		Ease_Trigonometric_In,
		Ease_Trigonometric_Out,
		Ease_Trigonometric_InOut,
		
		Ease_Exponential_In,
		Ease_Exponential_Out,
		Ease_Exponential_InOut,
		
		Ease_Sqrt_In,
		Ease_Sqrt_Out,
		Ease_Sqrt_InOut,
		
		Ease_Bounce_In,
		Ease_Bounce_Out,
		Ease_Bounce_InOut,
		
		Ease_Back_In,
		Ease_Back_Out,
		Ease_Back_InOut,
		
		Ease_Elastic_In,
		Ease_Elastic_Out,
		Ease_Elastic_InOut,
	};

	//*****************************
	//*	VARIABLES
	//*****************************

	//*****************************
	//*	MAIN METHODS
	//*****************************	

	//*****************************
	//*	EASING METHODS
	//*****************************	
	public static float Ease(float pFrom, float pTo, float pRatio, EaseType pType){
		
		//*** Process
		return EaseMethods[(int)pType](pFrom, pTo, pRatio);
	}	
	
	//*****************************
	//*	GENERAL EASING METHODS
	//*****************************	
	public static float Ease_Linear(float pFrom, float pTo, float pValue){
		
		//*** Return Linear Interpolation
		return Mathf.Lerp(pFrom, pTo, pValue);
	}
	public static float Ease_Spring(float pFrom, float pTo, float pValue){
		pValue = Mathf.Clamp01(pValue);
		pValue = (Mathf.Sin(pValue * Mathf.PI * (0.2f + 2.5f * pValue * pValue * pValue)) * Mathf.Pow(1f - pValue, 2.2f) + pValue) * (1f + (1.2f * (1f - pValue)));
		return pFrom + (pTo - pFrom) * pValue;
	}

	//*****************************
	//*	QUADRATIC EASING METHODS
	//*****************************	
	public static float Ease_Quadratic_In(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		return pTo * pValue * pValue + pFrom;
	}
	public static float Ease_Quadratic_Out(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		return -pTo * pValue * (pValue - 2) + pFrom;
	}
	public static float Ease_Quadratic_InOut(float pFrom, float pTo, float pValue){
		pValue /= .5f;
		pTo -= pFrom;
		if (pValue < 1) return pTo / 2 * pValue * pValue + pFrom;
		pValue--;
		return -pTo / 2 * (pValue * (pValue - 2) - 1) + pFrom;
	}

	//*****************************
	//*	CUBIC EASING METHODS
	//*****************************	
	public static float Ease_Cubic_In(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		return pTo * pValue * pValue * pValue + pFrom;
	}
	public static float Ease_Cubic_Out(float pFrom, float pTo, float pValue){
		pValue--;
		pTo -= pFrom;
		return pTo * (pValue * pValue * pValue + 1) + pFrom;
	}
	public static float Ease_Cubic_InOut(float pFrom, float pTo, float pValue){
		pValue /= .5f;
		pTo -= pFrom;
		if (pValue < 1) return pTo / 2 * pValue * pValue * pValue + pFrom;
		pValue -= 2;
		return pTo / 2 * (pValue * pValue * pValue + 2) + pFrom;
	}

	//*****************************
	//*	QUATERNION EASING METHODS
	//*****************************	
	public static float Ease_Quartic_In(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		return pTo * pValue * pValue * pValue * pValue + pFrom;
	}
	public static float Ease_Quartic_Out(float pFrom, float pTo, float pValue){
		pValue--;
		pTo -= pFrom;
		return -pTo * (pValue * pValue * pValue * pValue - 1) + pFrom;
	}
	public static float Ease_Quartic_InOut(float pFrom, float pTo, float pValue){
		pValue /= .5f;
		pTo -= pFrom;
		if (pValue < 1) return pTo / 2 * pValue * pValue * pValue * pValue + pFrom;
		pValue -= 2;
		return -pTo / 2 * (pValue * pValue * pValue * pValue - 2) + pFrom;
	}

	//*****************************
	//*	QUINTOUPLE EASING METHODS
	//*****************************	
	public static float Ease_Quintic_In(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		return pTo * pValue * pValue * pValue * pValue * pValue + pFrom;
	}
	public static float Ease_Quintic_Out(float pFrom, float pTo, float pValue){
		pValue--;
		pTo -= pFrom;
		return pTo * (pValue * pValue * pValue * pValue * pValue + 1) + pFrom;
	}
	public static float Ease_Quintic_InOut(float pFrom, float pTo, float pValue){
		pValue /= .5f;
		pTo -= pFrom;
		if (pValue < 1) return pTo / 2 * pValue * pValue * pValue * pValue * pValue + pFrom;
		pValue -= 2;
		return pTo / 2 * (pValue * pValue * pValue * pValue * pValue + 2) + pFrom;
	}

	//*****************************
	//*	POWER EASING METHODS
	//*****************************	
	public static float Ease_Power_In(float pFrom, float pTo, float pValue, float pPower){
		pTo -= pFrom;
		return pTo * Mathf.Pow(pValue, pPower) + pFrom;
	}
	public static float Ease_Power_Out(float pFrom, float pTo, float pValue, float pPower){
		pValue--;
		pTo -= pFrom;
		return pTo * (Mathf.Pow(pValue, pPower) + 1) + pFrom;
	}
	public static float Ease_Power_InOut(float pFrom, float pTo, float pValue, float pPower){
		pValue /= .5f;
		pTo -= pFrom;
		if (pValue < 1) return pTo / 2 * Mathf.Pow(pValue, pPower) + pFrom;
		pValue -= 2;
		return pTo / 2 * (Mathf.Pow(pValue, pPower) + 2) + pFrom;
	}

	//*****************************
	//*	TRIGONOMETRIC EASING METHODS
	//*****************************	
	public static float Ease_Trigonometric_In(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		return -pTo * Mathf.Cos(pValue / 1 * (Mathf.PI / 2)) + pTo + pFrom;
	}
	public static float Ease_Trigonometric_Out(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		return pTo * Mathf.Sin(pValue / 1 * (Mathf.PI / 2)) + pFrom;
	}
	public static float Ease_Trigonometric_InOut(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		return -pTo / 2 * (Mathf.Cos(Mathf.PI * pValue / 1) - 1) + pFrom;
	}

	//*****************************
	//*	EXPONENTIAL EASING METHODS
	//*****************************	
	public static float Ease_Exponential_In(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		return pTo * Mathf.Pow(2, 10 * (pValue / 1 - 1)) + pFrom;
	}
	public static float Ease_Exponential_Out(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		return pTo * (-Mathf.Pow(2, -10 * pValue / 1) + 1) + pFrom;
	}
	public static float Ease_Exponential_InOut(float pFrom, float pTo, float pValue){
		pValue /= .5f;
		pTo -= pFrom;
		if (pValue < 1) return pTo / 2 * Mathf.Pow(2, 10 * (pValue - 1)) + pFrom;
		pValue--;
		return pTo / 2 * (-Mathf.Pow(2, -10 * pValue) + 2) + pFrom;
	}

	//*****************************
	//*	SQUARE ROUTE EASING METHODS
	//*****************************	
	public static float Ease_Sqrt_In(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		return -pTo * (Mathf.Sqrt(1 - pValue * pValue) - 1) + pFrom;
	}
	public static float Ease_Sqrt_Out(float pFrom, float pTo, float pValue){
		pValue--;
		pTo -= pFrom;
		return pTo * Mathf.Sqrt(1 - pValue * pValue) + pFrom;
	}
	public static float Ease_Sqrt_InOut(float pFrom, float pTo, float pValue){
		pValue /= .5f;
		pTo -= pFrom;
		if (pValue < 1) return -pTo / 2 * (Mathf.Sqrt(1 - pValue * pValue) - 1) + pFrom;
		pValue -= 2;
		return pTo / 2 * (Mathf.Sqrt(1 - pValue * pValue) + 1) + pFrom;
	}

	//*****************************
	//*	BOUNCE EASING METHODS
	//*****************************	
	public static float Ease_Bounce_In(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		float d = 1f;
		return pTo - Ease_Bounce_Out(0, pTo, d-pValue) + pFrom;
	}
	public static float Ease_Bounce_Out(float pFrom, float pTo, float pValue){
		pValue /= 1f;
		pTo -= pFrom;
		if (pValue < (1 / 2.75f)){
			return pTo * (7.5625f * pValue * pValue) + pFrom;
		}else if (pValue < (2 / 2.75f)){
			pValue -= (1.5f / 2.75f);
			return pTo * (7.5625f * (pValue) * pValue + .75f) + pFrom;
		}else if (pValue < (2.5 / 2.75)){
			pValue -= (2.25f / 2.75f);
			return pTo * (7.5625f * (pValue) * pValue + .9375f) + pFrom;
		}else{
			pValue -= (2.625f / 2.75f);
			return pTo * (7.5625f * (pValue) * pValue + .984375f) + pFrom;
		}
	}
	public static float Ease_Bounce_InOut(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		float d = 1f;
		if (pValue < d/2) return Ease_Bounce_In(0, pTo, pValue*2) * 0.5f + pFrom;
		else return Ease_Bounce_Out(0, pTo, pValue*2-d) * 0.5f + pTo*0.5f + pFrom;
	}

	//*****************************
	//*	BACK EASING METHODS
	//*****************************	
	public static float Ease_Back_In(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		pValue /= 1;
		float s = 1.70158f;
		return pTo * (pValue) * pValue * ((s + 1) * pValue - s) + pFrom;
	}
	public static float Ease_Back_Out(float pFrom, float pTo, float pValue){
		float s = 1.70158f;
		pTo -= pFrom;
		pValue = (pValue / 1) - 1;
		return pTo * ((pValue) * pValue * ((s + 1) * pValue + s) + 1) + pFrom;
	}
	public static float Ease_Back_InOut(float pFrom, float pTo, float pValue){
		float s = 1.70158f;
		pTo -= pFrom;
		pValue /= .5f;
		if ((pValue) < 1){
			s *= (1.525f);
			return pTo / 2 * (pValue * pValue * (((s) + 1) * pValue - s)) + pFrom;
		}
		pValue -= 2;
		s *= (1.525f);
		return pTo / 2 * ((pValue) * pValue * (((s) + 1) * pValue + s) + 2) + pFrom;
	}

	//*****************************
	//*	PUNCH EASING METHODS
	//*****************************	
	public static float Ease_Punch(float amplitude, float pValue){
		float s = 9;
		if (pValue == 0){
			return 0;
		}
		if (pValue == 1){
			return 0;
		}
		float period = 1 * 0.3f;
		s = period / (2 * Mathf.PI) * Mathf.Asin(0);
		return (amplitude * Mathf.Pow(2, -10 * pValue) * Mathf.Sin((pValue * 1 - s) * (2 * Mathf.PI) / period));
    }

	//*****************************
	//*	ELASTIC EASING METHODS
	//*****************************	
	public static float Ease_Elastic_In(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		
		float d = 1f;
		float p = d * .3f;
		float s = 0;
		float a = 0;
		
		if (pValue == 0) return pFrom;
		
		if ((pValue /= d) == 1) return pFrom + pTo;
		
		if (a == 0f || a < Mathf.Abs(pTo)){
			a = pTo;
			s = p / 4;
			}else{
			s = p / (2 * Mathf.PI) * Mathf.Asin(pTo / a);
		}
		
		return -(a * Mathf.Pow(2, 10 * (pValue-=1)) * Mathf.Sin((pValue * d - s) * (2 * Mathf.PI) / p)) + pFrom;
	}		
	public static float Ease_Elastic_Out(float pFrom, float pTo, float pValue){
		
		pTo -= pFrom;
		
		float d = 1f;
		float p = d * .3f;
		float s = 0;
		float a = 0;
		
		if (pValue == 0) return pFrom;
		
		if ((pValue /= d) == 1) return pFrom + pTo;
		
		if (a == 0f || a < Mathf.Abs(pTo)){
			a = pTo;
			s = p / 4;
			}else{
			s = p / (2 * Mathf.PI) * Mathf.Asin(pTo / a);
		}
		
		return (a * Mathf.Pow(2, -10 * pValue) * Mathf.Sin((pValue * d - s) * (2 * Mathf.PI) / p) + pTo + pFrom);
	}		
	public static float Ease_Elastic_InOut(float pFrom, float pTo, float pValue){
		pTo -= pFrom;
		
		float d = 1f;
		float p = d * .3f;
		float s = 0;
		float a = 0;
		
		if (pValue == 0) return pFrom;
		
		if ((pValue /= d/2) == 2) return pFrom + pTo;
		
		if (a == 0f || a < Mathf.Abs(pTo)){
			a = pTo;
			s = p / 4;
			}else{
			s = p / (2 * Mathf.PI) * Mathf.Asin(pTo / a);
		}
		
		if (pValue < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (pValue-=1)) * Mathf.Sin((pValue * d - s) * (2 * Mathf.PI) / p)) + pFrom;
		return a * Mathf.Pow(2, -10 * (pValue-=1)) * Mathf.Sin((pValue * d - s) * (2 * Mathf.PI) / p) * 0.5f + pTo + pFrom;
	}		
}
