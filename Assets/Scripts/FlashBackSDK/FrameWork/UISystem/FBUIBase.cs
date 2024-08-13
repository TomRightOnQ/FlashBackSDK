using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class of FlashBack SDK UIs
/// </summary>
public class FBUIBase : FBObject
{
    // Whether the UI has been opened once
    [SerializeField, ReadOnly] public bool bHasOpenedOnce = false;
    public bool HasOpened => bHasOpenedOnce;
    [SerializeField, ReadOnly] public bool bShowing = false;
    public bool IsShowing => bShowing;

    // LifeCycle
    /// <summary>
    /// Init is called automatically, DO NOT OVERRIDE
    /// </summary>
    public virtual void Init()
    {
        this.tag = "UI";
    }

    /// <summary>
    /// Called when the UI is created
    /// </summary>
    public virtual void OnCreate()
    {
    
    }

    /// <summary>
    /// Called when the UI is shown FOR THE FIRST TIME
    /// </summary>
    public virtual void OnOpen()
    {

    }


    /// <summary>
    /// Called after OnOpen whenever the UI is shown
    /// </summary>
    public virtual void OnRefresh()
    {
    
    }

    /// <summary>
    /// Called when the UI is hidden
    /// The UI's GameObject is set as NOT Active after this
    /// </summary>
    public virtual void OnHide()
    {
    
    }

    /// <summary>
    /// Called when the UI is manually destroyed
    /// This will NOT be called when automatically deleted upon scene change
    /// </summary>
    public virtual void OnRemove()
    {
    
    }
}
