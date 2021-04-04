using UnityEngine;
using System.Collections;
using ubv.http;
using System.Net.Http;
using System.Net;

namespace ubv.microservices
{
    public class DispatcherMicroservice : MonoBehaviour
    {
        [SerializeField] private HTTPClient m_HTTPClient;
        [SerializeField] string m_dispatcherEndpoint;

        [Header("Mocked properties")]
        [SerializeField] private bool m_mock;
        [SerializeField] string m_serverAddress;
        [SerializeField] int m_serverTCPPort;
        [SerializeField] int m_serverUDPPort;

        public delegate void OnServerInfo(ServerInfo? info);

        private OnServerInfo m_onServerInfoCallback;

        public struct ServerInfo
        {
            public string server_ip;
            public int tcp_port;
            public int udp_port;
        }

        private struct JSONDispatcherRequest
        {
            public int id;
            public string ip;
        }

        public void RequestServerInfo(int playerID, OnServerInfo onServerInfo)
        {
            if (m_mock)
            {
                onServerInfo(new ServerInfo
                {
                    server_ip = m_serverAddress,
                    tcp_port = m_serverTCPPort,
                    udp_port = m_serverUDPPort
                });
                return;
            }

            m_onServerInfoCallback = onServerInfo;
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

                m_onServerInfoCallback.Invoke(serverInfo);
                m_onServerInfoCallback = null;
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
