using UnityEngine;
using System;
using System.Collections;

/*******************************************
* SIM SETTINGS
********************************************/	
[System.Serializable]
public class DataSettingsSim {
	public float updateTime = 1.0f;
	public float cycleCritterTime = 7.5f;
	
    public float checkWhiteBoardTime = 1.5f;
    public float minPopulationPercentageForLeaveCycleToStart = 0.5f;
    public float ownedMinLifeTime = 90.0f;
    public float ownedLeaveChanceBase = 0.1f;
    public float ownedLeaveChanceInc = 0.05f;
    public float ownedAddChanceBase = 0.5f;
    public float ownedAddChanceInc = 0.1f;
    public float systemMinLifeTime = 45.0f;
    public float systemLeaveChanceBase = 0.2f;
    public float systemLeaveChanceInc = 0.1f;
    public float minSystemRoamerInSphereLifetime = 50.0f;
    public float maxSystemRoamerInSphereLifetime = 70.0f;
}

/*******************************************
* METRICS SETTINGS
********************************************/	
[System.Serializable]
public class DataSettingsMetrics {
	public string s3Policy = "eyJleHBpcmF0aW9uIjogIjIwMTQtMDEtMDFUMDA6MDA6MDBaIiwKICAiY29uZGl0aW9ucyI6IFsgCiAgICB7ImJ1Y2tldCI6ICJkaWFyeS50aGVibHUuY29tIn0sIAogICAgWyJzdGFydHMtd2l0aCIsICIka2V5IiwgIiJdLAogICAgeyJhY2wiOiAicHJpdmF0ZSJ9LAogICAgWyJzdGFydHMtd2l0aCIsICIkQ29udGVudC1UeXBlIiwgIiJdLAogICAgWyJjb250ZW50LWxlbmd0aC1yYW5nZSIsIDAsIDEwNDg1NzZdCiAgXQp9Cg==";
   	public string s3Signature = "PbV0z0gQdMo9q0UAfThwvr3+RxM=";
    public string accessKeyId = "AKIAJPA6DGEPD5H373YA";		
}

/*******************************************
* PRIMARY SETTINGS OBJECT
********************************************/	
[System.Serializable]
public class DataSettings {
	
	public string assetServer = "http://sandbox.assets.theblu.com/";	
	
	public float roamingCheckoutDuration = 75f;

	public int audioBundleVersion = 1;
	
	public DataSettingsSim sim = new DataSettingsSim();
	
	public DataSettingsMetrics metrics = new DataSettingsMetrics();
}
