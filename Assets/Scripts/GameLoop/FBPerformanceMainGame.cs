using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The Game Instance made for Performance Editor preview
/// </summary>
public class FBPerformanceMainGame : MonoBehaviour
{
    private static FBPerformanceMainGame instance;
    public static FBPerformanceMainGame Game => instance;

    public GameObject PerformanceRoot;

    private Dictionary<string, object> gameSystemDictionary = new Dictionary<string, object>();

    // Place the references of all systems here
    [SerializeField, ReadOnly] private FBObjectManager FBObjectManager;
    public FBObjectManager ObjectManager => FBObjectManager;

    [SerializeField, ReadOnly] private FBEventSystem FBEventSystem;
    public FBEventSystem EventSystem => FBEventSystem;

    [SerializeField, ReadOnly] private FBUIManager FBUIManager;
    public FBUIManager UI => FBUIManager;

    [SerializeField, ReadOnly] private FBResourceManager FBResourceManager;
    public FBResourceManager ResourceManager => FBResourceManager;

    [SerializeField, ReadOnly] private FBBattleSystem FBBattleSystem;
    public FBBattleSystem BattleSystem => FBBattleSystem;

    [SerializeField, ReadOnly] private FBBuffSystem FBBuffSystem;
    public FBBuffSystem BuffSystem => FBBuffSystem;

    public void Init()
    {
        gameObject.tag = "Manager";
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }


        // Edit System in SystemConfig.cs
        CreateSystem();
        OnSystemInit();
    }

    public void DestroyPerformanceGame()
    {
        foreach (KeyValuePair<string, object> system in gameSystemDictionary)
        {
            if (system.Value is FBGameSystem gameSystem && gameSystem != null)
            {
                DestroyImmediate(gameSystem.gameObject);
            }
            else if (system.Value is GameObject gameObj && gameObj != null)
            {
                DestroyImmediate(gameObj);
            }
        }
        DestroyImmediate(PerformanceRoot);
        DestroyImmediate(this.gameObject);
        instance = null;
    }

    public void CreateSystem()
    {
        // 1. FBResourceManager
        GameObject O_FBResourceManager = new GameObject("FBResourceManager");
        FBResourceManager = O_FBResourceManager.AddComponent<FBResourceManager>();
        gameSystemDictionary["FBResourceManager"] = FBResourceManager;
        FBResourceManager.OnSystemCreate();

        // 2. FBObjectManager
        GameObject O_FBObjectManager = new GameObject("FBObjectManager");
        FBObjectManager = O_FBObjectManager.AddComponent<FBObjectManager>();
        gameSystemDictionary["FBObjectManager"] = FBObjectManager;
        FBObjectManager.OnSystemCreate();

        // 3. FBEventSystem
        GameObject O_FBEventSystem = new GameObject("FBEventSystem");
        FBEventSystem = O_FBEventSystem.AddComponent<FBEventSystem>();
        gameSystemDictionary["FBEventSystem"] = FBEventSystem;
        FBEventSystem.OnSystemCreate();

        // 4. FBUIManager
        GameObject O_FBUIManager = new GameObject("FBUIManager");
        FBUIManager = O_FBUIManager.AddComponent<FBUIManager>();
        gameSystemDictionary["FBUIManager"] = FBUIManager;
        FBUIManager.OnSystemCreate();

        // 6. FBBattleSystem
        GameObject O_FBBattleSystem = new GameObject("FBBattleSystem");
        FBBattleSystem = O_FBBattleSystem.AddComponent<FBBattleSystem>();
        gameSystemDictionary["FBBattleSystem"] = FBBattleSystem;
        FBBattleSystem.OnSystemCreate();

        // 7. FBBuffSystem
        GameObject O_FBBuffSystem = new GameObject("FBBuffSystem");
        FBBuffSystem = O_FBBuffSystem.AddComponent<FBBuffSystem>();
        gameSystemDictionary["FBBuffSystem"] = FBBuffSystem;
        FBBuffSystem.OnSystemCreate();
    }

    /// <summary>
    /// Called after all game systems are inited in this.Init()
    /// Run the OnSystemInit of all systems
    /// </summary>
    private void OnSystemInit()
    {
        PerformanceRoot = new GameObject("PERFORMANCE_ROOT");
        PerformanceRoot.transform.position = new Vector3(0, 0, 0);

        // Call OnSystemInit for each manager
        foreach (var manager in gameSystemDictionary.Values)
        {
            FBGameSystem gameSystem = manager as FBGameSystem;
            if (gameSystem != null)
            {
                gameSystem.gameObject.transform.SetParent(PerformanceRoot.transform);
                gameSystem.OnSystemInit();
            }
        }
    }

    // Exposed Methods:
    /// <summary>
    /// Get the FBGameSystemInstance
    /// </summary>
    /// <typeparam name="T"> Type Name of the manager </typeparam>
    /// <returns></returns> 
    public T Get<T>() where T : class
    {
        if (gameSystemDictionary.TryGetValue("FBSystemManager", out object manager))
        {
            return manager as T;
        }
        return null;
    }
}
