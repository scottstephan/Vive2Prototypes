using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public delegate void LoggingDelegate(Log.LogLevel eventLevel, string text);

// there is only ONE instance of the log class,
// but there are subclasses, which are wrappers
// or null or whatever wants to override
public class Log {
    public enum LogLevel
    {
        NULL,
        EVERYTHING,
        TRIVIAL,
        DEBUG,
        TRACE,
        INFO,
        WARNING,
        ERROR,
        FATAL,
        NOTHING,
    }

    public static LogLevel LevelFromString(string s) {
        switch (s.ToUpper()) {
            case "EVERYTHING" : return LogLevel.EVERYTHING;
            case "TRIVIAL" : return LogLevel.TRIVIAL;
            case "DEBUG" : return LogLevel.DEBUG;
            case "TRACE" : return LogLevel.TRACE;
            case "INFO" : return LogLevel.INFO;
            case "WARNING" : return LogLevel.WARNING;
            case "ERROR" : return LogLevel.ERROR;
            case "FATAL" : return LogLevel.FATAL;
            case "NOTHING" : return LogLevel.NOTHING;
            default: return LogLevel.NULL;
        }
    }

    [System.Serializable]
    public class LogThreshold {
        public string component;
        public LogLevel hudLevel;
        public LogLevel consoleLevel;
        public LogLevel serverLevel;

        // this is a default 'passthrough' threshold        
        public LogThreshold(string c) {
            component = c;
            hudLevel = LogLevel.NULL;
            consoleLevel = LogLevel.NULL;
            serverLevel = LogLevel.NULL;
        }

        // this is a 
        public LogThreshold(string c, LogLevel h, LogLevel f, LogLevel s) {
            component = c;
            hudLevel = h;
            consoleLevel = f;
            serverLevel = s;
        }

        // this is useful in an editor context
        public LogThreshold(LogThreshold other) {
            component = other.component;
            hudLevel = other.hudLevel;
            consoleLevel = other.consoleLevel;
            serverLevel = other.serverLevel;
        }
    }

    public static LoggingDelegate ConsoleLogging;

    public static LogThreshold DefaultLogThreshold = new LogThreshold("Main", LogLevel.NOTHING, LogLevel.NOTHING, LogLevel.NOTHING);

    public LogThreshold threshold;
    protected string moduleName;

    public string dateFormat = "ddd MMM d HH:mm ";

    public Log(string name) {
        moduleName = name;
        if(name == "Main")
            threshold = DefaultLogThreshold;
        else
            threshold = new LogThreshold(name);
    }

    // logging is not reentrant - prevent infinite loops
//    private static bool currentlyLogging = false;
	
	public bool WillLog( LogLevel level ) {
        // FILE logging
        LogLevel curLevel;
        curLevel = threshold.consoleLevel;
        // null - fallback to global
        if(curLevel == LogLevel.NULL) {
            curLevel = Log.Main.threshold.consoleLevel;
        }		
		
        return (level.CompareTo(curLevel) >= 0);
	}
	
    public void LogString(LogLevel eventLevel, string text) {
/*        string prefix = "" + eventLevel;
        string outText = prefix + ": " + moduleName + ": " + text;

        // console [stdout and, if EDITOR, also player console], hud, then server

        // FILE logging
        LogLevel curLevel;
        curLevel = threshold.consoleLevel;
        // null - fallback to global
        if(curLevel == LogLevel.NULL) {
            curLevel = Log.Main.threshold.consoleLevel;
        }

        // console
        //DEBUG
        if (eventLevel.CompareTo(curLevel) >= 0) {
            DateTime time = DateTime.Now;
            Console.Write(time.ToString(dateFormat));
            System.Console.WriteLine(outText);
        }

        if (currentlyLogging) {
            return;
        }
        
        // the remainder of logging is not re-entraant...
        currentlyLogging = true;

        // Player.log -- editor only
#if UNITY_EDITOR
        if (eventLevel.CompareTo(curLevel) >= 0) {
            
            if (eventLevel == LogLevel.FATAL) {
                UnityEngine.Debug.LogError(outText);
            }
            else if (eventLevel == LogLevel.ERROR) {
                // errors could be downgraded to warnings in the EDITOR.
                UnityEngine.Debug.LogError(outText);
                // UnityEngine.Debug.LogWarning(outText);        
            }
            else if(eventLevel == LogLevel.WARNING) {
                UnityEngine.Debug.LogWarning(outText);        
            }
            else {
                //UnityEngine.Debug.Log(outText);        
            }
        }
#endif

        // HUD display
        curLevel = threshold.hudLevel;
        // null - fallback to global
        if(curLevel == LogLevel.NULL) {
            curLevel = Log.Main.threshold.hudLevel;
        }
        if (eventLevel.CompareTo(curLevel) >= 0) {
            if ( ConsoleLogging != null ) {
                ConsoleLogging(eventLevel, outText);
            }
        }

        // server logging
        curLevel = threshold.serverLevel;
        // null - fallback to global
        if(curLevel == LogLevel.NULL) {
            curLevel = Log.Main.threshold.serverLevel;
        }

        currentlyLogging = false;*/
    }

    // this is how to customize subclasses
    public virtual void RouteLog(LogLevel eventLevel, string text) {
        LogString(eventLevel, text);
    }

    public void Trivial(string text) { RouteLog(LogLevel.TRIVIAL, text); }
    public void Debug(string text) { RouteLog(LogLevel.DEBUG, text); }
    public void Trace(string text) { RouteLog(LogLevel.TRACE, text); }
    public void Info(string text) { RouteLog(LogLevel.INFO, text); }
    public void Warning(string text) { RouteLog(LogLevel.WARNING, text); }
    public void Error(string text) { RouteLog(LogLevel.ERROR, text); }
    public void Fatal(string text) { RouteLog(LogLevel.FATAL, text); }
    public void State(string text) { RouteLog(LogLevel.EVERYTHING, text); }

    // global table of loggers
    private static Dictionary<string, Log> logTable = new Dictionary<string, Log>();
    public static Log GetLog(string name) {
        if(logTable.ContainsKey(name)) {
            return logTable[name];
        }
        Log newLog = new Log(name);
        logTable.Add(name, newLog);
        return newLog;
    }

    // global singleton main logger
    private static Log _singleton = null;
    public static Log Main {
        get {
            if (_singleton == null) {
                _singleton = new Log("Main");
            }
            return _singleton;
        }
    }
}

// a future idea - optimized removal of logging?
public class NullLog : Log {
    public NullLog() : base("null") {}
    public override void RouteLog(LogLevel level, string text) 
        // junk it
        {}
}
