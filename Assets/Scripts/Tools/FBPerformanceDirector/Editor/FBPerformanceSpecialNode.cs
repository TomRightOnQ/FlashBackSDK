using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

// 特殊节点（如起始和结束）直接存储在Scene当中，而不是作为完整的Section存储

/// <summary>
/// 剧情起始节点 - 只有一个输出
/// </summary>
public class FBPerformanceEntryNode : Node
{
    public Port OutputPort { get; private set; }

    public void Initialize(Vector2 position)
    {
        title = "START";
        SetPosition(new Rect(position, Vector2.zero));
        SetupNode();
    }

    private void SetupNode()
    {
        // 特殊样式 - 绿色起始节点
        AddToClassList("entry-node");

        // 只有一个输出端口
        OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        OutputPort.portName = "Out";
        outputContainer.Add(OutputPort);

        // 不允许删除起始节点
        capabilities &= ~Capabilities.Deletable;

        RefreshExpandedState();
        RefreshPorts();
    }
}

/// <summary>
/// 剧情结束节点 - 有多个输入
/// </summary>
public class FBPerformanceExitNode : Node
{
    private EO_PerformanceScene boundScene;
    private List<Port> inputPorts = new List<Port>();
    public event Action OnPinsChanged;

    public void Initialize(Vector2 position, EO_PerformanceScene scene)
    {
        title = "END";
        SetPosition(new Rect(position, Vector2.zero));
        boundScene = scene;
        SetupNode();
    }

    private void SetupNode()
    {
        // 特殊样式 - 红色结束节点
        AddToClassList("exit-node");

        // 根据保存的数据创建输入端口
        if (boundScene != null)
        {
            for (int i = 0; i < boundScene.ExitNodeInputCount(); i++)
            {
                AddInputPort(i);
            }
        }
        else
        {
            // 默认创建一个输入端口
            AddInputPort(0);
        }

        // 添加"添加端口"按钮
        var addPortButton = new Button(() => AddInputPort()) { text = "+ Add Input" };
        addPortButton.AddToClassList("add-port-button");
        mainContainer.Add(addPortButton);

        // 不允许删除结束节点
        capabilities &= ~Capabilities.Deletable;

        RefreshExpandedState();
        RefreshPorts();
    }

    /// <summary>
    /// 添加新的输入端口
    /// </summary>
    public void AddInputPort(int? specificIndex = null)
    {
        var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));

        int portIndex = specificIndex ?? inputPorts.Count;
        inputPort.portName = $"In {portIndex + 1}";
        inputPorts.Add(inputPort);

        // 如果这是新添加的端口（不是从数据加载的），更新数据
        if (boundScene != null && specificIndex == null)
        {
            boundScene.AddExitNodeInput(EPerformanceSectionEndEventType.NONE);
        }

        // 添加删除按钮（除了第一个端口）
        if (inputPorts.Count > 1)
        {
            var deleteButton = new Button(() => RemoveInputPort(inputPort)) { text = "×" };
            deleteButton.AddToClassList("delete-port-button");

            var portContainer = new VisualElement();
            portContainer.style.flexDirection = FlexDirection.Row;
            portContainer.style.alignItems = Align.Center;
            portContainer.Add(inputPort);
            portContainer.Add(deleteButton);

            inputContainer.Add(portContainer);
        }
        else
        {
            inputContainer.Add(inputPort);
        }

        // 触发端点事件用来标脏
        OnPinsChanged?.Invoke();

        // 刷新端口显示
        RefreshExpandedState();
        RefreshPorts();
    }

    /// <summary>
    /// 移除指定的输入端口
    /// </summary>
    public void RemoveInputPort(Port port)
    {
        if (inputPorts.Count <= 1) return; // 至少保留一个端口

        int portIndex = inputPorts.IndexOf(port);
        if (portIndex >= 0)
        {
            inputPorts.Remove(port);

            // 从数据中移除端口配置
            if (boundScene != null)
            {
                boundScene.RemoveExitNodeInput(portIndex);
            }

            // 从inputContainer中移除端口或其容器
            foreach (var child in inputContainer.Children())
            {
                if (child == port || (child is VisualElement container && container.Contains(port)))
                {
                    inputContainer.Remove(child);
                    break;
                }
            }

            // 触发端点事件用来标脏
            OnPinsChanged?.Invoke();

            // 刷新端口显示
            RefreshExpandedState();
            RefreshPorts();
        }
    }

    /// <summary>
    /// 获取所有输入端口
    /// </summary>
    public List<Port> GetInputPorts()
    {
        return new List<Port>(inputPorts);
    }

    /// <summary>
    /// 根据端口索引获取输入端口
    /// </summary>
    public Port GetInputPortByIndex(int portIndex)
    {
        return portIndex >= 0 && portIndex < inputPorts.Count ? inputPorts[portIndex] : null;
    }
}