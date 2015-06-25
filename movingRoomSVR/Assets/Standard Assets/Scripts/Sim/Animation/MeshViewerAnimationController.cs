using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// this is independent animation controlller that is only used in preview scene for previweing animations.
/// </summary>
//[RequireComponent(typeof(Animation))]
public class MeshViewerAnimationController : MonoBehaviour {
	
	private string[] animationList;
	private int currentAnimationIndex;

	// Use this for initialization
	void Start () {	
		currentAnimationIndex = 0;
		GetComponent<Animation>().playAutomatically = true;
		GetComponent<Animation>().wrapMode = WrapMode.Loop;
		animationList = GetAnimationNames(GetComponent<Animation>());	
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log(animationList.Length);
		if(animationList.Length !=0)
		{
			if(!GetComponent<Animation>().IsPlaying(animationList[currentAnimationIndex]))
				GetComponent<Animation>().Play(animationList[currentAnimationIndex]);
			if(InputManager.GetKeyDown(KeyCode.A))
			{
				++currentAnimationIndex;
				if(currentAnimationIndex >= animationList.Length)
				{
					currentAnimationIndex = 0;
				}
			}
		}
	}
	string[] GetAnimationNames(Animation anim)
	{
		// make an Array that can grow
		List<string> tmpList = new List<string>();
	
		// enumerate all states
		foreach (AnimationState state in anim) {
			// add name to tmpList
			tmpList.Add(state.name);
		}
		// convert to (faster) buildin array (but can't grow anymore)
		string[] list = tmpList.ToArray();
		return list;
	}
	void OnGUI()
	{
		GUI.color = new Color( 0.0F,1.0F,0.0F);
		if(animationList.Length !=0)
		{
			GUI.Label(new Rect(Screen.width * 0.4f, Screen.height * 0.3f, 600,100), animationList[currentAnimationIndex] + " Time : " + GetComponent<Animation>().GetClip( animationList[currentAnimationIndex]).length);
		}
	}
}
