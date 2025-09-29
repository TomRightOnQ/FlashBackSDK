using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// Network Manager
/// RPC sending and protocol parsing
/// </summary>
public partial class FBNetworkManager : FBGameSystem
{
    /// Sttucts and configs in NetworkDefs.cs

    #region Fields


    private NetworkConfig config;
    private Dictionary<string, ProtocolEndpoint> endpointMap = new Dictionary<string, ProtocolEndpoint>();
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isRunning = true;

    // Callback signature: void HandleMessage(string endpointName, float[] floats, int[] ints, bool[] bools)
    public event Action<string, float[], int[], bool[]> OnMessageReceived;
    // All handlers
    private Dictionary<string, object> protocolHandlers = new Dictionary<string, object>();

    #endregion

    public override void OnSystemCreate()
    {
        base.OnSystemCreate();
    }

    public override void OnSystemInit() 
    {
        // Load all network protocols
        LoadConfiguration();
        LoadProtocolDefinitions();
    }

    public override void OnSceneUnloaded() 
    {
        Disconnect();
    }

    public override void OnSceneChange() { }

    public override void OnSceneLoadComplete() { }

    public override void ManualInit() { }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    #region Network Init and Connection

    /// <summary>
    /// Load the basic Configs
    /// </summary>
    private void LoadConfiguration()
    {
        TextAsset configFile = Resources.Load<TextAsset>("NetworkProtocols/NetworkConfig");
        config = JsonUtility.FromJson<NetworkConfig>(configFile.text);
    }

    /// <summary>
    /// Load all config jsons that defines the protocols
    /// </summary>
    private void LoadProtocolDefinitions()
    {
        // Load all protocol JSON files
        TextAsset[] protocolFiles = Resources.LoadAll<TextAsset>("NetworkProtocols/Components");

        foreach (TextAsset file in protocolFiles)
        {
            try
            {
                NetworkProtocol protocol = JsonUtility.FromJson<NetworkProtocol>(file.text);

                // Register each endpoint
                foreach (var endpoint in protocol.endpoints)
                {
                    endpointMap[endpoint.message_type] = endpoint;

                    // Find and instantiate handler class if it exists
                    string handlerClassName = file.name.Replace(".json", "");
                    if (!protocolHandlers.ContainsKey(handlerClassName))
                    {
                        Type handlerType = Type.GetType(handlerClassName);
                        if (handlerType != null)
                        {
                            protocolHandlers[handlerClassName] = Activator.CreateInstance(handlerType);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                FBDebug.Instance.FBLogError($"Failed to load protocol {file.name}: {e.Message}", this.gameObject);
            }
        }
    }

    /// <summary>
    /// Connect To Server
    /// </summary>
    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            client.Connect(config.serverIP, config.serverPort);
            stream = client.GetStream();

            receiveThread = new Thread(ReceiveLoop);
            receiveThread.Start();
            FBMainGame.System.EventSystem.PostEvent(GameEvent.Event.ON_SERVER_CONNECT);
        }
        catch (Exception e)
        {
            FBDebug.Instance.FBLogError($"Connection failed: {e.Message}", this.gameObject);
        }
    }

    /// <summary>
    /// DIsconnect from the server
    /// </summary>
    public void Disconnect()
    {
        isRunning = false;
        try
        {
            stream?.Close();
            client?.Close();
        }
        catch {}

        if (receiveThread != null && receiveThread.IsAlive)
        {
            bool threadStopped = receiveThread.Join(100);

            if (!threadStopped && Application.isEditor)
            {
                receiveThread.Abort();
            }
        }
    }


    public void SendRPC(string messageType, float[] floats = null, int[] ints = null, bool[] bools = null)
    {
        if (!endpointMap.TryGetValue(messageType, out var endpoint))
        {
            Debug.LogError($"Unknown message type: {messageType}");
            return;
        }

        try
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                // Write message header (type length + type)
                byte[] typeBytes = Encoding.ASCII.GetBytes(messageType);
                writer.Write((byte)typeBytes.Length);
                writer.Write(typeBytes);

                // Write data in field order
                foreach (var field in endpoint.fields)
                {
                    switch (field.type)
                    {
                        case "float":
                            writer.Write(floats != null && floats.Length > 0 ? floats[0] : 0f);
                            if (floats != null && floats.Length > 0)
                                floats = floats[1..]; // Remove used float
                            break;
                        case "int":
                            writer.Write(ints != null && ints.Length > 0 ? ints[0] : 0);
                            if (ints != null && ints.Length > 0)
                                ints = ints[1..]; // Remove used int
                            break;
                        case "bool":
                            writer.Write(bools != null && bools.Length > 0 ? bools[0] : false);
                            if (bools != null && bools.Length > 0)
                                bools = bools[1..]; // Remove used bool
                            break;
                    }
                }

                byte[] data = ms.ToArray();
                stream.Write(data, 0, data.Length);
            }
        }
        catch (Exception e)
        {
            FBDebug.Instance.FBLogError($"Send failed: {e.Message}", this.gameObject);
        }
    }

    private void ReceiveLoop()
    {
        byte[] buffer = new byte[1024];

        while (isRunning && client.Connected)
        {
            try
            {
                // Read message type
                int typeLength = stream.ReadByte();
                if (typeLength == -1) continue;

                byte[] typeBytes = new byte[typeLength];
                stream.Read(typeBytes, 0, typeLength);
                string messageType = Encoding.ASCII.GetString(typeBytes).TrimEnd('\0');

                if (!endpointMap.TryGetValue(messageType, out var endpoint))
                {
                    FBDebug.Instance.FBLogWarning($"Unknown message type received: {messageType}", this.gameObject);
                    continue;
                }

                // Prepare arrays for data
                List<float> floats = new List<float>();
                List<int> ints = new List<int>();
                List<bool> bools = new List<bool>();

                // Read data in field order
                using (BinaryReader reader = new BinaryReader(new NetworkStream(client.Client, false)))
                {
                    foreach (var field in endpoint.fields)
                    {
                        switch (field.type)
                        {
                            case "float":
                                floats.Add(reader.ReadSingle());
                                break;
                            case "int":
                                ints.Add(reader.ReadInt32());
                                break;
                            case "bool":
                                bools.Add(reader.ReadBoolean());
                                break;
                        }
                    }
                }

                string handlerName = GetHandlerNameForMessage(messageType);
                if (!string.IsNullOrEmpty(handlerName) && protocolHandlers.TryGetValue(handlerName, out var handler))
                {
                    InvokeHandlerMethod(handler, messageType, floats.ToArray(), ints.ToArray(), bools.ToArray());
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    private string GetHandlerNameForMessage(string messageType)
    {
        foreach (var kvp in protocolHandlers)
        {
            var endpoint = endpointMap.FirstOrDefault(e => e.Value.message_type == messageType);
            if (endpoint.Value != null)
            {
                return kvp.Key;
            }
        }
        return null;
    }

    private void InvokeHandlerMethod(object handler, string messageType, float[] f, int[] i, bool[] b)
    {
        Type handlerType = handler.GetType();
        MethodInfo method = handlerType.GetMethod("On_" + messageType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (method != null)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                try
                {
                    method.Invoke(handler, new object[] { f, i, b });
                }
                catch (Exception e)
                {
                    FBDebug.Instance.FBLogError($"Error invoking handler for {messageType}: {e.InnerException?.Message}", this.gameObject);
                }
            });
        }
    }

    #endregion
}
