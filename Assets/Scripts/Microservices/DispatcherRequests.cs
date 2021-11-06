using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace ubv.microservices
{
    public class ServerInfo
    {
        public readonly string GameID;
        public readonly string ServerTCPAddress;
        public readonly string ServerUDPAddress;

        public readonly int ServerTCPPort;
        public readonly int ServerUDPPort;

        public ServerInfo(string gameID, string TCPAddress, string UDPAddress, int TCPPort, int UDPPort)
        {
            GameID = gameID;
            ServerTCPAddress = TCPAddress;
            ServerUDPAddress = UDPAddress;
            ServerTCPPort = TCPPort;
            ServerUDPPort = UDPPort;
        }
    }
    
    public class GetServerInfoRequest : GetMicroserviceRequest
    {
        public readonly string GameID;
        public readonly UnityAction<ServerInfo> SuccessCallback;

        public GetServerInfoRequest(string gameID, UnityAction<ServerInfo> callback, UnityAction<string> failCallback) : base(failCallback)
        {
            GameID = gameID;
            SuccessCallback = callback;
        }

        public override string URL()
        {
            return "IP/" + GameID;
        }
    }

    public class PostServerRequest : PostMicroserviceRequest
    {
        public readonly UnityAction<ServerInfo> Success;

        public PostServerRequest(UnityAction<ServerInfo> success, UnityAction<string> fail) : base(fail)
        {
            Success = success;
        }

        public override string JSONString()
        {
            return string.Empty;
        }

        public override string URL()
        {
            return "GameServer";
        }
    }

}
