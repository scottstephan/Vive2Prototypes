// Copyright (c) 2014 Nathan Martz

using UnityEngine;
using System.Collections;

/// \ingroup Core
/// Implements a basic door component that is Portal aware. Also, provides an interface
/// that more complex doors can implement.
/// 
/// This door contains two base states (Open and Closed) and two transitional states (Opening and
/// Closing). The animations for Open and Closed should be Looping animations, with one-shot
/// animations for the transitions.
/// 
/// Door supports an optional reference to a SECTR_Portal. If set, the Door will manage the Closed
/// flag of the Portal, which other systems will find useful. 
#if UNITY_3_5
[RequireComponent(typeof(Animation))]
#else
[RequireComponent(typeof(Animator))]
#endif
[AddComponentMenu("SECTR/Audio/SECTR Door")]
public class SECTR_Door : MonoBehaviour 
{
	#region Private Details
#if UNITY_3_5
	enum DoorState
	{
		Open,
		Closed,
		Opening,
		Closing,
	}
	
	private DoorState state = DoorState.Closed;
	private string openName = "Open";
	private string closingName = "Closing";
	private string closedName = "Closed";
	private string openingName = "Opening";
	private Animation cachedAnimation = null;
#else
	private int controlParam = 0;
	private int canOpenParam = 0;
	private int closedState = 0;
	private int waitingState = 0;
	private int openingState = 0;
	private int openState = 0;
	private int closingState = 0;
	private int lastState = 0;
	private Animator cachedAnimator = null;
#endif

	private int openCount = 0;
	#endregion

	[SECTR_ToolTip("The portal this door affects (if any).")]
	public SECTR_Portal Portal = null;

#if UNITY_3_5
	[SECTR_ToolTip("The animation to loop while open.")]
	public AnimationClip OpenLoop;
	[SECTR_ToolTip("The animation to loop while closed.")]
	public AnimationClip ClosedLoop;
	[SECTR_ToolTip("The animation to play when going from closed to open.")]
	public AnimationClip Opening;
	[SECTR_ToolTip("The animation to play when going from open to closed.")]
	public AnimationClip Closing;
#else
	[SECTR_ToolTip("The name of the control param in the door.")]
	public string ControlParam = "Open";
	[SECTR_ToolTip("The name of the control param that indicates if we are allowed to open.")]
	public string CanOpenParam = "CanOpen";
	[SECTR_ToolTip("The full name (layer and state) of the Open state in the Animation Controller.")]
	public string OpenState = "Base Layer.Open";
	[SECTR_ToolTip("The full name (layer and state) of the Closed state in the Animation Controller.")]
	public string ClosedState = "Base Layer.Closed";
	[SECTR_ToolTip("The full name (layer and state) of the Opening state in the Animation Controller.")]
	public string OpeningState = "Base Layer.Opening";
	[SECTR_ToolTip("The full name (layer and state) of the Closing state in the Animation Controller.")]
	public string ClosingState = "Base Layer.Closing";
	[SECTR_ToolTip("The full name (layer and state) of the Wating state in the Animation Controller.")]
	public string WaitingState = "Base Layer.Waiting";
#endif

	/// Opens the door. Exposed for use by other script classes.
	public void OpenDoor()
	{
		++openCount;
	}
	
	/// Closes the door. Exposed for use by other script classes.
	public void CloseDoor()
	{
		--openCount;
	}
	
	public bool IsFullyOpen()
	{
#if UNITY_3_5
		return state == DoorState.Open;
#else
		AnimatorStateInfo info = cachedAnimator.GetCurrentAnimatorStateInfo(0);
		return info.nameHash == openState;
#endif
	}

	public bool IsClosed()
	{
		#if UNITY_3_5
		return state == DoorState.Closed;
		#else
		AnimatorStateInfo info = cachedAnimator.GetCurrentAnimatorStateInfo(0);
		return info.nameHash == closedState;
		#endif
	}

	#region Unity Interface
#if UNITY_3_5
	protected virtual void OnEnable()
	{
		cachedAnimation = animation;
		if(OpenLoop)
		{
			cachedAnimation.AddClip(OpenLoop, openName);
		}
		if(ClosedLoop)
		{
			cachedAnimation.AddClip(ClosedLoop, closedName);
		}
		if(Opening)
		{
			cachedAnimation.AddClip(Opening, openingName);
		}
		if(Closing)
		{
			cachedAnimation.AddClip(Closing, closingName);
		}
	}

	void Start()
	{
		cachedAnimation.wrapMode = WrapMode.Loop;
		cachedAnimation.Play(closedName);
		state = DoorState.Closed;
		if(Portal)
		{
			Portal.SetFlag(SECTR_Portal.PortalFlags.Closed, true);
		}
		openCount = 0;
		SendMessage("OnClose", SendMessageOptions.DontRequireReceiver);
	}

	void Update()
	{
		switch(state)
		{
		case DoorState.Closed:
			if(openCount > 0 && CanOpen())
			{
				cachedAnimation.wrapMode = WrapMode.Once;
				cachedAnimation.Play(openingName);
				if(Portal)
				{
					Portal.SetFlag(SECTR_Portal.PortalFlags.Closed, false);
				}
				state = DoorState.Opening;
				SendMessage("OnOpening", SendMessageOptions.DontRequireReceiver);
			}
			break;
		case DoorState.Opening:
			if(!cachedAnimation.IsPlaying(openingName))
			{
				cachedAnimation.wrapMode = WrapMode.Loop;
				cachedAnimation.Play(openName);
				if(Portal)
				{
					Portal.SetFlag(SECTR_Portal.PortalFlags.Closed, false);
				}
				state = DoorState.Open;
				SendMessage("OnOpen", SendMessageOptions.DontRequireReceiver);
			}
			break;
		case DoorState.Open:
			if(openCount == 0)
			{
				cachedAnimation.wrapMode = WrapMode.Once;
				cachedAnimation.Play(closingName);
				if(Portal)
				{
					Portal.SetFlag(SECTR_Portal.PortalFlags.Closed, false);
				}
				state = DoorState.Closing;
				SendMessage("OnClosing", SendMessageOptions.DontRequireReceiver);
			}
			break;
		case DoorState.Closing:
			if(!cachedAnimation.IsPlaying(closingName))
			{
				cachedAnimation.wrapMode = WrapMode.Loop;
				cachedAnimation.Play(closedName);
				if(Portal)
				{
					Portal.SetFlag(SECTR_Portal.PortalFlags.Closed, true);
				}
				state = DoorState.Closed;
				SendMessage("OnClose", SendMessageOptions.DontRequireReceiver);
			}
			break;
		}
	}
#else
	protected virtual void OnEnable()
	{
		cachedAnimator = GetComponent<Animator>();

		controlParam = Animator.StringToHash(ControlParam);
		canOpenParam = Animator.StringToHash(CanOpenParam);
		closedState = Animator.StringToHash(ClosedState);
		waitingState = Animator.StringToHash(WaitingState);
		openingState = Animator.StringToHash(OpeningState);
		openState = Animator.StringToHash(OpenState);
		closingState = Animator.StringToHash(ClosingState);
	}

	void Start()
	{
		if(controlParam != 0)
		{
			cachedAnimator.SetBool(controlParam, false);
		}
		if(canOpenParam != 0)
		{
			cachedAnimator.SetBool(canOpenParam, false);
		}
		if(Portal)
		{
			Portal.SetFlag(SECTR_Portal.PortalFlags.Closed, true);
		}
		openCount = 0;
		lastState = closedState;
		SendMessage("OnClose", SendMessageOptions.DontRequireReceiver);
	}
	
	void Update()
	{
		bool canOpen = CanOpen();
		if(canOpenParam != 0)
		{
			cachedAnimator.SetBool(canOpenParam, canOpen);
		}
		if(controlParam != 0 && (canOpen || canOpenParam != 0))
		{
			if(openCount > 0)
			{
				cachedAnimator.SetBool(controlParam, true);
			}
			else
			{
				cachedAnimator.SetBool(controlParam, false);
			}
		}

		int currentState = cachedAnimator.GetCurrentAnimatorStateInfo(0).nameHash;
		if(currentState != lastState)
		{
			if(currentState == closedState)
			{
				SendMessage("OnClose", SendMessageOptions.DontRequireReceiver);
			}
			if(currentState == waitingState)
			{
				SendMessage("OnWaiting", SendMessageOptions.DontRequireReceiver);
			}
			else if(currentState == openingState)
			{
				SendMessage("OnOpening", SendMessageOptions.DontRequireReceiver);
			}
			if(currentState == openState)
			{
				SendMessage("OnOpen", SendMessageOptions.DontRequireReceiver);
			}
			else if(currentState == closingState)
			{
				SendMessage("OnClosing", SendMessageOptions.DontRequireReceiver);
			}
			lastState = currentState;
		}
		
		if(Portal)
		{
			Portal.SetFlag(SECTR_Portal.PortalFlags.Closed, IsClosed());
		}
	}
#endif

	protected virtual void OnTriggerEnter(Collider other)
	{
		++openCount;
	}
	
	protected virtual void OnTriggerExit(Collider other)
	{
		--openCount;
	}
	#endregion

	#region Door Interface
	// For subclasses to override
	protected virtual bool CanOpen()
	{
		return true;
	}
	#endregion
}
