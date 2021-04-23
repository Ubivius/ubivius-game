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
using static ubv.microservices.CharacterDataService;
using UnityEngine.Events;
using ubv.utils;

namespace ubv.client.logic
{
    public class ClientSyncInit : ClientSyncState, udp.client.IUDPClientReceiver, tcp.client.ITCPClientReceiver
    {
        [SerializeField] private string m_clientLobbyScene;

        private ServerInfo? m_cachedServerInfo;

        [SerializeField] private float m_UDPPingTimerIntervalMS = 500f;
        private float m_UPDPingTimer;
        
        private byte[] m_identificationMessageBytes;

        private Flag m_readyToGoToLobby;
        private bool m_loadingLobby;

        static private CharacterData m_activeCharacter = null;
        
        protected override void StateAwake()
        {
            m_readyToGoToLobby = new Flag();
            m_loadingLobby = false;
            ClientSyncState.m_initState = this;
            ClientSyncState.m_currentState = this;
            Init();
        }

        public void Init(bool clearServerInfo = true)
        {
#if DEBUG_LOG
            Debug.Log("Initializing client state [init]");
#endif // DEBUG_LOG
            
            m_identificationMessageBytes = new IdentificationMessage().GetBytes();
            m_TCPClient.SetPlayerID(PlayerID.Value);
            m_TCPClient.Subscribe(this);
            if (clearServerInfo)
            {
                m_cachedServerInfo = null;
            }
        }

        private void Start()
        {
            // try to fetch characters from microservice
            if (m_activeCharacter == null)
            { 
                m_characterService.GetCharacters(UserInfo.ID, OnCharactersFetchedFromService);
            }
        }

        private void OnCharactersFetchedFromService(CharacterData[] characters)
        {
            // for now, assume only one character 
            // take the only character available and treat it as active
            m_activeCharacter = characters[0];
#if DEBUG_LOG
            Debug.Log("Fetched character " + m_activeCharacter.Name + " from microservice.");
#endif //DEBUG_LOG
            
        }

        public CharacterData GetActiveCharacter()
        {
            return m_activeCharacter;
        }

        protected override void StateUpdate()
        {
            lock (m_lock)
            {
                if (m_TCPClient.IsConnected())
                {
                    if (!m_loadingLobby && m_activeCharacter != null)
                    {
                        if (m_readyToGoToLobby.Read())
                        {
                            GoToLobby();
                        }
                    }
                }
            }

            if (m_TCPClient.IsConnected() && !m_readyToGoToLobby.Peek())
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
            if (!m_TCPClient.IsConnected())
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
            m_loadingLobby = true;
            StartCoroutine(LoadLobbyCoroutine());
        }
        
        private IEnumerator LoadLobbyCoroutine()
        {
            m_UDPClient.Unsubscribe(this);

            // animation petit cercle de load to scene
            AsyncOperation loadLobby = SceneManager.LoadSceneAsync(m_clientLobbyScene);
            while (!loadLobby.isDone)
            {
                yield return null;
            }

            m_loadingLobby = false;
            m_TCPClient.Unsubscribe(this);
            SetLobbyStateActive();
        }

        private void SetLobbyStateActive()
        {
            ClientSyncState.m_lobbyState.Init(GetActiveCharacter().ID);
            ClientSyncState.m_currentState = ClientSyncState.m_lobbyState;
        }

        public void OnDisconnect()
        {
#if DEBUG_LOG
            Debug.Log("Disconnected from server");
#endif // DEBUG_LOG

            m_readyToGoToLobby.Reset();
        }

        public void OnSuccessfulTCPConnect()
        {
#if DEBUG_LOG
            Debug.Log("Successful TCP connection to server. Sending UDP identification message with ID " + PlayerID.Value);
#endif // DEBUG_LOG
            
            m_UDPClient.Subscribe(this);
            m_readyToGoToLobby.Reset();
            m_UDPClient.SetTargetServer(m_cachedServerInfo.Value.server_ip, m_cachedServerInfo.Value.udp_port);
            m_UDPClient.Send(m_identificationMessageBytes, PlayerID.Value);
        }

        public void ReceivePacket(UDPToolkit.Packet packet)
        {
            ServerSuccessfulConnectMessage serverSuccessPing = common.serialization.IConvertible.CreateFromBytes<ServerSuccessfulConnectMessage>(packet.Data.ArraySegment());
            if (serverSuccessPing != null)
            {
#if DEBUG_LOG
                Debug.Log("Received UDP connection confirmation.");
#endif // DEBUG_LOG

                m_readyToGoToLobby.Raise();
            }
        }

        public void ReceivePacket(TCPToolkit.Packet packet)
        { }
    }   
}
