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
    /// Called when a new scene is loading, complete before the scene change happen
    /// </summary>
    public virtual void OnSceneUnloaded()
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
    /// Called when all OnSceneChange is called
    /// All systems should be ready at this moment
    /// OnSceneLoadComplete should NOT contain anythin that relies on other OnSceneLoadComplete
    /// </summary>
    public virtual void OnSceneLoadComplete()
    {

    }

    /// <summary>
    /// Manual Init
    /// </summary>
    public virtual void ManualInit()
    {
    
    }
}
