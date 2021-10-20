using System.Threading;
using ubv.common.data;
using UnityEngine;
using static ubv.microservices.DispatcherMicroservice;

namespace ubv.client.logic
{
    public class ClientGameSearch : ClientSyncState, tcp.client.ITCPClientReceiver
    {
        private enum SubState
        {
            SUBSTATE_WAITING_FOR_SERVER_INFO,
            SUBSTATE_WAITING_FOR_TCP,
            SUBSTATE_WAITING_FOR_UDP,
            SUBSTATE_WAITING_FOR_REJOIN_CONFIRM,
            SUBSTATE_GOING_TO_LOBBY,
            SUBSTATE_GOING_TO_GAME,
            SUBSTATE_TRANSITION,
        }

        [SerializeField] private string m_clientGameScene;
        [SerializeField] private string m_clientLobbyScene;

        [SerializeField] private float m_rejoinDemandTime = 10.0f;
        private float m_rejoinDemandTimer;

        [SerializeField] private float m_TCPTimeout = 10.0f;
        private float m_TCPTimeoutTimer;

        [SerializeField] private float m_dispatcherTimeout = 10.0f;
        private float m_dispatcherTimeoutTimer;

        [SerializeField] private float m_UDPPingTimerIntervalMS = 500f;
        private float m_UPDPingTimer;

        private byte[] m_identificationMessageBytes;
        
        private SubState m_currentSubState;
        
        protected override void StateLoad()
        {
            m_rejoinDemandTimer = 0;
            m_TCPTimeoutTimer = 0;
            m_currentSubState = SubState.SUBSTATE_WAITING_FOR_SERVER_INFO;

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
                case SubState.SUBSTATE_WAITING_FOR_SERVER_INFO:
                    m_dispatcherTimeoutTimer += Time.deltaTime;
                    if (m_dispatcherTimeoutTimer > m_dispatcherTimeout)
                    {
#if DEBUG_LOG
                        Debug.Log("Server info cannot be found.");
#endif // DEBUG_LOG
                        GoBackToPreviousState();
                    }
                    break;
                case SubState.SUBSTATE_WAITING_FOR_TCP:
                    m_TCPTimeoutTimer += Time.deltaTime;
                    if(m_TCPTimeoutTimer > m_TCPTimeout)
                    {
#if DEBUG_LOG
                        Debug.Log("Cannot connect to TCP Server");
#endif // DEBUG_LOG
                        GoBackToPreviousState();
                    }
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
                    m_rejoinDemandTimer += Time.deltaTime;
                    if(m_rejoinDemandTimer >= m_rejoinDemandTime)
                    {
                        // show a message meaning the game hasn't been found
#if DEBUG_LOG
                        Debug.Log("Old server game cannot be found.");
#endif // DEBUG_LOG
                        GoBackToPreviousState();
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
                Debug.Log("Trying to establish TCP connection to game server...");
#endif // DEBUG_LOG

                m_currentSubState = SubState.SUBSTATE_WAITING_FOR_TCP;
                m_TCPClient.Connect(data.LoadingData.ServerInfo.Value.server_tcp_ip, data.LoadingData.ServerInfo.Value.tcp_port);
            }
#if DEBUG_LOG
            else
            {
                m_currentSubState = SubState.SUBSTATE_WAITING_FOR_UDP;
                Debug.Log("Already connected to game server via TCP.");
            }
#endif // DEBUG_LOG
        }

        private void GoToLobby()
        {
            m_currentSubState = SubState.SUBSTATE_TRANSITION;
            ClientStateManager.Instance.PushScene(m_clientLobbyScene);
        }

        private void GoToGame()
        {
            m_currentSubState = SubState.SUBSTATE_TRANSITION;
            ClientStateManager.Instance.PushScene(m_clientGameScene);
        }

        private void GoBackToPreviousState()
        {
            m_currentSubState = SubState.SUBSTATE_TRANSITION;
            ClientStateManager.Instance.PopState();
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
                        m_rejoinDemandTimer = 0;
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

