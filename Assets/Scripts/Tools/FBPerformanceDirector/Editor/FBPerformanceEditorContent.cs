using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

/// <summary>
/// GraphView-based performance editor content
/// </summary>
public class FBPerformanceGraphView : GraphView
{
    #region Fields
    private EO_PerformanceScene currentScene;
    private TextField sceneNameField;
    // 特殊节点引用
    private FBPerformanceEntryNode currentEntryNode;
    private FBPerformanceExitNode currentExitNode;

    // 状态标记
    private bool isRefreshing = false;
    private bool suppressDataEvents = false;

    public event Action<string> OnSceneNameChanged;
    public event Action OnRequestSave;
    #endregion

    #region Constructor and Setup
    public FBPerformanceGraphView(TextField sceneNameField)
    {
        this.sceneNameField = sceneNameField;
        SetupGraphView();
    }

    private void SetupGraphView()
    {
        // Add grid background
        var gridBackground = new GridBackground();
        Insert(0, gridBackground);

        // Enable zoom and pan
        this.SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        // Enable dragging and selection
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        // Node creation context menu
        this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

        // 监听节点位置变化
        this.graphViewChanged += OnGraphViewChanged;

        // Set style
        this.style.flexGrow = 1;
    }
    #endregion

    #region GraphView Event Handlers
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Create Dialogue Section",
            action => CreatePerformanceNode<FBPerformanceNode>("Dialogue Section", evt.mousePosition));
    }

    /// <summary>
    /// 监听图形变化（包括节点移动）
    /// </summary>
    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        // 如果正在刷新UI，忽略数据变化事件
        if (isRefreshing || suppressDataEvents) return graphViewChange;
        suppressDataEvents = true;

        if (graphViewChange.movedElements != null)
        {
            foreach (var element in graphViewChange.movedElements)
            {
                if (element is FBPerformanceEntryNode entryNode)
                {
                    // 更新起始节点位置到数据
                    currentScene.EntryNodePosition = entryNode.GetPosition().position;
                }
                else if (element is FBPerformanceExitNode exitNode)
                {
                    // 更新结束节点位置到数据
                    currentScene.ExitNodePosition = exitNode.GetPosition().position;
                }
                else if (element is FBPerformanceNode perfNode)
                {
                    // 更新普通节点位置到数据
                    if (perfNode.BoundSection != null)
                    {
                        perfNode.BoundSection.Position = element.GetPosition().position;
                    }
                }
            }
        }

        // 监听节点删除
        if (graphViewChange.elementsToRemove != null)
        {
            foreach (var element in graphViewChange.elementsToRemove)
            {
                if (element is Edge edge)
                {
                    RemoveConnectionFromData(edge);
                }
                else if (element is FBPerformanceNode perfNode)
                {
                    RemoveNodeFromData(perfNode);
                }
                // 注意：特殊节点（Entry/Exit）不应该被删除，所以不处理它们
            }
        }

        // 监听连接变化
        if (graphViewChange.edgesToCreate != null)
        {
            foreach (var edge in graphViewChange.edgesToCreate)
            {
                SaveConnectionToData(edge);
            }
        }

        if (graphViewChange.elementsToRemove != null)
        {
            foreach (var element in graphViewChange.elementsToRemove)
            {
                if (element is Edge edge)
                {
                    RemoveConnectionFromData(edge);
                }
            }
        }
        currentScene.MarkDirty();
        suppressDataEvents = false;
        return graphViewChange;
    }

    /// <summary>
    /// 订阅所有节点的引脚变化事件
    /// </summary>
    private void SubscribeToPinChangeEvents()
    {
        foreach (var node in nodes)
        {
            if (node is FBPerformanceNode perfNode)
            {
                perfNode.OnPinsChanged += OnNodePinsChanged;
            }
            else if (node is FBPerformanceExitNode exitNode)
            {
                exitNode.OnPinsChanged += OnNodePinsChanged;
            }
        }
    }

    /// <summary>
    /// 处理节点引脚变化
    /// </summary>
    private void OnNodePinsChanged()
    {
        // Validate connections when pins change
        ValidateAllConnections();

        // Mark scene as dirty
        if (currentScene != null)
        {
            currentScene.MarkDirty();
        }
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        foreach (var port in ports.ToList())
        {
            if (startPort != port && startPort.node != port.node)
                compatiblePorts.Add(port);
        }

        return compatiblePorts;
    }

    /// <summary>
    /// 从数据中移除节点及其所有连接
    /// </summary>
    private void RemoveNodeFromData(FBPerformanceNode node)
    {
        if (currentScene == null || node.BoundSection == null) return;

        // 移除与该节点相关的所有连接
        string sectionGuid = node.BoundSection.SectionGUID;
        currentScene.Connections.RemoveAll(c =>
            c.FromSectionGUID == sectionGuid || c.ToSectionGUID == sectionGuid);

        // 从场景数据中移除节点
        currentScene.RemoveSection(node.BoundSection);

        Debug.Log($"Removed section node: {node.BoundSection.SectionName}");
    }

    /// <summary>
    /// 检查连线是否有效
    /// 节点的端点改变后进行检查
    /// </summary>
    private void ValidateAllConnections()
    {
        if (currentScene == null) return;

        var connectionsToRemove = new List<SectionConnection>();

        foreach (var connection in currentScene.Connections)
        {
            if (!IsConnectionValid(connection))
            {
                connectionsToRemove.Add(connection);
            }
        }

        // Remove invalid connections
        foreach (var invalidConnection in connectionsToRemove)
        {
            currentScene.Connections.Remove(invalidConnection);

            // Also remove the visual edge if it exists
            var edgeToRemove = edges.ToList().Find(edge =>
                GetNodeGuid(edge.output.node) == invalidConnection.FromSectionGUID &&
                GetNodeGuid(edge.input.node) == invalidConnection.ToSectionGUID &&
                GetPortIndex(edge.output) == invalidConnection.FromPortIndex &&
                GetPortIndex(edge.input) == invalidConnection.ToPortIndex);

            if (edgeToRemove != null)
            {
                RemoveElement(edgeToRemove);
            }
        }

        if (connectionsToRemove.Count > 0)
        {
            currentScene.MarkDirty();
            Debug.Log($"Removed {connectionsToRemove.Count} invalid connections");
        }
    }

    /// <summary>
    /// 检查连接是否有效
    /// </summary>
    private bool IsConnectionValid(SectionConnection connection)
    {
        Node outputNode = FindNodeByGuid(connection.FromSectionGUID);
        Node inputNode = FindNodeByGuid(connection.ToSectionGUID);

        if (outputNode == null || inputNode == null)
            return false;

        Port outputPort = GetOutputPort(outputNode, connection.FromPortIndex);
        Port inputPort = GetInputPort(inputNode, connection.ToPortIndex);

        return outputPort != null && inputPort != null;
    }

    #endregion

    #region Scene Binding and Management
    /// <summary>
    /// 绑定当前界面到剧情资产
    /// </summary>
    public void BindToScene(EO_PerformanceScene scene)
    {
        // 解除旧场景的事件监听
        if (currentScene != null)
        {
            currentScene.OnDataChanged -= OnSceneDataChanged;
        }

        currentScene = scene;

        // 监听新场景的数据变化
        if (currentScene != null)
        {
            currentScene.OnDataChanged += OnSceneDataChanged;
        }

        Refresh();
    }

    /// <summary>
    /// 检查是否已绑定到指定场景
    /// </summary>
    public bool IsBoundToScene(EO_PerformanceScene scene)
    {
        return currentScene == scene;
    }

    /// <summary>
    /// 处理场景数据变化
    /// </summary>
    private void OnSceneDataChanged()
    {
        // 如果正在刷新或抑制事件，忽略
        if (isRefreshing || suppressDataEvents) return;

        // 延迟刷新，避免递归
        this.schedule.Execute(() => {
            if (!suppressDataEvents)
            {
                Refresh();
            }
        }).ExecuteLater(10);
    }
    #endregion

    #region UI Refresh and Display
    public void Refresh()
    {
        if (currentScene == null)
        {
            Clear();
            return;
        }
        isRefreshing = true;
        try
        {
            // Update scene name field
            sceneNameField?.SetValueWithoutNotify(currentScene.SceneName);

            // Refresh node graph display
            RefreshNodeGraph();
        }
        finally
        {
            isRefreshing = false;
        }
    }

    public void ClearGraph()
    {
        sceneNameField?.SetValueWithoutNotify("");
        ClearNodeGraph();
    }

    private void RefreshNodeGraph()
    {
        // Clear existing graph
        ClearNodeGraph();

        if (currentScene == null) return;

        // 从数据创建特殊节点
        CreateSpecialNodesFromData();

        foreach (var section in currentScene.Sections)
        {
            CreateNodeFromSection(section);
        }

        // 检查有没有失效的连线
        ValidateAllConnections();

        // Create connections
        CreateConnections();
    }

    private void CreateSpecialNodesFromData()
    {
        if (currentScene == null) return;

        // 创建起始节点并保存引用
        currentEntryNode = new FBPerformanceEntryNode();
        currentEntryNode.Initialize(currentScene.EntryNodePosition);
        AddElement(currentEntryNode);

        // 创建结束节点并保存引用（传递场景数据）
        currentExitNode = new FBPerformanceExitNode();
        currentExitNode.Initialize(currentScene.ExitNodePosition, currentScene);

        // Subscribe to pin change events for exit node
        currentExitNode.OnPinsChanged += OnNodePinsChanged;

        AddElement(currentExitNode);
    }

    private void CreatePerformanceNode<T>(string nodeTitle, Vector2 position) where T : FBPerformanceNode, new()
    {
        if (currentScene == null) return;

        var graphMousePos = this.ChangeCoordinatesTo(contentViewContainer, position);
        var node = new T();
        node.Initialize(nodeTitle, new Rect(graphMousePos, Vector2.zero));

        // Create corresponding section data
        var section = currentScene.CreateNewSection(graphMousePos);
        section.SectionName = nodeTitle;
        node.BindSectionData(section);

        AddElement(node);
        currentScene.MarkDirty();
    }

    private void CreateNodeFromSection(EO_PerformanceSection section)
    {
        var node = new FBPerformanceNode();
        node.Initialize(section.SectionName, new Rect(section.Position, Vector2.zero));
        node.BindSectionData(section);

        // Subscribe to pin change events
        node.OnPinsChanged += OnNodePinsChanged;

        AddElement(node);
    }

    private void ClearNodeGraph()
    {
        // 清理全部事件订阅，随后删除节点和连线
        foreach (var node in nodes)
        {
            if (node is FBPerformanceNode perfNode)
            {
                perfNode.OnPinsChanged -= OnNodePinsChanged;
            }
            else if (node is FBPerformanceExitNode exitNode)
            {
                exitNode.OnPinsChanged -= OnNodePinsChanged;
            }
        }

        DeleteElements(nodes);
        DeleteElements(edges);

        currentEntryNode = null;
        currentExitNode = null;
    }
    #endregion

    #region Connection Management
    private void CreateConnections()
    {
        if (currentScene == null) return;

        foreach (var connection in currentScene.Connections)
        {
            // Find the nodes for this connection（支持特殊节点）
            Node outputNode = FindNodeByGuid(connection.FromSectionGUID);
            Node inputNode = FindNodeByGuid(connection.ToSectionGUID);

            if (outputNode != null && inputNode != null)
            {
                Port outputPort = GetOutputPort(outputNode, connection.FromPortIndex);
                Port inputPort = GetInputPort(inputNode, connection.ToPortIndex);

                if (outputPort != null && inputPort != null)
                {
                    var edge = outputPort.ConnectTo(inputPort);
                    AddElement(edge);
                }
            }
        }
    }

    /// <summary>
    /// 保存连接到数据
    /// </summary>
    private void SaveConnectionToData(Edge edge)
    {
        if (currentScene == null) return;

        var outputNode = edge.output.node;
        var inputNode = edge.input.node;

        string fromGuid = GetNodeGuid(outputNode);
        string toGuid = GetNodeGuid(inputNode);

        // 获取端口索引
        int fromPortIndex = GetPortIndex(edge.output);
        int toPortIndex = GetPortIndex(edge.input);

        if (!string.IsNullOrEmpty(fromGuid) && !string.IsNullOrEmpty(toGuid))
        {
            currentScene.AddConnection(fromGuid, fromPortIndex, toGuid, toPortIndex);
        }
    }

    /// <summary>
    /// 从数据移除连接
    /// </summary>
    private void RemoveConnectionFromData(Edge edge)
    {
        if (currentScene == null) return;

        var outputNode = edge.output.node;
        var inputNode = edge.input.node;

        string fromGuid = GetNodeGuid(outputNode);
        string toGuid = GetNodeGuid(inputNode);

        // 获取端口索引
        int fromPortIndex = GetPortIndex(edge.output);
        int toPortIndex = GetPortIndex(edge.input);

        if (!string.IsNullOrEmpty(fromGuid) && !string.IsNullOrEmpty(toGuid))
        {
            currentScene.Connections.RemoveAll(c =>
                c.FromSectionGUID == fromGuid &&
                c.ToSectionGUID == toGuid &&
                c.FromPortIndex == fromPortIndex &&
                c.ToPortIndex == toPortIndex);
            currentScene.MarkDirty();
        }
    }
    #endregion

    #region Node and Port Utilities
    /// <summary>
    /// 通过GUID查找节点（支持特殊节点）
    /// </summary>
    private Node FindNodeByGuid(string nodeGuid)
    {
        if (nodeGuid == currentScene.EntryNodeGUID)
        {
            return currentEntryNode;
        }
        else if (nodeGuid == currentScene.ExitNodeGUID)
        {
            return currentExitNode;
        }
        else
        {
            foreach (var node in nodes)
            {
                if (node is FBPerformanceNode perfNode && perfNode.BoundSection.SectionGUID == nodeGuid)
                {
                    return perfNode;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 获取节点的GUID（支持特殊节点和普通节点）
    /// </summary>
    private string GetNodeGuid(Node node)
    {
        if (node is FBPerformanceEntryNode)
        {
            return currentScene?.EntryNodeGUID;
        }
        else if (node is FBPerformanceExitNode)
        {
            return currentScene?.ExitNodeGUID;
        }
        else if (node is FBPerformanceNode perfNode)
        {
            return perfNode.BoundSection?.SectionGUID;
        }
        return null;
    }

    /// <summary>
    /// 获取端口索引（支持多端口节点）
    /// </summary>
    private int GetPortIndex(Port port)
    {
        if (port.node is FBPerformanceExitNode exitNode)
        {
            // 对于结束节点，返回输入端口索引
            var ports = exitNode.GetInputPorts();
            return ports.IndexOf(port as Port);
        }
        else if (port.node is FBPerformanceEntryNode)
        {
            return 0; // 起始节点只有一个输出端口
        }
        else if (port.node is FBPerformanceNode perfNode)
        {
            // 对于普通节点，检查是输入还是输出端口
            var inputPorts = perfNode.inputContainer.Query<Port>().ToList();
            var outputPorts = perfNode.GetOutputPorts();

            if (inputPorts.Contains(port))
            {
                return 0; // 输入端口只有一个
            }
            else if (outputPorts.Contains(port))
            {
                return outputPorts.IndexOf(port);
            }
        }

        return 0; // 默认返回第一个端口
    }

    /// <summary>
    /// 获取节点的输出端口（支持多端口）
    /// </summary>
    private Port GetOutputPort(Node node, int portIndex = 0)
    {
        if (node is FBPerformanceEntryNode entryNode)
        {
            return entryNode.OutputPort;
        }
        else if (node is FBPerformanceNode perfNode)
        {
            var outputPorts = perfNode.GetOutputPorts();
            return portIndex >= 0 && portIndex < outputPorts.Count ? outputPorts[portIndex] : null;
        }
        return null;
    }

    /// <summary>
    /// 获取节点的输入端口（支持多端口）
    /// </summary>
    private Port GetInputPort(Node node, int portIndex = 0)
    {
        if (node is FBPerformanceExitNode exitNode)
        {
            var ports = exitNode.GetInputPorts();
            return portIndex >= 0 && portIndex < ports.Count ? ports[portIndex] : null;
        }
        else if (node is FBPerformanceNode perfNode)
        {
            return perfNode.inputContainer.Q<Port>();
        }
        return null;
    }

    /// <summary>
    /// 获取节点的输出端口
    /// </summary>
    private Port GetOutputPort(Node node)
    {
        if (node is FBPerformanceEntryNode entryNode)
        {
            return entryNode.OutputPort;
        }
        else if (node is FBPerformanceNode perfNode)
        {
            return perfNode.outputContainer.Q<Port>();
        }
        return null;
    }

    /// <summary>
    /// 获取节点的输入端口
    /// </summary>
    private Port GetInputPort(Node node)
    {
        if (node is FBPerformanceExitNode exitNode)
        {
            var ports = exitNode.GetInputPorts();
            return ports.Count > 0 ? ports[0] : null;
        }
        else if (node is FBPerformanceNode perfNode)
        {
            return perfNode.inputContainer.Q<Port>();
        }
        return null;
    }
    #endregion

    #region Data Persistence
    /// <summary>
    /// 安全地更新节点位置（不触发事件循环）
    /// </summary>
    public void UpdateNodePositionsSilently()
    {
        suppressDataEvents = true;
        try
        {
            if (currentScene != null)
            {
                foreach (var node in nodes)
                {
                    if (node is FBPerformanceEntryNode entryNode)
                    {
                        var position = entryNode.GetPosition();
                        if (currentScene.EntryNodePosition != position.position)
                        {
                            currentScene.EntryNodePosition = position.position;
                        }
                    }
                    else if (node is FBPerformanceExitNode exitNode)
                    {
                        var position = exitNode.GetPosition();
                        if (currentScene.ExitNodePosition != position.position)
                        {
                            currentScene.ExitNodePosition = position.position;
                        }
                    }
                    else if (node is FBPerformanceNode perfNode)
                    {
                        if (perfNode.BoundSection != null)
                        {
                            perfNode.BoundSection.Position = node.GetPosition().position;
                        }
                    }
                }
            }
        }
        finally
        {
            suppressDataEvents = false;
        }
    }

    /// <summary>
    /// 手动保存特殊节点位置（在窗口关闭或保存时调用）
    /// </summary>
    public void SaveSpecialNodePositions()
    {
        if (currentScene == null) return;

        if (currentEntryNode != null)
        {
            currentScene.EntryNodePosition = currentEntryNode.GetPosition().position;
        }
        if (currentExitNode != null)
        {
            currentScene.ExitNodePosition = currentExitNode.GetPosition().position;
        }

        currentScene.MarkDirty();
    }
    #endregion
}