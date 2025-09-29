using System.Collections.Generic;

public partial class FBNetworkManager
{
    #region Configuration

    [System.Serializable]
    public class NetworkConfig
    {
        public string serverIP = "127.0.0.1";
        public int serverPort = 8080;
    }

    [System.Serializable]
    public class NetworkProtocol
    {
        public string protocol_version;
        public List<ProtocolEndpoint> endpoints;
    }

    [System.Serializable]
    public class ProtocolEndpoint
    {
        public string name;
        public string message_type;
        public List<ProtocolField> fields;
    }

    [System.Serializable]
    public class ProtocolField
    {
        public string name;
        public string type; // "float", "int", "bool"
    }
    #endregion
}
