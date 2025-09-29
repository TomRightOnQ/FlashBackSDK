using System;
using System.Collections.Generic;
using UnityEngine;

public partial class FBMainGame
{
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

    [SerializeField, ReadOnly] private FBBattleSystem FBBattleSystem;
    public FBBattleSystem BattleSystem => FBBattleSystem;

    [SerializeField, ReadOnly] private FBBuffSystem FBBuffSystem;
    public FBBuffSystem BuffSystem => FBBuffSystem;

    [SerializeField, ReadOnly] private FBNetworkManager FBNetworkManager;
    public FBNetworkManager NetworkManager => FBNetworkManager;

    private void CreateSystem()
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

        // 5. FBLevelSystem
        GameObject O_FBLevelSystem = new GameObject("FBLevelSystem");
        FBLevelSystem = O_FBLevelSystem.AddComponent<FBLevelSystem>();
        gameSystemDictionary["FBLevelSystem"] = FBLevelSystem;
        FBLevelSystem.OnSystemCreate();

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

        // 8. FBNetworkManager
        GameObject O_FBNetworkManager = new GameObject("FBNetworkManager");
        FBNetworkManager = O_FBNetworkManager.AddComponent<FBNetworkManager>();
        gameSystemDictionary["FBNetworkManager"] = FBNetworkManager;
        FBNetworkManager.OnSystemCreate();
    }
}
