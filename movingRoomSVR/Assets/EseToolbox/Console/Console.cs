using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ese;
using Ese.Events;
using UnityEngine;
using EventType = Ese.EventType;
using Input = Ese.Input;
using Math = Ese.Math;

/// <summary>
/// The manager class for logging messages to the screen in unity.
/// </summary>
public class Console : SingletonMonoBehaviour<Console>
{
    /// <summary>
    /// The logo to be displayed in the bottom right corner of the console.
    /// </summary>
    [SerializeField] 
    private Texture2D logo;
    private const string LogoPath = "Assets/EseToolbox/Editor/ese_logo.psd";

    /// <summary>
    /// The color of the background in the console.
    /// </summary>
    [SerializeField] 
    private Color backgroundColor = new Color(41f, 41f, 41f, 1f);
        
    // class log
    private static Log log = Log.GetLog(typeof(Console).FullName); 

    /// <summary>
    /// The buttons you can push on the keyboard to make it appear.
    /// </summary>
    [SerializeField] 
    private string consoleKeys = new string(new char[]
                                                    {
                                                        '~', // The original?
                                                        '+', // Plus
                                                        '|', // Norwegian
                                                        '^', // German
                                                        '`', // American
                                                        '½', // Danish
                                                        '§', // Swedish and Finnish
                                                        'ё', // Russion
                                                        '²', // Dutch
                                                        'º', // Spain
                                                        '\'' // Portugal
                                                    });

    /// <summary>
    /// The virtual button you can push on the keyboard to make it appear.
    /// </summary>
    [SerializeField] 
    private string[] consoleButtons = null;

    /// <summary>
    /// This will show or hide the FPS counter in the top right.
    /// </summary>
    [SerializeField] 
    public bool ShowFPS = false;

    /// <summary>
    /// The crosshair to show when using the console.
    /// </summary>
    [SerializeField] 
    private string crossHairSymbol = "+";


    /// <summary>
    /// Is the console open
    /// </summary>
    [HideInInspector] 
    public bool IsOpen = false;

    [HideInInspector] 
    public bool consoleEnabled = false;
    
    [HideInInspector]
    public bool forceToggle = false;

    /// <summary>
    /// The height of the console
    /// </summary>
    [HideInInspector] 
    public float ConsoleHeight;

    /// <summary>
    /// The console commands.
    /// </summary>
    public readonly Dictionary<string, ConsoleCommand> ConsoleCommands = new Dictionary<string, ConsoleCommand>();

    /// <summary>
    /// The y position of the bottom part of the console. 
    /// This variable lerps towards the console height when the console is opened.
    /// </summary>
    public float BottomPosition { get; set; }


    private readonly GUIStyle crossHairGUIStyle = new GUIStyle();
    private const float CrossHairWidth = 100f;
    private const float CrossHairHeight = 20f;
    private const float TextFieldHeight = 24f;
        
    private readonly LinkedList<LogEvent> logEvents = new LinkedList<LogEvent>();
    private readonly FPSConsoleCounter counter = new FPSConsoleCounter(0.5F);
    private const float TextHeight = 22f;
    private const string TextWithObject = "{0} ({1})";
    private const string TextWithoutObject = "{0}";
    private const int EventCount = 60;
    private Texture2D successIcon;
    private Texture2D infoIcon;
    private Texture2D warningIcon;
    private Texture2D errorIcon;
    private ConsoleTextField textField;

    private static char[] consoleCharKeys;


    /// <summary>
    /// Callback for log events to the console.
    /// </summary>
    /// <param name="logEvent"></param>
    public event Action<LogEvent> LogEvent;

    /*/// <summary>
    /// Register a callback for the LogEvents.
    /// </summary>
    /// <param name="callback"></param>
    public void RegisterLogCallback(LogCallback callback)
    {
        callbacks.Add(callback);
    }*/

    /// <summary>
    /// The constructor
    /// </summary>
    public void Awake()
    {
        log.Trace("Awakening");
        useGUILayout = false;
        textField = new ConsoleTextField(this, TextFieldHeight);
        logo = LoadLogo(logo, LogoPath);
        infoIcon = LoadIcon(EventType.Info);
        warningIcon = LoadIcon(EventType.Warning);
        errorIcon = LoadIcon(EventType.Error);
        successIcon = LoadIcon(EventType.Success);
		ConsoleHeight =  100f;//Screen.GetResolution[0].height / 3f;
        GetConsoleCommands();
        crossHairGUIStyle.alignment = TextAnchor.MiddleCenter;
        consoleCharKeys = consoleKeys.ToCharArray();
        Log.ConsoleLogging += LogThisEvent;
//        Application.RegisterLogCallback(AddEventsFromUnityLog);
        counter.Start();
    }

    public void Start() {
        ConsoleHeight =  5 * Screen.height / 6f;
    }

    public static void Out(string text) {
        Instance.AddLogEventToShow(new LogEvent(text, null));
    }

    public static void EnableTheConsole() {
        if (Instance == null) {
            return;
        }

        Instance.consoleEnabled = true;
    }

    public static void LogThisEvent(Log.LogLevel eventLevel, string text) {
        if( Instance == null ) {
            return;
        }
        
        switch(eventLevel)
        {
            case Log.LogLevel.FATAL:
            case Log.LogLevel.ERROR:
                Instance.AddLogEventToShow(new ErrorEvent(text, null));
                break;
            case Log.LogLevel.WARNING:
                Instance.AddLogEventToShow(new WarningEvent(text, null));
                break;
            case Log.LogLevel.INFO:
                Instance.AddLogEventToShow(new SuccessEvent(text, null));
                break;
            default:
                Instance.AddLogEventToShow(new LogEvent(text, null));
                break;
        }
    }

    private void AddEventsFromUnityLog(string condition, string stackTrace, LogType type)
    {
        string callback = string.Format("{0}\n{1}", condition, stackTrace);
        switch(type)
        {
            case LogType.Error:
                AddLogEventToShow(new ErrorEvent(callback, null));
                break;
            case LogType.Assert:
                AddLogEventToShow(new ErrorEvent(callback, null));
                break;
            case LogType.Warning:
                AddLogEventToShow(new WarningEvent(callback, null));
                break;
            case LogType.Log:
                if (condition.StartsWith("Success!"))
                {
                    AddLogEventToShow(new SuccessEvent(callback, null));
                }
                else
                {
                    AddLogEventToShow(new LogEvent(callback, null));
                }
                break;
            case LogType.Exception:
                AddLogEventToShow(new ErrorEvent(callback, null));
                break;
        }
    }

    private Texture2D LoadIcon(EventType type)
    {
        Texture2D texture = new Texture2D(1, 1);
            
        switch(type)
        {
            case EventType.Info:
                texture.SetPixel(0, 0, Color.grey);
                break;
            case EventType.Warning:
                texture.SetPixel(0, 0, Color.yellow);
                break;
            case EventType.Error:
                texture.SetPixel(0, 0, Color.red);
                break;
            case EventType.Success:
                texture.SetPixel(0, 0, Color.green);
                break;
        }

        texture.Apply();

        return texture;
    }

    private void GetConsoleCommands()
    {
        ConsoleCommand[] commandComponents = GetComponents<ConsoleCommand>();
        foreach (ConsoleCommand command in commandComponents)
        {
            foreach(string textCommand in command.GetCommands())
            {   
                string key = textCommand.ToLower();
                if (!ConsoleCommands.ContainsKey(key))
                {
                    ConsoleCommands.Add(key, command);
                }
            }
        }
    }

    /// <summary>
    /// Sets the default height of the console.
    /// </summary>
    /// <param name="height">The height the console will get.</param>
    public void SetConsoleHeight(float height)
    {
        ConsoleHeight = height;
        if(IsOpen)
        {
            BottomPosition = ConsoleHeight;
        }
    }

    /// <summary>
    /// This method is run every frame.
    /// </summary>
    public void Update()
    {
        bool enableKeyDown = false;
        if ( UnityEngine.Input.GetKeyDown(KeyCode.F6) || forceToggle) {
            consoleEnabled = true;
            enableKeyDown = true;
            forceToggle = false;
        }

        if (!consoleEnabled)
            return;

        if (enableKeyDown || IsConsoleCommandKeyDown())
        {
            if (!IsOpen)
            {
                textField.ClearText();
            }
            StartCoroutine(LerpConsolePosition(!IsOpen, 0, ConsoleHeight, 0.1f));
        }
        
        counter.Update();
            
    }

    private bool IsConsoleCommandKeyDown()
    {
        return (!consoleButtons.IsNullOrEmpty() && Input.ButtonDown(consoleButtons)) || Input.KeyDown(consoleCharKeys);
    }

    private IEnumerator LerpConsolePosition(bool show, float startPosition, float endPosition, float time)
    {
        if(show)
        {
            IsOpen = true;
        }

        float i = 0.0f;
        float rate = 1.0f / time;
        if (show)
        {
            while (i < 1.0f)
            {
                i += Time.deltaTime * rate;
                BottomPosition = Math.Lerp(startPosition, endPosition, i);
                yield return null;
            }
        }
        else
        {
            while (i < 1.0f)
            {
                i += Time.deltaTime * rate;
                BottomPosition = Math.Lerp(endPosition, startPosition, i);
                yield return null;
            }
        }
        IsOpen = show;
    }

        
    /// <summary>
    /// This method is run 4 times per frame and handles all the GUI events.
    /// </summary>
    public void OnGUI()
    {
        if (IsOpen)
        {
            if (logo)
            {
                DrawLogoScreen();
            }

            ShowLastLogEvents();
            textField.OnGUI();

            Rect crossHairRectangle = new Rect((Screen.width / 2f) - (CrossHairWidth / 2f), (Screen.height / 2f) - (CrossHairHeight / 2f), CrossHairWidth, CrossHairHeight);
            GUI.Label(crossHairRectangle, crossHairSymbol, crossHairGUIStyle);
        }

        if (ShowFPS)
        {
            counter.DrawFPS(new Rect(Screen.width - 100f, 0, 100f, TextHeight));
        }
    }

    private Texture2D LoadLogo(Texture2D logoToLoad, string path)
    {
        if (logoToLoad == null)
        {
            logoToLoad = Resources.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
            if (logoToLoad == null)
            {
                Logger.LogWarning("Could not find ese logo at " + LogoPath, gameObject);
            }
        }
        return logoToLoad;
    }

    private void DrawLogoScreen()
    {
        GUI.color = new Color(1f, 1f, 1f, 1f);

        Texture2D blackBackgroundTexture = new Texture2D(1, 1);
        blackBackgroundTexture.SetPixel(0, 0, backgroundColor);
        blackBackgroundTexture.Apply();

        GUIStyle backgroundStyle = new GUIStyle {normal = {background = blackBackgroundTexture}};

        GUI.Label(new Rect(0, 0, Screen.width, BottomPosition), string.Empty, backgroundStyle);

        GUIStyle logoStyle = new GUIStyle {normal = {background = logo}};
        GUI.Label(new Rect(Screen.width - logo.width, BottomPosition - logo.height - TextFieldHeight, logo.width, logo.height), string.Empty, logoStyle);
    }

    private void ShowLastLogEvents()
    {
        if(!logEvents.IsNullOrEmpty())
        {
            float yPosition = BottomPosition - TextFieldHeight;
            logEvents.Aggregate(yPosition, CreateLogEvent);
        }
    }

    private float CreateLogEvent(float yPosition, LogEvent logEvent)
    {
        if (logEvent != null)
        {
            yPosition = ShowLogEvent(logEvent, yPosition);
        }
        return yPosition;
    }

    private float ShowLogEvent(LogEvent logEvent, float yPosition)
    {
        float heightToUse = 20f;//GetHeightOfContent(logEvent.Content.Text);

        const float iconSize = 20f;
        const float inset = 4f;
        Rect iconRect = new Rect(inset, yPosition - heightToUse + inset, iconSize - (inset * 2), iconSize - (inset * 2));
        GUIStyle backgroundStyle = new GUIStyle {normal = {background = GetLogEventIcon(logEvent.Type)}};

        GUI.Label(iconRect, string.Empty, backgroundStyle);

        GUIStyle labelStyle = new GUIStyle("Label")
                                    {
                                        alignment = TextAnchor.UpperLeft,
                                        contentOffset = Vector2.zero,
                                        margin = new RectOffset(0, 0, 0, 0)
                                    };

        Rect textRect = new Rect(iconSize, yPosition - heightToUse, Screen.width, heightToUse);
        GUI.Label(textRect, GetLogEventText(logEvent), labelStyle);
        return yPosition - heightToUse;
    }

    private string GetLogEventText(LogEvent logEvent)
    {
        if (logEvent.Context != null)
        {
            return string.Format(TextWithObject, logEvent.Content.OriginalText, logEvent.Context.name);
        }
        return string.Format(TextWithoutObject, logEvent.Content.OriginalText);
    }

    private Texture2D GetLogEventIcon(EventType type)
    {
        switch(type)
        {
            case EventType.Info:
                return infoIcon;
            case EventType.Warning:
                return warningIcon;
            case EventType.Error:
                return errorIcon;
            case EventType.Success:
                return successIcon;
        }
        return null;
    }

    private static float GetHeightOfContent(string text)
    {
        int count = 1;
        if (text != null)
        {
            count += text.Count(c => c.Equals('\n'));
        }

        if(count == 1)
        {
            return TextHeight;
        }

        return 4f + count * 16f;
    }

    /// <summary>
    /// Adds a log event to this gui manager.
    /// </summary>
    /// <param name="logEventToAdd"></param>
    public void AddLogEventToShow(LogEvent logEventToAdd)
    {
        if (logEvents.Count >= EventCount)
        {
            logEvents.RemoveLast();
        }
        logEvents.AddFirst(logEventToAdd);

        if(LogEvent != null)
        {
            LogEvent(logEventToAdd);
        }
    }

    /// <summary>
    /// Clears the console of text.
    /// </summary>
    public void Clear()
    {
        logEvents.Clear();
    }
}