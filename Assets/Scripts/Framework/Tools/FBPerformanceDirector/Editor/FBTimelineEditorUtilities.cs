using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEditor.Timeline;

/// <summary>
/// Simple component to control Timeline playback in editor
/// </summary>
public class FBTimelineController : MonoBehaviour
{
    [Header("Timeline References")]
    public FBPerformanceTimeline performanceTimeline;
    public PlayableDirector playableDirector;

    private void Reset()
    {
        // Auto-get PlayableDirector component
        playableDirector = GetComponent<PlayableDirector>();
        if (playableDirector == null)
        {
            playableDirector = gameObject.AddComponent<PlayableDirector>();
        }
    }

    private void Awake()
    {
        EnsurePlayableDirector();
    }

    /// <summary>
    /// Set the timeline asset and prepare for playback
    /// </summary>
    public void SetTimeline(FBPerformanceTimeline timeline)
    {
        performanceTimeline = timeline;
        EnsurePlayableDirector();

        if (playableDirector != null && timeline != null)
        {
            playableDirector.playableAsset = timeline;

            // Rebind tracks if needed
            RebindTrackBindings();
        }
    }

    /// <summary>
    /// Ensure we have a PlayableDirector component
    /// </summary>
    private void EnsurePlayableDirector()
    {
        if (playableDirector == null)
        {
            playableDirector = GetComponent<PlayableDirector>();
        }

        if (playableDirector == null)
        {
            playableDirector = gameObject.AddComponent<PlayableDirector>();
        }
    }

    /// <summary>
    /// Rebind track bindings to appropriate GameObjects
    /// </summary>
    private void RebindTrackBindings()
    {
        if (playableDirector == null || performanceTimeline == null) return;

        // Rebind dialogue track to this GameObject or a specific target
        var dialogueTrack = performanceTimeline.GetDialogueTrack(false);
        if (dialogueTrack != null)
        {
            // Bind to this GameObject or create a dialogue manager
            playableDirector.SetGenericBinding(dialogueTrack, gameObject);
        }

        // Refresh the timeline editor if it's open
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () => {
            if (playableDirector != null)
            {
                playableDirector.RebuildGraph();
            }
        };
#endif
    }

    /// <summary>
    /// Play the timeline
    /// </summary>
    public void Play()
    {
        if (playableDirector != null)
        {
            playableDirector.Play();
        }
    }

    /// <summary>
    /// Stop the timeline
    /// </summary>
    public void Stop()
    {
        if (playableDirector != null)
        {
            playableDirector.Stop();
        }
    }

    /// <summary>
    /// Set time and evaluate (for preview)
    /// </summary>
    public void SetTime(double time)
    {
        if (playableDirector != null)
        {
            playableDirector.time = time;
            playableDirector.Evaluate();
        }
    }

    /// <summary>
    /// Get the current timeline time
    /// </summary>
    public double GetCurrentTime()
    {
        return playableDirector != null ? playableDirector.time : 0;
    }

    /// <summary>
    /// Check if timeline is playing
    /// </summary>
    public bool IsPlaying()
    {
        return playableDirector != null && playableDirector.state == PlayState.Playing;
    }
}



public static class FBTimelineEditorUtilities
{
#if UNITY_EDITOR
    private const string TIMELINE_CONTROLLER_NAME = "FB_TimelineController";

    [MenuItem("Tools/Performance System/Create Timeline Controller", false, 100)]
    public static void CreateTimelineController()
    {
        // Find or create the controller GameObject
        GameObject controllerGO = GameObject.Find(TIMELINE_CONTROLLER_NAME);
        if (controllerGO == null)
        {
            controllerGO = new GameObject(TIMELINE_CONTROLLER_NAME);
            Undo.RegisterCreatedObjectUndo(controllerGO, "Create Timeline Controller");
        }

        // Add required components
        var controller = controllerGO.GetComponent<FBTimelineController>();
        if (controller == null)
        {
            controller = Undo.AddComponent<FBTimelineController>(controllerGO);
        }

        var playableDirector = controllerGO.GetComponent<PlayableDirector>();
        if (playableDirector == null)
        {
            playableDirector = Undo.AddComponent<PlayableDirector>(controllerGO);
            controller.playableDirector = playableDirector;
        }

        // Select the object
        Selection.activeGameObject = controllerGO;
        EditorGUIUtility.PingObject(controllerGO);

        Debug.Log("Timeline Controller created. Select a Performance Timeline asset and assign it to play.");
    }

    [MenuItem("Tools/Performance System/Open Timeline With Controller", true)]
    public static bool ValidateOpenTimelineWithController()
    {
        return Selection.activeObject is FBPerformanceTimeline;
    }

    [MenuItem("Tools/Performance System/Open Timeline With Controller", false, 101)]
    public static void OpenTimelineWithController()
    {
        var timeline = Selection.activeObject as FBPerformanceTimeline;
        if (timeline == null) return;

        // Ensure we have a controller
        CreateTimelineController();

        var controllerGO = GameObject.Find(TIMELINE_CONTROLLER_NAME);
        var controller = controllerGO.GetComponent<FBTimelineController>();

        // Set the timeline
        controller.SetTimeline(timeline);

        // Open Timeline window using the public API
        OpenTimelineWindowWithDirector(controller.playableDirector);

        Debug.Log($"Timeline '{timeline.name}' is now ready for playback with controller.");
    }

    [MenuItem("Assets/Performance System/Open in Timeline", false, 322)]
    public static void OpenTimelineFromAsset()
    {
        var timeline = Selection.activeObject as FBPerformanceTimeline;
        if (timeline != null)
        {
            OpenTimelineWithController();
        }
    }

    /// <summary>
    /// Open Timeline window and set the director using public APIs
    /// </summary>
    public static void OpenTimelineWindowWithDirector(PlayableDirector director)
    {
        if (director == null) return;

        // Method 1: Use TimelineEditor public API (Unity 2019.3+)
#if UNITY_2019_3_OR_NEWER
        // Set the inspected asset and director
        Selection.activeObject = director.playableAsset;
#endif

        // Method 2: Simply open the Timeline window - it should auto-detect the selected director
        EditorWindow.GetWindow<EditorWindow>("Timeline");

        // Method 3: Ensure the director's gameObject is selected
        Selection.activeGameObject = director.gameObject;

        // Force refresh
        EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
    }

    /// <summary>
    /// Get the currently open Timeline window (if any)
    /// </summary>
    public static EditorWindow GetTimelineWindow()
    {
        var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
        foreach (var window in windows)
        {
            if (window.GetType().Name == "TimelineWindow")
            {
                return window;
            }
        }
        return null;
    }

    /// <summary>
    /// Check if Timeline window is open and has a valid director
    /// </summary>
    public static bool IsTimelineWindowReady()
    {
        var window = GetTimelineWindow();
        if (window == null) return false;

        // Check if we have a valid director through reflection as fallback
        var timelineEditorType = typeof(TimelineEditor);
        var directorProperty = timelineEditorType.GetProperty("inspectedDirector");
        if (directorProperty != null)
        {
            var currentDirector = directorProperty.GetValue(null) as PlayableDirector;
            return currentDirector != null;
        }

        return false;
    }

    /// <summary>
    /// Auto-setup when a timeline asset is selected
    /// </summary>
    [InitializeOnLoadMethod]
    public static void InitializeTimelineAutoSetup()
    {
        EditorApplication.update += AutoSetupTimelineController;
    }

    private static void AutoSetupTimelineController()
    {
        // Only run occasionally to avoid performance issues
        if (Time.frameCount % 100 != 0) return;

        // Check if we're in a context where timeline setup makes sense
        var currentAsset = TimelineEditor.inspectedAsset;
        if (currentAsset is FBPerformanceTimeline performanceTimeline)
        {
            // Check if we already have a director for this timeline
            var existingControllers = Object.FindObjectsOfType<FBTimelineController>();
            FBTimelineController suitableController = null;

            foreach (var controller in existingControllers)
            {
                if (controller.playableDirector != null &&
                    controller.playableDirector.playableAsset == performanceTimeline)
                {
                    suitableController = controller;
                    break;
                }
            }

            // If no suitable controller found, create one but don't auto-assign to avoid confusion
            if (suitableController == null)
            {
                // We'll let the user manually setup through the menu to avoid unexpected behavior
            }
        }
    }
#endif
}
