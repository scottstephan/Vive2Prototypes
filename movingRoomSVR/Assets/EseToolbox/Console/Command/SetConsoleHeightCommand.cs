using System;
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
///     Sets the console height.
/// </summary>
public class SetConsoleHeightCommand : ConsoleCommand
{
    public override string[] GetCommands()
    {
        return new string[] { "SetConsoleHeight" , "consoleheight"};
    }

    public override string GetDescription()
    {
        return "Sets the height of the console";
    }

    public override void OnCommand(ConsoleContext context)
    {
        Console console = GetComponent<Console>();
        if (console != null)
        {
            if (context.Parameter.IsNullOrEmpty())
            {
                console.BottomPosition = Screen.height / 3f;
            }
            else
            {
                float number = GetContextFloatValue(context);
                if(number >= 0)
                {
                    console.BottomPosition = number;
                    Console.Out("Setting the console height to be: " + number);
                }
            }
        }
    }
}
