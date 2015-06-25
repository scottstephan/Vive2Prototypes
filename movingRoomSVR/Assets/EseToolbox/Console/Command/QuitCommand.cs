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
///     Quits the game
/// </summary>
public class QuitCommand : ConsoleCommand
{
    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>"Quit", "quit", "Exit", "exit"</returns>
    public override string[] GetCommands()
    {
        return new string[] { "Quit", "Exit"};
    }


    /// <summary>
    /// Quits the game
    /// </summary>
    /// <returns></returns>
    public override string GetDescription()
    {
        return "Quits the game";
    }

    public override void OnCommand(ConsoleContext context)
    {
        if(!Application.isEditor)
        {
            Logger.LogSuccess("Quitting the game");
            Application.Quit();
        }
        Logger.LogWarning("Cant quit the game in the editor");
    }
}
