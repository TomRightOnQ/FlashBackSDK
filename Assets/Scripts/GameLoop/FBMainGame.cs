using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// THIS IS THE CORE BEGINNING/ENTRY POINT OF THE GAME
/// FBMain Game will be placed in the game's first loading scene,
/// holding the references of all systems, and persists through the game
/// </summary>
public class FBMainGame : MonoBehaviour
{
    private static FBMainGame instance;
    public static FBMainGame System => instance;

    [SerializeField, ReadOnly] private bool bSytemsInited = false;

    private Dictionary<string, object> gameSystemDictionary = new Dictionary<string, object>();

    // Place the references of all systems here
    [SerializeField, ReadOnly] private FBObjectManager FBObjectManager;
    public FBObjectManager ObjectManager => FBObjectManager;

    [SerializeField, ReadOnly] private FBEventSystem FBEventSystem;
    public FBEventSystem EventSystem => FBEventSystem;

    [SerializeField, ReadOnly] private FBUIManager FBUIManager;
    public FBUIManager UI => FBUIManager;

    [SerializeField, ReadOnly] private FBLevelSystem FBLevelSystem;
    public FBLevelSystem LevelSystem => FBLevelSystem;


    [SerializeField, ReadOnly] private FBResourceManager FBResourceManager;
    public FBResourceManager ResourceManager => FBResourceManager;


    private void Awake()
    {
        FBDebug.Instance.FBMainLog("Main Game Awake");
        gameObject.tag = "Manager";
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            Init();
        }
        else
        {
            Destroy(gameObject);
            FBDebug.Instance.FBMainLogWarning("Repeated Main Game found, the duplication is already destroyed");
        }
    }

    private void Init()
    {
        if (bSytemsInited)
        {
            FBDebug.Instance.FBMainLogError("Unable to init the game because the game is already booted once!!!");
        }
        // Init Sequence
        FBDebug.Instance.FBMainLog("STEP I: Create ALL FBGameSystems");

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

        // 5. FBLevelSystem
        GameObject O_FBLevelSystem = new GameObject("FBLevelSystem");
        FBLevelSystem = O_FBLevelSystem.AddComponent<FBLevelSystem>();
        gameSystemDictionary["FBLevelSystem"] = FBLevelSystem;
        FBLevelSystem.OnSystemCreate();

        bSytemsInited = true;
        FBDebug.Instance.FBMainLog("STEP I COMPLETE");
        OnSystemInit();
    }

    /// <summary>
    /// Called after all game systems are inited in this.Init()
    /// Run the OnSystemInit of all systems
    /// </summary>
    private void OnSystemInit()
    {
        FBDebug.Instance.FBMainLog("STEP II: Init ALL FBGameSystems");

        if (!bSytemsInited)
        {
            FBDebug.Instance.FBMainLogError("Unable to call OnSystemInit beacause the game is not ready!");
        }

        // Call OnSystemInit for each manager
        foreach (var manager in gameSystemDictionary.Values)
        {
            FBGameSystem gameSystem = manager as FBGameSystem;
            if (gameSystem != null)
            {
                gameSystem.OnSystemInit();
            }
        }

        FBDebug.Instance.FBMainLog("STEP II COMPLETE");
        // Notify Level System
    }

    /// <summary>
    /// Called before scene change happening
    /// </summary>
    public void OnSceneUnloaded()
    {
        // Call OnSceneUnloaded for each manager
        foreach (var manager in gameSystemDictionary.Values)
        {
            FBGameSystem gameSystem = manager as FBGameSystem;
            if (gameSystem != null)
            {
                gameSystem.OnSceneUnloaded();
            }
        }
    }

    /// <summary>
    /// Called immediately after scene loaded
    /// </summary>
    public void OnSceneLoaded()
    {
        // Call OnSceneChange for each manager
        foreach (var manager in gameSystemDictionary.Values)
        {
            FBGameSystem gameSystem = manager as FBGameSystem;
            if (gameSystem != null)
            {
                gameSystem.OnSceneChange();
            }
        }
        foreach (var manager in gameSystemDictionary.Values)
        {
            FBGameSystem gameSystem = manager as FBGameSystem;
            if (gameSystem != null)
            {
                gameSystem.OnSceneLoadComplete();
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
        FBDebug.Instance.FBMainLogWarning("Unable to accquire the Game System");
        return null;
    }
}
