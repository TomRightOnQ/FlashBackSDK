using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FlackBackSDK MANAGER
/// Timer process
/// </summary>
public class FBTimerSystem : FBGameSystem
{
    // Buckets for better performance
    private List<FBTimer> tickTimers = new List<FBTimer>();        // Frame-based
    private List<FBTimer> shortTimers = new List<FBTimer>();      // < 1s
    private List<FBTimer> mediumTimers = new List<FBTimer>();     // 1-5s
    private List<FBTimer> longTimers = new List<FBTimer>();       // > 5s

    public static float SHORT_TIMER_MAX_TIME = 1f;
    public static float MEDIUM_TIMER_MAX_TIME = 5f;

    private Dictionary<int, FBTimer> timerDictionary = new Dictionary<int, FBTimer>();
    private Queue<int> timersToRemove = new Queue<int>();
    private int nextTimerID = 1;

    // Process short/medium/long timers every 0.1s
    public float TickInterval = 0.1f;
    private float nextBucketProcessTime;

    public override void OnSystemInit() { }

    public override void OnSceneUnloaded()
    {
        // CAREFULLY CHANGE HERE TO AVOID SEQUENTIAL ISSUE
    }

    public override void OnSceneChange()
    {
        // CAREFULLY CHANGE HERE TO AVOID SEQUENTIAL ISSUE
    }

    public override void OnSceneLoadComplete()
    {
        nextBucketProcessTime = Time.time + TickInterval;
    }

    // public:
    /// <summary>
    /// Add a timer
    /// </summary>
    /// <param name="timeInterval"> Time between each cycle </param>
    /// <param name="invokeNums"> Total nums of invoke, -1 for infinity </param>
    /// <param name="caller"> The calling object </param>
    /// <param name="callback"> Callback function </param>
    /// <returns></returns>
    public int AddTimer(float timeInterval, int invokeNums, FBTimerUser caller, Action callback)
    {
        // Validate
        if (caller == null)
        {
            FBDebug.Instance.FBLogError("Cannot add timer with null caller", this.gameObject);
            return -1;
        }

        if (callback == null)
        {
            FBDebug.Instance.FBLogError("Cannot add timer with null callback", this.gameObject);
            return -1;
        }

        if (caller == null)
        {
            FBDebug.Instance.FBLogError($"Failed to create timer user for {caller.GetType().Name}", this.gameObject);
            return -1;
        }

        int id = nextTimerID++;

        var timer = new FBTimer(id, timeInterval, invokeNums, caller, callback, TimerType.Normal);
        timer.NextTriggerTime = Time.time + timeInterval;

        // Add to appropriate bucket
        AddTimerToBucket(timer);
        timerDictionary[id] = timer;

        // Register with user
        caller.RegisterTimer(id);
        return id;
    }

    public int AddTickTimer(int invokeNums, FBTimerUser caller, Action callback)
    {
        // Validate
        if (caller == null)
        {
            FBDebug.Instance.FBLogError("Cannot add timer with null caller", this.gameObject);
            return -1;
        }

        if (callback == null)
        {
            FBDebug.Instance.FBLogError("Cannot add timer with null callback", this.gameObject);
            return -1;
        }

        if (caller == null)
        {
            FBDebug.Instance.FBLogError($"Failed to create timer user for {caller.GetType().Name}", this.gameObject);
            return -1;
        }

        int id = nextTimerID++;

        var timer = new FBTimer(id, 0, invokeNums, caller, callback, TimerType.Tick);

        // Add to tick bucket
        tickTimers.Add(timer);
        timerDictionary[id] = timer;

        // Register with user
        caller.RegisterTimer(id);

        return id;
    }

    /// <summary>
    /// Delete one timer internally
    /// </summary>
    /// <param name="timerID"></param>
    public void DeleteTimer(int timerID, bool bExternallyRemove = false)
    {
        if (timerDictionary.TryGetValue(timerID, out FBTimer targetTimer))
        {
            if (targetTimer != null)
            {
                // Notify user if this is external removal
                if (bExternallyRemove && targetTimer.User != null)
                {
                    targetTimer.User.OnTimerCanceled(timerID);
                }

                // Mark for removal
                timersToRemove.Enqueue(timerID);

                // Remove from bucket immediately to prevent execution
                RemoveTimerFromBucket(targetTimer);
            }
        }
    }

    public void DeleteAllTimers(FBTimerUser userObject)
    {
        userObject.DeleteAllTimers();
    }

    private void AddTimerToBucket(FBTimer timer)
    {
        if (timer.Interval < SHORT_TIMER_MAX_TIME)
            shortTimers.Add(timer);
        else if (timer.Interval < MEDIUM_TIMER_MAX_TIME)
            mediumTimers.Add(timer);
        else
            longTimers.Add(timer);
    }

    private void RemoveTimerFromBucket(FBTimer timer)
    {
        switch (timer.Type)
        {
            case TimerType.Tick:
                tickTimers.Remove(timer);
                break;
            default:
                if (timer.Interval < 1f)
                    shortTimers.Remove(timer);
                else if (timer.Interval < 5f)
                    mediumTimers.Remove(timer);
                else
                    longTimers.Remove(timer);
                break;
        }
    }

    private void ClearAllTimers()
    {
        tickTimers.Clear();
        shortTimers.Clear();
        mediumTimers.Clear();
        longTimers.Clear();
        timerDictionary.Clear();
        timersToRemove.Clear();
        nextTimerID = 1;
        FBDebug.Instance.FBLog("All timers cleared", this.gameObject);
    }

    #region TimerUpdates
    void Update()
    {
        float currentTime = Time.time;
        float unscaledTime = Time.unscaledTime;

        // Process tick timers every frame
        ProcessTimerList(tickTimers, currentTime, unscaledTime, true);

        // Process other timers in batches for performance
        if (currentTime >= nextBucketProcessTime)
        {
            ProcessTimerList(shortTimers, currentTime, unscaledTime, false);
            ProcessTimerList(mediumTimers, currentTime, unscaledTime, false);
            ProcessTimerList(longTimers, currentTime, unscaledTime, false);

            nextBucketProcessTime = currentTime + TickInterval;
        }

        // Clean up removed timers
        while (timersToRemove.Count > 0)
        {
            int id = timersToRemove.Dequeue();
            timerDictionary.Remove(id);
        }
    }

    private void ProcessTimerList(List<FBTimer> timers, float currentTime, float unscaledTime, bool isTickBucket)
    {
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            var timer = timers[i];

            // Check if timer or its user is invalid
            if (timer == null || !timer.IsValid())
            {
                timersToRemove.Enqueue(timer?.ID ?? -1);
                timers.RemoveAt(i);
                continue;
            }

            // Check if timer should trigger
            float checkTime = timer.Type == TimerType.Unscaled ? unscaledTime : currentTime;

            // For tick timers, they trigger every frame
            // For others, check if it's time
            if (isTickBucket || checkTime >= timer.NextTriggerTime)
            {
                bool shouldContinue = timer.Execute();

                if (shouldContinue)
                {
                    // Reschedule
                    if (timer.Type == TimerType.Tick)
                    {
                        // Tick timers don't need next trigger time
                    }
                    else
                    {
                        timer.NextTriggerTime = checkTime + timer.Interval;
                    }
                }
                else
                {

                    // Notify user
                    timer.User?.OnTimerCanceled(timer.ID);

                    timersToRemove.Enqueue(timer.ID);
                    timers.RemoveAt(i);
                }
            }
        }
    }

    #endregion TimerUpdates
}