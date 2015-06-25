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
///     This command sets the target framerate
/// </summary>
public class TargetFramerateCommband : ConsoleCommand
{

    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>The command</returns>
    public override string[] GetCommands()
    {
        return new string[] { "TargetFramerate", "TargetFPS", "MaxFPS"};
    }

    public override string GetDescription()
    {
        return "Limits the framerate to whatever you specify";
    }

    /// <summary>
    /// Perform this action when the command in GetCommand is received.
    /// </summary>
    /// <param name="context"></param>
    public override void OnCommand(ConsoleContext context)
    {
        int number = GetContextIntValue(context);
        if (number >= 0)
        {
            Application.targetFrameRate = number;
            Logger.LogSuccess("Set the target frame rate to " + number);
        }
    }
}
