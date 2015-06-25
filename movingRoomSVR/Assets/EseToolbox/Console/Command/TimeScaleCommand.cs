using System;
using Ese;
using UnityEngine;

public class TimeScaleCommand : ConsoleCommand
{
    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>"TimeScale", "timescale"</returns>
    public override string[] GetCommands()
    {
        return new string[] { "TimeScale"};
    }

    /// <summary>
    /// The description of the console command.
    /// </summary>
    /// <returns>"Takes a screenshot at [parameter] location"</returns>
    public override string GetDescription()
    {
        return "Sets the timescale";
    }

    /// <summary>
    /// Perform this action when the command in GetCommand is received.
    /// </summary>
    /// <param name="context"></param>
    public override void OnCommand(ConsoleContext context)
    {
        float number = GetContextFloatValue(context);
        if(number >= 0)
        {
            Time.timeScale = number;
            Logger.LogSuccess("Successfully set the timescale to " + number);
        }
    }
}
