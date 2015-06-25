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
///     Loads a level
/// </summary>
public class LoadLevelAdditiveAsyncCommand : ConsoleCommand
{
    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>The command</returns>
    public override string[] GetCommands()
    {
        return new string[] { "LoadLevelAdditiveAsync" };
    }

    /// <summary>
    /// The description of the console command.
    /// </summary>
    /// <returns></returns>
    public override string GetDescription()
    {
        return "Loads a level additively named [parameter]";
    }

    public override void OnCommand(ConsoleContext context)
    {
        Application.LoadLevelAdditiveAsync(context.Parameter);
    }
}
