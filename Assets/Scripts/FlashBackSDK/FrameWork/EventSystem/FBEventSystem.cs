using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FlackBackSDK MANAGER
/// Allowing the user to customize and throw events
/// </summary>
public class FBEventSystem : FBGameSystem
{
    // Dictionary to hold all the delegates, keyed by event type.
    private Dictionary<GameEvent.Event, System.Action> eventDictionary;

    public override void OnSystemInit()
    {

    }

    public override void OnSceneChange()
    {

    }

    public override void ManualInit()
    {

    }

    // Public:
    /// <summary>
    /// Add a listener for a specific event type
    /// </summary>
    /// <param name="eventType">  Event Enum </param>
    /// <param name="listener"> CallBack functionm of the event </param>
    public void AddListener(GameEvent.Event eventType, System.Action listener)
    {
        if (!eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType] = null;
        }

        eventDictionary[eventType] += listener;
    }

    /// <summary>
    /// Remove a listener for a specific event type
    /// </summary>
    /// <param name="eventType"> Event Enum </param>
    /// <param name="listener"> CallBack functionm of the event </param>
    public void RemoveListener(GameEvent.Event eventType, System.Action listener)
    {
        if (eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType] -= listener;
        }
    }

    /// <summary>
    /// Post an event of a specific type
    /// </summary>
    /// <param name="eventType"> Event Enum </param>
    public void PostEvent(GameEvent.Event eventType)
    {
        if (eventDictionary.ContainsKey(eventType) && eventDictionary[eventType] != null)
        {
            eventDictionary[eventType].Invoke();
        }
    }
}
