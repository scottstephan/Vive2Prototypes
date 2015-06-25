using UnityEngine;

public delegate void FatalDelegate( object obj );
public delegate void BasicDelegate();

public enum ErrorResolutionType {
	Restart,
	Continue,
	Quit
}

public class GameStateManager : SingletonBehaviour<GameStateManager> {
    static GameStateManager()
    {
        IsCreateInstance = true;
    }

    public static void Fatal(object obj) {
        Fatal(obj.ToString(), obj.ToString());
    }
	
	public static void FatalQuit(object obj) {
		FatalQuit(obj.ToString(), obj.ToString());
    }

    // quit with a message to the ui and a separate message to the logs
    public static void FatalQuit(string uiMessage, string logMessage) {
		Debug.LogError( "ERROR FATAL QUIT: " + uiMessage + " :: " + logMessage );
    }

    // we don't want to show the user the stacktrace, but that goes in the log
    // (and we get different results if we extrace the stacktrace from here!)
    public static void Fatal(string uiMessage, string logMessage) {
		Debug.LogError( "ERROR FATAL: " + logMessage );
    }
	
    // this is like Fatal, but doesn't issue a fatal in the log and always quits
    public static void ErrorQuit(string uiMessage) {
		Debug.LogError( "ERROR QUIT: " + uiMessage );
    }
}
