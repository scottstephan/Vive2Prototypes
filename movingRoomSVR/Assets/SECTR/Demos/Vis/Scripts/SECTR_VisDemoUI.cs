using UnityEngine;
using System.Collections;

public class SECTR_VisDemoUI : SECTR_DemoUI
{
	#region Unity Interface
	protected override void OnEnable ()
	{
		watermarkLocation = WatermarkLocation.UpperCenter;
		AddButton(KeyCode.C, "Enable Culling", "Disable Culling", ToggleCulling);
		base.OnEnable();
	}

	protected override void OnGUI ()
	{
		base.OnGUI();

		if(passedIntro)
		{
			int renderersCulled = 0;
			int lightsCulled = 0;
			int terrainsCulled = 0;
			int numCullers = SECTR_Culler.All.Count;
			for(int cullerIndex = 0; cullerIndex < numCullers; ++cullerIndex)
			{
				SECTR_Culler culler = SECTR_Culler.All[cullerIndex];
				renderersCulled += culler.RenderersCulled;
				lightsCulled += culler.LightsCulled;
				terrainsCulled += culler.TerrainsCulled;
			}

			string statsString = "Culling Stats\n";
			statsString += "Renderers: " + renderersCulled + "\n";
			statsString += "Lights: " + lightsCulled + "\n";
			statsString += "Terrains: " + terrainsCulled;

			GUIContent statsContent = new GUIContent(statsString);
			float width = Screen.width * 0.33f;
			float height = demoButtonStyle.CalcHeight(statsContent, width);
			Rect statsRect = new Rect(Screen.width - width, 0, width, height);
			GUI.Box(statsRect, statsContent, demoButtonStyle);
		}
	}
	#endregion

	#region Private Details
	protected void ToggleCulling(bool active)
	{
		SECTR_CullingCamera cullingCamera = GetComponent<SECTR_CullingCamera>();
		if(cullingCamera)
		{
			cullingCamera.enabled = !active;
			int numCullers = SECTR_Culler.All.Count;
			for(int cullerIndex = 0; cullerIndex < numCullers; ++cullerIndex)
			{
				SECTR_Culler culler = SECTR_Culler.All[cullerIndex];
				culler.ResetStats();
			}
		}
	}
	#endregion
}
