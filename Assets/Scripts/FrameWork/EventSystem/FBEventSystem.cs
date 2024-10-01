using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FlackBackSDK MANAGER
/// Allowing the user to customize and throw events
/// </summary>
public class FBEventSystem : FBGameSystem
{
    // Dictionary to hold all the delegates, keyed by event type.
    private Dictionary<GameEvent.Event, List<Delegate>> eventDictionary = new Dictionary<GameEvent.Event, List<Delegate>>();

    // Queue to hold actions for execution
    private Queue<Action> actionQueue = new Queue<Action>();

    public override void OnSystemInit() { }

    public override void OnSceneUnloaded() 
    {
        CleanupInvalidListeners();
    }

    public override void OnSceneChange() { }

    public override void OnSceneLoadComplete() { }

    public override void ManualInit() { }

    // Public:
    /// <summary>
    /// Add a listener for a specific event type
    /// </summary>
    /// <param name="eventType">  Event Enum </param>
    /// <param name="listener"> CallBack functionm of the event </param>
    public void AddListener<T>(GameEvent.Event eventType, Action<T> listener)
    {
        if (!eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType] = new List<Delegate>();
        }

        eventDictionary[eventType].Add(listener);
    }

    /// <summary>
    /// Add a listener for a specific event type
    /// </summary>
    /// <param name="eventType">  Event Enum </param>
    /// <param name="listener"> CallBack functionm of the event </param>
    public void AddListener(GameEvent.Event eventType, Action listener)
    {
        if (!eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType] = new List<Delegate>();
        }

        eventDictionary[eventType].Add(listener);
    }

    /// <summary>
    /// Remove a listener for a specific event type
    /// </summary>
    /// <param name="eventType"> Event Enum </param>
    /// <param name="listener"> CallBack functionm of the event </param>
    public void RemoveListener<T>(GameEvent.Event eventType, Action<T> listener)
    {
        if (eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType].Remove(listener);
        }
    }

    /// <summary>
    /// Remove a listener for a specific event type
    /// </summary>
    /// <param name="eventType"> Event Enum </param>
    /// <param name="listener"> CallBack functionm of the event </param>
    public void RemoveListener(GameEvent.Event eventType, Action listener)
    {
        if (eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType].Remove(listener);
        }
    }

    /// <summary>
    /// Post an event of a specific type
    /// </summary>
    /// <param name="eventType"> Event Enum </param>
    /// <param name="T"> Event Parameters </param>
    public void PostEvent<T>(GameEvent.Event eventType, T eventParams)
    {
        if (eventDictionary.ContainsKey(eventType))
        {
            foreach (var delegateItem in eventDictionary[eventType])
            {
                if (delegateItem is Action<T> actionWithParams)
                {
                    // Add the action to the queue for execution
                    actionQueue.Enqueue(() => actionWithParams(eventParams));
                }
            }
        }

        // Execute all actions in the queue
        while (actionQueue.Count > 0)
        {
            Action action = actionQueue.Dequeue();
            action.Invoke();
        }
    }

    /// <summary>
    /// Post an event of a specific type
    /// </summary>
    /// <param name="eventType"> Event Enum </param>
    /// <param name="T"> Event Parameters </param>
    public void PostEvent(GameEvent.Event eventType)
    {
        if (eventDictionary.ContainsKey(eventType))
        {
            foreach (var delegateItem in eventDictionary[eventType])
            {
                if (delegateItem is Action actionWithParams)
                {
                    // Add the action to the queue for execution
                    actionQueue.Enqueue(() => actionWithParams());
                }
            }
        }

        // Execute all actions in the queue
        while (actionQueue.Count > 0)
        {
            Action action = actionQueue.Dequeue();
            action.Invoke();
        }
    }

    public void CleanupInvalidListeners()
    {
        var keysToRemove = new List<GameEvent.Event>();

        foreach (var pair in eventDictionary)
        {
            for (int i = pair.Value.Count - 1; i >= 0; i--)
            {
                if (pair.Value[i].Target == null || (pair.Value[i].Target is MonoBehaviour && !(pair.Value[i].Target as MonoBehaviour).gameObject.activeInHierarchy))
                {
                    pair.Value.RemoveAt(i);
                }
            }

            if (pair.Value.Count == 0)
            {
                keysToRemove.Add(pair.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            eventDictionary.Remove(key);
        }
    }
}
