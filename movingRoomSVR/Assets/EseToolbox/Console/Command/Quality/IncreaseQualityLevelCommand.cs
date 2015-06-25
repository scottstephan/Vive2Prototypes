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
///     Increase the quality level.
/// </summary>
public class IncreaseQualityLevelCommand : ConsoleCommand
{
    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>The command</returns>
    public override string[] GetCommands()
    {
        return new string[] { "IncreaseQualityLevel", "IncreaseQualitySettings" };
    }

    /// <summary>
    /// The description of the console command.
    /// </summary>
    /// <returns></returns>
    public override string GetDescription()
    {
        return "Increases the quality level of the game.";
    }

    /// <summary>
    /// Perform this action when the command in GetCommand is received.
    /// </summary>
    /// <param name="context">The command and parameter</param>
    public override void OnCommand(ConsoleContext context)
    {
        /*QualityLevel levelBefore = QualitySettings.currentLevel;
        QualitySettings.IncreaseLevel();
        QualityLevel levelAfter = QualitySettings.currentLevel;
        if(levelBefore != levelAfter)
        {
            Logger.LogSuccess("Increased the graphics level to: " + Enum.GetName(typeof(QualityLevel), levelAfter));
        }
        else*/
        {
            Logger.LogWarning("Could not increase the graphics level.");
        }
    }
}
