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
///     18th of July, 2011
/// 
/// Purpose:
///     Clears the console
/// </summary>
public class ClearConsoleCommand : ConsoleCommand
{
    private Console console;

    /// <summary>
    /// The constructor
    /// </summary>
    public void Awake()
    {
        console = Console.Instance;
    }

    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>"Destroy", "DestroyGameObject"</returns>
    public override string[] GetCommands()
    {
        return new string[] { "Clear", "Reset" };
    }

    /// <summary>
    /// The description of the console command.
    /// </summary>
    /// <returns>"Clears the console view of messages"</returns>
    public override string GetDescription()
    {
        return "Clears the console view of messages";
    }

    /// <summary>
    /// Perform this action when the command in GetCommand is received.
    /// </summary>
    /// <param name="context"></param>
    public override void OnCommand(ConsoleContext context)
    {
        console.Clear();
    }
}
