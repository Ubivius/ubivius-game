using UnityEngine;
using System.Collections;
using ubv.common.data;
using static ubv.microservices.CharacterDataService;
using static ubv.microservices.DispatcherMicroservice;
using System.Threading;

namespace ubv.client.logic
{
    public class ClientGameSearch : ClientSyncState, tcp.client.ITCPClientReceiver
    {
        private enum SubState
        {
            SUBSTATE_WAITING_FOR_TCP,
            SUBSTATE_WAITING_FOR_UDP,
            SUBSTATE_WAITING_FOR_REJOIN_CONFIRM,
            SUBSTATE_GOING_TO_LOBBY,
            SUBSTATE_GOING_TO_GAME,
            SUBSTATE_TRANSITION,
        }

        [SerializeField] private string m_clientGameScene;
        [SerializeField] private string m_clientLobbyScene;

        [SerializeField] private float m_UDPPingTimerIntervalMS = 500f;
        private float m_UPDPingTimer;

        private byte[] m_identificationMessageBytes;
        
        private SubState m_currentSubState;
        
        protected override void StateLoad()
        {
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_TCP;

#if DEBUG_LOG
            Debug.Log("Initializing client state [init]");
#endif // DEBUG_LOG

            m_identificationMessageBytes = new IdentificationMessage().GetBytes();
        }

        private void Start()
        {
            m_TCPClient.SetPlayerID(PlayerID.Value);
            m_TCPClient.Subscribe(this);
            SendConnectionRequestToServer();
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
                    GoToLobby();
                    break;
                case SubState.SUBSTATE_GOING_TO_GAME:
                    GoToGame();
                    break;
                case SubState.SUBSTATE_WAITING_FOR_REJOIN_CONFIRM:
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

            DispatcherService.RequestServerInfo(PlayerID.Value, OnServerInfoReceived);
        }

        private void OnServerInfoReceived(ServerInfo? info)
        {
            data.LoadingData.ServerInfo = info;
            EstablishConnectionToServer();
        }

        private void EstablishConnectionToServer()
        {
            if (!m_TCPClient.IsConnected())
            {
#if DEBUG_LOG
                Debug.Log("Trying to establish connection to game server...");
#endif // DEBUG_LOG

                m_TCPClient.Connect(data.LoadingData.ServerInfo.Value.server_tcp_ip, data.LoadingData.ServerInfo.Value.tcp_port);
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

        private void GoToGame()
        {
            m_currentSubState = SubState.SUBSTATE_TRANSITION;
            ClientStateManager.Instance.PushState(m_clientGameScene);
        }

        public void OnDisconnect()
        {
#if DEBUG_LOG
            Debug.Log("Disconnected from server");
#endif // DEBUG_LOG
            ClientStateManager.Instance.PopState();
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

        public void ReceivePacket(tcp.TCPToolkit.Packet packet)
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
                    m_UDPClient.SetTargetServer(data.LoadingData.ServerInfo.Value.server_udp_ip, data.LoadingData.ServerInfo.Value.udp_port);
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

                    if (data.LoadingData.IsTryingToRejoinGame)
                    {
                        m_TCPClient.Send(new ServerRejoinGameDemand().GetBytes());
                        m_currentSubState = SubState.SUBSTATE_WAITING_FOR_REJOIN_CONFIRM;
                    }
                    else
                    {
                        m_currentSubState = SubState.SUBSTATE_GOING_TO_LOBBY;
                    }
                }
            }
            else if(m_currentSubState == SubState.SUBSTATE_WAITING_FOR_REJOIN_CONFIRM)
            {
                Thread deserializeWorldThread = new Thread(() =>
                {
#if DEBUG_LOG
                    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                    watch.Start();
#endif // DEBUG_LOG
                    ServerInitMessage init = common.serialization.IConvertible.CreateFromBytes<ServerInitMessage>(packet.Data.ArraySegment());
#if DEBUG_LOG
                    watch.Stop();
#endif //DEBUG_LOG
                    if (init != null)
                    {
#if DEBUG_LOG
                        Debug.Log("Player is in server game. Going to game.");
#endif

                        data.LoadingData.ServerInit = init;
                        m_currentSubState = SubState.SUBSTATE_GOING_TO_GAME;
                        return;
                    }
                });
                deserializeWorldThread.Start();
                
                ServerRejoinGameDemand rejoinResponse = common.serialization.IConvertible.CreateFromBytes<ServerRejoinGameDemand>(packet.Data.ArraySegment());
                if(rejoinResponse != null)
                {
#if DEBUG_LOG
                    Debug.Log("Player is not in server game. Leaving.");
#endif
                    // TODO : Leave to character select (pop state)
                }
            }
        }

        protected override void StateUnload()
        {
            m_TCPClient.Unsubscribe(this);
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_TCP;
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

