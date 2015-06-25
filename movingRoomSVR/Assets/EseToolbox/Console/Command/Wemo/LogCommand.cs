using System;
using Ese;
using UnityEngine;


public class LogCommand : ConsoleCommand
{
    public override string[] GetCommands()
    {
        return new string[] { "log" };
    }

    public override string GetDescription()
    {
        return "Set loglevels\nCan be log HUDLEVEL or\nlog component HUDLEVEL or\nlog component {hud/console/server} LEVEL";
    }

    public override void OnCommand(ConsoleContext context)
    {
        Console console = GetComponent<Console>();
        if (console == null)
            return;

           if (context.Command == "log") {
            if (context.Parameter.IsNullOrEmpty()) {
                Console.Out("hud log level currently: " + Log.Main.threshold.hudLevel);
            }
            else {
                string[] args = context.Parameter.Split(' ');
                string component;
                string levelStr;
                bool doHud = true;
                bool doConsole = true;
                bool doServer = false;
                string whichOutput = "hud&console";

                if(args.Length == 1) {
                    component = "Main";
                    levelStr = args[0];
                }
                else if(args.Length == 2) {
                    component = args[0];
                    levelStr = args[1];
                }
                else {
                    component = args[0];
                    levelStr = args[1];
                    if (args[2].ToLower() == "console") {
                        doHud = false;
                        doConsole = true;
                        doServer = false;
                        whichOutput = "console";
                    }
                    else if (args[2].ToLower() == "hud") {
                        doHud = false;
                        doConsole = false;
                        doServer = true;
                        whichOutput = "hud";
                    }
                    else if (args[2].ToLower() == "server") {
                        doHud = false;
                        doConsole = false;
                        doServer = true;
                        whichOutput = "server";
                    }
                }

                Log log = Log.GetLog(component);
                Log.LogLevel level = Log.LevelFromString(levelStr);
                if(level != Log.LogLevel.NULL) {
                    if(doHud)
                        log.threshold.hudLevel = level;
                    if(doConsole)
                        log.threshold.consoleLevel = level;
                    if(doServer)
                        log.threshold.serverLevel = level;
                    Console.Out(whichOutput + " " + component + " log level set to : " + level);
                }
                else {
                    Console.Out("cannot set " + whichOutput + " " + component + " log  level to  : " + levelStr);
                }
            }
           }
    }
}
