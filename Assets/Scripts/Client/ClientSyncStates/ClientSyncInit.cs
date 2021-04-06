using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;
using System.Net.Http;
using static ubv.microservices.DispatcherMicroservice;

namespace ubv.client.logic
{
    public class ClientSyncInit : ClientSyncState, udp.client.IUDPClientReceiver, tcp.client.ITCPClientReceiver
    {
        private ServerInfo? m_cachedServerInfo;
        private bool m_connected;
        private bool m_waitingOnUDPResponse;

        [SerializeField] private float m_reconnectTimerIntervalMS = 3000f;
        private float m_requestResendTimer;

        [SerializeField] private float m_UDPPingTimerIntervalMS = 500f;
        private float m_UPDPingTimer;
        
        private byte[] m_identificationMessageBytes;
        
        protected override void StateAwake()
        {
            ClientSyncState.m_initState = this;
            ClientSyncState.m_currentState = this;
            m_cachedServerInfo = null;
            Init();
        }

        public void Init()
        {
#if DEBUG_LOG
            Debug.Log("Initializing client state [init]");
#endif // DEBUG_LOG
            m_connected = false;
            m_waitingOnUDPResponse = false;
            m_requestResendTimer = 0;
            m_identificationMessageBytes = new IdentificationMessage(PlayerID.Value).GetBytes();
            m_TCPClient.SetPlayerID(PlayerID.Value);
            m_UDPClient.Subscribe(this);
        }

        protected override void StateUpdate()
        {
            if (m_cachedServerInfo != null && !m_connected && !m_waitingOnUDPResponse)
            {
                m_requestResendTimer += Time.deltaTime;
                if(m_requestResendTimer > m_reconnectTimerIntervalMS / 1000f)
                {
#if DEBUG_LOG
                    Debug.Log("Trying to reconnect to server : " + m_cachedServerInfo.Value.server_ip.ToString() + " ...");
#endif // DEBUG_LOG
                    m_requestResendTimer = 0;
                    EstablishConnectionToServer(m_cachedServerInfo.Value);
                }
            }

            if (m_waitingOnUDPResponse)
            {
                m_UDPPingTimerIntervalMS += Time.deltaTime;
                if (m_UPDPingTimer > m_UDPPingTimerIntervalMS / 1000f)
                {
                    m_UPDPingTimer = 0;
                    m_UDPClient.Send(m_identificationMessageBytes, PlayerID.Value);
                }
            }
        }

        public void SendConnectionRequestToServer()
        {
#if DEBUG_LOG
            Debug.Log("Sending connection request to dispatcher...");
#endif // DEBUG_LOG

            m_dispatcherService.RequestServerInfo(PlayerID.Value, OnServerInfoReceived);
        }

        private void OnServerInfoReceived(ServerInfo? info)
        {
            m_cachedServerInfo = info;
            EstablishConnectionToServer(m_cachedServerInfo.Value);
        }

        private void EstablishConnectionToServer(ServerInfo serverInfo)
        {
            if (!m_connected)
            {
#if DEBUG_LOG
                Debug.Log("Trying to establish connection to game server...");
#endif // DEBUG_LOG
                
                m_TCPClient.Connect(serverInfo.server_ip, serverInfo.tcp_port);

                // TODO : make sure server receives UDP ping
                // maybe with TCP reception of player list and confirming itself?
                m_UDPClient.SetTargetServer(serverInfo.server_ip, serverInfo.udp_port);
                m_UDPClient.Send(m_identificationMessageBytes, PlayerID.Value);
                m_waitingOnUDPResponse = true;
            }
        }
        
        private void GoToLobby()
        {
#if DEBUG_LOG
            Debug.Log("Received TCP connection confirmation. Going to lobby");
#endif // DEBUG_LOG

            ClientSyncState.m_lobbyState.Init(PlayerID.Value);
            ClientSyncState.m_currentState = ClientSyncState.m_lobbyState;
            m_UDPClient.Unsubscribe(this);
        }

        public void OnDisconnect()
        {
#if DEBUG_LOG
            Debug.Log("Disconnected from server");
#endif // DEBUG_LOG
            m_connected = false;
        }

        public void OnSuccessfulConnect()
        {
#if DEBUG_LOG
            Debug.Log("Successful connection to server. Sending identification message with ID " + PlayerID.Value);
#endif // DEBUG_LOG
            m_TCPClient.Send(m_identificationMessageBytes); // sends a ping to the server
            m_connected = true;
        }

        public void ReceivePacket(UDPToolkit.Packet packet)
        {
            ServerSuccessfulConnectMessage serverSuccessPing = common.serialization.IConvertible.CreateFromBytes<ServerSuccessfulConnectMessage>(packet.Data.ArraySegment());
            if (serverSuccessPing != null)
            {
                m_waitingOnUDPResponse = false;
                GoToLobby();
            }
        }

        public void ReceivePacket(TCPToolkit.Packet packet)
        { }
    }   
}
