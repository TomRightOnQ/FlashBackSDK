using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Timer Interface
/// </summary>
public class FBTimerUser
{
    // All active timer IDs for the current object
    private List<int> activeTimerIds = new List<int>();
    private WeakReference<object> objectRef;

    public FBTimerUser(object owner)
    {
        objectRef = new WeakReference<object>(owner);
    }

    public bool IsValid => objectRef != null;

    // Add Timer - Called by the object
    public int AddTimer(float timeInterval, int invokeNums, FBTimerUser caller, Action callback)
    {
        return FBMainGame.System.TimerSystem.AddTimer(timeInterval, invokeNums, caller, callback);
    }

    // Add Timer = back from the system
    public void RegisterTimer(int timerId)
    {
        if (!activeTimerIds.Contains(timerId))
        {
            activeTimerIds.Add(timerId);
        }
    }

    public List<int> GetActiveTimerIds()
    {
        return new List<int>(activeTimerIds);
    }
    
    // Delete timers internally
    public void DeleteTimer(int timerID)
    {
        if (activeTimerIds.Remove(timerID))
        {
            FBMainGame.System.TimerSystem.DeleteTimer(timerID, false);
        }
    }

    public void DeleteAllTimers()
    {
        foreach (var timerID in activeTimerIds)
        {
            FBMainGame.System.TimerSystem.DeleteTimer(timerID, false);
        }
        activeTimerIds.Clear();
    }

    // Delete timers externally
    public void OnTimerCanceled(int timerId)
    {
        activeTimerIds.Remove(timerId);
    }

    // OnDestroy
    public void OnObjectDestroyed()
    {
        if (activeTimerIds.Count > 0)
        {
            DeleteAllTimers();
        }
    }
}