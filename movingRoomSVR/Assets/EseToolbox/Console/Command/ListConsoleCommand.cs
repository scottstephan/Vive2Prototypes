using System.Text;
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
///     Lists all the console commands.
/// </summary>
public class ListConsoleCommand : ConsoleCommand
{
    private const string Separator = " - ";
    private const string DescriptionIndentationContent = "\n\t\t";

    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>"List"</returns>
    public override string[] GetCommands()
    {
        return new string[] {"List"};
    }

    /// <summary>
    /// The description of the console command.
    /// </summary>
    /// <returns>"Lists the console commands"</returns>
    public override string GetDescription()
    {
        return "Lists the console commands";
    }

    /// <summary>
    /// Perform this action when the command in GetCommand is received.
    /// </summary>
    /// <param name="context"></param>
    public override void OnCommand(ConsoleContext context)
    {
        ConsoleCommand[] commandComponents = GetComponents<ConsoleCommand>();
        if (!commandComponents.IsNullOrEmpty())
        {
            Console.Out("Listing " + commandComponents.Length + " commands.");
            foreach (ConsoleCommand consoleCommand in commandComponents)
            {
                StringBuilder command = new StringBuilder();
                foreach(string commandText in consoleCommand.GetCommands())
                {
                    command.Append(commandText);
                    command.Append(Separator);
                }

                command.Remove(command.Length - Separator.Length, Separator.Length);
                command.Append(DescriptionIndentationContent);
                command.Append(consoleCommand.GetDescription());

                Console.Out(command.ToString());
            }
        }
    }
}

