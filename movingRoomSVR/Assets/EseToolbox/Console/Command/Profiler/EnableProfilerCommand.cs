using System;
using System.Collections;
using Ese;
using UnityEngine;


public class EnableProfilerCommand : ConsoleCommand
{
    public static int FrameStartedProfiler = int.MinValue;
    private bool isProfiling = false;
    private int frameAmount = 0;

    public override string[] GetCommands()
    {
        return new string[] { "EnableProfiler", "StartProfiler", "ProfileGame", "Profile" };
    }

    public override void OnCommand(ConsoleContext context)
    {
        int value = GetContextIntValue(context);
        if(value > 0 && !isProfiling)
        {
            StartCoroutine(EnableProfile(value));
        }
    }

    private IEnumerator EnableProfile(int value)
    {
        isProfiling = true;

        frameAmount = value;
        string fileName = string.Format("profiler_data_{0}_{1}.log",
                             Application.loadedLevelName,
                             DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

        string previousLogFileName = ToggleProfiler(fileName, true);
        Logger.LogSuccess("Writing profiling information to " + fileName + " for " + frameAmount + " amount of frames");

        FrameStartedProfiler = Time.frameCount;
        while(FrameStartedProfiler + frameAmount > Time.frameCount)
        {
            yield return null;
        }

        ToggleProfiler(previousLogFileName, false);
        Logger.LogSuccess("Ended profiling session.");
        isProfiling = false;
    }

    private static string ToggleProfiler(string fileName, bool toggle)
    {
        string previousLogFileName = Profiler.logFile;
        Profiler.logFile = fileName;
//3.5        Profiler.enableBinaryLog = toggle;
        Profiler.enabled = toggle;
        return previousLogFileName;
    }
}