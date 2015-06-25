using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class DatabaseUpdater : ScriptableWizard{
	 
	public string userName;
	public string password;
	public string wampInstallDrive;
	static string LastUpdateSettingFilePath = string.Concat(Application.dataPath, "/../WEMOData/LastDatabaseUpdateSetting.json");
	
	static string externalApplicationDirectiry = string.Concat(Application.dataPath, "/../ExternalApplication/ThreeSharpTest/");
	[MenuItem("WEMOTools/Build/[Database] Database Updater")]
	static void CreateWizard()
	{
		DatabaseUpdater me = ScriptableWizard.DisplayWizard("Database Updater", typeof(DatabaseUpdater), "Update To New Database" ) as DatabaseUpdater;
		if(File.Exists(LastUpdateSettingFilePath))
		{
			string[] lastBuildSettings = File.ReadAllLines(LastUpdateSettingFilePath);
			me.userName = lastBuildSettings[0];
			me.password = lastBuildSettings[1];
			me.wampInstallDrive = lastBuildSettings[2];
		}

	}
	void OnWizardCreate()
	{
		string WEMODataDirectory = string.Concat(Application.dataPath, "/../WEMOData/");
		if(!Directory.Exists(WEMODataDirectory))
		{
			Directory.CreateDirectory(WEMODataDirectory);
		}
		if(File.Exists(LastUpdateSettingFilePath))
		{
			File.Delete(LastUpdateSettingFilePath);
		}
		string[] lastBuildSettings = new string[3];
		lastBuildSettings[0] = userName;
		lastBuildSettings[1] = password;
		lastBuildSettings[2] = wampInstallDrive;
		File.WriteAllLines(LastUpdateSettingFilePath, lastBuildSettings);
		UpdateDataBase();
		
		
	}
 	void UpdateDataBase()
	{
		Process updateingDatabaseProcess = new Process();
		string applicationPath = externalApplicationDirectiry + "Release/ThreeSharpTest.exe";
	    try
        {
            updateingDatabaseProcess.StartInfo.FileName = applicationPath;
            updateingDatabaseProcess.StartInfo.UseShellExecute = true;
            updateingDatabaseProcess.Start();
        }
        catch (Exception e)
        {
            System.Console.Write(e.Message);
        }
	}
	
}

