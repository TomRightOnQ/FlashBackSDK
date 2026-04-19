using System;
using UnityEngine;

/// <summary>
/// Timer Base - Represents a single timer instance
/// </summary>
public class FBTimer
{
    public int ID { get; private set; }
    public float Interval { get; private set; }
    public int RemainingExecutions { get; private set; }  // -1 = infinite
    public float NextTriggerTime { get; set; }
    public FBTimerUser User { get; private set; }
    public Action Callback { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsPaused { get; private set; }
    public TimerType Type { get; private set; }

    // For debugging
    public string DebugInfo { get; private set; }

    public FBTimer(int id, float interval, int invokeNums, FBTimerUser user,
                   Action callback, TimerType type = TimerType.Normal)
    {
        ID = id;
        Interval = interval;
        RemainingExecutions = invokeNums;
        User = user;
        Callback = callback;
        IsActive = true;
        IsPaused = false;
        Type = type;
    }

    public bool IsValid()
    {
        // Check if user is still alive
        return User != null && User.IsValid;
    }

    public bool Execute()
    {
        if (!IsActive || IsPaused || !IsValid())
            return false;

        try
        {
            Callback?.Invoke();

            if (RemainingExecutions > 0)
            {
                RemainingExecutions--;
            }

            // Check if timer should continue
            return IsActive && (RemainingExecutions == -1 || RemainingExecutions > 0);
        }
        catch (Exception e)
        {
            FBDebug.Instance.FBLogError($"[Timer] Error executing timer {ID}: {e}\nDebugInfo: {DebugInfo}");
            return false;
        }
    }

    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;
    public void Stop() => IsActive = false;
}

public enum TimerType
{
    Normal,     // Regular timer using Time.time
    Tick,       // Every frame
    Unscaled    // Using Time.unscaledTime
}