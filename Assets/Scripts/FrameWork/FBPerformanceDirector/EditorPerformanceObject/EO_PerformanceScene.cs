using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 编辑器运行时的剧情资产
/// </summary>
public class EO_PerformanceScene
{
    public PerformanceScene Asset { get; private set; }
    public string SceneName { get; set; }
    public bool IsDirty { get; private set; }

    public event Action OnDataChanged;

    // 系统节点数据
    public Vector2 EntryNodePosition { get; set; }
    public string EntryNodeGUID { get; private set; }

    public Vector2 ExitNodePosition { get; set; }
    public string ExitNodeGUID { get; private set; }
    public List<ExitNodeInput> ExitNodeInputs { get; private set; }


    private bool suppressEvents = false;

    public EO_PerformanceScene(PerformanceScene asset)
    {
        Asset = asset;

        // 确保GUIDs已设置
        asset.EnsureGUIDs();

        SceneName = asset.SceneName;
        IsDirty = false;

        // 加载系统节点数据
        EntryNodePosition = asset.EntryNodePosition;
        EntryNodeGUID = asset.EntryNodeGUID;
        ExitNodePosition = asset.ExitNodePosition;
        ExitNodeGUID = asset.ExitNodeGUID;
        ExitNodeInputs = new List<ExitNodeInput>(asset.ExitNodeInputs);

        // Load sections from asset
        LoadSectionsFromAsset();

        // Load connections from asset
        Connections = new List<SectionConnection>(asset.Connections);
    }

    #region 资产操作
    private void LoadSectionsFromAsset()
    {
        Sections.Clear();

        foreach (var section in Asset.PerformanceSectionList)
        {
            var eoSection = new EO_PerformanceSection(section);

            // Load position
            var position = Asset.SectionPositions.Find(sp => sp.SectionGUID == section.SectionGUID);
            if (position.SectionGUID != null)
            {
                eoSection.Position = position.Position;
            }
            Sections.Add(eoSection);
        }
    }

    public void MarkDirty()
    {
        IsDirty = true;
        if (!suppressEvents)
        {
            OnDataChanged?.Invoke();
        }
    }

    /// <summary>
    /// 执行操作而不触发数据变化事件
    /// </summary>
    public void ExecuteWithoutEvents(Action action)
    {
        suppressEvents = true;
        try
        {
            action();
        }
        finally
        {
            suppressEvents = false;
        }
    }

    public void SaveToAsset()
    {
        if (Asset == null) return;

        // Save data back to the ScriptableObject
        Asset.SceneName = SceneName;

        // 保存系统节点数据
        Asset.EntryNodePosition = EntryNodePosition;
        Asset.EntryNodeGUID = EntryNodeGUID;
        Asset.ExitNodePosition = ExitNodePosition;
        Asset.ExitNodeGUID = ExitNodeGUID;
        Asset.ExitNodeInputs = new List<ExitNodeInput>(ExitNodeInputs);

        // Save sections
        Asset.PerformanceSectionList.Clear();
        foreach (var eoSection in Sections)
        {
            Asset.PerformanceSectionList.Add(eoSection.Section);
        }

        // Save positions
        Asset.SectionPositions.Clear();
        foreach (var eoSection in Sections)
        {
            Asset.SectionPositions.Add(new SectionPosition(eoSection.Section.SectionGUID, eoSection.Position));
        }

        // Save connections
        Asset.Connections = new List<SectionConnection>(Connections);

        // Mark asset as dirty for Unity to save
        MarkAssetDirty(Asset);
        IsDirty = false;
    }

    private void MarkAssetDirty(UnityEngine.Object obj)
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(obj);
#endif
    }
    #endregion 资产操作

    #region 剧情节段节点管理
    // Editor-time node management
    public List<EO_PerformanceSection> Sections { get; private set; } = new List<EO_PerformanceSection>();
    public List<SectionConnection> Connections { get; private set; } = new List<SectionConnection>();

    /// <summary>
    /// 新建一个节点
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public EO_PerformanceSection CreateNewSection(Vector2 position)
    {
        var newSection = new EO_PerformanceSection(new PerformanceSection($"Section {Sections.Count + 1}"));
        newSection.Position = position;
        Sections.Add(newSection);
        MarkDirty();
        return newSection;
    }

    /// <summary>
    /// 删除节点
    /// </summary>
    /// <param name="section"></param>
    public void RemoveSection(EO_PerformanceSection section)
    {
        // Remove all connections involving this section
        Connections.RemoveAll(c => c.FromSectionGUID == section.Section.SectionGUID ||
                                  c.ToSectionGUID == section.Section.SectionGUID);
        Sections.Remove(section);
        MarkDirty();
    }

    /// <summary>
    /// 连接节点
    /// </summary>
    /// <param name="fromSectionGuid"></param>
    /// <param name="fromPortIndex"></param>
    /// <param name="toSectionGuid"></param>
    /// <param name="toPortIndex"></param>
    public void AddConnection(string fromSectionGuid, int fromPortIndex, string toSectionGuid, int toPortIndex)
    {
        var connection = new SectionConnection(fromSectionGuid, fromPortIndex, toSectionGuid, toPortIndex);
        Connections.Add(connection);
        MarkDirty();
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    /// <param name="connection"></param>
    public void RemoveConnection(SectionConnection connection)
    {
        Connections.Remove(connection);
        MarkDirty();
    }

    /// <summary>
    /// 为结束节点添加输入端口
    /// </summary>
    public void AddExitNodeInput(EPerformanceSectionEndEventType eventType = EPerformanceSectionEndEventType.NONE)
    {
        ExitNodeInputs.Add(new ExitNodeInput(eventType));
        MarkDirty();
    }

    /// <summary>
    /// 移除结束节点的指定输入端口
    /// </summary>
    public void RemoveExitNodeInput(int index)
    {
        if (ExitNodeInputs.Count > 1 && index >= 0 && index < ExitNodeInputs.Count)
        {
            ExitNodeInputs.RemoveAt(index);
            MarkDirty();
        }
    }

    /// <summary>
    /// 获取结束节点输入端口数量
    /// </summary>
    public int ExitNodeInputCount()
    {
        return ExitNodeInputs.Count;
    }

    #endregion 剧情节段节点管理
}