// Copyright (c) 2014 Nathan Martz

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SECTR_Member))]
[CanEditMultipleObjects]
public class SECTR_MemberEditor : SECTR_Editor
{
	public override void OnInspectorGUI()
	{
		SECTR_Member myMember = (SECTR_Member)target;
		serializedObject.Update();
		if(!myMember.IsSector)
		{
			DrawProperty("PortalDetermined");
			DrawProperty("ForceStartSector");
		}
		if(!myMember.gameObject.isStatic)
		{
			DrawProperty("BoundsUpdateMode");
		}
		DrawProperty("ExtraBounds");
		DrawProperty("OverrideBounds");
		DrawProperty("BoundsOverride");
		if(SECTR_Modules.VIS)
		{
			DrawProperty("DirShadowCaster");
			DrawProperty("DirShadowDistance");
		}
		serializedObject.ApplyModifiedProperties();
	}
}
