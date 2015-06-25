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
///     6th of December, 2011
/// 
/// Purpose:
///     Finds a gameobject
/// </summary>
public class FindGameObjectCommand : ConsoleCommand
{
    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>"Find", "Search"}</returns>
    public override string[] GetCommands()
    {
        return new string[] { "find", "search"};
    }


    /// <summary>
    /// Finds a gameobject
    /// </summary>
    /// <returns></returns>
    public override string GetDescription()
    {
        return "Finds a gameobject";
    }

    public override void OnCommand(ConsoleContext context)
    {
        if(!context.Command.IsNullOrEmpty())
        {
            GameObject go = GameObject.Find(context.Command);
            if(go != null)
            {
                Logger.LogSuccess("Found a gameobject with the name '" + go.name + "' and its located at position " + go.transform.position.DebugLog(), go);
            }
            else
            {
                Logger.LogError("Could not find a gameobject with the name '" + context.Command + "'.");
            }
        }
        else
        {
            Logger.LogError("You need to specify a name to find.");
        }
    }
}