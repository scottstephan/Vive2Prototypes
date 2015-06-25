using UnityEngine;
using System.Collections;

public class GlowScreen : MonoBehaviour {
	
	float opacity = 0f;
	public float opacityInc = 0.03f;
	int sign = 1;
	Material myMaterial;
	Color myColor;
	bool prepSetOff = false;
	public bool isOn = false;
	public bool useWaldoSystem = false;	
		
	// Use this for initialization
	void Start () {
        myMaterial = GetComponent<Renderer>().sharedMaterial;
		myColor = myMaterial.GetColor("_TintColor");
		Color c = new Color(myColor.r, myColor.g, myColor.b, 0f);
		myMaterial.SetColor("_TintColor", c);
		
	}
	
	public void setGlowScreenOpacity(float newOpacity){
		opacity = newOpacity;
	}
	
	
	// Update is called once per frame
	void Update () 
	{
		if (useWaldoSystem){
			Color c = new Color(myColor.r, myColor.g, myColor.b, opacity);
			myMaterial.SetColor("_TintColor", c);			
		}
		else{
			if(isOn)
			{
				opacity+= opacityInc * sign;
				if(opacity>=1) sign = -1;
				if(opacity<=0)
				{
					sign = 1;
					if(prepSetOff) 
					{
						isOn = false;
						GetComponent<Renderer>().enabled = false;
					}
				}
				Color c = new Color(myColor.r, myColor.g, myColor.b, opacity);
				myMaterial.SetColor("_TintColor", c);
			}
		}
	}
	
	public void SetOff()
	{
		prepSetOff = true;
	}
	public void SetOn()
	{
		prepSetOff = false;
		GetComponent<Renderer>().enabled = true;
		isOn = true;
	}
}
