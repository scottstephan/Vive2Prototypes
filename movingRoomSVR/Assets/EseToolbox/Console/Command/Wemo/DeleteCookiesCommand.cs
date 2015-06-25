using Ese;

public class DeleteCookiesCommand : ConsoleCommand
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
        return new string[] { "clearcookies", "ClearCookies" };
    }

    /// <summary>
    /// The description of the console command.
    /// </summary>
    public override string GetDescription()
    {
        return "Deletes cookies from HTTP.CookieJar.";
    }

    /// <summary>
    /// Perform this action when the command in GetCommand is received.
    /// </summary>
    /// <param name="context"></param>
    public override void OnCommand(ConsoleContext context)
    {
        DebugDisplay.AddDebugText( "Clearing cookies..." );
        HTTP.CookieJar.Instance.Clear();
    }
}
