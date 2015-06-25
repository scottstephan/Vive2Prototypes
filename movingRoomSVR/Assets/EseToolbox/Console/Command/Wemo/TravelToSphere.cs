using Ese;

public class TravelToSphere : ConsoleCommand
{
    private Console console;

    /// <summary>
    /// The constructor
    /// </summary>
    public void Awake()
    {
    }

    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    public override string[] GetCommands()
    {
        return new string[] { "Travel", "travel" };
    }

    /// <summary>
    /// The description of the console command.
    /// </summary>
    public override string GetDescription()
    {
        return "travels to the sphere typed in.";
    }

    /// <summary>
    /// Perform this action when the command in GetCommand is received.
    /// </summary>
    /// <param name="context"></param>
    public override void OnCommand(ConsoleContext context)
    {
        Console console = GetComponent<Console>();
        if (console == null)
            return;

        if (context.Command == "travel") {
            if (context.Parameter.IsNullOrEmpty()) {
                Console.Out("Where do you want to travel?");
            }
            else {
                string[] args = context.Parameter.Split(' ');
                string levelStr;
                if(args.Length != 1) {
                    Console.Out("Too many arguments");
                }
                levelStr = args[0];
                DataItem sphere = App.SphereManager.GetDataForName( levelStr );
                if( sphere != null ) {
                    App.SphereManager.TravelToSphere( null, sphere );
                    Console.Instance.forceToggle = true;
                }
            }
        }
    }
}
