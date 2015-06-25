using UnityEngine;
using System.Collections;

public class LevelCameraData : MonoBehaviour
{
	public GameObject oculusStart = null;
    public float oculusScale = 5f;
	public GameObject[] cameraOrder;
	public Vector3 followCamSphereCenter = new Vector3(0f,-220f,0f);
	public float followCamSphereDistance = 450f;

    public float menuOceanAdjustDist = 2000f;
    public GameObject menuOcean;
    public GameObject[] normalOceans;

    void Start()
    {
        FloatingMenuManager.menuOceanAdjustDist = menuOceanAdjustDist;
        FloatingMenuManager.menuOcean = menuOcean;
        FloatingMenuManager.normalOceans = normalOceans;
    }

    void OnDisable()
    {
        FloatingMenuManager.menuOceanAdjustDist = 2000f;
        FloatingMenuManager.menuOcean = null;
        FloatingMenuManager.normalOceans = null;
    }
}
