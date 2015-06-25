/*
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
///     Example script for extending the console with javascript commands.
/// </summary>
public class JavascriptCommand extends Ese.Logging.ConsoleCommand
{
    function  GetCommands() : String[]
    {
        var commands = new String[2];
        commands[0] = "js";
        commands[1] = "javascript";
        return commands;
    }

    function GetDescription() : String
    {
        return "Destroys targeted gameobject";
    }

    function OnCommand(context : Ese.Logging.ConsoleContext) : void
    {
        Ese.Logging.Logger.LogSuccess("Javascript works successfully!");
    }
}
*/
