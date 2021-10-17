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
using ubv.microservices;

namespace ubv.client.logic
{
    /// <summary>
    /// Represents the client state after he's logged in, and before he finds a lobby
    /// </summary>
    public class ClientCharacterSelectState : ClientSyncState, tcp.client.ITCPClientReceiver
    {
        private enum SubState
        {
            SUBSTATE_WAITING_FOR_TCP,
            SUBSTATE_WAITING_FOR_UDP,
            SUBSTATE_GOING_TO_LOBBY,
            SUBSTATE_TRANSITION,
        }

        [SerializeField] private string m_clientLobbyScene;
        
        [SerializeField] private float m_UDPPingTimerIntervalMS = 500f;
        private float m_UPDPingTimer;
        
        private byte[] m_identificationMessageBytes;

        private bool m_receivedUDPConfirmation;
        private SubState m_currentSubState;

        static private CharacterData m_activeCharacter = null;
        static private ServerInfo? m_cachedServerInfo = null;
        
        protected override void StateLoad()
        {
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_TCP;
            m_receivedUDPConfirmation = false;

#if DEBUG_LOG
            Debug.Log("Initializing client state [init]");
#endif // DEBUG_LOG

            m_identificationMessageBytes = new IdentificationMessage().GetBytes();
            m_TCPClient.SetPlayerID(PlayerID.Value);
            m_TCPClient.Subscribe(this);

            // try to fetch characters from microservice
            if (m_activeCharacter == null)
            {
                m_characterService.Request(new GetCharactersFromUserRequest(UserInfo.ID, OnCharactersFetchedFromService));
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

        public override void StateUpdate()
        {
            switch (m_currentSubState)
            {
                case SubState.SUBSTATE_WAITING_FOR_TCP:
                    break;
                case SubState.SUBSTATE_WAITING_FOR_UDP:
                    if (m_TCPClient.IsConnected())
                    {
                        m_UDPPingTimerIntervalMS += Time.deltaTime;
                        if (m_UPDPingTimer > m_UDPPingTimerIntervalMS / 1000f)
                        {
                            m_UPDPingTimer = 0;
                            SendUDPIdentificationPing();
                        }
                    }
                    break;
                case SubState.SUBSTATE_GOING_TO_LOBBY:
                    if (m_activeCharacter != null)
                    {
                        LoadingData.ActiveCharacterID = m_activeCharacter.ID;
                        GoToLobby();
                    }
                    break;
                default:
                    break;
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

                m_TCPClient.Connect(serverInfo.server_tcp_ip, serverInfo.tcp_port);
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
            m_currentSubState = SubState.SUBSTATE_TRANSITION;
            ClientStateManager.Instance.PushState(m_clientLobbyScene);
        }
        

        public void OnDisconnect()
        {
#if DEBUG_LOG
            Debug.Log("Disconnected from server");
#endif // DEBUG_LOG
            
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_TCP;
            m_receivedUDPConfirmation = false;
        }

        public void OnSuccessfulTCPConnect()
        {
#if DEBUG_LOG
            Debug.Log("Successful TCP connection to server.");
#endif // DEBUG_LOG
            
        }

        private void SendUDPIdentificationPing()
        {
            m_UDPClient.Send(m_identificationMessageBytes, PlayerID.Value);
        }
        
        public void ReceivePacket(TCPToolkit.Packet packet)
        {
            if (m_currentSubState == SubState.SUBSTATE_WAITING_FOR_TCP)
            {
                ServerSuccessfulTCPConnectMessage serverSuccessPing = common.serialization.IConvertible.CreateFromBytes<ServerSuccessfulTCPConnectMessage>(packet.Data.ArraySegment());
                if (serverSuccessPing != null)
                {
#if DEBUG_LOG
                    Debug.Log("Received TCP connection confirmation (via TCP).");
#endif // DEBUG_LOG
                    m_currentSubState = SubState.SUBSTATE_WAITING_FOR_UDP;
                    m_UDPClient.SetTargetServer(m_cachedServerInfo.Value.server_udp_ip, m_cachedServerInfo.Value.udp_port);
                    SendUDPIdentificationPing();
                }
            }
            else if (m_currentSubState == SubState.SUBSTATE_WAITING_FOR_UDP)
            {
                ServerSuccessfulUDPConnectMessage serverSuccessPing = common.serialization.IConvertible.CreateFromBytes<ServerSuccessfulUDPConnectMessage>(packet.Data.ArraySegment());
                if (serverSuccessPing != null)
                {
#if DEBUG_LOG
                    Debug.Log("Received UDP connection confirmation (via TCP).");
#endif // DEBUG_LOG
                    m_currentSubState = SubState.SUBSTATE_GOING_TO_LOBBY;
                }
            }
        }

        protected override void StateUnload()
        {
            m_TCPClient.Unsubscribe(this);
        }

        protected override void StatePause()
        {
            m_TCPClient.Unsubscribe(this);
        }

        protected override void StateResume()
        {
            m_TCPClient.Subscribe(this);
        }
    }   
}
