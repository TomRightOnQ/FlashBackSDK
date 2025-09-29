using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using System.IO;

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
        if (selectedAsset == null)
        {
            Debug.LogError("No UI prefab selected.");
            return;
        }

        // Get the prefab path and extract system folder name
        string prefabPath = AssetDatabase.GetAssetPath(selectedAsset);
        string systemFolder = UIScriptWriter.ExtractSystemFolderFromPath(prefabPath);

        if (string.IsNullOrEmpty(systemFolder))
        {
            Debug.LogError("Could not determine system folder from prefab path: " + prefabPath);
            return;
        }

        // Create script directories if they don't exist
        string systemScriptPath = $"Assets/Scripts/Gameplay/Systems/{systemFolder}";
        UIScriptWriter.EnsureDirectoryExists(systemScriptPath);

        string className = selectedAsset.name;
        string viewClassName = className + "View";

        // Create or update the main UI class (preserves user code)
        UIScriptWriter.CreateMainUIClass(systemScriptPath, className, viewClassName);

        // Create or regenerate the View class (can be completely rewritten)
        UIScriptWriter.CreateViewClass(systemScriptPath, viewClassName, selectedAsset);

        // Attach both scripts to the prefab
        AttachScriptsToPrefab(className, viewClassName, systemFolder);

        AssetDatabase.Refresh();
        Debug.Log($"Successfully created/updated scripts for {className} in {systemScriptPath}");
    }

    private void AttachScriptsToPrefab(string className, string viewClassName, string systemFolder)
    {
        // Load the main UI script
        MonoScript mainScript = AssetDatabase.LoadAssetAtPath<MonoScript>($"Assets/Scripts/Gameplay/Systems/{systemFolder}/{className}.cs");
        MonoScript viewScript = AssetDatabase.LoadAssetAtPath<MonoScript>($"Assets/Scripts/Gameplay/Systems/{systemFolder}/{viewClassName}.cs");

        if (mainScript == null || viewScript == null)
        {
            Debug.LogError("Failed to load generated scripts.");
            return;
        }

        // Get the prefab instance
        GameObject prefabInstance = PrefabUtility.InstantiatePrefab(selectedAsset) as GameObject;

        if (prefabInstance == null)
        {
            Debug.LogError("Failed to instantiate prefab.");
            return;
        }

        // Remove existing components of the same type
        var existingMain = prefabInstance.GetComponent(className);
        var existingView = prefabInstance.GetComponent(viewClassName);

        if (existingMain != null) DestroyImmediate(existingMain);
        if (existingView != null) DestroyImmediate(existingView);

        // Add the new components
        prefabInstance.AddComponent(mainScript.GetClass());
        prefabInstance.AddComponent(viewScript.GetClass());

        // Get the FBUIBase component and assign the view reference
        var uiBase = prefabInstance.GetComponent<FBUIBase>();
        var viewComponent = prefabInstance.GetComponent(viewScript.GetClass()) as FBUIView;

        if (uiBase != null && viewComponent != null)
        {
            // Use reflection to set the SelfWidgets field
            var field = typeof(FBUIBase).GetField("SelfWidgets");
            if (field != null)
            {
                field.SetValue(uiBase, viewComponent);
            }
        }

        // Save changes back to the prefab
        PrefabUtility.ApplyPrefabInstance(prefabInstance, InteractionMode.UserAction);
        DestroyImmediate(prefabInstance);
    }

    /// <summary>
    /// Scan and wrtie all references by using rules indicated in UIConfig.widgetName
    /// </summary>
    void WriteAllReferences()
    {
        if (selectedAsset == null)
        {
            Debug.LogError("No UI prefab selected.");
            return;
        }

        // Get the prefab instance
        GameObject prefabInstance = PrefabUtility.InstantiatePrefab(selectedAsset) as GameObject;

        if (prefabInstance == null)
        {
            Debug.LogError("Failed to instantiate prefab.");
            return;
        }

        try
        {
            // Get the view component
            string className = selectedAsset.name;
            string viewClassName = className + "View";
            var viewComponent = prefabInstance.GetComponent(viewClassName) as MonoBehaviour;

            if (viewComponent == null)
            {
                Debug.LogError($"View component {viewClassName} not found on prefab.");
                return;
            }

            // Scan and assign all references
            ScanAndAssignReferences(prefabInstance.transform, viewComponent);

            // Save changes back to the prefab
            PrefabUtility.ApplyPrefabInstance(prefabInstance, InteractionMode.UserAction);
            Debug.Log($"Successfully assigned all UI references for {className}");
        }
        finally
        {
            DestroyImmediate(prefabInstance);
        }
    }

    /// <summary>
    /// Recursively scans and assigns UI component references
    /// </summary>
    private void ScanAndAssignReferences(Transform current, MonoBehaviour viewComponent)
    {
        if (current == null) return;

        // Get all components on this GameObject
        var components = current.GetComponents<Component>();

        foreach (var component in components)
        {
            if (component == null) continue;

            string fieldName = UIScriptWriter.MakeValidFieldName(current.name, component);
            string fieldType = UIScriptWriter.GetComponentTypeName(component);

            if (!string.IsNullOrEmpty(fieldType))
            {
                // Use reflection to find and set the field
                var field = viewComponent.GetType().GetField(fieldName,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);

                if (field != null && field.FieldType.IsAssignableFrom(component.GetType()))
                {
                    field.SetValue(viewComponent, component);
                }
            }
        }

        // Handle FBUIBase references
        var childUIBase = current.GetComponent<FBUIBase>();
        if (childUIBase != null && current != current.root)
        {
            string fieldName = UIScriptWriter.MakeValidFieldName(current.name, childUIBase);
            var field = viewComponent.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);

            if (field != null && field.FieldType == typeof(FBUIBase))
            {
                field.SetValue(viewComponent, childUIBase);
            }
        }

        // Recursively scan children
        for (int i = 0; i < current.childCount; i++)
        {
            ScanAndAssignReferences(current.GetChild(i), viewComponent);
        }
    }
}
