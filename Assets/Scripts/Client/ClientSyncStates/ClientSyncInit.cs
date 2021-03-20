using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;
using System.Net.Http;

namespace ubv.client.logic
{
    public class ClientSyncInit : ClientSyncState, tcp.client.ITCPClientReceiver
    {
        // when dispatcher is ready
        // [SerializeField] EndPoint m_dispatcherEndpoint;

        [SerializeField] string m_serverTCPAddress;
        [SerializeField] int m_serverTCPPort;
        [SerializeField] string m_serverUDPAddress;
        [SerializeField] int m_serverUDPPort;

        private struct JSONServerInfo
        {
            public string TCPAddress;
            public int TCPPort;
            public string UDPAddress;
            public int UDPPort;
        }

        private int? m_playerID;

        protected override void StateAwake()
        {
            ClientSyncState.m_initState = this;
            ClientSyncState.m_currentState = this;
        }
        
        public void SendConnectionRequestToServer()
        {
            int playerID = System.Guid.NewGuid().GetHashCode(); // for now
            m_playerID = playerID;

            // mock dispatcher response for now
            HttpResponseMessage msg = new HttpResponseMessage();
            string jsonString = JsonUtility.ToJson(new JSONServerInfo
            {
                TCPAddress = m_serverTCPAddress,
                TCPPort = m_serverTCPPort,
                UDPAddress = m_serverUDPAddress,
                UDPPort = m_serverUDPPort
            }).ToString();
            msg.Content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
            msg.StatusCode = HttpStatusCode.OK;
            OnDispatcherResponse(msg);

            // uncomment when dispatcher ready
            // m_HTTPClient.Get("dispatcher/" + playerID.ToString(), OnDispatcherResponse);
        }

        private void OnDispatcherResponse(HttpResponseMessage message)
        {
            if (message.StatusCode == HttpStatusCode.OK)
            {
                string JSON = message.Content.ReadAsStringAsync().Result;
                JSONServerInfo serverInfo = JsonUtility.FromJson<JSONServerInfo>(JSON);
                Debug.Log("Received from dispatcher : " + JSON);
                string address = serverInfo.TCPAddress;
                int port = serverInfo.TCPPort;

                // send a ping to the server to make it known that the player received its ID
                IdentificationMessage identificationMessage = new IdentificationMessage(m_playerID.Value);

                m_TCPClient.Connect(address, port);
                m_TCPClient.Subscribe(this);
                m_TCPClient.Send(identificationMessage.GetBytes()); // sends a ping to the server
                
                // TODO : make sure server receives UDP ping
                // maybe with TCP reception of player list and confirming itself?
                m_UDPClient.SetTargetServer(serverInfo.UDPAddress, serverInfo.UDPPort);
                m_UDPClient.Send(identificationMessage.GetBytes());
            }
            else
            {
                Debug.Log("Dispatcher GET request was not successful");
            }
        }

        public void ReceivePacket(tcp.TCPToolkit.Packet packet)
        {
            // receive auth message and set player id
            ServerSuccessfulConnectMessage serverSuccessPing = common.serialization.IConvertible.CreateFromBytes<ServerSuccessfulConnectMessage>(packet.Data);
            if (serverSuccessPing != null)
            {
#if DEBUG_LOG
                Debug.Log("Received TCP connection confirmation. Going to lobby");
#endif // DEBUG_LOG

                
                ClientSyncState.m_lobbyState.Init(m_playerID.Value);
                ClientSyncState.m_currentState = ClientSyncState.m_lobbyState;
                m_TCPClient.Unsubscribe(this);
            }
        }

        public void OnDisconnect()
        {

        }
    }   
}
