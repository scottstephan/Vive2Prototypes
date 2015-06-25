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
///     Decreases the quality level.
/// </summary>
public class DecreaseQualityLevelCommand : ConsoleCommand
{

    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>The command</returns>
    public override string[] GetCommands()
    {
        return new string[] { "DecreaseQualityLevel", "DecreaseQualitySettings" };
    }

    /// <summary>
    /// The description of the console command.
    /// </summary>
    /// <returns></returns>
    public override string GetDescription()
    {
        return "Decreases the quality level of the game.";
    }

    public override void OnCommand(ConsoleContext context)
    {
        /*QualityLevel levelBefore = QualitySettings.currentLevel;
        QualitySettings.DecreaseLevel();
        QualityLevel levelAfter = QualitySettings.currentLevel;
        if (levelBefore != levelAfter)
        {
            Logger.LogSuccess("Decreased the graphics level to: " + Enum.GetName(typeof(QualityLevel), levelAfter));
        }
        else*/
        {
            Logger.LogWarning("Could not increase the graphics level.");
        }
    }
}
