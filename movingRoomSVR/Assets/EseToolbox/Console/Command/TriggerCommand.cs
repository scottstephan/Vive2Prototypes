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
///     Sends the message "DoActivateTrigger" to the targeted gameobject.
/// </summary>
public class TriggerCommand : SendMessageCommand
{
    public override string[] GetCommands()
    {
        return new string[] { "Trigger" };
    }

    public override string GetDescription()
    {
        return "Sends the message \"DoActivateTrigger\" to the targeted gameobject.";
    }

    protected override void Send(string parameter)
    {
        if(parameter.IsNullOrEmpty())
        {
            parameter = string.Empty;
        }

        if (!SendMessageCommandWithValue("DoActivateTrigger", parameter))
        {
            Logger.LogError("Could not send message.");
        }
    }
}
