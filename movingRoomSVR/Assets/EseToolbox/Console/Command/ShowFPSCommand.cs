using System;
using Ese;

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
///     This command shows the FPS in the top right corner.
/// </summary>
public class ShowFPSCommand : ConsoleCommand
{

    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>The command</returns>
    public override string[] GetCommands()
    {
        return new string[] { "ShowFPS", "FPS",};
    }

    public override string GetDescription()
    {
        return "Shows the FPS Counter in the top right corner [parameter = 0 / 1]";
    }

    /// <summary>
    /// Perform this action when the command in GetCommand is received.
    /// </summary>
    /// <param name="context"></param>
    public override void OnCommand(ConsoleContext context)
    {
        Console console = GetComponent<Console>();
        if(console != null)
        {
            if(context.Parameter.IsNullOrEmpty())
            {
                console.ShowFPS = true;
            }
            else
            {
                float number = GetContextFloatValue(context);
                console.ShowFPS = (number > 0);
            }

            if(console.ShowFPS)
            {
                Logger.LogSuccess("Showing FPS counter");
            }
            else
            {
                Logger.LogSuccess("Hiding FPS counter");
            }
        }
    }
}
