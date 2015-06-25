using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class WemoEditorWindow : EditorWindow {
    public static WemoEditorWindow singleton;
	
	public static bool drawGameDebugInfo = false;
	public static bool tagsFoldout = false, _update = false;
	public static bool cameraViewerFoldout = false;
	public static List<string> tags = new List<string>();
	string _tagToAdd = "Untagged";
	public static bool drawFishAvoidanceDebugInfo = false;
    
	RenderTexture renderTexture;
	float cameraRenderWidth = 640f;
	float cameraRenderHeight = 480f;
	private Camera mainCamera = null;
	
	private BaseCameraMode curBaseCamera = null;
	
	[DrawGizmo(GizmoType.SelectedOrChild | GizmoType.NotSelected)]
	static void DrawGameObjectName(Transform transform, GizmoType gizmoType)
	{
	    if(EditorApplication.isPlaying && drawGameDebugInfo) {
			GeneralSpeciesData fish_object = transform.gameObject.GetComponent<GeneralSpeciesData>();

			if( fish_object != null ) { 
//				string lead = "";
//				if (fish_object.myCritterInfo.swimSchoolFollowData.leaderCritterInfo != null	) { 
//					lead = fish_object.myCritterInfo.swimSchoolFollowData.leaderCritterInfo.critterObject.name;
//				}
				float parkingLevel = 0f;
				if(fish_object.myCritterInfo.swimParkingData) parkingLevel = fish_object.myCritterInfo.swimParkingData.parkingLevel;
				
				string xx = fish_object.myCritterInfo.critterObject.name + "  " + fish_object.myCurrentBehaviorType.ToString() 
					//+ "\n isHungry " + fish_object.isHungry.ToString()
					+ "\n hungerLevel " + fish_object.hungerLevel
					+ "\n airLevel " + fish_object.airLevel
					+ "\n parkingLevel " + parkingLevel;
					//+ "\n leader " + lead;
					//+ "\n isLeaderInt " + fish_object.myCritterInfo.swimSchoolFollowData.isLeaderInt;
				    //+ "\n targetPos " + fish_object.myCritterInfo.swimTargetedData.targetPosition;
				Handles.Label(fish_object.gameObject.transform.position,xx);
			}
		}
	}
	
	[MenuItem("WEMOTools/Open WEMO Editor")]
    static void CreateWindow()
    {
        singleton = (WemoEditorWindow)GetWindow(typeof(WemoEditorWindow));

        singleton.wantsMouseMove = true;
        singleton.title = "WEMO EDITOR";
        singleton.minSize = new Vector2(200, 200);
        singleton.autoRepaintOnSceneChange = true;
        singleton.Show();
        singleton.ShowUtility();
    }
	
    public void Awake () {
        renderTexture = new RenderTexture((int)cameraRenderWidth, 
                    (int)cameraRenderHeight, 
                    (int)RenderTextureFormat.ARGB32 );
    }
	
    public void Update() {
		if( mainCamera == null ) {
			mainCamera = CameraManager.GetCurrentCamera();
		}
        
		if( mainCamera != null 
			&& cameraViewerFoldout
			&& Selection.activeGameObject != null ) {
			BaseCameraMode cameraMode = Selection.activeGameObject.GetComponent<BaseCameraMode>();
			if( cameraMode != null ) {
				mainCamera.transform.position = Selection.activeGameObject.transform.position;
				mainCamera.transform.rotation = Selection.activeGameObject.transform.rotation;
	            mainCamera.targetTexture = renderTexture;
    	        mainCamera.Render();
        	    mainCamera.targetTexture = null;
		        if( renderTexture.width != cameraRenderWidth 
					|| renderTexture.height != cameraRenderHeight ) {
        		    renderTexture = new RenderTexture((int)cameraRenderWidth, 
            		                (int)cameraRenderHeight, 
                		            (int)RenderTextureFormat.ARGB32 );
				}
			}
        }
		
		// auto gizmo selected cameras..
		if( Selection.activeGameObject ) {
			BaseCameraMode bc = Selection.activeGameObject.GetComponent<BaseCameraMode>();
			GeneralMotionData gmd = Selection.activeGameObject.GetComponent<GeneralMotionData>();
			if( bc == null ) {
				if( Selection.activeGameObject.transform.parent ) {
					bc = Selection.activeGameObject.transform.parent.GetComponent<BaseCameraMode>();
				}
			}

			if( bc == null ) {
				if( curBaseCamera != null ) {
					curBaseCamera.SetDrawGizmos( false );
					curBaseCamera = null;
				}
			}
			else {
				if( curBaseCamera != null ) {
					curBaseCamera.SetDrawGizmos( false );
				}
				curBaseCamera = bc;
				curBaseCamera.SetDrawGizmos( true );
			}
			// fishAvoidanceDebugInfo
			if(gmd != null && drawFishAvoidanceDebugInfo){
				//CritterInfo critter_info = gsd.myCritterInfo;
				//GeneralMotionData gmd = critter_info.generalMotionData;
				GeneralMotion.SetDrawGizmos( true, gmd );
				//Debug.DrawLine(critter_info.critterTransform.position,critter_info.critterTransform.position + gmd.desiredDirNorm,Color.magenta);
			}
		}
		else if( curBaseCamera != null ) {
			curBaseCamera.SetDrawGizmos( false );
			curBaseCamera = null;
		}
		
		
    }

	void OnGUI() {
        _update = false;
//        _targetObject = target as WemoObject;

//        EditorGUILayout.Separator();
		drawGameDebugInfo = GUILayout.Toggle(drawGameDebugInfo,"Draw Game Debug Info");
		drawFishAvoidanceDebugInfo = GUILayout.Toggle(drawFishAvoidanceDebugInfo,"Draw Fish Avoidance Debug Info");
//        GUI.color = Color.gray;
        GUILayout.Label("Draw Gizmo Tags : " + tags.Count);
//       GUI.color = Color.black;
        tagsFoldout = EditorGUILayout.Foldout(tagsFoldout, "");
//       GUI.color = AntaresEdWindow.colorGUI;

        if( tagsFoldout ) {
			DrawTagsFoldout();
		}

		if (_update) {
			string[] tag_list = new string[tags.Count];
			tags.CopyTo(tag_list);
			WemoObject[] wemo_objects = GameObject.FindObjectsOfType(typeof(WemoObject)) as WemoObject[];
			
			foreach( WemoObject wemo_obj in wemo_objects ) {
				if( wemo_obj.gameObject.HasAnyTag(tag_list) ) {
					wemo_obj.SetDrawGizmos( true );
				}
				else {
					wemo_obj.SetDrawGizmos( false );
				}
			}
        }

        GUILayout.Label("Draw Camera Viewer");
        cameraViewerFoldout = EditorGUILayout.Foldout(cameraViewerFoldout, "");
		
		if( cameraViewerFoldout ) {
			DrawCameraViewFoldout();
		}
		
//        Routines.DrawFooter();
//        GUI.color = AntaresEdWindow.colorGUI;
    }
	
	private void DrawCameraViewFoldout() {
		Rect horz = EditorGUILayout.BeginHorizontal();
		horz.width = cameraRenderWidth;
		horz.height = cameraRenderHeight;
		GUILayout.BeginArea( horz, renderTexture );
		GUILayout.EndArea();
        EditorGUILayout.EndHorizontal();
	//cameraRenderHeight
	}

    private void DrawTagsFoldout() {
//        bool iMInScene = Selection.activeTransform == _targetObject.transform, 
		bool tagAdded = false;
//        GUI.enabled = iMInScene;
        EditorGUILayout.BeginHorizontal();

        string tagBefore = _tagToAdd;

        _tagToAdd = EditorGUILayout.TagField(_tagToAdd, GUILayout.Width(128));
        if (_tagToAdd != tagBefore && _tagToAdd != "Untagged" && _tagToAdd != "") tagAdded = true;
        GUI.enabled = !(_tagToAdd == "Untagged");
//        GUI.color = _buttonColor;
        GUILayout.Label("   Select tag to add");

	    int count = tags.Count;
        if (tagAdded)
        {
//            Undo.RegisterSceneUndo("Add Tag");
			if( count == 0 ) { // just add it
           	    tags.Add(_tagToAdd);
			}
    	    else {
				for (int i = 0; i < count; i++) {
        	    	if (!tags.Contains(_tagToAdd)) {			
    	        	    tags.Add(_tagToAdd);
					}
				}
			}
			_tagToAdd = "Untagged";
            _update = true;
        }

        EditorGUILayout.EndHorizontal();

//        List<string> tagsList;
//        tagsList = _targetObject.gameObject.GetTags();

        count = tags.Count;
        EditorGUILayout.Separator();
        GUI.enabled = true;
		Color def = GUI.color;
        GUI.color = Color.white;
		for (int i = 0; i < count; i++)
        {
            EditorGUILayout.BeginHorizontal();
		
            GUILayout.Button(tags[i], EditorStyles.miniButtonLeft, GUILayout.Width(128));
            if (GUILayout.Button("Delete", EditorStyles.miniButtonRight, GUILayout.Width(64)))
            {
           		tags.Remove(tags[i]);
				_update = true;
				i--;
				count--;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Separator();
        GUI.enabled = !(count == 0);
        if (GUILayout.Button("Remove All Tags", EditorStyles.miniButton, GUILayout.Width(128)))
        {
			Undo.RecordObject(this, "Remove all Tags");
            tags.Clear();
            _update = true;
        }
		GUI.enabled = true;
        GUI.color = def;
    }
	
	void OnDestroy() {
		if( curBaseCamera ) {
			curBaseCamera.SetDrawGizmos( false );
			curBaseCamera = null;
		}
	}

}