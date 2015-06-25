using Ese;
using UnityEngine;
using SendMessageOptions = UnityEngine.SendMessageOptions;

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
///     Sends the message "NoClip" to the main camera.
/// </summary>
public class NoClipCommand : ConsoleCommand
{
    public override string[] GetCommands()
    {
        return new string[] { "NoClip", "FreeRoam" };
    }

    public override string GetDescription()
    {
        return "Sends the message \"NoClip\" to the camera.";
    }

    public override void OnCommand(ConsoleContext context)
    {
		Camera mainCamera = CameraManager.GetCurrentCamera();
        mainCamera.SendMessage("NoClip", SendMessageOptions.DontRequireReceiver);
        Logger.LogSuccess("Sent message 'NoClip' to " + mainCamera.name);
    }
}
