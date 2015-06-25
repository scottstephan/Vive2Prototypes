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
///     10th of July, 2011
/// 
/// Purpose:
///     Sends a message to the targeted gameobject.
/// </summary>
public class SendMessageCommand : ConsoleCommand
{
    private Camera mainCamera;
    private Transform mainCameraTransform;

    /// <summary>
    /// The constructor
    /// </summary>
    public void Awake()
    {
		mainCamera =  CameraManager.GetCurrentCamera();
        mainCameraTransform = mainCamera.transform;
    }

    public override string[] GetCommands()
    {
        return new string[] {"SendMessage"};
    }

    public override string GetDescription()
    {
        return "Sends a message [parameter] to the targeted gameobject.";
    }

    public override void OnCommand(ConsoleContext context)
    {
        Send(context.Parameter);
    }

    protected virtual void Send(string parameter)
    {
        if (!SendMessageCommandWithValue(parameter, string.Empty))
        {
            Logger.LogError("Could not send message.");
        }
    }

    protected bool SendMessageCommandWithValue(string methodName, string parameter)
    {
        RaycastHit hitInfo;
        Vector3 startRay = mainCameraTransform.position;
        Vector3 directionRay = mainCameraTransform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(startRay, directionRay, out hitInfo))
        {
            Transform hitTransform = hitInfo.transform;
            Logger.LogSuccess("Sending message '" + methodName + "' (" + parameter + ") to " + hitTransform.name, hitTransform);
            hitTransform.SendMessage(methodName, parameter, SendMessageOptions.RequireReceiver);
            return true;
        }
        return false;
    }
}
