using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 剧情节点
/// </summary>
public class FBPerformanceNode : Node
{
    public EO_PerformanceSection BoundSection { get; private set; }

    private List<Port> outputPorts = new List<Port>();
    public event Action OnPinsChanged;

    public void Initialize(string nodeTitle, Rect position)
    {
        title = nodeTitle;
        SetPosition(position);
    }

    public void BindSectionData(EO_PerformanceSection section)
    {
        BoundSection = section;
        SetupNode();
        // Update node title when section name changes
        this.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            if (BoundSection != null)
            {
                BoundSection.SectionName = title;
            }
        });
    }

    protected virtual void SetupNode()
    {
        // Input port (保持单个输入)
        var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        inputPort.portName = "In";
        inputContainer.Add(inputPort);

        // 根据保存的数据创建输出端口
        if (BoundSection != null)
        {
            for (int i = 0; i < BoundSection.OutputPortCount(); i++)
            {
                AddOutputPort(i);
            }

            // 添加Timeline状态指示器
            AddTimelineIndicator();
        }
        else
        {
            // 默认创建一个输出端口
            AddOutputPort(0);
        }

        if (BoundSection != null && BoundSection.HasTimeline)
        {
            AddTimelineTestButton();
        }

        // 添加"添加输出端口"按钮
        var addOutputButton = new Button(() => AddOutputPort()) { text = "+ Add Output" };
        addOutputButton.AddToClassList("add-output-button");
        mainContainer.Add(addOutputButton); ;

        // Add some sample content
        var content = new Label("Double-click to edit content");
        content.style.marginTop = 8;
        content.style.marginLeft = 4;
        content.style.fontSize = 10;
        content.style.color = new Color(0.7f, 0.7f, 0.7f);
        mainContainer.Add(content);

        // Refresh to apply changes
        RefreshExpandedState();
        RefreshPorts();
    }

    #region Timeline
    /// <summary>
    /// 添加Timeline状态指示器
    /// </summary>
    private void AddTimelineIndicator()
    {
        if (BoundSection == null) return;

        var timelineIndicator = new VisualElement();
        timelineIndicator.style.flexDirection = FlexDirection.Row;
        timelineIndicator.style.alignItems = Align.Center;
        timelineIndicator.style.marginTop = 4;
        timelineIndicator.style.marginBottom = 4;

        var icon = new Label("🎬");
        icon.style.fontSize = 12;
        icon.style.marginRight = 4;

        var statusLabel = new Label(BoundSection.HasTimeline ? "Has Timeline" : "No Timeline");
        statusLabel.style.fontSize = 9;
        statusLabel.style.color = BoundSection.HasTimeline ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f);

        timelineIndicator.Add(icon);
        timelineIndicator.Add(statusLabel);
        mainContainer.Add(timelineIndicator);
    }

    /// <summary>
    /// 添加Timeline测试按钮
    /// </summary>
    private void AddTimelineTestButton()
    {
        var testButton = new Button(() => {
            if (BoundSection != null)
            {
                BoundSection.TestTimelineInEditor();
            }
        })
        {
            text = "Test Timeline"
        };

        testButton.AddToClassList("test-timeline-button");
        testButton.style.marginTop = 4;
        testButton.style.height = 16;
        testButton.style.fontSize = 9;

        mainContainer.Add(testButton);
    }
    #endregion Timeline

    /// <summary>
    /// 添加新的输出端口
    /// </summary>
    public void AddOutputPort(int? specificIndex = null)
    {
        var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));

        int portIndex = specificIndex ?? outputPorts.Count;
        outputPort.portName = $"Out {portIndex + 1}";
        outputPorts.Add(outputPort);

        // 如果这是新添加的端口（不是从数据加载的），更新数据
        if (BoundSection != null && specificIndex == null)
        {
            BoundSection.AddOutputEvent(EPerformanceSectionEndEventType.NONE);
        }

        // 添加删除按钮（除了第一个端口）
        if (outputPorts.Count > 1)
        {
            var deleteButton = new Button(() => RemoveOutputPort(outputPort)) { text = "×" };
            deleteButton.AddToClassList("delete-port-button");

            var portContainer = new VisualElement();
            portContainer.style.flexDirection = FlexDirection.Row;
            portContainer.style.alignItems = Align.Center;
            portContainer.Add(outputPort);
            portContainer.Add(deleteButton);

            outputContainer.Add(portContainer);
        }
        else
        {
            outputContainer.Add(outputPort);
        }

        // 触发端点事件用来标脏
        OnPinsChanged?.Invoke();

        // 刷新端口显示
        RefreshExpandedState();
        RefreshPorts();
    }

    /// <summary>
    /// 移除指定的输出端口
    /// </summary>
    public void RemoveOutputPort(Port port)
    {
        if (outputPorts.Count <= 1) return; // 至少保留一个端口

        outputPorts.Remove(port);

        // 从outputContainer中移除端口或其容器
        foreach (var child in outputContainer.Children())
        {
            if (child == port || (child is VisualElement container && container.Contains(port)))
            {
                outputContainer.Remove(child);
                break;
            }
        }

        // 触发端点事件用来标脏
        OnPinsChanged?.Invoke();

        // 刷新端口显示
        RefreshExpandedState();
        RefreshPorts();
    }

    /// <summary>
    /// 获取所有输出端口
    /// </summary>
    public List<Port> GetOutputPorts()
    {
        return new List<Port>(outputPorts);
    }

    /// <summary>
    /// 根据端口索引获取输出端口
    /// </summary>
    public Port GetOutputPortByIndex(int portIndex)
    {
        return portIndex >= 0 && portIndex < outputPorts.Count ? outputPorts[portIndex] : null;
    }
}