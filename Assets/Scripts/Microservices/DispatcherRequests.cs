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
    
    public class GetServerInfoRequest : GetDispatcherRequest
    {
        public readonly string GameID;

        public GetServerInfoRequest(string gameID, UnityAction<ServerInfo> callback, UnityAction<string> failCallback) : base(callback, failCallback)
        {
            GameID = gameID;
        }

        public override string URL()
        {
            return "IP/" + GameID;
        }
    }

    public class GetNewServerInfoRequest : GetDispatcherRequest
    {
        public GetNewServerInfoRequest(UnityAction<ServerInfo> callback, UnityAction<string> failCallback) : base(callback, failCallback)
        {
            Debug.Log("Made it to Line 49 of DispatcherRequests.cs");
        }

        public override string URL()
        {
            return "GameServer";
        }
    }

    public abstract class GetDispatcherRequest : GetMicroserviceRequest
    {
        public readonly UnityAction<ServerInfo> SuccessCallback;

        public GetDispatcherRequest(UnityAction<ServerInfo> callback, UnityAction<string> failCallback) : base(failCallback)
        {
            SuccessCallback = callback;
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
