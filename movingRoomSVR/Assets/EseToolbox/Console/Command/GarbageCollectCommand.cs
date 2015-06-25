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
///     Runs unity garbage collection
/// </summary>
public class GarbageCollectCommand : ConsoleCommand
{
    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>"GarbageCollect", "Collect"</returns>
    public override string[] GetCommands()
    {
        return new string[] { "GarbageCollect", "Collect" };
    }

    /// <summary>
    /// The description of the console command.
    /// </summary>
    /// <returns>"Runs unity garbage collection"</returns>
    public override string GetDescription()
    {
        return "Runs unity garbage collection";
    }

    /// <summary>
    /// Perform this action when the command in GetCommand is received.
    /// </summary>
    /// <param name="context"></param>
    public override void OnCommand(ConsoleContext context)
    {
        GC.Collect();
        Logger.LogSuccess("Performed garbage collection.");
    }
}
