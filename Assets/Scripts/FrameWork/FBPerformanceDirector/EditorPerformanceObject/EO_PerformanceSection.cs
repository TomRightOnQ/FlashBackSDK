using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 运行时的节点
/// </summary>
public class EO_PerformanceSection
{
    public PerformanceSection Section { get; private set; }
    public Vector2 Position { get; set; }

    // Node visual properties
    public bool IsSelected { get; set; }

    public EO_PerformanceSection(PerformanceSection section)
    {
        Section = section;
        Position = Vector2.zero;
    }

    public string SectionGUID => Section.SectionGUID;
    public string SectionName
    {
        get => Section.SectionName;
        set => Section.SectionName = value;
    }

    // Timeline相关属性
    public FBPerformanceTimeline Timeline
    {
        get => Section.GetTimelineAsset();
        set => Section.SetTimelineAsset(value);
    }

    public bool HasTimeline => Section.HasValidTimeline();

    /// <summary>
    /// 获取输出端口数量
    /// </summary>
    public int OutputPortCount()
    {
        return Section.OutputEvents.Count;
    }

    /// <summary>
    /// 添加输出端口
    /// </summary>
    public void AddOutputEvent(EPerformanceSectionEndEventType eventType = EPerformanceSectionEndEventType.NONE)
    {
        Section.AddOutputEvent(eventType);
    }

    /// <summary>
    /// 移除输出端口
    /// </summary>
    public void RemoveOutputEvent(int index)
    {
        Section.RemoveOutputEvent(index);
    }

    /// <summary>
    /// 获取指定输出端口的事件配置
    /// </summary>
    public SectionEndEvent GetOutputEvent(int index)
    {
        if (index >= 0 && index < Section.OutputEvents.Count)
        {
            return Section.OutputEvents[index];
        }
        return null;
    }

    #region Editor

    /// <summary>
    /// 创建新的Timeline资源
    /// </summary>
    public void CreateNewTimeline()
    {
#if UNITY_EDITOR
        var timeline = ScriptableObject.CreateInstance<FBPerformanceTimeline>();
        timeline.name = $"{SectionName}_Timeline";

        // 自动创建默认对话轨道
        if (timeline.CreateDefaultDialogueTrack)
        {
            timeline.GetDialogueTrack(true);
        }

        Section.SetTimelineAsset(timeline);
#endif
    }

    /// <summary>
    /// 在Timeline编辑器中打开此Section的Timeline
    /// </summary>
    public void OpenTimelineInEditor()
    {
#if UNITY_EDITOR
        var timeline = Timeline;
        if (timeline != null)
        {
            UnityEditor.Selection.activeObject = timeline;
            // UnityEditor.EditorWindow.GetWindow<UnityEditor.Timeline.TimelineWindow>();
        }
#endif
    }

    /// <summary>
    /// 在Timeline编辑器中测试播放此Section的Timeline
    /// </summary>
    public void TestTimelineInEditor()
    {
#if UNITY_EDITOR
        var timeline = Timeline;
        if (timeline != null)
        {
            // Select the timeline asset
            UnityEditor.Selection.activeObject = timeline;

            // Use editor utility to setup playback
            OpenTimeline();
        }
        else
        {
            Debug.LogWarning("No timeline assigned to this section.");
        }
#endif
    }

    private void OpenTimeline()
    {
#if UNITY_EDITOR
        try
        {
            // Get the editor utilities type using reflection
            var editorUtilitiesType = Type.GetType("FBTimelineEditorUtilities, Assembly-CSharp-Editor");
            if (editorUtilitiesType == null)
            {
                // Try alternative assembly name
                editorUtilitiesType = Type.GetType("FBTimelineEditorUtilities, Assembly-CSharp-firstpass");
            }

            if (editorUtilitiesType != null)
            {
                // Get the static method
                var method = editorUtilitiesType.GetMethod("OpenTimelineWithController",
                    BindingFlags.Public | BindingFlags.Static);

                if (method != null)
                {
                    // Call the method (it's static, so no instance needed)
                    method.Invoke(null, null);
                    return;
                }
            }

            Debug.LogWarning("Could not auto-setup Timeline. Please manually:\n" +
                           "1. Go to: Tools > Performance System > Create Timeline Controller\n" +
                           "2. Drag this timeline to the controller's Performance Timeline field\n" +
                           "3. Open Timeline window and select 'FB_TimelineController' from director dropdown");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to open timeline in editor: {e.Message}");

            // Fallback instructions
            Debug.Log("Please manually setup Timeline controller via: Tools > Performance System > Create Timeline Controller");
        }
#endif
    }

    #endregion Editor
}