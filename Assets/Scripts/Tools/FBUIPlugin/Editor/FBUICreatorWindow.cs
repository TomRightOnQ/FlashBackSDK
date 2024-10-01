using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// FBUIPlugin - MainWindow
/// </summary>
public class FBUICreatorWindow : EditorWindow
{
    // Static instance of the window
    public static FBUICreatorWindow Instance { get; private set; }

    // Currently selected asset
    private GameObject selectedAsset;
    [SerializeField] private string scriptPath = "Assets/Scripts/UI/";
    [SerializeField] private UILayer layer = UILayer.Bottom;
    [SerializeField] private bool bPersistent = false;
    [SerializeField] private bool bAutoShow = false;

    public static void ShowWindow()
    {
        Instance = (FBUICreatorWindow)GetWindow(typeof(FBUICreatorWindow), false, "FBUICreator");
        Instance.Show();
    }

    void InitData()
    {
        if (selectedAsset != null)
        {
            EditorGUILayout.LabelField("Selected UI Prefab", AssetDatabase.GetAssetPath(selectedAsset));
            OnPrefabSelected(selectedAsset);
        }
        else
        {
            EditorGUILayout.LabelField("Selected UI Prefab", "None");
        }
    }

    // Define the GUI behavior
    void OnGUI()
    {
        GUILayout.Label(selectedAsset.name, EditorStyles.boldLabel);

        scriptPath = EditorGUILayout.TextField("Script Path", scriptPath);
        layer = (UILayer)EditorGUILayout.EnumPopup("UI Layer", layer);
        bPersistent = EditorGUILayout.Toggle("Persistent on SceneChange", bPersistent);
        bAutoShow = EditorGUILayout.Toggle("Show after SceneChange", bAutoShow);

        // If an asset is selected and it's a GameObject, show the create button
        if (selectedAsset != null)
        {
            GUILayout.Label("\n", EditorStyles.label);
            GUILayout.Label("Save settings to the prefab asset", EditorStyles.boldLabel);
            if (GUILayout.Button("Save"))
            {
                SaveData();
            }

            // Check if we are in Prefab Mode
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            bool isInPrefabMode = prefabStage != null;

            GUILayout.Label(isInPrefabMode ? "Exit Prefab Mode to Click" : "Generate Script", EditorStyles.boldLabel);

            // Disable the button if in Prefab Mode
            using (new EditorGUI.DisabledGroupScope(isInPrefabMode))
            {
                if (GUILayout.Button("Create Script"))
                {
                    CreateOrUpdateScript();
                }
                if (GUILayout.Button("Write References"))
                {
                    WriteAllReferences();
                }
            }
        }
    }


    // Assign a FBUIBase to the window
    public void AssignPrefab(FBUICreator fbuiCreator)
    {
        selectedAsset = fbuiCreator.gameObject;
        Instance.InitData();
    }

    // This method will be called when a new prefab is selected
    void OnPrefabSelected(GameObject prefab)
    {
        if (prefab != null)
        {
            // Read data
            FBUICreator fbuiCreator = prefab.GetComponent<FBUICreator>();
            if (fbuiCreator != null)
            {
                scriptPath = fbuiCreator.ScriptPath;
                layer = fbuiCreator.Layer;
                bPersistent = fbuiCreator.IsPersistent;
                bAutoShow = fbuiCreator.IsAutoShow;
            }
            else
            {
                Debug.LogError("The selected prefab does not have an FBUICreator component.");
            }
        }
    }

    // Save the data back to the prefab
    void SaveData()
    {
        if (selectedAsset is GameObject prefabAsset)
        {
            // Get the FBUICreator component from the prefab asset
            var fbuiCreator = prefabAsset.GetComponent<FBUICreator>();
            if (fbuiCreator != null)
            {
                fbuiCreator.ScriptPath = scriptPath;
                fbuiCreator.Layer = layer;
                fbuiCreator.IsPersistent = bPersistent;
                fbuiCreator.IsAutoShow = bAutoShow;
                EditorUtility.SetDirty(prefabAsset);
            }
            else
            {
                Debug.LogError("The selected prefab does not have an FBUICreator component.");
            }
        }
        else
        {
            Debug.LogError("The selected asset is not a GameObject.");
        }
    }

    /// <summary>
    /// Create/Update the script of the UI prefab
    /// Also update the config
    /// </summary>
    void CreateOrUpdateScript()
    {
        // To update a specific entry in the configuration file
        string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(selectedAsset);
        UIConfigWriter.UpdateOrCreateUIDataEntry(selectedAsset.name, assetPath);
        UIScriptWriter.CreateScript(selectedAsset.name, scriptPath);
    }

    /// <summary>
    /// Scan and wrtie all references by using rules indicated in UIConfig.widgetName
    /// </summary>
    void WriteAllReferences()
    {
        UIScriptWriter.WriteReferences(selectedAsset.name, scriptPath, selectedAsset);
    }
}
