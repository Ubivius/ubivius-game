using UnityEngine;
using System.Collections;
using ubv.http.client;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;

namespace ubv.microservices
{
    public class DispatcherMicroservice : MonoBehaviour
    {
        [SerializeField] private HTTPClient m_HTTPClient;
        [SerializeField] string m_dispatcherEndpoint;

        [Header("Mocked properties")]
        [SerializeField] private bool m_mock;
        [SerializeField] string m_serverTCPAddress;
        [SerializeField] string m_serverUDPAddress;
        [SerializeField] int m_serverTCPPort;
        [SerializeField] int m_serverUDPPort;

        public delegate void OnServerInfo(ServerInfo? info);
        private class ServerInfoRequest
        {
            public int PlayerID;
            public OnServerInfo Callback;
        }

        private bool m_readyForNextRequest;
        private Queue<ServerInfoRequest> m_serverInfoRequests;
        
        public struct ServerInfo
        {
            public string server_ip;
            public string server_tcp_ip;
            public string server_udp_ip;
            public int tcp_port;
            public int udp_port;
        }

        private struct JSONDispatcherRequest
        {
            public int id;
            public string ip;
        }

        private void Awake()
        {
            m_readyForNextRequest = true;
            m_serverInfoRequests = new Queue<ServerInfoRequest>();
        }

        private void Update()
        {
            if (m_readyForNextRequest)
            {
                if(m_serverInfoRequests.Count > 0)
                {
                    ServerInfoRequest request = m_serverInfoRequests.Peek();
                    RequestServerInfo(request.PlayerID);
                }
            }
        }

        public void RequestServerInfo(int playerID, OnServerInfo onServerInfo)
        {
            if (m_mock)
            {
                onServerInfo(new ServerInfo
                {
                    server_tcp_ip = m_serverTCPAddress,
                    server_udp_ip = m_serverUDPAddress,
                    tcp_port = m_serverTCPPort,
                    udp_port = m_serverUDPPort
                });
                return;
            }

            if (!m_readyForNextRequest)
            {
                m_serverInfoRequests.Enqueue(new ServerInfoRequest() { PlayerID = playerID, Callback = onServerInfo });
                return;
            }

            m_readyForNextRequest = false;
            RequestServerInfo(playerID);
        }

        private void RequestServerInfo(int playerID)
        {
            m_HTTPClient.SetEndpoint(m_dispatcherEndpoint);

            string jsonString = JsonUtility.ToJson(new JSONDispatcherRequest
            {
                id = playerID,
                ip = "0.0.0.0",
            }).ToString();

            m_HTTPClient.PostJSON("player", jsonString, OnDispatcherResponse);
        }

        private void OnDispatcherResponse(HttpResponseMessage message)
        {
            if (message.StatusCode == HttpStatusCode.OK)
            {
                string JSON = message.Content.ReadAsStringAsync().Result;
                ServerInfo serverInfo = JsonUtility.FromJson<ServerInfo>(JSON);
                
#if DEBUG_LOG
                Debug.Log("Received from dispatcher : " + JSON);
#endif // DEBUG_LOG

                m_serverInfoRequests.Dequeue().Callback.Invoke(serverInfo);
                m_readyForNextRequest = true;
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("Dispatcher request was not successful");
#endif // DEBUG_LOG
            }
        }
    }
}
