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
///     10th of July, 2011
/// 
/// Purpose:
///     Destroys the target gameobject
/// </summary>
public class DestroyCommand : ConsoleCommand
{
    private Camera mainCamera;
    private Transform mainCameraTransform;

    /// <summary>
    /// The constructor
    /// </summary>
    public void Awake()
    {
		mainCamera = CameraManager.GetCurrentCamera();
        mainCameraTransform = mainCamera.transform;
    }

    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>"Destroy", "DestroyGameObject"</returns>
    public override string[] GetCommands()
    {
        return new string[] { "Destroy", "DestroyGameObject" };
    }

    /// <summary>
    /// The description of the console command.
    /// </summary>
    /// <returns>"Destroys targeted gameobject"</returns>
    public override string GetDescription()
    {
        return "Destroys targeted gameobject";
    }

    /// <summary>
    /// Perform this action when the command in GetCommand is received.
    /// </summary>
    /// <param name="context"></param>
    public override void OnCommand(ConsoleContext context)
    {
        if (!DestroyObject())
        {
            Logger.LogError("Could not destroy object");
        }
    }

    protected bool DestroyObject()
    {
        RaycastHit hitInfo;
        Vector3 startRay = mainCameraTransform.position;
        Vector3 directionRay = mainCameraTransform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(startRay, directionRay, out hitInfo))
        {
            GameObject hitGameObject = hitInfo.transform.gameObject;
            Logger.LogSuccess("Destroying " + hitGameObject.name, hitGameObject);
            Destroy(hitGameObject);
            return true;
        }
        return false;
    }
}
