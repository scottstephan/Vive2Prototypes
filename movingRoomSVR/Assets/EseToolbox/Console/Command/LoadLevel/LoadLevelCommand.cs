using Ese;
using UnityEngine;

/// <summary>
/// Loads a level
/// </summary>
public class LoadLevelCommand : ConsoleCommand
{

    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>"LoadLevel", "ChangeLevel", "changelevel"</returns>
    public override string[] GetCommands()
    {
        return new string[] { "LoadLevel", "ChangeLevel", "Map" };
    }

    /// <summary>
    /// The description of the console command.
    /// </summary>
    /// <returns>"Loads a level named [parameter]"</returns>
    public override string GetDescription()
    {
        return "Loads a level named [parameter]";
    }

    /// <summary>
    /// Perform this action when the command in GetCommand is received.
    /// </summary>
    /// <param name="context">The command and parameter</param>
    public override void OnCommand(ConsoleContext context)
    {
        Application.LoadLevel(context.Parameter);
    }
}
