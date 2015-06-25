using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WemoItemData : MonoBehaviour {
    public bool vrInteractable = false;

    public delegate void OnVRInteract();

	public Vector3 vrIconOffset = new Vector3( 0f, 2f, 0f );
	public Vector3 vrIconScale = new Vector3( 1f, 1f, 1f );
	private Vector3 ogLocalScale;
    private GameObject icon = null;
    private Transform myTransform = null;
    private Transform iconTransform = null;

    public OnVRInteract vrInteractCallback;

    // TODO>drive this data from the db.
    public string speciesName = "SPECIES NAME";
    public string makerNames = "Model, Texture, Rig, Animation";
    
    public AudioClip voiceInfoClip = null;
    public AudioClipContainer voiceInfoClipList = null;
    public AudioClipContainer specificVoiceInfoClipList = null;

	public int itemID = -1;
	public int variantID = -1;
	
    public float cooldownTime = 0f;
    private float cooldownTimer = 0f;

    private bool isSpeciesFirstInteract;

    // hovering effect
    public bool useHoverEffect = false;
    public bool hoverEffectUseCritterSize = true;
    public float hoverEffectScale = 1f;
    public Vector3 hoverEffectOffset = Vector3.zero;
    private GameObject hoverEffect = null;
    private ParticleSystem hoverParticles = null;
    private Transform hoverTransform = null;

	public GameObject hoverObj;

    [HideInInspector]
    public bool hoveringActive = false;
    private float hoveringTime = 0f;
	private bool hoveringState = false; // similar to hoveringActive but doesn't reset every frame, only when there is a change

	[HideInInspector]
	public string buildInfoString = null;

    [HideInInspector]
    public CritterInfo critterInfo = null;

    [HideInInspector]
    const float AUDIO_PASSBY_MINTIME = 10f;
    public float audioLastPassbyTime = -AUDIO_PASSBY_MINTIME;


	public bool wasClicked = false;
	public ParticleSystem fxHover;
	public GameObject hoverGlow;
	public float hoverGlowDirection = 1f;


    //private Material hoverMat = null;
	//public GameObject fxTrail;
	//public Material outlineMaterial;
	//public Material origMaterial;
	//public GameObject outlineShape;

    public int myEducationalMode = 0;

    private static GameObject fishNameObj = null;
    private static FishNameUI fishNameUI = null;
    private static Transform fishNameTransform = null;
    const float FishNamePanelDistance = 70f;
    const float FishNamePanelCloseDistance = 80f;
    const float FishNamePanelFarDistance = 150f;
    const float BottomOffsetScale = 1.3f;

    void Start() {
        if( fishNameUI == null ) {
            fishNameObj = GameObject.Find("FishNameUI");
            if( fishNameObj != null ) {
                fishNameTransform = fishNameObj.transform;
                fishNameUI = fishNameObj.GetComponent<FishNameUI>();
                fishNameObj.SetActive( false );
            }
        }

        myTransform = gameObject.transform;
//        if( hoverGlow != null ) {
//           	hoverMat = hoverGlow.renderer.material;
//        }

        if( App.UserManager != null ) {
            myEducationalMode = App.UserManager.educationalMode;
        }

//		if(outlineShape != null)
//			outlineShape.SetActive(false);

    }

    // called from within the SwimPlayerView motion LateUpdate. Ensures,
    public void SetFishNameUI() {
        if( fishNameUI == null ) {
            return;
        }

        fishNameObj.SetActive( true );

        if (isSpeciesFirstInteract)
        {
            isSpeciesFirstInteract = false;
            // keywords: localization loctodo loc todo
            fishNameUI.SetName(speciesName + " (NEW!)");
        }
        else
        {
            fishNameUI.SetName(speciesName);
        }

        // make sure we are positioned properly.
        UpdateFishNameUI();
    }

    public void UpdateFishNameUI() {
        if( fishNameUI == null ) {
            return;
        }

        // position our name object.
        Vector3 bot = critterInfo.cachedPosition - ( critterInfo.generalMotionData.critterBoxColliderRadius * BottomOffsetScale * Vector3.up );
        Vector3 cam = CameraManager.GetCurrentCameraPosition();
        Vector3 fwd = CameraManager.GetCurrentCameraForward();
        Vector3 off = bot - cam;

        if( Vector3.Dot( fwd, off ) < ( FishNamePanelDistance * 0.1f ) ) {
            fishNameObj.SetActive( false );
            return;
        }

        fishNameObj.SetActive( true );

        float dist = off.magnitude;
        if( dist > FishNamePanelFarDistance ) {
            off *=  FishNamePanelFarDistance / dist;
        }
        else if( dist < FishNamePanelCloseDistance ) {
            off *=  FishNamePanelCloseDistance / dist;
        }
        bot = cam + off;
        fishNameTransform.position = bot;
        off.y *= 0.5f; // keep some y rot towards camera for up/down movement, but not full to reduce gimbal lock-y problems and looking weird on top of fish
        fishNameTransform.rotation = Quaternion.LookRotation( off );
    }

    public void TurnFishNameUIOff() {
        if( fishNameUI == null ) {
            return;
        }
        
        fishNameObj.SetActive( false );
    }

    public void SetVRInteractable( bool b, OnVRInteract callback = null ) {
        if (vrInteractable != b &&
            critterInfo != null &&
            critterInfo.swimDisperseData != null)
        {
            if (b)
            {
                critterInfo.swimDisperseData.playerDisperseDisableCount++;
            }
            else
            {
                critterInfo.swimDisperseData.playerDisperseDisableCount--;               
            }
        }

        vrInteractable = b;
        vrInteractCallback = callback;
        wasClicked = false;
//		if(fxTrail != null){
//			fxTrail.SetActive(!b);
//		}
    }
    
    public void SetInfoClip( AudioClip ac ) {
        voiceInfoClip = ac;
    }
    public void SetInfoClipList( AudioClipContainer acc ) {
        voiceInfoClipList = acc;
    }

    public void SetIconOffset( Vector3 new_offset ) {
        vrIconOffset = new_offset;
    }
    
    public void SetCooldownTime( float new_cooldown_time ) {
        cooldownTime = new_cooldown_time;
    }
    
    public bool Hovering() {
        if( !vrInteractable ) {
            hoveringActive = false;
            return false;        
        }

        if( myEducationalMode == 0 )
        {
            hoveringActive = false;
            return false;
        }

        hoveringActive = true;
		if(!wasClicked) {
			SetHoverShader(true);
			return true;
		}

		return false;
    }

    void PlayVoice() 
    {
        if ( voiceInfoClip != null ) 
        {
            AudioManager.Instance.PlayInfoVoiceClip( voiceInfoClip );
        }

        // both specific and general lists.
        else if ( specificVoiceInfoClipList != null 
                && specificVoiceInfoClipList.HasRandomClips
                && voiceInfoClipList != null 
                && voiceInfoClipList.HasRandomClips )
        {
            int tc = specificVoiceInfoClipList.RandomClips.Length + voiceInfoClipList.RandomClips.Length;

            int rnd = Random.Range(0,tc);

            AudioClipContainer clip_list = null;

            if( rnd <= specificVoiceInfoClipList.RandomClips.Length ) {
                clip_list = specificVoiceInfoClipList;
            }
            else {
                clip_list = voiceInfoClipList;
            }

            AudioClip ac = clip_list.GetRandomClip();
            AudioManager.Instance.PlayInfoVoiceClip( ac );
        }
        // only specific list
        else if ( specificVoiceInfoClipList != null 
                 && specificVoiceInfoClipList.HasRandomClips )
        {
            AudioClip ac = specificVoiceInfoClipList.GetRandomClip();
            AudioManager.Instance.PlayInfoVoiceClip( ac );
        }
        // only general list
        else if ( voiceInfoClipList != null 
                 && voiceInfoClipList.HasRandomClips)
        {
            AudioClip ac = voiceInfoClipList.GetRandomClip();
            AudioManager.Instance.PlayInfoVoiceClip( ac );
        }
    }

    bool UseDefaultInteract()
    {
        if (myEducationalMode == 0 ||
            critterInfo == null ||
            critterInfo.swimPlayerViewData == null)
        {
            return false;
        }

        // check if we can't interrupt
/*
        if (critterInfo.generalSpeciesData.myCurrentBehaviorType != SwimBehaviorType.SWIM_DISPERSE &&
            critterInfo.generalSpeciesData.myCurrentBehavior != null &&
            critterInfo.generalSpeciesData.myCurrentBehavior.EvaluatePriority(critterInfo) >  critterInfo.swimPlayerViewData.priorityValue)
        {
            return false;
        }
*/
        return true;
    }

    public bool TriggerInteract() 
    {
/*        if( !vrInteractable )
        {
            if (UseDefaultInteract())
            {
                AudioManager.Instance.PlayAudio( (int) SoundFXID.WaldoBeep );
                SwimInPlayerView();
                return true;
            }

            return false;
        }*/

		wasClicked = true;
		SetHoverShader(false);
		
        cooldownTimer = cooldownTime;

        if (vrInteractCallback != null)
        {
            vrInteractCallback();
            vrInteractCallback = null; // only do this once
        }

        // Turn off interactable now that it's been used. Use function to make sure player disperse on critter is enabled/disabled correctly
        SetVRInteractable(false, null); // only call callback first time

		return true;
    }

    void ClearCritterBehavior()
    {
        // for critters out of scripted singleton behaviors now to interact
        if( critterInfo.generalSpeciesData != null &&
            critterInfo.generalSpeciesData.myCurrentBehaviorType == SwimBehaviorType.CIRCLE_AROUND_OBJECT )
        {
            AI.ForceSwitchToBehavior( critterInfo, SwimBehaviorType.SWIM_FREE );
        }
    }

    void SwimInPlayerView()
    {
        if (critterInfo == null ||
            critterInfo.swimPlayerViewData == null)
        {
            return;
        }

        if (critterInfo.generalSpeciesData.myCurrentBehaviorType != SwimBehaviorType.SWIM_PLAYER_VIEW)
        {
            critterInfo.swimPlayerViewData.startMe = true;            
            critterInfo.generalMotionData.isDispersed = false;
            critterInfo.generalMotionData.isBeingChased = false;
            if (critterInfo.generalSpeciesData.myCurrentBehavior.IsSingletonBehavior())
            {
                critterInfo.swimPlayerViewData.prevSingletonBehavior = (int)critterInfo.generalSpeciesData.myCurrentBehaviorType;
            }

            AI.ForceSwitchToBehavior(critterInfo, SwimBehaviorType.SWIM_PLAYER_VIEW);
        }
    }

    public void DONOTUpdate() {
        if( !vrInteractable && cooldownTimer > 0.00001f ) {
            cooldownTimer -= Time.deltaTime;
            if( cooldownTimer <= 0.00001f ) {
                SetVRInteractable(true, vrInteractCallback);
            }
        }

        if( App.UserManager != null && 
           myEducationalMode != App.UserManager.educationalMode ) {
            myEducationalMode = App.UserManager.educationalMode;
        }

        // udpate passby audio trigger
        if (!SimInstance.Instance.slowdownActive &&
            !SimInstance.Instance.IsSimPaused() &&
            critterInfo.audioData != null &&
            critterInfo.audioData.passbyClip != null &&
            Time.time-audioLastPassbyTime > AUDIO_PASSBY_MINTIME &&
            critterInfo.generalSpeciesData.myCurrentBehaviorType != SwimBehaviorType.SWIM_FOLLOWPATH)
        {
            Vector3 camPos = CameraManager.GetCurrentCameraPosition();
                       
            Vector3 critterToCamDir = camPos - critterInfo.cachedPosition;
            critterToCamDir.y = 0f;
            
            float critterToCamDist = critterToCamDir.magnitude;
            if (critterInfo.audioData.passbyDistMax < 0f || 
                critterToCamDist <= critterInfo.audioData.passbyDistMax)
            {
                if (critterToCamDist > 0)
                {
                    critterToCamDir /= critterToCamDist;
                }
            
                Vector3 camDir = CameraManager.GetCurrentCameraFlattenedForward();
                float dot = Vector3.Dot (critterToCamDir, camDir);
            
                if (dot < -0.85f)
                {
                    AudioManager.PlayCritterPassby(critterInfo);
                    audioLastPassbyTime = Time.time;
                }
            }
        }
    }

    void DONOTLateUpdate() {
        // update our rotation .. doing this late so that we have the latest camera orientations applied.
        if( iconTransform != null ) {
//            Transform cam_trans = CameraManager.GetCurrentCameraTransform();
			// since we are not using billboards anymore . the rotation should be the fish rotation for the particle system to work
			iconTransform.rotation = myTransform.rotation ;//cam_trans.rotation;//Quaternion.LookRotation( cam_trans.forward, cam_trans.up );
			iconTransform.position = myTransform.position + myTransform.forward*vrIconOffset.z + myTransform.right*vrIconOffset.x + Vector3.up*vrIconOffset.y;
			
			Vector3 toCam = Camera.main.transform.position - iconTransform.position;
			if(hoverGlow != null){
				iconTransform.position += toCam.normalized * hoverGlow.transform.localScale.z * 1.2f ;
				Vector3 camDiff = iconTransform.position - Camera.main.transform.position;
				float camDist = camDiff.magnitude;

				iconTransform.localScale = Vector3.one * ( 0.05f + 0.95f * Mathf.Clamp(camDist/40f, 0f, 1f));

				if(camDist < 2f)
					icon.SetActive(false);
				else
					icon.SetActive(true);
			}

			//iconTransform.localScale = ogLocalScale;

			if (hoverObj != null) {

				if (hoveringActive)
					hoverObj.SetActive(true);
				else 
					hoverObj.SetActive(false);
			}
            else if( hoveringActive ) {
                //iconTransform.localScale *= 2f + ( 0.15f * Mathf.Sin( hoveringTime * 15f ) );
                hoveringTime += Time.deltaTime;

                if( hoverTransform != null ) {                    
                    hoverTransform.position = myTransform.position + hoverEffectOffset;
                }
                if( hoverParticles != null && !hoverParticles.isPlaying ) {
                    hoverParticles.time = 0f;
                    hoverParticles.Clear();
                    hoverParticles.Play();
                }
            }
            else {
                hoveringTime = 0f;
				SetHoverShader(false);

                if( hoverParticles != null && hoverParticles.isPlaying ) {
                    hoverParticles.Stop();
                }
            }
        }

		
/*		if( hoverGlow != null ) {
            Color col = hoverMat.color;
			float alpha = col.a;
			if(hoveringActive){
				alpha += Time.deltaTime * 2f * hoverGlowDirection;
				if(alpha >= 1f || alpha <= 0f)
					hoverGlowDirection *= -1f;
			}
			else
				alpha -= Time.deltaTime * 2f;
			if(alpha < 0f)
				hoverGlow.SetActive(false);
			alpha = Mathf.Clamp(alpha, 0f, 1f);
			if(!hoverGlow.activeSelf && alpha > 0f)
				hoverGlow.SetActive(true);
            hoverMat.color = new Color(col.r, col.g, col.b, alpha);
		}*/
			
        hoveringActive = false;
    }

	private void SetHoverShader(bool glow){
		if(glow == hoveringState)
			return; // don't do anything if there is no change in state.... changing shader could be expansive

		/*
		Shader fs = GlobalOceanShaderAdjust.Instance.lodStyleData[(int)GlobalOceanShaderAdjust.Instance.lodStyle].fishFlashShader;
		Shader sh = GlobalOceanShaderAdjust.Instance.lodStyleData[(int)GlobalOceanShaderAdjust.Instance.lodStyle].shader;
		LODLevel currentLod = critterInfo.critterLODData.curLOD;
		foreach(LODLevel lod in critterInfo.critterLODData.LODs){
			lod.LOD.SetActive(true);
		}
		SkinnedMeshRenderer[] smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach(SkinnedMeshRenderer smr in smrs){
			if(glow){
				if(fs != null)
					smr.renderer.material.shader = fs;
			}
			else{
				if(sh != null)
					smr.renderer.material.shader = sh;
			}
		}
		foreach(LODLevel lod in critterInfo.critterLODData.LODs){
			if(lod != currentLod)
				lod.LOD.SetActive(false);
		}
		*/


		
		

		hoveringState = glow;

		if(fxHover != null){
			ParticleSystem[] pss = fxHover.GetComponentsInChildren<ParticleSystem>();
			foreach(ParticleSystem ps in pss){
				ps.enableEmission = glow;
			}

			if(glow)
				fxHover.emissionRate = 140f;
			else
				fxHover.emissionRate = 0f;
		}

		if(icon != null){
			ParticleSystem ps = icon.GetComponent<ParticleSystem>();
			if(ps != null){
				if(glow)
					ps.startSize = 140f;
				else
					ps.startSize = 90f;
			}

			
			/*
			TrailRenderer[] trs = icon.transform.GetComponentsInChildren<TrailRenderer>();
			if(trs.Length > 0){
				for(int i = 2; i < icon.transform.childCount; i++){
					icon.transform.GetChild(i).gameObject.SetActive(glow);
				}
			}
			int i = 0;
			foreach(TrailRenderer tr in trs){
				Debug.Log("Trail " + i);
				if(i > 1)
					tr.gameObject.SetActive(glow);
				i++;
			}
			*/
		}
	}

	public void SetOutlineMaterial(bool outline){
//		if(outlineShape != null){
//			outlineShape.SetActive(outline);
//		}
		/*
		SkinnedMeshRenderer[] smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach(SkinnedMeshRenderer smr in smrs){
			if(outline){
				if(outlineMaterial != null)
					smr.renderer.material = outlineMaterial;
			}
			else{
				if(origMaterial != null)
					smr.renderer.sharedMaterial = origMaterial;
			}
		}
		*/
	}

    void OnDisable()
    {
        // cleaup the icon on disable b/c we don't get OnDestroy when we're already disabled
        CleanupIcon();
    }

    void OnDestroy()
    {
        CleanUp();
    }

    public void CleanUp() {
        critterInfo = null;
        CleanupIcon();
    }

    void CleanupIcon()
    {
        if( icon != null ) 
        {
            GameObject.Destroy( icon );
            icon = null;
            iconTransform = null;
        }
        
        if( hoverEffect != null ) 
        {
            GameObject.Destroy( hoverEffect );
            hoverEffect = null;
            hoverParticles = null;
            hoverTransform = null;
        }
    }
}
