using UnityEngine;
using System.Collections;

public class TranslateOcean : MonoBehaviour {
    const float ZDIFFCOPY = 10f;

    static TranslateOcean singleton = null;
//    GameObject myObj;
    Renderer myRenderer;
    Transform myTransform;
    GameObject copyObj;
    Renderer copyRenderer;
    Transform copyTransform;
    Transform collisionTransform;
    float moveDim;
	float startY;
    Vector3 startLocalScale;
    Vector3 lastCamPos;

    bool bUsingCopy;

	// Use this for initialization
	void Start ()
    {
//        myObj = gameObject;
        myRenderer = GetComponent<Renderer>();
        myTransform = transform;
        moveDim = Mathf.Min (myTransform.localScale.x, myTransform.localScale.z);

        startLocalScale = myTransform.localScale;

		startY = myTransform.position.y;

        copyObj = GameObject.Instantiate(gameObject) as GameObject;
        copyRenderer = copyObj.GetComponent<Renderer>();
        copyTransform = copyObj.transform;
        copyTransform.parent = myTransform.parent;

        TranslateOcean copyComp = copyObj.GetComponent<TranslateOcean>();
        Destroy (copyComp);

        GameObject collisionObj = GameObject.Find ("OceanSurfaceCollision");
        if (collisionObj != null)
        {
            collisionTransform = collisionObj.transform;
        }

        copyRenderer.enabled = false;

        bUsingCopy = false;
	}
	

	// Update is called once per frame
	void LateUpdate ()
    {
        if (singleton == null)
        {
            singleton = this;
        }

		if (CameraManager.IntroOceanOn())
        {   
            myRenderer.enabled = false;
            copyRenderer.enabled = false;
            return;
        }
            
        Vector3 camPos = CameraManager.GetCurrentCameraPosition();
        Vector3 camFwd = CameraManager.GetCurrentCameraForward();
        Vector3 curOceanPos = bUsingCopy ? copyTransform.position : myTransform.position;
		Vector3 oceanToCamDir = camPos - curOceanPos;
		float oceanToCamDist = oceanToCamDir.magnitude;

		if (oceanToCamDist > 0f) 
		{
			oceanToCamDir /= oceanToCamDist;
		}

        if (Vector3.Dot (oceanToCamDir, camFwd) > 0.65f ||
            OculusCameraFadeManager.IsFaded())
        {
            UpdatePos(camPos);
        }
        else
        {
            Vector3 dir = camPos - lastCamPos;
            dir.y = 0f;
            float dist = dir.magnitude;

            //teleporting
            if (dist > 100f)
            {
                UpdatePos(camPos);
            }
            else
            {
                Vector3 oceanToCamDirXZ = camPos - curOceanPos;
                oceanToCamDirXZ.y = 0;
                float oceanToCamDistXZ = oceanToCamDirXZ.magnitude;
                if (oceanToCamDistXZ > 0f)
                {
                    oceanToCamDirXZ /= oceanToCamDistXZ;
                }

                if (oceanToCamDistXZ > moveDim)
                {
                    if (dist > 0f)
                    {
                        dir /= dist;
                    }
                    else
                    {
                        dir = oceanToCamDirXZ;
                    }

                    Vector3 newOceanPos = camPos + (dir*moveDim);
                    if (bUsingCopy)
                    {
                        newOceanPos.y = startY;
    					myTransform.position = newOceanPos;
                        myRenderer.enabled = true;
                        bUsingCopy = false;
                    }
                    else
                    {
                        newOceanPos.y = startY - ZDIFFCOPY;
                        copyTransform.position = newOceanPos;
                        copyRenderer.enabled = true;
                        bUsingCopy = true;
                    }

                    if (collisionTransform != null)
                    {
                        collisionTransform.position = newOceanPos;
                    }
                }
                else if (oceanToCamDistXZ > moveDim * 2f)
                {
                    // inside distance, turn off other object
                    if (bUsingCopy)
                    {
                        myRenderer.enabled = false;
                    }
                    else
                    {
                        copyRenderer.enabled = false;
                    }
                }
            }
        }

        lastCamPos = camPos;
	}

    void OnDestroy()
    {
        singleton = null;
    }

    void UpdatePos(Vector3 camPos)
    {        
        if (bUsingCopy)
        {
            copyTransform.position = new Vector3(camPos.x, startY-ZDIFFCOPY, camPos.z);
            myRenderer.enabled = false;
            copyRenderer.enabled = true;
        }
        else
        {
            myTransform.position = new Vector3(camPos.x, startY, camPos.z);
            copyRenderer.enabled = false;
            myRenderer.enabled = true;
        }

        if (collisionTransform != null)
        {
            collisionTransform.position = new Vector3(camPos.x, startY, camPos.z);
        }
    }

    static public void UpdateMenu(bool bMenuEnabled)
    {
        if (singleton == null)
        {
            return;
        }

        singleton.UpdatePos(CameraManager.GetCurrentCameraPosition());

        if (bMenuEnabled)
        {
            if (FloatingMenuManager.menuOceanAdjustSize > 0f)
            {
                singleton.copyTransform.localScale = new Vector3(FloatingMenuManager.menuOceanAdjustSize, singleton.startLocalScale.y, FloatingMenuManager.menuOceanAdjustSize);
                singleton.myTransform.localScale = new Vector3(FloatingMenuManager.menuOceanAdjustSize, singleton.startLocalScale.y, FloatingMenuManager.menuOceanAdjustSize);
            }
        }
        else
        {
            singleton.copyTransform.localScale = singleton.startLocalScale;
            singleton.myTransform.localScale = singleton.startLocalScale;
        }
    }
}
