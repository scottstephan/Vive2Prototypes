using System;
using Ese;
using UnityEngine;

/// <summary>
/// Author: 
///     Skjalg S. Mæhre
/// 
/// Company: 
///     Exit Strategy Entertainment
/// 
/// Website: 
///     http://www.exitstrategyentertainment.com/
/// 
/// Date:
///     10th of July, 2011
/// 
/// Purpose:
///     Captures a screenshot at path filename as a PNG file.
///     If the file exists already, it will be overwritten. 
///     This function does nothing if used from inside the web player or a Dashboard widget.
/// </summary>
public class CaptureScreenShotCommand : ConsoleCommand
{
    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>"ScreenShot", "CaptureScreenShot"</returns>
    public override string[] GetCommands()
    {
        return new string[] { "ScreenShot", "CaptureScreenShot" };
    }

    /// <summary>
    /// The description of the console command.
    /// </summary>
    /// <returns>"Takes a screenshot at [parameter] location"</returns>
    public override string GetDescription()
    {
        return "Takes a screenshot at [parameter] location";
    }

    /// <summary>
    /// Perform this action when the command in GetCommand is received.
    /// </summary>
    /// <param name="context"></param>
    public override void OnCommand(ConsoleContext context)
    {
        string parameter = context.Parameter;
        if(parameter.IsNullOrEmpty())
        {
            parameter = ScreenShotName();
        }
        Application.CaptureScreenshot(parameter);
        Logger.LogSuccess("Captured screenshot to location: " + parameter);
    }

    public static string ScreenShotName()
    {
        return string.Format("{0}/screen_{1}_{2}.png",
                             Application.dataPath,
                             Application.loadedLevel,
                             DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }
}
