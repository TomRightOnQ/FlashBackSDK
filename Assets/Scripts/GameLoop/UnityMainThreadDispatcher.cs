using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;
    private static readonly Queue<Action> executionQueue = new Queue<Action>();
    private static readonly object lockObj = new object();

    // Simple way to check if we're on main thread
    public static bool IsMainThread => UnityEngine.SystemInfo.renderingThreadingMode ==
                                     UnityEngine.Rendering.RenderingThreadingMode.Direct;

    public static UnityMainThreadDispatcher Instance
    {
        get
        {
            if (instance == null)
            {
                throw new System.InvalidOperationException(
                    "MainThreadDispatcher not initialized. " +
                    "Call InitializeMainThreadDispatcher() in FBMainGame.Init() first.");
            }
            return instance;
        }
    }

    public static bool InstanceExists => instance != null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Enqueue(Action action)
    {
        lock (lockObj)
        {
            executionQueue.Enqueue(action);
        }
    }

    private void Update()
    {
        lock (lockObj)
        {
            while (executionQueue.Count > 0)
            {
                try
                {
                    executionQueue.Dequeue()?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"MainThreadDispatcher error: {e}");
                }
            }
        }
    }

    public static void EnsureInitialized()
    {
        if (instance == null)
        {
            Debug.LogError("MainThreadDispatcher was accessed before initialization!");
        }
    }
}