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
    public static FBMainGame Game => instance;

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

    /*
    [SerializeField, ReadOnly] private FBResourceManager FBResourceManager;
    public FBResourceManager => ResourceManager;
     */

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

        // 1. FBObjectManager
        GameObject managerGameObject_0 = new GameObject("FBObjectManager");
        FBObjectManager = managerGameObject_0.AddComponent<FBObjectManager>();
        gameSystemDictionary["FBObjectManager"] = FBObjectManager;
        FBObjectManager.OnSystemCreate();

        // 2. FBEventSystem
        GameObject managerGameObject_1 = new GameObject("FBEventSystem");
        FBEventSystem = managerGameObject_1.AddComponent<FBEventSystem>();
        gameSystemDictionary["FBEventSystem"] = FBEventSystem;
        FBEventSystem.OnSystemCreate();

        // 3. FBUIManager
        GameObject managerGameObject_2 = new GameObject("FBUIManager");
        FBUIManager = managerGameObject_2.AddComponent<FBUIManager>();
        gameSystemDictionary["FBUIManager"] = FBUIManager;
        FBUIManager.OnSystemCreate();

        // 4. FBLevelSystem
        GameObject managerGameObject_3 = new GameObject("FBLevelSystem");
        FBLevelSystem = managerGameObject_3.AddComponent<FBLevelSystem>();
        gameSystemDictionary["FBLevelSystem"] = FBLevelSystem;
        FBLevelSystem.OnSystemCreate();

        // 5. FBResourceManager
        /*
        GameObject managerGameObject_4 = new GameObject("FBResourceManager");
        FBResourceManager = managerGameObject_4.AddComponent<FBResourceManager>();
        gameSystemDictionary["FBResourceManager"] = FBResourceManager;
        FBResourceManager.OnSystemCreate();
        */

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
    /// Called immediately after scene loaded
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
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
