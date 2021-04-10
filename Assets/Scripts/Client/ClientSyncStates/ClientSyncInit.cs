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
        [SerializeField] private string m_clientLobbyScene;

        private ServerInfo? m_cachedServerInfo;
        private bool m_connected;
        private bool m_waitingOnUDPResponse;

        [SerializeField] private float m_reconnectTimerIntervalMS = 3000f;
        private float m_requestResendTimer;

        [SerializeField] private float m_UDPPingTimerIntervalMS = 500f;
        private float m_UPDPingTimer;
        
        private byte[] m_identificationMessageBytes;

        private bool m_readyToGoToLobby;
        
        protected override void StateAwake()
        {
            m_readyToGoToLobby = false;
            ClientSyncState.m_initState = this;
            ClientSyncState.m_currentState = this;
            m_cachedServerInfo = null;
            Init();
        }

        public void Init(bool clearServerInfo = true)
        {
#if DEBUG_LOG
            Debug.Log("Initializing client state [init]");
#endif // DEBUG_LOG
            m_connected = false;
            m_waitingOnUDPResponse = false;
            m_requestResendTimer = 0;
            m_identificationMessageBytes = new IdentificationMessage().GetBytes();
            m_TCPClient.SetPlayerID(PlayerID.Value);
            m_UDPClient.Subscribe(this);
            m_TCPClient.Subscribe(this);
            if (clearServerInfo)
            {
                m_cachedServerInfo = null;
            }
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

            if (m_connected && m_waitingOnUDPResponse)
            {
                m_UDPPingTimerIntervalMS += Time.deltaTime;
                if (m_UPDPingTimer > m_UDPPingTimerIntervalMS / 1000f)
                {
                    m_UPDPingTimer = 0;
                    m_UDPClient.Send(m_identificationMessageBytes, PlayerID.Value);
                }
            }

            if (m_readyToGoToLobby)
            {
                m_readyToGoToLobby = false;
                GoToLobby();
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
            }
#if DEBUG_LOG
            else
            {
                Debug.Log("Already connected to game server.");
            }
#endif // DEBUG_LOG
        }

        private void GoToLobby()
        {
            m_UDPClient.Unsubscribe(this);
            m_TCPClient.Unsubscribe(this);
            StartCoroutine(LoadLobbyCoroutine());
        }
        
        private IEnumerator LoadLobbyCoroutine()
        {
            // animation petit cercle de load to scene
            AsyncOperation loadLobby = SceneManager.LoadSceneAsync(m_clientLobbyScene);
            while (!loadLobby.isDone)
            {
                yield return null;
            }

            ClientSyncState.m_lobbyState.Init(PlayerID.Value);
            ClientSyncState.m_currentState = ClientSyncState.m_lobbyState;
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

            m_UDPClient.SetTargetServer(m_cachedServerInfo.Value.server_ip, m_cachedServerInfo.Value.udp_port);
            m_UDPClient.Send(m_identificationMessageBytes, PlayerID.Value);
            m_waitingOnUDPResponse = true;
        }

        public void ReceivePacket(UDPToolkit.Packet packet)
        {
            ServerSuccessfulConnectMessage serverSuccessPing = common.serialization.IConvertible.CreateFromBytes<ServerSuccessfulConnectMessage>(packet.Data.ArraySegment());
            if (serverSuccessPing != null)
            {
                m_waitingOnUDPResponse = false;
#if DEBUG_LOG
                Debug.Log("Received TCP/UDP connection confirmation. Going to lobby");
#endif // DEBUG_LOG
                m_readyToGoToLobby = true;
            }
        }

        public void ReceivePacket(TCPToolkit.Packet packet)
        { }
    }   
}
