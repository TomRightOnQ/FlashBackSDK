using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FlackBackSDK: Base class for singleton managers
/// </summary>

public class FBGameSystem: MonoBehaviour 
{
    /// Life Cycle
    /// <summary>
    /// The begining code of the system
    /// All systems are created in the main game loop
    /// </summary>
    public virtual void OnSystemCreate()
    {
        this.tag = "Manager";
        DontDestroyOnLoad(this);
    }

    /// <summary>
    ///  Called when the game boot, after ALL systems created
    /// </summary>
    public virtual void OnSystemInit()
    {

    }

    /// <summary>
    /// Called when the scene is changed
    /// Will NOT be called when the game first started, but will do so for future scene change
    /// </summary>
    public virtual void OnSceneChange()
    {
    
    }

    /// <summary>
    /// Manual Init
    /// </summary>
    public virtual void ManualInit()
    {
    
    }
}
