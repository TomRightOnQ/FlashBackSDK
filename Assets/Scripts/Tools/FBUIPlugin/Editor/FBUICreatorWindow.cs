using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

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
        GUILayout.Label("FBUICreator", EditorStyles.boldLabel);
        scriptPath = EditorGUILayout.TextField("Script Path", scriptPath);
        layer = (UILayer)EditorGUILayout.EnumPopup("UI Layer", layer);

        // If an asset is selected and it's a GameObject, show the create button
        if (selectedAsset != null)
        {
            if (GUILayout.Button("Save"))
            {
                SaveData();
            }

            if (GUILayout.Button("Create/Update Script"))
            {
                CreateOrUpdateScript();
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
        UIData newData = new UIData(scriptPath, layer);
        UIConfigWriter.UpdateOrCreateUIDataEntry(selectedAsset.name, newData);
    }
}
