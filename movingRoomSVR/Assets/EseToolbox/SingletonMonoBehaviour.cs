using UnityEngine;

/// <summary>
/// This class is a singleton. Only one instance of this class can exist.
/// </summary>
public class SingletonMonoBehaviour<T> : MonoBehaviour where T : Component
{
    /// <summary>
    /// Should this singleton create an instance of itself if it cant find itself in the scene? Defaults to no.
    /// </summary>
    protected static bool IsCreateInstance = false;
    
    /// <summary>
    /// Has this singleton given out a warning?
    /// </summary>
    private static bool hasWarned = false;

    /// <summary>
    /// The cached instance
    /// </summary>
    private static T instance;

    /// <summary>
    /// The singleton instance of this class.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (instance == null)
            {

                instance = (T)FindObjectOfType(typeof(T));

                if (instance == null)
                {
                    if (IsCreateInstance)
                    {
                        instance = CreateSingletonInstance();
                    }
                    else if(!hasWarned)
                    {
                        Debug.LogWarning("Attempt to access " + typeof(T).GetType().Name + " singleton before Start or without singleton being attached.");
                        hasWarned = true;
                    }
                }
            }

            return instance;
        }
    }

    /// <summary>
    /// Create a gameobject with itself attached.
    /// </summary>
    /// <returns>Return an instance of itself.</returns>
    protected static T CreateSingletonInstance()
    {
        GameObject gameObject = new GameObject(typeof(T).GetType().Name);
        Debug.LogWarning("Could not find " + gameObject.name + ", creating", gameObject);
        return gameObject.AddComponent<T>();
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    public void OnDestroy()
    {
        ResetInstance();
    }

    /// <summary>
    /// Sent to all game objects before the application is quit.
    /// </summary>
    public void OnApplicationQuit()
    {
        ResetInstance();
    }

    /// <summary>
    /// Destroys the singleton. Important for cleaning up the static reference.
    /// </summary>
    protected void ResetInstance()
    {
        instance = null;
    }
}
