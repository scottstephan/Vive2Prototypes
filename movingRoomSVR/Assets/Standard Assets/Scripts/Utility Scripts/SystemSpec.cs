using UnityEngine;
using System.Collections;

public enum SystemLevel {
	Low,
	Medium,
	High,
}

public class SystemSpec {
	
	public static SystemLevel sysLevel = SystemLevel.High;
	
	public static void Setup() {
		string operatingSystem = SystemInfo.operatingSystem;
		int graphicsMemorySize = SystemInfo.graphicsMemorySize;
		int systemMemorySize = SystemInfo.systemMemorySize;
		
		// based on our OS chose the appropriate antialiasing setting
		int antiAliasingSetting = 0;		
		if(operatingSystem.IndexOf("mac",System.StringComparison.OrdinalIgnoreCase) < 0)
		{
			antiAliasingSetting = 8;
		}
		QualitySettings.antiAliasing = antiAliasingSetting;
		
		// based on a combination of graphics memory and system memory
		// chose our system level
		if( graphicsMemorySize > 1000 && systemMemorySize > 4000 ) {
			sysLevel = SystemLevel.High;		
		}
		else if( graphicsMemorySize > 500 && systemMemorySize > 2000 ) {
			sysLevel = SystemLevel.Medium;		
		}
		else {
			sysLevel = SystemLevel.Low;		
		}		
	}	
	
	public static void OutputSetup() {		
		string operatingSystem = SystemInfo.operatingSystem;
		int graphicsMemorySize = SystemInfo.graphicsMemorySize;
		int systemMemorySize = SystemInfo.systemMemorySize;

		DebugDisplay.AddDebugText("SystemInfo | Chosen System Level:: " + sysLevel.ToString());
		
#if UNITY_EDITOR
		DebugDisplay.AddDebugText("SystemInfo | Operating System: " + operatingSystem);
		DebugDisplay.AddDebugText("SystemInfo | Processor Type: " + SystemInfo.processorType);
		DebugDisplay.AddDebugText("SystemInfo | Processor Count: " + SystemInfo.processorCount.ToString());
		DebugDisplay.AddDebugText("SystemInfo | System Memory Size: " + systemMemorySize.ToString() + " MB");
		DebugDisplay.AddDebugText("SystemInfo | Graphics Memory Size: " + graphicsMemorySize.ToString() + " MB");
		DebugDisplay.AddDebugText("SystemInfo | Graphics Device Name: " + SystemInfo.graphicsDeviceName);
		DebugDisplay.AddDebugText("SystemInfo | Graphics Device Vendor: " + SystemInfo.graphicsDeviceVendor);
		DebugDisplay.AddDebugText("SystemInfo | Graphics Device ID: " + SystemInfo.graphicsDeviceID.ToString());
		DebugDisplay.AddDebugText("SystemInfo | Graphics Device Vendor ID: " + SystemInfo.graphicsDeviceVendorID.ToString());
		DebugDisplay.AddDebugText("SystemInfo | Graphics Device Version: " + SystemInfo.graphicsDeviceVersion);
		DebugDisplay.AddDebugText("SystemInfo | Graphics Shader Level: " + SystemInfo.graphicsShaderLevel.ToString());
		DebugDisplay.AddDebugText("SystemInfo | Graphics Pixel Fillrate: " + SystemInfo.graphicsPixelFillrate.ToString() + " MPix/s");
		DebugDisplay.AddDebugText("SystemInfo | Supports Shadows: " + SystemInfo.supportsShadows.ToString());
		DebugDisplay.AddDebugText("SystemInfo | Supports Render Textures: " + SystemInfo.supportsRenderTextures.ToString());
		DebugDisplay.AddDebugText("SystemInfo | Supports Image Effects: " + SystemInfo.supportsImageEffects.ToString());
#endif
	}
}
