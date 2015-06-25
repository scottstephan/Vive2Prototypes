using UnityEngine;
using System.Collections;

public class CopyBootMenuToPopup : MonoBehaviour {

	public TranslationUI tui;

	public GameObject startPos;
	public GameObject PopUpFloatingMenu;
	public GameObject BootMenu;

	public TravelMenuManager srcTMM;
	public TravelMenuManager dstTMM;

	// Use this for initialization
	void Start () {

		if (PopUpFloatingMenu == null)
			return;

        if (srcTMM.blocker != null)
		    srcTMM.blocker.SetActive(true);

		//clone the boot menu from inside VR_BOOT
		GameObject bootObject = (GameObject)GameObject.Instantiate(BootMenu);

		//enable the background object in this cloned instance (because camera flags don't work)
        if (srcTMM.blocker != null) {
            GameObject b = bootObject.transform.FindChild("Background").gameObject;
		    dstTMM.blocker = b;
		    srcTMM.blocker.SetActive(false);
        }

		//copy the popup menu's transform to the clone object so they are in the same position
		bootObject.transform.parent = PopUpFloatingMenu.transform.parent;
		bootObject.transform.position = PopUpFloatingMenu.transform.position;
		bootObject.transform.rotation = PopUpFloatingMenu.transform.rotation;
		bootObject.transform.localScale = PopUpFloatingMenu.transform.localScale;
		bootObject.name = "FloatingMenu-NEW";

		//make these the right layer for the popup
		ChangeLayersRecursively(bootObject.transform, "VRUI");

		//move it out the proper distance from the camera so it looks the same as in the boot
		Vector3 newPos = bootObject.transform.position;
		newPos.z = (BootMenu.transform.position.z + startPos.transform.position.z);
		//newPos.z = (BootMenu.transform.position.z - startPos.transform.position.z);
		bootObject.transform.position = newPos;

		//tui.UIPoint.transform.position = newPos;

		//kill the old floating menu since it's not needed
		GameObject.DestroyImmediate(PopUpFloatingMenu);
		PopUpFloatingMenu = null; //never do this again!
	}

	public static void ChangeLayersRecursively(Transform trans, string name)
	{
		trans.gameObject.layer = LayerMask.NameToLayer(name);

		foreach(Transform child in trans)
			ChangeLayersRecursively(child, name);
	}
}
