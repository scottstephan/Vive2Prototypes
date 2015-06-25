using System;
using UnityEngine;

/// <summary>
/// Attach this to a GUIText to make a frames/second indicator.
///
/// It calculates frames/second over each updateInterval,
/// so the display does not keep changing wildly.
///
/// It is also fairly accurate at very low FPS counts below 10
/// 
/// We do this not by simply counting frames per interval, but
/// by accumulating FPS for each frame. This way we end up with
/// correct overall FPS even if the interval renders something like
/// 5.5 frames.
/// </summary>
public class FPSConsoleCounter 
{
    /// <summary>
    /// The way the fps will be drawn on screen.
    /// </summary>
    private const string FpsContent = "{0:F2} FPS";

    /// <summary>
    /// The interval at which the fps updates.
    /// </summary>
    private readonly float updateInterval = 0.5F;

    /// <summary>
    /// last time we checked what the FPS was.
    /// </summary>
    private float lastInterval = 0;
    
    /// <summary>
    /// Frames drawn over the interval
    /// </summary>
    private int frames = 0;
    
    /// <summary>
    /// The current FPS.
    /// </summary>
    private float fps = 0; 

    /// <summary>
    /// The gui style to use when drawing the fps on screen.
    /// </summary>
    private readonly GUIStyle style = new GUIStyle();
 
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="interval">The interval at which we update the fps</param>
    public FPSConsoleCounter(float interval)
    {
        updateInterval = interval;
    }

    public void Start()
    {
        style.alignment = TextAnchor.MiddleRight;
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
    }

    /// <summary>
    /// Runs every frame.
    /// </summary>
    public void Update()
    {
        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + updateInterval)
        {
            fps = frames / (timeNow - lastInterval);
            frames = 0;
            lastInterval = timeNow;
        }
    }

    /// <summary>
    /// Draws the fps on the screen.
    /// </summary>
    /// <param name="position"></param>
    public void DrawFPS(Rect position)
    {
        style.normal.textColor = GetFPSColor(fps);

        GUI.Label(position, string.Format(FpsContent, fps), style);
    }

    /// <summary>
    /// Retreives the color the fps should be drawn with.
    /// </summary>
    /// <param name="fps">the current fps</param>
    /// <returns>The color for the fps.</returns>
    private static Color GetFPSColor(float fps)
    {
        if (fps <= 60f)
        {
            if(fps <= 30f)
            {
                return Color.Lerp(Color.red, Color.yellow, fps / 30f);
            }
            return Color.Lerp(Color.yellow, Color.green, (fps - 30f) / 30f);
            
        }

        return Color.green;
    }
}