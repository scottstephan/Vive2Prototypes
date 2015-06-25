using System;
using Ese;
using UnityEngine;


/// <summary>
/// A console command.
/// </summary>
[RequireComponent(typeof(Console))]
public abstract class ConsoleCommand : MonoBehaviour
{
    private const string NotAvailableContent = "Not available";

    /// <summary>
    /// This is the command that needs to be typed in for this 
    /// console command to receive the OnCommand event
    /// </summary>
    /// <returns>The command</returns>
    public abstract string[] GetCommands();

    /// <summary>
    /// The description of the console command.
    /// </summary>
    /// <returns>The description</returns>
    public virtual string GetDescription()
    {
        return NotAvailableContent;
    }

    /// <summary>
    /// Perform this action when the command in GetCommand is received.
    /// </summary>
    /// <param name="context">The command and parameter</param>
    public abstract void OnCommand(ConsoleContext context);


    public static int GetContextIntValue(ConsoleContext context)
    {
        int value = 0;
        try
        {
            value = int.Parse(context.Parameter);
        }
        catch (FormatException)
        {
            Logger.LogError("Could not parse '" + context.Parameter + "' to a number");
        }
        return value;
    }

    public static float GetContextFloatValue(ConsoleContext context)
    {
        float value = -1;
        try
        {
            value = float.Parse(context.Parameter);
        }
        catch (FormatException)
        {
            Logger.LogError("Could not parse '" + context.Parameter + "' to a number");
        }
        return value;
    }
}