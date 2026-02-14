using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Timeline;

// Performance

/// <summary>
/// 每一个PerformanceScene视为一个独立的剧情表演资产，是该功能的根节点数据，包含了其中每个剧情片段(SPerformanceSection)的信息
/// </summary>
[CreateAssetMenu(fileName = "NewPerformanceScene", menuName = "Performance System/Performance Scene")]
public class PerformanceScene : ScriptableObject
{
    // 资产的唯一ID
    public string SceneGUID;
    // 资产的名称
    public string SceneName;

    // 所包含的对话节点
    public List<PerformanceSection> PerformanceSectionList = new List<PerformanceSection>();

    // 节点位置数据 [SectionGUID -> Position]
    public List<SectionPosition> SectionPositions = new List<SectionPosition>();

    // 连接关系数据
    public List<SectionConnection> Connections = new List<SectionConnection>();

    // 起点节点信息
    public Vector2 EntryNodePosition = new Vector2(50, 200);
    public string EntryNodeGUID;

    // 终点节点信息
    public Vector2 ExitNodePosition = new Vector2(800, 200);
    public string ExitNodeGUID;
    public List<ExitNodeInput> ExitNodeInputs = new List<ExitNodeInput>(); // 结束节点的输入端口配置

    // 确保剧情资产的GUID不出问题
    private void OnValidate()
    {
        EnsureGUIDs();
        EnsureExitNodeInputs();
    }

    public void EnsureGUIDs()
    {
        // Generate GUID if empty
        if (string.IsNullOrEmpty(SceneGUID))
        {
            SceneGUID = Guid.NewGuid().ToString();
        }

        // Generate Entry/Exit GUIDs if empty
        if (string.IsNullOrEmpty(EntryNodeGUID))
        {
            EntryNodeGUID = "ENTRY_" + Guid.NewGuid().ToString();
        }
        if (string.IsNullOrEmpty(ExitNodeGUID))
        {
            ExitNodeGUID = "EXIT_" + Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// 确保结束节点至少有一个输入端口
    /// </summary>
    private void EnsureExitNodeInputs()
    {
        if (ExitNodeInputs.Count == 0)
        {
            ExitNodeInputs.Add(new ExitNodeInput(EPerformanceSectionEndEventType.NONE));
        }
    }

    /// <summary>
    /// 为结束节点添加输入端口
    /// </summary>
    public void AddExitNodeInput(EPerformanceSectionEndEventType eventType = EPerformanceSectionEndEventType.NONE)
    {
        ExitNodeInputs.Add(new ExitNodeInput(eventType));
    }

    /// <summary>
    /// 移除结束节点的指定输入端口
    /// </summary>
    public void RemoveExitNodeInput(int index)
    {
        if (ExitNodeInputs.Count > 1 && index >= 0 && index < ExitNodeInputs.Count)
        {
            ExitNodeInputs.RemoveAt(index);
        }
    }
}

/// <summary>
/// 结束节点输入端口配置
/// </summary>
[System.Serializable]
public class ExitNodeInput
{
    public EPerformanceSectionEndEventType ExpectedEventType = EPerformanceSectionEndEventType.NONE;
    public List<CustomVariable> RequiredVariables = new List<CustomVariable>();

    public ExitNodeInput() { }

    public ExitNodeInput(EPerformanceSectionEndEventType eventType)
    {
        ExpectedEventType = eventType;
    }
}

/// <summary>
/// 演出片段结束事件配置
/// </summary>
[System.Serializable]
public class SectionEndEvent
{
    public EPerformanceSectionEndEventType EventType = EPerformanceSectionEndEventType.NONE;
    public List<CustomVariable> CustomVariables = new List<CustomVariable>();

    public SectionEndEvent() { }

    public SectionEndEvent(EPerformanceSectionEndEventType eventType)
    {
        EventType = eventType;
    }
}

/// <summary>
/// 自定义变量
/// </summary>
[System.Serializable]
public class CustomVariable
{
    public string VariableName;
    public string VariableValue;
    public string VariableType; // 可以扩展为enum: "string", "int", "float", "bool"

    public CustomVariable() { }

    public CustomVariable(string name, string value, string type = "string")
    {
        VariableName = name;
        VariableValue = value;
        VariableType = type;
    }
}


/// <summary>
/// 一个对话资产由多个Section组成，这些Section通过Node相互连接，构成一段完整的剧情
/// </summary>
[System.Serializable]
public class PerformanceSection
{
    // 节点唯一标识
    public string SectionGUID;

    // 节点名称
    public string SectionName;

    // 节点类型
    public EPerformanceSectionType SectionType = EPerformanceSectionType.TICK;

    // 节点数据
    public string SectionData;

    // 输出端口配置（每个输出端口对应一个结束事件）
    public List<SectionEndEvent> OutputEvents = new List<SectionEndEvent>();

    // Timeline引用 - 每个Section拥有一个特殊的FBPerformanceTimeline
    public FBPerformanceTimeline SectionTimeline;

    // Timeline资源路径（备用引用方式）
    public string TimelineAssetPath;

    public PerformanceSection()
    {
        SectionGUID = Guid.NewGuid().ToString();
        SectionName = "New Section";
        // 默认创建一个输出端口
        OutputEvents.Add(new SectionEndEvent(EPerformanceSectionEndEventType.NONE));
    }

    public PerformanceSection(string name)
    {
        SectionGUID = Guid.NewGuid().ToString();
        SectionName = name;
        // 默认创建一个输出端口
        OutputEvents.Add(new SectionEndEvent(EPerformanceSectionEndEventType.NONE));
    }

    /// <summary>
    /// 添加新的输出端口
    /// </summary>
    public void AddOutputEvent(EPerformanceSectionEndEventType eventType = EPerformanceSectionEndEventType.NONE)
    {
        OutputEvents.Add(new SectionEndEvent(eventType));
    }

    /// <summary>
    /// 移除指定的输出端口
    /// </summary>
    public void RemoveOutputEvent(int index)
    {
        if (OutputEvents.Count > 1 && index >= 0 && index < OutputEvents.Count)
        {
            OutputEvents.RemoveAt(index);
        }
    }

    /// <summary>
    /// 获取Timeline资源（如果只有路径引用，则加载资源）
    /// </summary>
    public FBPerformanceTimeline GetTimelineAsset()
    {
        if (SectionTimeline != null)
            return SectionTimeline;

        if (!string.IsNullOrEmpty(TimelineAssetPath))
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<FBPerformanceTimeline>(TimelineAssetPath);
#else
            return Resources.Load<FBPerformanceTimeline>(TimelineAssetPath);
#endif
        }

        return null;
    }

    /// <summary>
    /// 设置Timeline资源并更新路径引用
    /// </summary>
    public void SetTimelineAsset(FBPerformanceTimeline timeline)
    {
        SectionTimeline = timeline;

        if (timeline != null)
        {
#if UNITY_EDITOR
            TimelineAssetPath = UnityEditor.AssetDatabase.GetAssetPath(timeline);
#else
            TimelineAssetPath = timeline.name; // 运行时回退方案
#endif
        }
        else
        {
            TimelineAssetPath = string.Empty;
        }
    }

    /// <summary>
    /// 检查是否拥有有效的Timeline
    /// </summary>
    public bool HasValidTimeline()
    {
        return SectionTimeline != null || !string.IsNullOrEmpty(TimelineAssetPath);
    }
}

/// <summary>
/// 节点位置数据
/// </summary>
[System.Serializable]
public struct SectionPosition
{
    public string SectionGUID;
    public Vector2 Position;

    public SectionPosition(string guid, Vector2 position)
    {
        SectionGUID = guid;
        Position = position;
    }
}

/// <summary>
/// 节点连接数据
/// </summary>
[System.Serializable]
public struct SectionConnection
{
    public string FromSectionGUID;
    public int FromPortIndex;
    public string ToSectionGUID;
    public int ToPortIndex;

    public SectionConnection(string fromSection, int fromPortIndex, string toSection, int toPortIndex)
    {
        FromSectionGUID = fromSection;
        FromPortIndex = fromPortIndex;
        ToSectionGUID = toSection;
        ToPortIndex = toPortIndex;
    }
}

/// <summary>
/// 每个Section会拥有一个特殊的FBPerfromanceTimeline，包含多条轨道，这些轨道会进行具体的操作或者调用
/// 轨道之间可能不同，因为一些特殊轨道，如相机和展示对话，有不同的逻辑
/// </summary>
public abstract class PerformanceTrack : TrackAsset
{
    // 基础轨道属性
    [Tooltip("轨道是否启用")]
    public bool TrackEnabled = true;

    [Tooltip("轨道描述")]
    public string TrackDescription;

    /// <summary>
    /// 检查轨道是否有效
    /// </summary>
    public virtual bool IsValid()
    {
        return TrackEnabled && !muted;
    }
}