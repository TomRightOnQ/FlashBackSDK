using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using UnityEditor.Experimental.GraphView;

public class FBPerformanceEditorWindow : EditorWindow
{
    private EO_PerformanceScene currentScene;
    private bool isLoading = false;

    // UI Elements references
    private VisualElement loadingOverlay;
    private VisualElement welcomeScreen;
    private VisualElement editorContent;
    private VisualElement trackContent;
    private Label sceneTitle;
    private Button saveButton;
    private TextField sceneNameField;

    // GraphView-based editor content
    private FBPerformanceGraphView performanceGraphView;
    private bool suppressSceneRefresh = false;

    [MenuItem("Tools/Performance Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<FBPerformanceEditorWindow>();
        window.titleContent = new GUIContent("Performance Editor");
        window.minSize = new Vector2(1920, 1080);
    }

    private void CreateGUI()
    {
        // Load UXML template
        string uxmlPath = "Assets/Editor/PerformanceEditor/FBPerformanceEditorWindowLayout.uxml";
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
        if (visualTree != null)
        {
            visualTree.CloneTree(rootVisualElement);
        }
        else
        {
            CreateFallbackUI();
            return;
        }

        // Load USS styles
        string ussPath = "Assets/Editor/PerformanceEditor/FBPerformanceEditorWindowStyles.uss";
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
        if (styleSheet != null)
        {
            rootVisualElement.styleSheets.Add(styleSheet);
        }

        // Get references to UI elements
        InitializeReferences();

        // Create GraphView-based editor content
        performanceGraphView = new FBPerformanceGraphView(sceneNameField);
        performanceGraphView.OnSceneNameChanged += OnSceneNameChanged;
        performanceGraphView.OnRequestSave += SaveCurrentScene;

        // Add GraphView to editor content
        editorContent.Add(performanceGraphView);

        // Set up event handlers
        SetupEventHandlers();

        // Show appropriate initial view
        UpdateUIState();
    }

    private void CreateFallbackUI()
    {
        // Create basic UI structure programmatically as fallback
        rootVisualElement.Add(new Label("Dialogue Editor") { name = "title" });
        rootVisualElement.Add(new VisualElement() { name = "toolbar" });
        rootVisualElement.Add(new VisualElement() { name = "content" });
        rootVisualElement.Add(new VisualElement() { name = "loading-overlay" });
    }

    private void InitializeReferences()
    {
        // Get references to key UI elements
        loadingOverlay = rootVisualElement.Q<VisualElement>("loading-overlay");
        welcomeScreen = rootVisualElement.Q<VisualElement>("welcome-screen");
        editorContent = rootVisualElement.Q<VisualElement>("editor-content");
        trackContent = rootVisualElement.Q<VisualElement>("track-content");
        sceneTitle = rootVisualElement.Q<Label>("scene-title");
        saveButton = rootVisualElement.Q<Button>("save-button");
        sceneNameField = rootVisualElement.Q<TextField>("scene-name-field");

        // Hide all panels initially
        SetElementVisible(loadingOverlay, false);
        SetElementVisible(welcomeScreen, false);
        SetElementVisible(editorContent, false);
        SetElementVisible(trackContent, false);
    }

    private void SetupEventHandlers()
    {
        // Toolbar buttons
        var newButton = rootVisualElement.Q<Button>("new-button");
        var openButton = rootVisualElement.Q<Button>("open-button");
        var closeButton = rootVisualElement.Q<Button>("close-button");

        newButton?.RegisterCallback<ClickEvent>(evt => CreateNewScene());
        openButton?.RegisterCallback<ClickEvent>(evt => OpenScene());
        closeButton?.RegisterCallback<ClickEvent>(evt => CloseCurrentScene());
        saveButton?.RegisterCallback<ClickEvent>(evt => SaveCurrentScene());

        // Scene name field
        sceneNameField?.RegisterValueChangedCallback(evt =>
        {
            if (currentScene != null)
            {
                currentScene.SceneName = evt.newValue;
                currentScene.MarkDirty();
                UpdateUIState();
            }
        });

        // Welcome screen buttons
        var welcomeNewButton = rootVisualElement.Q<Button>("welcome-new-button");
        var welcomeOpenButton = rootVisualElement.Q<Button>("welcome-open-button");

        welcomeNewButton?.RegisterCallback<ClickEvent>(evt => CreateNewScene());
        welcomeOpenButton?.RegisterCallback<ClickEvent>(evt => OpenScene());
    }

    private void UpdateUIState()
    {
        if (isLoading)
        {
            ShowLoadingScreen();
        }
        else if (currentScene == null)
        {
            ShowWelcomeScreen();
        }
        else
        {
            ShowEditorContent();
        }

        UpdateToolbarState();
    }

    private void ShowLoadingScreen()
    {
        SetElementVisible(loadingOverlay, true);
        SetElementVisible(welcomeScreen, false);
        SetElementVisible(editorContent, false);
        SetElementVisible(trackContent, false);
    }

    private void ShowWelcomeScreen()
    {
        SetElementVisible(loadingOverlay, false);
        SetElementVisible(welcomeScreen, true);
        SetElementVisible(editorContent, false);
        SetElementVisible(trackContent, false);
    }

    private void ShowEditorContent()
    {
        SetElementVisible(loadingOverlay, false);
        SetElementVisible(welcomeScreen, false);
        SetElementVisible(editorContent, true);
        SetElementVisible(trackContent, true);

        // 只有在需要时才重新绑定
        if (!performanceGraphView.IsBoundToScene(currentScene))
        {
            performanceGraphView.BindToScene(currentScene);
        }
    }

    /// <summary>
    /// 打开编辑特定Sectiuon的轨道的界面
    /// </summary>
    private void ShowTrackContent()
    {
        SetElementVisible(loadingOverlay, false);
        SetElementVisible(welcomeScreen, false);
        SetElementVisible(editorContent, true);
        SetElementVisible(trackContent, true);
    }

    private void UpdateToolbarState()
    {
        if (saveButton != null)
        {
            saveButton.SetEnabled(currentScene != null && currentScene.IsDirty);
        }

        if (sceneTitle != null && currentScene != null)
        {
            var dirtyMarker = currentScene.IsDirty ? "*" : "";
            sceneTitle.text = $"{currentScene.SceneName}{dirtyMarker}";
        }
    }

    private void OnSceneNameChanged(string newName)
    {
        UpdateToolbarState();
    }

    private void SetElementVisible(VisualElement element, bool visible)
    {
        if (element != null)
        {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    // Asset management methods (keep existing implementation)
    private async void CreateNewScene()
    {
        await CloseCurrentSceneAsync();

        isLoading = true;
        UpdateUIState();

        try
        {
            var newScene = CreateInstance<PerformanceScene>();
            newScene.name = "New Performance Scene";

            string path = EditorUtility.SaveFilePanelInProject(
                "Create New Performance Scene",
                "NewPerformanceScene",
                "asset",
                "Select where to save the performance scene");

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newScene, path);
                AssetDatabase.SaveAssets();
                await LoadSceneAsync(newScene);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create new scene: {e.Message}");
            EditorUtility.DisplayDialog("Error", $"Failed to create new scene: {e.Message}", "OK");
        }
        finally
        {
            isLoading = false;
            UpdateUIState();
        }
    }

    private async void OpenScene()
    {
        await CloseCurrentSceneAsync();

        string path = EditorUtility.OpenFilePanel("Open Performance Scene", "Assets", "asset");
        if (!string.IsNullOrEmpty(path))
        {
            await LoadSceneFromPathAsync(path);
        }
    }

    private async void CloseCurrentScene()
    {
        await CloseCurrentSceneAsync();
        UpdateUIState();
    }

    private void SaveCurrentScene()
    {
        if (currentScene != null)
        {
            suppressSceneRefresh = true;
            try
            {
                // 安全地更新所有节点位置
                performanceGraphView?.UpdateNodePositionsSilently();
                performanceGraphView?.SaveSpecialNodePositions();

                currentScene.SaveToAsset();
                AssetDatabase.SaveAssets();
                UpdateUIState();
            }
            finally
            {
                suppressSceneRefresh = false;
            }
        }
    }

    private async System.Threading.Tasks.Task LoadSceneFromPathAsync(string path)
    {
        isLoading = true;
        UpdateUIState();

        if (path.StartsWith(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
        }

        var asset = AssetDatabase.LoadAssetAtPath<PerformanceScene>(path);
        if (asset != null)
        {
            await LoadSceneAsync(asset);
        }

        isLoading = false;
        UpdateUIState();
    }

    private async System.Threading.Tasks.Task CloseCurrentSceneAsync()
    {
        if (currentScene != null)
        {
            if (currentScene.IsDirty)
            {
                bool shouldSave = EditorUtility.DisplayDialog("Unsaved Changes",
                    "You have unsaved changes. Do you want to save before closing?",
                    "Save", "Don't Save");

                if (shouldSave)
                {
                    SaveCurrentScene();
                }
            }

            currentScene.OnDataChanged -= OnSceneDataChanged;
            currentScene = null;

            await System.Threading.Tasks.Task.Delay(50);
            TryDestroyPerformanceGame();
        }
    }

    private async System.Threading.Tasks.Task LoadSceneAsync(PerformanceScene asset)
    {
        await System.Threading.Tasks.Task.Delay(100);
        currentScene = new EO_PerformanceScene(asset);
        currentScene.OnDataChanged += OnSceneDataChanged;
        OnSceneLoaded();
    }

    private void OnSceneDataChanged()
    {
        if (suppressSceneRefresh) return;

        UpdateUIState();
    }

    private void OnSceneLoaded()
    {
        Debug.Log($"Scene loaded: {currentScene.SceneName}");
        TryCreateGameForPerformance();
        UpdateUIState();
    }

    #region Game Instance for performance preview

    private void TryCreateGameForPerformance()
    {
        FBPerformanceMainGame mainGameReference = FBPerformanceMainGame.Game;
        if (mainGameReference == null)
        {
            GameObject O_PerformanceMainGame = new GameObject("PerformanceMainGame");
            FBPerformanceMainGame PerformanceMainGame = O_PerformanceMainGame.AddComponent<FBPerformanceMainGame>();
            PerformanceMainGame.Init();
        }
    }

    private void TryDestroyPerformanceGame()
    {
        FBPerformanceMainGame mainGameReference = FBPerformanceMainGame.Game;
        if (mainGameReference != null)
        {
            mainGameReference.DestroyPerformanceGame();
        }
    }

    #endregion Game Instance for performance preview
}