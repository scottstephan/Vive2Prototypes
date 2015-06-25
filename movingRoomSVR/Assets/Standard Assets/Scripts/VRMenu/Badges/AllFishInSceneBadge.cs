using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AllFishInSceneBadge : BadgeBase {
    static AllFishInSceneBadge singleton;

    string sceneName = "";
    List<string> sceneSpeciesNames = new List<string>();

    public override string PlayerPrefsString() {
        return sceneName + "_SPECIESCLICKCOUNT";
    }
    
    // Use this for initialization
	public override void Start () 
    {
        base.Start();
	}
	
    public void StartScene()
    {
        string appSceneName = App.SphereManager.currentSphereName;

        if (AppBase.Instance.RunningAsPreview() && 
            string.IsNullOrEmpty(appSceneName))
        {
            appSceneName = Application.loadedLevelName;
        }

        if (appSceneName!= sceneName)
        {
            sceneName = appSceneName;
            sceneSpeciesNames.Clear();
        }
    }

    public void SpeciesAdd(string s)
    {
        bool bFound = false;
        for (int i=0; i<sceneSpeciesNames.Count; ++i)
        {
            if (string.Compare(sceneSpeciesNames[i], s, System.StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                bFound = true;
            }
        }

        if (!bFound)
        {
            sceneSpeciesNames.Add (s.ToUpper());
        }
    }

    public bool SpeciesInteract(string s)
    {
        string speciesPrefName = sceneName + "_" + s.ToUpper();
        int interactCount = PlayerPrefs.GetInt(speciesPrefName, 0);
        bool bSpeciesFirstInteract = (interactCount == 0);

        if (bSpeciesFirstInteract)
        {
            int speciesCount = PlayerPrefs.GetInt(PlayerPrefsString(), 0);
            ++speciesCount;
            PlayerPrefs.SetInt(PlayerPrefsString(), speciesCount);
        }

        ++interactCount;
        PlayerPrefs.SetInt(speciesPrefName, interactCount);
        return bSpeciesFirstInteract;
    }

    public int GetNumSpeciesTotal()
    {
        return sceneSpeciesNames.Count;
    }
    public int GetNumSpeciesInteracted()
    {
        int count = PlayerPrefs.GetInt(PlayerPrefsString(), 0);

        if (count > sceneSpeciesNames.Count)
        {
            count = sceneSpeciesNames.Count;
        }

        return count;
    }


	// Update is called once per frame
	void Update () {
	
	}
}
