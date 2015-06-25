using UnityEngine;
using System.Collections;

public class SettingsScreenButton : VRButton {
    
    // Use this for initialization
    void Start () {
        
    }
    
    // Update is called once per frame
    void Update () {
        
        if (!_isEnabled)
            return;
        
        ButtonInput();
        
        if (ProcessInput())
            OnClick();
    }
    
    public override bool OnClick ()
    {
        if (!base.OnClick ())
            return false;
        
        FloatingMenuManager.HideMenu(false);
        FloatingMenuManager.ShowMenu(FloatingMenuManager.MenuType.Settings);
        
        return true;
    }
}
