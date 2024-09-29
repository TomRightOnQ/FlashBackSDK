using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FlackBackSDK SINGLETON MANAGER
/// This System is NOT controlled by the main game loop
/// Showing debug with options
/// </summary>
public class FBDebug : MonoBehaviour
{
    private static FBDebug instance;
    public static FBDebug Instance => instance;

    // Configurations the log system
    [SerializeField] private bool LOG_NORMAL = true;
    [SerializeField] private bool LOG_WARNING = true;
    [SerializeField] private bool LOG_ERROR = true;

    [SerializeField] private bool LOG_MAIN_GAME = true;
    [SerializeField] private bool LOG_GAME_SYSTEM = true;
    [SerializeField] private bool LOG_UI = true;
    [SerializeField] private bool LOG_FBOBJECT = true;
    [SerializeField] private bool LOG_CUSTOM = true;


    private void Awake()
    {
        gameObject.tag = "Manager";
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Log Methods
    /// <summary>
    /// Log
    /// </summary>
    /// <param name="logString"> The class you make this call </param>
    /// <param name="caller"> Log content </param>
    public void FBLog(string logString, GameObject caller)
    {
        if (shouldLog(caller, LOG_NORMAL))
        {
            string outPut = $"FEDebug: {caller.GetType()}: {logString}";
            Debug.Log(outPut);
        }
    }

    /// <summary>
    /// Log Warning
    /// </summary>
    /// <param name="logString"> The class you make this call </param>
    /// <param name="caller"> Log Warning content </param>
    public void FBLogWarning(string logString, GameObject caller)
    {
        if (shouldLog(caller, LOG_WARNING))
        {
            string outPut = $"FEDebug: {caller.GetType()}: {logString}";
            Debug.LogWarning(outPut);
        }
    }

    /// <summary>
    /// Log Error
    /// </summary>
    /// <param name="logString"> The class you make this call </param>
    /// <param name="caller"> Log Error content </param>
    public void FBLogError(string logString, GameObject caller)
    {
        if (shouldLog(caller, LOG_ERROR))
        {
            string outPut = $"FEDebug: {caller.GetType()}: {logString}";
            Debug.LogError(outPut);
        }
    }

    // Used by main game loop ONLY
    // Use it in FBMainGame
    /// <summary>
    /// Log
    /// </summary>
    /// <param name="logString"></param>
    public void FBMainLog(string logString)
    {
        if (!LOG_MAIN_GAME || !LOG_NORMAL) 
        {
            return;
        }
        string outPut = $"FEDebug GAMEMAIN: {logString}";
        Debug.Log(outPut);
    }

    /// <summary>
    /// Log Warinng
    /// </summary>
    /// <param name="logString"></param>
    public void FBMainLogWarning(string logString)
    {
        if (!LOG_MAIN_GAME || !LOG_WARNING)
        {
            return;
        }
        string outPut = $"FEDebug GAMEMAIN: {logString}";
        Debug.LogWarning(outPut);
    }

    /// <summary>
    /// Log Error
    /// </summary>
    /// <param name="logString"></param>
    public void FBMainLogError(string logString)
    {
        if (!LOG_MAIN_GAME || !LOG_ERROR)
        {
            return;
        }
        string outPut = $"FEDebug GAMEMAIN: {logString}";
        Debug.LogError(outPut);
    }

    /// <summary>
    /// Check Conditions
    /// </summary>
    /// <param name="caller"></param>
    /// <param name="logEnabled"></param>
    /// <returns></returns>
    private bool shouldLog(GameObject caller, bool logEnabled)
    {
        if (!logEnabled) return false;

        if (caller == null) return false;

        // Check if the caller is a child of a specific type
        if (caller.GetComponentInParent<FBUIBase>() != null)
        {
            return LOG_UI;
        }
        else if (caller.GetComponentInParent<FBGameSystem>() != null)
        {
            return LOG_GAME_SYSTEM;
        }
        else if (caller.GetComponentInParent<FBObject>() != null)
        {
            return LOG_FBOBJECT;
        }
        else
        {
            return LOG_CUSTOM;
        }
    }
}
